using System;
using System.Threading.Tasks;

namespace Bundlingway.Core.Interfaces
{
    /// <summary>
    /// Service for user notifications and messages.
    /// Implementations will handle UI-specific notification display.
    /// </summary>
    public interface IUserNotificationService
    {
        /// <summary>
        /// Displays a general announcement to the user.
        /// </summary>
        /// <param name="message">The message to announce</param>
        Task AnnounceAsync(string message);
        /// <summary>
        /// Displays an error message to the user.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="exception">Optional exception details</param>
        Task ShowErrorAsync(string message, Exception? exception = null);

        /// <summary>
        /// Displays an informational message to the user.
        /// </summary>
        /// <param name="message">The information message</param>
        Task ShowInfoAsync(string message);

        /// <summary>
        /// Displays a warning message to the user.
        /// </summary>
        /// <param name="message">The warning message</param>
        Task ShowWarningAsync(string message);

        /// <summary>
        /// Displays a success message to the user.
        /// </summary>
        /// <param name="message">The success message</param>
        Task ShowSuccessAsync(string message);

        /// <summary>
        /// Asks the user for confirmation on an action.
        /// </summary>
        /// <param name="message">The confirmation message</param>
        /// <param name="title">Optional title for the confirmation dialog</param>
        /// <returns>True if user confirms, false otherwise</returns>
        Task<bool> ConfirmAsync(string message, string title = "Confirm");

        /// <summary>
        /// Brings the main application window to front.
        /// </summary>
        Task BringToFrontAsync();
    }
}
