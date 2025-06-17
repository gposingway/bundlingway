using Bundlingway.Core.Interfaces;
using Bundlingway.Model;
using Bundlingway.Utilities.Extensions;
using Serilog;

namespace Bundlingway.Core.Services
{
    /// <summary>
    /// Configuration service implementation that manages BundlingwayConfig.
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private BundlingwayConfig _configuration;
        private readonly string _configurationFilePath;

        public ConfigurationService(string configurationFilePath)
        {
            _configurationFilePath = configurationFilePath;
            _configuration = new BundlingwayConfig();
        }

        public BundlingwayConfig Configuration => _configuration;

        public string ConfigurationFilePath => _configurationFilePath;

        public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;

        public async Task LoadAsync()
        {
            try
            {
                if (File.Exists(_configurationFilePath))
                {
                    var loadedConfig = _configurationFilePath.FromJsonFile<BundlingwayConfig>();
                    if (loadedConfig != null)
                    {
                        _configuration = loadedConfig;
                        Log.Information("Configuration loaded successfully from {ConfigPath}", _configurationFilePath);
                    }
                    else
                    {
                        Log.Warning("Failed to deserialize configuration file, using defaults");
                        _configuration = new BundlingwayConfig();
                    }
                }
                else
                {
                    Log.Information("Configuration file not found, using defaults");
                    _configuration = new BundlingwayConfig();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading configuration from {ConfigPath}", _configurationFilePath);
                _configuration = new BundlingwayConfig();
            }

            await Task.CompletedTask;
        }

        public async Task SaveAsync()
        {
            try
            {
                // Ensure directory exists
                var directory = Path.GetDirectoryName(_configurationFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                _configuration.ToJsonFile(_configurationFilePath);
                Log.Information("Configuration saved successfully to {ConfigPath}", _configurationFilePath);

                // Fire configuration changed event
                ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs
                {
                    PropertyName = "Configuration",
                    NewValue = _configuration
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving configuration to {ConfigPath}", _configurationFilePath);
                throw;
            }

            await Task.CompletedTask;
        }

        public async Task ResetToDefaultsAsync()
        {
            var oldConfig = _configuration;
            _configuration = new BundlingwayConfig();
            
            ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs
            {
                PropertyName = "Configuration",
                OldValue = oldConfig,
                NewValue = _configuration
            });

            await SaveAsync();
        }

        public async Task<bool> ValidateAsync()
        {
            // Basic validation - can be extended
            if (_configuration == null)
                return false;

            // Validate game installation folder if set
            if (!string.IsNullOrEmpty(_configuration.Game?.InstallationFolder))
            {
                if (!Directory.Exists(_configuration.Game.InstallationFolder))
                {
                    Log.Warning("Game installation folder does not exist: {GameFolder}", _configuration.Game.InstallationFolder);
                    return false;
                }
            }

            await Task.CompletedTask;
            return true;
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }
}
