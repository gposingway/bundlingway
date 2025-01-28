using Bundlingway.Model;
using Bundlingway.PostProcess.PresetItem;
using Bundlingway.Utilities;

namespace Bundlingway
{
    public class Instances
    {
        public static ConfigProvider<GPosingwayConfig> LocalConfigProvider { get; set; }
        public static string DataFolder { get; set; }
        public static BindingSource MainDataSource { get; set; }
        public static string TempFolder { get; set; }

        public static List<ResourcePackage> ResourcePackages { get; set; } = [];
        public static Dictionary<string, ShaderPackage> Packages { get; set; } = [];
        public static string PackageFolder { get; set; }

        public static string GPosingwayConfigFileName = "gposingway-definitions.json";
        public static string GPosingwayConfigFileUrl = "https://github.com/gposingway/gposingway/releases/latest/download/gposingway-definitions.json";

        public static List<string> ValidTextureExtensions { get; set; } = [".jpg", ".jpeg",".png", ".dds"];

        public static List<IRawFileProcess> RawFileProcessors = IoC.GetClassesByInterface<IRawFileProcess>().CreateInstances<IRawFileProcess>().ToList();
        public static List<IPresetProcess> PresetProcessors = IoC.GetClassesByInterface<IPresetProcess>().CreateInstances<IPresetProcess>().ToList();

    }

}
