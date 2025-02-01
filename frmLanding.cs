using Bundlingway.Model;
using Bundlingway.Utilities;
using Bundlingway.Utilities.Handler;
using Serilog;

namespace Bundlingway
{
    public partial class frmLanding : Form
    {
        public frmLanding()
        {
            UI._landing = this;

            InitializeComponent();

            SwitchToDarkMode();

            UI.Announce(Constants.MessageCategory.ApplicationStart);

            Bootstrap.DetectSettings().ContinueWith(a => EvaluateButtonStates());

            mainSource.DataSource = Instances.LocalConfigProvider.Configuration;
            Instances.MainDataSource = mainSource;

            PopulateGrid();

            ProcessHelper.NotificationReceived += ProcessHelper_NotificationReceived;
            ProcessHelper.ListenForNotifications();

            UI.Announce(Constants.MessageCategory.Ready);
        }

        private void SwitchToDarkMode()
        {
            // Dark Mode Colors
            Color backColor = Color.FromArgb(32, 33, 36); // Dark background
            Color foreColor = Color.FromArgb(220, 221, 225); // Light text
            Color panelColor = Color.FromArgb(43, 44, 47); // Slightly lighter panel background
            Color controlBackColor = Color.FromArgb(55, 56, 59); //Even Lighter Background for controls like textboxes.
            Color gridBackColor = Color.FromArgb(48, 49, 52); //Background color for datagridview.
            Color gridForeColor = Color.FromArgb(170, 171, 175); //Foreground color for datagridview.
            Color borderColor = Color.FromArgb(68, 69, 72); //Border color for controls.


            this.BackColor = backColor;
            this.ForeColor = foreColor;

            // Apply to controls
            splitContainer1.Panel1.BackColor = backColor;
            splitContainer1.Panel2.BackColor = backColor;
            flpSideMenu.BackColor = panelColor;

            foreach (Control control in this.Controls)
            {
                ApplyDarkMode(control, backColor, foreColor, panelColor, controlBackColor, gridBackColor, gridForeColor, borderColor);
            }

        }


        private void ApplyDarkMode(Control control, Color backColor, Color foreColor, Color panelColor, Color controlBackColor, Color gridBackColor, Color gridForeColor, Color borderColor)
        {

            var originalBackColor = control.BackColor;
            var originalForeColor = control.ForeColor;

            control.BackColor = backColor;
            control.ForeColor = foreColor;

            if (control is Panel || control is FlowLayoutPanel || control is GroupBox)
            {
                control.BackColor = panelColor;
            }
            else if (control is TextBox)
            {
                control.BackColor = backColor;
                ((TextBox)control).BorderStyle = BorderStyle.FixedSingle;
                ((TextBox)control).ForeColor = foreColor;

            }
            else if (control is Button)
            {
                control.BackColor = controlBackColor;
                ((Button)control).FlatStyle = FlatStyle.Flat;
                ((Button)control).FlatAppearance.BorderColor = borderColor;
                ((Button)control).FlatAppearance.BorderSize = 1;
            }
            else if (control is Label)
            {
                // Labels with a specific background color should keep it (e.g. Title Labels)
                if (control.BackColor == SystemColors.Control)
                {
                    control.BackColor = panelColor;
                }
                else
                {
                    control.ForeColor = originalForeColor;
                    control.BackColor = originalBackColor;
                }
            }
            else if (control is DataGridView)
            {
                DataGridView dataGridView = (DataGridView)control;
                dataGridView.BackgroundColor = gridBackColor;
                dataGridView.ForeColor = gridForeColor;
                dataGridView.GridColor = borderColor;
                dataGridView.ColumnHeadersDefaultCellStyle.BackColor = gridBackColor;
                dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = gridForeColor;
                dataGridView.RowHeadersDefaultCellStyle.BackColor = gridBackColor;
                dataGridView.RowHeadersDefaultCellStyle.ForeColor = gridForeColor;
                dataGridView.EnableHeadersVisualStyles = false; // Important for header styling
                dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
                dataGridView.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
                dataGridView.BorderStyle = BorderStyle.FixedSingle;

                foreach (DataGridViewColumn column in dataGridView.Columns)
                {
                    column.DefaultCellStyle.BackColor = gridBackColor;
                    column.DefaultCellStyle.ForeColor = gridForeColor;
                }

            }
            else if (control is FontAwesome.Sharp.IconButton)
            {
                control.BackColor = controlBackColor;
                ((FontAwesome.Sharp.IconButton)control).FlatStyle = FlatStyle.Flat;
                ((FontAwesome.Sharp.IconButton)control).FlatAppearance.BorderColor = borderColor;
                ((FontAwesome.Sharp.IconButton)control).FlatAppearance.BorderSize = 1;
            }
            else if (control is PictureBox)
            {
                control.BackColor = panelColor;
            }

            foreach (Control child in control.Controls)
            {
                ApplyDarkMode(child, backColor, foreColor, panelColor, controlBackColor, gridBackColor, gridForeColor, borderColor);
            }
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

        private void frmLanding_Load(object sender, EventArgs e)
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
            Log.Information("frmLanding: Generic_DragDrop - Drag and drop event");
            Name = "flowLayoutPanel1_DragDrop";
            TryInstalPackages(e);
        }

        private void Generic_DragEnter(object sender, DragEventArgs e)
        {
            Log.Information("frmLanding: Generic_DragEnter - Drag enter event");
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
            Log.Information("frmLanding: Generic_DragOver - Drag over event");
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
            var selectedPackages = dgvPackages.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(row => (ResourcePackage)row.Tag)
                .ToList();

            Task.WhenAll(selectedPackages.Select(package => Task.Run(() => Package.Remove(package)))).ContinueWith(t =>
            {
                UI.Announce(Constants.MessageCategory.UninstallPackage);
                PopulateGrid();
            });

        }

        private void btnUninstall_Click(object sender, EventArgs e)
        {
            var selectedPackages = dgvPackages.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(row => (ResourcePackage)row.Tag)
                .ToList();

            Task.WhenAll(selectedPackages.Select(package => Task.Run(() => Package.Uninstall(package)))).ContinueWith(t =>
            {
                UI.Announce(Constants.MessageCategory.UninstallPackage);
                PopulateGrid();
            });
        }

        private void btnReinstall_Click(object sender, EventArgs e)
        {
            var selectedPackages = dgvPackages.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(row => (ResourcePackage)row.Tag)
                .ToList();

            Task.WhenAll(selectedPackages.Select(package => Task.Run(() => Package.Reinstall(package)))).ContinueWith(t =>
            {
                UI.Announce(Constants.MessageCategory.ReinstallPackage);
                PopulateGrid();
            });
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
