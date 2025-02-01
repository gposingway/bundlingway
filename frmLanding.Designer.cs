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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLanding));
            contextMenuStrip1 = new ContextMenuStrip(components);
            splitContainer1 = new SplitContainer();
            flpSideMenu = new FlowLayoutPanel();
            btnPackagesFolder = new FontAwesome.Sharp.IconButton();
            btnGameFolder = new FontAwesome.Sharp.IconButton();
            btnDebug = new FontAwesome.Sharp.IconButton();
            lblSpacing1 = new Label();
            btnAbout = new FontAwesome.Sharp.IconButton();
            pnlAbout = new Panel();
            lblAnnouncement = new Label();
            pictureBox1 = new PictureBox();
            lblBundlingwaySays = new Label();
            pnlPackages = new Panel();
            dgvPackages = new DataGridView();
            TypeCol = new DataGridViewTextBoxColumn();
            NameCol = new DataGridViewTextBoxColumn();
            StatusCol = new DataGridViewTextBoxColumn();
            flpPackageOptions = new FlowLayoutPanel();
            btnRemove = new Button();
            btnUninstall = new Button();
            btnReinstall = new Button();
            btnInstallPackage = new Button();
            lblGrpPackages = new Label();
            pnlSettings = new Panel();
            tableLayoutPanel1 = new TableLayoutPanel();
            btnInstallGPosingway = new Button();
            textBox3 = new TextBox();
            mainSource = new BindingSource(components);
            label4 = new Label();
            btnInstallReShade = new Button();
            textBox2 = new TextBox();
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
            ((System.ComponentModel.ISupportInitialize)mainSource).BeginInit();
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
            splitContainer1.Location = new Point(0, 0);
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
            splitContainer1.Size = new Size(715, 701);
            splitContainer1.SplitterDistance = 151;
            splitContainer1.TabIndex = 1;
            // 
            // flpSideMenu
            // 
            flpSideMenu.AllowDrop = true;
            flpSideMenu.AutoScroll = true;
            flpSideMenu.BackColor = SystemColors.ControlLight;
            flpSideMenu.Controls.Add(btnPackagesFolder);
            flpSideMenu.Controls.Add(btnGameFolder);
            flpSideMenu.Controls.Add(btnDebug);
            flpSideMenu.Controls.Add(lblSpacing1);
            flpSideMenu.Controls.Add(btnAbout);
            flpSideMenu.Dock = DockStyle.Fill;
            flpSideMenu.Location = new Point(0, 0);
            flpSideMenu.Name = "flpSideMenu";
            flpSideMenu.Size = new Size(151, 701);
            flpSideMenu.TabIndex = 0;
            flpSideMenu.DragDrop += Generic_DragDrop;
            flpSideMenu.DragEnter += Generic_DragEnter;
            // 
            // btnPackagesFolder
            // 
            btnPackagesFolder.IconChar = FontAwesome.Sharp.IconChar.FolderOpen;
            btnPackagesFolder.IconColor = Color.SkyBlue;
            btnPackagesFolder.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnPackagesFolder.IconSize = 32;
            btnPackagesFolder.ImageAlign = ContentAlignment.MiddleLeft;
            btnPackagesFolder.Location = new Point(3, 3);
            btnPackagesFolder.Margin = new Padding(3, 3, 3, 0);
            btnPackagesFolder.Name = "btnPackagesFolder";
            btnPackagesFolder.Size = new Size(145, 37);
            btnPackagesFolder.TabIndex = 8;
            btnPackagesFolder.Text = "Repository";
            btnPackagesFolder.TextAlign = ContentAlignment.MiddleRight;
            btnPackagesFolder.UseVisualStyleBackColor = true;
            btnPackagesFolder.Click += btnPackagesFolder_Click;
            // 
            // btnGameFolder
            // 
            btnGameFolder.IconChar = FontAwesome.Sharp.IconChar.FolderOpen;
            btnGameFolder.IconColor = Color.SkyBlue;
            btnGameFolder.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnGameFolder.IconSize = 32;
            btnGameFolder.ImageAlign = ContentAlignment.MiddleLeft;
            btnGameFolder.Location = new Point(3, 40);
            btnGameFolder.Margin = new Padding(3, 0, 3, 0);
            btnGameFolder.Name = "btnGameFolder";
            btnGameFolder.Size = new Size(145, 37);
            btnGameFolder.TabIndex = 9;
            btnGameFolder.Text = "FFXIV Game Folder";
            btnGameFolder.TextAlign = ContentAlignment.MiddleRight;
            btnGameFolder.UseVisualStyleBackColor = true;
            btnGameFolder.Click += btnGameFolder_Click;
            // 
            // btnDebug
            // 
            btnDebug.IconChar = FontAwesome.Sharp.IconChar.ChevronRight;
            btnDebug.IconColor = Color.SkyBlue;
            btnDebug.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnDebug.IconSize = 32;
            btnDebug.ImageAlign = ContentAlignment.MiddleLeft;
            btnDebug.Location = new Point(3, 77);
            btnDebug.Margin = new Padding(3, 0, 3, 0);
            btnDebug.Name = "btnDebug";
            btnDebug.Size = new Size(145, 37);
            btnDebug.TabIndex = 13;
            btnDebug.Text = "Debug Information";
            btnDebug.TextAlign = ContentAlignment.MiddleRight;
            btnDebug.UseVisualStyleBackColor = true;
            btnDebug.Click += btnDebug_Click;
            // 
            // lblSpacing1
            // 
            lblSpacing1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblSpacing1.AutoSize = true;
            lblSpacing1.Font = new Font("Segoe UI", 12F);
            lblSpacing1.Location = new Point(3, 114);
            lblSpacing1.Name = "lblSpacing1";
            lblSpacing1.Size = new Size(0, 21);
            lblSpacing1.TabIndex = 6;
            // 
            // btnAbout
            // 
            btnAbout.IconChar = FontAwesome.Sharp.IconChar.Carrot;
            btnAbout.IconColor = Color.SkyBlue;
            btnAbout.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnAbout.IconSize = 32;
            btnAbout.ImageAlign = ContentAlignment.MiddleLeft;
            btnAbout.Location = new Point(3, 135);
            btnAbout.Margin = new Padding(3, 0, 3, 0);
            btnAbout.Name = "btnAbout";
            btnAbout.Size = new Size(145, 37);
            btnAbout.TabIndex = 12;
            btnAbout.Text = "About";
            btnAbout.TextAlign = ContentAlignment.MiddleRight;
            btnAbout.UseVisualStyleBackColor = true;
            // 
            // pnlAbout
            // 
            pnlAbout.Controls.Add(lblAnnouncement);
            pnlAbout.Controls.Add(pictureBox1);
            pnlAbout.Controls.Add(lblBundlingwaySays);
            pnlAbout.Dock = DockStyle.Bottom;
            pnlAbout.Location = new Point(0, 551);
            pnlAbout.Name = "pnlAbout";
            pnlAbout.Padding = new Padding(5);
            pnlAbout.Size = new Size(560, 150);
            pnlAbout.TabIndex = 3;
            // 
            // lblAnnouncement
            // 
            lblAnnouncement.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblAnnouncement.FlatStyle = FlatStyle.Flat;
            lblAnnouncement.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblAnnouncement.Location = new Point(26, 55);
            lblAnnouncement.Name = "lblAnnouncement";
            lblAnnouncement.Size = new Size(392, 70);
            lblAnnouncement.TabIndex = 2;
            lblAnnouncement.Text = "Gathering my tools, one sec...";
            lblAnnouncement.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pictureBox1
            // 
            pictureBox1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pictureBox1.Image = Properties.Resources.ffxiv_dx11_OkamiClarity_2024_02_18_16_07_46_icon;
            pictureBox1.Location = new Point(443, 33);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(117, 117);
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // lblBundlingwaySays
            // 
            lblBundlingwaySays.BackColor = SystemColors.ControlDarkDark;
            lblBundlingwaySays.Dock = DockStyle.Top;
            lblBundlingwaySays.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblBundlingwaySays.ForeColor = SystemColors.ButtonHighlight;
            lblBundlingwaySays.Location = new Point(5, 5);
            lblBundlingwaySays.Name = "lblBundlingwaySays";
            lblBundlingwaySays.Padding = new Padding(3);
            lblBundlingwaySays.Size = new Size(550, 25);
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
            pnlPackages.Size = new Size(560, 569);
            pnlPackages.TabIndex = 1;
            // 
            // dgvPackages
            // 
            dgvPackages.AllowDrop = true;
            dgvPackages.AllowUserToAddRows = false;
            dgvPackages.AllowUserToDeleteRows = false;
            dgvPackages.BackgroundColor = SystemColors.Control;
            dgvPackages.BorderStyle = BorderStyle.None;
            dgvPackages.CausesValidation = false;
            dgvPackages.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPackages.Columns.AddRange(new DataGridViewColumn[] { TypeCol, NameCol, StatusCol });
            dgvPackages.ContextMenuStrip = contextMenuStrip1;
            dgvPackages.Dock = DockStyle.Fill;
            dgvPackages.Location = new Point(5, 30);
            dgvPackages.Name = "dgvPackages";
            dgvPackages.ReadOnly = true;
            dgvPackages.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPackages.Size = new Size(550, 363);
            dgvPackages.TabIndex = 1;
            dgvPackages.DragDrop += Generic_DragDrop;
            dgvPackages.DragEnter += Generic_DragEnter;
            dgvPackages.DragOver += Generic_DragOver;
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
            flpPackageOptions.Dock = DockStyle.Bottom;
            flpPackageOptions.FlowDirection = FlowDirection.RightToLeft;
            flpPackageOptions.Location = new Point(5, 393);
            flpPackageOptions.Name = "flpPackageOptions";
            flpPackageOptions.Size = new Size(550, 26);
            flpPackageOptions.TabIndex = 4;
            // 
            // btnRemove
            // 
            btnRemove.Location = new Point(456, 3);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new Size(91, 23);
            btnRemove.TabIndex = 5;
            btnRemove.Text = "Remove";
            btnRemove.UseVisualStyleBackColor = true;
            btnRemove.Click += btnRemove_Click;
            // 
            // btnUninstall
            // 
            btnUninstall.Location = new Point(359, 3);
            btnUninstall.Name = "btnUninstall";
            btnUninstall.Size = new Size(91, 23);
            btnUninstall.TabIndex = 7;
            btnUninstall.Text = "Uninstall";
            btnUninstall.UseVisualStyleBackColor = true;
            btnUninstall.Click += btnUninstall_Click;
            // 
            // btnReinstall
            // 
            btnReinstall.Location = new Point(262, 3);
            btnReinstall.Name = "btnReinstall";
            btnReinstall.Size = new Size(91, 23);
            btnReinstall.TabIndex = 6;
            btnReinstall.Text = "Reinstall";
            btnReinstall.UseVisualStyleBackColor = true;
            btnReinstall.Click += btnReinstall_Click;
            // 
            // btnInstallPackage
            // 
            btnInstallPackage.Location = new Point(165, 3);
            btnInstallPackage.Name = "btnInstallPackage";
            btnInstallPackage.Size = new Size(91, 23);
            btnInstallPackage.TabIndex = 4;
            btnInstallPackage.Text = "Add Package";
            btnInstallPackage.UseVisualStyleBackColor = true;
            btnInstallPackage.Click += btnInstallPackage_Click;
            // 
            // lblGrpPackages
            // 
            lblGrpPackages.BackColor = SystemColors.ControlDarkDark;
            lblGrpPackages.Dock = DockStyle.Top;
            lblGrpPackages.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblGrpPackages.ForeColor = SystemColors.ButtonHighlight;
            lblGrpPackages.Location = new Point(5, 5);
            lblGrpPackages.Name = "lblGrpPackages";
            lblGrpPackages.Padding = new Padding(3);
            lblGrpPackages.Size = new Size(550, 25);
            lblGrpPackages.TabIndex = 0;
            lblGrpPackages.Text = "Packages";
            // 
            // pnlSettings
            // 
            pnlSettings.Controls.Add(tableLayoutPanel1);
            pnlSettings.Controls.Add(lblGrpSettings);
            pnlSettings.Dock = DockStyle.Top;
            pnlSettings.Location = new Point(0, 0);
            pnlSettings.Name = "pnlSettings";
            pnlSettings.Padding = new Padding(5);
            pnlSettings.Size = new Size(560, 132);
            pnlSettings.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
            tableLayoutPanel1.Controls.Add(btnInstallGPosingway, 2, 2);
            tableLayoutPanel1.Controls.Add(textBox3, 1, 2);
            tableLayoutPanel1.Controls.Add(label4, 0, 2);
            tableLayoutPanel1.Controls.Add(btnInstallReShade, 2, 1);
            tableLayoutPanel1.Controls.Add(textBox2, 1, 1);
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
            tableLayoutPanel1.Size = new Size(550, 101);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // btnInstallGPosingway
            // 
            btnInstallGPosingway.Dock = DockStyle.Fill;
            btnInstallGPosingway.Location = new Point(488, 66);
            btnInstallGPosingway.Name = "btnInstallGPosingway";
            btnInstallGPosingway.Size = new Size(54, 23);
            btnInstallGPosingway.TabIndex = 8;
            btnInstallGPosingway.Text = "Install";
            btnInstallGPosingway.UseVisualStyleBackColor = true;
            btnInstallGPosingway.Visible = false;
            btnInstallGPosingway.Click += btnInstallGPosingway_Click;
            // 
            // textBox3
            // 
            textBox3.BorderStyle = BorderStyle.None;
            textBox3.DataBindings.Add(new Binding("Text", mainSource, "GPosingway.Status", true));
            textBox3.Dock = DockStyle.Fill;
            textBox3.Location = new Point(148, 70);
            textBox3.Margin = new Padding(3, 7, 3, 3);
            textBox3.Name = "textBox3";
            textBox3.ReadOnly = true;
            textBox3.Size = new Size(334, 16);
            textBox3.TabIndex = 7;
            textBox3.Text = "Not Installed";
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
            btnInstallReShade.DataBindings.Add(new Binding("Visible", mainSource, "ReShade.IsMissing", true));
            btnInstallReShade.Dock = DockStyle.Fill;
            btnInstallReShade.Location = new Point(488, 37);
            btnInstallReShade.Name = "btnInstallReShade";
            btnInstallReShade.Size = new Size(54, 23);
            btnInstallReShade.TabIndex = 5;
            btnInstallReShade.Text = "Install";
            btnInstallReShade.UseVisualStyleBackColor = true;
            btnInstallReShade.Visible = false;
            btnInstallReShade.Click += btnInstallReShade_Click;
            // 
            // textBox2
            // 
            textBox2.BorderStyle = BorderStyle.None;
            textBox2.DataBindings.Add(new Binding("Text", mainSource, "ReShade.Status", true));
            textBox2.Dock = DockStyle.Fill;
            textBox2.Location = new Point(148, 41);
            textBox2.Margin = new Padding(3, 7, 3, 3);
            textBox2.Name = "textBox2";
            textBox2.ReadOnly = true;
            textBox2.Size = new Size(334, 16);
            textBox2.TabIndex = 4;
            textBox2.Text = "Not Installed";
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
            txtXivPath.BorderStyle = BorderStyle.FixedSingle;
            txtXivPath.DataBindings.Add(new Binding("DataContext", mainSource, "XIVPath", true));
            txtXivPath.DataBindings.Add(new Binding("Text", mainSource, "XIVPath", true));
            txtXivPath.Dock = DockStyle.Fill;
            txtXivPath.Location = new Point(148, 8);
            txtXivPath.Name = "txtXivPath";
            txtXivPath.Size = new Size(334, 23);
            txtXivPath.TabIndex = 1;
            // 
            // btnDetectSettings
            // 
            btnDetectSettings.Location = new Point(488, 8);
            btnDetectSettings.Name = "btnDetectSettings";
            btnDetectSettings.Size = new Size(54, 23);
            btnDetectSettings.TabIndex = 2;
            btnDetectSettings.Text = "Detect";
            btnDetectSettings.UseVisualStyleBackColor = true;
            btnDetectSettings.Click += btnDetectSettings_Click;
            // 
            // lblGrpSettings
            // 
            lblGrpSettings.BackColor = SystemColors.ControlDarkDark;
            lblGrpSettings.Dock = DockStyle.Top;
            lblGrpSettings.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblGrpSettings.ForeColor = SystemColors.ButtonHighlight;
            lblGrpSettings.Location = new Point(5, 5);
            lblGrpSettings.Name = "lblGrpSettings";
            lblGrpSettings.Padding = new Padding(3);
            lblGrpSettings.Size = new Size(550, 25);
            lblGrpSettings.TabIndex = 0;
            lblGrpSettings.Text = "Settings";
            // 
            // frmLanding
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(715, 701);
            Controls.Add(splitContainer1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(575, 551);
            Name = "frmLanding";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Bundlingway Package Manager";
            Load += frmLanding_Load;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            flpSideMenu.ResumeLayout(false);
            flpSideMenu.PerformLayout();
            pnlAbout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            pnlPackages.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvPackages).EndInit();
            flpPackageOptions.ResumeLayout(false);
            pnlSettings.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)mainSource).EndInit();
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
        private TextBox textBox2;
        private Button btnDetectSettings;
        private Button btnInstallGPosingway;
        private TextBox textBox3;
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
        private BindingSource mainSource;
        private FlowLayoutPanel flpPackageOptions;
        private Button btnInstallPackage;
        private Button btnRemove;
        private Button btnReinstall;
        private DataGridViewTextBoxColumn TypeCol;
        private DataGridViewTextBoxColumn NameCol;
        private DataGridViewTextBoxColumn StatusCol;
        private Button btnUninstall;
        private Label lblSpacing1;
        private FontAwesome.Sharp.IconButton btnPackagesFolder;
        private FontAwesome.Sharp.IconButton btnGameFolder;
        private FontAwesome.Sharp.IconButton btnAbout;
        private Label lblAnnouncement;
        private PictureBox pictureBox1;
        private FontAwesome.Sharp.IconButton btnDebug;
    }
}
