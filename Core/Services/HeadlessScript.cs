using Bundlingway.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace Bundlingway.Core.Services
{
    /// <summary>
    /// Simple CLI automation script for headless package management.
    /// </summary>
    public static class HeadlessScript
    {
        public static async Task RunAsync(IServiceProvider provider, string[] args)
        {
            var notification = provider.GetService(typeof(IUserNotificationService)) as IUserNotificationService;
            var packageService = provider.GetService(typeof(PackageService)) as PackageService;
            if (notification == null || packageService == null)
            {
                Console.Error.WriteLine("Required services not available.");
                return;
            }

            if (args.Length < 2)
            {
                await notification.AnnounceAsync("Usage: --headless <command> [options]");
                await notification.AnnounceAsync("Commands: scan, install <file>, uninstall <id>");
                return;
            }

            switch (args[1].ToLower())
            {
                case "install":
                    if (args.Length < 3)
                    {
                        await notification.ShowErrorAsync("Missing file argument for install.");
                        return;
                    }
                    await notification.AnnounceAsync($"Installing package from {args[2]}...");
                    await packageService.OnboardPackageAsync(args[2]);
                    await notification.AnnounceAsync("Install complete.");
                    break;
                case "uninstall":
                    if (args.Length < 3)
                    {
                        await notification.ShowErrorAsync("Missing package ID for uninstall.");
                        return;
                    }
                    var pkg = await packageService.GetPackageByNameAsync(args[2]);
                    if (pkg == null)
                    {
                        await notification.ShowErrorAsync($"Package not found: {args[2]}");
                        return;
                    }
                    await notification.AnnounceAsync($"Uninstalling package {args[2]}...");
                    await packageService.UninstallPackageAsync(pkg);
                    await notification.AnnounceAsync("Uninstall complete.");
                    break;
                default:
                    await notification.ShowErrorAsync($"Unknown command: {args[1]}");
                    break;
            }
        }
    }
}
