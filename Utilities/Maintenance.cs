
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

            string localCatalogFilePath = Path.Combine(Instances.SinglePresetsFolder, Constants.WellKnown.CatalogEntryFile);

            if (!File.Exists(localCatalogFilePath)) Constants.SingleFileCatalog.ToJsonFile(localCatalogFilePath);



            Log.Logger = new LoggerConfiguration()
                      .WriteTo.File(
                          Path.Combine(Instances.BundlingwayDataFolder, Constants.WellKnown.LogFileName),
                          fileSizeLimitBytes: 8388608 // 8 MB
                      )
                      .CreateLogger();

            Log.Information("==============================================================================================");
        }
    }
}
