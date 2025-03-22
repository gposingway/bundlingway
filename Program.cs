using Bundlingway.Utilities;
using System.Diagnostics;

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
            // Prepare the environment (e.g., create necessary directories)
            Maintenance.PrepareEnvironmentAsync().Wait();

            // Initialize application configuration
            ApplicationConfiguration.Initialize();

            // Initialize the application (e.g., load settings)
            Bootstrap.Initialize().Wait();
            Instances.LocalConfigProvider.Load();
            Maintenance.EnsureConfiguration().Wait();

            // Check if command-line arguments are provided (headless mode)
            if (args.Length > 0)
            {
                // Process command-line arguments
                var result = Utilities.Handler.CommandLineArgs.ProcessAsync(args).Result;

                // Check if another instance of the application is already running
                if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
                {
                    // Notify other instances and close the client
                    ProcessHelper.NotifyOtherInstances(new Model.IPCNotification() { Topic = Constants.Events.PackageInstalled, Message = result });
                    Process.GetCurrentProcess().Kill();
                }
            }

            // Check for duplicate instances (UI mode)
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                // Notify other instances and close the client
                ProcessHelper.NotifyOtherInstances(new Model.IPCNotification() { Topic = Constants.Events.DuplicatedInstances });
                Process.GetCurrentProcess().Kill();
            }

            // Run the application in UI mode
            Application.Run(new frmLanding());
        }
    }
}
