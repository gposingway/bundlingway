
namespace Bundlingway.Utilities.Handler
{
    public static class CommandLineArgs
    {
        public static async Task ProcessAsync(string[] args)
        {
            if (args == null || args.Length == 0) return;

            var url = args[0];
            if (url.StartsWith("gwpreset://open/?", StringComparison.OrdinalIgnoreCase))
            {
                url = url["gwpreset://open/?".Length..];

                // Run the installation form in a separate thread

                await UI.NotifyAsync("Installing package", url);
                await Package.DownloadAndInstall(url);
            }
        }
    }
}
