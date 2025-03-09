namespace Bundlingway
{
    partial class frmLanding
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLanding));
            contextMenuStrip1 = new ContextMenuStrip(components);
            splitContainer1 = new SplitContainer();
            flpSideMenu = new FlowLayoutPanel();
            btnShortcuts = new FontAwesome.Sharp.IconButton();
            btnFixIt = new FontAwesome.Sharp.IconButton();
            label5 = new Label();
            btnPackagesFolder = new FontAwesome.Sharp.IconButton();
            btnGameFolder = new FontAwesome.Sharp.IconButton();
            btnBackup = new FontAwesome.Sharp.IconButton();
            btnDebug = new FontAwesome.Sharp.IconButton();
            label1 = new Label();
            btnEmporium = new FontAwesome.Sharp.IconButton();
            btnAbout = new FontAwesome.Sharp.IconButton();
            label6 = new Label();
            btnUpdate = new FontAwesome.Sharp.IconButton();
            pnlAbout = new Panel();
            prgCommon = new ProgressBar();
            lblAnnouncement = new Label();
            pictureBox1 = new PictureBox();
            lblBundlingwaySays = new Label();
            pnlPackages = new Panel();
            dgvPackages = new DataGridView();
            FavCol = new DataGridViewTextBoxColumn();
            TypeCol = new DataGridViewTextBoxColumn();
            NameCol = new DataGridViewTextBoxColumn();
            StatusCol = new DataGridViewTextBoxColumn();
            flpPackageOptions = new FlowLayoutPanel();
            btnRemove = new Button();
            btnUninstall = new Button();
            btnReinstall = new Button();
            btnInstallPackage = new Button();
            btnLockPackage = new Button();
            btnFavPackage = new Button();
            lblGrpPackages = new Label();
            pnlSettings = new Panel();
            btnTopMost = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            btnInstallGPosingway = new Button();
            txtGPosingwayStatus = new TextBox();
            label4 = new Label();
            btnInstallReShade = new Button();
            txtReShadeStatus = new TextBox();
            label3 = new Label();
            label2 = new Label();
            txtXivPath = new TextBox();
            btnDetectSettings = new Button();
            lblGrpSettings = new Label();
            resourcePackageBindingSource = new BindingSource(components);
            instancesBindingSource = new BindingSource(components);
            instancesBindingSource1 = new BindingSource(components);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            flpSideMenu.SuspendLayout();
            pnlAbout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            pnlPackages.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPackages).BeginInit();
            flpPackageOptions.SuspendLayout();
            pnlSettings.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)resourcePackageBindingSource).BeginInit();
            ((System.ComponentModel.ISupportInitialize)instancesBindingSource).BeginInit();
            ((System.ComponentModel.ISupportInitialize)instancesBindingSource1).BeginInit();
            SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(61, 4);
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.IsSplitterFixed = true;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Margin = new Padding(0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(flpSideMenu);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.AutoScroll = true;
            splitContainer1.Panel2.Controls.Add(pnlAbout);
            splitContainer1.Panel2.Controls.Add(pnlPackages);
            splitContainer1.Panel2.Controls.Add(pnlSettings);
            splitContainer1.Size = new Size(661, 636);
            splitContainer1.SplitterDistance = 151;
            splitContainer1.TabIndex = 1;
            // 
            // flpSideMenu
            // 
            flpSideMenu.AutoScroll = true;
            flpSideMenu.BackColor = SystemColors.ControlLight;
            flpSideMenu.Controls.Add(btnShortcuts);
            flpSideMenu.Controls.Add(btnFixIt);
            flpSideMenu.Controls.Add(label5);
            flpSideMenu.Controls.Add(btnPackagesFolder);
            flpSideMenu.Controls.Add(btnGameFolder);
            flpSideMenu.Controls.Add(btnBackup);
            flpSideMenu.Controls.Add(btnDebug);
            flpSideMenu.Controls.Add(label1);
            flpSideMenu.Controls.Add(btnEmporium);
            flpSideMenu.Controls.Add(btnAbout);
            flpSideMenu.Controls.Add(label6);
            flpSideMenu.Controls.Add(btnUpdate);
            flpSideMenu.Dock = DockStyle.Fill;
            flpSideMenu.Location = new Point(0, 0);
            flpSideMenu.Margin = new Padding(0);
            flpSideMenu.MinimumSize = new Size(151, 516);
            flpSideMenu.Name = "flpSideMenu";
            flpSideMenu.Size = new Size(151, 636);
            flpSideMenu.TabIndex = 0;
            // 
            // btnShortcuts
            // 
            btnShortcuts.IconChar = FontAwesome.Sharp.IconChar.Keyboard;
            btnShortcuts.IconColor = SystemColors.Highlight;
            btnShortcuts.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnShortcuts.IconSize = 32;
            btnShortcuts.ImageAlign = ContentAlignment.MiddleLeft;
            btnShortcuts.Location = new Point(3, 3);
            btnShortcuts.Margin = new Padding(3, 3, 3, 0);
            btnShortcuts.Name = "btnShortcuts";
            btnShortcuts.Size = new Size(145, 37);
            btnShortcuts.TabIndex = 23;
            btnShortcuts.Tag = "Customize your in-game ReShade shortcuts! Make it easy to toggle effects and adjust settings on the fly.";
            btnShortcuts.Text = "In-game Shortcuts";
            btnShortcuts.TextAlign = ContentAlignment.MiddleRight;
            btnShortcuts.UseVisualStyleBackColor = true;
            btnShortcuts.Click += btnShortcuts_Click;
            // 
            // btnFixIt
            // 
            btnFixIt.IconChar = FontAwesome.Sharp.IconChar.Hammer;
            btnFixIt.IconColor = SystemColors.Highlight;
            btnFixIt.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnFixIt.IconSize = 32;
            btnFixIt.ImageAlign = ContentAlignment.MiddleLeft;
            btnFixIt.Location = new Point(3, 43);
            btnFixIt.Margin = new Padding(3, 3, 3, 0);
            btnFixIt.Name = "btnFixIt";
            btnFixIt.Size = new Size(145, 37);
            btnFixIt.TabIndex = 28;
            btnFixIt.Tag = "Broken presets? Duplicated effects? Missing files? I'll try to fix any problems. A full Loporrit service!";
            btnFixIt.Text = "Fix Everything!";
            btnFixIt.TextAlign = ContentAlignment.MiddleRight;
            btnFixIt.UseVisualStyleBackColor = true;
            btnFixIt.Click += btnFixIt_Click;
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label5.Font = new Font("Segoe UI", 6F);
            label5.Location = new Point(3, 80);
            label5.Name = "label5";
            label5.Size = new Size(100, 10);
            label5.TabIndex = 21;
            // 
            // btnPackagesFolder
            // 
            btnPackagesFolder.IconChar = FontAwesome.Sharp.IconChar.FolderOpen;
            btnPackagesFolder.IconColor = SystemColors.Highlight;
            btnPackagesFolder.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnPackagesFolder.IconSize = 32;
            btnPackagesFolder.ImageAlign = ContentAlignment.MiddleLeft;
            btnPackagesFolder.Location = new Point(3, 93);
            btnPackagesFolder.Margin = new Padding(3, 3, 3, 0);
            btnPackagesFolder.Name = "btnPackagesFolder";
            btnPackagesFolder.Size = new Size(145, 37);
            btnPackagesFolder.TabIndex = 3;
            btnPackagesFolder.Tag = "Where all your precious presets and shaders live! Don’t worry, they’re well-fed.";
            btnPackagesFolder.Text = "Repository";
            btnPackagesFolder.TextAlign = ContentAlignment.MiddleRight;
            btnPackagesFolder.UseVisualStyleBackColor = true;
            btnPackagesFolder.Click += btnPackagesFolder_Click;
            btnPackagesFolder.MouseEnter += btnPackagesFolder_MouseEnter;
            // 
            // btnGameFolder
            // 
            btnGameFolder.IconChar = FontAwesome.Sharp.IconChar.FolderOpen;
            btnGameFolder.IconColor = SystemColors.Highlight;
            btnGameFolder.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnGameFolder.IconSize = 32;
            btnGameFolder.ImageAlign = ContentAlignment.MiddleLeft;
            btnGameFolder.Location = new Point(3, 133);
            btnGameFolder.Margin = new Padding(3, 3, 3, 0);
            btnGameFolder.Name = "btnGameFolder";
            btnGameFolder.Size = new Size(145, 37);
            btnGameFolder.TabIndex = 2;
            btnGameFolder.Tag = "Game files, game files everywhere! Tread carefully, adventurer!";
            btnGameFolder.Text = "FFXIV Game Folder";
            btnGameFolder.TextAlign = ContentAlignment.MiddleRight;
            btnGameFolder.UseVisualStyleBackColor = true;
            btnGameFolder.Click += btnGameFolder_Click;
            btnGameFolder.MouseEnter += btnGameFolder_MouseEnter;
            // 
            // btnBackup
            // 
            btnBackup.IconChar = FontAwesome.Sharp.IconChar.DiceD6;
            btnBackup.IconColor = SystemColors.Highlight;
            btnBackup.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnBackup.IconSize = 32;
            btnBackup.ImageAlign = ContentAlignment.MiddleLeft;
            btnBackup.Location = new Point(3, 173);
            btnBackup.Margin = new Padding(3, 3, 3, 0);
            btnBackup.Name = "btnBackup";
            btnBackup.Size = new Size(145, 37);
            btnBackup.TabIndex = 25;
            btnBackup.Tag = "Create a backup of your managed packages. Better safe than sorry, friend!";
            btnBackup.Text = "Backup";
            btnBackup.TextAlign = ContentAlignment.MiddleRight;
            btnBackup.UseVisualStyleBackColor = true;
            btnBackup.Click += btnBackup_Click;
            // 
            // btnDebug
            // 
            btnDebug.IconChar = FontAwesome.Sharp.IconChar.ChevronRight;
            btnDebug.IconColor = SystemColors.Highlight;
            btnDebug.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnDebug.IconSize = 32;
            btnDebug.ImageAlign = ContentAlignment.MiddleLeft;
            btnDebug.Location = new Point(3, 213);
            btnDebug.Margin = new Padding(3, 3, 3, 0);
            btnDebug.Name = "btnDebug";
            btnDebug.Size = new Size(145, 37);
            btnDebug.TabIndex = 4;
            btnDebug.Tag = "Oh dear, what have we here? A log full of secrets! (and probably some errors…)";
            btnDebug.Text = "Log";
            btnDebug.TextAlign = ContentAlignment.MiddleRight;
            btnDebug.UseVisualStyleBackColor = true;
            btnDebug.Click += btnDebug_Click;
            btnDebug.MouseEnter += btnDebug_MouseEnter;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label1.Font = new Font("Segoe UI", 6F);
            label1.Location = new Point(3, 250);
            label1.Name = "label1";
            label1.Size = new Size(100, 10);
            label1.TabIndex = 26;
            // 
            // btnEmporium
            // 
            btnEmporium.IconChar = FontAwesome.Sharp.IconChar.Gifts;
            btnEmporium.IconColor = SystemColors.Highlight;
            btnEmporium.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnEmporium.IconSize = 32;
            btnEmporium.ImageAlign = ContentAlignment.MiddleLeft;
            btnEmporium.Location = new Point(3, 263);
            btnEmporium.Margin = new Padding(3, 3, 3, 0);
            btnEmporium.Name = "btnEmporium";
            btnEmporium.Size = new Size(145, 37);
            btnEmporium.TabIndex = 5;
            btnEmporium.Tag = "A Loporrit-approved selection of presets and shaders! Fluffy, fancy, and fantastic!";
            btnEmporium.Text = "The Emporium";
            btnEmporium.TextAlign = ContentAlignment.MiddleRight;
            btnEmporium.UseVisualStyleBackColor = true;
            btnEmporium.Click += btnEmporium_Click;
            btnEmporium.MouseEnter += btnEmporium_MouseEnter;
            // 
            // btnAbout
            // 
            btnAbout.IconChar = FontAwesome.Sharp.IconChar.Carrot;
            btnAbout.IconColor = SystemColors.Highlight;
            btnAbout.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnAbout.IconSize = 32;
            btnAbout.ImageAlign = ContentAlignment.MiddleLeft;
            btnAbout.Location = new Point(3, 303);
            btnAbout.Margin = new Padding(3, 3, 3, 0);
            btnAbout.Name = "btnAbout";
            btnAbout.Size = new Size(145, 37);
            btnAbout.TabIndex = 6;
            btnAbout.Tag = "About? About what? Oh! The project! Yes, yes, right this way!";
            btnAbout.Text = "About";
            btnAbout.TextAlign = ContentAlignment.MiddleRight;
            btnAbout.UseVisualStyleBackColor = true;
            btnAbout.Click += btnAbout_Click;
            btnAbout.MouseEnter += btnAbout_MouseEnter;
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label6.Font = new Font("Segoe UI", 6F);
            label6.Location = new Point(3, 340);
            label6.Name = "label6";
            label6.Size = new Size(100, 10);
            label6.TabIndex = 27;
            // 
            // btnUpdate
            // 
            btnUpdate.IconChar = FontAwesome.Sharp.IconChar.Download;
            btnUpdate.IconColor = Color.Red;
            btnUpdate.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnUpdate.IconSize = 32;
            btnUpdate.ImageAlign = ContentAlignment.MiddleLeft;
            btnUpdate.Location = new Point(3, 353);
            btnUpdate.Margin = new Padding(3, 3, 3, 0);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(145, 37);
            btnUpdate.TabIndex = 22;
            btnUpdate.Tag = "A new Bundlingway version is out - Click here to download and install!";
            btnUpdate.Text = "Update Available!";
            btnUpdate.TextAlign = ContentAlignment.MiddleRight;
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Visible = false;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // pnlAbout
            // 
            pnlAbout.Controls.Add(prgCommon);
            pnlAbout.Controls.Add(lblAnnouncement);
            pnlAbout.Controls.Add(pictureBox1);
            pnlAbout.Controls.Add(lblBundlingwaySays);
            pnlAbout.Dock = DockStyle.Bottom;
            pnlAbout.Location = new Point(0, 486);
            pnlAbout.Name = "pnlAbout";
            pnlAbout.Padding = new Padding(5);
            pnlAbout.Size = new Size(506, 150);
            pnlAbout.TabIndex = 3;
            // 
            // prgCommon
            // 
            prgCommon.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            prgCommon.Location = new Point(6, 139);
            prgCommon.Name = "prgCommon";
            prgCommon.Size = new Size(377, 6);
            prgCommon.TabIndex = 3;
            prgCommon.Visible = false;
            // 
            // lblAnnouncement
            // 
            lblAnnouncement.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblAnnouncement.FlatStyle = FlatStyle.Flat;
            lblAnnouncement.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblAnnouncement.Location = new Point(26, 49);
            lblAnnouncement.Name = "lblAnnouncement";
            lblAnnouncement.Size = new Size(338, 70);
            lblAnnouncement.TabIndex = 2;
            lblAnnouncement.Text = "Gathering my tools, one sec...";
            lblAnnouncement.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pictureBox1
            // 
            pictureBox1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pictureBox1.Image = Properties.Resources.ffxiv_dx11_OkamiClarity_2024_02_18_16_07_46_icon;
            pictureBox1.Location = new Point(389, 33);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(117, 117);
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // lblBundlingwaySays
            // 
            lblBundlingwaySays.BackColor = SystemColors.Highlight;
            lblBundlingwaySays.Dock = DockStyle.Top;
            lblBundlingwaySays.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblBundlingwaySays.ForeColor = SystemColors.Control;
            lblBundlingwaySays.Location = new Point(5, 5);
            lblBundlingwaySays.Name = "lblBundlingwaySays";
            lblBundlingwaySays.Padding = new Padding(3);
            lblBundlingwaySays.Size = new Size(496, 25);
            lblBundlingwaySays.TabIndex = 0;
            lblBundlingwaySays.Text = "Bundlingway Says...";
            // 
            // pnlPackages
            // 
            pnlPackages.Controls.Add(dgvPackages);
            pnlPackages.Controls.Add(flpPackageOptions);
            pnlPackages.Controls.Add(lblGrpPackages);
            pnlPackages.Dock = DockStyle.Fill;
            pnlPackages.Location = new Point(0, 132);
            pnlPackages.Name = "pnlPackages";
            pnlPackages.Padding = new Padding(5, 5, 5, 150);
            pnlPackages.Size = new Size(506, 504);
            pnlPackages.TabIndex = 1;
            // 
            // dgvPackages
            // 
            dgvPackages.AllowUserToAddRows = false;
            dgvPackages.AllowUserToDeleteRows = false;
            dgvPackages.AllowUserToResizeColumns = false;
            dgvPackages.AllowUserToResizeRows = false;
            dgvPackages.BackgroundColor = SystemColors.Control;
            dgvPackages.BorderStyle = BorderStyle.None;
            dgvPackages.CausesValidation = false;
            dgvPackages.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPackages.Columns.AddRange(new DataGridViewColumn[] { FavCol, TypeCol, NameCol, StatusCol });
            dgvPackages.ContextMenuStrip = contextMenuStrip1;
            dgvPackages.Dock = DockStyle.Fill;
            dgvPackages.Location = new Point(5, 30);
            dgvPackages.Name = "dgvPackages";
            dgvPackages.ReadOnly = true;
            dgvPackages.RowHeadersVisible = false;
            dgvPackages.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPackages.Size = new Size(496, 298);
            dgvPackages.TabIndex = 10;
            // 
            // FavCol
            // 
            FavCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            FavCol.DefaultCellStyle = dataGridViewCellStyle1;
            FavCol.HeaderText = "🔒";
            FavCol.MinimumWidth = 3;
            FavCol.Name = "FavCol";
            FavCol.ReadOnly = true;
            FavCol.Resizable = DataGridViewTriState.False;
            FavCol.Width = 44;
            // 
            // TypeCol
            // 
            TypeCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            TypeCol.HeaderText = "Type";
            TypeCol.Name = "TypeCol";
            TypeCol.ReadOnly = true;
            TypeCol.Width = 57;
            // 
            // NameCol
            // 
            NameCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            NameCol.HeaderText = "Name";
            NameCol.Name = "NameCol";
            NameCol.ReadOnly = true;
            // 
            // StatusCol
            // 
            StatusCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            StatusCol.HeaderText = "Status";
            StatusCol.Name = "StatusCol";
            StatusCol.ReadOnly = true;
            StatusCol.Width = 64;
            // 
            // flpPackageOptions
            // 
            flpPackageOptions.Controls.Add(btnRemove);
            flpPackageOptions.Controls.Add(btnUninstall);
            flpPackageOptions.Controls.Add(btnReinstall);
            flpPackageOptions.Controls.Add(btnInstallPackage);
            flpPackageOptions.Controls.Add(btnLockPackage);
            flpPackageOptions.Controls.Add(btnFavPackage);
            flpPackageOptions.Dock = DockStyle.Bottom;
            flpPackageOptions.FlowDirection = FlowDirection.RightToLeft;
            flpPackageOptions.Location = new Point(5, 328);
            flpPackageOptions.Name = "flpPackageOptions";
            flpPackageOptions.Size = new Size(496, 26);
            flpPackageOptions.TabIndex = 4;
            // 
            // btnRemove
            // 
            btnRemove.Location = new Point(402, 3);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new Size(91, 23);
            btnRemove.TabIndex = 14;
            btnRemove.Tag = "Tidy up your list! Remove the selected packages from the game and Bundlingway.";
            btnRemove.Text = "Remove";
            btnRemove.UseVisualStyleBackColor = true;
            btnRemove.Click += btnRemove_Click;
            // 
            // btnUninstall
            // 
            btnUninstall.Location = new Point(305, 3);
            btnUninstall.Name = "btnUninstall";
            btnUninstall.Size = new Size(91, 23);
            btnUninstall.TabIndex = 13;
            btnUninstall.Tag = "Remove the selected packages from your game installation, but keep'em handy if you decide to reinstall!";
            btnUninstall.Text = "Uninstall";
            btnUninstall.UseVisualStyleBackColor = true;
            btnUninstall.Click += btnUninstall_Click;
            // 
            // btnReinstall
            // 
            btnReinstall.Location = new Point(208, 3);
            btnReinstall.Name = "btnReinstall";
            btnReinstall.Size = new Size(91, 23);
            btnReinstall.TabIndex = 12;
            btnReinstall.Tag = "Reinstall the selected packages. Useful if something seems broken or if you want a fresh start.";
            btnReinstall.Text = "Reinstall";
            btnReinstall.UseVisualStyleBackColor = true;
            btnReinstall.Click += btnReinstall_Click;
            // 
            // btnInstallPackage
            // 
            btnInstallPackage.Location = new Point(111, 3);
            btnInstallPackage.Name = "btnInstallPackage";
            btnInstallPackage.Size = new Size(91, 23);
            btnInstallPackage.TabIndex = 11;
            btnInstallPackage.Tag = "Ready for more beautiful graphics? Click here to add new packages to the game!";
            btnInstallPackage.Text = "Add Package";
            btnInstallPackage.UseVisualStyleBackColor = true;
            btnInstallPackage.Click += btnInstallPackage_Click;
            // 
            // btnLockPackage
            // 
            btnLockPackage.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnLockPackage.Location = new Point(81, 3);
            btnLockPackage.Name = "btnLockPackage";
            btnLockPackage.Size = new Size(24, 23);
            btnLockPackage.TabIndex = 16;
            btnLockPackage.Tag = "Lock Package: this toggle will keep the selected packages safe and sound! This will disable the uninstall and remove options.";
            btnLockPackage.Text = "🔒";
            btnLockPackage.UseVisualStyleBackColor = true;
            btnLockPackage.Click += btnLockPackage_Click;
            // 
            // btnFavPackage
            // 
            btnFavPackage.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnFavPackage.Location = new Point(51, 3);
            btnFavPackage.Name = "btnFavPackage";
            btnFavPackage.Size = new Size(24, 23);
            btnFavPackage.TabIndex = 15;
            btnFavPackage.Tag = "Favorite: Add the selected packages to your favorites list for quick access. A Loporrit-approved way to keep track of your top picks!";
            btnFavPackage.Text = "★";
            btnFavPackage.UseVisualStyleBackColor = true;
            btnFavPackage.Click += btnFavPackage_Click;
            // 
            // lblGrpPackages
            // 
            lblGrpPackages.BackColor = SystemColors.Highlight;
            lblGrpPackages.Dock = DockStyle.Top;
            lblGrpPackages.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblGrpPackages.ForeColor = SystemColors.Control;
            lblGrpPackages.Location = new Point(5, 5);
            lblGrpPackages.Name = "lblGrpPackages";
            lblGrpPackages.Padding = new Padding(3);
            lblGrpPackages.Size = new Size(496, 25);
            lblGrpPackages.TabIndex = 0;
            lblGrpPackages.Text = "Packages";
            // 
            // pnlSettings
            // 
            pnlSettings.Controls.Add(btnTopMost);
            pnlSettings.Controls.Add(tableLayoutPanel1);
            pnlSettings.Controls.Add(lblGrpSettings);
            pnlSettings.Dock = DockStyle.Top;
            pnlSettings.Location = new Point(0, 0);
            pnlSettings.Name = "pnlSettings";
            pnlSettings.Padding = new Padding(5);
            pnlSettings.Size = new Size(506, 132);
            pnlSettings.TabIndex = 0;
            // 
            // btnTopMost
            // 
            btnTopMost.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTopMost.BackColor = SystemColors.Highlight;
            btnTopMost.FlatAppearance.BorderSize = 0;
            btnTopMost.FlatStyle = FlatStyle.Flat;
            btnTopMost.Font = new Font("Segoe UI", 9.1F, FontStyle.Bold);
            btnTopMost.ForeColor = SystemColors.Control;
            btnTopMost.Location = new Point(477, 6);
            btnTopMost.Margin = new Padding(0);
            btnTopMost.Name = "btnTopMost";
            btnTopMost.Size = new Size(23, 23);
            btnTopMost.TabIndex = 17;
            btnTopMost.Tag = "Pin this window over all others. Pretty handy sometimes!";
            btnTopMost.Text = "📌";
            btnTopMost.UseVisualStyleBackColor = false;
            btnTopMost.Click += btnTopMost_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
            tableLayoutPanel1.Controls.Add(btnInstallGPosingway, 2, 2);
            tableLayoutPanel1.Controls.Add(txtGPosingwayStatus, 1, 2);
            tableLayoutPanel1.Controls.Add(label4, 0, 2);
            tableLayoutPanel1.Controls.Add(btnInstallReShade, 2, 1);
            tableLayoutPanel1.Controls.Add(txtReShadeStatus, 1, 1);
            tableLayoutPanel1.Controls.Add(label3, 0, 1);
            tableLayoutPanel1.Controls.Add(label2, 0, 0);
            tableLayoutPanel1.Controls.Add(txtXivPath, 1, 0);
            tableLayoutPanel1.Controls.Add(btnDetectSettings, 2, 0);
            tableLayoutPanel1.Dock = DockStyle.Top;
            tableLayoutPanel1.Location = new Point(5, 30);
            tableLayoutPanel1.Margin = new Padding(3, 10, 3, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.Padding = new Padding(5);
            tableLayoutPanel1.RowCount = 6;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(496, 101);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // btnInstallGPosingway
            // 
            btnInstallGPosingway.Dock = DockStyle.Fill;
            btnInstallGPosingway.Location = new Point(434, 66);
            btnInstallGPosingway.Name = "btnInstallGPosingway";
            btnInstallGPosingway.Size = new Size(54, 23);
            btnInstallGPosingway.TabIndex = 9;
            btnInstallGPosingway.Text = "Install";
            btnInstallGPosingway.UseVisualStyleBackColor = true;
            btnInstallGPosingway.Visible = false;
            btnInstallGPosingway.Click += btnInstallGPosingway_Click;
            // 
            // txtGPosingwayStatus
            // 
            txtGPosingwayStatus.BorderStyle = BorderStyle.None;
            txtGPosingwayStatus.Dock = DockStyle.Fill;
            txtGPosingwayStatus.Location = new Point(148, 70);
            txtGPosingwayStatus.Margin = new Padding(3, 7, 3, 3);
            txtGPosingwayStatus.Name = "txtGPosingwayStatus";
            txtGPosingwayStatus.ReadOnly = true;
            txtGPosingwayStatus.Size = new Size(280, 16);
            txtGPosingwayStatus.TabIndex = 41;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Dock = DockStyle.Fill;
            label4.Location = new Point(8, 63);
            label4.Name = "label4";
            label4.Size = new Size(134, 29);
            label4.TabIndex = 6;
            label4.Text = "GPosingway Collection";
            label4.TextAlign = ContentAlignment.MiddleRight;
            // 
            // btnInstallReShade
            // 
            btnInstallReShade.Dock = DockStyle.Fill;
            btnInstallReShade.Location = new Point(434, 37);
            btnInstallReShade.Name = "btnInstallReShade";
            btnInstallReShade.Size = new Size(54, 23);
            btnInstallReShade.TabIndex = 8;
            btnInstallReShade.Tag = "You need to shut down the game client before you can update!";
            btnInstallReShade.Text = "Install";
            btnInstallReShade.UseVisualStyleBackColor = true;
            btnInstallReShade.Visible = false;
            btnInstallReShade.Click += btnInstallReShade_Click;
            btnInstallReShade.MouseEnter += btnInstallReShade_MouseEnter;
            // 
            // txtReShadeStatus
            // 
            txtReShadeStatus.BorderStyle = BorderStyle.None;
            txtReShadeStatus.Dock = DockStyle.Fill;
            txtReShadeStatus.Location = new Point(148, 41);
            txtReShadeStatus.Margin = new Padding(3, 7, 3, 3);
            txtReShadeStatus.Name = "txtReShadeStatus";
            txtReShadeStatus.ReadOnly = true;
            txtReShadeStatus.Size = new Size(280, 16);
            txtReShadeStatus.TabIndex = 40;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = DockStyle.Fill;
            label3.Location = new Point(8, 34);
            label3.Name = "label3";
            label3.Size = new Size(134, 29);
            label3.TabIndex = 3;
            label3.Text = "ReShade";
            label3.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = DockStyle.Fill;
            label2.Location = new Point(8, 5);
            label2.Name = "label2";
            label2.Size = new Size(134, 29);
            label2.TabIndex = 0;
            label2.Text = "FFXIV Path";
            label2.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtXivPath
            // 
            txtXivPath.BackColor = SystemColors.Control;
            txtXivPath.BorderStyle = BorderStyle.None;
            txtXivPath.Dock = DockStyle.Fill;
            txtXivPath.Location = new Point(148, 12);
            txtXivPath.Margin = new Padding(3, 7, 3, 3);
            txtXivPath.Name = "txtXivPath";
            txtXivPath.Size = new Size(280, 16);
            txtXivPath.TabIndex = 1;
            // 
            // btnDetectSettings
            // 
            btnDetectSettings.Location = new Point(434, 8);
            btnDetectSettings.Name = "btnDetectSettings";
            btnDetectSettings.Size = new Size(54, 23);
            btnDetectSettings.TabIndex = 7;
            btnDetectSettings.Tag = "Let Bundlingway sniff out your game installation, ReShade and GPosingway settings automatically!";
            btnDetectSettings.Text = "Detect";
            btnDetectSettings.UseVisualStyleBackColor = true;
            btnDetectSettings.Click += btnDetectSettings_Click;
            // 
            // lblGrpSettings
            // 
            lblGrpSettings.BackColor = SystemColors.Highlight;
            lblGrpSettings.Dock = DockStyle.Top;
            lblGrpSettings.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblGrpSettings.ForeColor = SystemColors.Control;
            lblGrpSettings.Location = new Point(5, 5);
            lblGrpSettings.Name = "lblGrpSettings";
            lblGrpSettings.Padding = new Padding(3);
            lblGrpSettings.Size = new Size(496, 25);
            lblGrpSettings.TabIndex = 0;
            lblGrpSettings.Text = "Settings";
            // 
            // frmLanding
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(661, 636);
            Controls.Add(splitContainer1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(599, 555);
            Name = "frmLanding";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Bundlingway Package Manager";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            flpSideMenu.ResumeLayout(false);
            pnlAbout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            pnlPackages.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvPackages).EndInit();
            flpPackageOptions.ResumeLayout(false);
            pnlSettings.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)resourcePackageBindingSource).EndInit();
            ((System.ComponentModel.ISupportInitialize)instancesBindingSource).EndInit();
            ((System.ComponentModel.ISupportInitialize)instancesBindingSource1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private ContextMenuStrip contextMenuStrip1;
        private SplitContainer splitContainer1;
        private FlowLayoutPanel flpSideMenu;
        private Panel pnlSettings;
        private Label lblGrpSettings;
        private TableLayoutPanel tableLayoutPanel1;
        private Label label2;
        private TextBox txtXivPath;
        private Button btnInstallReShade;
        private TextBox txtReShadeStatus;
        private Button btnDetectSettings;
        private Button btnInstallGPosingway;
        private TextBox txtGPosingwayStatus;
        private Label label4;
        private Label label3;
        private Panel pnlPackages;
        private Label lblGrpPackages;
        private Panel pnlAbout;
        private Label lblBundlingwaySays;
        private DataGridView dgvPackages;
        private BindingSource instancesBindingSource;
        private BindingSource resourcePackageBindingSource;
        private BindingSource instancesBindingSource1;
        private FlowLayoutPanel flpPackageOptions;
        private Button btnInstallPackage;
        private Button btnRemove;
        private Button btnReinstall;
        private Button btnUninstall;
        private FontAwesome.Sharp.IconButton btnPackagesFolder;
        private FontAwesome.Sharp.IconButton btnGameFolder;
        private FontAwesome.Sharp.IconButton btnAbout;
        private Label lblAnnouncement;
        private PictureBox pictureBox1;
        private FontAwesome.Sharp.IconButton btnDebug;
        private FontAwesome.Sharp.IconButton btnEmporium;
        private Label label5;
        private FontAwesome.Sharp.IconButton btnUpdate;
        private Button btnFavPackage;
        private FontAwesome.Sharp.IconButton btnShortcuts;
        private FontAwesome.Sharp.IconButton btnBackup;
        private Label label1;
        private Label label6;
        private DataGridViewTextBoxColumn FavCol;
        private DataGridViewTextBoxColumn TypeCol;
        private DataGridViewTextBoxColumn NameCol;
        private DataGridViewTextBoxColumn StatusCol;
        private Button btnLockPackage;
        private ProgressBar prgCommon;
        private FontAwesome.Sharp.IconButton btnFixIt;
        private Button btnTopMost;
    }
}
