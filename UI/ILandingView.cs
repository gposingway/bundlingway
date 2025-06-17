using System.Collections.Generic;
using System.Threading.Tasks;
using Bundlingway.Model;

namespace Bundlingway.UI
{
    public interface ILandingView
    {
        // UI update methods
        Task SetAnnouncementAsync(string message);
        Task SetPackagesAsync(IEnumerable<ResourcePackage> packages);
        Task SetPackageOpsAvailableAsync(bool available);
        Task SetProgressAsync(long value, long max);
        Task ShowProgressAsync(long max);
        Task HideProgressAsync();
        Task SetReShadeStatusAsync(string status, bool enabled, bool visible, string buttonText);
        Task SetGPosingwayStatusAsync(string status, bool enabled, bool visible, string buttonText);
        Task SetGamePathAsync(string path);
        Task SetGameElementsEnabledAsync(bool enabled);
        Task SetUpdateButtonVisibleAsync(bool visible);
        Task SetUpdateButtonTextAsync(string text);
        Task SetDesktopShortcutStatusAsync(string status);
        Task SetBrowserIntegrationStatusAsync(string status);
        // ...add more as needed for UI updates

        // Method to get selected packages
        IEnumerable<ResourcePackage> GetSelectedPackages();
    }
}
