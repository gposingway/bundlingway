using System;
using System.Linq;
using Bundlingway.Model;
using HtmlAgilityPack;

namespace Bundlingway.Utilities
{
    public static class HtmlHelper
    {
        public static ReShadeDownloadInfo ParseReShadeDownloadInfo(string htmlContent)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlContent);
            var info = new ReShadeDownloadInfo();

            // Find the download section
            var downloadSection = doc.DocumentNode.SelectSingleNode("//h1[contains(text(),'Download')]/following-sibling::*");
            if (downloadSection == null)
                return info;            // Find the version (look for 'Version X.Y.Z' in strong tags)
            var versionNode = downloadSection.SelectSingleNode(".//strong[contains(text(),'Version')]");
            if (versionNode != null)
            {
                var text = versionNode.InnerText;
                var versionStart = text.IndexOf("Version ") + 8;
                if (versionStart >= 8)
                {
                    info.Version = text.Substring(versionStart).Trim();
                }
            }

            // Find download links
            var links = downloadSection.SelectNodes(".//a[contains(@href, 'ReShade_Setup_')]");
            if (links != null)
            {
                foreach (var link in links)
                {
                    var href = link.GetAttributeValue("href", "");
                    if (href.EndsWith(".exe"))
                    {
                        if (href.Contains("Addon"))
                            info.AddonDownloadUrl = href.StartsWith("http") ? href : $"https://reshade.me{href}";
                        else
                            info.DownloadUrl = href.StartsWith("http") ? href : $"https://reshade.me{href}";
                    }
                }
            }
            return info;
        }
    }
}
