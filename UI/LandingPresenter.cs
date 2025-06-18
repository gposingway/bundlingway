using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bundlingway.Core.Interfaces;
using Bundlingway.Core.Services;
using Bundlingway.Model;
using Bundlingway.Utilities;

namespace Bundlingway.UI
{
    public class LandingPresenter
    {
        private readonly ILandingView _view;
        private readonly IPackageService _packageService;
        private readonly ReShadeService _reShadeService;
        private readonly GPosingwayService _gPosingwayService;
        private readonly IConfigurationService _configService;
        private readonly IAppEnvironmentService _envService;

        public LandingPresenter(ILandingView view, IPackageService packageService, ReShadeService reShadeService, GPosingwayService gPosingwayService, IConfigurationService configService, IAppEnvironmentService envService)
        {
            _view = view;
            _packageService = packageService;
            _reShadeService = reShadeService;
            _gPosingwayService = gPosingwayService;
            _configService = configService;
            _envService = envService;
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
            await Bundlingway.Utilities.Bootstrap.DetectSettings(_envService);
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
            await _view.SetGamePathAsync(gameIsInstalled ? gamePath : "Click [Detect] with the game running");

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

        public async Task OnInstallPackageAsync()
        {
            // Show file dialog and onboard selected packages
            var validExtensions = Bundlingway.Constants.InstallableExtensions;
            var filter = string.Join(";", validExtensions.Select(ext => $"*{ext}"));
            var ofd = new System.Windows.Forms.OpenFileDialog
            {
                Filter = $"Archive files ({filter})|{filter}",
                Title = "Select a Package File",
                Multiselect = true
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                await _packageService.OnboardPackagesAsync(ofd.FileNames);
                await PopulateGridAsync();
            }
        }

        public async Task OnRemovePackagesAsync()
        {
            // Remove selected packages
            var selectedPackages = _view.GetSelectedPackages();
            foreach (var pkg in selectedPackages)
                await _packageService.RemovePackageAsync(pkg);
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

        public void OpenPackagesFolder()
        {
            string repositoryPath = _envService.PackageFolder;
            if (string.IsNullOrEmpty(repositoryPath) || !System.IO.Directory.Exists(repositoryPath))
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
            if (string.IsNullOrEmpty(gamePath) || !System.IO.Directory.Exists(gamePath))
            {
                System.Windows.Forms.MessageBox.Show("Game Folder not found or path is empty.", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }
            System.Diagnostics.Process.Start("explorer.exe", gamePath);
        }

        public void OpenLogFile()
        {
            var logDirectory = _envService.BundlingwayDataFolder;
            if (string.IsNullOrEmpty(logDirectory) || !System.IO.Directory.Exists(logDirectory))
                return;
            var logFiles = System.IO.Directory.GetFiles(logDirectory, Bundlingway.Constants.Files.Log.Split('.')[0] + "*.txt");
            if (logFiles.Length == 0) return;
            var latestLogFile = logFiles.OrderByDescending(f => new System.IO.FileInfo(f).LastWriteTime).First();
            if (System.IO.File.Exists(latestLogFile))
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
            var target = System.IO.Path.Combine(_envService.BundlingwayDataFolder, Bundlingway.Constants.Folders.Backup);
            if (string.IsNullOrEmpty(target))
            {
                ModernUI.Announce("Backup path is invalid.");
                return;
            }
            if (!System.IO.Directory.Exists(target))
            {
                System.IO.Directory.CreateDirectory(target);
            }
            var source1 = System.IO.Path.Combine(_envService.BundlingwayDataFolder, Bundlingway.Constants.Folders.Cache);
            if (System.IO.Directory.Exists(source1))
            {
                foreach (var file in System.IO.Directory.GetFiles(source1))
                {
                    var destFile = System.IO.Path.Combine(target, System.IO.Path.GetFileName(file));
                    System.IO.File.Copy(file, destFile, true);
                }
            }
            var source2 = System.IO.Path.Combine(_envService.BundlingwayDataFolder, Bundlingway.Constants.Folders.Packages);
            if (System.IO.Directory.Exists(source2))
            {
                foreach (var folder in System.IO.Directory.GetDirectories(source2))
                {
                    var source = System.IO.Path.Combine(folder, Bundlingway.Constants.Folders.SourcePackage);
                    if (System.IO.Path.GetFileName(folder).Equals(Bundlingway.Constants.Folders.SinglePresets))
                    {
                        foreach (var acceptableFile in Bundlingway.Constants.AcceptableFilesInPresetFolder)
                            foreach (var file in System.IO.Directory.GetFiles(folder, acceptableFile))
                            {
                                var destFile = System.IO.Path.Combine(target, System.IO.Path.GetFileName(file));
                                System.IO.File.Copy(file, destFile, true);
                            }
                    }
                    else if (System.IO.Directory.Exists(source))
                    {
                        foreach (var file in System.IO.Directory.GetFiles(source))
                        {
                            var destFile = System.IO.Path.Combine(target, System.IO.Path.GetFileName(file));
                            System.IO.File.Copy(file, destFile, true);
                        }
                    }
                }
            }
            ModernUI.Announce("Backup complete!");
            System.Diagnostics.Process.Start("explorer.exe", target);
        }

        public async Task ToggleTopMostAsync()
        {
            var c = _configService.Configuration;
            c.UI ??= new Bundlingway.Model.BundlingwayConfig.UIData();
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
            await CustomProtocolHandler.RegisterCustomProtocolAsync(Bundlingway.Constants.GPosingwayProtocolHandler, "A collection of GPosingway-compatible ReShade resources", true);
            await _view.SetBrowserIntegrationStatusAsync(CustomProtocolHandler.IsCustomProtocolRegistered(Bundlingway.Constants.GPosingwayProtocolHandler));
        }

        public void OpenEmporiumAnnouncement()
        {
            ModernUI.Announce("A Loporrit-approved selection of presets and shaders! Fluffy, fancy, and fantastic!");
        }
        public void OpenDebugAnnouncement()
        {
            ModernUI.Announce("Oh dear, what have we here? A log full of secrets! (and probably some errors…)");
        }
        public void OpenAboutAnnouncement()
        {
            ModernUI.Announce("About? About what? Oh! The project! Yes, yes, right this way!");
        }
        public void OpenPackagesFolderAnnouncement()
        {
            ModernUI.Announce("Where all your precious presets and shaders live! Don’t worry, they’re well-fed.");
        }
        public void OpenGameFolderAnnouncement()
        {
            ModernUI.Announce("Game files, game files everywhere! Tread carefully, adventurer!");
        }
        public void OpenFixItAnnouncement()
        {
            ModernUI.Announce("Duplicated shaders? green tint everywhere? No worries, 'Fix It' is here to save the day!");
        }
        public void OpenInstallReShadeAnnouncement()
        {
            ModernUI.Announce("You need to shut down the game client before you can update!");
        }

        // ... Add more methods for all business logic previously in frmLanding
    }
}
