using Bundlingway.Core.Interfaces;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;
using System.ComponentModel;
using Serilog;

namespace Bundlingway.Core.Services
{
    /// <summary>
    /// Service for handling UAC elevation requests on-demand.
    /// Implements the principle of least privilege by only requesting elevation when necessary.
    /// </summary>
    public class ElevationService : IElevationService
    {
        private readonly IUserNotificationService _notificationService;

        public ElevationService(IUserNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Checks if the current process is running with elevated privileges.
        /// </summary>
        public bool IsElevated
        {
            get
            {
                try
                {
                    using var identity = WindowsIdentity.GetCurrent();
                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to check elevation status");
                    return false;
                }
            }
        }

        /// <summary>
        /// Requests elevation for a specific operation.
        /// </summary>
        public async Task<bool> RequestElevationAsync(string operationName, string operationId, string arguments = "")
        {
            if (IsElevated)
            {
                Log.Information("Process already elevated for operation: {OperationName}", operationName);
                return true;
            }
            var result = MessageBox.Show(
                $"The following operation requires administrator privileges:\n\n{operationName}\n\nWould you like to restart Bundlingway as administrator to perform this operation?",
                "Administrator Privileges Required",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                Log.Information("User declined elevation request for operation: {OperationName}", operationName);
                await _notificationService.AnnounceAsync($"Administrator privileges required for: {operationName}");
                return false;
            }

            return await RestartAsAdminAsync(operationId, arguments);
        }

        /// <summary>
        /// Executes an operation that requires elevation.
        /// </summary>
        public async Task<bool> ExecuteElevatedAsync(string operationName, string operationId, Func<Task> operation, Func<Task>? fallback = null)
        {
            if (IsElevated)
            {
                try
                {
                    Log.Information("Executing elevated operation: {OperationName}", operationName);
                    await operation();
                    await _notificationService.AnnounceAsync($"Successfully completed: {operationName}");
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to execute elevated operation: {OperationName}", operationName);
                    await _notificationService.AnnounceAsync($"Failed to complete: {operationName}. Error: {ex.Message}");
                    return false;
                }
            }

            // Not elevated - request elevation
            var elevationGranted = await RequestElevationAsync(operationName, operationId);

            if (!elevationGranted && fallback != null)
            {
                Log.Information("Executing fallback for operation: {OperationName}", operationName);
                try
                {
                    await fallback();
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Fallback operation failed: {OperationName}", operationName);
                    await _notificationService.AnnounceAsync($"Operation failed: {operationName}");
                    return false;
                }
            }

            return elevationGranted;
        }

        /// <summary>
        /// Restarts the application with elevated privileges to perform a specific operation.
        /// </summary>
        private async Task<bool> RestartAsAdminAsync(string operationId, string arguments)
        {
            try
            {
                var currentExecutable = Application.ExecutablePath;
                var elevatedArgs = $"--elevated-operation={operationId}";

                if (!string.IsNullOrWhiteSpace(arguments))
                {
                    elevatedArgs += $" {arguments}";
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = currentExecutable,
                    Arguments = elevatedArgs,
                    Verb = "runas", // This triggers UAC prompt
                    UseShellExecute = true,
                    WorkingDirectory = Application.StartupPath
                };

                Log.Information("Restarting application with elevation. Arguments: {Arguments}", elevatedArgs);

                var process = Process.Start(startInfo); if (process != null)
                {
                    await _notificationService.AnnounceAsync("Restarting with administrator privileges...");

                    // Give the new process time to start
                    await Task.Delay(1000);

                    // Exit current non-elevated instance
                    Application.Exit();
                    return true;
                }
                else
                {
                    Log.Error("Failed to start elevated process");
                    await _notificationService.AnnounceAsync("Failed to restart with elevated privileges");
                    return false;
                }
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1223) // ERROR_CANCELLED
            {
                Log.Information("User cancelled UAC elevation prompt");
                await _notificationService.AnnounceAsync("Administrator privileges required but not granted");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to restart application with elevation");
                await _notificationService.AnnounceAsync($"Failed to request administrator privileges: {ex.Message}");
                return false;
            }
        }
    }
}
