using Bundlingway.Utilities;
using Bundlingway.Core.Services;
using Bundlingway.UI.WinForms; // Assuming WinForms services are here
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
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
                // Prepare the environment (e.g., create necessary directories)
                Maintenance.PrepareEnvironmentAsync(envService).Wait();

                // Initialize application configuration
                ApplicationConfiguration.Initialize();

                // Set up DI container
                var services = new ServiceCollection();

                // Register core services (no UI dependencies)
                services.AddSingleton<IAppEnvironmentService>(envService);
                services.AddSingleton<IConfigurationService>(provider =>
                {
                    var configService = new ConfigurationService(Path.Combine(envService.BundlingwayDataFolder, Constants.Files.BundlingwayConfig));
                    configService.LoadAsync().Wait();
                    return configService;
                });
                services.AddSingleton<IFileSystemService, FileSystemService>();
                services.AddSingleton<IHttpClientService, HttpClientService>();
                services.AddSingleton<PackageService>();
                services.AddSingleton<BundlingwayService>();
                services.AddSingleton<ReShadeService>();
                services.AddSingleton<GPosingwayService>();
                services.AddSingleton<ICommandLineService, CommandLineService>();
                services.AddSingleton<IApplicationHost, ApplicationHost>();
                services.AddLogging(configure => configure.AddConsole());
                services.AddSingleton<EnvironmentService>();

                // Defer building the provider until after UI services are registered

                // Register frmLanding as singleton
                services.AddSingleton<frmLanding>();
                // Register frmShortcuts for DI
                services.AddTransient<frmShortcuts>(provider =>
                    new Bundlingway.UI.WinForms.frmShortcuts(
                        provider.GetRequiredService<PackageService>(),
                        provider.GetRequiredService<IConfigurationService>()
                    )
                );
                // Register UI-specific services with the actual mainForm (resolved from provider)
                services.AddSingleton<IUserNotificationService>(provider => new WinFormsNotificationService(provider.GetRequiredService<frmLanding>()));
                services.AddSingleton<IProgressReporter>(provider => new WinFormsProgressReporter(provider.GetRequiredService<frmLanding>()));
                // Headless mode registration
                if (args != null && args.Length > 0 && args[0] == "--headless")
                {
                    services.AddSingleton<IUserNotificationService, ConsoleNotificationService>();
                    services.AddSingleton<IProgressReporter, ConsoleProgressReporter>();
                }

                // Now build the final provider
                var serviceProvider = services.BuildServiceProvider();

                // Resolve main form from DI
                var mainForm = serviceProvider.GetRequiredService<frmLanding>();

                // Initialize services in the main form
                mainForm.InitializeServices(serviceProvider);

                // Run the application in UI mode
                Application.Run(mainForm);

                // Headless mode entry point
                if (args != null && args.Length > 0 && args[0] == "--headless")
                {
                    var appHost = serviceProvider.GetRequiredService<IApplicationHost>();
                    await appHost.InitializeAsync();
                    await HeadlessScript.RunAsync(serviceProvider, args);
                    await appHost.ShutdownAsync();
                    return;
                }
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
    }
}
