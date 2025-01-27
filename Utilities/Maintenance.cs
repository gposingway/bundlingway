using Bundlingway.Model;

namespace Bundlingway.Utilities
{
    public static class Maintenance
    {

        public static void RemoveTempDir()
        {
            if (Directory.Exists(Instances.AppDataTempFolder))
            {
                Directory.Delete(Instances.AppDataTempFolder, true);
            }
        }
        public static void RemoveCacheDir()
        {
            if (Directory.Exists(Instances.AppDataCacheFolder))
            {
                Directory.Delete(Instances.AppDataCacheFolder, true);
            }
        }
    }
}
