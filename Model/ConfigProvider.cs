using Bundlingway.Utilities.Extensions;
using System.Reflection;

namespace Bundlingway.Model
{
    public class ConfigProvider<T> where T : new()
    {
        public readonly string configFilePath;
        public readonly string appName;
        public readonly string appVersion;
        public readonly string commonAppDataPath;

        public T Configuration = new();

        public ConfigProvider()
        {
            appName = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;
            appVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version;

            string commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            commonAppDataPath = Path.Combine(commonAppData, appName);

            Directory.CreateDirectory(commonAppDataPath);
            configFilePath = Path.Combine(commonAppDataPath, "config.json");

            Load();
        }

        public void Save()
        {
            try
            {
                Configuration.ToJsonFile(configFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving configuration: " + ex.Message);
            }
        }

        public void Load()
        {
            if (File.Exists(configFilePath))
            {
                try
                {
                    Configuration = Serialization.FromJsonFile<T>(configFilePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading configuration: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Configuration file not found.");
            }
        }
    }
}
