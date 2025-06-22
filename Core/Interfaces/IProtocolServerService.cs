namespace Bundlingway.Core.Interfaces
{
    public interface IProtocolServerService
    {
        /// <summary>
        /// Starts the named pipe server to listen for protocol requests from other instances.
        /// </summary>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a message to an existing instance via named pipe.
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <returns>True if message was sent successfully, false otherwise</returns>
        Task<bool> SendMessageToExistingInstanceAsync(string message);

        /// <summary>
        /// Sends a notification message to an existing UI instance.
        /// </summary>
        /// <param name="notificationText">The notification text to display</param>
        /// <returns>True if notification was sent successfully, false otherwise</returns>
        Task<bool> NotifyExistingInstanceAsync(string notificationText);

        /// <summary>
        /// Requests that an existing instance brings itself to the front.
        /// </summary>
        /// <returns>True if request was sent successfully, false otherwise</returns>
        Task<bool> BringExistingInstanceToFrontAsync();
    }
}
