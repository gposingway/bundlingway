using Bundlingway.Core.Interfaces;
using Bundlingway.Core.Services;
using Bundlingway.Model;
using Bundlingway.Utilities;
using Serilog;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Bundlingway.UI
{    public class LandingPresenter
    {
        private readonly ILandingView _view;
        private readonly PackageService _packageService;
        private readonly ReShadeService _reShadeService;
        private readonly GPosingwayService _gPosingwayService;
        private readonly IConfigurationService _configService;
        private readonly IAppEnvironmentService _envService;
        private readonly BundlingwayService _bundlingwayService;        private readonly IFileSystemService _fileSystem;
        private readonly IUserNotificationService _notificationService;
        private readonly IElevationService _elevationService;

        public LandingPresenter(ILandingView view, PackageService packageService, ReShadeService reShadeService, GPosingwayService gPosingwayService, IConfigurationService configService, IAppEnvironmentService envService, BundlingwayService bundlingwayService, IFileSystemService fileSystem, IUserNotificationService notificationService, IElevationService elevationService)
        {
            _view = view;
            _packageService = packageService;
            _reShadeService = reShadeService;
            _gPosingwayService = gPosingwayService;
            _configService = configService;            _envService = envService; _bundlingwayService = bundlingwayService;
            _fileSystem = fileSystem;
            _notificationService = notificationService;
            _elevationService = elevationService;
        }        public async Task InitializeAsync()
        {
            var c = _configService.Configuration;
            var gamePath = c.Game.InstallationFolder;
            
            // Check Bundlingway status (independent of game installation)
            await _bundlingwayService.CheckStatusAsync();
            
            if (!string.IsNullOrEmpty(gamePath))
            {
                await _reShadeService.CheckStatusAsync();
                await _gPosingwayService.CheckStatusAsync();
            }
            await UpdateElementsAsync();
            await PopulateGridAsync();
        }
        public async Task OnDetectSettingsAsync()
        {            // Perform real detection
            await Bootstrap.DetectSettings(_envService, _configService, _bundlingwayService, _gPosingwayService, _reShadeService, _notificationService);

            // After detection, check Bundlingway status (independent of game path)
            await _bundlingwayService.CheckStatusAsync();

            // After detection, fetch local/remote info if game path is present
            var c = _configService.Configuration;
            var gamePath = c.Game.InstallationFolder;
            if (!string.IsNullOrEmpty(gamePath))
            {
                await _reShadeService.CheckStatusAsync();
                await _gPosingwayService.CheckStatusAsync();
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
            await _view.SetGamePathAsync(gameIsInstalled ? (gamePath ?? string.Empty) : "Click [Detect] with the game running");

            if (!gameIsInstalled)
            {                await _view.SetReShadeStatusAsync("Waiting game detection...", false, false, "Install");
                await _view.SetGPosingwayStatusAsync("Waiting game detection...", false, false, "Install");
                await _view.SetUpdateButtonStateAsync(UpdateButtonState.Hidden);
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
            bool gPosingwayBtnEnabled = true;            string gPosingwayBtnText = c.GPosingway.Status == EPackageStatus.NotInstalled ? "Install" : c.GPosingway.Status == EPackageStatus.Outdated ? "Update" : "";
            await _view.SetGPosingwayStatusAsync(gPosingwayText, gPosingwayBtnEnabled, gPosingwayBtnVisible, gPosingwayBtnText);
            
            // Update button
            await _view.SetUpdateButtonStateAsync(
                c.Bundlingway.RemoteVersion != c.Bundlingway.LocalVersion 
                    ? UpdateButtonState.UpdateAvailable 
                    : UpdateButtonState.Hidden, 
                c.Bundlingway.RemoteVersion);
        }

        public async Task PopulateGridAsync()
        {
            var packages = await _packageService.GetAllPackagesAsync() ?? [];
            await _view.SetPackagesAsync(packages);
        }
        public async Task OnboardPackagesAsync(IEnumerable<string> files)
        {
            await _packageService.OnboardPackagesAsync(files);
            // Note: UI updates will be handled by PackagesUpdated events
        }

        public async Task OnInstallPackageAsync()
        {

            try
            {
                // Show file dialog and onboard selected packages
                var validExtensions = Bundlingway.Constants.InstallableExtensions;
                var filter = string.Join(";", validExtensions.Select(ext => $"*{ext}"));
                var ofd = new System.Windows.Forms.OpenFileDialog
                {
                    Filter = $"Archive files ({filter})|{filter}",
                    Title = "Select a Package File",
                    Multiselect = true
                }; if (ofd.ShowDialog() == DialogResult.OK)
                {
                    await _packageService.OnboardPackagesAsync(ofd.FileNames);
                    // Note: UI updates will be handled by PackagesUpdated events
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to onboard packages");
                throw;
            }

        }
        public async Task OnRemovePackagesAsync()
        {
            // Remove selected packages
            var selectedPackages = _view.GetSelectedPackages();
            await Task.WhenAll(selectedPackages.Select(_packageService.RemovePackageAsync));
            _notificationService.AnnounceAsync("All selected packages removed!");
            // Note: UI updates will be handled by PackagesUpdated events
        }
        public async Task OnUninstallPackagesAsync()
        {
            var selectedPackages = _view.GetSelectedPackages();
            await Task.WhenAll(selectedPackages.Select(_packageService.UninstallPackageAsync));
            _notificationService.AnnounceAsync("All selected packages uninstalled!");
            // Note: UI updates will be handled by PackagesUpdated events
        }

        public async Task OnInstallReShadeAsync()
        {
            await _reShadeService.UpdateAsync();
            await UpdateElementsAsync();
        }

        public async Task OnInstallGPosingwayAsync()
        {
            await _gPosingwayService.UpdateAsync(true);
            await UpdateElementsAsync();
            await PopulateGridAsync();
        }
        public async Task OnReinstallPackagesAsync()
        {
            var selectedPackages = _view.GetSelectedPackages();
            await Task.WhenAll(selectedPackages.Select(_packageService.ReinstallPackageAsync));
            _notificationService.AnnounceAsync("All selected packages reinstalled!");
            // Note: UI updates will be handled by PackagesUpdated events
        }
        public async Task OnToggleFavoriteAsync()
        {
            var selectedPackages = _view.GetSelectedPackages();
            await Task.WhenAll(selectedPackages.Select(_packageService.ToggleFavoriteAsync));
            // Note: UI updates will be handled by PackagesUpdated events
        }
        public async Task OnToggleLockedAsync()
        {
            var selectedPackages = _view.GetSelectedPackages();
            await Task.WhenAll(selectedPackages.Select(_packageService.ToggleLockedAsync));
            // Note: UI updates will be handled by PackagesUpdated events
        }
        public void OpenPackagesFolder()
        {
            string repositoryPath = _envService.PackageFolder;
            if (string.IsNullOrEmpty(repositoryPath) || !_fileSystem.DirectoryExists(repositoryPath))
            {
                System.Windows.Forms.MessageBox.Show("Package Folder not found or path is empty.", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }
            System.Diagnostics.Process.Start("explorer.exe", repositoryPath);
        }
        public void OpenGameFolder()
        {
            var config = _configService.Configuration;
            var gamePath = config?.Game?.InstallationFolder;
            if (string.IsNullOrEmpty(gamePath) || !_fileSystem.DirectoryExists(gamePath))
            {
                System.Windows.Forms.MessageBox.Show("Game Folder not found or path is empty.", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }
            System.Diagnostics.Process.Start("explorer.exe", gamePath);
        }
        public void OpenLogFile()
        {
            var logDirectory = _envService.BundlingwayDataFolder;
            if (string.IsNullOrEmpty(logDirectory) || !_fileSystem.DirectoryExists(logDirectory))
                return;
            var logFiles = _fileSystem.GetFiles(logDirectory, Bundlingway.Constants.Files.Log.Split('.')[0] + "*.txt");
            if (!logFiles.Any()) return;
            var latestLogFile = logFiles.OrderByDescending(f => _fileSystem.GetLastWriteTime(f)).First();
            if (_fileSystem.FileExists(latestLogFile))
            {
                System.Diagnostics.Process.Start("notepad.exe", latestLogFile);
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
        public void BackupData()
        {
            var target = Path.Combine(_envService.BundlingwayDataFolder, Bundlingway.Constants.Folders.Backup);
            if (string.IsNullOrEmpty(target))
            {
                _notificationService.AnnounceAsync("Backup path is invalid.");
                return;
            }
            if (!_fileSystem.DirectoryExists(target))
            {
                _fileSystem.CreateDirectory(target);
            }
            var source1 = Path.Combine(_envService.BundlingwayDataFolder, Bundlingway.Constants.Folders.Cache);
            if (_fileSystem.DirectoryExists(source1))
            {
                foreach (var file in _fileSystem.GetFiles(source1))
                {
                    var destFile = Path.Combine(target, Path.GetFileName(file));
                    _fileSystem.CopyFile(file, destFile, true);
                }
            }
            var source2 = Path.Combine(_envService.BundlingwayDataFolder, Bundlingway.Constants.Folders.Packages);
            if (_fileSystem.DirectoryExists(source2))
            {
                foreach (var folder in _fileSystem.GetDirectories(source2))
                {
                    var source = Path.Combine(folder, Bundlingway.Constants.Folders.SourcePackage);
                    if (Path.GetFileName(folder).Equals(Bundlingway.Constants.Folders.SinglePresets))
                    {
                        foreach (var acceptableFile in Bundlingway.Constants.AcceptableFilesInPresetFolder)
                            foreach (var file in _fileSystem.GetFiles(folder, acceptableFile))
                            {
                                var destFile = Path.Combine(target, Path.GetFileName(file));
                                _fileSystem.CopyFile(file, destFile, true);
                            }
                    }
                    else if (_fileSystem.DirectoryExists(source))
                    {
                        foreach (var file in _fileSystem.GetFiles(source))
                        {
                            var destFile = Path.Combine(target, Path.GetFileName(file));
                            _fileSystem.CopyFile(file, destFile, true);
                        }
                    }
                }
            }
            _notificationService.AnnounceAsync("Backup complete!");
            System.Diagnostics.Process.Start("explorer.exe", target);
        }

        public async Task ToggleTopMostAsync()
        {
            var c = _configService.Configuration;
            c.UI ??= new Bundlingway.Model.BundlingwayConfig.UIData();
            c.UI.TopMost = !c.UI.TopMost;
            await _configService.SaveAsync();
            // The view should update itself after this
        }        public async Task CreateDesktopShortcutAsync()
        {
            var success = await _elevationService.ExecuteElevatedAsync(
                "Create Desktop Shortcut",
                "create-desktop-shortcut",
                async () =>
                {
                    await ProcessHelper.PinToStartScreenAsync();
                    ProcessHelper.EnsureDesktopShortcut();
                },
                async () =>
                {
                    // Fallback: try without elevation
                    ProcessHelper.EnsureDesktopShortcut();
                }
            );

            if (success)
            {
                await _view.SetDesktopShortcutStatusAsync(ProcessHelper.CheckDesktopShortcutStatus());
            }
        }        public async Task SetBrowserIntegrationAsync()
        {
            var success = await _elevationService.ExecuteElevatedAsync(
                "Register Browser Integration",
                "register-browser-integration",
                async () =>
                {
                    await CustomProtocolHandler.RegisterCustomProtocolAsync(
                        Constants.GPosingwayProtocolHandler, 
                        "A collection of GPosingway-compatible ReShade resources", 
                        true);
                },
                async () =>
                {
                    // Fallback: inform user that manual registration might be needed
                    await _notificationService.AnnounceAsync("Browser integration requires administrator privileges. Some features may not work.");
                }
            );

            if (success)
            {
                await _view.SetBrowserIntegrationStatusAsync(
                    CustomProtocolHandler.IsCustomProtocolRegistered(Constants.GPosingwayProtocolHandler));
            }
        }

        public void OpenEmporiumAnnouncement()
        {
            _notificationService.AnnounceAsync("A Loporrit-approved selection of presets and shaders! Fluffy, fancy, and fantastic!");
        }
        public void OpenDebugAnnouncement()
        {
            _notificationService.AnnounceAsync("Oh dear, what have we here? A log full of secrets! (and probably some errors…)");
        }
        public void OpenAboutAnnouncement()
        {
            _notificationService.AnnounceAsync("About? About what? Oh! The project! Yes, yes, right this way!");
        }
        public void OpenPackagesFolderAnnouncement()
        {
            _notificationService.AnnounceAsync("Where all your precious presets and shaders live! Don't worry, they're well-fed.");
        }
        public void OpenGameFolderAnnouncement()
        {
            _notificationService.AnnounceAsync("Game files, game files everywhere! Tread carefully, adventurer!");
        }
        public void OpenFixItAnnouncement()
        {
            _notificationService.AnnounceAsync("Duplicated shaders? green tint everywhere? No worries, 'Fix It' is here to save the day!");
        }
        public void OpenInstallReShadeAnnouncement()
        {
            _notificationService.AnnounceAsync("You need to shut down the game client before you can update!");
        }

        // ... Add more methods for all business logic previously in frmLanding
    }
}
