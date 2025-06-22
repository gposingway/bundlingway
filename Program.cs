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
        }
        static async Task MainAsync(string[] args)
        {
            var envService = new AppEnvironmentService();

            try
            {
                // Check if this is a protocol invocation EARLY
                bool isProtocolInvocation = args?.Length > 0 && args[0].StartsWith(Constants.GPosingwayProtocolHandler);
                if (isProtocolInvocation && args != null)
                {
                    // Handle the protocol ourselves in headless mode
                    await HandleProtocolInvocation(envService, args[0]);
                    return;
                }                // Regular UI startup - check for existing instance
                var singleInstanceService = new SingleInstanceService();
                if (await singleInstanceService.IsAnotherInstanceRunningAsync())
                {
                    // Another UI instance is already running, bring it to front and exit
                    var protocolService = new ProtocolServerService(null!); // Temporary for client-only usage
                    await protocolService.BringExistingInstanceToFrontAsync();
                    return;
                }

                // Prepare the environment (e.g., create necessary directories)
                Maintenance.PrepareEnvironmentAsync(envService).Wait();

                // Initialize application configuration
                ApplicationConfiguration.Initialize();                // Set up DI container
                var services = new ServiceCollection();

                RegisterCoreServices(services, envService);

                // Register frmLanding as singleton
                services.AddSingleton<frmLanding>();                // Register UI-specific services with the actual mainForm (resolved from provider)
                services.AddSingleton<IUserNotificationService>(provider => new WinFormsNotificationService(provider.GetRequiredService<frmLanding>()));
                services.AddSingleton<IProgressReporter>(provider => new WinFormsProgressReporter(provider.GetRequiredService<frmLanding>()));
                // Register frmShortcuts for DI
                services.AddTransient<frmShortcuts>(provider =>
                    new frmShortcuts(
                        provider.GetRequiredService<PackageService>(),
                        provider.GetRequiredService<IConfigurationService>()
                    )
                );

                // Now build the final provider
                var serviceProvider = services.BuildServiceProvider();

                // Resolve main form from DI
                frmLanding mainForm = serviceProvider.GetRequiredService<frmLanding>();

                // Initialize services in the main form
                mainForm.InitializeServices(serviceProvider);

                // ModernUI bridge (static)
                var notificationService = serviceProvider.GetRequiredService<IUserNotificationService>();
                var progressReporter = serviceProvider.GetRequiredService<IProgressReporter>();
                ModernUI.Initialize(notificationService, progressReporter);                // Start protocol server for inter-instance communication
                var protocolServer = serviceProvider.GetRequiredService<IProtocolServerService>();
                _ = Task.Run(() => protocolServer.StartAsync());

                // Run the application in UI mode
                Application.Run(mainForm);


            }
            catch (Exception ex)
            {
                // Log startup errors and show user-friendly message
                try
                {
                    Serilog.Log.Fatal(ex, "Fatal error during application startup");
                }
                catch
                {
                    // If logging fails, show basic error dialog
                }

                MessageBox.Show($"Bundlingway failed to start properly. Error: {ex.Message}\n\nPlease check the log files for more details.",
                               "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private static void RegisterCoreServices(IServiceCollection services, IAppEnvironmentService envService)
        {
            // Register core services needed by both UI and headless modes
            services.AddSingleton<IAppEnvironmentService>(envService);
            services.AddSingleton<IConfigurationService>(provider =>
            {
                var configService = new ConfigurationService(Path.Combine(envService.BundlingwayDataFolder, Constants.Files.BundlingwayConfig));
                configService.LoadAsync().Wait();
                return configService;
            });
            services.AddSingleton<IFileSystemService, FileSystemService>();
            services.AddSingleton<IHttpClientService, HttpClientService>();
            services.AddSingleton<PostProcessorService>();
            services.AddSingleton<PackageService>();
            services.AddSingleton<BundlingwayService>();
            services.AddSingleton<ReShadeService>();
            services.AddSingleton<GPosingwayService>();
            services.AddSingleton<ICommandLineService, CommandLineService>();
            services.AddSingleton<IApplicationHost, ApplicationHost>();
            services.AddLogging(configure => configure.AddConsole());            services.AddSingleton<EnvironmentService>();
            services.AddSingleton<ISingleInstanceService, SingleInstanceService>();
            services.AddSingleton<IProtocolServerService, ProtocolServerService>();
        }

        private static async Task HandleProtocolInvocation(IAppEnvironmentService envService, string protocolUrl)
        {
            try
            {
                // Prepare minimal environment for headless operation
                Maintenance.PrepareEnvironmentAsync(envService).Wait();                // Set up minimal DI container for headless operation
                var services = new ServiceCollection();
                RegisterCoreServices(services, envService);

                services.AddSingleton<IUserNotificationService, ConsoleNotificationService>();
                services.AddSingleton<IProgressReporter, ConsoleProgressReporter>();

                var serviceProvider = services.BuildServiceProvider();
                var commandLineService = serviceProvider.GetRequiredService<ICommandLineService>();// Handle the protocol URL
                await commandLineService.HandleProtocolAsync(protocolUrl);

                // Notify any existing UI instance about success
                var protocolService = serviceProvider.GetRequiredService<IProtocolServerService>();
                await protocolService.NotifyExistingInstanceAsync("Package installed successfully from browser!");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to handle protocol invocation: {ProtocolUrl}", protocolUrl);

                // Try to notify existing instance about the failure
                try
                {
                    var services = new ServiceCollection();
                    RegisterCoreServices(services, envService);
                    services.AddSingleton<IUserNotificationService, ConsoleNotificationService>();
                    services.AddSingleton<IProgressReporter, ConsoleProgressReporter>();

                    var serviceProvider = services.BuildServiceProvider();
                    var protocolService = serviceProvider.GetRequiredService<IProtocolServerService>();
                    await protocolService.NotifyExistingInstanceAsync("Failed to install package from browser due to an error.");
                }
                catch
                {
                    // Ignore notification failures during error handling
                }
            }
        }
    }
}
