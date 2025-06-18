using Bundlingway.Core.Services;
using Bundlingway.Model;
using Bundlingway.Core.Interfaces;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Bundlingway.Core.Constants;
using Bundlingway.Core.Utilities;
using Bundlingway.Utilities;

namespace Bundlingway.Core.UI
{
    public class LandingPresenter
    {
        private readonly ILandingView _view;
        private readonly PackageService _packageService;
        private readonly ReShadeService _reShadeService;
        private readonly GPosingwayService _gPosingwayService;
        private readonly IConfigurationService _configService;
        private readonly IAppEnvironmentService _envService;
        private readonly BundlingwayService _bundlingwayService;
        private readonly IUserNotificationService _notificationService;
        private readonly IBackupService _backupService;
        private readonly ISystemService _systemService;

        public LandingPresenter(
            ILandingView view,
            PackageService packageService,
            ReShadeService reShadeService,
            GPosingwayService gPosingwayService,
            IConfigurationService configService,
            IAppEnvironmentService envService,
            BundlingwayService bundlingwayService,
            IUserNotificationService notificationService,
            IBackupService backupService,
            ISystemService systemService
        )
        {
            _view = view;
            _packageService = packageService;
            _reShadeService = reShadeService;
            _gPosingwayService = gPosingwayService;
            _configService = configService;
            _envService = envService;
            _bundlingwayService = bundlingwayService;
            _notificationService = notificationService;
            _backupService = backupService;
            _systemService = systemService;
        }

        public async Task InitializeAsync()
        {
            var c = _configService.Configuration;
            var gamePath = c.Game.InstallationFolder;
            if (!string.IsNullOrEmpty(gamePath))
            {
                await _reShadeService.GetLocalInfoAsync();
                await _reShadeService.GetRemoteInfoAsync();
                await _gPosingwayService.GetLocalInfoAsync();
                await _gPosingwayService.GetRemoteInfoAsync();
            }
            await UpdateElementsAsync();
            await PopulateGridAsync();
        }

        public async Task OnDetectSettingsAsync()
        {
            // Perform real detection
            await Bundlingway.Utilities.Bootstrap.DetectSettings(_envService, _configService, _bundlingwayService, _gPosingwayService, _reShadeService, _notificationService);
            // After detection, fetch local/remote info if game path is present
            var c = _configService.Configuration;
            var gamePath = c.Game.InstallationFolder;
            if (!string.IsNullOrEmpty(gamePath))
            {
                await _reShadeService.GetLocalInfoAsync();
                await _reShadeService.GetRemoteInfoAsync();
                await _gPosingwayService.GetLocalInfoAsync();
                await _gPosingwayService.GetRemoteInfoAsync();
            }
            // Persist any newly detected values
            await _configService.SaveAsync();
            await UpdateElementsAsync();
            await PopulateGridAsync();
        }

        public async Task UpdateElementsAsync()
        {
            var c = _configService.Configuration;
            var gamePath = c.Game.InstallationFolder;
            var gameIsInstalled = !string.IsNullOrEmpty(gamePath);
            await _view.SetGameElementsEnabledAsync(gameIsInstalled);
            await _view.SetGamePathAsync(gameIsInstalled ? gamePath ?? string.Empty : "Click [Detect] with the game running");

            if (!gameIsInstalled)
            {
                await _view.SetReShadeStatusAsync("Waiting game detection...", false, false, "Install");
                await _view.SetGPosingwayStatusAsync("Waiting game detection...", false, false, "Install");
                await _view.SetUpdateButtonVisibleAsync(false);
                return;
            }

            // ReShade status
            string reShadeText = c.ReShade.Status switch
            {
                EPackageStatus.Installed => $"Installed ({c.ReShade.LocalVersion}{(c.ReShade.LocalVersion == c.ReShade.RemoteVersion ? " - Latest" : "")})",
                EPackageStatus.NotInstalled => "Not Installed",
                EPackageStatus.Outdated => $"Outdated (local: {c.ReShade.LocalVersion}, remote: {c.ReShade.RemoteVersion})",
                _ => "Unknown"
            };
            bool reShadeBtnVisible = c.ReShade.Status == EPackageStatus.NotInstalled || c.ReShade.Status == EPackageStatus.Outdated;
            bool reShadeBtnEnabled = c.ReShade.Status != EPackageStatus.NotInstalled ? !_envService.IsGameRunning : true;
            string reShadeBtnText = c.ReShade.Status == EPackageStatus.NotInstalled ? "Install" : c.ReShade.Status == EPackageStatus.Outdated ? "Update" : "";
            await _view.SetReShadeStatusAsync(reShadeText, reShadeBtnEnabled, reShadeBtnVisible, reShadeBtnText);
            // GPosingway status
            string gPosingwayText = c.GPosingway.Status switch
            {
                EPackageStatus.Installed => $"Installed ({c.GPosingway.LocalVersion}{(c.GPosingway.LocalVersion == c.GPosingway.RemoteVersion ? " - Latest" : "")})",
                EPackageStatus.NotInstalled => "Not Installed",
                EPackageStatus.Outdated => $"Outdated (local: {c.GPosingway.LocalVersion}, remote: {c.GPosingway.RemoteVersion})",
                _ => "Unknown"
            };
            bool gPosingwayBtnVisible = c.GPosingway.Status == EPackageStatus.NotInstalled || c.GPosingway.Status == EPackageStatus.Outdated;
            bool gPosingwayBtnEnabled = true;
            string gPosingwayBtnText = c.GPosingway.Status == EPackageStatus.NotInstalled ? "Install" : c.GPosingway.Status == EPackageStatus.Outdated ? "Update" : "";
            await _view.SetGPosingwayStatusAsync(gPosingwayText, gPosingwayBtnEnabled, gPosingwayBtnVisible, gPosingwayBtnText);
            // Update button
            await _view.SetUpdateButtonVisibleAsync(c.Bundlingway.RemoteVersion != c.Bundlingway.LocalVersion);
            await _view.SetUpdateButtonTextAsync("Update");
        }

        public async Task PopulateGridAsync()
        {
            await _packageService.ScanPackagesAsync();
            var packages = await _packageService.GetAllPackagesAsync() ?? Enumerable.Empty<ResourcePackage>();
            await _view.SetPackagesAsync(packages);
        }

        public async Task OnboardPackagesAsync(IEnumerable<string> files)
        {
            await _packageService.OnboardPackagesAsync(files);
            await PopulateGridAsync();
        }

        public async Task OnInstallPackageAsync(IEnumerable<string> fileNames)
        {
            // fileNames should be provided by the UI layer (e.g., from OpenFileDialog in WinForms)
            if (fileNames == null || !fileNames.Any())
                return;
            await _packageService.OnboardPackagesAsync(fileNames);
            await PopulateGridAsync();
        }

        public async Task OnRemovePackagesAsync()
        {
            var selectedPackages = _view.GetSelectedPackages();
            foreach (var pkg in selectedPackages)
            {
                try
                {
                    await _packageService.RemovePackageAsync(pkg);
                }
                catch (Exception ex)
                {
                    await _notificationService.AnnounceAsync($"Failed to remove package {pkg.Name}: {ex.Message}");
                }
            }
            await PopulateGridAsync();
        }

        public async Task OnUninstallPackagesAsync()
        {
            var selectedPackages = _view.GetSelectedPackages();
            foreach (var pkg in selectedPackages)
                await _packageService.UninstallPackageAsync(pkg);
            await PopulateGridAsync();
        }

        public async Task OnInstallReShadeAsync()
        {
            await _reShadeService.UpdateAsync();
            await UpdateElementsAsync();
        }

        public async Task OnInstallGPosingwayAsync()
        {
            await _gPosingwayService.UpdateAsync();
            await UpdateElementsAsync();
            await PopulateGridAsync();
        }

        public async Task OnReinstallPackagesAsync()
        {
            var selectedPackages = _view.GetSelectedPackages();
            foreach (var pkg in selectedPackages)
                await _packageService.ReinstallPackageAsync(pkg);
            await PopulateGridAsync();
        }

        public async Task OnToggleFavoriteAsync()
        {
            var selectedPackages = _view.GetSelectedPackages();
            foreach (var pkg in selectedPackages)
                await _packageService.ToggleFavoriteAsync(pkg);
            await PopulateGridAsync();
        }

        public async Task OnToggleLockedAsync()
        {
            var selectedPackages = _view.GetSelectedPackages();
            foreach (var pkg in selectedPackages)
                await _packageService.ToggleLockedAsync(pkg);
            await PopulateGridAsync();
        }

        public async Task OpenPackagesFolder(string repositoryPath)
        {
            if (string.IsNullOrEmpty(repositoryPath) || !Directory.Exists(repositoryPath))
            {
                // UI should handle error notification
                return;
            }
            await _systemService.OpenInExplorerAsync(repositoryPath);
        }

        public async Task OpenGameFolder(string gamePath)
        {
            if (string.IsNullOrEmpty(gamePath) || !Directory.Exists(gamePath))
            {
                // UI should handle error notification
                return;
            }
            await _systemService.OpenInExplorerAsync(gamePath);
        }

        public async Task OpenLogFile()
        {
            var logDirectory = _envService.BundlingwayDataFolder;
            if (string.IsNullOrEmpty(logDirectory) || !Directory.Exists(logDirectory))
                return;
            var logFiles = Directory.GetFiles(logDirectory, Files.Log.Split('.')[0] + "*.txt");
            if (logFiles.Length == 0) return;
            var latestLogFile = logFiles.OrderByDescending(f => new FileInfo(f).LastWriteTime).First();
            if (File.Exists(latestLogFile))
            {
                await _systemService.OpenInNotepadAsync(latestLogFile);
            }
        }

        public void OpenEmporium()
        {
            ProcessHelper.OpenUrlInBrowser("https://gposingway.github.io/bundlingways-emporium");
        }

        public void OpenAbout()
        {
            ProcessHelper.OpenUrlInBrowser("https://github.com/gposingway/bundlingway");
        }

        public async Task BackupData()
        {
            await _backupService.BackupDataAsync();
        }

        public async Task ToggleTopMostAsync()
        {
            var c = _configService.Configuration;
            c.UI ??= new BundlingwayConfig.UIData();
            c.UI.TopMost = !c.UI.TopMost;
            await _configService.SaveAsync();
            // The view should update itself after this
        }

        public async Task CreateDesktopShortcutAsync()
        {
            await ProcessHelper.PinToStartScreenAsync();
            ProcessHelper.EnsureDesktopShortcut();
            await _view.SetDesktopShortcutStatusAsync(ProcessHelper.CheckDesktopShortcutStatus());
        }

        public async Task SetBrowserIntegrationAsync()
        {
            await CustomProtocolHandler.RegisterCustomProtocolAsync(GPosingwayProtocolHandler, "A collection of GPosingway-compatible ReShade resources", true);
            await _view.SetBrowserIntegrationStatusAsync(CustomProtocolHandler.IsCustomProtocolRegistered(GPosingwayProtocolHandler));
        }

        public async Task OpenEmporiumAnnouncement()
        {
            await _notificationService.AnnounceAsync("A Loporrit-approved selection of presets and shaders! Fluffy, fancy, and fantastic!");
        }
        public async Task OpenDebugAnnouncement()
        {
            await _notificationService.AnnounceAsync("Oh dear, what have we here? A log full of secrets! (and probably some errors…)");
        }
        public async Task OpenAboutAnnouncement()
        {
            await _notificationService.AnnounceAsync("About? About what? Oh! The project! Yes, yes, right this way!");
        }
        public async Task OpenPackagesFolderAnnouncement()
        {
            await _notificationService.AnnounceAsync("Where all your precious presets and shaders live! Don’t worry, they’re well-fed.");
        }
        public async Task OpenGameFolderAnnouncement()
        {
            await _notificationService.AnnounceAsync("Game files, game files everywhere! Tread carefully, adventurer!");
        }
        public async Task OpenFixItAnnouncement()
        {
            await _notificationService.AnnounceAsync("Duplicated shaders? green tint everywhere? No worries, 'Fix It' is here to save the day!");
        }
        public async Task OpenInstallReShadeAnnouncement()
        {
            await _notificationService.AnnounceAsync("You need to shut down the game client before you can update!");
        }

        // ... Add more methods for all business logic previously in frmLanding
    }
}
