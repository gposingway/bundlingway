#nullable disable
using Bundlingway.Core.Services;
using Bundlingway.Core.Interfaces;
using Bundlingway.Model;
using Bundlingway.UI;
using Bundlingway.UI.WinForms;
using Bundlingway.Utilities;
using Bundlingway.Utilities.Extensions;
using Serilog;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Bundlingway
{
    public partial class frmLanding : Form, ILandingView
    {
        private PackageService _packageService;
        private ReShadeService _reShadeService;
        private GPosingwayService _gPosingwayService;
        private IConfigurationService _configService;
        private IAppEnvironmentService _envService;
        private BundlingwayService _bundlingwayService;
        private LandingPresenter _presenter;
        private readonly IServiceProvider _serviceProvider;

        public frmLanding(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializeComponent();
        }
        public void InitializeServices(IServiceProvider serviceProvider)
        {
            _envService = serviceProvider.GetRequiredService<IAppEnvironmentService>();
            _packageService = serviceProvider.GetRequiredService<PackageService>();
            _reShadeService = serviceProvider.GetRequiredService<ReShadeService>();
            _gPosingwayService = serviceProvider.GetRequiredService<GPosingwayService>();
            _configService = serviceProvider.GetRequiredService<IConfigurationService>();
            _bundlingwayService = serviceProvider.GetRequiredService<BundlingwayService>();
            var fileSystemService = serviceProvider.GetRequiredService<IFileSystemService>();

            _presenter = new LandingPresenter(this, _packageService, _reShadeService, _gPosingwayService, _configService, _envService, _bundlingwayService, fileSystemService);
            Text = $"Bundlingway · v{_envService.AppVersion}";

            BindAllTaggedControls();

            _ = ModernUI.Announce(Constants.MessageCategory.ApplicationStart.ToString());

            txtDesktopShortcut.DoAction(() => txtDesktopShortcut.Text = ProcessHelper.CheckDesktopShortcutStatus());
            txtBrowserIntegration.DoAction(() => txtBrowserIntegration.Text = CustomProtocolHandler.IsCustomProtocolRegistered(Constants.GPosingwayProtocolHandler));

            ProcessHelper.NotificationReceived += ProcessHelper_NotificationReceived;
            _packageService.PackagesUpdated += PackageService_PackagesUpdated;

            _ = ProcessHelper.ListenForNotifications();
            _ = ModernUI.UpdateElements();
            _ = ModernUI.Announce(Constants.MessageCategory.Ready.ToString());
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _ = OnShownAsync(e).ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    Serilog.Log.Error(t.Exception, "Unhandled exception in OnShownAsync");
                }
            }, System.Threading.CancellationToken.None, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async Task OnShownAsync(EventArgs e)
        {
            await _presenter.InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await Bootstrap.DetectSettings(_envService, _configService, _bundlingwayService, _gPosingwayService, _reShadeService);
            await UpdateElements();
            await PopulateGridAsync();
        }

        private void BindAllTaggedControls(Control parent = null)
        {

            parent ??= this;

            foreach (Control control in parent.Controls)
            {
                if (control.Tag != null)
                {
                    control.MouseEnter += Generic_MouseEnter;
                }
                if (control.HasChildren)
                {
                    BindAllTaggedControls(control);
                }
            }
        }

        private void Generic_MouseEnter(object sender, EventArgs e)
        {
            _ = ModernUI.Announce(((Control)sender).Tag.ToString());
        }
        private void ProcessHelper_NotificationReceived(object sender, IPCNotification e)
        {
            switch (e.Topic)
            {
                case Constants.Events.PackageInstalling:
                case Constants.Events.PackageInstalled:
                    _ = ModernUI.Announce(e.Message);
                    break;
                case Constants.Events.DuplicatedInstances:
                    _ = ModernUI.BringToFront(); // If implemented, otherwise call the method directly
                    break;
            }
        }        private void PackageService_PackagesUpdated(object sender, PackageEventArgs e)
        {
            // Update only the specific packages that changed based on operation type
            this.DoAction(() =>
            {
                if (e.Packages != null && e.Packages.Any())
                {
                    // Determine operation type from message
                    if (!string.IsNullOrEmpty(e.Message) && e.Message.Contains("Removed"))
                    {
                        // Package was removed from catalog - remove from grid
                        _ = RemovePackagesFromGridAsync(e.Packages);
                    }
                    else
                    {
                        // Package was added/updated - update in grid
                        var shouldSelectAndScroll = !string.IsNullOrEmpty(e.Message) && e.Message.Contains("Installed");
                        _ = UpdatePackagesInGridAsync(e.Packages, shouldSelectAndScroll);
                    }
                }
            });
        }// Helper for safe async event handling
        private async void RunSafeAsync(Func<Task> asyncAction)
        {
            try
            {
                await asyncAction();
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Unhandled exception in async UI event handler.");
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDetectSettings_Click(object sender, EventArgs e)
        {
            RunSafeAsync(() => _presenter.OnDetectSettingsAsync());
        }

        private void btnInstallPackage_Click(object sender, EventArgs e)
        {
            RunSafeAsync(() => _presenter.OnInstallPackageAsync());
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            RunSafeAsync(() => _presenter.OnRemovePackagesAsync());
        }

        private void btnUninstall_Click(object sender, EventArgs e)
        {
            RunSafeAsync(() => _presenter.OnUninstallPackagesAsync());
        }

        private void btnInstallReShade_Click(object sender, EventArgs e)
        {
            RunSafeAsync(() => _presenter.OnInstallReShadeAsync());
        }

        private void btnInstallGPosingway_Click(object sender, EventArgs e)
        {
            RunSafeAsync(() => _presenter.OnInstallGPosingwayAsync());
        }

        private void btnReinstall_Click(object sender, EventArgs e)
        {
            RunSafeAsync(() => _presenter.OnReinstallPackagesAsync());
        }

        private void btnFavPackage_Click(object sender, EventArgs e)
        {
            RunSafeAsync(() => _presenter.OnToggleFavoriteAsync());
        }

        private void btnLockPackage_Click(object sender, EventArgs e)
        {
            RunSafeAsync(() => _presenter.OnToggleLockedAsync());
        }

        private async Task PopulateGridAsync()
        {
            Log.Information("frmLanding: PopulateGrid - Populating the grid with resource packages");
            var packages = await _packageService.GetAllPackagesAsync() ?? [];

            if (dgvPackages == null)
            {
                Log.Warning("frmLanding: PopulateGrid - dgvPackages is null. UI not initialized?");
                return;
            }

            List<SortOrder> sortOrders = new List<SortOrder>();
            List<string> sortColumnNames = new List<string>();
            List<object> selectedPackages = new List<object>(); // Replace 'object' with your actual package type

            dgvPackages?.DoAction(() =>
            {
                // 1. Store selected packages *before* clearing
                foreach (DataGridViewRow row in dgvPackages.SelectedRows)
                {
                    if (row.Tag != null)
                    {
                        selectedPackages.Add(((ResourcePackage)row.Tag).Name);
                    }
                }

                dgvPackages.Rows.Clear(); // Clear rows *after* saving selection

                foreach (DataGridViewColumn column in dgvPackages.Columns)
                {
                    sortOrders.Add(column.HeaderCell.SortGlyphDirection);
                    sortColumnNames.Add(column.Name);
                }
            });

            foreach (var package in packages)
            {
                var rowObj = new DataGridViewRow();

                var tagLine = "";
                if (package.Favorite) tagLine += "★";
                if (package.Locked) tagLine += "🔒";

                rowObj.CreateCells(dgvPackages,
                    tagLine,
                    package.Type.GetDescription(),
                    package.Label ?? package.Name,
                    package.Status.GetDescription()
                );
                rowObj.Tag = package;

                dgvPackages?.DoAction(() =>
                {
                    dgvPackages.Rows.Add(rowObj);
                    lblGrpPackages.Text = $"{dgvPackages.Rows.Count} Packages";
                });
            }

            dgvPackages?.DoAction(() =>
            {
                // 2. Re-select rows *after* repopulating
                foreach (DataGridViewRow row in dgvPackages.Rows)
                {
                    if (row.Tag != null && selectedPackages.Contains(((ResourcePackage)row.Tag).Name))
                    {
                        row.Selected = true;
                    }
                    else
                    {
                        row.Selected = false;
                    }
                }

                // 3. Sort the grid *after* re-selecting
                for (int i = 0; i < sortColumnNames.Count; i++)
                {
                    var columnName = sortColumnNames[i];
                    var sortOrder = sortOrders[i];

                    if (sortOrder != SortOrder.None)
                    {
                        DataGridViewColumn column = dgvPackages.Columns[columnName];
                        ListSortDirection listSortDirection = sortOrder == SortOrder.Ascending ?
                            ListSortDirection.Ascending :
                            ListSortDirection.Descending; dgvPackages.Sort(column, listSortDirection);
                        column.HeaderCell.SortGlyphDirection = sortOrder;
                        break; // Remove for multi-column sorting (and implement a DataGridViewColumnComparer)
                    }
                }
            });
        }        /// <summary>
                 /// Efficiently updates only the specified packages in the grid without full reload.
                 /// </summary>
                 /// <param name="packagesToUpdate">The packages to update in the grid</param>
        private async Task UpdatePackagesInGridAsync(IEnumerable<ResourcePackage> packagesToUpdate, bool selectAndScrollToFirst = false)
        {
            if (dgvPackages == null || packagesToUpdate == null)
            {
                Log.Warning("frmLanding: UpdatePackagesInGridAsync - dgvPackages is null or no packages to update");
                return;
            }

            var packageList = packagesToUpdate.ToList();
            if (!packageList.Any()) return;

            Log.Information($"frmLanding: UpdatePackagesInGridAsync - Updating {packageList.Count} package(s) in grid");

            dgvPackages?.DoAction(async () =>
            {
                bool needsVisibleRefresh = false;

                foreach (var updatedPackage in packageList)
                {
                    // Find existing row for this package
                    DataGridViewRow existingRow = null;
                    int rowIndex = -1;

                    for (int i = 0; i < dgvPackages.Rows.Count; i++)
                    {
                        var row = dgvPackages.Rows[i];
                        if (row.Tag is ResourcePackage pkg &&
                            pkg.Name.Equals(updatedPackage.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            existingRow = row;
                            rowIndex = i;
                            break;
                        }
                    }

                    // Create updated row data
                    var tagLine = "";
                    if (updatedPackage.Favorite) tagLine += "★";
                    if (updatedPackage.Locked) tagLine += "🔒";

                    if (existingRow != null)
                    {
                        // Update existing row
                        existingRow.Cells[0].Value = tagLine;
                        existingRow.Cells[1].Value = updatedPackage.Type.GetDescription();
                        existingRow.Cells[2].Value = updatedPackage.Label ?? updatedPackage.Name;
                        existingRow.Cells[3].Value = updatedPackage.Status.GetDescription();
                        existingRow.Tag = updatedPackage;

                        // Check if this row is visible
                        if (IsRowVisible(rowIndex))
                        {
                            needsVisibleRefresh = true;
                        }
                    }
                    else
                    {
                        // Add new row if package doesn't exist
                        var newRow = new DataGridViewRow();
                        newRow.CreateCells(dgvPackages,
                            tagLine,
                            updatedPackage.Type.GetDescription(),
                            updatedPackage.Label ?? updatedPackage.Name,
                            updatedPackage.Status.GetDescription()
                        );
                        newRow.Tag = updatedPackage;
                        dgvPackages.Rows.Add(newRow);

                        // New rows are typically at the end, check if visible
                        if (IsRowVisible(dgvPackages.Rows.Count - 1))
                        {
                            needsVisibleRefresh = true;
                        }
                    }
                }                // Only refresh if visible rows were updated
                if (needsVisibleRefresh)
                {
                    dgvPackages.Refresh();
                    dgvPackages.Invalidate();
                    await Task.Delay(1);
                }

                // Update package count label
                lblGrpPackages.Text = $"{dgvPackages.Rows.Count} Packages";
                
                // If requested, select and scroll to the first updated package
                if (selectAndScrollToFirst && packageList.Any())
                {
                    var firstPackage = packageList.First();
                    await SelectAndScrollToPackageAsync(firstPackage.Name);
                }
            });
        }/// <summary>
                 /// Removes packages from the grid that are no longer in the service's package list.
                 /// </summary>
                 /// <param name="packagesToRemove">The packages to remove from the grid</param>
        private async Task RemovePackagesFromGridAsync(IEnumerable<ResourcePackage> packagesToRemove)
        {
            if (dgvPackages == null || packagesToRemove == null)
            {
                return;
            }

            var packageList = packagesToRemove.ToList();
            if (!packageList.Any()) return;

            Log.Information($"frmLanding: RemovePackagesFromGridAsync - Removing {packageList.Count} package(s) from grid");

            dgvPackages?.DoAction(() =>
            {
                var rowsToRemove = new List<DataGridViewRow>();

                foreach (var packageToRemove in packageList)
                {
                    foreach (DataGridViewRow row in dgvPackages.Rows)
                    {
                        if (row.Tag is ResourcePackage pkg &&
                            pkg.Name.Equals(packageToRemove.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            rowsToRemove.Add(row);
                            break;
                        }
                    }
                }

                // Remove rows
                foreach (var row in rowsToRemove)
                {
                    dgvPackages.Rows.Remove(row);
                }

                // Update package count label
                lblGrpPackages.Text = $"{dgvPackages.Rows.Count} Packages";
            });
        }

        private void btnPackagesFolder_Click(object sender, EventArgs e)
        {
            _presenter.OpenPackagesFolder();
        }

        private void btnGameFolder_Click(object sender, EventArgs e)
        {
            _presenter.OpenGameFolder();
        }

        // Fix: Rename BringToFront to AvoidConflict
        internal async Task BringToFrontForm()
        {
            this.DoAction(() =>
            {
                if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
                Activate();
                Focus();
            });
        }

        internal async Task Announce(string message)
        {
            if (lblAnnouncement == null) return;

            lblAnnouncement?.DoAction(() =>
            {
                lblAnnouncement.Text = message;
                this.Refresh();
            });
        }

        private void btnDebug_Click(object sender, EventArgs e)
        {
            _presenter.OpenLogFile();
        }

        private void btnEmporium_Click(object sender, EventArgs e)
        {
            _presenter.OpenEmporium();
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            _presenter.OpenAbout();
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            _presenter.BackupData();
        }

        private async void btnTopMost_Click(object sender, EventArgs e)
        {
            await _presenter.ToggleTopMostAsync();
        }

        private async void btnCreateDesktopShortcut_Click(object sender, EventArgs e)
        {
            await _presenter.CreateDesktopShortcutAsync();
        }

        private async void btnSetDrowserIntegration_Click(object sender, EventArgs e)
        {
            await _presenter.SetBrowserIntegrationAsync();
        }

        private void btnEmporium_MouseEnter(object sender, EventArgs e)
        {
            _presenter.OpenEmporiumAnnouncement();
        }

        private void btnDebug_MouseEnter(object sender, EventArgs e)
        {
            _presenter.OpenDebugAnnouncement();
        }

        private void btnAbout_MouseEnter(object sender, EventArgs e)
        {
            _presenter.OpenAboutAnnouncement();
        }

        private void btnPackagesFolder_MouseEnter(object sender, EventArgs e)
        {
            _presenter.OpenPackagesFolderAnnouncement();
        }

        private void btnGameFolder_MouseEnter(object sender, EventArgs e)
        {
            _presenter.OpenGameFolderAnnouncement();
        }

        private void btnFixIt_MouseEnter(object sender, EventArgs e)
        {
            _presenter.OpenFixItAnnouncement();
        }

        private void btnInstallReShade_MouseEnter(object sender, EventArgs e)
        {
            _presenter.OpenInstallReShadeAnnouncement();
        }

        internal async Task UpdateElements()
        {

            var c = _configService.Configuration;

            var txtGamePathText = "";
            var mustDetect = false;

            //Now elements in general.

            var gameIsInstalled = !string.IsNullOrEmpty(c.Game.InstallationFolder);

            btnFixIt.DoAction(() =>
            {
                btnFixIt.Enabled = gameIsInstalled;
                btnGameFolder.Enabled = gameIsInstalled;
                pnlPackages.Enabled = gameIsInstalled;
                btnPackagesFolder.Enabled = gameIsInstalled;
                btnBackup.Enabled = gameIsInstalled;

                btnTopMost.ForeColor = c.UI.TopMost ? Color.White : Color.Gray;
                TopMost = c.UI.TopMost;
            });

            if (gameIsInstalled)
            {
                txtGamePathText = c.Game.InstallationFolder;
            }
            else
            {
                txtGamePathText = "Click [Detect] with the game running";
                mustDetect = true;
            }


            txtXivPath?.DoAction(() => { txtXivPath.Text = txtGamePathText; });

            if (mustDetect)
            {
                txtReShadeStatus?.DoAction(() =>
                {
                    txtReShadeStatus.Text = "Waiting...";
                    txtGPosingwayStatus.Text = "Waiting...";
                    btnInstallReShade.Visible = false;
                    btnInstallGPosingway.Visible = false;
                });

                return;
            }

            if (c.Bundlingway.RemoteVersion != c.Bundlingway.LocalVersion)
            {
                btnUpdate?.DoAction(() => { btnUpdate.Visible = true; });

                _ = ModernUI.Announce($"A new Bundlingway version ({c.Bundlingway.RemoteVersion}) is out!");
            }
            else
            {
                btnUpdate?.DoAction(() => { btnUpdate.Visible = false; });
            }

            var reShadeBtnEnabled = true;
            var reShadeBtnVisible = true;
            string reShadeBtnText = null;


            btnInstallReShade.Enabled = true;
            var reShadeText = c.ReShade.Status switch
            {
                EPackageStatus.Installed => "Installed",
                EPackageStatus.NotInstalled => "Not Installed",
                EPackageStatus.Outdated => "Outdated",
                _ => "Unknown"
            };

            if (c.ReShade.Status == EPackageStatus.Installed)
            {
                var latestReShadeLabel = c.ReShade.LocalVersion == c.ReShade.RemoteVersion ? " - Latest" : "";

                reShadeText += $" ({c.ReShade.LocalVersion}{latestReShadeLabel})";
            }

            if (c.ReShade.Status == EPackageStatus.Outdated)
            {
                reShadeText += $" (local: {c.ReShade.LocalVersion}, remote: {c.ReShade.RemoteVersion})";
                reShadeBtnText = "Update";
            }

            if (c.ReShade.Status == EPackageStatus.NotInstalled)
            {
                reShadeBtnText = "Install";
            }

            reShadeBtnVisible = (c.ReShade.Status == EPackageStatus.NotInstalled) || (c.ReShade.Status == EPackageStatus.Outdated);

            if (reShadeBtnVisible)
            {
                if (c.ReShade.Status != EPackageStatus.NotInstalled)
                {
                    reShadeBtnEnabled = !_envService.IsGameRunning;
                    _ = ModernUI.Announce("If you want to update ReShade, shut down the game first!");
                }
            }

            txtReShadeStatus?.DoAction(() =>
            {
                txtReShadeStatus.Text = reShadeText;
                btnInstallReShade.Enabled = reShadeBtnEnabled;
                btnInstallReShade.Visible = reShadeBtnVisible;
                btnInstallReShade.Text = reShadeBtnText;
            });

            var gPosingwayBtnEnabled = true;
            var gPosingwayBtnVisible = true;
            string gPosingwayBtnText = null;

            btnInstallGPosingway.Enabled = true;
            var GPosingwayText = c.GPosingway.Status switch
            {
                EPackageStatus.Installed => "Installed",
                EPackageStatus.NotInstalled => "Not Installed",
                EPackageStatus.Outdated => "Outdated",
                _ => "Unknown"
            };

            if (c.GPosingway.Status == EPackageStatus.Installed)
            {
                var latestGPosingwayLabel = c.GPosingway.LocalVersion == c.GPosingway.RemoteVersion ? " - Latest" : "";

                GPosingwayText += $" ({c.GPosingway.LocalVersion}{latestGPosingwayLabel})";
            }

            if (c.GPosingway.Status == EPackageStatus.Outdated)
            {
                GPosingwayText += $" (local: {c.GPosingway.LocalVersion}, remote: {c.GPosingway.RemoteVersion})";
                gPosingwayBtnText = "Update";
            }

            if (c.GPosingway.Status == EPackageStatus.NotInstalled)
            {
                gPosingwayBtnText = "Install";
            }

            gPosingwayBtnVisible = (c.GPosingway.Status == EPackageStatus.NotInstalled) || (c.GPosingway.Status == EPackageStatus.Outdated);

            txtGPosingwayStatus?.DoAction(() =>
            {
                txtGPosingwayStatus.Text = GPosingwayText;
                btnInstallGPosingway.Enabled = gPosingwayBtnEnabled;
                btnInstallGPosingway.Visible = gPosingwayBtnVisible;
                btnInstallGPosingway.Text = gPosingwayBtnText;
            });

        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            btnUpdate.Text = "Updating...";
            btnUpdate.Enabled = false;
            // _ = BundlingwayService.Update(btnUpdate); // Handler removed, use BundlingwayService.UpdateAsync if needed
        }

        private void btnShortcuts_Click(object sender, EventArgs e)
        {
            var shortcutsForm = _serviceProvider.GetRequiredService<frmShortcuts>();
            shortcutsForm.ShowDialog();
        }

        internal async Task StartProgress(long count)
        {
            await StartProgress(count, null);
        }

        internal async Task StartProgress(long count, string description)
        {
            prgCommon?.DoAction(() =>
            {
                prgCommon.Value = 0;
                prgCommon.Tag = count;
                prgCommon.Visible = true;
                // Set description for accessibility or tooltip if available
                if (!string.IsNullOrWhiteSpace(description))
                {
                    prgCommon.AccessibleDescription = description;
                }
            });
        }

        internal async Task SetProgress(long value)
        {
            // Fix: Null check before unboxing prgCommon.Tag
            var tagValue = prgCommon.Tag;
            var percentage = tagValue is long tagLong && tagLong > 0 ? (int)Math.Round((double)value / tagLong * 100) : 0;

            prgCommon?.DoAction(() =>
            {
                prgCommon.Value = percentage;
            });
        }

        internal async Task StopProgress()
        {
            prgCommon?.DoAction(() =>
            {
                prgCommon.Value = 0;
                prgCommon.Tag = 0;
                prgCommon.Visible = false;
            });
        }

        private void btnFixIt_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "This process can take a little while as Bundlingway analyses everything and fixes your setup, friend.\n(Don't worry, I'll backup everything in case you want to revert changes.)\n\nDo you want to re-download ReShade and GPosingway?\n(If not, cached copies will be used.)",
                "Lots of work ahead!",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning
            );

            switch (result)
            {
                case DialogResult.Yes:
                    DisableEverything();
                    // Handler removed, use BundlingwayService.FixItAsync(true) if implemented
                    EnableEverything();
                    break;
                case DialogResult.No:
                    DisableEverything();
                    // Handler removed, use BundlingwayService.FixItAsync(false) if implemented
                    EnableEverything();
                    break;
                default:
                    break;
            }
        }

        internal void DisableEverything()
        {
            flpSideMenu.DoAction(() =>
            {
                flpSideMenu.Enabled = false;
                flpPackageOptions.Enabled = false;
                pnlSettings.Enabled = false;
                pnlPackages.Enabled = false;
            });
        }

        internal void EnableEverything()
        {
            flpSideMenu.DoAction(() =>
            {
                flpSideMenu.Enabled = true;
                flpPackageOptions.Enabled = true;
                pnlSettings.Enabled = true;
                pnlPackages.Enabled = true;
            });
        }

        // Update context menu event handlers to use async
        private async void dvgPackagesContextMenu_Opening(object sender, CancelEventArgs e)
        {
            // Clear existing items
            dvgPackagesContextMenu.Items.Clear();

            // Check if we have any rows selected
            if (dgvPackages.SelectedRows.Count == 0)
            {
                e.Cancel = true;
                return;
            }

            // For single row selection
            if (dgvPackages.SelectedRows.Count == 1)
            {
                // Add single row options
                ToolStripMenuItem openFolderItem = new("Open Folder");
                openFolderItem.Click += async (s, args) => await OpenPackageFolder();
                dvgPackagesContextMenu.Items.Add(openFolderItem);

                ToolStripMenuItem renameItem = new("Rename");
                renameItem.Click += async (s, args) => await RenamePackage();
                dvgPackagesContextMenu.Items.Add(renameItem);

                dvgPackagesContextMenu.Items.Add(new ToolStripSeparator());
            }

            // Add common options for both single and multiple selection
            ToolStripMenuItem installItem = new("Install");
            installItem.Click += (s, args) => InstallSelectedPackages();
            dvgPackagesContextMenu.Items.Add(installItem);

            ToolStripMenuItem uninstallItem = new("Uninstall");
            uninstallItem.Click += (s, args) => UninstallSelectedPackages();
            dvgPackagesContextMenu.Items.Add(uninstallItem);

            ToolStripMenuItem removeItem = new("Remove");
            removeItem.Click += (s, args) => RemoveSelectedPackages();
            dvgPackagesContextMenu.Items.Add(removeItem);
        }

        // Helper method to open the folder of the selected package
        private async Task OpenPackageFolder()
        {
            if (dgvPackages.SelectedRows.Count != 1) return;

            var selectedRow = dgvPackages.SelectedRows[0];
            var package = selectedRow.Tag as ResourcePackage;

            if (package == null) return;

            try
            {
                string packagePath = package.LocalFolder;
                if (Directory.Exists(packagePath))
                {
                    Process.Start("explorer.exe", packagePath);
                }
                else
                {
                    await ModernUI.Announce("Package folder not found!");
                }
            }
            catch (Exception ex)
            {
                await ModernUI.Announce($"Could not open folder: {ex.Message}");
            }
        }

        // Helper method to rename the selected package
        private async Task RenamePackage()
        {
            if (dgvPackages.SelectedRows.Count != 1) return;

            var selectedRow = dgvPackages.SelectedRows[0];
            var package = selectedRow.Tag as ResourcePackage;

            if (package == null) return;

            string currentName = package.Label ?? package.Name;
            string newName = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter new package name:",
                "Rename Package",
                currentName);

            if (!string.IsNullOrWhiteSpace(newName) && newName != currentName)
            {
                try
                {
                    package.Label = newName;
                    // Update UI
                    _ = PopulateGridAsync();
                    await ModernUI.Announce($"Package renamed to \"{newName}\"");
                }
                catch (Exception ex)
                {
                    await ModernUI.Announce($"Could not rename package: {ex.Message}");
                }
            }
        }

        // Helper method to install the selected packages
        private void InstallSelectedPackages()
        {
            // Reuse functionality from btnReinstall_Click
            btnReinstall_Click(this, EventArgs.Empty);
        }

        // Helper method to uninstall the selected packages
        private void UninstallSelectedPackages()
        {
            // Reuse functionality from btnUninstall_Click
            btnUninstall_Click(this, EventArgs.Empty);
        }

        // Helper method to remove the selected packages
        private void RemoveSelectedPackages()
        {
            // Reuse functionality from btnRemove_Click
            btnRemove_Click(this, EventArgs.Empty);
        }

        public IEnumerable<ResourcePackage> GetSelectedPackages()
        {
            if (dgvPackages == null) yield break;
            foreach (DataGridViewRow row in dgvPackages.SelectedRows)
            {
                if (row.Tag is ResourcePackage pkg)
                    yield return pkg;
            }
        }

        public async Task SetAnnouncementAsync(string message)
        {
            await Announce(message);
        }

        public async Task SetPackagesAsync(IEnumerable<ResourcePackage> packages)
        {
            if (dgvPackages == null) return;
            dgvPackages.Rows.Clear();
            foreach (var package in packages)
            {
                var rowObj = new DataGridViewRow();
                var tagLine = "";
                if (package.Favorite) tagLine += "★";
                if (package.Locked) tagLine += "🔒";
                rowObj.CreateCells(dgvPackages,
                    tagLine,
                    package.Type.GetDescription(),
                    package.Label ?? package.Name,
                    package.Status.GetDescription()
                );
                rowObj.Tag = package;
                dgvPackages.Rows.Add(rowObj);
            }
            lblGrpPackages.Text = $"{dgvPackages.Rows.Count} Packages";
        }

        public async Task SetPackageOpsAvailableAsync(bool available)
        {
            btnInstallPackage?.DoAction(() =>
            {
                btnInstallPackage.Enabled = available;
                btnRemove.Enabled = available;
                btnUninstall.Enabled = available;
                btnReinstall.Enabled = available;
            });
        }

        public async Task SetProgressAsync(long value, long max)
        {
            await SetProgress(value);
        }

        public async Task ShowProgressAsync(long max)
        {
            await StartProgress(max);
        }

        public async Task HideProgressAsync()
        {
            await StopProgress();
        }

        public async Task SetReShadeStatusAsync(string status, bool enabled, bool visible, string buttonText)
        {
            this.DoAction(() =>
            {
                txtReShadeStatus.Text = status;
                btnInstallReShade.Enabled = enabled;
                btnInstallReShade.Visible = visible;
                btnInstallReShade.Text = buttonText;
            });
        }

        public async Task SetGPosingwayStatusAsync(string status, bool enabled, bool visible, string buttonText)
        {
            this.DoAction(() =>
            {
                txtGPosingwayStatus.Text = status;
                btnInstallGPosingway.Enabled = enabled;
                btnInstallGPosingway.Visible = visible;
                btnInstallGPosingway.Text = buttonText;
            });
        }

        public async Task SetGamePathAsync(string path)
        {
            txtXivPath.Text = path;
        }

        public async Task SetGameElementsEnabledAsync(bool enabled)
        {
            btnFixIt.Enabled = enabled;
            btnGameFolder.Enabled = enabled;
            pnlPackages.Enabled = enabled;
            btnPackagesFolder.Enabled = enabled;
            btnBackup.Enabled = enabled;
        }

        public async Task SetUpdateButtonVisibleAsync(bool visible)
        {
            btnUpdate.Visible = visible;
        }

        public async Task SetUpdateButtonTextAsync(string text)
        {
            btnUpdate.Text = text;
        }

        public async Task SetDesktopShortcutStatusAsync(string status)
        {
            txtDesktopShortcut.Text = status;
        }

        public async Task SetBrowserIntegrationStatusAsync(string status)
        {
            txtBrowserIntegration.Text = status;
        }

        /// <summary>
        /// Checks if a row at the given index is currently visible in the DataGridView viewport.
        /// </summary>
        /// <param name="rowIndex">The row index to check</param>
        /// <returns>True if the row is visible, false otherwise</returns>
        private bool IsRowVisible(int rowIndex)
        {
            if (dgvPackages == null || rowIndex < 0 || rowIndex >= dgvPackages.Rows.Count)
                return false;

            try
            {
                int firstVisible = dgvPackages.FirstDisplayedScrollingRowIndex;
                if (firstVisible < 0) return false;

                int displayedCount = dgvPackages.DisplayedRowCount(false);
                int lastVisible = firstVisible + displayedCount - 1;

                return rowIndex >= firstVisible && rowIndex <= lastVisible;
            }
            catch
            {
                // If there's any exception getting visibility info, assume it's visible to be safe
                return true;
            }
        }        /// <summary>
        /// Selects and scrolls to a specific package in the grid by name.
        /// </summary>
        /// <param name="packageName">The name of the package to select and scroll to</param>
        private async Task SelectAndScrollToPackageAsync(string packageName)
        {
            if (dgvPackages == null || string.IsNullOrWhiteSpace(packageName))
                return;

            dgvPackages.DoAction(() =>
            {
                try
                {
                    // Find the row with the matching package
                    DataGridViewRow targetRow = null;
                    int targetIndex = -1;

                    for (int i = 0; i < dgvPackages.Rows.Count; i++)
                    {
                        var row = dgvPackages.Rows[i];
                        if (row.Tag is ResourcePackage pkg &&
                            pkg.Name.Equals(packageName, StringComparison.OrdinalIgnoreCase))
                        {
                            targetRow = row;
                            targetIndex = i;
                            break;
                        }
                    }

                    if (targetRow != null)
                    {
                        // Clear current selection
                        dgvPackages.ClearSelection();
                        
                        // Select the target row
                        targetRow.Selected = true;
                        dgvPackages.CurrentCell = targetRow.Cells[0];
                        
                        // Scroll to make the row visible
                        dgvPackages.FirstDisplayedScrollingRowIndex = Math.Max(0, targetIndex - 2);
                        
                        // Bring the main window to front to ensure visibility
                        if (WindowState == FormWindowState.Minimized)
                            WindowState = FormWindowState.Normal;
                        BringToFront();
                        Activate();
                        
                        Log.Information("Selected and scrolled to package: {PackageName} at row {RowIndex}", packageName, targetIndex);
                    }
                    else
                    {
                        Log.Warning("Package not found in grid for selection: {PackageName}", packageName);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to select and scroll to package: {PackageName}", packageName);
                }
            });
        }
    }
}
