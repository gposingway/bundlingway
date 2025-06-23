﻿using Bundlingway.Core.Interfaces;
using Bundlingway.Core.Services;
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
        public static async Task DetectSettings(IAppEnvironmentService envService, IConfigurationService configService, BundlingwayService bundlingwayService, GPosingwayService gPosingwayService, ReShadeService reShadeService, IUserNotificationService notifications)
        {
            try
            {
                await notifications.AnnounceAsync(Constants.Bundlingway.GetMessage(Constants.MessageCategory.DetectingSettings));
                Log.Information("Bootstrap.DetectSettings: Starting settings detection.");
                await configService.LoadAsync();

                await CheckBundlingway(envService, bundlingwayService);
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
        }        private static async Task CheckBundlingway(IAppEnvironmentService envService, BundlingwayService bundlingwayService)
        {
            try
            {
                Log.Information("Bootstrap.CheckBundlingway: Checking Bundlingway.");
                await bundlingwayService.CheckStatusAsync();
                Log.Information("Bootstrap.CheckBundlingway: Bundlingway check completed.");
            }
            catch (Exception ex)
            {
                Log.Information($"Bootstrap.CheckBundlingway: Error in CheckBundlingway: {ex.Message}");
            }
        }

        public static async Task CheckGameClient(IAppEnvironmentService envService, IConfigurationService configService)
        {
            try
            {
                var c = configService.Configuration.Game;
                Log.Information("Bootstrap.CheckGameClient: Checking game client.");

                if (envService.IsGameRunning)
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
                    bool folderExists = Directory.Exists(c.InstallationFolder);
                    bool exeExists = File.Exists(c.ClientLocation);
                    if (!folderExists || !exeExists)
                    {
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
                    await gPosingwayService.CheckStatusAsync();
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
                await reShadeService.CheckStatusAsync();
                Log.Information("Bootstrap.CheckReShade: ReShade check completed.");
            }
            catch (Exception ex)
            {
                Log.Information($"Bootstrap.CheckReShade: Error in CheckReShade: {ex.Message}");
            }
        }
    }
}
