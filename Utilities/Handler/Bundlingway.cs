using Bundlingway.Utilities.Extensions;
using Serilog;
using System.Text.Json;

namespace Bundlingway.Utilities.Handler
{
    public static class Bundlingway
    {
        internal static async Task FixIt(bool refreshCache = false)
        {
            await UI.Announce("Doing Hydaelyn's work, fixing the game's shaders...");

            await Backup();

            // wipe all contents from the reshade-shaders/shaders folders
            var gameFolder = Instances.LocalConfigProvider.Configuration.Game.InstallationFolder;
            var reshadeShadersFolder = Path.Combine(gameFolder, Constants.Folders.GameShaders, Constants.Folders.PackageShaders);
            if (Directory.Exists(reshadeShadersFolder)) Directory.Delete(reshadeShadersFolder, true);

            await ReShade.Update();
            await GPosingway.Update(refreshCache);

            //Reinstall all installed Shader packages
            await Package.Scan();
            foreach (var package in Instances.ResourcePackages)
            {
                if (package.Type == Model.ResourcePackage.EType.ShaderCollection && package.Status == Model.ResourcePackage.EStatus.Installed)
                    await package.Install();
            }

            _ = UI.Announce("Done!");

        }

        internal static async Task<string> Backup()
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            try
            {
                _ = UI.Announce($"Backing up the reshade-shaders and reshade-presets folders to {timestamp}...");

                // Backup the whole content of the game folder's reshade-shaders and reshade-presets folders to the backup folder
                var backupFolder = Path.Combine(Instances.BundlingwayDataFolder, Constants.Folders.Backup);
                if (!Directory.Exists(backupFolder)) Directory.CreateDirectory(backupFolder);

                // Create a timestamped backup folder
                var backupFolderTimestamped = Path.Combine(backupFolder, timestamp);
                Directory.CreateDirectory(backupFolderTimestamped);

                var gameFolder = Instances.LocalConfigProvider.Configuration.Game.InstallationFolder;

                // Backup reshade-shaders folder
                var reshadeShadersFolder = Path.Combine(gameFolder, Constants.Folders.GameShaders);
                if (Directory.Exists(reshadeShadersFolder))
                {
                    var destinationFolder = Path.Combine(backupFolderTimestamped, Constants.Folders.GameShaders);
                    FS.DirectoryCopy(reshadeShadersFolder, destinationFolder, true);
                }

                // Backup reshade-presets folder
                var reshadePresetsFolder = Path.Combine(gameFolder, Constants.Folders.GamePresets);
                if (Directory.Exists(reshadePresetsFolder))
                {
                    var destinationFolder = Path.Combine(backupFolderTimestamped, Constants.Folders.GamePresets);
                    FS.DirectoryCopy(reshadePresetsFolder, destinationFolder, true);
                }

                // Also copy the reshade.ini file
                var reshadeIniFile = Path.Combine(gameFolder, Constants.Files.LocalReshadeConfig);
                if (File.Exists(reshadeIniFile))
                {
                    var destinationFile = Path.Combine(backupFolderTimestamped, Constants.Files.LocalReshadeConfig);
                    File.Copy(reshadeIniFile, destinationFile, true);
                }

                // Keep only the last 5 backup directories
                var backupDirectories = Directory.GetDirectories(backupFolder).OrderByDescending(d => d).Skip(5);
                foreach (var dir in backupDirectories) { Directory.Delete(dir, true); }
            }
            catch (Exception e)
            {
                Log.Error($"Bundlingway.Backup: Error in Backup: {e.Message}");
            }

            return timestamp;
        }


        internal static async Task GetLocalInfo()
        {
            Instances.LocalConfigProvider.Configuration.Bundlingway.LocalVersion = Instances.AppVersion;
        }

        internal static async Task GetRemoteInfo()
        {
            try
            {
                var b = Instances.LocalConfigProvider.Configuration.Bundlingway;

                using HttpClient client = new();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

                HttpResponseMessage response = await client.GetAsync(Constants.Urls.BundlingwayPackageLatestTag);
                response.EnsureSuccessStatusCode();
                string content = await response.Content.ReadAsStringAsync();

                var jsonDocument = JsonDocument.Parse(content);
                string tagName = jsonDocument.RootElement.GetProperty("tag_name").GetString();

                if (tagName.StartsWith("v")) tagName = tagName.Substring(1);

                // Convert all parts of the tagName to numeric
                tagName = string.Join(".", tagName.Split('.').Select(part => int.Parse(part).ToString()));

                b.RemoteVersion = tagName;

                string downloadUrl = jsonDocument.RootElement.GetProperty("assets")[0].GetProperty("browser_download_url").GetString();
                b.RemoteLink = downloadUrl;

                Log.Information($"Bundlingway.GetRemoteInfo: Remote version is {tagName}, download URL is {downloadUrl}.");

            }
            catch (Exception e)
            {
                Log.Error($"Bundlingway.GetRemoteInfo: Error in GetRemoteInfo: {e.Message}");
            }
        }

        internal static async Task Update(Control control)
        {
            var b = Instances.LocalConfigProvider.Configuration.Bundlingway;

            _ = UI.Announce("Downloading a new Bundlingway version...");

            b.Location = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            Instances.LocalConfigProvider.Save();

            var storageFolder = Path.Combine(Instances.BundlingwayDataFolder, Constants.Folders.Core, Constants.Folders.BundlingwayPackage);

            if (!Directory.Exists(storageFolder)) Directory.CreateDirectory(storageFolder);

            string fileName = Path.GetFileName(new Uri(b.RemoteLink).LocalPath);
            string filePath = Path.Combine(storageFolder, fileName);

            var tempFolder = Path.Combine(Instances.TempFolder, Constants.Folders.BundlingwayPackage);
            if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);


            using (HttpClient client = new())
            using (HttpResponseMessage response = await client.GetAsync(b.RemoteLink, HttpCompletionOption.ResponseHeadersRead)) // Get headers first
            {
                response.EnsureSuccessStatusCode();

                long? totalBytes = response.Content.Headers.ContentLength;
                long downloadedBytes = 0;
                byte[] buffer = new byte[8192]; // 8KB buffer
                bool progressReported = false; // Flag to avoid reporting 100% twice

                control?.DoAction(() => control.Text = "Downloading 0%"); // Initial text

                using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true)) // Use FileStream for async operations
                {
                    long totalFileSize = totalBytes.HasValue ? totalBytes.Value : -1;

                    _ = UI.StartProgress(totalFileSize);

                    while (true)
                    {
                        int bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                        {
                            // End of stream
                            break;
                        }

                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        downloadedBytes += bytesRead;


                        if (totalFileSize > 0) // Only report progress if total size is known
                        {
                            _ = UI.SetProgress(downloadedBytes);
                            double percentage = (double)downloadedBytes / totalFileSize * 100;
                            control?.DoAction(() => control.Text = $"Downloading {percentage:F0}%"); // Update progress in UI
                            progressReported = percentage >= 100; // Set flag when reaching 100% (or close to it due to potential rounding)
                        }
                    }

                    if (totalFileSize <= 0 || !progressReported) // Report 100% if size was unknown or if not reported in loop
                    {
                        _ = UI.SetProgress(totalFileSize);
                        control?.DoAction(() => control.Text = $"Downloading 100%"); // Final 100% update
                    }

                    Log.Information($"Bundlingway.Update: Downloaded file to {filePath}.");
                }
            }

            _ = UI.Announce("Unzipping the new version...");

            control?.DoAction(() => control.Text = "Unzipping...");

            // Unzip the downloaded file to the temp folder
            System.IO.Compression.ZipFile.ExtractToDirectory(filePath, tempFolder, true);

            Log.Information($"Bundlingway.Update: Extracted file to {tempFolder}.");

            // Check if a file with the same name as the executable exists
            string executablePath = Path.Combine(tempFolder, Path.GetFileName(b.Location));
            if (File.Exists(executablePath))
            {
                // Start the executable with "update-client" command line argument
                System.Diagnostics.Process.Start(executablePath, Constants.CommandLineOptions.UpdateClient);
                Log.Information($"Bundlingway.Update: Started {executablePath} with 'update-client' argument.");
                Environment.Exit(0);
            }
        }
    }
}