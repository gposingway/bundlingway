using Bundlingway.Model;
using Bundlingway.PostProcess.PresetItem;
using Bundlingway.Utilities;
using System.Reflection;

namespace Bundlingway.Core.Services
{
    public interface IAppEnvironmentService
    {
        string AppName { get; }
        string AppVersion { get; }
        string OSAppDataPath { get; }
        string BundlingwayDataFolder { get; set; }
        string ConfigFilePath { get; }
        string TempFolder { get; }
        string CacheFolder { get; set; }
        string PackageFolder { get; set; }
        string SinglePresetsFolder { get; set; }
        bool IsGameRunning { get; }
        List<ResourcePackage> ResourcePackages { get; set; }
        Dictionary<string, ShaderPackage> Packages { get; set; }
        List<IRawFileProcess> RawFileProcessors { get; }
        List<IPresetProcess> PresetProcessors { get; }
    }

    public class AppEnvironmentService : IAppEnvironmentService
    {
        public string AppName { get; }
        public string AppVersion { get; }
        public string OSAppDataPath { get; }
        public string BundlingwayDataFolder { get; set; }
        public string ConfigFilePath { get; }
        public string TempFolder { get; }
        public string CacheFolder { get; set; }
        public string PackageFolder { get; set; }
        public string SinglePresetsFolder { get; set; }
        public bool IsGameRunning => ProcessHelper.IsGameRunning();
        public List<ResourcePackage> ResourcePackages { get; set; } = new();
        public Dictionary<string, ShaderPackage> Packages { get; set; } = new();
        public List<IRawFileProcess> RawFileProcessors { get; }
        public List<IPresetProcess> PresetProcessors { get; }

        public AppEnvironmentService()
        {
            AppName = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "Bundlingway";
            AppVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0";
            OSAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            BundlingwayDataFolder = Path.Combine(OSAppDataPath, AppName);
            ConfigFilePath = Path.Combine(BundlingwayDataFolder, Constants.Files.BundlingwayConfig);
            TempFolder = Path.Combine(BundlingwayDataFolder, Constants.Folders.Temp);
            CacheFolder = Path.Combine(BundlingwayDataFolder, Constants.Folders.Cache);
            PackageFolder = Path.Combine(BundlingwayDataFolder, Constants.Folders.Packages);
            SinglePresetsFolder = Path.Combine(PackageFolder, Constants.Folders.SinglePresets);
            RawFileProcessors = IoC.GetClassesByInterface<IRawFileProcess>().CreateInstances<IRawFileProcess>().OrderBy(i => i.Order).ToList();
            PresetProcessors = IoC.GetClassesByInterface<IPresetProcess>().CreateInstances<IPresetProcess>().OrderBy(i => i.Order).ToList();
        }
    }
}
