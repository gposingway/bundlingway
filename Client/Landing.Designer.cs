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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Landing));
            contextMenuStrip1 = new ContextMenuStrip(components);
            splitContainer1 = new SplitContainer();
            flowLayoutPanel1 = new FlowLayoutPanel();
            btnSettings = new Button();
            btnDownloads = new Button();
            btnBackup = new Button();
            btnAbout = new Button();
            picGPosingway = new PictureBox();
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
            pnlBackup = new Panel();
            tableLayoutPanel3 = new TableLayoutPanel();
            button5 = new Button();
            button4 = new Button();
            label12 = new Label();
            label13 = new Label();
            label18 = new Label();
            pnlPackages = new Panel();
            dgvPackages = new DataGridView();
            typeDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            NameDataGridViewTextBox = new DataGridViewTextBoxColumn();
            statusDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            localBasePathDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            mainSource = new BindingSource(components);
            label8 = new Label();
            btnInstallPackage = new Button();
            pnlSettings = new Panel();
            tableLayoutPanel1 = new TableLayoutPanel();
            button3 = new Button();
            textBox3 = new TextBox();
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
            stsFootnote = new StatusStrip();
            tssMessage = new ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picGPosingway).BeginInit();
            pnlAbout.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            pnlBackup.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            pnlPackages.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPackages).BeginInit();
            ((System.ComponentModel.ISupportInitialize)mainSource).BeginInit();
            pnlSettings.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)resourcePackageBindingSource).BeginInit();
            ((System.ComponentModel.ISupportInitialize)instancesBindingSource).BeginInit();
            ((System.ComponentModel.ISupportInitialize)instancesBindingSource1).BeginInit();
            stsFootnote.SuspendLayout();
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
            splitContainer1.Panel1.Controls.Add(flowLayoutPanel1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.AutoScroll = true;
            splitContainer1.Panel2.Controls.Add(pnlAbout);
            splitContainer1.Panel2.Controls.Add(pnlBackup);
            splitContainer1.Panel2.Controls.Add(pnlPackages);
            splitContainer1.Panel2.Controls.Add(pnlSettings);
            splitContainer1.Size = new Size(772, 684);
            splitContainer1.SplitterDistance = 153;
            splitContainer1.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AllowDrop = true;
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.BackColor = SystemColors.ControlLight;
            flowLayoutPanel1.Controls.Add(btnSettings);
            flowLayoutPanel1.Controls.Add(btnDownloads);
            flowLayoutPanel1.Controls.Add(btnBackup);
            flowLayoutPanel1.Controls.Add(btnAbout);
            flowLayoutPanel1.Controls.Add(picGPosingway);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(0, 0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(153, 684);
            flowLayoutPanel1.TabIndex = 0;
            flowLayoutPanel1.DragDrop += flowLayoutPanel1_DragDrop;
            flowLayoutPanel1.DragEnter += flowLayoutPanel1_DragEnter;
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
            // btnBackup
            // 
            btnBackup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnBackup.FlatAppearance.BorderSize = 0;
            btnBackup.FlatStyle = FlatStyle.Flat;
            btnBackup.Location = new Point(3, 61);
            btnBackup.Name = "btnBackup";
            btnBackup.Size = new Size(145, 23);
            btnBackup.TabIndex = 2;
            btnBackup.Text = "Backup and Restore";
            btnBackup.UseVisualStyleBackColor = true;
            btnBackup.Click += btnBackup_Click;
            // 
            // btnAbout
            // 
            btnAbout.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnAbout.FlatAppearance.BorderSize = 0;
            btnAbout.FlatStyle = FlatStyle.Flat;
            btnAbout.Location = new Point(3, 90);
            btnAbout.Name = "btnAbout";
            btnAbout.Size = new Size(145, 23);
            btnAbout.TabIndex = 5;
            btnAbout.Text = "About";
            btnAbout.UseVisualStyleBackColor = true;
            btnAbout.Click += btnAbout_Click;
            // 
            // picGPosingway
            // 
            picGPosingway.BackgroundImage = (Image)resources.GetObject("picGPosingway.BackgroundImage");
            picGPosingway.BackgroundImageLayout = ImageLayout.Center;
            picGPosingway.InitialImage = null;
            picGPosingway.Location = new Point(3, 119);
            picGPosingway.Name = "picGPosingway";
            picGPosingway.Size = new Size(145, 542);
            picGPosingway.TabIndex = 6;
            picGPosingway.TabStop = false;
            // 
            // pnlAbout
            // 
            pnlAbout.Controls.Add(tableLayoutPanel4);
            pnlAbout.Controls.Add(label25);
            pnlAbout.Dock = DockStyle.Top;
            pnlAbout.Location = new Point(0, 487);
            pnlAbout.Name = "pnlAbout";
            pnlAbout.Padding = new Padding(5);
            pnlAbout.Size = new Size(615, 174);
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
            tableLayoutPanel4.Size = new Size(605, 139);
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
            lblCopyright.Size = new Size(389, 15);
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
            label22.Size = new Size(389, 15);
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
            lblGPosingwayVersion.Size = new Size(389, 51);
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
            label25.Size = new Size(605, 25);
            label25.TabIndex = 0;
            label25.Text = "About";
            // 
            // pnlBackup
            // 
            pnlBackup.Controls.Add(tableLayoutPanel3);
            pnlBackup.Controls.Add(label18);
            pnlBackup.Dock = DockStyle.Top;
            pnlBackup.Location = new Point(0, 382);
            pnlBackup.Name = "pnlBackup";
            pnlBackup.Padding = new Padding(5);
            pnlBackup.Size = new Size(615, 105);
            pnlBackup.TabIndex = 2;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 3;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
            tableLayoutPanel3.Controls.Add(button5, 2, 1);
            tableLayoutPanel3.Controls.Add(button4, 2, 0);
            tableLayoutPanel3.Controls.Add(label12, 0, 0);
            tableLayoutPanel3.Controls.Add(label13, 1, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(5, 30);
            tableLayoutPanel3.Margin = new Padding(3, 10, 3, 3);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.Padding = new Padding(5);
            tableLayoutPanel3.RowCount = 6;
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.Size = new Size(605, 70);
            tableLayoutPanel3.TabIndex = 1;
            // 
            // button5
            // 
            button5.Dock = DockStyle.Fill;
            button5.Location = new Point(543, 37);
            button5.Name = "button5";
            button5.Size = new Size(54, 23);
            button5.TabIndex = 15;
            button5.Text = "Restore";
            button5.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            button4.Dock = DockStyle.Fill;
            button4.Location = new Point(543, 8);
            button4.Name = "button4";
            button4.Size = new Size(54, 23);
            button4.TabIndex = 14;
            button4.Text = "Backup";
            button4.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Dock = DockStyle.Fill;
            label12.Location = new Point(8, 5);
            label12.Name = "label12";
            label12.Size = new Size(134, 29);
            label12.TabIndex = 13;
            label12.Text = "Last Backup";
            label12.TextAlign = ContentAlignment.TopRight;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Dock = DockStyle.Fill;
            label13.Location = new Point(148, 5);
            label13.Margin = new Padding(3, 0, 3, 5);
            label13.Name = "label13";
            label13.Size = new Size(389, 24);
            label13.TabIndex = 12;
            label13.Text = "Never";
            // 
            // label18
            // 
            label18.BackColor = SystemColors.ControlDark;
            label18.Dock = DockStyle.Top;
            label18.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label18.ForeColor = SystemColors.ButtonHighlight;
            label18.Location = new Point(5, 5);
            label18.Name = "label18";
            label18.Padding = new Padding(3);
            label18.Size = new Size(605, 25);
            label18.TabIndex = 0;
            label18.Text = "Backup and Restore";
            // 
            // pnlPackages
            // 
            pnlPackages.Controls.Add(dgvPackages);
            pnlPackages.Controls.Add(label8);
            pnlPackages.Controls.Add(btnInstallPackage);
            pnlPackages.Dock = DockStyle.Top;
            pnlPackages.Location = new Point(0, 132);
            pnlPackages.Name = "pnlPackages";
            pnlPackages.Padding = new Padding(5);
            pnlPackages.Size = new Size(615, 250);
            pnlPackages.TabIndex = 1;
            // 
            // dgvPackages
            // 
            dgvPackages.AllowDrop = true;
            dgvPackages.AllowUserToAddRows = false;
            dgvPackages.AllowUserToDeleteRows = false;
            dgvPackages.AutoGenerateColumns = false;
            dgvPackages.BackgroundColor = SystemColors.Control;
            dgvPackages.BorderStyle = BorderStyle.Fixed3D;
            dgvPackages.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPackages.Columns.AddRange(new DataGridViewColumn[] { typeDataGridViewTextBoxColumn, NameDataGridViewTextBox, statusDataGridViewTextBoxColumn, localBasePathDataGridViewTextBoxColumn });
            dgvPackages.ContextMenuStrip = contextMenuStrip1;
            dgvPackages.DataMember = "ResourcePackages";
            dgvPackages.DataSource = mainSource;
            dgvPackages.Dock = DockStyle.Fill;
            dgvPackages.Location = new Point(5, 30);
            dgvPackages.Name = "dgvPackages";
            dgvPackages.ReadOnly = true;
            dgvPackages.Size = new Size(605, 192);
            dgvPackages.TabIndex = 1;
            dgvPackages.DragDrop += dgvPackages_DragDrop;
            dgvPackages.DragEnter += dgvPackages_DragEnter;
            // 
            // typeDataGridViewTextBoxColumn
            // 
            typeDataGridViewTextBoxColumn.DataPropertyName = "Type";
            typeDataGridViewTextBoxColumn.HeaderText = "Type";
            typeDataGridViewTextBoxColumn.Name = "typeDataGridViewTextBoxColumn";
            typeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // NameDataGridViewTextBox
            // 
            NameDataGridViewTextBox.DataPropertyName = "Name";
            NameDataGridViewTextBox.HeaderText = "Name";
            NameDataGridViewTextBox.Name = "NameDataGridViewTextBox";
            NameDataGridViewTextBox.ReadOnly = true;
            NameDataGridViewTextBox.Width = 235;
            // 
            // statusDataGridViewTextBoxColumn
            // 
            statusDataGridViewTextBoxColumn.DataPropertyName = "Status";
            statusDataGridViewTextBoxColumn.HeaderText = "Status";
            statusDataGridViewTextBoxColumn.Name = "statusDataGridViewTextBoxColumn";
            statusDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // localBasePathDataGridViewTextBoxColumn
            // 
            localBasePathDataGridViewTextBoxColumn.DataPropertyName = "LocalBasePath";
            localBasePathDataGridViewTextBoxColumn.HeaderText = "Path";
            localBasePathDataGridViewTextBoxColumn.Name = "localBasePathDataGridViewTextBoxColumn";
            localBasePathDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // mainSource
            // 
            mainSource.DataSource = typeof(Model.GPosingwayConfig);
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
            label8.Size = new Size(605, 25);
            label8.TabIndex = 0;
            label8.Text = "Packages";
            // 
            // btnInstallPackage
            // 
            btnInstallPackage.Dock = DockStyle.Bottom;
            btnInstallPackage.Location = new Point(5, 222);
            btnInstallPackage.Name = "btnInstallPackage";
            btnInstallPackage.Size = new Size(605, 23);
            btnInstallPackage.TabIndex = 3;
            btnInstallPackage.Text = "Add Package";
            btnInstallPackage.UseVisualStyleBackColor = true;
            btnInstallPackage.Click += btnInstallPackage_Click;
            // 
            // pnlSettings
            // 
            pnlSettings.Controls.Add(tableLayoutPanel1);
            pnlSettings.Controls.Add(label1);
            pnlSettings.Dock = DockStyle.Top;
            pnlSettings.Location = new Point(0, 0);
            pnlSettings.Name = "pnlSettings";
            pnlSettings.Padding = new Padding(5);
            pnlSettings.Size = new Size(615, 132);
            pnlSettings.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
            tableLayoutPanel1.Controls.Add(button3, 2, 2);
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
            tableLayoutPanel1.Size = new Size(605, 101);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // button3
            // 
            button3.DataBindings.Add(new Binding("Visible", mainSource, "GPosingway.Missing", true));
            button3.Dock = DockStyle.Fill;
            button3.Location = new Point(543, 66);
            button3.Name = "button3";
            button3.Size = new Size(54, 23);
            button3.TabIndex = 8;
            button3.Text = "Install";
            button3.UseVisualStyleBackColor = true;
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
            textBox3.Size = new Size(389, 16);
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
            btnInstallReShade.Location = new Point(543, 37);
            btnInstallReShade.Name = "btnInstallReShade";
            btnInstallReShade.Size = new Size(54, 23);
            btnInstallReShade.TabIndex = 5;
            btnInstallReShade.Text = "Install";
            btnInstallReShade.UseVisualStyleBackColor = true;
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
            textBox2.Size = new Size(389, 16);
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
            txtXivPath.Size = new Size(389, 23);
            txtXivPath.TabIndex = 1;
            // 
            // btnDetectSettings
            // 
            btnDetectSettings.Location = new Point(543, 8);
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
            label1.Size = new Size(605, 25);
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
            // stsFootnote
            // 
            stsFootnote.Items.AddRange(new ToolStripItem[] { tssMessage });
            stsFootnote.Location = new Point(0, 662);
            stsFootnote.Name = "stsFootnote";
            stsFootnote.RightToLeft = RightToLeft.Yes;
            stsFootnote.Size = new Size(772, 22);
            stsFootnote.TabIndex = 2;
            stsFootnote.Text = "statusStrip1";
            // 
            // tssMessage
            // 
            tssMessage.Name = "tssMessage";
            tssMessage.Size = new Size(42, 17);
            tssMessage.Text = "Ready.";
            // 
            // Landing
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(772, 684);
            Controls.Add(stsFootnote);
            Controls.Add(splitContainer1);
            MaximumSize = new Size(788, 723);
            MinimumSize = new Size(788, 723);
            Name = "Landing";
            Text = "GPosingway Package Manager";
            Load += Landing_Load;
            DragDrop += Landing_DragDrop;
            DragEnter += Landing_DragEnter;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picGPosingway).EndInit();
            pnlAbout.ResumeLayout(false);
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            pnlBackup.ResumeLayout(false);
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            pnlPackages.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvPackages).EndInit();
            ((System.ComponentModel.ISupportInitialize)mainSource).EndInit();
            pnlSettings.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)resourcePackageBindingSource).EndInit();
            ((System.ComponentModel.ISupportInitialize)instancesBindingSource).EndInit();
            ((System.ComponentModel.ISupportInitialize)instancesBindingSource1).EndInit();
            stsFootnote.ResumeLayout(false);
            stsFootnote.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ContextMenuStrip contextMenuStrip1;
        private SplitContainer splitContainer1;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button btnDownloads;
        private Button btnBackup;
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
        private Button button3;
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
        private Panel pnlBackup;
        private TableLayoutPanel tableLayoutPanel3;
        private Button button4;
        private Label label12;
        private Label label13;
        private Label label18;
        private Button button5;
        private DataGridView dgvPackages;
        private BindingSource instancesBindingSource;
        private BindingSource resourcePackageBindingSource;
        private BindingSource instancesBindingSource1;
        private DataGridViewTextBoxColumn urlDataGridViewTextBoxColumn;
        private BindingSource mainSource;
        private Button btnInstallPackage;
        private DataGridViewTextBoxColumn NameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn xIVPathDataGridViewTextBoxColumn;
        private PictureBox picGPosingway;
        private StatusStrip stsFootnote;
        private ToolStripStatusLabel tssMessage;
        private DataGridViewTextBoxColumn typeDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn NameDataGridViewTextBox;
        private DataGridViewTextBoxColumn statusDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn localBasePathDataGridViewTextBoxColumn;
    }
}
