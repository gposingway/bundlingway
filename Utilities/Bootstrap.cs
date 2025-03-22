﻿using Bundlingway.Model;
using Bundlingway.Utilities.Handler;
using Serilog;

namespace Bundlingway.Utilities
{
    public class Bootstrap
    {
        /// <summary>
        /// Initializes the application.
        /// </summary>
        public static async Task Initialize()
        {
            try
            {
                // Initialize the local configuration provider
                Instances.LocalConfigProvider = new ConfigProvider<BundlingwayConfig>();

                Log.Information("Bootstrap.Initialize: Initialization completed.");
            }
            catch (Exception ex)
            {
                Log.Information($"Bootstrap.Initialize: Error in Initialize: {ex.Message}");
            }
        }

        /// <summary>
        /// Detects settings for the application and related components.
        /// </summary>
        public static async Task DetectSettings()
        {
            try
            {
                _ = UI.Announce(Constants.Bundlingway.GetMessage(Constants.MessageCategory.DetectingSettings));
                Log.Information("Bootstrap.DetectSettings: Starting settings detection.");
                // Load the local configuration
                Instances.LocalConfigProvider.Load();

                // Initialize the resource packages list if it's null
                if (Instances.ResourcePackages == null)
                    Instances.ResourcePackages = [];

                // Check for Bundlingway updates
                await CheckBundlingway();

                // Check for the game client and related components
                await CheckGameClient().ContinueWith(async a =>
                {
                    await Task.WhenAll(
                        CheckReShade(),
                        CheckGPosingway()
                        );
                });

                // Save the local configuration
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

        /// <summary>
        /// Checks for Bundlingway updates.
        /// </summary>
        private static async Task CheckBundlingway()
        {
            try
            {
                Log.Information("Bootstrap.CheckBundlingway: Checking Bundlingway.");
                // Get local and remote Bundlingway information
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

        /// <summary>
        /// Checks for the game client and related settings.
        /// </summary>
        public static async Task CheckGameClient()
        {
            try
            {
                var c = Instances.LocalConfigProvider.Configuration.Game;

                Log.Information("Bootstrap.CheckGameClient: Checking game client.");

                // If the game is running, get the process path
                if (Instances.IsGameRunning)
                {
                    var procPath = ProcessHelper.GetProcessPath(Constants.Files.GameProcess);

                    if (procPath != null)
                    {
                        c.ClientLocation = procPath;
                    }
                }

                // If the client location is not valid, reset it
                if (c.ClientLocation != null)
                    if (!File.Exists(c.ClientLocation))
                    {
                        c.ClientLocation = null;
                        c.InstallationFolder = null;
                    }
                    else
                    {
                        // Get the installation folder and shader folder path
                        c.InstallationFolder = Path.GetDirectoryName(c.ClientLocation);

                        var shaderFolderPath = Path.Combine(c.InstallationFolder, Constants.Folders.GameShaders);

                        // If the shader folder exists, analyze the shaders
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

        /// <summary>
        /// Checks for GPosingway updates.
        /// </summary>
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

        /// <summary>
        /// Checks for ReShade updates.
        /// </summary>
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
