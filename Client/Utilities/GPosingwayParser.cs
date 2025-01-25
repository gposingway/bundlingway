using Bundlingway.Model;
using Newtonsoft.Json.Linq;

namespace Bundlingway.Utilities
{
    public static class GPosingwayParser
    {
        private static readonly HttpClient client = new();

        // Usage: (bool success, string version, string downloadLink) = await FetchRemoteInfo();

        public static async Task<(bool success, string version)> GetRemoteInfo()
        {
            Console.WriteLine("GPosingwayParser.GetRemoteInfo: Start fetching remote info");

            // Fetch the webpage HTML content
            string htmlContent = await FetchHtmlContent("https://github.com/gposingway/gposingway/releases/latest/download/gposingway-definitions.json");

            if (string.IsNullOrEmpty(htmlContent))
            {
                Console.WriteLine("GPosingwayParser.GetRemoteInfo: Failed to fetch HTML content");
                return (false, string.Empty);
            }

            // Extract the version and download link using regular expressions
            var version = JObject.Parse(htmlContent).SelectToken("version")?.ToString() ?? "";

            if (version == null)
            {
                Console.WriteLine("GPosingwayParser.GetRemoteInfo: Version not found in HTML content");
                return (false, string.Empty);
            }

            Instances.LocalConfigProvider.Configuration.GPosingway.RemoteVersion = version;
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
                    var appDataGposingwayConfigProbe = Path.Combine(Instances.VersionedAppDataFolder, Instances.ConfigFileName);
                    var appDataGposingwayConfigExists = File.Exists(appDataGposingwayConfigProbe);

                    if (!appDataGposingwayConfigExists)
                    {
                        var gameGposingwayConfigProbe = Path.Combine(Instances.LocalConfigProvider.Configuration.GameFolder, ".gposingway", Instances.ConfigFileName);
                        var gameGposingwayConfigExists = File.Exists(gameGposingwayConfigProbe);

                        if (gameGposingwayConfigExists)
                        {
                            File.Copy(gameGposingwayConfigProbe, appDataGposingwayConfigProbe);
                            appDataGposingwayConfigProbe = Path.Combine(Instances.VersionedAppDataFolder, Instances.ConfigFileName);
                            appDataGposingwayConfigExists = File.Exists(appDataGposingwayConfigProbe);
                        }
                    }

                    if (!appDataGposingwayConfigExists)
                    {
                        Instances.LocalConfigProvider.Configuration.GPosingway.Status = "Not Installed";
                        Instances.LocalConfigProvider.Configuration.GPosingway.LocalVersion = "N/A";
                        Instances.LocalConfigProvider.Configuration.GPosingway.IsMissing = true;
                        Console.WriteLine("GPosingwayParser.GetLocalInfo: GPosingway not installed locally");
                    }
                    else
                    {
                        Instances.LocalConfigProvider.Configuration.GPosingway.Status = "Found";
                        Instances.LocalConfigProvider.Configuration.GPosingway.IsMissing = false;
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
    }
}
