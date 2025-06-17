using Bundlingway.Utilities;
using Bundlingway.Core.Services;
using Bundlingway.Core.Interfaces;
using Bundlingway.UI.WinForms; // Assuming WinForms services are here
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
            try
            {
                // Prepare the environment (e.g., create necessary directories)
                Maintenance.PrepareEnvironmentAsync().Wait();

                // Initialize application configuration
                ApplicationConfiguration.Initialize();

                // Initialize core services BEFORE other initialization
                ServiceLocator.InitializeCoreServices(Instances.ConfigFilePath);

                // Initialize the application (e.g., load settings)
                Bootstrap.Initialize().Wait();
                Instances.LocalConfigProvider.Load();
                Maintenance.EnsureConfiguration().Wait();

                // Check for duplicate instances (UI mode)
                if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
                {
                    // Notify other instances and close the client
                    ProcessHelper.NotifyOtherInstances(new Model.IPCNotification() { Topic = Constants.Events.DuplicatedInstances });
                    Process.GetCurrentProcess().Kill();
                }

                // Core services
                var configService = new ConfigurationService(Path.Combine(Instances.BundlingwayDataFolder, Constants.Files.BundlingwayConfig));
                ServiceLocator.Register<IConfigurationService>(configService);
                ServiceLocator.Register<IFileSystemService>(new FileSystemService());
                ServiceLocator.Register<IHttpClientService>(new HttpClientService());
                // Register BundlingwayService (which depends on the above)
                ServiceLocator.Register(new BundlingwayService(
                    configService,
                    ServiceLocator.TryGetService<IHttpClientService>()!,
                    ServiceLocator.TryGetService<IFileSystemService>()!,
                    new WinFormsProgressReporter(null), // Will be replaced after mainForm is created
                    new WinFormsNotificationService(null) // Will be replaced after mainForm is created
                ));

                // Register PackageService for service-based package management
                ServiceLocator.Register<IPackageService>(new PackageService(
                    configService,
                    ServiceLocator.TryGetService<IFileSystemService>()!,
                    ServiceLocator.TryGetService<IHttpClientService>()!,
                    new WinFormsProgressReporter(null), // Will be replaced after mainForm is created
                    new WinFormsNotificationService(null) // Will be replaced after mainForm is created
                ));

                // Register ReShadeService for service-based ReShade management
                ServiceLocator.Register<IReShadeService>(new ReShadeService(
                    configService,
                    ServiceLocator.TryGetService<IFileSystemService>()!,
                    new WinFormsNotificationService(null),
                    ServiceLocator.TryGetService<IHttpClientService>()!
                ));

                // Create main form instance
                frmLanding mainForm = new frmLanding();

                // UI-specific services that depend on the main form
                var userNotificationService = new WinFormsNotificationService(mainForm);
                var progressReporter = new WinFormsProgressReporter(mainForm);
                ServiceLocator.Register<IUserNotificationService>(userNotificationService);
                ServiceLocator.Register<IProgressReporter>(progressReporter);

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
