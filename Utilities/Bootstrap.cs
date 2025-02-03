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

                await Task.WhenAll(
                    CheckGameClient(), 
                    CheckReShade(), 
                    CheckGPosingway()
                    );

                Instances.LocalConfigProvider.Save();

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
                if (Instances.IsGameRunning)
                {
                    var procPath = ProcessHelper.GetProcessPath("ffxiv_dx11");

                    if (procPath != null)
                    {
                        Instances.LocalConfigProvider.Configuration.XIVPath = procPath;
                    }
                }

                if (Instances.LocalConfigProvider.Configuration.XIVPath != null)
                    if (!File.Exists(Instances.LocalConfigProvider.Configuration.XIVPath))
                    {
                        Instances.LocalConfigProvider.Configuration.XIVPath = null;
                        Instances.LocalConfigProvider.Configuration.GameFolder = null;
                    }
                    else
                    {
                        Instances.LocalConfigProvider.Configuration.GameFolder = Path.GetDirectoryName(Instances.LocalConfigProvider.Configuration.XIVPath);
                    }

                
                _= UI.UpdateElements();

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
                await GPosingway.GetLocalInfo();
                await GPosingway.GetRemoteInfo();
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
                await ReShade.GetLocalInfo();
                await ReShade.GetRemoteInfo();
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
