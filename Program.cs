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
            Maintenance.PrepareEnvironmentAsync().Wait();

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            Bootstrap.Initialize().Wait();

            Instances.LocalConfigProvider.Load();

            if (args.Length > 0)
            {
                Utilities.Handler.CommandLineArgs.ProcessAsync(args).Wait();

                // Check if another instance of the application is already running
                if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
                {
                    // Notify other instances and close the client
                    ProcessHelper.NotifyOtherInstances(Constants.Events.PackageInstalled);
                    Process.GetCurrentProcess().Kill();
                }

            }

            Application.Run(new frmLanding());
        }
    }
}