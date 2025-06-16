using Bundlingway.Core.Interfaces;
using Bundlingway.Model;
using Bundlingway.Utilities.Extensions;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bundlingway.Core.Services
{
    /// <summary>
    /// Service-based implementation of package management logic, decoupled from UI.
    /// </summary>
    public class PackageService : IPackageService
    {
        private readonly IConfigurationService _configService;
        private readonly IFileSystemService _fileSystem;
        private readonly IHttpClientService _httpClient;
        private readonly IProgressReporter _progressReporter;
        private readonly IUserNotificationService _notifications;

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
        public event EventHandler<PackageOperationEventArgs>? PackageOperationStarted;
        public event EventHandler<PackageOperationEventArgs>? PackageOperationCompleted;

        public async Task<IEnumerable<ResourcePackage>> GetAllPackagesAsync()
        {
            // TODO: Implement logic to enumerate all packages from storage
            throw new System.NotImplementedException();
        }

        public async Task<ResourcePackage?> GetPackageByNameAsync(string packageName)
        {
            // TODO: Implement logic to retrieve a package by name
            throw new System.NotImplementedException();
        }

        public async Task ScanPackagesAsync()
        {
            // Example: Scan package folder and update list
            var packageFolder = _fileSystem.GetPackageFolder();
            var packageDirs = _fileSystem.GetDirectories(packageFolder);
            var foundPackages = new List<ResourcePackage>();
            foreach (var dir in packageDirs)
            {
                // TODO: Load package metadata from catalog file
                // foundPackages.Add(...)
            }
            PackagesUpdated?.Invoke(this, new PackageEventArgs { Packages = foundPackages, Message = "Scan complete" });
            await Task.CompletedTask;
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
                // TODO: Decouple and call post-processing pipeline if needed
                if (autoInstall)
                {
                    // await InstallPackageAsync(...); // TODO: implement install logic
                }
                return new ResourcePackage { Name = newFileName, Type = ResourcePackage.EType.SinglePreset };
            }

            // Archive onboarding (zip, rar, 7z)
            var archiveExtension = Path.GetExtension(filePath).ToLower();
            var tempFolderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var originalTempFolderPath = tempFolderPath;
            if (!_fileSystem.DirectoryExists(tempFolderPath)) _fileSystem.CreateDirectory(tempFolderPath);
            if (archiveExtension == ".zip")
            {
                await Task.Run(() => System.IO.Compression.ZipFile.ExtractToDirectory(filePath, tempFolderPath, true));
            }
            // TODO: Add support for .rar and .7z using SharpCompress if needed

            // Validate package structure (stubbed for now)
            // TODO: Implement ValidatePackage(tempFolderPath) logic in service style

            // Prepare ResourcePackage
            var collectionName = Path.GetFileNameWithoutExtension(filePath);
            var newCatalogEntry = new ResourcePackage
            {
                Type = ResourcePackage.EType.PresetCollection,
                Source = filePath,
                Name = packageName != null ? packageName.ToFileSystemSafeName() : collectionName,
                Status = ResourcePackage.EStatus.Unpacking
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

            // TODO: Copy files from tempFolderPath to respective folders (presets, textures, shaders) using _fileSystem
            // TODO: Remove 'Previews' directories from texturesFolder
            // TODO: Clean up temp folders
            // TODO: Save catalog entry as JSON

            // Return the onboarded ResourcePackage
            return newCatalogEntry;
        }

        public async Task OnboardPackagesAsync(IEnumerable<string> filePaths)
        {
            // TODO: Implement onboarding multiple packages
            await Task.WhenAll(filePaths.Select(f => OnboardPackageAsync(f)));
        }

        public async Task<ResourcePackage> InstallPackageAsync(ResourcePackage package)
        {
            // Example: Mark as installed and update status
            package.Status = ResourcePackage.EStatus.Installed;
            // TODO: Add actual install logic (file copy, config update, etc.)
            PackagesUpdated?.Invoke(this, new PackageEventArgs { Packages = new[] { package }, Message = $"Installed {package.Name}" });
            return await Task.FromResult(package);
        }

        public async Task UninstallPackageAsync(ResourcePackage package)
        {
            // Example: Mark as uninstalled and update status
            package.Status = ResourcePackage.EStatus.NotInstalled;
            // TODO: Add actual uninstall logic (file delete, config update, etc.)
            PackagesUpdated?.Invoke(this, new PackageEventArgs { Packages = new[] { package }, Message = $"Uninstalled {package.Name}" });
            await Task.CompletedTask;
        }

        public async Task ReinstallPackageAsync(ResourcePackage package)
        {
            // TODO: Implement reinstall logic
            await Task.CompletedTask;
        }

        public async Task<string> DownloadAndInstallAsync(string url, string? packageName = null)
        {
            // TODO: Implement download and install logic
            return await Task.FromResult("");
        }

        public async Task ToggleFavoriteAsync(ResourcePackage package)
        {
            // TODO: Implement favorite toggle logic
            await Task.CompletedTask;
        }

        public async Task ToggleLockedAsync(ResourcePackage package)
        {
            // TODO: Implement locked toggle logic
            await Task.CompletedTask;
        }

        public async Task<bool> ValidatePackageAsync(ResourcePackage package)
        {
            // TODO: Implement validation logic
            return await Task.FromResult(true);
        }

        public async Task RemovePackageAsync(ResourcePackage package)
        {
            // TODO: Move logic from Utilities/Handler/Package.Remove
            throw new System.NotImplementedException();
        }
    }
}
