using Bundlingway.Model;
using ICSharpCode.SharpZipLib.Zip;
using SharpCompress.Archives;
using SharpCompress.Common;
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

        internal static async Task Update()
        {
            var remoteLink = Instances.LocalConfigProvider.Configuration.ReShade.RemoteLink;
            var tempFolder = Path.Combine(Instances.LocalConfigProvider.localAppDataPath, "temp", "ReShade");
            var gameFolder = Instances.LocalConfigProvider.Configuration.GameFolder;

            if (string.IsNullOrEmpty(remoteLink) || string.IsNullOrEmpty(tempFolder) || string.IsNullOrEmpty(gameFolder))
            {
                Console.WriteLine("Invalid configuration settings.");
                return;
            }

            try
            {
                // Ensure cache folder exists
                Directory.CreateDirectory(tempFolder);

                // Download the file
                //var fileName = Path.Combine(tempFolder, Path.GetFileName(Instances.LocalConfigProvider.Configuration.ReShade.RemoteLink));
                var fileName = Path.Combine(tempFolder, "temp.zip");

                using (var response = await client.GetAsync(remoteLink, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    await using var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
                    await using var stream = await response.Content.ReadAsStreamAsync();
                    await stream.CopyToAsync(fs);
                }

                // Unzip the file using SharpZipLib
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
                }

                // Rename and copy the DLL
                var sourceDll = Path.Combine(extractPath, "ReShade64.dll");
                var destinationDll = Path.Combine(gameFolder, "dxgi.dll");
                if (File.Exists(sourceDll))
                {
                    File.Copy(sourceDll, destinationDll, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during update: {ex.Message}");
            }
        }
    }
}
