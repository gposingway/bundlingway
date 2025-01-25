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

            Bootstrap.Initialize().ContinueWith(a => EvaluateButtonStates());


            dgvPackages.DataSource = Instances.LocalConfigProvider.Configuration;
            mainSource.DataSource = Instances.LocalConfigProvider.Configuration;
            Instances.MainDataSource = mainSource;
        }

        private void EvaluateButtonStates()
        {
            if (btnInstallReShade.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate { btnInstallReShade.Visible = false; }));
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

                if (btnInstallReShade.InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate { btnInstallReShade.Visible = true; }));
                }
                else
                {
                    btnInstallReShade.Visible = true;
                }
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
            Bootstrap.DetectSettings().ContinueWith(a =>
            {

                EvaluateButtonStates();
            });
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

        private void btnInstallReShade_Click(object sender, EventArgs e)
        {
            ReShadeParser.Update().ContinueWith(a =>
            {
                Bootstrap.DetectSettings().ContinueWith(a => EvaluateButtonStates());
            }); ;

        }
    }
}
