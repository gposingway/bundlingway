using Bundlingway.Model;

namespace Bundlingway.Utilities
{
    public class Bootstrap
    {
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
                MessageBox.Show($"Error in Initialize: {ex.Message}");
            }
        }

        public static async Task DetectSettings()
        {
            try
            {
                Instances.LocalConfigProvider.Load();

                if (Instances.LocalConfigProvider.Configuration.ResourcePackages == null) Instances.LocalConfigProvider.Configuration.ResourcePackages = new();

                await CheckGameClient();
                await CheckReShade();
                await CheckGPosingway();

                Instances.LocalConfigProvider.Save();
                Instances.MainDataSource.ResetBindings(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in DetectSettings: {ex.Message}");
            }
        }

        public static async Task CheckGameClient()
        {
            try
            {
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in CheckGameClient: {ex.Message}");
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
                MessageBox.Show($"Error in CheckGPosingway: {ex.Message}");
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
                MessageBox.Show($"Error in CheckReShade: {ex.Message}");
            }
        }
    }
}
