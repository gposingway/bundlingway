
using Microsoft.Extensions.Logging;

namespace Bundlingway.Model
{
    public class Instances
    {
        public static ConfigProvider<GPosingwayConfig> LocalConfigProvider { get; set; }
        public static string VersionedAppDataFolder { get; internal set; }
        public static string AppDataFolder { get; internal set; }
        public static BindingSource MainDataSource { get; set; }
        public static string ConfigFileName = "gposingway-definitions.json";
    }

}
