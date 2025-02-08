
using Serilog;
using System.Diagnostics;

namespace Bundlingway.Utilities.Handler
{
    public static class CommandLineArgs
    {
        public static async Task ProcessAsync(string[] args)
        {
            if (args == null || args.Length == 0) return;

            var firstArg = args[0];
            Log.Information($"Processing command line argument: {firstArg}");

            if (firstArg.StartsWith("gwpreset://open/?", StringComparison.OrdinalIgnoreCase))
            {
                firstArg = firstArg["gwpreset://open/?".Length..];
                Log.Information($"Opening preset with argument: {firstArg}");

                // Run the installation form in a separate thread
                await UI.NotifyAsync("Installing package", firstArg);
                var packageName = Package.DownloadAndInstall(firstArg).Result;
                await UI.NotifyAsync("Installation Complete", packageName);
                Log.Information($"Package {packageName} installed successfully.");
            }

            if (firstArg == Constants.CommandLineOptions.UpdateClient)
            {
                var targetFile = Instances.LocalConfigProvider.Configuration.Bundlingway.Location;
                Log.Information($"Updating client with target file: {targetFile}");

                if (File.Exists(targetFile))
                {
                    var currentExePath = Process.GetCurrentProcess().MainModule.FileName;
                    Log.Information($"Copying current executable from {currentExePath} to {targetFile}");
                    File.Copy(currentExePath, targetFile, true);

                    var process = new Process { StartInfo = new ProcessStartInfo { FileName = targetFile, UseShellExecute = true } };
                    process.Start();
                    Log.Information("New client process started. Exiting current process.");

                    Environment.Exit(0);
                }
                else
                {
                    Log.Error($"CommandLineArgs.ProcessAsync: {targetFile} not found.");
                }
            }
        }
    }
}
