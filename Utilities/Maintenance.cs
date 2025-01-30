
using Serilog;

namespace Bundlingway.Utilities
{
    public static class Maintenance
    {

        public static void RemoveTempDir()
        {
            if (Directory.Exists(Instances.TempFolder))
            {
                Directory.Delete(Instances.TempFolder, true);
            }
        }

        internal static async Task PrepareEnvironmentAsync()
        {
            if (!Directory.Exists(Instances.BundlingwayDataFolder))
                Directory.CreateDirectory(Instances.BundlingwayDataFolder);

            Log.Logger = new LoggerConfiguration()
                      .WriteTo.File(Path.Combine(Instances.BundlingwayDataFolder, Constants.WellKnown.LogFileName))
                      .CreateLogger();

            Log.Information("==============================================================================================");


        }
    }
}
