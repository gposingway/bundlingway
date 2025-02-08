using Bundlingway.Model;
using Bundlingway.Utilities;
using Bundlingway.Utilities.Handler;
using Serilog;
using static Bundlingway.Model.GPosingwayConfig;

namespace Bundlingway
{
    public partial class frmLanding : Form
    {

        public frmLanding()
        {
            UI._landing = this;
            InitializeComponent();

            this.Text = $"Bundlingway · v{Instances.AppVersion}";

            _ = UI.Announce(Constants.MessageCategory.ApplicationStart);

            _ = Bootstrap.DetectSettings();

            PopulateGrid();

            ProcessHelper.NotificationReceived += ProcessHelper_NotificationReceived;
            _ = ProcessHelper.ListenForNotifications();

            _ = UI.Announce(Constants.MessageCategory.Ready);
        }

        private void ProcessHelper_NotificationReceived(object? sender, string e)
        {
            if (e == Constants.Events.PackageInstalled)
            {
                PopulateGrid();
                _ = UI.Announce(Constants.MessageCategory.Finished);
            }
        }


        private void btnDetectSettings_Click(object sender, EventArgs e)
        {
            _ = Bootstrap.DetectSettings().ContinueWith(async a => { await UpdateElements(); });
        }

        private void TryInstalPackages(DragEventArgs e)
        {
            if (e.Data == null) return;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                List<string> files = new List<string>(e.Data.GetData(DataFormats.FileDrop) as string[]);

                foreach (string file in files)
                {
                    string fileExtension = Path.GetExtension(file).ToLower();

                    if (Constants.InstallableExtensions.Contains(fileExtension))
                    {
                        _ = Package.Onboard(file);
                    }
                }
            }
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

            if (btnInstallPackage.InvokeRequired)
            {
                {
                    Invoke(new MethodInvoker(delegate
                {
                    btnInstallPackage.Enabled = v;
                    btnRemove.Enabled = v;
                    btnUninstall.Enabled = v;
                    btnReinstall.Enabled = v;
                }));
                }
            }
            else
            {
                btnInstallPackage.Enabled = v;
                btnRemove.Enabled = v;
                btnUninstall.Enabled = v;
                btnReinstall.Enabled = v;
            }


        }

        private void PopulateGrid()
        {
            Log.Information("frmLanding: PopulateGrid - Populating the grid with resource packages");
            Package.Scan().Wait();

            if (dgvPackages.InvokeRequired) { Invoke(new MethodInvoker(dgvPackages.Rows.Clear)); }
            else { dgvPackages.Rows.Clear(); }

            foreach (var package in Instances.ResourcePackages)
            {
                var rowObj = new DataGridViewRow();
                rowObj.CreateCells(dgvPackages, package.Type, package.Name, package.Status);
                rowObj.Tag = package;

                if (dgvPackages.InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate
                    {
                        dgvPackages.Rows.Add(rowObj);
                        lblGrpPackages.Text = $"{dgvPackages.Rows.Count} Packages";
                    }));
                }
                else
                {
                    dgvPackages.Rows.Add(rowObj);
                    lblGrpPackages.Text = $"{dgvPackages.Rows.Count} Packages";
                }
            }
        }

        private void Generic_DragDrop(object sender, DragEventArgs e)
        {
            Name = "flowLayoutPanel1_DragDrop";
            TryInstalPackages(e);
        }

        private void Generic_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy; // You can set this to another effect if needed
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Generic_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy; // You can set this to another effect if needed
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
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

        private void btnReinstall_Click(object sender, EventArgs e)
        {
            SetPackageOpsAvailable(false);

            var selectedPackages = dgvPackages.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(row => (ResourcePackage)row.Tag)
                .ToList();

            Task.WhenAll(selectedPackages.Select(package => Task.Run(() => Package.Reinstall(package)))).ContinueWith(t =>
            {
                Maintenance.RemoveTempDir();
                _ = UI.Announce(Constants.MessageCategory.ReinstallPackage);
                PopulateGrid();
                SetPackageOpsAvailable(true);
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

            if (lblAnnouncement.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate { lblAnnouncement.Text = message; }));
            }
            else
            {
                lblAnnouncement.Text = message;
            }
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


            if (txtXivPath.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate { txtXivPath.Text = txtGamePathText; }));
            }
            else
            {
                txtXivPath.Text = txtGamePathText;
            }

            if (mustDetect)
            {

                if (txtReShadeStatus.InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate
                    {

                        txtReShadeStatus.Text = "Waiting...";
                        txtGPosingwayStatus.Text = "Waiting...";

                        btnInstallReShade.Visible = false;
                        btnInstallGPosingway.Visible = false;
                    }));
                }
                else
                {
                    txtReShadeStatus.Text = "Waiting...";
                    txtGPosingwayStatus.Text = "Waiting...";

                    btnInstallReShade.Visible = false;
                    btnInstallGPosingway.Visible = false;
                }

                return;
            }

            if (c.Bundlingway.RemoteVersion != c.Bundlingway.LocalVersion)
            {
                if (btnUpdate.InvokeRequired) { Invoke(new MethodInvoker(delegate { btnUpdate.Visible = true; })); }
                else { btnUpdate.Visible = true; }

                _ = UI.Announce($"A new Bundlingway version ({c.Bundlingway.RemoteVersion}) is out!");
            }
            else
            {
                if (btnUpdate.InvokeRequired) { Invoke(new MethodInvoker(delegate { btnUpdate.Visible = false; })); }
                else { btnUpdate.Visible = false; }
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


            if (txtReShadeStatus.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate
                {
                    txtReShadeStatus.Text = reShadeText;

                    btnInstallReShade.Enabled = reShadeBtnEnabled;
                    btnInstallReShade.Visible = reShadeBtnVisible;
                    btnInstallReShade.Text = reShadeBtnText;
                }));
            }
            else
            {
                txtReShadeStatus.Text = reShadeText;

                btnInstallReShade.Enabled = reShadeBtnEnabled;
                btnInstallReShade.Visible = reShadeBtnVisible;
                btnInstallReShade.Text = reShadeBtnText;
            }

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

            if (txtGPosingwayStatus.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate
                {
                    txtGPosingwayStatus.Text = GPosingwayText;

                    btnInstallGPosingway.Enabled = gPosingwayBtnEnabled;
                    btnInstallGPosingway.Visible = gPosingwayBtnVisible;
                    btnInstallGPosingway.Text = gPosingwayBtnText;
                }));
            }
            else
            {
                txtGPosingwayStatus.Text = GPosingwayText;

                btnInstallGPosingway.Enabled = gPosingwayBtnEnabled;
                btnInstallGPosingway.Visible = gPosingwayBtnVisible;
                btnInstallGPosingway.Text = gPosingwayBtnText;
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            btnUpdate.Text = "Updating...";
            btnUpdate.Enabled = false;
            Utilities.Handler.Bundlingway.Update();
        }

        private void btnUpdate_MouseEnter(object sender, EventArgs e)
        {
            _ = UI.Announce($"A new Bundlingway version ({Instances.LocalConfigProvider.Configuration.Bundlingway.RemoteVersion}) is out!");
        }
    }
}
