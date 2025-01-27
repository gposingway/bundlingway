using Bundlingway.Utilities;
using System.Reflection;
using System.Text.Json;

namespace Bundlingway.Model
{
    public class ConfigProvider<T> where T : new()
    {
        public readonly string configFilePath;
        public readonly string appName;
        public readonly string appVersion;
        public readonly string versionedLocalAppDataPath;
        public readonly string localAppDataPath;

        public T Configuration = new();

        public ConfigProvider()
        {
            appName = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;
            appVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version;

            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            localAppDataPath = Path.Combine(localAppData, appName);
            versionedLocalAppDataPath = Path.Combine(localAppData, appName, appVersion);
            Directory.CreateDirectory(versionedLocalAppDataPath); // Ensure the directory exists

            configFilePath = Path.Combine(versionedLocalAppDataPath, "config.json");

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
