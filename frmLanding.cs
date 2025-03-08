using Bundlingway.Model;
using Bundlingway.Utilities;
using Bundlingway.Utilities.Extensions;
using Bundlingway.Utilities.Handler;
using Serilog;
using System.ComponentModel;

namespace Bundlingway
{
    public partial class frmLanding : Form
    {

        public frmLanding()
        {
            UI._landing = this;
            InitializeComponent();

            Text = $"Bundlingway · v{Instances.AppVersion}";

            BindAllTaggedControls();

            _ = UI.Announce(Constants.MessageCategory.ApplicationStart);

            _ = Bootstrap.DetectSettings();

            PopulateGrid();

            ProcessHelper.NotificationReceived += ProcessHelper_NotificationReceived;
            _ = ProcessHelper.ListenForNotifications();

            _ = UI.Announce(Constants.MessageCategory.Ready);
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
            _ = UI.Announce(((Control)sender).Tag.ToString());
        }

        private void ProcessHelper_NotificationReceived(object? sender, IPCNotification e)
        {
            if (e.Topic == Constants.Events.PackageInstalled)
            {
                PopulateGrid();
                _ = UI.Announce(e.Message);
            }
        }


        private void btnDetectSettings_Click(object sender, EventArgs e)
        {
            _ = Bootstrap.DetectSettings().ContinueWith(async a => { await UpdateElements(); });
        }

        private void btnInstallPackage_Click(object sender, EventArgs e)
        {
            _ = UI.Announce(Constants.MessageCategory.BeforeAddPackageSelection);

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                var filter = string.Join(";", Constants.InstallableExtensions.Select(ext => $"*{ext}"));
                openFileDialog.Filter = $"Archive files ({filter})|{filter}";
                openFileDialog.Title = "Select a Package File";
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    SetPackageOpsAvailable(false);

                    var validExtensions = new HashSet<string>(Constants.InstallableExtensions);
                    var selectedFiles = openFileDialog.FileNames
                        .Where(file => validExtensions.Contains(Path.GetExtension(file).ToLower()))
                        .ToList();

                    Package.Onboard(selectedFiles).ContinueWith(a =>
                    {
                        Maintenance.RemoveTempDir();
                        Instances.LocalConfigProvider.Save();
                        PopulateGrid();
                        SetPackageOpsAvailable(true);

                        _ = UI.Announce(Constants.MessageCategory.Finished);
                    });
                }
                else
                {
                    _ = UI.Announce(Constants.MessageCategory.AddPackageSelectionCancelled);
                }
            }
        }

        private void SetPackageOpsAvailable(bool v)
        {
            btnInstallPackage?.DoAction(() =>
            {
                btnInstallPackage.Enabled = v;
                btnRemove.Enabled = v;
                btnUninstall.Enabled = v;
                btnReinstall.Enabled = v;
            });
        }

        private void PopulateGrid()
        {
            Log.Information("frmLanding: PopulateGrid - Populating the grid with resource packages");
            Package.Scan().Wait();

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

            foreach (var package in Instances.ResourcePackages)
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
                    if (row.Tag != null && selectedPackages.Contains(((ResourcePackage)row.Tag).Name)) // Or use custom comparison if needed (see previous response)
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
                            ListSortDirection.Descending;

                        dgvPackages.Sort(column, listSortDirection);
                        column.HeaderCell.SortGlyphDirection = sortOrder;
                        break; // Remove for multi-column sorting (and implement a DataGridViewColumnComparer)
                    }
                }
            });
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            SetPackageOpsAvailable(false);

            var selectedPackages = dgvPackages.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(row => (ResourcePackage)row.Tag)
                .ToList();

            Task.WhenAll(selectedPackages.Select(package => Task.Run(() => Package.Remove(package)))).ContinueWith(t =>
            {
                _ = UI.Announce(Constants.MessageCategory.UninstallPackage);
                PopulateGrid();
                SetPackageOpsAvailable(true);
            });

        }

        private void btnUninstall_Click(object sender, EventArgs e)
        {
            SetPackageOpsAvailable(false);

            var selectedPackages = dgvPackages.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(row => (ResourcePackage)row.Tag)
                .ToList();

            Task.WhenAll(selectedPackages.Select(package => Task.Run(() => Package.Uninstall(package)))).ContinueWith(t =>
            {
                Maintenance.RemoveTempDir();
                _ = UI.Announce(Constants.MessageCategory.UninstallPackage);
                SetPackageOpsAvailable(true);
                PopulateGrid();
            });
        }

        private void btnInstallReShade_Click(object sender, EventArgs e)
        {

            btnInstallReShade.Enabled = false;

            ReShade.Update().ContinueWith(a =>
            {
                Bootstrap.DetectSettings().ContinueWith(async b =>
                {
                    Maintenance.RemoveTempDir();
                });
            });
        }

        private void btnInstallGPosingway_Click(object sender, EventArgs e)
        {

            btnInstallGPosingway.Enabled = false;

            GPosingway.Update().ContinueWith(a =>
            {
                Bootstrap.DetectSettings().ContinueWith(b =>
                {
                    Maintenance.RemoveTempDir();
                    PopulateGrid();
                });
            });
        }

        private void btnPackagesFolder_Click(object sender, EventArgs e)
        {

            string repositoryPath = Instances.PackageFolder;
            if (Directory.Exists(repositoryPath))
            {
                System.Diagnostics.Process.Start("explorer.exe", repositoryPath);
            }
            else
            {
                MessageBox.Show("Package Folder not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGameFolder_Click(object sender, EventArgs e)
        {
            // Open Game Folder in a new explorer window

            string gamePath = Instances.LocalConfigProvider.Configuration.Game.InstallationFolder;
            if (Directory.Exists(gamePath))
            {
                System.Diagnostics.Process.Start("explorer.exe", gamePath);
            }
            else
            {
                MessageBox.Show("Game Folder not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        internal async Task Announce(string message)
        {
            if (lblAnnouncement == null) return;

            lblAnnouncement?.DoAction(() => { lblAnnouncement.Text = message; });
        }

        private void btnDebug_Click(object sender, EventArgs e)
        {
            var logDirectory = Instances.BundlingwayDataFolder;
            var logFiles = Directory.GetFiles(logDirectory, Constants.Files.Log.Split('.')[0] + "*.txt");

            if (logFiles.Length == 0)
            {
                return;
            }

            var latestLogFile = logFiles.OrderByDescending(f => new FileInfo(f).LastWriteTime).First();

            if (File.Exists(latestLogFile))
            {
                System.Diagnostics.Process.Start("notepad.exe", latestLogFile);
            }
        }

        private void btnEmporium_Click(object sender, EventArgs e)
        {
            ProcessHelper.OpenUrlInBrowser("https://gposingway.github.io/bundlingways-emporium");
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            ProcessHelper.OpenUrlInBrowser("https://github.com/gposingway/bundlingway");
        }

        private void btnEmporium_MouseEnter(object sender, EventArgs e)
        {
            _ = UI.Announce("A Loporrit-approved selection of presets and shaders! Fluffy, fancy, and fantastic!");
        }

        private void btnDebug_MouseEnter(object sender, EventArgs e)
        {
            _ = UI.Announce("Oh dear, what have we here? A log full of secrets! (and probably some errors…)");
        }

        private void btnAbout_MouseEnter(object sender, EventArgs e)
        {
            _ = UI.Announce("About? About what? Oh! The project! Yes, yes, right this way!");
        }

        private void btnPackagesFolder_MouseEnter(object sender, EventArgs e)
        {
            _ = UI.Announce("Where all your precious presets and shaders live! Don’t worry, they’re well-fed.");
        }

        private void btnGameFolder_MouseEnter(object sender, EventArgs e)
        {
            _ = UI.Announce("Game files, game files everywhere! Tread carefully, adventurer!");
        }

        private void btnFixIt_MouseEnter(object sender, EventArgs e)
        {
            _ = UI.Announce("Duplicated shaders? green tint everywhere? No worries, 'Fix It' is here to save the day!");
        }

        private void btnInstallReShade_MouseEnter(object sender, EventArgs e)
        {
            _ = UI.Announce("You need to shut down the game client before you can update!");
        }

        internal async Task UpdateElements()
        {

            var c = Instances.LocalConfigProvider.Configuration;

            var txtGamePathText = "";
            var mustDetect = false;


            if (c.Game.InstallationFolder != null)
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

                _ = UI.Announce($"A new Bundlingway version ({c.Bundlingway.RemoteVersion}) is out!");
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
                    reShadeBtnEnabled = !Instances.IsGameRunning;
                    _ = UI.Announce("If you want to update ReShade, shut down the game first!");
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
            _ = Utilities.Handler.Bundlingway.Update(btnUpdate);
        }

        private void btnReinstall_Click(object sender, EventArgs e)
        {
            SetPackageOpsAvailable(false);
            var selectedPackages = dgvPackages.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(row => (ResourcePackage)row.Tag)
                .ToList();

            var count = selectedPackages.Count;
            var pos = 0;

            _ = UI.StartProgress(count);

            Task.WhenAll(selectedPackages.Select(package => Task.Run(() =>
            {
                Package.Reinstall(package);
                _ = UI.SetProgress(++pos);
            }))).ContinueWith(t =>
            {

                Maintenance.RemoveTempDir();
                _ = UI.Announce(Constants.MessageCategory.ReinstallPackage);
                PopulateGrid();
                SetPackageOpsAvailable(true);

                _ = UI.StopProgress();

            });
        }

        private void btnShortcuts_Click(object sender, EventArgs e)
        {
            frmShortcuts shortcutsForm = new frmShortcuts();
            shortcutsForm.ShowDialog();
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            var target = Path.Combine(Instances.BundlingwayDataFolder, Constants.Folders.Backup);
            if (!Directory.Exists(target))
            {
                Directory.CreateDirectory(target);
            }

            _ = UI.Announce("Backing up cache and packages...");

            var source1 = Path.Combine(Instances.BundlingwayDataFolder, Constants.Folders.Cache);

            //Copy all files from the cache folder to backup
            foreach (var file in Directory.GetFiles(source1))
            {
                var destFile = Path.Combine(target, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            var source2 = Path.Combine(Instances.BundlingwayDataFolder, Constants.Folders.Packages);

            // for each folder in source2, copy the contents of its 'source' folder to the target folder
            foreach (var folder in Directory.GetDirectories(source2))
            {
                var source = Path.Combine(folder, Constants.Folders.SourcePackage);

                if (Path.GetFileName(folder).Equals(Constants.Folders.SinglePresets))
                {
                    foreach (var file in Directory.GetFiles(folder, "*.ini"))
                    {
                        var destFile = Path.Combine(target, Path.GetFileName(file));
                        File.Copy(file, destFile, true);
                    }

                }
                else
                {
                    foreach (var file in Directory.GetFiles(source))
                    {
                        var destFile = Path.Combine(target, Path.GetFileName(file));
                        File.Copy(file, destFile, true);
                    }
                }
            }

            _ = UI.Announce("Backup complete!");

            //Open the backup folder
            System.Diagnostics.Process.Start("explorer.exe", target);
        }

        private void btnFavPackage_Click(object sender, EventArgs e)
        {
            var selectedPackages = dgvPackages.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(row => (ResourcePackage)row.Tag)
                .ToList();

            Task.WhenAll(selectedPackages.Select(package => Task.Run(() => Package.ToggleFavorite(package)))).ContinueWith(t =>
            {
                PopulateGrid();
            });
        }

        private void btnLockPackage_Click(object sender, EventArgs e)
        {
            var selectedPackages = dgvPackages.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(row => (ResourcePackage)row.Tag)
                .ToList();

            Task.WhenAll(selectedPackages.Select(package => Task.Run(() => Package.ToggleLocked(package)))).ContinueWith(t =>
            {
                PopulateGrid();
            });
        }

        internal async Task StartProgress(long count)
        {
            prgCommon?.DoAction(() =>
            {
                prgCommon.Value = 0;
                prgCommon.Tag = count;
                prgCommon.Visible = true;
            });
        }

        internal async Task SetProgress(long value)
        {
            var percentage = (int)Math.Round((double)value / (long)prgCommon.Tag * 100);

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
                "This process can take a little while as Bundlingway analyses everything and fixes your setup, friend.\n(Don't worry, I'll backup everything in case you want to revert changes.)\n\nDo you want to continue?",
                "Lots of work ahead!",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                UI.DisableEverything();
                Utilities.Handler.Bundlingway.FixIt().Wait();
                UI.EnableEverything();
            }
            else
            {
                // User chose not to proceed
            }
        }

        internal void DisableEverything()
        {
            flpSideMenu.DoAction(() => { flpSideMenu.Enabled = false; });
            flpPackageOptions.DoAction(() => { flpPackageOptions.Enabled = false; });
            pnlSettings.DoAction(() => { pnlSettings.Enabled = false; });
        }


        internal void EnableEverything()
        {
            flpSideMenu.DoAction(() => { flpSideMenu.Enabled = true; });
            flpPackageOptions.DoAction(() => { flpPackageOptions.Enabled = true; });
            pnlSettings.DoAction(() => { pnlSettings.Enabled = true; });
        }
    }
}
