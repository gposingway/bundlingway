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

                // Register core services
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

                // UI-specific services (will be replaced after mainForm is created)
                services.AddSingleton<IUserNotificationService>(provider => new WinFormsNotificationService(null));
                services.AddSingleton<IProgressReporter>(provider => new WinFormsProgressReporter(null));

                // Build the service provider
                var serviceProvider = services.BuildServiceProvider();

                // Initialize the application (e.g., load settings)
                Bootstrap.Initialize().Wait();
                var configService = serviceProvider.GetRequiredService<IConfigurationService>();
                Maintenance.EnsureConfiguration(configService).Wait();

                // Check for duplicate instances (UI mode)
                if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
                {
                    // Notify other instances and close the client
                    var processHelper = serviceProvider.GetRequiredService<IFileSystemService>();
                    ProcessHelper.NotifyOtherInstances(new Model.IPCNotification() { Topic = Constants.Events.DuplicatedInstances, Message = string.Empty });
                    Process.GetCurrentProcess().Kill();
                }

                // Create main form instance, passing the service provider
                frmLanding mainForm = new frmLanding(serviceProvider);

                // Replace UI-specific services with mainForm-aware implementations
                var userNotificationService = new WinFormsNotificationService(mainForm);
                var progressReporter = new WinFormsProgressReporter(mainForm);
                services.AddSingleton<IUserNotificationService>(userNotificationService);
                services.AddSingleton<IProgressReporter>(progressReporter);

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
