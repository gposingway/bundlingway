using Bundlingway.Core.Interfaces;
using Bundlingway.Model;
using Bundlingway.Utilities.Extensions;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Bundlingway.Core.Services
{
    public class GPosingwayService
    {
        private readonly PackageService _packageService;
        private readonly IConfigurationService _configService;
        private readonly IUserNotificationService _notificationService;
        private readonly IFileSystemService _fileSystemService;
        private readonly IHttpClientService _httpClientService;
        private readonly IAppEnvironmentService _envService;

        public GPosingwayService(
            PackageService packageService,
            IConfigurationService configService,
            IUserNotificationService notificationService,
            IFileSystemService fileSystemService,
            IHttpClientService httpClientService,
            IAppEnvironmentService envService)
        {
            _packageService = packageService;
            _configService = configService;
            _notificationService = notificationService;
            _fileSystemService = fileSystemService;
            _httpClientService = httpClientService;
            _envService = envService;
        }

        public async Task<(bool success, string version)> GetRemoteInfoAsync()
        {
            var c = _configService.Configuration.GPosingway;
            string url = Constants.Urls.GPosingwayConfigFileUrl;
            string methodName = nameof(GetRemoteInfoAsync);

            // Fetch the webpage HTML content
            string htmlContent = await _httpClientService.GetStringAsync(url);

            if (string.IsNullOrEmpty(htmlContent))
            {
                Log.Information($"{methodName}: Failed to fetch HTML content");
                return (false, string.Empty);
            }

            // Extract the version and download link using JSON
            var version = JObject.Parse(htmlContent).SelectToken("version")?.ToString();
            var downloadUrl = JObject.Parse(htmlContent).SelectToken("gposingwayUrl")?.ToString();

            if (version == null)
            {
                Log.Information($"{methodName}: Version not found in HTML content");
                return (false, string.Empty);
            }

            if (downloadUrl == null)
            {
                Log.Information($"{methodName}: DownloadUrl not found in HTML content");
                return (false, string.Empty);
            }

            c.RemoteVersion = version;
            c.RemoteLink = downloadUrl;
            await _configService.SaveAsync();
            Log.Information($"{methodName}: Remote version fetched: {version}");

            if (!string.IsNullOrEmpty(c.LocalVersion) && c.LocalVersion != c.RemoteVersion)
                c.Status = EPackageStatus.Outdated;

            await _notificationService.AnnounceAsync($"GPosingway remote version: {version}");
            return (true, version);
        }

        public async Task GetLocalInfoAsync()
        {
            string methodName = nameof(GetLocalInfoAsync);
            var c = _configService.Configuration.GPosingway;
            var gposingwayPackage = await _packageService.GetPackageByNameAsync(Bundlingway.Constants.GPosingwayDefaultPackage(_envService).Name);

            if (gposingwayPackage == null)
            {
                c.Status = EPackageStatus.NotInstalled;
                c.LocalVersion = string.Empty;
                Log.Information($"{methodName}: GPosingway not installed locally");
            }
            else
            {
                c.Status = EPackageStatus.Installed;
                c.LocalVersion = gposingwayPackage.Version;
                Log.Information($"{methodName}: Local version found: {c.LocalVersion}");
            }
            await _configService.SaveAsync();
            await _notificationService.AnnounceAsync($"GPosingway local status: {c.Status}, version: {c.LocalVersion}");
        }

        public async Task UpdateAsync(bool force = true)
        {
            string methodName = nameof(UpdateAsync);

            try
            {

                var c = _configService.Configuration.GPosingway;
                var configUrl = Constants.Urls.GPosingwayConfigFileUrl;

                // Prepare or get the package using the service
                var gposingwayPackage = await _packageService.GetPackageByNameAsync(Bundlingway.Constants.GPosingwayDefaultPackage(_envService).Name);

                if (gposingwayPackage == null)
                {
                    gposingwayPackage = Constants.GPosingwayDefaultPackage(_envService);
                    _packageService.SaveAsync(gposingwayPackage).Wait(); // Ensure package is saved before proceeding
                }


                var configDestPath = Path.Combine(gposingwayPackage.LocalFolder, Bundlingway.Constants.Files.GPosingwayConfig);


                // Step 1: Download the gposingway-definitions.json config file
                if (force || !_fileSystemService.FileExists(configDestPath))
                {
                    await _notificationService.AnnounceAsync($"Downloading {Bundlingway.Constants.Files.GPosingwayConfig}...");
                    var response = await _httpClientService.GetAsync(configUrl);
                    response.EnsureSuccessStatusCode();
                    var dirPath = Path.GetDirectoryName(configDestPath);
                    if (!string.IsNullOrEmpty(dirPath))
                        _fileSystemService.CreateDirectory(dirPath);
                    using (var fs = _fileSystemService.OpenWrite(configDestPath))
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        await stream.CopyToAsync(fs);
                    }
                    Log.Information($"{methodName}: Successfully downloaded the gposingway-definitions.json file.");
                }

                // Step 2: Parse the config file to get version and download URL
                var configContent = await _fileSystemService.ReadAllTextAsync(configDestPath);
                var gposingwayDef = JObject.Parse(configContent);
                var version = gposingwayDef["version"]?.ToString();
                var packageUrl = gposingwayDef["gposingwayUrl"]?.ToString();

                if (string.IsNullOrEmpty(version) || string.IsNullOrEmpty(packageUrl))
                {
                    Log.Warning($"{methodName}: Invalid config file - missing version or URL");
                    await _notificationService.AnnounceAsync("Error: Invalid GPosingway configuration file");
                    return;
                }

                // Check if we need to update
                if (!force && gposingwayPackage.Version == version)
                {
                    Log.Information($"{methodName}: Already up to date (version {version})");
                    await _notificationService.AnnounceAsync($"GPosingway is already up to date (version {version})");
                    return;
                }

                // Step 3: Download the actual GPosingway package ZIP
                await _notificationService.AnnounceAsync($"Downloading GPosingway package v{version}...");
                var packageZipPath = Path.Combine(gposingwayPackage.LocalFolder, Constants.Files.GPosingwayPackage);

                var packageResponse = await _httpClientService.GetAsync(packageUrl);
                packageResponse.EnsureSuccessStatusCode();
                using (var fs = _fileSystemService.OpenWrite(packageZipPath))
                using (var stream = await packageResponse.Content.ReadAsStreamAsync())
                {
                    await stream.CopyToAsync(fs);
                }
                Log.Information($"{methodName}: Successfully downloaded GPosingway package.");

                // Step 4: Extract the ZIP file
                await _notificationService.AnnounceAsync("Extracting GPosingway package...");
                var extractPath = Path.Combine(gposingwayPackage.LocalFolder, "extracted");
                if (_fileSystemService.DirectoryExists(extractPath))
                    _fileSystemService.DeleteDirectory(extractPath, true);
                _fileSystemService.CreateDirectory(extractPath);

                using (var fileStream = _fileSystemService.OpenRead(packageZipPath))
                using (var zipStream = new ZipInputStream(fileStream))
                {
                    ZipEntry entry;
                    while ((entry = zipStream.GetNextEntry()) != null)
                    {
                        if (!entry.IsFile) continue;

                        var entryPath = Path.Combine(extractPath, entry.Name);
                        var entryDir = Path.GetDirectoryName(entryPath);
                        if (!string.IsNullOrEmpty(entryDir))
                            _fileSystemService.CreateDirectory(entryDir);

                        using (var output = _fileSystemService.OpenWrite(entryPath))
                        {
                            zipStream.CopyTo(output);
                        }
                    }
                }
                Log.Information($"{methodName}: Successfully extracted GPosingway package.");

                // Step 5: Copy shaders and presets to both package and game directories
                await _notificationService.AnnounceAsync("Installing GPosingway files...");
                var gameFolder = _configService.Configuration.Game.InstallationFolder;
                if (string.IsNullOrEmpty(gameFolder))
                {
                    Log.Warning($"{methodName}: Game folder not found - cannot install files");
                    await _notificationService.AnnounceAsync("Error: Game folder not found");
                    return;
                }

                if (string.IsNullOrEmpty(gposingwayPackage.LocalShaderFolder)) // Ensure the package has a local shader folder
                    gposingwayPackage.LocalShaderFolder = Path.Combine(gameFolder, Constants.Folders.GameShaders, Constants.Folders.PackageShaders);

                if (string.IsNullOrEmpty(gposingwayPackage.LocalPresetFolder)) // Ensure the package has a local preset folder
                    gposingwayPackage.LocalPresetFolder = Path.Combine(gameFolder, Constants.Folders.GamePresets);

                if (string.IsNullOrEmpty(gposingwayPackage.LocalTextureFolder)) // Ensure the package has a local texture folder
                    gposingwayPackage.LocalTextureFolder = Path.Combine(gameFolder, Constants.Folders.GameShaders, Constants.Folders.PackageTextures);  

                var shadersSource = Path.Combine(extractPath, Constants.Folders.GameShaders, Constants.Folders.PackageShaders);
                if (_fileSystemService.DirectoryExists(shadersSource))
                {
                    // Copy to package's local folders
                    var packageShadersDestination = Path.Combine(gposingwayPackage.LocalFolder, Constants.Folders.PackageShaders);
                    CopyDirectory(shadersSource, packageShadersDestination);

                    Log.Information($"{methodName}: Copied shaders to package directory.");
                }

                var texturesSource = Path.Combine(extractPath, Constants.Folders.GameShaders, Constants.Folders.PackageTextures);
                if (_fileSystemService.DirectoryExists(texturesSource))
                {
                    // Copy to package's local folders
                    var packagetexturesDestination = Path.Combine(gposingwayPackage.LocalFolder, Constants.Folders.PackageTextures);
                    CopyDirectory(texturesSource, packagetexturesDestination);

                    Log.Information($"{methodName}: Copied textures to package and game directory.");
                }

                // Copy presets to both package and game directories
                var presetsSource = Path.Combine(extractPath, Constants.Folders.GamePresets);
                if (_fileSystemService.DirectoryExists(presetsSource))
                {
                    // Copy to game's preset folder
                    var packagepresetsDestination = Path.Combine(gposingwayPackage.LocalFolder, Constants.Folders.PackagePresets);
                    CopyDirectory(presetsSource, packagepresetsDestination);

                    Log.Information($"{methodName}: Copied presets to package directoriy.");
                }
                
                // Step 6: Update package version and status
                gposingwayPackage.Version = version;
                gposingwayPackage.Label = $"GPosingway v{version}";
                gposingwayPackage.Status = ResourcePackage.EStatus.NotInstalled;

                _packageService.SaveAsync(gposingwayPackage).Wait(); // Ensure package is saved before proceeding

                // Run post-processing pipeline
                await Task.Run(() => new PostProcessorService(_envService).RunPipeline(gposingwayPackage));

                // Final installation step
                await _packageService.InstallPackageAsync(gposingwayPackage);

                // Step 7: Cleanup
                if (_fileSystemService.FileExists(packageZipPath))
                    _fileSystemService.DeleteFile(packageZipPath);
                if (_fileSystemService.DirectoryExists(extractPath))
                    _fileSystemService.DeleteDirectory(extractPath, true);

                // Update configuration
                c.LocalVersion = version;
                c.Status = EPackageStatus.Installed;
                await _configService.SaveAsync();

                await _notificationService.AnnounceAsync($"GPosingway v{version} successfully installed!");
                Log.Information($"{methodName}: GPosingway update completed successfully.");
            }
            catch (Exception ex)
            {
                Log.Warning($"{methodName}: Error during GPosingway update: {ex.Message}");
                await _notificationService.AnnounceAsync("Error during GPosingway update; check logs for details.");
            }
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            if (!_fileSystemService.DirectoryExists(destinationDir))
                _fileSystemService.CreateDirectory(destinationDir);

            var files = _fileSystemService.GetFiles(sourceDir);
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var destFile = Path.Combine(destinationDir, fileName);
                _fileSystemService.CopyFile(file, destFile, true);
            }

            var directories = _fileSystemService.GetDirectories(sourceDir);
            foreach (var directory in directories)
            {
                var dirName = Path.GetFileName(directory);
                var destDir = Path.Combine(destinationDir, dirName);
                CopyDirectory(directory, destDir);
            }
        }

        public async Task CheckStatusAsync()
        {
            Log.Information("GPosingwayService.CheckStatusAsync: Starting combined local and remote info check.");

            // Check local info first
            await GetLocalInfoAsync();

            // Then check remote info
            await GetRemoteInfoAsync();

            Log.Information("GPosingwayService.CheckStatusAsync: Combined check completed.");
        }
    }
}
