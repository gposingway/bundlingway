using Bundlingway.Utilities.Extensions;
using Serilog;
using System.Reflection;

namespace Bundlingway.Model
{
    public class ConfigProvider<T> where T : new()
    {
        public T Configuration = new();

        public ConfigProvider()
        {
            Load();
        }

        public void Save()
        {
            try
            {
                Configuration.ToJsonFile(Instances.ConfigFilePath);
            }
            catch (Exception ex)
            {
                Log.Information("Error saving configuration: " + ex.Message);
            }
        }

        public void Load()
        {
            if (File.Exists(Instances.ConfigFilePath))
            {
                try
                {
                    Configuration = Serialization.FromJsonFile<T>(Instances.ConfigFilePath);
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
