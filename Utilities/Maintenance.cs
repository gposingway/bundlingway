using Bundlingway.Core.Interfaces;
using Bundlingway.Core.Services;
using Bundlingway.Utilities.Extensions;
using Newtonsoft.Json;
using Serilog;

namespace Bundlingway.Utilities
{
    public static class Maintenance
    {
        private static IConfigurationService ConfigService => ServiceLocator.GetService<IConfigurationService>();

        public static void RemoveTempDir()
        {
            if (Directory.Exists(Instances.TempFolder))
                Directory.Delete(Instances.TempFolder, true);
        }

        internal static async Task EnsureConfiguration()
        {

            var mustRefresh = false;

            if (ConfigService.Configuration.Shortcuts == null)
            {
                ConfigService.Configuration.Shortcuts = [];
                mustRefresh = true;
            }

            foreach (var kvp in Constants.DefaultShortcuts)
            {
                if (!ConfigService.Configuration.Shortcuts.ContainsKey(kvp.Key))
                {
                    ConfigService.Configuration.Shortcuts.Add(kvp.Key, kvp.Value);
                    mustRefresh = true;
                }
            }

            if (mustRefresh) await ConfigService.SaveAsync();
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
                        fileSizeLimitBytes: 1 * 1024 * 1024 * 1024, // 1 GB
                        retainedFileCountLimit: 10,
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true
                      )
                      .WriteTo.Console(Serilog.Events.LogEventLevel.Verbose)
                      .CreateLogger();

            Log.Information("==============================================================================================");
        }
    }
}
