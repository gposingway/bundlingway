using Bundlingway.Core.Interfaces;
using Bundlingway.Model;
using Bundlingway.Utilities.Extensions;
using Serilog;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Bundlingway.Core.Services
{
    /// <summary>
    /// Service-based implementation of package management logic, decoupled from UI.
    /// </summary>
    public class PackageService
    {
        private readonly IConfigurationService _configService;
        private readonly IFileSystemService _fileSystem;
        private readonly IHttpClientService _httpClient;
        private readonly IProgressReporter _progressReporter;
        private readonly IUserNotificationService _notifications;

        private ImmutableList<ResourcePackage> _cachedPackages = ImmutableList<ResourcePackage>.Empty;

        public PackageService(
            IConfigurationService configService,
            IFileSystemService fileSystem,
            IHttpClientService httpClient,
            IProgressReporter progressReporter,
            IUserNotificationService notifications)
        {
            _configService = configService;
            _fileSystem = fileSystem;
            _httpClient = httpClient;
            _progressReporter = progressReporter;
            _notifications = notifications;
        }

        public event EventHandler<PackageEventArgs>? PackagesUpdated;

        public Task<IEnumerable<ResourcePackage>> GetAllPackagesAsync()
        {
            // Return cached packages if available
            return Task.FromResult<IEnumerable<ResourcePackage>>(_cachedPackages);
        }

        public Task<ResourcePackage?> GetPackageByNameAsync(string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName))
                return Task.FromResult<ResourcePackage?>(null);
            // Search cached packages by Name (case-insensitive)
            return Task.FromResult(_cachedPackages.FirstOrDefault(pkg => pkg.Name.Equals(packageName, StringComparison.OrdinalIgnoreCase)));
        }

        public async Task ScanPackagesAsync()
        {
            var packageFolder = _fileSystem.GetPackageFolder();
            var packageDirs = _fileSystem.GetDirectories(packageFolder);
            var foundPackages = new List<ResourcePackage>();
            foreach (var dir in packageDirs)
            {
                var catalogPath = Path.Combine(dir, Bundlingway.Constants.Files.CatalogEntry);
                if (_fileSystem.FileExists(catalogPath))
                {
                    try
                    {
                        var json = await _fileSystem.ReadAllTextAsync(catalogPath);
                        var pkg = json.FromJson<ResourcePackage>();
                        if (pkg != null)
                            foundPackages.Add(pkg);
                    }
                    catch { /* Ignore invalid or missing catalog files */ }
                }
            }
            _cachedPackages = foundPackages.ToImmutableList();
            PackagesUpdated?.Invoke(this, new PackageEventArgs { Packages = foundPackages, Message = "Scan complete" });
            await Task.CompletedTask.ConfigureAwait(false);
        }

        public async Task<ResourcePackage> OnboardPackageAsync(string filePath, string? packageName = null, bool autoInstall = true)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (ext == ".ini" || (ext == ".txt" && (await _fileSystem.ReadAllTextAsync(filePath)).Contains("Techniques=", System.StringComparison.InvariantCultureIgnoreCase)))
            {
                // Handle single preset files (.ini/.txt)
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string fileExtensionPreset = ext == ".txt" ? ".ini" : ext;
                string newFileName = packageName != null ? $"{packageName.ToFileSystemSafeName()}{fileExtensionPreset}" : $"{fileName}{fileExtensionPreset}";
                if (packageName == null && !System.Text.RegularExpressions.Regex.IsMatch(filePath, @"\[\w{6}\]"))
                {
                    using var stream = _fileSystem.OpenRead(filePath);
                    byte[] hashBytes = System.Security.Cryptography.SHA256.HashData(stream);
                    var hash = string.Concat(hashBytes.Select(b => b.ToString("X2"))).Substring(0, 6);
                    newFileName = $"{fileName} [{hash}]{fileExtensionPreset}";
                }
                var targetPath = Path.Combine(_fileSystem.GetSinglePresetsFolder(), Bundlingway.Constants.Folders.PackagePresets);
                var targetFileName = Path.Combine(targetPath, newFileName);
                if (!_fileSystem.DirectoryExists(targetPath)) _fileSystem.CreateDirectory(targetPath);
                _fileSystem.CopyFile(filePath, targetFileName, true);
                // Call post-processing pipeline if needed (stub)
                // PostProcessorExtensions.RunRawFilePipeline(...)
                var pkg = new ResourcePackage
                {
                    Name = newFileName,
                    Label = newFileName,
                    Version = "1.0.0",
                    Source = filePath,
                    Type = ResourcePackage.EType.SinglePreset,
                    Status = ResourcePackage.EStatus.Installed,
                    LocalPresetFolder = targetPath,
                    LocalTextureFolder = string.Empty,
                    LocalShaderFolder = string.Empty,
                    LocalFolder = targetPath
                };
                if (autoInstall)
                    await InstallPackageAsync(pkg);

                _cachedPackages.Add(pkg);
                PackagesUpdated?.Invoke(this, new PackageEventArgs { Packages = [pkg], Message = "Onboarded" });

                return pkg;
            }

            // Archive onboarding (zip, rar, 7z)
            var archiveExtension = Path.GetExtension(filePath).ToLower();
            var tempFolderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            if (!_fileSystem.DirectoryExists(tempFolderPath)) _fileSystem.CreateDirectory(tempFolderPath);
            if (archiveExtension == ".zip")
            {
                // Secure extraction to prevent Zip-Slip
                await Task.Run(() =>
                {
                    using (var archive = System.IO.Compression.ZipFile.OpenRead(filePath))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            var entryPath = entry.FullName.Replace("/", Path.DirectorySeparatorChar.ToString());
                            if (string.IsNullOrWhiteSpace(entryPath) || entryPath.Contains(".."))
                                continue; // Prevent path traversal
                            var destinationPath = Path.GetFullPath(Path.Combine(tempFolderPath, entryPath));
                            if (!destinationPath.StartsWith(Path.GetFullPath(tempFolderPath)))
                                continue; // Prevent extraction outside tempFolderPath
                            if (entry.Name == "")
                            {
                                // Directory
                                if (!_fileSystem.DirectoryExists(destinationPath))
                                    _fileSystem.CreateDirectory(destinationPath);
                            }
                            else
                            {
                                // File
                                var destDir = Path.GetDirectoryName(destinationPath);
                                if (!string.IsNullOrEmpty(destDir) && !_fileSystem.DirectoryExists(destDir))
                                    _fileSystem.CreateDirectory(destDir);
                                entry.ExtractToFile(destinationPath, true);
                            }
                        }
                    }
                });
            }
            else if (archiveExtension == ".rar" || archiveExtension == ".7z")
            {
                // Use SharpCompress for .rar/.7z
                using var archive = SharpCompress.Archives.ArchiveFactory.Open(filePath);
                foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                {
                    if (string.IsNullOrEmpty(entry.Key)) continue;
                    var destPath = Path.Combine(tempFolderPath, entry.Key);
                    var fullDestPath = Path.GetFullPath(destPath);
                    var fullBasePath = Path.GetFullPath(tempFolderPath);
                    if (!fullDestPath.StartsWith(fullBasePath, StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Warning($"Archive entry '{entry.Key}' attempts path traversal. Skipping.");
                        continue;
                    }
                    var destDir = Path.GetDirectoryName(fullDestPath);
                    if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
                    using (var entryStream = entry.OpenEntryStream())
                    using (var fs = File.OpenWrite(fullDestPath))
                        entryStream.CopyTo(fs);
                }
            }

            // Validate package structure
            if (!ValidateExtractedPackage(tempFolderPath))
            {
                _fileSystem.DeleteDirectory(tempFolderPath, true);
                throw new InvalidDataException("Invalid package structure.");
            }

            // Prepare ResourcePackage
            var collectionName = Path.GetFileNameWithoutExtension(filePath);
            var newCatalogEntry = new ResourcePackage
            {
                Type = ResourcePackage.EType.PresetCollection,
                Source = filePath,
                Name = packageName != null ? packageName.ToFileSystemSafeName() : collectionName,
                Label = packageName != null ? packageName.ToFileSystemSafeName() : collectionName,
                Version = "1.0.0",
                Status = ResourcePackage.EStatus.Unpacking,
                LocalPresetFolder = string.Empty,
                LocalTextureFolder = string.Empty,
                LocalShaderFolder = string.Empty,
                LocalFolder = string.Empty
            };

            // Target package path
            var targetPackagePath = Path.Combine(_fileSystem.GetPackageFolder(), newCatalogEntry.Name.ToFileSystemSafeName());
            if (!_fileSystem.DirectoryExists(targetPackagePath)) _fileSystem.CreateDirectory(targetPackagePath);
            _fileSystem.CreateDirectory(Path.Combine(targetPackagePath, Bundlingway.Constants.Folders.SourcePackage));
            var target = Path.Combine(targetPackagePath, Bundlingway.Constants.Folders.SourcePackage, Path.GetFileName(filePath));
            _fileSystem.CopyFile(filePath, target, true);

            // Presets folder
            var presetsFolder = Path.Combine(targetPackagePath, Bundlingway.Constants.Folders.PackagePresets);
            if (!_fileSystem.DirectoryExists(presetsFolder)) _fileSystem.CreateDirectory(presetsFolder);
            newCatalogEntry.LocalPresetFolder = presetsFolder;

            // Textures folder
            var texturesFolder = Path.Combine(targetPackagePath, Bundlingway.Constants.Folders.PackageTextures);
            if (!_fileSystem.DirectoryExists(texturesFolder)) _fileSystem.CreateDirectory(texturesFolder);
            newCatalogEntry.LocalTextureFolder = texturesFolder;

            // Shaders folder
            var shadersFolder = Path.Combine(targetPackagePath, Bundlingway.Constants.Folders.PackageShaders);
            if (_fileSystem.DirectoryExists(shadersFolder)) _fileSystem.DeleteDirectory(shadersFolder, true);
            _fileSystem.CreateDirectory(shadersFolder);
            newCatalogEntry.LocalShaderFolder = shadersFolder;

            // Copy files from tempFolderPath to respective folders
            CopyExtractedFilesToPackage(tempFolderPath, presetsFolder, texturesFolder, shadersFolder);

            // Remove 'Previews' directories from texturesFolder
            RemovePreviewsDirectories(texturesFolder);

            // Clean up temp folders
            _fileSystem.DeleteDirectory(tempFolderPath, true);

            // Save catalog entry as JSON
            var catalogPath = Path.Combine(targetPackagePath, Bundlingway.Constants.Files.CatalogEntry);
            System.IO.File.WriteAllText(catalogPath, newCatalogEntry.ToJson());

            // Add the new package to the in-memory cache
            _cachedPackages = _cachedPackages.Add(newCatalogEntry);

            // Return the onboarded ResourcePackage
            return newCatalogEntry;
        }

        private bool ValidateExtractedPackage(string tempFolderPath)
        {
            // Validate if the directory exists
            if (!Directory.Exists(tempFolderPath))
                return false;
            // Must have at least one .ini or .fx file
            bool hasIni = Directory.GetFiles(tempFolderPath, "*.ini", SearchOption.AllDirectories).Length > 0;
            bool hasFx = Directory.GetFiles(tempFolderPath, "*.fx", SearchOption.AllDirectories).Length > 0;
            return hasIni || hasFx;
        }

        private void CopyExtractedFilesToPackage(string tempFolder, string presetsFolder, string texturesFolder, string shadersFolder)
        {
            foreach (var file in Directory.GetFiles(tempFolder, "*.*", SearchOption.AllDirectories))
            {
                var ext = Path.GetExtension(file).ToLowerInvariant();
                var dest = ext switch
                {
                    ".ini" => Path.Combine(presetsFolder, Path.GetFileName(file)),
                    ".png" or ".jpg" or ".jpeg" or ".dds" => Path.Combine(texturesFolder, Path.GetFileName(file)),
                    ".fx" or ".h" => Path.Combine(shadersFolder, Path.GetFileName(file)),
                    _ => null
                };
                if (dest != null)
                    File.Copy(file, dest, true);
            }
        }

        private void RemovePreviewsDirectories(string texturesFolder)
        {
            foreach (var dir in Directory.GetDirectories(texturesFolder, "Previews", SearchOption.AllDirectories))
            {
                Directory.Delete(dir, true);
            }
        }

        public async Task OnboardPackagesAsync(IEnumerable<string> filePaths)
        {
            await Task.WhenAll(filePaths.Select(f => OnboardPackageAsync(f)));
        }

        public async Task<ResourcePackage> InstallPackageAsync(ResourcePackage package)
        {
            package.Status = ResourcePackage.EStatus.Installed;
            // Example: update config (stub)
            await _configService.SaveAsync();
            PackagesUpdated?.Invoke(this, new PackageEventArgs { Packages = new[] { package }, Message = $"Installed {package.Name}" });
            return await Task.FromResult(package);
        }

        public async Task UninstallPackageAsync(ResourcePackage package)
        {
            package.Status = ResourcePackage.EStatus.NotInstalled;
            // Remove package files
            if (!string.IsNullOrEmpty(package.LocalFolder) && Directory.Exists(package.LocalFolder))
                Directory.Delete(package.LocalFolder, true);
            await _configService.SaveAsync();
            PackagesUpdated?.Invoke(this, new PackageEventArgs { Packages = new[] { package }, Message = $"Uninstalled {package.Name}" });
            await Task.CompletedTask;
        }

        public async Task ReinstallPackageAsync(ResourcePackage package)
        {
            await UninstallPackageAsync(package);
            await InstallPackageAsync(package);
        }

        public async Task<string> DownloadAndInstallAsync(string url, string? packageName = null)
        {
            // Download file
            var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + Path.GetExtension(url));
            using (var stream = await _httpClient.GetStreamAsync(url))
            using (var fs = File.OpenWrite(tempFile))
                await stream.CopyToAsync(fs);
            var pkg = await OnboardPackageAsync(tempFile, packageName, true);
            return pkg.Name;
        }

        public async Task ToggleFavoriteAsync(ResourcePackage package)
        {
            package.Favorite = !package.Favorite;
            await _configService.SaveAsync();
            PackagesUpdated?.Invoke(this, new PackageEventArgs { Packages = [package], Message = $"Favorite toggled for {package.Name}" });
            await Task.CompletedTask;
        }

        public async Task ToggleLockedAsync(ResourcePackage package)
        {
            package.Locked = !package.Locked;
            await _configService.SaveAsync();
            PackagesUpdated?.Invoke(this, new PackageEventArgs { Packages = [package], Message = $"Locked toggled for {package.Name}" });
            await Task.CompletedTask;
        }

        public Task<bool> ValidatePackageAsync(ResourcePackage package)
        {
            // Validate that the package folders exist and have at least one preset or shader
            bool valid = false;
            if (!string.IsNullOrEmpty(package.LocalPresetFolder) && Directory.Exists(package.LocalPresetFolder))
                valid |= Directory.GetFiles(package.LocalPresetFolder, "*.ini", SearchOption.AllDirectories).Length > 0;
            if (!string.IsNullOrEmpty(package.LocalShaderFolder) && Directory.Exists(package.LocalShaderFolder))
                valid |= Directory.GetFiles(package.LocalShaderFolder, "*.fx", SearchOption.AllDirectories).Length > 0;
            return Task.FromResult(valid);
        }

        public async Task RemovePackageAsync(ResourcePackage package)
        {
            // Uninstall the package first if needed
            await UninstallPackageAsync(package);

            // Try to determine the package folder if LocalFolder is null or empty
            string? folder = package.LocalFolder;
            if (string.IsNullOrEmpty(folder))
            {
                var packageRoot = _fileSystem.GetPackageFolder();
                var safeName = package.Name.ToFileSystemSafeName();
                var candidate = Path.Combine(packageRoot, safeName);
                if (_fileSystem.DirectoryExists(candidate))
                    folder = candidate;
            }

            // Remove all files and folders for the package
            if (!string.IsNullOrEmpty(folder) && _fileSystem.DirectoryExists(folder))
            {
                _fileSystem.DeleteDirectory(folder, true);
            }
            PackagesUpdated?.Invoke(this, new PackageEventArgs { Packages = [package], Message = $"Removed {package.Name}" });
        }
    }
}
    public class PackageEventArgs : EventArgs
    {
        public IEnumerable<ResourcePackage>? Packages { get; set; }
        public string? Message { get; set; }
    }

    public class PackageOperationEventArgs : EventArgs
    {
        public ResourcePackage? Package { get; set; }
        public string? Operation { get; set; }
        public string? Message { get; set; }
        public bool IsSuccess { get; set; }
        public Exception? Exception { get; set; }
    }

