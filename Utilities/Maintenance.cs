
using Bundlingway.Utilities.Extensions;
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

        internal static async Task PrepareEnvironmentAsync()
        {
            if (!Directory.Exists(Instances.BundlingwayDataFolder))
                Directory.CreateDirectory(Instances.BundlingwayDataFolder);

            if (!Directory.Exists(Instances.SinglePresetsFolder))
                Directory.CreateDirectory(Instances.SinglePresetsFolder);


            Instances.IsGameRunning = ProcessHelper.IsProcessRunning("ffxiv_dx11");

            string localCatalogFilePath = Path.Combine(Instances.SinglePresetsFolder, Constants.Files.CatalogEntry);

            if (!File.Exists(localCatalogFilePath)) Constants.SingleFileCatalog.ToJsonFile(localCatalogFilePath);



            Log.Logger = new LoggerConfiguration()
                      .WriteTo.File(
                        Path.Combine(Instances.BundlingwayDataFolder, Constants.Files.Log),
                        fileSizeLimitBytes: 8388608, // 8 MB
                        retainedFileCountLimit: 5,
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true
                      )
                      .CreateLogger();

            Log.Information("==============================================================================================");
        }
    }
}
