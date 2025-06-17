﻿using Bundlingway.Core.Interfaces;
using Bundlingway.Core.Services;
using Bundlingway.Model;
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
                await ConfigService.LoadAsync();

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
                await ConfigService.SaveAsync();

                

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
                await BundlingwayService.Create().GetLocalInfoAsync();
                await BundlingwayService.Create().GetRemoteInfoAsync();
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
                var c = ConfigService.Configuration.Game;
                Log.Information("Bootstrap.CheckGameClient: Checking game client.");

                // If the game is running, get the process path
                if (Instances.IsGameRunning)
                {
                    var procPath = ProcessHelper.GetProcessPath(Constants.Files.GameProcess);
                    if (procPath != null)
                    {
                        c.ClientLocation = procPath;
                        c.InstallationFolder = Path.GetDirectoryName(procPath);
                    }
                }
                else if (!string.IsNullOrEmpty(c.InstallationFolder) && !string.IsNullOrEmpty(c.ClientLocation))
                {
                    // Game is not running, but a path is present
                    bool folderExists = Directory.Exists(c.InstallationFolder);
                    bool exeExists = File.Exists(c.ClientLocation);
                    if (!folderExists || !exeExists)
                    {
                        // If either is missing, clear both
                        c.ClientLocation = null;
                        c.InstallationFolder = null;
                    }
                    // else: both exist, proceed without overwriting (allow GPosingway/ReShade checks)
                }
                // else: no path, do nothing

                // Shader analysis (if valid)
                if (!string.IsNullOrEmpty(c.InstallationFolder))
                {
                    var shaderFolderPath = Path.Combine(c.InstallationFolder, Constants.Folders.GameShaders);
                    if (Directory.Exists(shaderFolderPath))
                    {
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
                var gPosingwayService = ServiceLocator.TryGetService<GPosingwayService>();
                if (gPosingwayService != null)
                {
                    await gPosingwayService.GetLocalInfoAsync();
                    await gPosingwayService.GetRemoteInfoAsync();
                }
                await ConfigService.SaveAsync();
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
                var reShadeService = ServiceLocator.GetService<ReShadeService>();
                await reShadeService.GetLocalInfoAsync();
                await reShadeService.GetRemoteInfoAsync();
                Log.Information("Bootstrap.CheckReShade: ReShade check completed.");
            }
            catch (Exception ex)
            {
                Log.Information($"Bootstrap.CheckReShade: Error in CheckReShade: {ex.Message}");
            }
        }

        private static IConfigurationService ConfigService => Core.Services.ServiceLocator.GetService<IConfigurationService>();
    }
}
