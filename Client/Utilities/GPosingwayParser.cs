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
            // Fetch the webpage HTML content
            string htmlContent = await FetchHtmlContent("https://github.com/gposingway/gposingway/releases/latest/download/gposingway-definitions.json");

            if (string.IsNullOrEmpty(htmlContent))
            {
                return (false, string.Empty);
            }

            // Extract the version and download link using regular expressions
            var version = JObject.Parse(htmlContent).SelectToken("version")?.ToString() ?? "";

            if (version == null)
            {
                return (false, string.Empty);
            }

            Instances.LocalConfigProvider.Configuration.GPosingway.RemoteVersion = version;

            return (true, version);
        }

        private static async Task<string> FetchHtmlContent(string url)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                Console.Write($"Error fetching HTML content: {e.Message}");
                return string.Empty;
            }
        }

        internal static void GetLocalInfo()
        {
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
                    }
                    else
                    {
                        Instances.LocalConfigProvider.Configuration.GPosingway.Status = "Found";
                        Instances.LocalConfigProvider.Configuration.GPosingway.IsMissing = false;
                        Instances.LocalConfigProvider.Configuration.GPosingway.LocalVersion = JObject.Parse(File.ReadAllText(appDataGposingwayConfigProbe)).SelectToken("version")?.ToString() ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in CheckGPosingway: {ex.Message}");
            }

            Instances.LocalConfigProvider.Save();
        }
    }
}
