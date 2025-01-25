using GPosingway.Model;

namespace GPosingway
{
    public partial class Landing : Form
    {
        public Landing()
        {
            InitializeComponent();
            dataGridView1.DataSource = Instances.ResourcePackages;
            mainSource.DataSource = Instances.Config;
        }

        private void Landing_Load(object sender, EventArgs e)
        {

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
            Utilities.Bootstrap.DetectSettings();
        }
    }
}
