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
            InitializeComponent();

            Bootstrap.Initialize();

            dgvPackages.DataSource = Instances.LocalConfigProvider.Configuration;
            mainSource.DataSource = Instances.LocalConfigProvider.Configuration;
            Instances.MainDataSource = mainSource;
        }

        private void Landing_Load(object sender, EventArgs e)
        {
            lblGPosingwayVersion.Text = $"GPosingway {Instances.LocalConfigProvider.appVersion}";
            lblCopyright.Text = $"2023-{DateTime.Now.Year} GPosingway Development Team";
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            pnlSettings.Focus();
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            pnlAbout.Focus();
        }

        private void btnDownloads_Click(object sender, EventArgs e)
        {
            pnlPackages.Focus();
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            pnlBackup.Focus();
        }

        private void btnDetectSettings_Click(object sender, EventArgs e)
        {
            Bootstrap.DetectSettings().Wait();
        }

        private void Landing_DragDrop(object sender, DragEventArgs e)
        {
            TryInstalPackages(e);
        }

        private void dgvPackages_DragDrop(object sender, DragEventArgs e)
        {
            this.Name = "dgvPackages_DragDrop";
            TryInstalPackages(e);
        }

        private void TryInstalPackages(DragEventArgs e)
        {
            if (e.Data == null) return;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                List<string> files = [.. (e.Data.GetData(DataFormats.FileDrop) as string[])];

                foreach (string file in files)
                {
                    string fileExtension = Path.GetExtension(file).ToLower();

                    var newCatalogEntry = new ResourcePackage();
                    Instances.LocalConfigProvider.Configuration.ResourcePackages.Add(newCatalogEntry);

                    if (fileExtension == ".rar" || fileExtension == ".zip")
                    {
                        PackageManager.InstallPackage(file, newCatalogEntry);
                    }
                }
            }
        }

        private void dgvPackages_DragEnter(object sender, DragEventArgs e)
        {
            this.Name = "dgvPackages_DragEnter";
            e.Effect = DragDropEffects.Copy;
        }

        private void Landing_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void btnInstallPackage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Archive files (*.zip;*.rar)|*.zip;*.rar";
                openFileDialog.Title = "Select a Package File";

                var newCatalogEntry = new ResourcePackage();
                Instances.LocalConfigProvider.Configuration.ResourcePackages.Add(newCatalogEntry);
                Instances.LocalConfigProvider.Save();
                mainSource.ResetBindings(false);

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFile = openFileDialog.FileName;
                    string fileExtension = Path.GetExtension(selectedFile).ToLower();

                    if (fileExtension == ".zip" || fileExtension == ".rar")
                    {
                        PackageManager.InstallPackage(selectedFile, newCatalogEntry);
                        Instances.LocalConfigProvider.Save();
                        mainSource.ResetBindings(false);
                    }
                }
            }
        }

        private void flowLayoutPanel1_DragDrop(object sender, DragEventArgs e)
        {
            this.Name = "flowLayoutPanel1_DragDrop";
            TryInstalPackages(e);
        }

        private void flowLayoutPanel1_DragEnter(object sender, DragEventArgs e)
        {
            this.Name = "flowLayoutPanel1_DragEnter";
            e.Effect = DragDropEffects.Copy;
        }
    }

   
}
