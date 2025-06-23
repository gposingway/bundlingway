using Bundlingway.Core.Interfaces;
using Bundlingway.Model;
using Bundlingway.Utilities;
using Bundlingway.Utilities.Extensions;
using Bundlingway.Utilities.ManagedResources;
using Serilog;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Bundlingway.Core.Services
{    /// <summary>
     /// Service-based implementation of package management logic, decoupled from UI.
     /// </summary>
    public class PackageService
    {
        private readonly IConfigurationService _configService;
        private readonly IFileSystemService _fileSystem;
        private readonly IHttpClientService _httpClient;
        private readonly IProgressReporter _progressReporter;
        private readonly IUserNotificationService _notifications;
        private readonly IAppEnvironmentService _appEnvironment;
        private readonly PostProcessorService _postProcessorService;
        private ImmutableList<ResourcePackage> _cachedPackages = ImmutableList<ResourcePackage>.Empty;
        private bool _packagesInitialized = false;
        private readonly object _initializationLock = new object();

        /// <summary>
        /// Gets the cached packages, initializing them on first access if necessary.
        /// </summary>
        private ImmutableList<ResourcePackage> CachedPackages
        {
            get
            {
                if (!_packagesInitialized)
                {
                    lock (_initializationLock)
                    {
                        if (!_packagesInitialized)
                        {
                            try
                            {
                                var packageFolder = _fileSystem.GetPackageFolder();
                                if (_fileSystem.DirectoryExists(packageFolder))
                                {
                                    var packageDirs = _fileSystem.GetDirectories(packageFolder);
                                    var foundPackages = new List<ResourcePackage>();
                                    foreach (var dir in packageDirs)
                                    {
                                        var catalogPath = Path.Combine(dir, Bundlingway.Constants.Files.CatalogEntry);
                                        if (_fileSystem.FileExists(catalogPath))
                                        {
                                            try
                                            {
                                                var json = _fileSystem.ReadAllTextAsync(catalogPath).Result;
                                                var pkg = json.FromJson<ResourcePackage>();

                                                if (pkg != null && string.IsNullOrEmpty(pkg.LocalFolder) && !string.IsNullOrEmpty(pkg.LocalPresetFolder))
                                                {
                                                    pkg.LocalFolder = dir;
                                                }

                                                if (pkg != null)
                                                    foundPackages.Add(pkg);
                                            }
                                            catch (Exception ex)
                                            {
                                                Log.Warning($"Failed to parse package catalog at '{catalogPath}': {ex.Message}");
                                            }
                                        }
                                    }
                                    _cachedPackages = [.. foundPackages];
                                }
                                _packagesInitialized = true;
                            }
                            catch (Exception ex)
                            {
                                Log.Warning($"Failed to initialize packages cache: {ex.Message}");
                                _cachedPackages = ImmutableList<ResourcePackage>.Empty;
                                _packagesInitialized = true;
                            }
                        }
                    }
                }
                return _cachedPackages;
            }
        }
        public PackageService(
            IConfigurationService configService,
            IFileSystemService fileSystem,
            IHttpClientService httpClient,
            IProgressReporter progressReporter,
            IUserNotificationService notifications,
            IAppEnvironmentService appEnvironment,
            PostProcessorService postProcessorService)
        {
            _configService = configService;
            _fileSystem = fileSystem;
            _httpClient = httpClient;
            _progressReporter = progressReporter;
            _notifications = notifications;
            _appEnvironment = appEnvironment;
            _postProcessorService = postProcessorService;
        }

        public event EventHandler<PackageEventArgs>? PackagesUpdated; public Task<IEnumerable<ResourcePackage>> GetAllPackagesAsync()
        {
            return Task.FromResult<IEnumerable<ResourcePackage>>(CachedPackages);
        }

        public Task<ResourcePackage?> GetPackageByNameAsync(string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName))
                return Task.FromResult<ResourcePackage?>(null);

            return Task.FromResult(CachedPackages.FirstOrDefault(pkg => pkg.Name.Equals(packageName, StringComparison.OrdinalIgnoreCase)));
        }

        public async Task<ResourcePackage> OnboardPackageAsync(string filePath, string? packageName = null, bool autoInstall = true)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (ext == ".ini" || (ext == ".txt" && (await _fileSystem.ReadAllTextAsync(filePath)).Contains("Techniques=", StringComparison.InvariantCultureIgnoreCase)))
            {
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
                }; _postProcessorService.RunPipeline(pkg);

                if (autoInstall) await InstallPackageAsync(pkg);

                lock (_initializationLock)
                {
                    _cachedPackages = _cachedPackages.Add(pkg);
                    _packagesInitialized = true;
                }
                PackagesUpdated?.Invoke(this, new PackageEventArgs { Packages = [pkg], Message = "Onboarded" });

                return pkg;
            }

            var archiveExtension = Path.GetExtension(filePath).ToLower();
            var tempFolderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            if (!_fileSystem.DirectoryExists(tempFolderPath)) _fileSystem.CreateDirectory(tempFolderPath);
            if (archiveExtension == ".zip")
            {
                await Task.Run(() =>
                {
                    using (var archive = ZipFile.OpenRead(filePath))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            var entryPath = entry.FullName.Replace("/", Path.DirectorySeparatorChar.ToString());
                            if (string.IsNullOrWhiteSpace(entryPath) || entryPath.Contains(".."))
                                continue;
                            var destinationPath = Path.GetFullPath(Path.Combine(tempFolderPath, entryPath));
                            if (!destinationPath.StartsWith(Path.GetFullPath(tempFolderPath)))
                                continue;
                            if (entry.Name == "")
                            {
                                if (!_fileSystem.DirectoryExists(destinationPath))
                                    _fileSystem.CreateDirectory(destinationPath);
                            }
                            else
                            {
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
                    if (!string.IsNullOrEmpty(destDir) && !_fileSystem.DirectoryExists(destDir)) _fileSystem.CreateDirectory(destDir);
                    using (var entryStream = entry.OpenEntryStream())
                    using (var fs = _fileSystem.OpenWrite(fullDestPath))
                        entryStream.CopyTo(fs);
                }
            }

            if (!ValidateExtractedPackage(tempFolderPath))
            {
                _fileSystem.DeleteDirectory(tempFolderPath, true);
                throw new InvalidDataException("Invalid package structure.");
            }

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

            var targetPackagePath = Path.Combine(_fileSystem.GetPackageFolder(), newCatalogEntry.Name.ToFileSystemSafeName());
            newCatalogEntry.LocalFolder = targetPackagePath;

            if (!_fileSystem.DirectoryExists(targetPackagePath)) _fileSystem.CreateDirectory(targetPackagePath);
            _fileSystem.CreateDirectory(Path.Combine(targetPackagePath, Constants.Folders.SourcePackage));
            var target = Path.Combine(targetPackagePath, Constants.Folders.SourcePackage, Path.GetFileName(filePath));
            _fileSystem.CopyFile(filePath, target, true); var presetsFolder = Path.Combine(targetPackagePath, Constants.Folders.PackagePresets);
            if (!_fileSystem.DirectoryExists(presetsFolder)) _fileSystem.CreateDirectory(presetsFolder);
            newCatalogEntry.LocalPresetFolder = presetsFolder;

            var texturesFolder = Path.Combine(targetPackagePath, Constants.Folders.PackageTextures);
            if (!_fileSystem.DirectoryExists(texturesFolder)) _fileSystem.CreateDirectory(texturesFolder);
            newCatalogEntry.LocalTextureFolder = texturesFolder;

            var shadersFolder = Path.Combine(targetPackagePath, Constants.Folders.PackageShaders);
            if (_fileSystem.DirectoryExists(shadersFolder)) _fileSystem.DeleteDirectory(shadersFolder, true);
            _fileSystem.CreateDirectory(shadersFolder);
            newCatalogEntry.LocalShaderFolder = shadersFolder;

            CopyExtractedFilesToPackage(tempFolderPath, presetsFolder, texturesFolder, shadersFolder);
            RemovePreviewsDirectories(texturesFolder);
            _fileSystem.DeleteDirectory(tempFolderPath, true);

            var catalogPath = Path.Combine(targetPackagePath, Bundlingway.Constants.Files.CatalogEntry);
            File.WriteAllText(catalogPath, newCatalogEntry.ToJson());

            _postProcessorService.RunPipeline(newCatalogEntry);

            if (autoInstall) await InstallPackageAsync(newCatalogEntry);
            return newCatalogEntry;
        }

        private bool ValidateExtractedPackage(string tempFolderPath)
        {
            if (!_fileSystem.DirectoryExists(tempFolderPath))
                return false;
            bool hasIni = _fileSystem.GetFiles(tempFolderPath, "*.ini", SearchOption.AllDirectories).Any();
            bool hasFx = _fileSystem.GetFiles(tempFolderPath, "*.fx", SearchOption.AllDirectories).Any();
            return hasIni || hasFx;
        }

        private void CopyExtractedFilesToPackage(string tempFolder, string presetsFolder, string texturesFolder, string shadersFolder)
        {
            foreach (var file in _fileSystem.GetFiles(tempFolder, "*.*", SearchOption.AllDirectories))
            {
                var ext = Path.GetExtension(file).ToLowerInvariant();
                var dest = ext switch
                {
                    ".ini" => Path.Combine(presetsFolder, Path.GetFileName(file)),
                    ".png" or ".jpg" or ".jpeg" or ".dds" => Path.Combine(texturesFolder, Path.GetFileName(file)),
                    ".fx" or ".fxh" or ".h" => Path.Combine(shadersFolder, Path.GetFileName(file)),
                    _ => null
                };
                if (dest != null)
                    File.Copy(file, dest, true);
            }
        }

        private void RemovePreviewsDirectories(string texturesFolder)
        {
            foreach (var dir in _fileSystem.GetDirectories(texturesFolder, "Previews", SearchOption.AllDirectories))
            {
                _fileSystem.DeleteDirectory(dir, true);
            }
        }

        public async Task OnboardPackagesAsync(IEnumerable<string> filePaths)
        {
            await Task.WhenAll(filePaths.Select(f => OnboardPackageAsync(f)));
        }

        public async Task<ResourcePackage> InstallPackageAsync(ResourcePackage package)
        {
            Log.Information($"PackageService.InstallPackageAsync: Installing package: {package.Name}");

            package.Status = ResourcePackage.EStatus.Installing;

            string sourcePackagePath = package.LocalFolder;
            string localCatalogFilePath = Path.Combine(sourcePackagePath, Constants.Files.CatalogEntry);

            if (!_fileSystem.FileExists(localCatalogFilePath))
            {
                Log.Information("PackageService.InstallPackageAsync: catalog-entry.json not found. Exiting.");
                return null;
            }

            ResourcePackage catalogEntry = SerializationExtensions.FromJsonFile<ResourcePackage>(localCatalogFilePath);
            var config = _configService.Configuration;
            string installationShaderAnalysisFilePath = Path.Combine(_appEnvironment.BundlingwayDataFolder, Constants.Files.ShaderAnalysis);
            string localShaderAnalysisFilePath = Path.Combine(sourcePackagePath, Constants.Files.ShaderAnalysis);

            var installationShaderAnalysis = _fileSystem.FileExists(installationShaderAnalysisFilePath)
                ? SerializationExtensions.FromJsonFile<Dictionary<string, ShaderSignature>>(installationShaderAnalysisFilePath)
                : new Dictionary<string, ShaderSignature>();
            var localShaderAnalysis = _fileSystem.FileExists(localShaderAnalysisFilePath)
                ? SerializationExtensions.FromJsonFile<Dictionary<string, ShaderSignature>>(localShaderAnalysisFilePath)
                : new Dictionary<string, ShaderSignature>();

            Log.Information("PackageService.InstallPackageAsync: Loaded catalog entry from file.");
            Log.Information($"PackageService.InstallPackageAsync: catalogEntry.Name: {catalogEntry.Name}");

            var collectionName = catalogEntry.Name;
            var sourcePresetsFolder = Path.Combine(sourcePackagePath, Constants.Folders.PackagePresets);
            var sourceTexturesFolder = Path.Combine(sourcePackagePath, Constants.Folders.PackageTextures);
            var sourceShadersFolder = Path.Combine(sourcePackagePath, Constants.Folders.PackageShaders);

            if (string.IsNullOrEmpty(config.Game.InstallationFolder))
            {
                Log.Error("PackageService.InstallPackageAsync: Game installation folder is not configured.");
                catalogEntry.Status = ResourcePackage.EStatus.Error;
                await SaveAsync(catalogEntry); // Ensure the package state is saved
                return catalogEntry;
            }

            string gamePresetsFolder = Path.Combine(config.Game.InstallationFolder, Constants.Folders.GamePresets);
            string? gameTexturesFolder = null;
            string gameShaderFolder = Path.Combine(config.Game.InstallationFolder, Constants.Folders.GameShaders);

            if (!catalogEntry.Bundle)
            {
                gamePresetsFolder = Path.Combine(gamePresetsFolder, collectionName);
                gameTexturesFolder = Path.Combine(config.Game.InstallationFolder, Constants.Folders.GameShaders, Constants.Folders.PackageTextures, collectionName);
                gameShaderFolder = Path.Combine(gameShaderFolder, Constants.Folders.PackageShaders, collectionName);

                catalogEntry.LocalPresetFolder = gamePresetsFolder;
                catalogEntry.LocalTextureFolder = gameTexturesFolder;
                catalogEntry.LocalShaderFolder = gameShaderFolder;
            }
            else
            {
                gameTexturesFolder = Path.Combine(config.Game.InstallationFolder, Constants.Folders.GameShaders, Constants.Folders.PackageTextures);
                gameShaderFolder = Path.Combine(config.Game.InstallationFolder, Constants.Folders.GameShaders, Constants.Folders.PackageShaders);
            }

            try
            {
                if (_fileSystem.DirectoryExists(sourcePresetsFolder))
                {
                    if (catalogEntry.Bundle)
                    {
                        foreach (var folder in _fileSystem.GetDirectories(sourcePresetsFolder))
                        {
                            var relativePath = Path.GetRelativePath(sourcePresetsFolder, folder);
                            var targetFolder = Path.Combine(gamePresetsFolder, relativePath);

                            _fileSystem.CreateDirectory(targetFolder);
                        }
                    }

                    _fileSystem.CreateDirectory(gamePresetsFolder);
                    Log.Information($"PackageService.InstallPackageAsync: Created game presets folder at: {gamePresetsFolder}");

                    foreach (var acceptableFile in Constants.AcceptableFilesInPresetFolder)
                    {
                        foreach (var file in _fileSystem.GetFiles(sourcePresetsFolder, acceptableFile, SearchOption.AllDirectories))
                        {
                            var relativePath = Path.GetRelativePath(sourcePresetsFolder, file);
                            var targetPath = Path.Combine(gamePresetsFolder, relativePath);
                            var targetDir = Path.GetDirectoryName(targetPath);
                            if (!string.IsNullOrEmpty(targetDir))
                                _fileSystem.CreateDirectory(targetDir);
                            _fileSystem.CopyFile(file, targetPath, true);
                            Log.Information($"PackageService.InstallPackageAsync: Copied preset file {file} to {targetPath}");
                        }
                    }

                    var replacements = config.Shortcuts.ToDictionary(k => "%" + k.Key + "%", v => v.Value);

                    if (catalogEntry.Bundle)
                    {
                        foreach (var folder in _fileSystem.GetDirectories(gamePresetsFolder))
                            PostProcessorExtensions.ReplaceValues(folder, replacements);

                        PostProcessorExtensions.ReplaceValues(gamePresetsFolder, replacements, false);
                    }
                    else
                    {
                        PostProcessorExtensions.ReplaceValues(gamePresetsFolder, replacements);
                    }
                }

                // Textures:
                if (!string.IsNullOrEmpty(catalogEntry.LocalTextureFolder))
                {
                    if (_fileSystem.DirectoryExists(sourceTexturesFolder))
                    {
                        if (_fileSystem.GetFiles(sourceTexturesFolder, "*.*", SearchOption.AllDirectories).Any() ||
                            _fileSystem.GetDirectories(sourceTexturesFolder).Any())
                        {
                            if (!string.IsNullOrEmpty(gameTexturesFolder)) _fileSystem.CreateDirectory(gameTexturesFolder);

                            Log.Information($"PackageService.InstallPackageAsync: Created game textures folder at: {gameTexturesFolder}");

                            foreach (var file in _fileSystem.GetFiles(sourceTexturesFolder, "*.*", SearchOption.AllDirectories))
                            {
                                if (!string.IsNullOrEmpty(gameTexturesFolder))
                                {
                                    var relativePath = Path.GetRelativePath(sourceTexturesFolder, file);
                                    var targetPath = Path.Combine(gameTexturesFolder, relativePath);
                                    var targetDir = Path.GetDirectoryName(targetPath);
                                    if (!string.IsNullOrEmpty(targetDir))
                                        _fileSystem.CreateDirectory(targetDir);
                                    _fileSystem.CopyFile(file, targetPath, true);
                                    Log.Information($"PackageService.InstallPackageAsync: Copied texture file {file} to {targetPath}");
                                }
                            }
                        }
                    }
                }

                // Shaders:
                if (catalogEntry.Type == ResourcePackage.EType.ShaderCollection || catalogEntry.Type == ResourcePackage.EType.CorePackage)
                {
                    if (_fileSystem.DirectoryExists(sourceShadersFolder))
                    {
                        _fileSystem.CreateDirectory(gameShaderFolder);

                        var referenceFolder = Path.Combine(catalogEntry.LocalFolder, Constants.Folders.PackageShaders);

                        foreach (var file in _fileSystem.GetFiles(sourceShadersFolder, "*.*", SearchOption.AllDirectories))
                        {
                            var shaderFileExtension = Path.GetExtension(file).ToLower();

                            if (!catalogEntry.Bundle && !Constants.ShaderExtensions.Contains(shaderFileExtension))
                                continue;
                            var relativePath = Path.GetRelativePath(referenceFolder, file);
                            var targetPath = Path.Combine(gameShaderFolder, relativePath);
                            var targetDir = Path.GetDirectoryName(targetPath);
                            if (!string.IsNullOrEmpty(targetDir))
                                _fileSystem.CreateDirectory(targetDir);
                            _fileSystem.CopyFile(file, targetPath, true);
                            Log.Information($"PackageService.InstallPackageAsync: Copied shader file {file} to {targetPath}");
                        }
                    }
                }

                catalogEntry.Status = ResourcePackage.EStatus.Installed;

                Log.Information("PackageService.InstallPackageAsync: Package installed successfully.");
                PackagesUpdated?.Invoke(this, new PackageEventArgs { Packages = [catalogEntry], Message = $"Installed {catalogEntry.Name}" });
            }
            catch (Exception e)
            {
                Log.Error(e, "PackageService.InstallPackageAsync: Error installing package");
                catalogEntry.Status = ResourcePackage.EStatus.Error;
            }

            await SaveAsync(catalogEntry); // Ensure the package state is saved    

            return catalogEntry;
        }
        public async Task UninstallPackageAsync(ResourcePackage package)
        {
            Log.Information($"PackageService.UninstallPackageAsync: Starting uninstall for package: {package.Name}");

            if (package.Locked)
            {
                Log.Warning($"PackageService.UninstallPackageAsync: Package {package.Name} is locked and cannot be uninstalled.");
                return;
            }

            package.Status = ResourcePackage.EStatus.NotInstalled;

            // Get the source package folders to determine what files should be removed
            var sourcePackagePath = package.LocalFolder;
            if (string.IsNullOrEmpty(sourcePackagePath) || !_fileSystem.DirectoryExists(sourcePackagePath))
            {
                Log.Warning($"PackageService.UninstallPackageAsync: Source package folder not found for {package.Name}");
                PackagesUpdated?.Invoke(this, new PackageEventArgs { Packages = [package], Message = $"Uninstalled {package.Name}" });
                return;
            }

            var sourcePresetsFolder = Path.Combine(sourcePackagePath, Constants.Folders.PackagePresets);
            var sourceTexturesFolder = Path.Combine(sourcePackagePath, Constants.Folders.PackageTextures);
            var sourceShadersFolder = Path.Combine(sourcePackagePath, Constants.Folders.PackageShaders);

            // Remove preset files that match the package content
            if (!string.IsNullOrEmpty(package.LocalPresetFolder) && _fileSystem.DirectoryExists(package.LocalPresetFolder))
            {
                await RemoveMatchingFilesAsync(sourcePresetsFolder, package.LocalPresetFolder, "presets");
            }

            // Remove texture files that match the package content
            if (!string.IsNullOrEmpty(package.LocalTextureFolder) && _fileSystem.DirectoryExists(package.LocalTextureFolder))
            {
                await RemoveMatchingFilesAsync(sourceTexturesFolder, package.LocalTextureFolder, "textures");
            }

            // Remove shader files that match the package content
            if (!string.IsNullOrEmpty(package.LocalShaderFolder) && _fileSystem.DirectoryExists(package.LocalShaderFolder))
            {
                await RemoveMatchingFilesAsync(sourceShadersFolder, package.LocalShaderFolder, "shaders");
            }

            await SaveAsync(package); // Save the updated package state

            //await _configService.SaveAsync();
            PackagesUpdated?.Invoke(this, new PackageEventArgs { Packages = [package], Message = $"Uninstalled {package.Name}" });

            Log.Information($"PackageService.UninstallPackageAsync: Completed uninstall for package: {package.Name}");
        }

        private async Task RemoveMatchingFilesAsync(string sourceFolder, string targetFolder, string fileType)
        {
            if (!_fileSystem.DirectoryExists(sourceFolder) || !_fileSystem.DirectoryExists(targetFolder))
            {
                Log.Information($"PackageService.RemoveMatchingFilesAsync: Source or target folder does not exist for {fileType}");
                return;
            }

            Log.Information($"PackageService.RemoveMatchingFilesAsync: Removing {fileType} files from {targetFolder}");

            try
            {
                // Get all files from the source package folder
                var sourceFiles = _fileSystem.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories);
                var filesToRemove = new List<string>(); foreach (var sourceFile in sourceFiles)
                {
                    // Calculate the relative path from the source folder
                    var relativePath = Path.GetRelativePath(sourceFolder, sourceFile);
                    var targetFile = Path.Combine(targetFolder, relativePath);

                    // Check if the corresponding file exists in the target location
                    if (_fileSystem.FileExists(targetFile))
                    {
                        // Mark for removal - we remove any file that matches the package structure
                        filesToRemove.Add(targetFile);
                        Log.Information($"PackageService.RemoveMatchingFilesAsync: Marked for removal: {targetFile}");
                    }
                }

                // Remove the identified files
                foreach (var fileToRemove in filesToRemove)
                {
                    try
                    {
                        _fileSystem.DeleteFile(fileToRemove);
                        Log.Information($"PackageService.RemoveMatchingFilesAsync: Removed file: {fileToRemove}");
                    }
                    catch (Exception ex)
                    {
                        Log.Warning($"PackageService.RemoveMatchingFilesAsync: Failed to remove file {fileToRemove}: {ex.Message}");
                    }
                }

                // Clean up empty directories
                await CleanupEmptyDirectoriesAsync(targetFolder);
            }
            catch (Exception ex)
            {
                Log.Error($"PackageService.RemoveMatchingFilesAsync: Error processing {fileType} files: {ex.Message}");
            }
        }

        private async Task CleanupEmptyDirectoriesAsync(string rootFolder)
        {
            if (!_fileSystem.DirectoryExists(rootFolder))
                return;

            try
            {
                // Get all subdirectories, process deepest first
                var directories = _fileSystem.GetDirectories(rootFolder, "*", SearchOption.AllDirectories)
                    .OrderByDescending(d => d.Length) // Process deepest paths first
                    .ToList();

                foreach (var directory in directories)
                {
                    try
                    {
                        // Check if directory is empty (no files and no subdirectories)
                        var hasFiles = _fileSystem.GetFiles(directory, "*", SearchOption.TopDirectoryOnly).Any();
                        var hasSubDirs = _fileSystem.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly).Any();

                        if (!hasFiles && !hasSubDirs)
                        {
                            _fileSystem.DeleteDirectory(directory, false);
                            Log.Information($"PackageService.CleanupEmptyDirectoriesAsync: Removed empty directory: {directory}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning($"PackageService.CleanupEmptyDirectoriesAsync: Failed to remove directory {directory}: {ex.Message}");
                    }
                }

                // Finally check if the root folder itself is empty
                var rootHasFiles = _fileSystem.GetFiles(rootFolder, "*", SearchOption.TopDirectoryOnly).Any();
                var rootHasSubDirs = _fileSystem.GetDirectories(rootFolder, "*", SearchOption.TopDirectoryOnly).Any();

                if (!rootHasFiles && !rootHasSubDirs)
                {
                    try
                    {
                        _fileSystem.DeleteDirectory(rootFolder, false);
                        Log.Information($"PackageService.CleanupEmptyDirectoriesAsync: Removed empty root directory: {rootFolder}");
                    }
                    catch (Exception ex)
                    {
                        Log.Warning($"PackageService.CleanupEmptyDirectoriesAsync: Failed to remove root directory {rootFolder}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"PackageService.CleanupEmptyDirectoriesAsync: Error cleaning up directories: {ex.Message}");
            }

            await Task.CompletedTask;
        }

        public async Task ReinstallPackageAsync(ResourcePackage package)
        {
            try
            {
                await UninstallPackageAsync(package);
                await InstallPackageAsync(package);

            }
            catch (Exception e)
            {
                Log.Error(e, $"PackageService.ReinstallPackageAsync: Error reinstalling package {package.Name}");
            }
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

            if (package.Type == ResourcePackage.EType.CorePackage)
            {
                Log.Warning($"PackageService.RemovePackageAsync: Cannot unlock core package {package.Name}.");
                return;
            }

            package.Locked = !package.Locked;
            await _configService.SaveAsync();
            PackagesUpdated?.Invoke(this, new PackageEventArgs { Packages = [package], Message = $"Locked toggled for {package.Name}" });
            await Task.CompletedTask;
        }

        public Task<bool> ValidatePackageAsync(ResourcePackage package)
        {
            // Validate that the package folders exist and have at least one preset or shader
            bool valid = false;
            if (!string.IsNullOrEmpty(package.LocalPresetFolder) && _fileSystem.DirectoryExists(package.LocalPresetFolder))
                valid |= _fileSystem.GetFiles(package.LocalPresetFolder, "*.ini", System.IO.SearchOption.AllDirectories).Any();
            if (!string.IsNullOrEmpty(package.LocalShaderFolder) && _fileSystem.DirectoryExists(package.LocalShaderFolder))
                valid |= _fileSystem.GetFiles(package.LocalShaderFolder, "*.fx", System.IO.SearchOption.AllDirectories).Any();
            return Task.FromResult(valid);
        }
        public async Task RemovePackageAsync(ResourcePackage package)
        {
            if (package.Locked)
            {
                Log.Warning($"PackageService.RemovePackageAsync: Package {package.Name} is locked and cannot be removed.");
                return;
            }

            if (package.Type == ResourcePackage.EType.CorePackage)
            {
                Log.Warning($"PackageService.RemovePackageAsync: Cannot remove core package {package.Name}.");
                return;
            }

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

            // Remove from cached packages
            lock (_initializationLock)
            {
                _cachedPackages = _cachedPackages.Remove(package);
            }

            PackagesUpdated?.Invoke(this, new PackageEventArgs { Packages = [package], Message = $"Removed {package.Name}" });
        }

        internal void Refresh()
        {
            _packagesInitialized = false;
        }

        /// <summary>
        /// Refreshes the package cache and triggers events for any newly discovered packages.
        /// </summary>
        public async Task RefreshAndNotifyAsync()
        {
            try
            {
                // Get current packages before refresh
                var packagesBefore = _cachedPackages.ToList();

                // Reset and reload packages
                _packagesInitialized = false;
                var packagesAfter = await GetAllPackagesAsync();

                // Find newly discovered packages (packages that exist now but didn't before)
                var newPackages = packagesAfter
                    .Where(after => !packagesBefore.Any(before =>
                        before.Name.Equals(after.Name, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                if (newPackages.Any())
                {
                    Log.Information("Found {Count} new packages after refresh: {Names}",
                        newPackages.Count,
                        string.Join(", ", newPackages.Select(p => p.Name)));

                    // Trigger PackagesUpdated event for new packages
                    foreach (var newPackage in newPackages)
                    {
                        PackagesUpdated?.Invoke(this, new PackageEventArgs
                        {
                            Packages = [newPackage],
                            Message = $"Installed {newPackage.Name}"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to refresh packages and notify");
            }
        }

        /// <summary>
        /// Saves a package's catalog entry, refreshes the cache, and announces the change.
        /// </summary>
        /// <param name="package">The package to save.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SaveAsync(ResourcePackage package)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));

            try
            {
                // Save the package catalog entry
                await package.SaveAsync(_fileSystem);

                // Refresh the cache entry
                lock (_initializationLock)
                {
                    var existingIndex = _cachedPackages.FindIndex(p => p.Name.Equals(package.Name, StringComparison.OrdinalIgnoreCase));
                    if (existingIndex >= 0)
                    {
                        // Update existing package in cache
                        _cachedPackages = _cachedPackages.SetItem(existingIndex, package);
                    }
                    else
                    {
                        // Add new package to cache
                        _cachedPackages = _cachedPackages.Add(package);
                    }
                }

                // Announce the change for UI update
                PackagesUpdated?.Invoke(this, new PackageEventArgs { Packages = [package], Message = $"Saved {package.Name}" });

                Log.Information($"PackageService.SaveAsync: Successfully saved package: {package.Name}");
            }
            catch (Exception ex)
            {
                Log.Error($"PackageService.SaveAsync: Failed to save package {package.Name}: {ex.Message}");
                throw;
            }
        }
    }
}

