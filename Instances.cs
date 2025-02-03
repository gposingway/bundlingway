using Bundlingway.Model;
using Bundlingway.PostProcess.PresetItem;
using Bundlingway.Utilities;
using System.Reflection;

namespace Bundlingway
{
    public class Instances
    {
        public static ConfigProvider<GPosingwayConfig> LocalConfigProvider { get; set; }
        public static string AppName { get; private set; } = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;
        public static string AppVersion { get; private set; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static string OSAppDataPath { get; private set; } = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        public static string BundlingwayDataFolder { get; set; } = Path.Combine(OSAppDataPath, AppName);
        public static string ConfigFilePath { get; private set; } = Path.Combine(BundlingwayDataFolder, Constants.Files.BundlingwayConfig);
        public static string TempFolder { get; private set; } = Path.Combine(BundlingwayDataFolder, Constants.Folders.Temp);
        public static string CacheFolder { get; set; } =  Path.Combine(BundlingwayDataFolder, Constants.Folders.Cache);
        public static string PackageFolder { get; set; } = Path.Combine(BundlingwayDataFolder, Constants.Folders.Packages);
        public static string SinglePresetsFolder { get; set; } = Path.Combine(PackageFolder, Constants.Folders.SinglePresets);
        public static bool IsGameRunning = false;

        public static List<ResourcePackage> ResourcePackages { get; set; } = [];
        public static Dictionary<string, ShaderPackage> Packages { get; set; } = [];
        public static string GPosingwayConfigFileUrl = "https://github.com/gposingway/gposingway/releases/latest/download/gposingway-definitions.json";

        public static List<IRawFileProcess> RawFileProcessors = IoC.GetClassesByInterface<IRawFileProcess>().CreateInstances<IRawFileProcess>().ToList();
        public static List<IPresetProcess> PresetProcessors = IoC.GetClassesByInterface<IPresetProcess>().CreateInstances<IPresetProcess>().ToList();
    }
}
