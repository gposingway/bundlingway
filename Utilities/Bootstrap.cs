using Bundlingway.Model;
using Bundlingway.Utilities.Handler;
using Serilog;

namespace Bundlingway.Utilities
{
    public class Bootstrap
    {
        public static async Task Initialize()
        {
            try
            {
                Log.Information("Bootstrap.Initialize: Starting initialization.");
                await ProcessHelper.PinToStartScreenAsync();
                await CustomProtocolHandler.RegisterCustomProtocolAsync("gwpreset", "A collection of presets for GPosingway", true);

                Instances.LocalConfigProvider = new ConfigProvider<GPosingwayConfig>();

                Log.Information("Bootstrap.Initialize: Initialization completed.");
            }
            catch (Exception ex)
            {
                Log.Information($"Bootstrap.Initialize: Error in Initialize: {ex.Message}");
            }
        }

        public static async Task DetectSettings()
        {
            try
            {
                UI.Announce(Constants.Bundlingway.GetMessage(Constants.MessageCategory.DetectingSettings));

                Log.Information("Bootstrap.DetectSettings: Starting settings detection.");
                Instances.LocalConfigProvider.Load();

                if (Instances.ResourcePackages == null)
                    Instances.ResourcePackages = [];

                await CheckGameClient();
                await CheckReShade();
                await CheckGPosingway();

                Instances.LocalConfigProvider.Save();
                Instances.MainDataSource.ResetBindings(true);

                Log.Information("Bootstrap.DetectSettings: Settings detection completed.");
                UI.Announce(Constants.Bundlingway.GetMessage(Constants.MessageCategory.Ready));

            }
            catch (Exception ex)
            {
                Log.Information($"Bootstrap.DetectSettings: Error in DetectSettings: {ex.Message}");
            }
        }

        private static bool ValidatePackageCatalog(ResourcePackage package)
        {
            Log.Information($"Bootstrap.ValidatePackageCatalog: Validating package {package.Name}.");
            return true;
        }

        public static async Task CheckGameClient()
        {
            try
            {
                Log.Information("Bootstrap.CheckGameClient: Checking game client.");
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
                Log.Information("Bootstrap.CheckGameClient: Game client check completed.");
            }
            catch (Exception ex)
            {
                Log.Information($"Bootstrap.CheckGameClient: Error in CheckGameClient: {ex.Message}");
            }
        }

        public static async Task CheckGPosingway()
        {
            try
            {
                Log.Information("Bootstrap.CheckGPosingway: Checking GPosingway.");
                GPosingway.GetLocalInfo();
                await GPosingway.GetRemoteInfo();

                Instances.LocalConfigProvider.Configuration.GPosingway.Status = $"Local: {Instances.LocalConfigProvider.Configuration.GPosingway.LocalVersion}, Remote: {Instances.LocalConfigProvider.Configuration.GPosingway.RemoteVersion}";
                Instances.LocalConfigProvider.Save();
                Log.Information("Bootstrap.CheckGPosingway: GPosingway check completed.");
            }
            catch (Exception ex)
            {
                Log.Information($"Bootstrap.CheckGPosingway: Error in CheckGPosingway: {ex.Message}");
            }
        }

        public static async Task CheckReShade()
        {
            try
            {
                Log.Information("Bootstrap.CheckReShade: Checking ReShade.");
                ReShade.GetLocalInfo();
                await ReShade.GetRemoteInfo();

                Instances.LocalConfigProvider.Configuration.ReShade.Status = $"Local: {Instances.LocalConfigProvider.Configuration.ReShade.LocalVersion}, Remote: {Instances.LocalConfigProvider.Configuration.ReShade.RemoteVersion}";
                Instances.LocalConfigProvider.Save();
                Log.Information("Bootstrap.CheckReShade: ReShade check completed.");
            }
            catch (Exception ex)
            {
                Log.Information($"Bootstrap.CheckReShade: Error in CheckReShade: {ex.Message}");
            }
        }
    }
}
