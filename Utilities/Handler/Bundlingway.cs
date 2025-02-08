using Serilog;
using System.Text.Json;
using Windows.Storage;

namespace Bundlingway.Utilities.Handler
{
    public static class Bundlingway
    {

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

        internal static async Task Update()
        {
            var b = Instances.LocalConfigProvider.Configuration.Bundlingway;

            b.Location = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            Instances.LocalConfigProvider.Save();

            var storageFolder = Path.Combine(Instances.BundlingwayDataFolder, Constants.Folders.Core, Constants.Folders.BundlingwayPackage);

            if (!Directory.Exists(storageFolder)) Directory.CreateDirectory(storageFolder);

            using HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync(b.RemoteLink);
            response.EnsureSuccessStatusCode();
            byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();

            string fileName = Path.GetFileName(new Uri(b.RemoteLink).LocalPath);
            string filePath = Path.Combine(storageFolder, fileName);
            await File.WriteAllBytesAsync(filePath, fileBytes);

            Log.Information($"Bundlingway.Update: Downloaded file to {filePath}.");

            var tempFolder = Path.Combine(Instances.TempFolder, Constants.Folders.BundlingwayPackage);
            if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);

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