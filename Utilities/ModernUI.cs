using Bundlingway.Core.Interfaces;
using Bundlingway.Core.Services;
using System.Threading.Tasks;
using static Bundlingway.Constants;

namespace Bundlingway.Utilities
{
    /// <summary>
    /// Modernized UI class that acts as a bridge between the old static approach
    /// and the new service-based approach. This enables gradual migration.
    /// </summary>
    public static class ModernUI
    {
        private static IUserNotificationService? _notificationService;
        private static IProgressReporter? _progressReporter;

        /// <summary>
        /// Initializes the modern UI with services.
        /// </summary>
        public static void Initialize(IUserNotificationService notificationService, IProgressReporter progressReporter)
        {
            _notificationService = notificationService;
            _progressReporter = progressReporter;
        }

        /// <summary>
        /// Announces a message to the user.
        /// Falls back to legacy UI if services are not available.
        /// </summary>
        public static async Task Announce(string message)
        {
            if (_notificationService != null)
            {
                await _notificationService.AnnounceAsync(message);
            }
            else
            {
                // Fallback to legacy UI
                await UI.Announce(message);
            }
        }

        /// <summary>
        /// Announces a message with category support for backward compatibility.
        /// </summary>
        public static async Task Announce(string category, params string[] parameters)
        {
            // For now, just pass the category as the message
            // This can be enhanced to handle message templates
            var message = parameters.Length > 0 ? $"{category}: {string.Join(", ", parameters)}" : category;
            await Announce(message);
        }

        /// <summary>
        /// Shows an error message to the user.
        /// </summary>
        public static async Task ShowError(string message, Exception? exception = null)
        {
            if (_notificationService != null)
            {
                await _notificationService.ShowErrorAsync(message, exception);
            }
            else
            {
                // Fallback to legacy approach
                await UI.Announce($"Error: {message}");
            }
        }

        /// <summary>
        /// Starts a progress operation.
        /// </summary>
        public static async Task StartProgress(long total, string? description = null)
        {
            if (_progressReporter != null)
            {
                await _progressReporter.StartProgressAsync(total, description);
            }
            else
            {
                // Fallback to legacy UI
                await UI.StartProgress(total);
            }
        }

        /// <summary>
        /// Updates progress.
        /// </summary>
        public static async Task SetProgress(long current)
        {
            if (_progressReporter != null)
            {
                await _progressReporter.UpdateProgressAsync(current);
            }
            else
            {
                // Fallback to legacy UI
                await UI.SetProgress(current);
            }
        }

        /// <summary>
        /// Stops progress operation.
        /// </summary>
        public static async Task StopProgress()
        {
            if (_progressReporter != null)
            {
                await _progressReporter.StopProgressAsync();
            }
            else
            {
                // Fallback to legacy UI
                await UI.StopProgress();
            }
        }

        /// <summary>
        /// Enables all UI elements.
        /// </summary>
        public static void EnableEverything()
        {
            // For now, delegate to legacy UI
            UI.EnableEverything();
        }

        /// <summary>
        /// Disables all UI elements.
        /// </summary>
        public static void DisableEverything()
        {
            // For now, delegate to legacy UI
            UI.DisableEverything();
        }

        /// <summary>
        /// Brings the main window to front.
        /// </summary>
        public static async Task BringToFront()
        {
            // For now, delegate to legacy UI
            await UI.BringToFront();
        }

        /// <summary>
        /// Updates UI elements.
        /// </summary>
        public static async Task UpdateElements()
        {
            // For now, delegate to legacy UI
            await UI.UpdateElements();
        }
    }
}
