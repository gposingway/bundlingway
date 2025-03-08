
using Bundlingway.Utilities.Extensions;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Serilog;

namespace Bundlingway.Utilities
{
    public static class Maintenance
    {

        public static void RemoveTempDir()
        {
            if (Directory.Exists(Instances.TempFolder))
                Directory.Delete(Instances.TempFolder, true);
        }

        internal static async Task EnsureConfiguration()
        {

            var mustRefresh = false;

            if (Instances.LocalConfigProvider.Configuration.Shortcuts == null)
            {
                Instances.LocalConfigProvider.Configuration.Shortcuts = [];
                mustRefresh = true;
            }

            foreach (var kvp in Constants.DefaultShortcuts)
            {
                if (!Instances.LocalConfigProvider.Configuration.Shortcuts.ContainsKey(kvp.Key))
                {
                    Instances.LocalConfigProvider.Configuration.Shortcuts.Add(kvp.Key, kvp.Value);
                    mustRefresh = true;

                }

            }

            if (mustRefresh) Instances.LocalConfigProvider.Save();
        }

        internal static async Task PrepareEnvironmentAsync()
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Converters = { new DescriptionConverter() }
            };

            JsonConvert.DefaultSettings = () => settings;

            if (!Directory.Exists(Instances.BundlingwayDataFolder))
                Directory.CreateDirectory(Instances.BundlingwayDataFolder);

            if (!Directory.Exists(Instances.SinglePresetsFolder))
                Directory.CreateDirectory(Instances.SinglePresetsFolder);

            string localCatalogFilePath = Path.Combine(Instances.SinglePresetsFolder, Constants.Files.CatalogEntry);

            if (!File.Exists(localCatalogFilePath)) Constants.SingleFileCatalog.ToJsonFile(localCatalogFilePath);

            Log.Logger = new LoggerConfiguration()
                      .WriteTo.File(
                        Path.Combine(Instances.BundlingwayDataFolder, Constants.Files.Log),
                        fileSizeLimitBytes: 1 * 1024 * 1024, // 1 MB
                        retainedFileCountLimit: 6,
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true
                      )
                      .CreateLogger();

            Log.Information("==============================================================================================");
        }
    }
}
