using GPosingway.Client.Model;
using Microsoft.Extensions.Logging;

namespace GPosingway.Client.Utilities
{
    public class Bootstrap
    {
        private static readonly ILogger<Bootstrap> logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<Bootstrap>();

        public static async Task Initialize()
        {
            try
            {
                CustomProtocolHandler.RegisterCustomProtocol("gwpreset", "A collection of presets for GPosingway", true);
                Instances.LocalConfigProvider = new ConfigProvider<GPosingwayConfig>();
                Instances.VersionedAppDataFolder = Instances.LocalConfigProvider.versionedLocalAppDataPath;
                Instances.AppDataFolder = Instances.LocalConfigProvider.localAppDataPath;

                await DetectSettings();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Initialize");
            }
        }

        public static async Task DetectSettings()
        {
            try
            {
                Instances.LocalConfigProvider.Load();
                Instances.LocalConfigProvider.Configuration.ResourcePackages ?= new();

                await CheckGameClient();
                await CheckReShade();
                await CheckGPosingway();

                Instances.LocalConfigProvider.Save();
                Instances.MainDataSource.ResetBindings(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in DetectSettings");
            }
        }

        public static async Task CheckGameClient()
        {
            try
            {
                if (ProcessHelper.IsProcessRunning("ffxiv_dx11"))
                {
                    var gameClientPath = ProcessHelper.GetProcessPath("ffxiv_dx11");

                    if (gameClientPath != null)
                    {
                        Instances.LocalConfigProvider.Configuration.XIVPath = gameClientPath;
                    }
                }

                if (Instances.LocalConfigProvider.Configuration.XIVPath != null)
                {
                    Instances.LocalConfigProvider.Configuration.GameFolder = Path.GetDirectoryName(Instances.LocalConfigProvider.Configuration.XIVPath);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in CheckGameClient");
            }
        }

        public static async Task CheckGPosingway()
        {
            try
            {
                GPosingwayParser.GetLocalInfo();
                await GPosingwayParser.GetRemoteInfo();

                Instances.LocalConfigProvider.Configuration.GPosingway.Status = $"Local: {Instances.LocalConfigProvider.Configuration.GPosingway.LocalVersion}, Remote: {Instances.LocalConfigProvider.Configuration.GPosingway.RemoteVersion}";
                Instances.LocalConfigProvider.Save();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in CheckGPosingway");
            }
        }

        public static async Task CheckReShade()
        {
            try
            {
                ReShadeParser.GetLocalInfo();
                await ReShadeParser.GetRemoteInfo();

                Instances.LocalConfigProvider.Configuration.ReShade.Status = $"Local: {Instances.LocalConfigProvider.Configuration.ReShade.LocalVersion}, Remote: {Instances.LocalConfigProvider.Configuration.ReShade.RemoteVersion}";
                Instances.LocalConfigProvider.Save();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in CheckReShade");
            }
        }
    }
}
