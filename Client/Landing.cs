using Bundlingway.Model;
using Bundlingway.Utilities;
using Microsoft.Extensions.Logging;

namespace Bundlingway
{
    public partial class Landing : Form
    {
        private readonly ILogger<Instances> _logger;

        public Landing()
        {
            Console.WriteLine("Landing: Constructor - Initializing components");
            InitializeComponent();

            Bootstrap.Initialize().ContinueWith(a => EvaluateButtonStates());
            mainSource.DataSource = Instances.LocalConfigProvider.Configuration;
            Instances.MainDataSource = mainSource;
            
            PopulateGrid();
        }

        private void EvaluateButtonStates()
        {
            Console.WriteLine("Landing: EvaluateButtonStates - Evaluating button states");
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
            Console.WriteLine("Landing: Landing_Load - Loading landing form");
            lblGPosingwayVersion.Text = $"GPosingway {Instances.LocalConfigProvider.appVersion}";
            lblCopyright.Text = $"2023-{DateTime.Now.Year} GPosingway Development Team";
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Landing: btnSettings_Click - Settings button clicked");
            pnlSettings.Focus();
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Landing: btnAbout_Click - About button clicked");
            pnlAbout.Focus();
        }

        private void btnDownloads_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Landing: btnDownloads_Click - Downloads button clicked");
            pnlPackages.Focus();
        }

        private void btnDetectSettings_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Landing: btnDetectSettings_Click - Detect settings button clicked");
            Bootstrap.DetectSettings().ContinueWith(a => EvaluateButtonStates());
        }

        private void TryInstalPackages(DragEventArgs e)
        {
            Console.WriteLine("Landing: TryInstalPackages - Trying to install packages");
            if (e.Data == null) return;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                List<string> files = new List<string>(e.Data.GetData(DataFormats.FileDrop) as string[]);

                foreach (string file in files)
                {
                    string fileExtension = Path.GetExtension(file).ToLower();

                    if (fileExtension == ".rar" || fileExtension == ".zip")
                    {
                        PackageManager.PreparePackageCatalog(file);
                    }
                }
            }
        }

        private void btnInstallPackage_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Landing: btnInstallPackage_Click - Install package button clicked");
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Archive files (*.zip;*.rar)|*.zip;*.rar";
                openFileDialog.Title = "Select a Package File";
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string selectedFile in openFileDialog.FileNames)
                    {
                        string fileExtension = Path.GetExtension(selectedFile).ToLower();

                        if (fileExtension == ".zip" || fileExtension == ".rar")
                            PackageManager.PreparePackageCatalog(selectedFile);
                    }

                    Instances.LocalConfigProvider.Save();
                    mainSource.ResetBindings(true);

                    PopulateGrid();
                }
            }
        }

        private void PopulateGrid()
        {
            Console.WriteLine("Landing: PopulateGrid - Populating the grid with resource packages");
            PackageManager.ScanPackages().Wait();
            dgvPackages.Rows.Clear();

            foreach (var package in Instances.ResourcePackages)
            {
                var index = dgvPackages.Rows.Add(
                    package.Type,
                    package.Name,
                    package.Status
                );

                // Set the row data as the package object
                dgvPackages.Rows[index].Tag = package;
            }
        }

        private void Generic_DragDrop(object sender, DragEventArgs e)
        {
            Console.WriteLine("Landing: Generic_DragDrop - Drag and drop event");
            Name = "flowLayoutPanel1_DragDrop";
            TryInstalPackages(e);
        }

        private void Generic_DragEnter(object sender, DragEventArgs e)
        {
            Console.WriteLine("Landing: Generic_DragEnter - Drag enter event");
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
            Console.WriteLine("Landing: Generic_DragOver - Drag over event");
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy; // You can set this to another effect if needed
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void btnInstallReShade_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Landing: btnInstallReShade_Click - Install ReShade button clicked");
            ReShadeParser.Update().ContinueWith(a =>
            {
                Bootstrap.DetectSettings().ContinueWith(b => EvaluateButtonStates());
            });
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Landing: btnRemoveSelectedPackages_Click - Remove selected packages button clicked");
            foreach (DataGridViewRow row in dgvPackages.SelectedRows)
            {
                var package = (ResourcePackage)row.Tag;
                PackageManager.RemovePackage(package);
            }
            PopulateGrid();

        }

        private void btnUninstall_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Landing: btnRemoveSelectedPackages_Click - Remove selected packages button clicked");
            foreach (DataGridViewRow row in dgvPackages.SelectedRows)
            {
                PackageManager.UninstallPackage((ResourcePackage)row.Tag);
            }
            PopulateGrid();
        }

        private void btnReinstall_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Landing: btnReinstall_Click - Reinstall button clicked");
            foreach (DataGridViewRow row in dgvPackages.SelectedRows)
            {
                var package = (ResourcePackage)row.Tag;
                PackageManager.ReinstallPackage(package);
            }

            PopulateGrid();

        }
    }
}
