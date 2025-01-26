
using Microsoft.Extensions.Logging;
using static System.Net.WebRequestMethods;

namespace Bundlingway.Model
{
    public class Instances
    {
        public static ConfigProvider<GPosingwayConfig> LocalConfigProvider { get; set; }
        public static string VersionedAppDataFolder { get; internal set; }
        public static string AppDataFolder { get; internal set; }
        public static BindingSource MainDataSource { get; set; }
        public static string AppDataCacheFolder { get; internal set; }
        public static string AppDataTempFolder { get; internal set; }

        public static List<ResourcePackage> ResourcePackages { get; set; } = [];

        public static string GPosingwayConfigFileName = "gposingway-definitions.json";
        public static string GPosingwayConfigFileUrl = "https://github.com/gposingway/gposingway/releases/latest/download/gposingway-definitions.json";
    }

}
