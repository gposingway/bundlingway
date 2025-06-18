using Bundlingway.Core.Services;
using Bundlingway.Utilities.Extensions;
using Serilog;
using System.Reflection;

namespace Bundlingway.Model
{
    public class ConfigProvider<T> where T : new()
    {
        private readonly IAppEnvironmentService _envService;

        public T Configuration = new();

        public ConfigProvider(IAppEnvironmentService envService)
        {
            _envService = envService;
            Load();
        }

        public void Save()
        {
            try
            {
                Configuration.ToJsonFile(_envService.ConfigFilePath);
            }
            catch (Exception ex)
            {
                Log.Information("Error saving configuration: " + ex.Message);
            }
        }

        public void Load()
        {
            if (File.Exists(_envService.ConfigFilePath))
            {
                try
                {
                    Configuration = SerializationExtensions.FromJsonFile<T>(_envService.ConfigFilePath);
                }
                catch (Exception ex)
                {
                    Log.Information("Error loading configuration: " + ex.Message);
                }
            }
            else
            {
                Log.Information("Configuration file not found.");
            }
        }
    }
}
