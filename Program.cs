using Bundlingway.Utilities;
using Bundlingway.Core.Services;
using Bundlingway.UI.WinForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Bundlingway.Core.Interfaces;

namespace Bundlingway
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }        static async Task MainAsync(string[] args)
        {
            var envService = new AppEnvironmentService();            try
            {                // Check if this is an elevated operation request
                if (args?.Length > 0 && args[0].StartsWith("--elevated-operation="))
                {
                    var elevatedExitCode = await HandleElevatedOperationAsync(envService, args);
                    if (elevatedExitCode != 0)
                    {
                        // If operation failed, exit with error code
                        Environment.Exit(elevatedExitCode);
                        return;
                    }
                    // If operation succeeded, continue with normal app flow (now elevated)
                    // This allows the user to keep working without restart
                }

                // Check if this is completion of an elevated operation
                if (args?.Length > 0 && args[0] == "--elevated-operation-completed")
                {
                    // Continue with normal startup, but maybe show a success message
                    // The elevated operation was completed successfully
                }

                bool isProtocolInvocation = args?.Length > 0 && args[0].StartsWith(Constants.GPosingwayProtocolHandler);
                if (isProtocolInvocation && args != null)
                {
                    await HandleProtocolInvocation(envService, args[0]);
                    return;
                }

                var singleInstanceService = new SingleInstanceService();
                if (await singleInstanceService.IsAnotherInstanceRunningAsync())
                {
                    var protocolService = new ProtocolServerService(null!);
                    await protocolService.BringExistingInstanceToFrontAsync();
                    return;
                }

                Maintenance.PrepareEnvironmentAsync(envService).Wait();
                ApplicationConfiguration.Initialize();

                var services = new ServiceCollection();
                RegisterCoreServices(services, envService); services.AddSingleton<frmLanding>();

                services.AddSingleton<IUserNotificationService>(provider => new WinFormsNotificationService(provider.GetRequiredService<frmLanding>()));
                services.AddSingleton<IProgressReporter>(provider => new WinFormsProgressReporter(provider.GetRequiredService<frmLanding>()));                services.AddTransient<frmShortcuts>(provider =>
                    new frmShortcuts(
                        provider.GetRequiredService<PackageService>(),
                        provider.GetRequiredService<IConfigurationService>(),
                        provider.GetRequiredService<IUserNotificationService>()
                    )
                );var serviceProvider = services.BuildServiceProvider();
                frmLanding mainForm = serviceProvider.GetRequiredService<frmLanding>();
                mainForm.InitializeServices(serviceProvider);

                var protocolServer = serviceProvider.GetRequiredService<IProtocolServerService>();
                _ = Task.Run(() => protocolServer.StartAsync());

                Application.Run(mainForm);


            }
            catch (Exception ex)
            {
                try
                {
                    Serilog.Log.Fatal(ex, "Fatal error during application startup");
                }
                catch
                {
                }

                MessageBox.Show($"Bundlingway failed to start properly. Error: {ex.Message}\n\nPlease check the log files for more details.",
                               "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private static void RegisterCoreServices(IServiceCollection services, IAppEnvironmentService envService)
        {
            services.AddSingleton<IAppEnvironmentService>(envService);
            services.AddSingleton<IConfigurationService>(provider =>
            {
                var configService = new ConfigurationService(Path.Combine(envService.BundlingwayDataFolder, Constants.Files.BundlingwayConfig));
                configService.LoadAsync().Wait();
                return configService;
            });            services.AddSingleton<IFileSystemService, FileSystemService>();
            services.AddSingleton<IHttpClientService, HttpClientService>();
            services.AddSingleton<PostProcessorService>();
            services.AddSingleton<PackageService>();
            services.AddSingleton<BundlingwayService>();
            services.AddSingleton<ReShadeService>();
            services.AddSingleton<GPosingwayService>();
            services.AddSingleton<ICommandLineService, CommandLineService>();
            services.AddSingleton<IApplicationHost, ApplicationHost>();
            services.AddSingleton<IElevationService, ElevationService>();
            services.AddSingleton<ElevatedOperationHandler>();
            services.AddLogging(configure => configure.AddConsole());
            services.AddSingleton<EnvironmentService>();
            services.AddSingleton<ISingleInstanceService, SingleInstanceService>();
            services.AddSingleton<IProtocolServerService, ProtocolServerService>();
        }
        private static async Task HandleProtocolInvocation(IAppEnvironmentService envService, string protocolUrl)
        {
            IProtocolServerService? protocolService = null;
            try
            {
                Maintenance.PrepareEnvironmentAsync(envService).Wait();

                var services = new ServiceCollection();
                RegisterCoreServices(services, envService);

                services.AddSingleton<IUserNotificationService, ConsoleNotificationService>();
                services.AddSingleton<IProgressReporter, ConsoleProgressReporter>();

                var serviceProvider = services.BuildServiceProvider();
                protocolService = serviceProvider.GetRequiredService<IProtocolServerService>();

                var packageName = ExtractPackageNameFromUrl(protocolUrl);

                if (!string.IsNullOrEmpty(packageName))
                {
                    await protocolService.NotifyExistingInstanceAsync($"Browser request received for package: {packageName}");
                }
                else
                {
                    await protocolService.NotifyExistingInstanceAsync("Browser installation request received...");
                }

                await protocolService.NotifyExistingInstanceAsync("Starting package installation...");

                var commandLineService = serviceProvider.GetRequiredService<ICommandLineService>();
                var installedPackageName = await commandLineService.HandleProtocolAsync(protocolUrl);

                if (!string.IsNullOrEmpty(installedPackageName))
                {
                    await protocolService.NotifyExistingInstanceAsync($"Package '{installedPackageName}' installed successfully from browser!");
                }
                else
                {
                    await protocolService.NotifyExistingInstanceAsync("Package installed successfully from browser!");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to handle protocol invocation: {ProtocolUrl}", protocolUrl);

                try
                {
                    if (protocolService == null)
                    {
                        var services = new ServiceCollection();
                        RegisterCoreServices(services, envService);
                        services.AddSingleton<IUserNotificationService, ConsoleNotificationService>();
                        services.AddSingleton<IProgressReporter, ConsoleProgressReporter>();

                        var serviceProvider = services.BuildServiceProvider();
                        protocolService = serviceProvider.GetRequiredService<IProtocolServerService>();
                    }

                    await protocolService.NotifyExistingInstanceAsync($"Failed to install package from browser: {ex.Message}");
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Extracts the package name from a protocol URL for early notification purposes.
        /// </summary>
        /// <param name="protocolUrl">The protocol URL to parse</param>
        /// <returns>The package name if found, otherwise null</returns>
        private static string ExtractPackageNameFromUrl(string protocolUrl)
        {
            try
            {
                var prefix = Constants.GPosingwayProtocolHandler + "://open/?";
                if (!protocolUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return null;

                var queryString = protocolUrl.Substring(prefix.Length);
                var queryParams = System.Web.HttpUtility.ParseQueryString(queryString);

                return queryParams["name"];
            }
            catch (Exception ex)
            {
                Serilog.Log.Debug(ex, "Failed to extract package name from URL: {ProtocolUrl}", protocolUrl);
                return null;
            }
        }        /// <summary>
        /// Handles elevated operations when the application is restarted with admin privileges.
        /// </summary>
        private static async Task<int> HandleElevatedOperationAsync(IAppEnvironmentService envService, string[] args)
        {
            try
            {
                Maintenance.PrepareEnvironmentAsync(envService).Wait();

                var services = new ServiceCollection();
                RegisterCoreServices(services, envService);

                // Use console-based services for elevated operations (no UI)
                services.AddSingleton<IUserNotificationService, ConsoleNotificationService>();
                services.AddSingleton<IProgressReporter, ConsoleProgressReporter>();

                var serviceProvider = services.BuildServiceProvider();
                var handler = serviceProvider.GetRequiredService<ElevatedOperationHandler>();

                // Extract operation ID and arguments
                var operationArg = args[0];
                var operationId = operationArg.Substring("--elevated-operation=".Length);
                var remainingArgs = args.Skip(1).ToArray();                var exitCode = await handler.ExecuteOperationAsync(operationId, remainingArgs);
                return exitCode;
            }
            catch (Exception ex)
            {
                Serilog.Log.Fatal(ex, "Fatal error during elevated operation execution");
                return 1;
            }
        }
    }
}
