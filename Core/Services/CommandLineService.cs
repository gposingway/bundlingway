using Bundlingway.Core.Interfaces;
using Bundlingway.Model;
using Bundlingway.Utilities.Extensions;
using Serilog;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Web;

namespace Bundlingway.Core.Services
{
    public class CommandLineService : ICommandLineService
    {
        private readonly PackageService _packageService;
        private readonly IUserNotificationService _notificationService;
        private readonly IConfigurationService _configService;

        public CommandLineService(PackageService packageService, IUserNotificationService notificationService, IConfigurationService configService)
        {
            _packageService = packageService;
            _notificationService = notificationService;
            _configService = configService;
        }

        public async Task<string?> ProcessAsync(string[] args)
        {
            if (args == null || args.Length == 0) return null;

            foreach (var arg in args)
            {
                var currentArg = arg;
                Log.Information($"Processing command line argument: {currentArg}");

                // Handle custom protocol for opening presets (e.g., gposingway://)
                var prefix = Constants.GPosingwayProtocolHandler + "://open/?";

                if (currentArg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    currentArg = currentArg[prefix.Length..];
                    Log.Information($"Opening preset with argument: {currentArg}");

                    var queryParams = HttpUtility.ParseQueryString(currentArg);

                    var name = queryParams["name"];
                    var url = queryParams["url"];

                    var packagePayload = queryParams["package"];

                    DownloadPackage package;

                    if (packagePayload != null)
                    {
                        try
                        {
                            var base64EncodedBytes = Convert.FromBase64String(packagePayload);
                            var decodedPayload = Encoding.UTF8.GetString(base64EncodedBytes);
                            decodedPayload = WebUtility.HtmlDecode(decodedPayload);

                            package = decodedPayload.FromJson<DownloadPackage>();
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Error deserializing package payload.");
                            return null;
                        }
                    }
                    else
                    {
                        if (url == null)
                        {
                            Log.Error("URL not found in query parameters.");
                            return null;
                        }
                        package = new DownloadPackage { Name = name, Url = url };
                    }                    Log.Information($"Parsed name: {name}, url: {url}");

                    await _notificationService.AnnounceAsync($"Downloading package: {name ?? url}");
                    if (package.Url == null || package.Name == null)
                    {
                        Log.Warning("Package Url or Name is null. Aborting installation.");
                        await _notificationService.AnnounceAsync("Package information is incomplete. Installation aborted.");
                        return null;
                    }

                    await _notificationService.AnnounceAsync($"Installing package: {package.Name}");
                    var packageName = await _packageService.DownloadAndInstallAsync(package.Url, package.Name);
                    await _notificationService.AnnounceAsync($"Installation completed: {packageName}");
                    Log.Information(packageName);
                    return packageName;
                }

                // Handle client update command
                if (currentArg == Constants.CommandLineOptions.UpdateClient)
                {
                    var targetFile = _configService.Configuration.Bundlingway.Location;
                    Log.Information($"Updating client with target file: {targetFile}");

                    if (File.Exists(targetFile))
                    {
                        var mainModule = Process.GetCurrentProcess().MainModule;
                        var currentExePath = mainModule != null && mainModule.FileName != null ? mainModule.FileName : string.Empty;
                        Log.Information($"Copying current executable from {currentExePath} to {targetFile}");

                        // Ensure no other instances of the application are running
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
                                while (!process.HasExited && waitAttempts < 30)
                                {
                                    Log.Debug($"Waiting for process {process.Id}, attempt {waitAttempts + 1}");
                                    process.WaitForExit(1000);
                                    waitAttempts++;
                                }
                                if (!process.HasExited)
                                {
                                    Log.Warning($"Process {process.Id} did not exit gracefully after waiting. Proceeding with update.");
                                }
                                else
                                {
                                    Log.Information($"Process {process.Id} exited.");
                                }
                            }
                        }

                        // Copy the current executable to the target location, with retries
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
                            catch (IOException ex) when (ex.HResult == -2147024864)
                            {
                                copyRetries--;
                                Log.Warning($"File copy failed due to sharing violation. Retrying in 1 second... Retries remaining: {copyRetries}");
                                await Task.Delay(1000);
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, $"Error during file copy to {targetFile}");
                                return null;
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
                                return null;
                            }
                        }
                        else
                        {
                            Log.Error($"File copy failed after multiple retries to {targetFile}. Update aborted.");
                            return null;
                        }
                    }
                    else
                    {
                        Log.Error($"CommandLineService.ProcessAsync: {targetFile} not found.");
                    }
                }
            }
            return null;
        }        public async Task<string?> HandleProtocolAsync(string protocolUrl)
        {
            try
            {
                Log.Information($"Handling protocol URL: {protocolUrl}");
                return await ProcessAsync(new[] { protocolUrl });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to handle protocol URL: {ProtocolUrl}", protocolUrl);
                throw;
            }
        }
    }
}
