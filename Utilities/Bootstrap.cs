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
                Instances.LocalConfigProvider = new ConfigProvider<BundlingwayConfig>();

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
                _ = UI.Announce(Constants.Bundlingway.GetMessage(Constants.MessageCategory.DetectingSettings));
                Log.Information("Bootstrap.DetectSettings: Starting settings detection.");
                Instances.LocalConfigProvider.Load();

                if (Instances.ResourcePackages == null)
                    Instances.ResourcePackages = [];

                await CheckBundlingway();

                await CheckGameClient().ContinueWith(async a =>
                {
                    await Task.WhenAll(
                        CheckReShade(),
                        CheckGPosingway()
                        );
                });

                Instances.LocalConfigProvider.Save();

                

                Log.Information("Bootstrap.DetectSettings: Settings detection completed.");
                _ = UI.Announce(Constants.Bundlingway.GetMessage(Constants.MessageCategory.Ready));

                _ = UI.UpdateElements();
            }
            catch (Exception ex)
            {
                Log.Information($"Bootstrap.DetectSettings: Error in DetectSettings: {ex.Message}");
            }
        }

        private static async Task CheckBundlingway()
        {
            try
            {
                Log.Information("Bootstrap.CheckBundlingway: Checking Bundlingway.");
                await Handler.Bundlingway.GetLocalInfo();
                await Handler.Bundlingway.GetRemoteInfo();
                Instances.LocalConfigProvider.Save();
                Log.Information("Bootstrap.CheckBundlingway: Bundlingway check completed.");
            }
            catch (Exception ex)
            {
                Log.Information($"Bootstrap.CheckBundlingway: Error in CheckBundlingway: {ex.Message}");
            }
        }

        public static async Task CheckGameClient()
        {
            try
            {
                var c = Instances.LocalConfigProvider.Configuration.Game;

                Log.Information("Bootstrap.CheckGameClient: Checking game client.");

                if (Instances.IsGameRunning)
                {
                    var procPath = ProcessHelper.GetProcessPath(Constants.Files.GameProcess);

                    if (procPath != null)
                    {
                        c.ClientLocation = procPath;
                    }
                }

                if (c.ClientLocation != null)
                    if (!File.Exists(c.ClientLocation))
                    {
                        c.ClientLocation = null;
                        c.InstallationFolder = null;
                    }
                    else
                    {
                        c.InstallationFolder = Path.GetDirectoryName(c.ClientLocation);

                        var shaderFolderPath = Path.Combine(c.InstallationFolder, Constants.Folders.GameShaders);

                        if (Directory.Exists(shaderFolderPath))
                        {
                            // In the background we will save the shader analysis to the data folder.
                            // _ = ManagedResources.Shader.SaveShaderAnalysisToPath(shaderFolderPath, Path.Combine(Instances.BundlingwayDataFolder, Constants.Files.ShaderAnalysis)).ContinueWith(i=> UI.Announce("Installed shaders analysis finished!"));
                        }
                    }

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
