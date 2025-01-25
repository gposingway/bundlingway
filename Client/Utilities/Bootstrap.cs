using Bundlingway.Model;

namespace Bundlingway.Utilities
{
    public class Bootstrap
    {
        public static async Task Initialize()
        {
            try
            {
                Console.WriteLine("Bootstrap.Initialize: Starting initialization.");
                CustomProtocolHandler.RegisterCustomProtocol("gwpreset", "A collection of presets for GPosingway", true);
                Instances.LocalConfigProvider = new ConfigProvider<GPosingwayConfig>();
                Instances.VersionedAppDataFolder = Instances.LocalConfigProvider.versionedLocalAppDataPath;
                Instances.AppDataFolder = Instances.LocalConfigProvider.localAppDataPath;
                Instances.AppDataCacheFolder = Path.Combine(Instances.AppDataFolder, "cache");
                Instances.AppDataTempFolder = Path.Combine(Instances.AppDataFolder, "temp");
                await PackageManager.ScanPackages();

                await DetectSettings();

                Console.WriteLine("Bootstrap.Initialize: Initialization completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bootstrap.Initialize: Error in Initialize: {ex.Message}");
                MessageBox.Show($"Bootstrap.Initialize: Error in Initialize: {ex.Message}");
            }
        }

        public static async Task DetectSettings()
        {
            try
            {
                Console.WriteLine("Bootstrap.DetectSettings: Starting settings detection.");
                Instances.LocalConfigProvider.Load();

                if (Instances.ResourcePackages == null)
                    Instances.ResourcePackages = [];

                await CheckGameClient();
                await CheckReShade();
                await CheckGPosingway();
                await PackageManager.ScanPackages();

                Instances.LocalConfigProvider.Save();
                Instances.MainDataSource.ResetBindings(true);
                Console.WriteLine("Bootstrap.DetectSettings: Settings detection completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bootstrap.DetectSettings: Error in DetectSettings: {ex.Message}");
                MessageBox.Show($"Bootstrap.DetectSettings: Error in DetectSettings: {ex.Message}");
            }
        }

        private static bool ValidatePackageCatalog(ResourcePackage package)
        {
            Console.WriteLine($"Bootstrap.ValidatePackageCatalog: Validating package {package.Name}.");
            return true;
        }

        public static async Task CheckGameClient()
        {
            try
            {
                Console.WriteLine("Bootstrap.CheckGameClient: Checking game client.");
                if (ProcessHelper.IsProcessRunning("ffxiv_dx11"))
                {
                    var procPath = ProcessHelper.GetProcessPath("ffxiv_dx11");

                    if (procPath != null)
                    {
                        Instances.LocalConfigProvider.Configuration.XIVPath = ProcessHelper.GetProcessPath("ffxiv_dx11");
                    }
                }

                if (Instances.LocalConfigProvider.Configuration.XIVPath != null)
                    Instances.LocalConfigProvider.Configuration.GameFolder = Path.GetDirectoryName(Instances.LocalConfigProvider.Configuration.XIVPath);
                Console.WriteLine("Bootstrap.CheckGameClient: Game client check completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bootstrap.CheckGameClient: Error in CheckGameClient: {ex.Message}");
                MessageBox.Show($"Bootstrap.CheckGameClient: Error in CheckGameClient: {ex.Message}");
            }
        }

        public static async Task CheckGPosingway()
        {
            try
            {
                Console.WriteLine("Bootstrap.CheckGPosingway: Checking GPosingway.");
                GPosingwayParser.GetLocalInfo();
                await GPosingwayParser.GetRemoteInfo();

                Instances.LocalConfigProvider.Configuration.GPosingway.Status = $"Local: {Instances.LocalConfigProvider.Configuration.GPosingway.LocalVersion}, Remote: {Instances.LocalConfigProvider.Configuration.GPosingway.RemoteVersion}";
                Instances.LocalConfigProvider.Save();
                Console.WriteLine("Bootstrap.CheckGPosingway: GPosingway check completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bootstrap.CheckGPosingway: Error in CheckGPosingway: {ex.Message}");
                MessageBox.Show($"Bootstrap.CheckGPosingway: Error in CheckGPosingway: {ex.Message}");
            }
        }

        public static async Task CheckReShade()
        {
            try
            {
                Console.WriteLine("Bootstrap.CheckReShade: Checking ReShade.");
                ReShadeParser.GetLocalInfo();
                await ReShadeParser.GetRemoteInfo();

                Instances.LocalConfigProvider.Configuration.ReShade.Status = $"Local: {Instances.LocalConfigProvider.Configuration.ReShade.LocalVersion}, Remote: {Instances.LocalConfigProvider.Configuration.ReShade.RemoteVersion}";
                Instances.LocalConfigProvider.Save();
                Console.WriteLine("Bootstrap.CheckReShade: ReShade check completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bootstrap.CheckReShade: Error in CheckReShade: {ex.Message}");
                MessageBox.Show($"Bootstrap.CheckReShade: Error in CheckReShade: {ex.Message}");
            }
        }
    }
}
