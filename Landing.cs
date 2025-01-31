using Bundlingway.Model;
using Bundlingway.Utilities;
using Bundlingway.Utilities.Handler;
using Serilog;

namespace Bundlingway
{
    public partial class Landing : Form
    {
        public Landing()
        {
            UI._landing = this;

            UI.Announce(Constants.MessageCategory.ApplicationStart);

            InitializeComponent();

            Bootstrap.DetectSettings().ContinueWith(a => EvaluateButtonStates());

            mainSource.DataSource = Instances.LocalConfigProvider.Configuration;
            Instances.MainDataSource = mainSource;

            PopulateGrid();

            ProcessHelper.NotificationReceived += ProcessHelper_NotificationReceived;
            ProcessHelper.ListenForNotifications();

            UI.Announce(Constants.MessageCategory.Ready);
        }

        private void ProcessHelper_NotificationReceived(object? sender, string e)
        {
            if (e == Constants.Events.PackageInstalled)
            {
                PopulateGrid();
                UI.Announce(Constants.MessageCategory.Finished);
            }
        }

        private void EvaluateButtonStates()
        {
            if (btnInstallReShade.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate
                {
                    btnInstallReShade.Visible = false;
                    btnInstallGPosingway.Visible = false;
                }));
            }
            else
            {
                btnInstallReShade.Visible = false;
            }

            if (Instances.LocalConfigProvider.Configuration.ReShade.LocalVersion != null &&
                Instances.LocalConfigProvider.Configuration.ReShade.LocalVersion != "N/A" &&
                Instances.LocalConfigProvider.Configuration.ReShade.RemoteVersion != null &&
                Instances.LocalConfigProvider.Configuration.ReShade.RemoteVersion != "N/A" &&
                Instances.LocalConfigProvider.Configuration.ReShade.LocalVersion != Instances.LocalConfigProvider.Configuration.ReShade.RemoteVersion)
            {
                if (btnInstallReShade.InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate { btnInstallReShade.Visible = true; }));
                }
                else
                {
                    btnInstallReShade.Visible = true;
                }
            }

            if (Instances.LocalConfigProvider.Configuration.GPosingway.LocalVersion != null &&
                Instances.LocalConfigProvider.Configuration.GPosingway.LocalVersion != "N/A" &&
                Instances.LocalConfigProvider.Configuration.GPosingway.RemoteVersion != null &&
                Instances.LocalConfigProvider.Configuration.GPosingway.RemoteVersion != "N/A" &&
                Instances.LocalConfigProvider.Configuration.GPosingway.LocalVersion != Instances.LocalConfigProvider.Configuration.GPosingway.RemoteVersion)
            {
                if (btnInstallGPosingway.InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate { btnInstallGPosingway.Visible = true; }));
                }
                else
                {
                    btnInstallGPosingway.Visible = true;
                }
            }
        }

        private void Landing_Load(object sender, EventArgs e)
        {
            //lblGPosingwayVersion.Text = $"GPosingway {Instances.LocalConfigProvider.appVersion}";
        }

        private void btnDetectSettings_Click(object sender, EventArgs e)
        {
            Bootstrap.DetectSettings().ContinueWith(a => EvaluateButtonStates());
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
                        Package.Onboard(file);
                    }
                }
            }
        }

        private void btnInstallPackage_Click(object sender, EventArgs e)
        {
            UI.Announce(Constants.MessageCategory.BeforeAddPackageSelection);

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                var filter = string.Join(";", Constants.InstallableExtensions.Select(ext => $"*{ext}"));
                openFileDialog.Filter = $"Archive files ({filter})|{filter}";
                openFileDialog.Title = "Select a Package File";
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var validExtensions = new HashSet<string>(Constants.InstallableExtensions);
                    var selectedFiles = openFileDialog.FileNames
                        .Where(file => validExtensions.Contains(Path.GetExtension(file).ToLower()))
                        .ToList();

                    Package.Onboard(selectedFiles).ContinueWith(a =>
                    {
                        Instances.LocalConfigProvider.Save();
                        PopulateGrid();

                        UI.Announce(Constants.MessageCategory.Finished);
                    });
                }
                else
                {
                    UI.Announce(Constants.MessageCategory.AddPackageSelectionCancelled);
                }
            }
        }

        private void PopulateGrid()
        {
            Log.Information("Landing: PopulateGrid - Populating the grid with resource packages");
            Package.Scan().Wait();

            if (dgvPackages.InvokeRequired) { Invoke(new MethodInvoker(dgvPackages.Rows.Clear)); }
            else { dgvPackages.Rows.Clear(); }

            foreach (var package in Instances.ResourcePackages)
            {
                var rowObj = new DataGridViewRow();
                rowObj.CreateCells(dgvPackages, package.Type, package.Name, package.Status);
                rowObj.Tag = package;

                if (dgvPackages.InvokeRequired) { Invoke(new MethodInvoker(delegate { 
                    dgvPackages.Rows.Add(rowObj);
                    lblPackagesTitle.Text = $"{dgvPackages.Rows.Count} Packages";
                })); }
                else {
                    dgvPackages.Rows.Add(rowObj);
                    lblPackagesTitle.Text = $"{dgvPackages.Rows.Count} Packages";
                }
            }
        }

        private void Generic_DragDrop(object sender, DragEventArgs e)
        {
            Log.Information("Landing: Generic_DragDrop - Drag and drop event");
            Name = "flowLayoutPanel1_DragDrop";
            TryInstalPackages(e);
        }

        private void Generic_DragEnter(object sender, DragEventArgs e)
        {
            Log.Information("Landing: Generic_DragEnter - Drag enter event");
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
            Log.Information("Landing: Generic_DragOver - Drag over event");
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
            foreach (DataGridViewRow row in dgvPackages.SelectedRows)
            {
                var package = (ResourcePackage)row.Tag;
                Package.Remove(package);
            }

            UI.Announce(Constants.MessageCategory.RemovePackage);

            PopulateGrid();

        }

        private void btnUninstall_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvPackages.SelectedRows)
            {
                Package.Uninstall((ResourcePackage)row.Tag);
            }

            UI.Announce(Constants.MessageCategory.UninstallPackage);

            PopulateGrid();
        }

        private void btnReinstall_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvPackages.SelectedRows)
            {
                var package = (ResourcePackage)row.Tag;
                Package.Reinstall(package);
            }

            UI.Announce(Constants.MessageCategory.ReinstallPackage);

            PopulateGrid();

        }

        private void btnInstallReShade_Click(object sender, EventArgs e)
        {
            ReShade.Update().ContinueWith(a =>
            {
                Bootstrap.DetectSettings().ContinueWith(b => EvaluateButtonStates());
            });
        }

        private void btnInstallGPosingway_Click(object sender, EventArgs e)
        {
            GPosingway.Update().ContinueWith(a =>
            {
                Bootstrap.DetectSettings().ContinueWith(b => EvaluateButtonStates());
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

            string gamePath = Instances.LocalConfigProvider.Configuration.GameFolder;
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
            string logFilePath = Path.Combine(Instances.BundlingwayDataFolder, Constants.WellKnown.LogFileName);
            if (File.Exists(logFilePath))
            {
                System.Diagnostics.Process.Start("notepad.exe", logFilePath);
            }
        }
    }
}
