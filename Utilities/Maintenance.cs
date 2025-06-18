using Bundlingway.Core.Interfaces;
using Bundlingway.Core.Services;
using Bundlingway.Utilities.Extensions;
using Newtonsoft.Json;
using Serilog;

namespace Bundlingway.Utilities
{
    public static class Maintenance
    {
        public static void RemoveTempDir(IAppEnvironmentService envService)
        {
            if (Directory.Exists(envService.TempFolder))
                Directory.Delete(envService.TempFolder, true);
        }

        internal static async Task EnsureConfiguration(IConfigurationService configService)
        {
            var mustRefresh = false;

            if (configService.Configuration.Shortcuts == null)
            {
                configService.Configuration.Shortcuts = [];
                mustRefresh = true;
            }

            foreach (var kvp in Constants.DefaultShortcuts)
            {
                if (!configService.Configuration.Shortcuts.ContainsKey(kvp.Key))
                {
                    configService.Configuration.Shortcuts.Add(kvp.Key, kvp.Value);
                    mustRefresh = true;
                }
            }

            if (mustRefresh) await configService.SaveAsync();
        }

        internal static async Task PrepareEnvironmentAsync(IAppEnvironmentService envService)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Converters = { new DescriptionConverter() }
            };

            JsonConvert.DefaultSettings = () => settings;

            if (!Directory.Exists(envService.BundlingwayDataFolder))
                Directory.CreateDirectory(envService.BundlingwayDataFolder);

            if (!Directory.Exists(envService.SinglePresetsFolder))
                Directory.CreateDirectory(envService.SinglePresetsFolder);

            string localCatalogFilePath = Path.Combine(envService.SinglePresetsFolder, Constants.Files.CatalogEntry);

            if (!File.Exists(localCatalogFilePath)) Constants.SingleFileCatalog(envService).ToJsonFile(localCatalogFilePath);

            Log.Logger = new LoggerConfiguration()
                      .WriteTo.File(
                        Path.Combine(envService.BundlingwayDataFolder, Constants.Files.Log),
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
