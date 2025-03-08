using Bundlingway.Model;
using Bundlingway.Utilities.Extensions;
using Bundlingway.Utilities.Handler;
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
            string htmlContent = await FetchHtmlContent(Constants.Urls.GPosingwayConfigFileUrl);

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

            ResourcePackage gposingwayPackage = Handler.Package.GetByName(Constants.GPosingwayDefaultPackage.Name);

            if (gposingwayPackage == null)
            {
                Instances.LocalConfigProvider.Configuration.GPosingway.Status = EPackageStatus.NotInstalled;
                Instances.LocalConfigProvider.Configuration.GPosingway.LocalVersion = null;
                Log.Information($"{methodName}: GPosingway not installed locally");
            }
            else
            {
                Instances.LocalConfigProvider.Configuration.GPosingway.Status = EPackageStatus.Installed;
                Instances.LocalConfigProvider.Configuration.GPosingway.LocalVersion = gposingwayPackage.Version;

                Log.Information($"{methodName}: Local version found: {Instances.LocalConfigProvider.Configuration.GPosingway.LocalVersion}");
            }
            _ = UI.UpdateElements();

            Instances.LocalConfigProvider.Save();
            Log.Information($"{methodName}: Local info fetching completed");
        }

        internal static async Task Update(bool force = true)
        {
            string methodName = ProcessHelper.GetCurrentMethodName();

            var downloadUrl = Constants.Urls.GPosingwayConfigFileUrl;

            var gposingwayPackage = Package.Prepare(Constants.GPosingwayDefaultPackage, true).Result;

            var destinationPath = Path.Combine(gposingwayPackage.LocalFolder, Constants.Files.GPosingwayConfig);

            try
            {
                if (force || !File.Exists(destinationPath))
                {
                    UI.Announce($"Downloading {Constants.Files.GPosingwayConfig}...");

                    using HttpClient client = new();
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Bundlingway Client" + Instances.AppVersion);
                    var response = client.GetAsync(downloadUrl).Result;

                    response.EnsureSuccessStatusCode();

                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)); // Create the directory if it doesn't exist

                    using var fs = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                    using var stream = response.Content.ReadAsStream();

                    stream.CopyTo(fs);

                    Log.Information($"{methodName}: Successfully downloaded the gposingway-definitions.json file.");
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"{methodName}: Error downloading the file: {ex.Message}");
                return; // Early return in case of download failure
            }

            try
            {
                // Read the JSON file...
                string jsonContent = File.ReadAllText(destinationPath);

                var definitions = jsonContent.FromJson<GposingwayDefinitions>();

                if (string.IsNullOrEmpty(definitions.gposingwayUrl))
                {
                    Log.Information($"{methodName}: Download link not found in JSON content.");
                    return;
                }

                gposingwayPackage.Version = definitions.version;
                gposingwayPackage.Label = $"GPosingway {definitions.version}";
                gposingwayPackage.Save();

                var storageFolder = Path.Combine(gposingwayPackage.LocalFolder, Constants.Folders.SourcePackage);
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

                var extractPath = Path.Combine(tempFolder, "Extracted");

                if (force || !File.Exists(storageFile))
                {
                    UI.Announce("Downloading the GPosingway package...");

                    using (HttpClient client = new())
                    {
                        var response = client.GetAsync(definitions.gposingwayUrl, HttpCompletionOption.ResponseHeadersRead).Result;
                        response.EnsureSuccessStatusCode();
                        long? totalBytes = response.Content.Headers.ContentLength;
                        long downloadedBytes = 0;
                        byte[] buffer = new byte[8192]; // 8KB buffer
                        bool progressReported = false;

                        using var fs = new FileStream(storageFile, FileMode.Create, FileAccess.Write, FileShare.None);
                        using var stream = response.Content.ReadAsStream();

                        long totalFileSize = totalBytes.HasValue ? totalBytes.Value : -1;
                        _ = UI.StartProgress(totalFileSize);

                        while (true)
                        {
                            int bytesRead = stream.Read(buffer, 0, buffer.Length);
                            if (bytesRead == 0)
                            {
                                // End of stream
                                break;
                            }

                            fs.Write(buffer, 0, bytesRead);
                            downloadedBytes += bytesRead;

                            if (totalFileSize > 0) // Only report progress if total size is known
                            {
                                _ = UI.SetProgress(downloadedBytes);
                                double percentage = (double)downloadedBytes / totalFileSize * 100;
                                progressReported = percentage >= 100;
                            }
                        }
                        if (totalFileSize <= 0 || !progressReported) // Report 100% if size was unknown or if not reported in loop
                        {
                            _ = UI.SetProgress(totalFileSize);
                        }
                        Log.Information($"{methodName}: Successfully downloaded the file.");
                    }

                    if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
                    Directory.CreateDirectory(extractPath);

                    _ = UI.Announce("Unpacking GPosingway...");
                    _ = UI.StartProgress(100);
                    _ = UI.SetProgress(0);

                    using (var fs = new FileStream(storageFile, FileMode.Open, FileAccess.Read))
                    using (var zf = new ZipFile(fs))
                    {
                        long totalEntries = zf.Count;
                        long processedEntries = 0;

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
                            processedEntries++;
                            double percentage = (double)processedEntries / totalEntries * 100;
                            _ = UI.SetProgress((long)percentage);
                        }
                        Log.Information($"{methodName}: Successfully extracted the file.");
                    }

                    _ = UI.Announce("Installing GPosingway...");

                    // Copy the content from extractPath + "reshade-shaders" to the game's "reshade-shaders" folder
                    var sourcePath = Path.Combine(extractPath, Constants.Folders.GameShaders);

                    if (Directory.Exists(sourcePath))
                    {
                        foreach (var dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                        {
                            Directory.CreateDirectory(dirPath.Replace(sourcePath, gposingwayPackage.LocalShaderFolder));
                        }

                        foreach (var newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                        {
                            File.Copy(newPath, newPath.Replace(sourcePath, gposingwayPackage.LocalShaderFolder), true);
                        }

                        Log.Information($"{methodName}: Successfully copied reshade-shaders to the GPosingway package folder.");
                    }
                    else
                    {
                        Log.Information($"{methodName}: Source reshade-shaders folder does not exist.");
                    }

                    // Copy the content from extractPath + "reshade-presets" to the game's "reshade-presets" folder
                    var sourcePresetsPath = Path.Combine(extractPath, Constants.Folders.GamePresets);

                    if (Directory.Exists(sourcePresetsPath))
                    {
                        foreach (var dirPath in Directory.GetDirectories(sourcePresetsPath, "*", SearchOption.AllDirectories))
                        {
                            Directory.CreateDirectory(dirPath.Replace(sourcePresetsPath, gposingwayPackage.LocalPresetFolder));
                        }

                        foreach (var newPath in Directory.GetFiles(sourcePresetsPath, "*.*", SearchOption.AllDirectories))
                        {
                            File.Copy(newPath, newPath.Replace(sourcePresetsPath, gposingwayPackage.LocalPresetFolder), true);
                        }

                        Log.Information($"{methodName}: Successfully copied reshade-presets to the GPosingway package folder.");
                    }
                    else
                    {
                        Log.Information($"{methodName}: Source reshade-presets folder does not exist.");
                    }

                    //cleanup
                    Maintenance.RemoveTempDir();
                }

                try
                {
                    Task.Run(() => PostProcessor.RunPipeline(gposingwayPackage)).Wait();

                    gposingwayPackage.Status = ResourcePackage.EStatus.Installed;
                    gposingwayPackage.Save();

                    Package.Install(gposingwayPackage.LocalFolder).Wait();
                }
                catch (Exception e)
                {
                    Log.Error($"{methodName}: {e.Message}.");
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"{methodName}: Error during update: {ex.Message}");
            }
            finally
            {
                UI.StopProgress();
            }
        }
    }
}
