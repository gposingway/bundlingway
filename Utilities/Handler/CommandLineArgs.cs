using Serilog;
using System.Diagnostics;

namespace Bundlingway.Utilities.Handler
{
    public static class CommandLineArgs
    {
        public static async Task<string> ProcessAsync(string[] args)
        {
            if (args == null || args.Length == 0) return null;

            var firstArg = args[0];
            Log.Information($"Processing command line argument: {firstArg}");

            var prefix = Constants.GPosingwayProtocolHandler + "://open/?";


            if (firstArg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                firstArg = firstArg[prefix.Length..];
                Log.Information($"Opening preset with argument: {firstArg}");

                // Run the installation form in a separate thread
                await UI.NotifyAsync("Installing package", firstArg);

                var packageName = Package.DownloadAndInstall(firstArg).Result; // Consider making this async
                await UI.NotifyAsync("Installation", packageName);
                Log.Information(packageName);

                return packageName;
            }

            if (firstArg == Constants.CommandLineOptions.UpdateClient)
            {
                var targetFile = Instances.LocalConfigProvider.Configuration.Bundlingway.Location;
                Log.Information($"Updating client with target file: {targetFile}");

                if (File.Exists(targetFile))
                {
                    var currentExePath = Process.GetCurrentProcess().MainModule.FileName;
                    Log.Information($"Copying current executable from {currentExePath} to {targetFile}");

                    var processName = Path.GetFileNameWithoutExtension(currentExePath);
                    var existingProcesses = Process.GetProcessesByName(processName)
                                                     .Where(p => p.Id != Process.GetCurrentProcess().Id)
                                                     .ToList();

                    if (existingProcesses.Any())
                    {
                        Log.Information($"Waiting for existing process {processName} to exit.");
                        foreach (var process in existingProcesses)
                        {
                            int waitAttempts = 0;
                            while (!process.HasExited && waitAttempts < 30) // Check for exit with shorter intervals
                            {
                                Log.Debug($"Waiting for process {process.Id}, attempt {waitAttempts + 1}");
                                process.WaitForExit(1000); // Wait 1 second
                                waitAttempts++;
                            }
                            if (!process.HasExited)
                            {
                                Log.Warning($"Process {process.Id} did not exit gracefully after waiting. Proceeding with update.");
                                // Optionally: process.Kill(); - Use with caution!
                            }
                            else
                            {
                                Log.Information($"Process {process.Id} exited.");
                            }
                        }
                    }

                    bool copySuccess = false;
                    int copyRetries = 3;
                    while (!copySuccess && copyRetries > 0)
                    {
                        try
                        {
                            File.Copy(currentExePath, targetFile, true);
                            copySuccess = true;
                            Log.Information($"File copied successfully to {targetFile}");
                        }
                        catch (IOException ex) when (ex.HResult == -2147024864) // Error code for sharing violation (File in use)
                        {
                            copyRetries--;
                            Log.Warning($"File copy failed due to sharing violation. Retrying in 1 second... Retries remaining: {copyRetries}");
                            await Task.Delay(1000);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Error during file copy to {targetFile}");
                            return null; // Or handle error as needed
                        }
                    }

                    if (copySuccess)
                    {
                        try
                        {
                            var processStartInfo = new ProcessStartInfo { FileName = targetFile, UseShellExecute = true };
                            var process = Process.Start(processStartInfo);
                            Log.Information("New client process started. Exiting current process.");
                            Environment.Exit(0);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Error starting new process.");
                            // Handle process start failure - maybe log and don't exit?
                            return null;
                        }
                    }
                    else
                    {
                        Log.Error($"File copy failed after multiple retries to {targetFile}. Update aborted.");
                        return null; // Or handle copy failure
                    }
                }
                else
                {
                    Log.Error($"CommandLineArgs.ProcessAsync: {targetFile} not found.");
                }
            }

            return null;
        }
    }
}