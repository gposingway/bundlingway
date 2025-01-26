namespace Bundlingway
{
    partial class Landing
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
            contextMenuStrip1 = new ContextMenuStrip(components);
            splitContainer1 = new SplitContainer();
            flpSideMenu = new FlowLayoutPanel();
            btnSettings = new Button();
            btnDownloads = new Button();
            btnAbout = new Button();
            pnlAbout = new Panel();
            tableLayoutPanel4 = new TableLayoutPanel();
            label19 = new Label();
            lblCopyright = new Label();
            label21 = new Label();
            label22 = new Label();
            label23 = new Label();
            lblGPosingwayVersion = new Label();
            linkLabel3 = new LinkLabel();
            label25 = new Label();
            pnlPackages = new Panel();
            flpPackageOptions = new FlowLayoutPanel();
            btnRemove = new Button();
            btnUninstall = new Button();
            btnReinstall = new Button();
            btnInstallPackage = new Button();
            dgvPackages = new DataGridView();
            TypeCol = new DataGridViewTextBoxColumn();
            NameCol = new DataGridViewTextBoxColumn();
            StatusCol = new DataGridViewTextBoxColumn();
            label8 = new Label();
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
            label1 = new Label();
            resourcePackageBindingSource = new BindingSource(components);
            instancesBindingSource = new BindingSource(components);
            instancesBindingSource1 = new BindingSource(components);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            flpSideMenu.SuspendLayout();
            pnlAbout.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            pnlPackages.SuspendLayout();
            flpPackageOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPackages).BeginInit();
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
            splitContainer1.Size = new Size(747, 482);
            splitContainer1.SplitterDistance = 153;
            splitContainer1.TabIndex = 1;
            // 
            // flpSideMenu
            // 
            flpSideMenu.AllowDrop = true;
            flpSideMenu.AutoScroll = true;
            flpSideMenu.BackColor = SystemColors.ControlLight;
            flpSideMenu.Controls.Add(btnSettings);
            flpSideMenu.Controls.Add(btnDownloads);
            flpSideMenu.Controls.Add(btnAbout);
            flpSideMenu.Dock = DockStyle.Fill;
            flpSideMenu.Location = new Point(0, 0);
            flpSideMenu.Name = "flpSideMenu";
            flpSideMenu.Size = new Size(153, 482);
            flpSideMenu.TabIndex = 0;
            flpSideMenu.DragDrop += Generic_DragDrop;
            flpSideMenu.DragEnter += Generic_DragEnter;
            // 
            // btnSettings
            // 
            btnSettings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnSettings.FlatAppearance.BorderSize = 0;
            btnSettings.FlatStyle = FlatStyle.Flat;
            btnSettings.Location = new Point(3, 3);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(145, 23);
            btnSettings.TabIndex = 4;
            btnSettings.Text = "Settings";
            btnSettings.UseVisualStyleBackColor = true;
            btnSettings.Click += btnSettings_Click;
            // 
            // btnDownloads
            // 
            btnDownloads.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnDownloads.FlatAppearance.BorderSize = 0;
            btnDownloads.FlatStyle = FlatStyle.Flat;
            btnDownloads.Location = new Point(3, 32);
            btnDownloads.Name = "btnDownloads";
            btnDownloads.Size = new Size(145, 23);
            btnDownloads.TabIndex = 1;
            btnDownloads.Text = "Packages";
            btnDownloads.UseVisualStyleBackColor = true;
            btnDownloads.Click += btnDownloads_Click;
            // 
            // btnAbout
            // 
            btnAbout.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnAbout.FlatAppearance.BorderSize = 0;
            btnAbout.FlatStyle = FlatStyle.Flat;
            btnAbout.Location = new Point(3, 61);
            btnAbout.Name = "btnAbout";
            btnAbout.Size = new Size(145, 23);
            btnAbout.TabIndex = 5;
            btnAbout.Text = "About";
            btnAbout.UseVisualStyleBackColor = true;
            btnAbout.Click += btnAbout_Click;
            // 
            // pnlAbout
            // 
            pnlAbout.Controls.Add(tableLayoutPanel4);
            pnlAbout.Controls.Add(label25);
            pnlAbout.Dock = DockStyle.Bottom;
            pnlAbout.Location = new Point(0, 308);
            pnlAbout.Name = "pnlAbout";
            pnlAbout.Padding = new Padding(5);
            pnlAbout.Size = new Size(590, 174);
            pnlAbout.TabIndex = 3;
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.ColumnCount = 3;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
            tableLayoutPanel4.Controls.Add(label19, 0, 2);
            tableLayoutPanel4.Controls.Add(lblCopyright, 1, 2);
            tableLayoutPanel4.Controls.Add(label21, 0, 4);
            tableLayoutPanel4.Controls.Add(label22, 1, 3);
            tableLayoutPanel4.Controls.Add(label23, 0, 3);
            tableLayoutPanel4.Controls.Add(lblGPosingwayVersion, 1, 0);
            tableLayoutPanel4.Controls.Add(linkLabel3, 1, 4);
            tableLayoutPanel4.Dock = DockStyle.Fill;
            tableLayoutPanel4.Location = new Point(5, 30);
            tableLayoutPanel4.Margin = new Padding(3, 10, 3, 3);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.Padding = new Padding(5);
            tableLayoutPanel4.RowCount = 6;
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.Size = new Size(580, 139);
            tableLayoutPanel4.TabIndex = 1;
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Dock = DockStyle.Fill;
            label19.Location = new Point(8, 56);
            label19.Name = "label19";
            label19.Size = new Size(134, 20);
            label19.TabIndex = 13;
            label19.Text = "Copyright";
            label19.TextAlign = ContentAlignment.TopRight;
            // 
            // lblCopyright
            // 
            lblCopyright.AutoSize = true;
            lblCopyright.Dock = DockStyle.Fill;
            lblCopyright.Location = new Point(148, 56);
            lblCopyright.Margin = new Padding(3, 0, 3, 5);
            lblCopyright.Name = "lblCopyright";
            lblCopyright.Size = new Size(364, 15);
            lblCopyright.TabIndex = 12;
            lblCopyright.Text = "2023-2024 GPosingway Development Team";
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Dock = DockStyle.Fill;
            label21.Location = new Point(8, 96);
            label21.Margin = new Padding(3, 0, 3, 5);
            label21.Name = "label21";
            label21.Size = new Size(134, 15);
            label21.TabIndex = 11;
            label21.Text = "Project Repository";
            label21.TextAlign = ContentAlignment.TopRight;
            // 
            // label22
            // 
            label22.AutoSize = true;
            label22.Dock = DockStyle.Fill;
            label22.Location = new Point(148, 76);
            label22.Margin = new Padding(3, 0, 3, 5);
            label22.Name = "label22";
            label22.Size = new Size(364, 15);
            label22.TabIndex = 9;
            label22.Text = "Creative Commons Attribution 4.0 International License (CC BY 4.0)";
            // 
            // label23
            // 
            label23.AutoSize = true;
            label23.Dock = DockStyle.Fill;
            label23.Location = new Point(8, 76);
            label23.Name = "label23";
            label23.Size = new Size(134, 20);
            label23.TabIndex = 6;
            label23.Text = "License";
            label23.TextAlign = ContentAlignment.TopRight;
            // 
            // lblGPosingwayVersion
            // 
            lblGPosingwayVersion.AutoSize = true;
            lblGPosingwayVersion.Dock = DockStyle.Fill;
            lblGPosingwayVersion.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblGPosingwayVersion.Location = new Point(148, 5);
            lblGPosingwayVersion.Name = "lblGPosingwayVersion";
            lblGPosingwayVersion.Padding = new Padding(0, 10, 0, 20);
            lblGPosingwayVersion.Size = new Size(364, 51);
            lblGPosingwayVersion.TabIndex = 0;
            lblGPosingwayVersion.Text = "GPosingway 0.0.0.1 (prototype)";
            lblGPosingwayVersion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // linkLabel3
            // 
            linkLabel3.AutoSize = true;
            linkLabel3.Location = new Point(148, 96);
            linkLabel3.Name = "linkLabel3";
            linkLabel3.Size = new Size(247, 15);
            linkLabel3.TabIndex = 10;
            linkLabel3.TabStop = true;
            linkLabel3.Text = "https://github.com/gposingway/gposingway";
            // 
            // label25
            // 
            label25.BackColor = SystemColors.ControlDark;
            label25.Dock = DockStyle.Top;
            label25.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label25.ForeColor = SystemColors.ButtonHighlight;
            label25.Location = new Point(5, 5);
            label25.Name = "label25";
            label25.Padding = new Padding(3);
            label25.Size = new Size(580, 25);
            label25.TabIndex = 0;
            label25.Text = "About";
            // 
            // pnlPackages
            // 
            pnlPackages.Controls.Add(flpPackageOptions);
            pnlPackages.Controls.Add(dgvPackages);
            pnlPackages.Controls.Add(label8);
            pnlPackages.Dock = DockStyle.Fill;
            pnlPackages.Location = new Point(0, 132);
            pnlPackages.Name = "pnlPackages";
            pnlPackages.Padding = new Padding(5, 5, 5, 178);
            pnlPackages.Size = new Size(590, 350);
            pnlPackages.TabIndex = 1;
            // 
            // flpPackageOptions
            // 
            flpPackageOptions.Controls.Add(btnRemove);
            flpPackageOptions.Controls.Add(btnUninstall);
            flpPackageOptions.Controls.Add(btnReinstall);
            flpPackageOptions.Controls.Add(btnInstallPackage);
            flpPackageOptions.Dock = DockStyle.Bottom;
            flpPackageOptions.FlowDirection = FlowDirection.RightToLeft;
            flpPackageOptions.Location = new Point(5, 146);
            flpPackageOptions.Name = "flpPackageOptions";
            flpPackageOptions.Size = new Size(580, 26);
            flpPackageOptions.TabIndex = 4;
            // 
            // btnRemove
            // 
            btnRemove.Location = new Point(486, 3);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new Size(91, 23);
            btnRemove.TabIndex = 5;
            btnRemove.Text = "Remove";
            btnRemove.UseVisualStyleBackColor = true;
            btnRemove.Click += btnRemove_Click;
            // 
            // btnUninstall
            // 
            btnUninstall.Location = new Point(389, 3);
            btnUninstall.Name = "btnUninstall";
            btnUninstall.Size = new Size(91, 23);
            btnUninstall.TabIndex = 7;
            btnUninstall.Text = "Uninstall";
            btnUninstall.UseVisualStyleBackColor = true;
            btnUninstall.Click += btnUninstall_Click;
            // 
            // btnReinstall
            // 
            btnReinstall.Location = new Point(292, 3);
            btnReinstall.Name = "btnReinstall";
            btnReinstall.Size = new Size(91, 23);
            btnReinstall.TabIndex = 6;
            btnReinstall.Text = "Reinstall";
            btnReinstall.UseVisualStyleBackColor = true;
            btnReinstall.Click += btnReinstall_Click;
            // 
            // btnInstallPackage
            // 
            btnInstallPackage.Location = new Point(195, 3);
            btnInstallPackage.Name = "btnInstallPackage";
            btnInstallPackage.Size = new Size(91, 23);
            btnInstallPackage.TabIndex = 4;
            btnInstallPackage.Text = "Add Package";
            btnInstallPackage.UseVisualStyleBackColor = true;
            btnInstallPackage.Click += btnInstallPackage_Click;
            // 
            // dgvPackages
            // 
            dgvPackages.AllowDrop = true;
            dgvPackages.AllowUserToAddRows = false;
            dgvPackages.AllowUserToDeleteRows = false;
            dgvPackages.BackgroundColor = SystemColors.Control;
            dgvPackages.BorderStyle = BorderStyle.Fixed3D;
            dgvPackages.CausesValidation = false;
            dgvPackages.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPackages.Columns.AddRange(new DataGridViewColumn[] { TypeCol, NameCol, StatusCol });
            dgvPackages.ContextMenuStrip = contextMenuStrip1;
            dgvPackages.Dock = DockStyle.Fill;
            dgvPackages.Location = new Point(5, 30);
            dgvPackages.Name = "dgvPackages";
            dgvPackages.ReadOnly = true;
            dgvPackages.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPackages.Size = new Size(580, 142);
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
            // label8
            // 
            label8.BackColor = SystemColors.ControlDark;
            label8.Dock = DockStyle.Top;
            label8.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label8.ForeColor = SystemColors.ButtonHighlight;
            label8.Location = new Point(5, 5);
            label8.Name = "label8";
            label8.Padding = new Padding(3);
            label8.Size = new Size(580, 25);
            label8.TabIndex = 0;
            label8.Text = "Packages";
            // 
            // pnlSettings
            // 
            pnlSettings.Controls.Add(tableLayoutPanel1);
            pnlSettings.Controls.Add(label1);
            pnlSettings.Dock = DockStyle.Top;
            pnlSettings.Location = new Point(0, 0);
            pnlSettings.Name = "pnlSettings";
            pnlSettings.Padding = new Padding(5);
            pnlSettings.Size = new Size(590, 132);
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
            tableLayoutPanel1.Size = new Size(580, 101);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // btnInstallGPosingway
            // 
            btnInstallGPosingway.Dock = DockStyle.Fill;
            btnInstallGPosingway.Location = new Point(518, 66);
            btnInstallGPosingway.Name = "btnInstallGPosingway";
            btnInstallGPosingway.Size = new Size(54, 23);
            btnInstallGPosingway.TabIndex = 8;
            btnInstallGPosingway.Text = "Install";
            btnInstallGPosingway.UseVisualStyleBackColor = true;
            btnInstallGPosingway.Visible = false;
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
            textBox3.Size = new Size(364, 16);
            textBox3.TabIndex = 7;
            textBox3.Text = "Not Installed";
            // 
            // mainSource
            // 
            mainSource.DataSource = typeof(Model.GPosingwayConfig);
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
            btnInstallReShade.Location = new Point(518, 37);
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
            textBox2.Size = new Size(364, 16);
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
            txtXivPath.Size = new Size(364, 23);
            txtXivPath.TabIndex = 1;
            // 
            // btnDetectSettings
            // 
            btnDetectSettings.Location = new Point(518, 8);
            btnDetectSettings.Name = "btnDetectSettings";
            btnDetectSettings.Size = new Size(54, 23);
            btnDetectSettings.TabIndex = 2;
            btnDetectSettings.Text = "Detect";
            btnDetectSettings.UseVisualStyleBackColor = true;
            btnDetectSettings.Click += btnDetectSettings_Click;
            // 
            // label1
            // 
            label1.BackColor = SystemColors.ControlDark;
            label1.Dock = DockStyle.Top;
            label1.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = SystemColors.ButtonHighlight;
            label1.Location = new Point(5, 5);
            label1.Name = "label1";
            label1.Padding = new Padding(3);
            label1.Size = new Size(580, 25);
            label1.TabIndex = 0;
            label1.Text = "Settings";
            // 
            // resourcePackageBindingSource
            // 
            resourcePackageBindingSource.DataSource = typeof(Model.ResourcePackage);
            // 
            // instancesBindingSource
            // 
            instancesBindingSource.DataSource = typeof(Model.Instances);
            // 
            // instancesBindingSource1
            // 
            instancesBindingSource1.DataSource = typeof(Model.Instances);
            // 
            // Landing
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(747, 482);
            Controls.Add(splitContainer1);
            MinimumSize = new Size(763, 521);
            Name = "Landing";
            Text = "GPosingway Package Manager";
            Load += Landing_Load;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            flpSideMenu.ResumeLayout(false);
            pnlAbout.ResumeLayout(false);
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            pnlPackages.ResumeLayout(false);
            flpPackageOptions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvPackages).EndInit();
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
        private Button btnDownloads;
        private Button btnSettings;
        private Button btnAbout;
        private Panel pnlSettings;
        private Label label1;
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
        private Label label8;
        private Panel pnlAbout;
        private TableLayoutPanel tableLayoutPanel4;
        private Label label19;
        private Label lblCopyright;
        private Label label21;
        private Label label22;
        private Label label23;
        private Label lblGPosingwayVersion;
        private LinkLabel linkLabel3;
        private Label label25;
        private DataGridView dgvPackages;
        private BindingSource instancesBindingSource;
        private BindingSource resourcePackageBindingSource;
        private BindingSource instancesBindingSource1;
        private DataGridViewTextBoxColumn urlDataGridViewTextBoxColumn;
        private BindingSource mainSource;
        private DataGridViewTextBoxColumn NameDataGridViewTextBoxColumn;
        private FlowLayoutPanel flpPackageOptions;
        private Button btnInstallPackage;
        private Button btnRemove;
        private Button btnReinstall;
        private DataGridViewTextBoxColumn typeDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn statusDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn TypeCol;
        private DataGridViewTextBoxColumn NameCol;
        private DataGridViewTextBoxColumn StatusCol;
        private Button btnUninstall;
    }
}
