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
    }
}
