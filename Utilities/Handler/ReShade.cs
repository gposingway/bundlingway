using Bundlingway.Model;
using Bundlingway.Properties;
using ICSharpCode.SharpZipLib.Zip;
using Serilog;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Bundlingway.Utilities.Handler
{
    public static class ReShade
    {
        private static readonly HttpClient client = new();

        private static async Task<string> FetchHtmlContent(string url)
        {
            Log.Information("ReShadeParser.FetchHtmlContent: Fetching HTML content from URL: " + url);
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                Log.Information("ReShadeParser.FetchHtmlContent: Successfully fetched HTML content.");
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                Log.Warning($"ReShadeParser.FetchHtmlContent: Error fetching HTML content: {e.Message}");
                return string.Empty;
            }
        }

        private static (string version, string downloadLink) ExtractVersionAndDownloadLink(string htmlContent)
        {
            Log.Information("ReShadeParser.ExtractVersionAndDownloadLink: Extracting version and download link from HTML content.");
            string pattern = @"<a\s+href=""\/(?<downloadLink>downloads\/ReShade_Setup_[^""""]+)"".*>Download ReShade (?<version>.*) with full";
            Match match = Regex.Match(htmlContent, pattern);

            if (match.Success)
            {
                string version = match.Groups["version"].Value.Trim();
                string downloadLink = "https://reshade.me/" + match.Groups["downloadLink"].Value;
                Log.Information("ReShadeParser.ExtractVersionAndDownloadLink: Successfully extracted version and download link.");
                return (version, downloadLink);
            }
            else
            {
                Log.Information("ReShadeParser.ExtractVersionAndDownloadLink: Could not find download link and version information.");
                return (string.Empty, string.Empty);
            }
        }

        public static async Task GetRemoteInfo()
        {
            var c = Instances.LocalConfigProvider.Configuration.ReShade;

            Log.Information("ReShadeParser.GetRemoteInfo: Starting to get remote info.");
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

            c.RemoteVersion = version;
            c.RemoteLink = downloadLink;
            Log.Information("ReShadeParser.GetRemoteInfo: Remote info updated. Version: " + version + ", Download Link: " + downloadLink);

            if (c.LocalVersion != null && c.RemoteVersion != null && c.LocalVersion != c.RemoteVersion)
                c.Status = EPackageStatus.Outdated;


            _ = UI.UpdateElements();

        }

        internal static async Task GetLocalInfo()
        {
            Log.Information("ReShadeParser.GetLocalInfo: Starting to get local info.");
            if (Instances.LocalConfigProvider.Configuration.Game.InstallationFolder != null)
            {
                var reShadeProbe = Path.Combine(Instances.LocalConfigProvider.Configuration.Game.InstallationFolder, Constants.Files.LocalReshadeBinary);

                if (!File.Exists(reShadeProbe))
                {
                    Instances.LocalConfigProvider.Configuration.ReShade.Status = EPackageStatus.NotInstalled;
                    Instances.LocalConfigProvider.Configuration.ReShade.LocalVersion = null;
                    Log.Information("ReShadeParser.GetLocalInfo: ReShade not installed.");
                }
                else
                {
                    Instances.LocalConfigProvider.Configuration.ReShade.Status = EPackageStatus.Installed;
                    var rfvi = await Task.Run(() => FileVersionInfo.GetVersionInfo(reShadeProbe));
                    Instances.LocalConfigProvider.Configuration.ReShade.LocalVersion = rfvi?.ProductVersion;
                    Log.Information("ReShadeParser.GetLocalInfo: ReShade found. Version: " + rfvi?.ProductVersion);
                }
            }
            _ = UI.UpdateElements();
        }

        internal static async Task Update()
        {
            var c = Instances.LocalConfigProvider.Configuration.ReShade;

            _ = UI.Announce("Updating ReShade...");

            Log.Information("ReShadeParser.Update: Starting update process.");

            if (Instances.IsGameRunning & c.Status != EPackageStatus.NotInstalled ) return;

            var remoteLink = Instances.LocalConfigProvider.Configuration.ReShade.RemoteLink;
            var tempFolder = Path.Combine(Instances.TempFolder, "ReShade");
            var gameFolder = Instances.LocalConfigProvider.Configuration.Game.InstallationFolder;

            if (string.IsNullOrEmpty(remoteLink) || string.IsNullOrEmpty(tempFolder) || string.IsNullOrEmpty(gameFolder))
            {
                Log.Information("ReShadeParser.Update: Invalid configuration settings.");
                return;
            }

            try
            {
                Directory.CreateDirectory(tempFolder);
                var fileName = Path.Combine(tempFolder, "temp.zip");

                using (var response = client.GetAsync(remoteLink, HttpCompletionOption.ResponseHeadersRead).Result)
                {
                    response.EnsureSuccessStatusCode();
                    using var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
                    using var stream = response.Content.ReadAsStreamAsync().Result;
                    stream.CopyTo(fs);
                    Log.Information("ReShadeParser.Update: Successfully downloaded the file.");
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

                        using var zipStream = zf.GetInputStream(entry);
                        using var streamWriter = File.Create(fullZipToPath);
                        zipStream.CopyTo(streamWriter);
                    }
                    Log.Information("ReShadeParser.Update: Successfully extracted the file.");
                }

                var sourceDll = Path.Combine(extractPath, "ReShade64.dll");
                var destinationDll = Path.Combine(gameFolder, Constants.Files.LocalReshadeBinary);
                if (File.Exists(sourceDll))
                {
                    File.Copy(sourceDll, destinationDll, true);
                    Log.Information("ReShadeParser.Update: Successfully copied the DLL.");
                }

                var destinationIni = Path.Combine(gameFolder, Constants.Files.LocalReshadeConfig);
                if (!File.Exists(destinationIni))
                {
                    var placeholderFile = Resources.ReShade_ini;

                    File.WriteAllText(destinationIni, Encoding.UTF8.GetString(placeholderFile));
                    Log.Information("ReShadeParser.Update: Successfully created the INI file.");
                }

                _ = UI.Announce("ReShade successfully updated!");

            }
            catch (Exception ex)
            {
                Log.Warning($"ReShadeParser.Update: Error during update: {ex.Message}");
                _ = UI.Announce("Error during ReShade update; check logs for details.");
            }
        }
    }
}
