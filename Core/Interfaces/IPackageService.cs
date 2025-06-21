using Bundlingway.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bundlingway.Core.Interfaces
{
    /// <summary>
    /// Core service for package management operations.
    /// Handles all package lifecycle operations without UI dependencies.
    /// </summary>
    public interface IPackageService
    {
        /// <summary>
        /// Gets all available packages.
        /// </summary>
        Task<IEnumerable<ResourcePackage>> GetAllPackagesAsync();

        /// <summary>
        /// Gets a package by name.
        /// </summary>
        /// <param name="packageName">Name of the package</param>
        Task<ResourcePackage?> GetPackageByNameAsync(string packageName);

        /// <summary>
        /// Scans for packages in the configured directories.
        /// </summary>
        Task ScanPackagesAsync();

        /// <summary>
        /// Onboards a package from a file path.
        /// </summary>
        /// <param name="filePath">Path to the package file</param>
        /// <param name="packageName">Optional custom package name</param>
        /// <param name="autoInstall">Whether to automatically install after onboarding</param>
        Task<ResourcePackage> OnboardPackageAsync(string filePath, string? packageName = null, bool autoInstall = true);

        /// <summary>
        /// Onboards multiple packages.
        /// </summary>
        /// <param name="filePaths">List of file paths to onboard</param>
        Task OnboardPackagesAsync(IEnumerable<string> filePaths);

        /// <summary>
        /// Installs a package.
        /// </summary>
        /// <param name="package">Package to install</param>
        Task<ResourcePackage> InstallPackageAsync(ResourcePackage package);

        /// <summary>
        /// Uninstalls a package.
        /// </summary>
        /// <param name="package">Package to uninstall</param>
        Task UninstallPackageAsync(ResourcePackage package);

        /// <summary>
        /// Reinstalls a package.
        /// </summary>
        /// <param name="package">Package to reinstall</param>
        Task ReinstallPackageAsync(ResourcePackage package);

        /// <summary>
        /// Removes a package completely.
        /// </summary>
        /// <param name="package">Package to remove</param>
        Task RemovePackageAsync(ResourcePackage package);

        /// <summary>
        /// Downloads and installs a package from a URL.
        /// </summary>
        /// <param name="url">URL to download from</param>
        /// <param name="packageName">Optional package name</param>
        Task<string> DownloadAndInstallAsync(string url, string? packageName = null);

        /// <summary>
        /// Toggles the favorite status of a package.
        /// </summary>
        /// <param name="package">Package to toggle</param>
        Task ToggleFavoriteAsync(ResourcePackage package);

        /// <summary>
        /// Toggles the locked status of a package.
        /// </summary>
        /// <param name="package">Package to toggle</param>
        Task ToggleLockedAsync(ResourcePackage package);

        /// <summary>
        /// Validates a package's integrity.
        /// </summary>
        /// <param name="package">Package to validate</param>
        Task<bool> ValidatePackageAsync(ResourcePackage package);

        /// <summary>
        /// Event fired when packages are updated.
        /// </summary>
        event EventHandler<PackageEventArgs>? PackagesUpdated;
    }

    /// <summary>
    /// Event arguments for package updates.
    /// </summary>
    public class PackageEventArgs : EventArgs
    {
        public IEnumerable<ResourcePackage>? Packages { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// Event arguments for package operations.
    /// </summary>
    public class PackageOperationEventArgs : EventArgs
    {
        public ResourcePackage? Package { get; set; }
        public string? Operation { get; set; }
        public string? Message { get; set; }
        public bool IsSuccess { get; set; }
        public Exception? Exception { get; set; }
    }
}
