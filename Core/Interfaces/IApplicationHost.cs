using System.Threading.Tasks;

namespace Bundlingway.Core.Interfaces
{
    /// <summary>
    /// Main application host interface.
    /// Coordinates all core services and manages application lifecycle.
    /// </summary>
    public interface IApplicationHost
    {
        /// <summary>
        /// Initializes the application host and all services.
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Starts the application in UI mode.
        /// </summary>
        Task StartUIAsync();

        /// <summary>
        /// Starts the application in headless mode.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        Task StartHeadlessAsync(string[] args);

        /// <summary>
        /// Shuts down the application gracefully.
        /// </summary>
        Task ShutdownAsync();

        /// <summary>
        /// Gets a service instance by type.
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        T GetService<T>() where T : class;

        /// <summary>
        /// Indicates whether the application is running in headless mode.
        /// </summary>
        bool IsHeadless { get; }

        /// <summary>
        /// Indicates whether the application is initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Event fired when the application is shutting down.
        /// </summary>
        event EventHandler? ApplicationShutdown;
    }
}
