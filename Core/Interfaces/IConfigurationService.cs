using Bundlingway.Model;
using System.Threading.Tasks;

namespace Bundlingway.Core.Interfaces
{
    /// <summary>
    /// Service for managing application configuration.
    /// Abstracts configuration access to enable testing and different storage mechanisms.
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        /// Gets the current application configuration.
        /// </summary>
        BundlingwayConfig Configuration { get; }

        /// <summary>
        /// Loads the configuration from storage.
        /// </summary>
        Task LoadAsync();

        /// <summary>
        /// Saves the current configuration to storage.
        /// </summary>
        Task SaveAsync();

        /// <summary>
        /// Resets the configuration to default values.
        /// </summary>
        Task ResetToDefaultsAsync();

        /// <summary>
        /// Validates the current configuration.
        /// </summary>
        /// <returns>True if configuration is valid, false otherwise</returns>
        Task<bool> ValidateAsync();

        void Save();

        /// <summary>
        /// Gets the configuration file path.
        /// </summary>
        string ConfigurationFilePath { get; }

        /// <summary>
        /// Event fired when configuration is changed.
        /// </summary>
        event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;
    }

    /// <summary>
    /// Event arguments for configuration changes.
    /// </summary>
    public class ConfigurationChangedEventArgs : EventArgs
    {
        public string? PropertyName { get; set; }
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
    }
}
