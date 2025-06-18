﻿using Bundlingway.Core.Interfaces;
using Bundlingway.Core.Services;
using Bundlingway.Model;
using Bundlingway.Core.Utilities;
using static Bundlingway.Core.Constants;
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
        public static async Task DetectSettings(IAppEnvironmentService envService, IConfigurationService configService, BundlingwayService bundlingwayService, GPosingwayService gPosingwayService, ReShadeService reShadeService, IUserNotificationService notificationService)
        {
            try
            {
                await notificationService.AnnounceAsync(Bundlingway.Core.Constants.Bundlingway.GetMessage(Bundlingway.Core.Constants.MessageCategory.DetectingSettings));
                Log.Information("Bootstrap.DetectSettings: Starting settings detection.");
                // Load the local configuration
                await configService.LoadAsync();

                // Check for Bundlingway updates
                await CheckBundlingway(envService, bundlingwayService);

                // Check for the game client and related components
                await CheckGameClient(envService, configService);
                await Task.WhenAll(
                    CheckReShade(envService, reShadeService),
                    CheckGPosingway(envService, gPosingwayService, configService)
                );
            }
            catch (Exception ex)
            {
                Log.Information($"Bootstrap.DetectSettings: Error in DetectSettings: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks for Bundlingway updates.
        /// </summary>
        private static async Task CheckBundlingway(IAppEnvironmentService envService, BundlingwayService bundlingwayService)
        {
            try
            {
                Log.Information("Bootstrap.CheckBundlingway: Checking Bundlingway.");
                await bundlingwayService.GetLocalInfoAsync();
                await bundlingwayService.GetRemoteInfoAsync();
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
        public static async Task CheckGameClient(IAppEnvironmentService envService, IConfigurationService configService)
        {
            try
            {
                var c = configService.Configuration.Game;
                Log.Information("Bootstrap.CheckGameClient: Checking game client.");

                // If the game is running, get the process path
                if (envService.IsGameRunning)
                {
                    var procPath = Bundlingway.Core.Utilities.ProcessHelper.GetProcessPath(Bundlingway.Core.Constants.Files.GameProcess);
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
                    var shaderFolderPath = Path.Combine(c.InstallationFolder, Bundlingway.Core.Constants.Folders.GameShaders);
                    if (Directory.Exists(shaderFolderPath))
                    {
                        // _ = ManagedResources.Shader.SaveShaderAnalysisToPath(shaderFolderPath, Path.Combine(envService.BundlingwayDataFolder, Constants.Files.ShaderAnalysis)).ContinueWith(i=> UI.Announce("Installed shaders analysis finished!"));
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
        public static async Task CheckGPosingway(IAppEnvironmentService envService, GPosingwayService gPosingwayService, IConfigurationService configService)
        {
            try
            {
                Log.Information("Bootstrap.CheckGPosingway: Checking GPosingway.");
                if (gPosingwayService != null)
                {
                    await gPosingwayService.GetLocalInfoAsync();
                    await gPosingwayService.GetRemoteInfoAsync();
                }
                await configService.SaveAsync();
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
        public static async Task CheckReShade(IAppEnvironmentService envService, ReShadeService reShadeService)
        {
            try
            {
                Log.Information("Bootstrap.CheckReShade: Checking ReShade.");
                await reShadeService.GetLocalInfoAsync();
                await reShadeService.GetRemoteInfoAsync();
                Log.Information("Bootstrap.CheckReShade: ReShade check completed.");
            }
            catch (Exception ex)
            {
                Log.Information($"Bootstrap.CheckReShade: Error in CheckReShade: {ex.Message}");
            }
        }
    }
}
