using Bundlingway.Core.Interfaces;
using Bundlingway.Utilities;
using Serilog;

namespace Bundlingway.Core.Services
{
    /// <summary>
    /// Handles execution of elevated operations when the application is restarted with admin privileges.
    /// Separates elevated operation logic from the main application flow.
    /// </summary>
    public class ElevatedOperationHandler
    {
        private readonly IConfigurationService _configService;
        private readonly IAppEnvironmentService _envService;

        public ElevatedOperationHandler(IConfigurationService configService, IAppEnvironmentService envService)
        {
            _configService = configService;
            _envService = envService;
        }

        /// <summary>
        /// Executes an elevated operation based on the operation ID.
        /// </summary>
        /// <param name="operationId">The ID of the operation to execute</param>
        /// <param name="arguments">Additional arguments for the operation</param>
        /// <returns>Exit code (0 for success, non-zero for failure)</returns>
        public async Task<int> ExecuteOperationAsync(string operationId, string[] arguments)
        {
            Log.Information("Executing elevated operation: {OperationId}", operationId);

            try
            {
                int result;
                switch (operationId.ToLowerInvariant())
                {
                    case "register-protocol":
                        result = await RegisterProtocolAsync(arguments);
                        break;

                    case "create-desktop-shortcut":
                        result = await CreateDesktopShortcutAsync(arguments);
                        break;

                    case "register-browser-integration":
                        result = await RegisterBrowserIntegrationAsync(arguments);
                        break;

                    case "pin-to-start":
                        result = await PinToStartAsync(arguments);
                        break;

                    default:
                        Log.Warning("Unknown elevated operation: {OperationId}", operationId);
                        return 1;
                }                // After successful completion, restart the application normally
                if (result == 0)
                {
                    // Operation completed successfully
                    Log.Information("Elevated operation completed successfully: {OperationId}", operationId);
                }

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to execute elevated operation: {OperationId}", operationId);
                return 1;
            }
        }

        /// <summary>
        /// Registers the custom protocol handler for browser integration.
        /// </summary>
        private async Task<int> RegisterProtocolAsync(string[] arguments)
        {
            try
            {
                var protocolName = Constants.GPosingwayProtocolHandler;
                var description = "A collection of GPosingway-compatible ReShade resources";

                await CustomProtocolHandler.RegisterCustomProtocolAsync(protocolName, description, true);

                Log.Information("Successfully registered protocol: {Protocol}", protocolName);
                return 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to register custom protocol");
                return 1;
            }
        }

        /// <summary>
        /// Creates a desktop shortcut for the application.
        /// </summary>
        private async Task<int> CreateDesktopShortcutAsync(string[] arguments)
        {
            try
            {
                ProcessHelper.EnsureDesktopShortcut();
                Log.Information("Successfully created desktop shortcut");
                return 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create desktop shortcut");
                return 1;
            }
        }

        /// <summary>
        /// Registers browser integration by setting up the custom protocol handler.
        /// </summary>
        private async Task<int> RegisterBrowserIntegrationAsync(string[] arguments)
        {
            try
            {
                var protocolName = Constants.GPosingwayProtocolHandler;
                var description = "A collection of GPosingway-compatible ReShade resources";

                await CustomProtocolHandler.RegisterCustomProtocolAsync(protocolName, description, true);

                Log.Information("Successfully registered browser integration");
                return 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to register browser integration");
                return 1;
            }
        }

        /// <summary>
        /// Pins the application to the Start menu.
        /// </summary>
        private async Task<int> PinToStartAsync(string[] arguments)
        {
            try
            {
                await ProcessHelper.PinToStartScreenAsync();
                Log.Information("Successfully pinned to Start menu");
                return 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to pin to Start menu");
                return 1;
            }
        }

        /// <summary>
        /// Restarts the application normally (without elevation) after completing elevated operations.
        /// </summary>
        private async Task RestartNormalApplicationAsync()
        {
            try
            {
                var currentExecutable = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName
                    ?? System.IO.Path.Combine(System.AppContext.BaseDirectory, "Bundlingway.exe");

                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = currentExecutable,
                    Arguments = "--elevated-operation-completed",
                    UseShellExecute = true,
                    WorkingDirectory = System.AppContext.BaseDirectory
                };

                Log.Information("Restarting application normally after elevated operation completion");
                System.Diagnostics.Process.Start(startInfo);

                // Small delay to ensure the new process starts
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to restart normal application after elevated operation");
            }
        }
    }
}
