using Bundlingway.Core.Interfaces;
using Bundlingway.Model;
using Serilog;
using System.Collections.Generic;
using System.IO.Compression;

namespace Bundlingway.Core.Services
{
    /// <summary>
    /// Modern Bundlingway service that handles application updates without UI dependencies.
    /// </summary>
    public class BundlingwayService
    {
        private readonly IConfigurationService _configService;
        private readonly IHttpClientService _httpClient;
        private readonly IFileSystemService _fileSystem;
        private readonly IProgressReporter _progressReporter;
        private readonly IUserNotificationService _notifications;
        private readonly IAppEnvironmentService _envService;

        public BundlingwayService(
            IConfigurationService configService,
            IHttpClientService httpClient,
            IFileSystemService fileSystem,
            IProgressReporter progressReporter,
            IUserNotificationService notifications,
            IAppEnvironmentService envService)
        {
            _configService = configService;
            _httpClient = httpClient;
            _fileSystem = fileSystem;
            _progressReporter = progressReporter;
            _notifications = notifications;
            _envService = envService;
        }

        /// <summary>
        /// Gets the local Bundlingway information.
        /// </summary>
        public async Task GetLocalInfoAsync()
        {
            // Set the local version to the current application version
            _configService.Configuration.Bundlingway.LocalVersion = _envService.AppVersion;
            await _configService.SaveAsync();
        }

        /// <summary>
        /// Gets the remote Bundlingway information.
        /// </summary>
        public async Task GetRemoteInfoAsync()
        {
            try
            {
                Log.Information("BundlingwayService.GetRemoteInfo: Getting remote Bundlingway information.");
                // Download the remote configuration
                var headers = new Dictionary<string, string>
                {
                    ["Accept"] = "application/vnd.github.v3+json"
                };
                var jsonContent = await _httpClient.GetStringAsync(Constants.Urls.BundlingwayPackageLatestTag, headers);
                // Deserialize the GitHub release/tag API response
                var release = System.Text.Json.JsonSerializer.Deserialize<GitHubReleaseInfo>(jsonContent);
                var config = _configService.Configuration;
                config.Bundlingway.RemoteVersion = release?.TagName ?? "unknown";
                config.Bundlingway.RemoteLink = release?.Assets?.FirstOrDefault(a => a.Name != null && a.Name.EndsWith(".exe"))?.BrowserDownloadUrl ?? string.Empty;

                // Determine status
                if (string.IsNullOrEmpty(config.Bundlingway.LocalVersion))
                {
                    config.Bundlingway.Status = EPackageStatus.NotInstalled;
                }
                else if (config.Bundlingway.LocalVersion != config.Bundlingway.RemoteVersion)
                {
                    config.Bundlingway.Status = EPackageStatus.Outdated;
                }
                else
                {
                    config.Bundlingway.Status = EPackageStatus.Installed;
                }

                await _configService.SaveAsync();
                Log.Information("BundlingwayService.GetRemoteInfo: Remote Bundlingway information retrieved successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "BundlingwayService.GetRemoteInfo: Error getting remote Bundlingway information.");
                await _notifications.ShowErrorAsync("Failed to check for Bundlingway updates", ex);
            }
        }

        /// <summary>
        /// Updates the Bundlingway application to the latest version.
        /// </summary>
        public async Task UpdateAsync()
        {
            var config = _configService.Configuration.Bundlingway;

            await _notifications.AnnounceAsync("Downloading a new Bundlingway version...");

            // Set the location of the current executable
            var mainModule = System.Diagnostics.Process.GetCurrentProcess().MainModule;
            config.Location = mainModule != null && mainModule.FileName != null ? mainModule.FileName : string.Empty;
            await _configService.SaveAsync();

            // Create the storage folder if it doesn't exist
            var storageFolder = Path.Combine(_envService.BundlingwayDataFolder, Constants.Folders.Core, Constants.Folders.BundlingwayPackage);

            if (!_fileSystem.DirectoryExists(storageFolder))
                _fileSystem.CreateDirectory(storageFolder);

            // Get the file name from the remote link
            string fileName = string.Empty;
            if (!string.IsNullOrEmpty(config.RemoteLink))
            {
                fileName = Path.GetFileName(new Uri(config.RemoteLink).LocalPath);
            }
            string filePath = Path.Combine(storageFolder, fileName);

            // Create a temporary folder for extracting the package
            var tempFolder = Path.Combine(_envService.TempFolder, Constants.Folders.BundlingwayPackage);
            if (!_fileSystem.DirectoryExists(tempFolder))
                _fileSystem.CreateDirectory(tempFolder);

            try
            {
                // Download the file with progress reporting
                if (!string.IsNullOrEmpty(config.RemoteLink))
                {
                    await _httpClient.DownloadFileAsync(config.RemoteLink, filePath, _progressReporter);
                    Log.Information($"BundlingwayService.Update: Downloaded file to {filePath}.");
                    await _notifications.AnnounceAsync("Unzipping the new version...");
                }
                else
                {
                    Log.Warning("BundlingwayService.Update: RemoteLink is null or empty, skipping download.");
                    return;
                }

                // Unzip the downloaded file to the temp folder
                using (var archive = ZipFile.OpenRead(filePath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        // Sanitize the entry path
                        var entryPath = entry.FullName.Replace("/", Path.DirectorySeparatorChar.ToString());
                        if (string.IsNullOrWhiteSpace(entryPath) || entryPath.Contains(".."))
                            continue; // Prevent path traversal
                        var destinationPath = Path.GetFullPath(Path.Combine(tempFolder, entryPath));
                        if (!destinationPath.StartsWith(Path.GetFullPath(tempFolder)))
                            continue; // Prevent extraction outside tempFolder
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

                Log.Information($"BundlingwayService.Update: Extracted file to {tempFolder}.");

                // Check if a file with the same name as the executable exists
                string executablePath = Path.Combine(tempFolder, Path.GetFileName(config.Location));
                if (_fileSystem.FileExists(executablePath))
                {
                    // Start the executable with "update-client" command line argument
                    System.Diagnostics.Process.Start(executablePath, Constants.CommandLineOptions.UpdateClient);
                    Log.Information($"BundlingwayService.Update: Started {executablePath} with 'update-client' argument.");
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "BundlingwayService.Update: Error during update process.");
                await _notifications.ShowErrorAsync("Failed to update Bundlingway", ex);
                throw;
            }
        }
        /// <summary>
        /// Checks the status of Bundlingway by getting both local and remote information.
        /// </summary>
        public async Task CheckStatusAsync()
        {
            Log.Information("BundlingwayService.CheckStatusAsync: Starting combined local and remote info check.");

            // Check local info first
            await GetLocalInfoAsync();

            // Then check remote info
            await GetRemoteInfoAsync();

            Log.Information("BundlingwayService.CheckStatusAsync: Combined check completed.");
        }
    }
}
