using Bundlingway.Model;
using Bundlingway.Utilities.Extensions;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Bundlingway.Utilities
{
    public static class GPosingway
    {
        private static readonly HttpClient client = new();

        public static async Task<(bool success, string version)> GetRemoteInfo()
        {
            var c = Instances.LocalConfigProvider.Configuration.GPosingway;
            string methodName = ProcessHelper.GetCurrentMethodName();

            // Fetch the webpage HTML content
            string htmlContent = await FetchHtmlContent(Instances.GPosingwayConfigFileUrl);

            if (string.IsNullOrEmpty(htmlContent))
            {
                Log.Information($"{methodName}: Failed to fetch HTML content");
                return (false, string.Empty);
            }

            // Extract the version and download link using regular expressions
            var version = JObject.Parse(htmlContent).SelectToken("version")?.ToString() ?? null;
            var downloadUrl = JObject.Parse(htmlContent).SelectToken("version")?.ToString() ?? null;

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

            Log.Information($"{methodName}: Remote version fetched: {version}");

            if (c.LocalVersion != null && c.RemoteVersion != null && c.LocalVersion != c.RemoteVersion)
                c.Status = EPackageStatus.Outdated;

            _ = UI.UpdateElements();

            return (true, version);
        }

        private static async Task<string> FetchHtmlContent(string url)
        {
            string methodName = ProcessHelper.GetCurrentMethodName();
            Log.Information($"{methodName}: Fetching HTML content from {url}");
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string content = await response.Content.ReadAsStringAsync();
                Log.Information($"{methodName}: Successfully fetched HTML content");
                return content;
            }
            catch (HttpRequestException e)
            {
                Log.Warning($"{methodName}: Error fetching HTML content: {e.Message}");
                return string.Empty;
            }
        }

        internal static async Task GetLocalInfo()
        {
            string methodName = ProcessHelper.GetCurrentMethodName();
            try
            {
                if (Instances.LocalConfigProvider.Configuration.Game.ClientLocation != null)
                {
                    var localGposingwayConfigProbe = Path.Combine(Instances.BundlingwayDataFolder, Constants.Files.GPosingwayConfig);
                    var appDataGposingwayConfigExists = File.Exists(localGposingwayConfigProbe);

                    if (!appDataGposingwayConfigExists)
                    {
                        var gameGposingwayConfigProbe = Path.Combine(Instances.LocalConfigProvider.Configuration.Game.InstallationFolder, ".gposingway", Constants.Files.GPosingwayConfig);
                        var gameGposingwayConfigExists = File.Exists(gameGposingwayConfigProbe);

                        if (gameGposingwayConfigExists)
                        {
                            File.Copy(gameGposingwayConfigProbe, localGposingwayConfigProbe);
                            localGposingwayConfigProbe = Path.Combine(Instances.BundlingwayDataFolder, Constants.Files.GPosingwayConfig);
                            appDataGposingwayConfigExists = File.Exists(localGposingwayConfigProbe);
                        }
                    }

                    if (!appDataGposingwayConfigExists)
                    {
                        Instances.LocalConfigProvider.Configuration.GPosingway.Status = EPackageStatus.NotInstalled;
                        Instances.LocalConfigProvider.Configuration.GPosingway.LocalVersion = null;
                        Log.Information($"{methodName}: GPosingway not installed locally");
                    }
                    else
                    {
                        Instances.LocalConfigProvider.Configuration.GPosingway.Status = EPackageStatus.Installed;
                        Instances.LocalConfigProvider.Configuration.GPosingway.LocalVersion = localGposingwayConfigProbe.GetTokenValueFromFile("version");

                        Log.Information($"{methodName}: Local version found: {Instances.LocalConfigProvider.Configuration.GPosingway.LocalVersion}");
                    }
                }
                _ = UI.UpdateElements();
            }
            catch (Exception ex)
            {
                Log.Warning($"{methodName}: Error in CheckGPosingway: {ex.Message}");
            }

            Instances.LocalConfigProvider.Save();
            Log.Information($"{methodName}: Local info fetching completed");
        }

        internal static async Task Update()
        {
            string methodName = ProcessHelper.GetCurrentMethodName();

            var downloadUrl = Instances.GPosingwayConfigFileUrl;
            var destinationPath = Path.Combine(Instances.TempFolder, Constants.Files.GPosingwayConfig);

            try
            {
                using var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);

                response.EnsureSuccessStatusCode();

                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)); // Create the directory if it doesn't exist

                await using var fs = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                await using var stream = await response.Content.ReadAsStreamAsync();
                await stream.CopyToAsync(fs);

                Log.Information($"{methodName}: Successfully downloaded the file as gposingway-definitions-new.json.");
            }
            catch (Exception ex)
            {
                Log.Warning($"{methodName}: Error downloading the file: {ex.Message}");
            }

            // Read the JSON file...
            string jsonContent = await File.ReadAllTextAsync(destinationPath);

            var definitions = jsonContent.FromJson<GposingwayDefinitions>();

            if (string.IsNullOrEmpty(definitions.gposingwayUrl))
            {
                Log.Information($"{methodName}: Download link not found in JSON content.");
                return;
            }

            var storageFolder = Path.Combine(Instances.BundlingwayDataFolder, Constants.Folders.Core, Constants.Folders.GposingwayPackage);
            var storageFile = Path.Combine(storageFolder, Constants.Files.GPosingwayPackage);

            if (!Directory.Exists(storageFolder))
                Directory.CreateDirectory(storageFolder);

            var tempFolder = Path.Combine(Instances.TempFolder, Constants.Folders.GposingwayPackage);
            Directory.CreateDirectory(tempFolder);

            var gameFolder = Instances.LocalConfigProvider.Configuration.Game.InstallationFolder;

            if (string.IsNullOrEmpty(definitions.gposingwayUrl) || string.IsNullOrEmpty(tempFolder) || string.IsNullOrEmpty(gameFolder))
            {
                Log.Information($"{methodName}: Invalid configuration settings.");
                return;
            }

            try
            {

                _ = UI.Announce("Downloading the GPosingway package...");

                using (var response = await client.GetAsync(definitions.gposingwayUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    await using var fs = new FileStream(storageFile, FileMode.Create, FileAccess.Write, FileShare.None);
                    await using var stream = await response.Content.ReadAsStreamAsync();
                    await stream.CopyToAsync(fs);
                    Log.Information($"{methodName}: Successfully downloaded the file.");
                }

                var extractPath = Path.Combine(tempFolder, "Extracted");

                if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
                Directory.CreateDirectory(extractPath);

                _ = UI.Announce("Unpacking GPosingway...");

                using (var fs = new FileStream(storageFile, FileMode.Open, FileAccess.Read))
                using (var zf = new ZipFile(fs))
                {
                    foreach (ZipEntry entry in zf)
                    {
                        if (!entry.IsFile) continue;

                        var entryFileName = entry.Name;
                        var fullZipToPath = Path.Combine(extractPath, entryFileName);
                        var directoryName = Path.GetDirectoryName(fullZipToPath);

                        if (directoryName.Length > 0)
                            Directory.CreateDirectory(directoryName);

                        using (var zipStream = zf.GetInputStream(entry))
                        using (var streamWriter = File.Create(fullZipToPath))
                        {
                            zipStream.CopyTo(streamWriter);
                        }
                    }
                    Log.Information($"{methodName}: Successfully extracted the file.");
                }

                _ = UI.Announce("Installing GPosingway...");

                // Copy the content from extractPath + "reshade-shaders" to the game's "reshade-shaders" folder
                var sourcePath = Path.Combine(extractPath, Constants.Folders.GameShaders);
                var gameShaderPath = Path.Combine(gameFolder, Constants.Folders.GameShaders);

                if (Directory.Exists(sourcePath))
                {
                    foreach (var dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                    {
                        Directory.CreateDirectory(dirPath.Replace(sourcePath, gameShaderPath));
                    }

                    foreach (var newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                    {
                        File.Copy(newPath, newPath.Replace(sourcePath, gameShaderPath), true);
                    }

                    Log.Information($"{methodName}: Successfully copied reshade-shaders to the game folder.");
                }
                else
                {
                    Log.Information($"{methodName}: Source reshade-shaders folder does not exist.");
                }

                // Copy the content from extractPath + "reshade-presets" to the game's "reshade-presets" folder
                var sourcePresetsPath = Path.Combine(extractPath, Constants.Folders.GamePresets);
                var gamePresetsPath = Path.Combine(gameFolder, Constants.Folders.GamePresets);

                if (Directory.Exists(sourcePresetsPath))
                {
                    foreach (var dirPath in Directory.GetDirectories(sourcePresetsPath, "*", SearchOption.AllDirectories))
                    {
                        Directory.CreateDirectory(dirPath.Replace(sourcePresetsPath, gamePresetsPath));
                    }

                    foreach (var newPath in Directory.GetFiles(sourcePresetsPath, "*.*", SearchOption.AllDirectories))
                    {
                        File.Copy(newPath, newPath.Replace(sourcePresetsPath, gamePresetsPath), true);
                    }

                    Log.Information($"{methodName}: Successfully copied reshade-presets to the game folder.");
                }
                else
                {
                    Log.Information($"{methodName}: Source reshade-presets folder does not exist.");
                }

                // Overwrite the gposingway-definitions.json in the appdata folder with the copy on temp
                var appDataPath = Path.Combine(Instances.BundlingwayDataFolder, "gposingway-definitions.json");
                File.Copy(destinationPath, appDataPath, true);
                Log.Information($"{methodName}: Successfully copied gposingway-definitions.json to the appdata folder.");

                //cleanup
                Maintenance.RemoveTempDir();
            }
            catch (Exception ex)
            {
                Log.Warning($"{methodName}: Error during update: {ex.Message}");
            }
        }
    }
}
