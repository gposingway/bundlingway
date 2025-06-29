using Bundlingway.Core.Interfaces;
using Bundlingway.Model;
using Bundlingway.Properties;
using ICSharpCode.SharpZipLib.Zip;
using Serilog;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Bundlingway.Core.Services
{
    /// <summary>
    /// Service-based implementation for ReShade operations.
    /// </summary>
    public class ReShadeService
    {
        private readonly IConfigurationService _configService;
        private readonly IFileSystemService _fileSystem;
        private readonly IUserNotificationService _notifications;
        private readonly IHttpClientService _httpClient;
        private readonly IAppEnvironmentService _envService;

        public ReShadeService(
            IConfigurationService configService,
            IFileSystemService fileSystem,
            IUserNotificationService notifications,
            IHttpClientService httpClient,
            IAppEnvironmentService envService)
        {
            _configService = configService;
            _fileSystem = fileSystem;
            _notifications = notifications;
            _httpClient = httpClient;
            _envService = envService;
        }

        public async Task GetRemoteInfoAsync()
        {
            var c = _configService.Configuration.ReShade;
            Log.Information("ReShadeService.GetRemoteInfoAsync: Starting to get remote info.");
            var version = "N/A";
            var downloadLink = "N/A";
            string htmlContent = await _httpClient.GetStringAsync("https://reshade.me/");
            if (!string.IsNullOrEmpty(htmlContent))
            {
                (version, downloadLink) = ExtractVersionAndDownloadLink(htmlContent);
                if (string.IsNullOrEmpty(version) || string.IsNullOrEmpty(downloadLink))
                {
                    version = "N/A";
                    downloadLink = "N/A";
                }
            }
            c.RemoteVersion = version;
            c.RemoteLink = downloadLink;
            Log.Information($"ReShadeService.GetRemoteInfoAsync: Remote info updated. Version: {version}, Download Link: {downloadLink}");
            if (!string.IsNullOrEmpty(c.LocalVersion) && !string.IsNullOrEmpty(c.RemoteVersion) && c.LocalVersion != c.RemoteVersion)
                c.Status = EPackageStatus.Outdated;
            await _configService.SaveAsync();
            await _notifications.AnnounceAsync($"ReShade remote version: {version}");
        }

        private (string version, string downloadLink) ExtractVersionAndDownloadLink(string htmlContent)
        {
            var info = Bundlingway.Utilities.HtmlHelper.ParseReShadeDownloadInfo(htmlContent);
            return (info.Version ?? string.Empty, info.AddonDownloadUrl ?? string.Empty);
        }

        public async Task GetLocalInfoAsync()
        {
            Log.Information("ReShadeService.GetLocalInfoAsync: Starting to get local info.");
            var c = _configService.Configuration;
            if (!string.IsNullOrEmpty(c.Game.InstallationFolder))
            {
                var reShadeProbe = Path.Combine(c.Game.InstallationFolder, Constants.Files.LocalReshadeBinary);
                if (!_fileSystem.FileExists(reShadeProbe))
                {
                    c.ReShade.Status = EPackageStatus.NotInstalled;
                    c.ReShade.LocalVersion = null;
                    Log.Information("ReShadeService.GetLocalInfoAsync: ReShade not installed.");
                }
                else
                {
                    c.ReShade.Status = EPackageStatus.Installed;
                    var rfvi = await Task.Run(() => FileVersionInfo.GetVersionInfo(reShadeProbe));
                    c.ReShade.LocalVersion = rfvi?.ProductVersion;
                    Log.Information($"ReShadeService.GetLocalInfoAsync: ReShade found. Version: {rfvi?.ProductVersion}");
                }
            }
            await _notifications.AnnounceAsync($"ReShade local info checked.");
        }

        public async Task UpdateAsync()
        {
            var c = _configService.Configuration.ReShade;
            await _notifications.AnnounceAsync("Updating ReShade...");
            Log.Information("ReShadeService.UpdateAsync: Starting update process.");

            // Use Instances.IsGameRunning if available, else skip check
            if (_envService.IsGameRunning && c.Status != EPackageStatus.NotInstalled) return;

            var remoteLink = c.RemoteLink;
            var tempFolder = Path.Combine(_envService.TempFolder, "ReShade");
            var gameFolder = _configService.Configuration.Game.InstallationFolder;

            if (string.IsNullOrEmpty(remoteLink) || string.IsNullOrEmpty(tempFolder) || string.IsNullOrEmpty(gameFolder))
            {
                Log.Information("ReShadeService.UpdateAsync: Invalid configuration settings.");
                return;
            }

            try
            {
                _fileSystem.CreateDirectory(tempFolder);
                var fileName = Path.Combine(tempFolder, "temp.zip");
                // Download the file with proper streaming
                using (var stream = await _httpClient.GetStreamAsync(remoteLink))
                using (var fs = _fileSystem.OpenWrite(fileName))
                {
                    await stream.CopyToAsync(fs);
                    Log.Information("ReShadeService.UpdateAsync: Successfully downloaded the file.");
                }

                var extractPath = Path.Combine(tempFolder, "Extracted");
                if (_fileSystem.DirectoryExists(extractPath))
                    _fileSystem.DeleteDirectory(extractPath, true);
                _fileSystem.CreateDirectory(extractPath);

                // Extract ZIP file using SharpZipLib
                using (var fs = _fileSystem.OpenRead(fileName))
                using (var zf = new ZipFile(fs))
                {
                    foreach (ZipEntry entry in zf)
                    {
                        if (!entry.IsFile) continue;

                        var entryFileName = entry.Name;
                        var fullZipToPath = Path.Combine(extractPath, entryFileName);
                        var directoryName = Path.GetDirectoryName(fullZipToPath);

                        if (!string.IsNullOrEmpty(directoryName))
                            _fileSystem.CreateDirectory(directoryName);

                        using var zipStream = zf.GetInputStream(entry);
                        using var streamWriter = _fileSystem.OpenWrite(fullZipToPath);
                        await zipStream.CopyToAsync(streamWriter);
                    }
                    Log.Information("ReShadeService.UpdateAsync: Successfully extracted the file.");
                }

                var sourceDll = Path.Combine(extractPath, "ReShade64.dll");
                var destinationDll = Path.Combine(gameFolder, Constants.Files.LocalReshadeBinary);
                if (_fileSystem.FileExists(sourceDll))
                {
                    _fileSystem.CopyFile(sourceDll, destinationDll, true);
                    Log.Information("ReShadeService.UpdateAsync: Successfully copied the DLL.");
                }

                var destinationIni = Path.Combine(gameFolder, Constants.Files.LocalReshadeConfig);
                if (!_fileSystem.FileExists(destinationIni))
                {
                    // Write placeholder INI content
                    var placeholderContent = GetDefaultReshadeIniContent();
                    await _fileSystem.WriteAllTextAsync(destinationIni, placeholderContent);
                    Log.Information("ReShadeService.UpdateAsync: Successfully created the INI file.");
                }

                await _notifications.AnnounceAsync("ReShade successfully updated!");
                await CheckStatusAsync();
            }
            catch (Exception ex)
            {
                Log.Warning($"ReShadeService.UpdateAsync: Error during update: {ex.Message}");
                await _notifications.AnnounceAsync("Error during ReShade update; check logs for details.");
            }
        }

        private string GetDefaultReshadeIniContent()
        {
            return Encoding.UTF8.GetString(Resources.ReShade_ini);
        }

        public async Task CheckStatusAsync()
        {
            Log.Information("ReShadeService.CheckStatusAsync: Starting combined local and remote info check.");
            
            // Check local info first
            await GetLocalInfoAsync();
            
            // Then check remote info
            await GetRemoteInfoAsync();
            
            Log.Information("ReShadeService.CheckStatusAsync: Combined check completed.");
        }
    }
}
