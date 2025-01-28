using Bundlingway;
using Bundlingway.Utilities;
using System.Diagnostics.Eventing.Reader;
using System.IO.Packaging;

using System.Threading;

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
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            Bootstrap.Initialize().Wait();

            Instances.LocalConfigProvider.Load();

            if (args.Length > 0)
            {
                var url = args[0];

                if (url.StartsWith("gwpreset://open/?"))
                {
                    url = url.Replace("gwpreset://open/?", "");

                    Utilities.Handler.Package.DownloadAndInstall(url).Wait();

                    // Notify other instances and close the client
                    ProcessHelper.NotifyOtherInstances(Constants.Events.PackageInstalled);

                    Environment.Exit(0);
                }
            }
            else
            {
                Application.Run(new Landing());
            }
        }




    }
}