using ICSharpCode.SharpZipLib.Zip;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Bundlingway.Utilities.Handler
{
    public static class ReShade
    {
        private static readonly HttpClient client = new();

        private static async Task<string> FetchHtmlContent(string url)
        {
            Console.WriteLine("ReShadeParser.FetchHtmlContent: Fetching HTML content from URL: " + url);
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                Console.WriteLine("ReShadeParser.FetchHtmlContent: Successfully fetched HTML content.");
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"ReShadeParser.FetchHtmlContent: Error fetching HTML content: {e.Message}");
                return string.Empty;
            }
        }

        private static (string version, string downloadLink) ExtractVersionAndDownloadLink(string htmlContent)
        {
            Console.WriteLine("ReShadeParser.ExtractVersionAndDownloadLink: Extracting version and download link from HTML content.");
            string pattern = @"<a\s+href=""\/(?<downloadLink>downloads\/ReShade_Setup_[^""""]+)"".*>Download ReShade (?<version>.*) with full";
            Match match = Regex.Match(htmlContent, pattern);

            if (match.Success)
            {
                string version = match.Groups["version"].Value.Trim();
                string downloadLink = "https://reshade.me/" + match.Groups["downloadLink"].Value;
                Console.WriteLine("ReShadeParser.ExtractVersionAndDownloadLink: Successfully extracted version and download link.");
                return (version, downloadLink);
            }
            else
            {
                Console.WriteLine("ReShadeParser.ExtractVersionAndDownloadLink: Could not find download link and version information.");
                return (string.Empty, string.Empty);
            }
        }

        public static async Task GetRemoteInfo()
        {
            Console.WriteLine("ReShadeParser.GetRemoteInfo: Starting to get remote info.");
            var version = "N/A";
            var downloadLink = "N/A";

            string htmlContent = await FetchHtmlContent("https://reshade.me/");

            if (!string.IsNullOrEmpty(htmlContent))
            {
                (version, downloadLink) = ExtractVersionAndDownloadLink(htmlContent);

                if (string.IsNullOrEmpty(version) || string.IsNullOrEmpty(downloadLink))
                {
                    version = "N/A";
                    downloadLink = "N/A";
                }
            }

            Instances.LocalConfigProvider.Configuration.ReShade.RemoteVersion = version;
            Instances.LocalConfigProvider.Configuration.ReShade.RemoteLink = downloadLink;
            Console.WriteLine("ReShadeParser.GetRemoteInfo: Remote info updated. Version: " + version + ", Download Link: " + downloadLink);
        }

        internal static void GetLocalInfo()
        {
            Console.WriteLine("ReShadeParser.GetLocalInfo: Starting to get local info.");
            if (Instances.LocalConfigProvider.Configuration.GameFolder != null)
            {
                var reShadeProbe = Path.Combine(Instances.LocalConfigProvider.Configuration.GameFolder, "dxgi.dll");

                if (!File.Exists(reShadeProbe))
                {
                    Instances.LocalConfigProvider.Configuration.ReShade.Status = "Not Installed";
                    Instances.LocalConfigProvider.Configuration.ReShade.LocalVersion = "N/A";
                    Console.WriteLine("ReShadeParser.GetLocalInfo: ReShade not installed.");
                }
                else
                {
                    Instances.LocalConfigProvider.Configuration.ReShade.Status = "Found";
                    var rfvi = FileVersionInfo.GetVersionInfo(reShadeProbe);
                    Instances.LocalConfigProvider.Configuration.ReShade.LocalVersion = rfvi.ProductVersion;
                    Console.WriteLine("ReShadeParser.GetLocalInfo: ReShade found. Version: " + rfvi.ProductVersion);
                }
            }
        }

        internal static async Task Update()
        {
            Console.WriteLine("ReShadeParser.Update: Starting update process.");
            var remoteLink = Instances.LocalConfigProvider.Configuration.ReShade.RemoteLink;
            var tempFolder = Path.Combine(Instances.TempFolder, "ReShade");
            var gameFolder = Instances.LocalConfigProvider.Configuration.GameFolder;

            if (string.IsNullOrEmpty(remoteLink) || string.IsNullOrEmpty(tempFolder) || string.IsNullOrEmpty(gameFolder))
            {
                Console.WriteLine("ReShadeParser.Update: Invalid configuration settings.");
                return;
            }

            try
            {
                Directory.CreateDirectory(tempFolder);
                var fileName = Path.Combine(tempFolder, "temp.zip");

                using (var response = await client.GetAsync(remoteLink, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    await using var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
                    await using var stream = await response.Content.ReadAsStreamAsync();
                    await stream.CopyToAsync(fs);
                    Console.WriteLine("ReShadeParser.Update: Successfully downloaded the file.");
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
                    Console.WriteLine("ReShadeParser.Update: Successfully extracted the file.");
                }

                var sourceDll = Path.Combine(extractPath, "ReShade64.dll");
                var destinationDll = Path.Combine(gameFolder, "dxgi.dll");
                if (File.Exists(sourceDll))
                {
                    File.Copy(sourceDll, destinationDll, true);
                    Console.WriteLine("ReShadeParser.Update: Successfully copied the DLL.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ReShadeParser.Update: Error during update: {ex.Message}");
            }
        }
    }
}
