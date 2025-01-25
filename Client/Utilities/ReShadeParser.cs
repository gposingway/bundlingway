using Bundlingway.Model;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Bundlingway.Utilities
{
    public static class ReShadeParser
    {
        private static readonly HttpClient client = new();

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

        private static (string version, string downloadLink) ExtractVersionAndDownloadLink(string htmlContent)
        {
            // Use regular expression to find the download link and version
            // This regex might need adjustments if the website structure changes
            string pattern = @"<a\s+href=""\/(?<downloadLink>downloads\/ReShade_Setup_[^""""]+)"".*>Download ReShade (?<version>.*) with full";
            Match match = Regex.Match(htmlContent, pattern);

            if (match.Success)
            {
                string version = match.Groups["version"].Value.Trim();
                string downloadLink = "https://reshade.me/" + match.Groups["downloadLink"].Value;
                return (version, downloadLink);
            }
            else
            {
                Console.Write("Could not find download link and version information.");
                return (string.Empty, string.Empty);
            }
        }

        public static async Task GetRemoteInfo()
        {
            var version = "N/A";
            var downloadLink = "N/A";

            // Fetch the webpage HTML content
            string htmlContent = await FetchHtmlContent("https://reshade.me/");

            if (!string.IsNullOrEmpty(htmlContent))
            {
                // Extract the version and download link using regular expressions
                (version, downloadLink) = ExtractVersionAndDownloadLink(htmlContent);

                if (string.IsNullOrEmpty(version) || string.IsNullOrEmpty(downloadLink))
                {
                    version = "N/A";
                    downloadLink = "N/A";
                }
            }

            Instances.LocalConfigProvider.Configuration.ReShade.RemoteVersion = version;
            Instances.LocalConfigProvider.Configuration.ReShade.RemoteLink = downloadLink;
        }

        internal static void GetLocalInfo()
        {
            if (Instances.LocalConfigProvider.Configuration.GameFolder != null)
            {
                var reShadeProbe = Path.Combine(Instances.LocalConfigProvider.Configuration.GameFolder, "dxgi.dll");

                if (!File.Exists(reShadeProbe))
                {
                    Instances.LocalConfigProvider.Configuration.ReShade.Status = "Not Installed";
                    Instances.LocalConfigProvider.Configuration.ReShade.LocalVersion = "N/A";
                    Instances.LocalConfigProvider.Configuration.ReShade.IsMissing = true;
                }
                else
                {
                    Instances.LocalConfigProvider.Configuration.ReShade.Status = "Found";
                    Instances.LocalConfigProvider.Configuration.ReShade.IsMissing = false;
                    var rfvi = FileVersionInfo.GetVersionInfo(reShadeProbe);
                    Instances.LocalConfigProvider.Configuration.ReShade.LocalVersion = rfvi.ProductVersion;
                }
            }
        }
    }
}
