using Bundlingway.Model;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;

namespace Bundlingway.Utilities
{
    public static class GPosingway
    {
        private static readonly HttpClient client = new();

        // Usage: (bool success, string version, string downloadLink) = await FetchRemoteInfo();

        public static async Task<(bool success, string version)> GetRemoteInfo()
        {
            Console.WriteLine("GPosingwayParser.GetRemoteInfo: Start fetching remote info");

            // Fetch the webpage HTML content
            string htmlContent = await FetchHtmlContent(Instances.GPosingwayConfigFileUrl);

            if (string.IsNullOrEmpty(htmlContent))
            {
                Console.WriteLine("GPosingwayParser.GetRemoteInfo: Failed to fetch HTML content");
                return (false, string.Empty);
            }

            // Extract the version and download link using regular expressions
            var version = JObject.Parse(htmlContent).SelectToken("version")?.ToString() ?? "";
            var downloadUrl = JObject.Parse(htmlContent).SelectToken("version")?.ToString() ?? "";

            if (version == null)
            {
                Console.WriteLine("GPosingwayParser.GetRemoteInfo: Version not found in HTML content");
                return (false, string.Empty);
            }

            if (downloadUrl == null)
            {
                Console.WriteLine("GPosingwayParser.GetRemoteInfo: DownloadUrl not found in HTML content");
                return (false, string.Empty);
            }

            Instances.LocalConfigProvider.Configuration.GPosingway.RemoteVersion = version;
            Instances.LocalConfigProvider.Configuration.GPosingway.RemoteLink = downloadUrl;

            Console.WriteLine($"GPosingwayParser.GetRemoteInfo: Remote version fetched: {version}");

            return (true, version);
        }

        private static async Task<string> FetchHtmlContent(string url)
        {
            Console.WriteLine($"GPosingwayParser.FetchHtmlContent: Fetching HTML content from {url}");
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string content = await response.Content.ReadAsStringAsync();
                Console.WriteLine("GPosingwayParser.FetchHtmlContent: Successfully fetched HTML content");
                return content;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"GPosingwayParser.FetchHtmlContent: Error fetching HTML content: {e.Message}");
                return string.Empty;
            }
        }

        internal static void GetLocalInfo()
        {
            Console.WriteLine("GPosingwayParser.GetLocalInfo: Start fetching local info");
            try
            {
                if (Instances.LocalConfigProvider.Configuration.XIVPath != null)
                {
                    var appDataGposingwayConfigProbe = Path.Combine(Instances.DataFolder, Instances.GPosingwayConfigFileName);
                    var appDataGposingwayConfigExists = File.Exists(appDataGposingwayConfigProbe);

                    if (!appDataGposingwayConfigExists)
                    {
                        var gameGposingwayConfigProbe = Path.Combine(Instances.LocalConfigProvider.Configuration.GameFolder, ".gposingway", Instances.GPosingwayConfigFileName);
                        var gameGposingwayConfigExists = File.Exists(gameGposingwayConfigProbe);

                        if (gameGposingwayConfigExists)
                        {
                            File.Copy(gameGposingwayConfigProbe, appDataGposingwayConfigProbe);
                            appDataGposingwayConfigProbe = Path.Combine(Instances.DataFolder, Instances.GPosingwayConfigFileName);
                            appDataGposingwayConfigExists = File.Exists(appDataGposingwayConfigProbe);
                        }
                    }

                    if (!appDataGposingwayConfigExists)
                    {
                        Instances.LocalConfigProvider.Configuration.GPosingway.Status = "Not Installed";
                        Instances.LocalConfigProvider.Configuration.GPosingway.LocalVersion = "N/A";
                        Console.WriteLine("GPosingwayParser.GetLocalInfo: GPosingway not installed locally");
                    }
                    else
                    {
                        Instances.LocalConfigProvider.Configuration.GPosingway.Status = "Found";
                        Instances.LocalConfigProvider.Configuration.GPosingway.LocalVersion = JObject.Parse(File.ReadAllText(appDataGposingwayConfigProbe)).SelectToken("version")?.ToString() ?? "";
                        Console.WriteLine($"GPosingwayParser.GetLocalInfo: Local version found: {Instances.LocalConfigProvider.Configuration.GPosingway.LocalVersion}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GPosingwayParser.GetLocalInfo: Error in CheckGPosingway: {ex.Message}");
            }

            Instances.LocalConfigProvider.Save();
            Console.WriteLine("GPosingwayParser.GetLocalInfo: Local info fetching completed");
        }

        internal static async Task Update()
        {
            Console.WriteLine("GPosingwayParser.Update: Starting update process.");

            //Download the file from Instances.GPosingwayConfigFileUrl and store it in Instances.AppDataTempFolder as gposingway-definitions-new.json
            var downloadUrl = Instances.GPosingwayConfigFileUrl;
            var destinationPath = Path.Combine(Instances.TempFolder, "gposingway-definitions.json");

            try
            {
                using (var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)); // Create the directory if it doesn't exist

                    await using var fs = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                    await using var stream = await response.Content.ReadAsStreamAsync();
                    await stream.CopyToAsync(fs);
                    Console.WriteLine("GPosingwayParser.Update: Successfully downloaded the file as gposingway-definitions-new.json.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GPosingwayParser.Update: Error downloading the file: {ex.Message}");
            }

            // Read the JSON file...
            string jsonContent = await File.ReadAllTextAsync(destinationPath);

            var definitions = jsonContent.FromJson<GposingwayDefinitions>();

            if (string.IsNullOrEmpty(definitions.gposingwayUrl))
            {
                Console.WriteLine("GPosingwayParser.Update: Download link not found in JSON content.");
                return;
            }

            var tempFolder = Path.Combine(Instances.TempFolder, "GPosingway");
            var gameFolder = Instances.LocalConfigProvider.Configuration.GameFolder;

            if (string.IsNullOrEmpty(definitions.gposingwayUrl) || string.IsNullOrEmpty(tempFolder) || string.IsNullOrEmpty(gameFolder))
            {
                Console.WriteLine("GPosingwayParser.Update: Invalid configuration settings.");
                return;
            }

            try
            {
                Directory.CreateDirectory(tempFolder);
                var fileName = Path.Combine(tempFolder, "gposingway.zip");

                using (var response = await client.GetAsync(definitions.gposingwayUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    await using var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
                    await using var stream = await response.Content.ReadAsStreamAsync();
                    await stream.CopyToAsync(fs);
                    Console.WriteLine("GPosingwayParser.Update: Successfully downloaded the file.");
                }

                var extractPath = Path.Combine(tempFolder, "Extracted");
                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                }
                Directory.CreateDirectory(extractPath);

                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
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
                    Console.WriteLine("GPosingwayParser.Update: Successfully extracted the file.");
                }

                // Copy the content from extractPath + "reshade-shaders" to the game's "reshade-shaders" folder
                var sourcePath = Path.Combine(extractPath, "reshade-shaders");
                var gameShaderPath = Path.Combine(gameFolder, "reshade-shaders");

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

                    Console.WriteLine("GPosingwayParser.Update: Successfully copied reshade-shaders to the game folder.");
                }
                else
                {
                    Console.WriteLine("GPosingwayParser.Update: Source reshade-shaders folder does not exist.");
                }

                // Copy the content from extractPath + "reshade-presets" to the game's "reshade-presets" folder
                var sourcePresetsPath = Path.Combine(extractPath, "reshade-presets");
                var gamePresetsPath = Path.Combine(gameFolder, "reshade-presets");

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

                    Console.WriteLine("GPosingwayParser.Update: Successfully copied reshade-presets to the game folder.");
                }
                else
                {
                    Console.WriteLine("GPosingwayParser.Update: Source reshade-presets folder does not exist.");
                }

                // Overwrite the gposingway-definitions.json in the appdata folder with the copy on temp
                var appDataPath = Path.Combine(Instances.DataFolder, "gposingway-definitions.json");
                File.Copy(destinationPath, appDataPath, true);
                Console.WriteLine("GPosingwayParser.Update: Successfully copied gposingway-definitions.json to the appdata folder.");

                //cleanup
                Maintenance.RemoveTempDir();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GPosingwayParser.Update: Error during update: {ex.Message}");
            }
        }
    }
}
