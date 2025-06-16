using Bundlingway.Model;
using Bundlingway.Utilities.Extensions;
using Serilog;
using SharpCompress.Archives;
using System.IO.Compression;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using SharpCompress.Common;
using System.Collections.Concurrent;
using Bundlingway.Utilities.ManagedResources;
using System.Diagnostics.Eventing.Reader;

namespace Bundlingway.Utilities.Handler
{
    [Obsolete("All logic has been migrated to PackageService. Use IPackageService via ServiceLocator.")]
    public static class Package
    {
        // All methods are now obsolete and throw NotImplementedException
        [Obsolete("Use IPackageService.OnboardPackageAsync instead.")]
        internal static Task<ResourcePackage> OnboardSinglePresetFile(string filePath, string? presetName = null)
            => throw new NotImplementedException("Use IPackageService.OnboardPackageAsync instead.");

        [Obsolete("Use IPackageService.PreparePackageAsync instead.")]
        internal static Task<ResourcePackage> Prepare(ResourcePackage package, bool force = false)
            => throw new NotImplementedException("Use IPackageService.PreparePackageAsync instead.");

        [Obsolete("Use IPackageService.GetPackageByNameAsync instead.")]
        internal static ResourcePackage GetByName(string packageName)
            => throw new NotImplementedException("Use IPackageService.GetPackageByNameAsync instead.");

        [Obsolete("Use IPackageService.OnboardPackageAsync instead.")]
        internal static Task<ResourcePackage> Onboard(string filePath, string? packageName = null, bool autoInstall = true)
            => throw new NotImplementedException("Use IPackageService.OnboardPackageAsync instead.");

        [Obsolete("Use IPackageService.InstallPackageAsync instead.")]
        public static Task<ResourcePackage> Install(string sourcePackagePath)
            => throw new NotImplementedException("Use IPackageService.InstallPackageAsync instead.");

        [Obsolete("Use IPackageService.RefreshInstalledPackagesAsync instead.")]
        public static Task RefreshInstalled()
            => throw new NotImplementedException("Use IPackageService.RefreshInstalledPackagesAsync instead.");

        [Obsolete("Use IPackageService.ScanPackagesAsync instead.")]
        public static Task Scan()
            => throw new NotImplementedException("Use IPackageService.ScanPackagesAsync instead.");

        [Obsolete("Use IPackageService.RemovePackageAsync instead.")]
        internal static void Remove(ResourcePackage package)
            => throw new NotImplementedException("Use IPackageService.RemovePackageAsync instead.");

        [Obsolete("Use IPackageService.ToggleFavoriteAsync instead.")]
        public static void ToggleFavorite(ResourcePackage package)
            => throw new NotImplementedException("Use IPackageService.ToggleFavoriteAsync instead.");

        [Obsolete("Use IPackageService.ToggleLockedAsync instead.")]
        internal static void ToggleLocked(ResourcePackage package)
            => throw new NotImplementedException("Use IPackageService.ToggleLockedAsync instead.");

        [Obsolete("Use IPackageService.SavePackageToFolderAsync instead.")]
        public static void SavePackageToFolder(ResourcePackage package)
            => throw new NotImplementedException("Use IPackageService.SavePackageToFolderAsync instead.");

        private static string GetHashSignature(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            byte[] hashBytes = SHA256.HashData(stream);
            StringBuilder hash = new();
            foreach (byte b in hashBytes)
            {
                hash.Append(b.ToString("X2"));
            }
            return hash.ToString().Substring(0, 6);
        }

        private static bool ValidatePackage(string tempFolderPath)
        {
            // Validate if the directory exists
            if (!Directory.Exists(tempFolderPath))
            {
                return false;
            }

            // Run individual validation methods
            bool hasIniFiles = ValidateIniFiles(tempFolderPath);
            bool hasFxFiles = ValidateFxFiles(tempFolderPath);

            // A package with no INIs but with FX is a shader package.
            bool validationFlag = hasIniFiles || (!hasIniFiles && hasFxFiles);

            Console.WriteLine(validationFlag ? "Package is valid." : "Package is not valid.");
            return validationFlag;
        }

        private static bool ValidateIniFiles(string tempFolderPath)
        {
            // Search for .INI files in the directory and its subdirectories
            string[] iniFiles = Directory.GetFiles(tempFolderPath, "*.ini", SearchOption.AllDirectories);

            // Validate if there is at least one .INI file
            return iniFiles.Length > 0;
        }

        private static bool ValidateFxFiles(string tempFolderPath)
        {
            // Search for .fx files in the directory and its subdirectories
            string[] fxFiles = Directory.GetFiles(tempFolderPath, "*.fx", SearchOption.AllDirectories);

            // Validate if there is at least one .fx file
            return fxFiles.Length > 0;
        }
    }
}
