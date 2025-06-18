using Bundlingway.Utilities;
using Bundlingway.Core.Services;
using Bundlingway.Core.Interfaces;
using Bundlingway.UI.WinForms; // Assuming WinForms services are here
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace Bundlingway
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
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
                services.AddSingleton<IPackageService, PackageService>();
                services.AddSingleton<BundlingwayService>();
                services.AddSingleton<ReShadeService>();
                services.AddSingleton<GPosingwayService>();
                services.AddSingleton<ICommandLineService, CommandLineService>();

                // Defer building the provider until after UI services are registered

                // Create main form instance, passing a temporary provider if needed
                var tempProvider = services.BuildServiceProvider();
                frmLanding mainForm = new frmLanding(tempProvider);

                // Register UI-specific services with the actual mainForm
                services.AddSingleton<IUserNotificationService>(provider => new WinFormsNotificationService(mainForm));
                services.AddSingleton<IProgressReporter>(provider => new WinFormsProgressReporter(mainForm));

                // Now build the final provider
                var serviceProvider = services.BuildServiceProvider();

                // ModernUI bridge (static)
                ModernUI.Initialize();

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
    }
}
