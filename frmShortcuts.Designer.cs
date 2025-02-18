namespace Bundlingway
{
    partial class frmShortcuts
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmShortcuts));
            tableLayoutPanel1 = new TableLayoutPanel();
            txtSrtOverlay = new TextBox();
            label3 = new Label();
            label1 = new Label();
            label2 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            label7 = new Label();
            txtSrtToggle = new TextBox();
            txtSrtReload = new TextBox();
            txtSrtScreenshot = new TextBox();
            txtSrtTextures = new TextBox();
            txtSrtPreviewer = new TextBox();
            txtSrtRatio = new TextBox();
            lblGrpShortcuts = new Label();
            btnSave = new FontAwesome.Sharp.IconButton();
            btnCancel = new FontAwesome.Sharp.IconButton();
            btnApplyAll = new FontAwesome.Sharp.IconButton();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Controls.Add(txtSrtOverlay, 1, 0);
            tableLayoutPanel1.Controls.Add(label3, 0, 2);
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(label2, 0, 1);
            tableLayoutPanel1.Controls.Add(label4, 0, 3);
            tableLayoutPanel1.Controls.Add(label5, 0, 4);
            tableLayoutPanel1.Controls.Add(label6, 0, 5);
            tableLayoutPanel1.Controls.Add(label7, 0, 6);
            tableLayoutPanel1.Controls.Add(txtSrtToggle, 1, 1);
            tableLayoutPanel1.Controls.Add(txtSrtReload, 1, 2);
            tableLayoutPanel1.Controls.Add(txtSrtScreenshot, 1, 3);
            tableLayoutPanel1.Controls.Add(txtSrtTextures, 1, 4);
            tableLayoutPanel1.Controls.Add(txtSrtPreviewer, 1, 5);
            tableLayoutPanel1.Controls.Add(txtSrtRatio, 1, 6);
            tableLayoutPanel1.Location = new Point(12, 28);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 7;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(292, 205);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // txtSrtOverlay
            // 
            txtSrtOverlay.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtSrtOverlay.BorderStyle = BorderStyle.FixedSingle;
            txtSrtOverlay.Location = new Point(159, 3);
            txtSrtOverlay.Name = "txtSrtOverlay";
            txtSrtOverlay.Size = new Size(130, 23);
            txtSrtOverlay.TabIndex = 2;
            txtSrtOverlay.Tag = "KeyOverlay";
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label3.AutoSize = true;
            label3.Location = new Point(9, 64);
            label3.Margin = new Padding(9, 6, 9, 6);
            label3.Name = "label3";
            label3.Size = new Size(138, 17);
            label3.TabIndex = 1;
            label3.Text = "Reload Preset";
            label3.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(9, 6);
            label1.Margin = new Padding(9, 6, 9, 6);
            label1.Name = "label1";
            label1.Size = new Size(138, 17);
            label1.TabIndex = 0;
            label1.Text = "Toggle ReShade overlay";
            label1.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new Point(9, 35);
            label2.Margin = new Padding(9, 6, 9, 6);
            label2.Name = "label2";
            label2.Size = new Size(138, 17);
            label2.TabIndex = 1;
            label2.Text = "Toggle Preset";
            label2.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label4.AutoSize = true;
            label4.Location = new Point(9, 93);
            label4.Margin = new Padding(9, 6, 9, 6);
            label4.Name = "label4";
            label4.Size = new Size(138, 17);
            label4.TabIndex = 2;
            label4.Text = "Capture Screenshot";
            label4.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label5.AutoSize = true;
            label5.Location = new Point(9, 122);
            label5.Margin = new Padding(9, 6, 9, 6);
            label5.Name = "label5";
            label5.Size = new Size(138, 17);
            label5.TabIndex = 3;
            label5.Text = "Toggle Textures";
            label5.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label6.AutoSize = true;
            label6.Location = new Point(9, 151);
            label6.Margin = new Padding(9, 6, 9, 6);
            label6.Name = "label6";
            label6.Size = new Size(138, 17);
            label6.TabIndex = 4;
            label6.Text = "Toggle Vertical Previewer";
            label6.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label7
            // 
            label7.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label7.AutoSize = true;
            label7.Location = new Point(9, 180);
            label7.Margin = new Padding(9, 6, 9, 6);
            label7.Name = "label7";
            label7.Size = new Size(138, 19);
            label7.TabIndex = 5;
            label7.Text = "Toggle Aspect Ratio";
            label7.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtSrtToggle
            // 
            txtSrtToggle.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtSrtToggle.BorderStyle = BorderStyle.FixedSingle;
            txtSrtToggle.Location = new Point(159, 32);
            txtSrtToggle.Name = "txtSrtToggle";
            txtSrtToggle.Size = new Size(130, 23);
            txtSrtToggle.TabIndex = 6;
            txtSrtToggle.Tag = "KeyEffects";
            // 
            // txtSrtReload
            // 
            txtSrtReload.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtSrtReload.BorderStyle = BorderStyle.FixedSingle;
            txtSrtReload.Location = new Point(159, 61);
            txtSrtReload.Name = "txtSrtReload";
            txtSrtReload.Size = new Size(130, 23);
            txtSrtReload.TabIndex = 7;
            txtSrtReload.Tag = "KeyReload";
            // 
            // txtSrtScreenshot
            // 
            txtSrtScreenshot.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtSrtScreenshot.BorderStyle = BorderStyle.FixedSingle;
            txtSrtScreenshot.Location = new Point(159, 90);
            txtSrtScreenshot.Name = "txtSrtScreenshot";
            txtSrtScreenshot.Size = new Size(130, 23);
            txtSrtScreenshot.TabIndex = 8;
            txtSrtScreenshot.Tag = "KeyScreenshot";
            // 
            // txtSrtTextures
            // 
            txtSrtTextures.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtSrtTextures.BorderStyle = BorderStyle.FixedSingle;
            txtSrtTextures.Location = new Point(159, 119);
            txtSrtTextures.Name = "txtSrtTextures";
            txtSrtTextures.Size = new Size(130, 23);
            txtSrtTextures.TabIndex = 9;
            txtSrtTextures.Tag = "KeyStageDepth@StageDepth.fx";
            // 
            // txtSrtPreviewer
            // 
            txtSrtPreviewer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtSrtPreviewer.BorderStyle = BorderStyle.FixedSingle;
            txtSrtPreviewer.Location = new Point(159, 148);
            txtSrtPreviewer.Name = "txtSrtPreviewer";
            txtSrtPreviewer.Size = new Size(130, 23);
            txtSrtPreviewer.TabIndex = 10;
            txtSrtPreviewer.Tag = "KeyVertical_Previewer@VerticalPreviewer.fx";
            // 
            // txtSrtRatio
            // 
            txtSrtRatio.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtSrtRatio.BorderStyle = BorderStyle.FixedSingle;
            txtSrtRatio.Location = new Point(159, 177);
            txtSrtRatio.Name = "txtSrtRatio";
            txtSrtRatio.Size = new Size(130, 23);
            txtSrtRatio.TabIndex = 11;
            txtSrtRatio.Tag = "KeyAspectRatioComposition@AspectRatioComposition.fx";
            // 
            // lblGrpShortcuts
            // 
            lblGrpShortcuts.BackColor = SystemColors.Highlight;
            lblGrpShortcuts.Dock = DockStyle.Top;
            lblGrpShortcuts.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblGrpShortcuts.ForeColor = SystemColors.Control;
            lblGrpShortcuts.Location = new Point(0, 0);
            lblGrpShortcuts.Name = "lblGrpShortcuts";
            lblGrpShortcuts.Padding = new Padding(3);
            lblGrpShortcuts.Size = new Size(313, 25);
            lblGrpShortcuts.TabIndex = 1;
            lblGrpShortcuts.Text = "Shortcuts";
            // 
            // btnSave
            // 
            btnSave.Enabled = false;
            btnSave.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnSave.IconChar = FontAwesome.Sharp.IconChar.Check;
            btnSave.IconColor = SystemColors.Highlight;
            btnSave.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnSave.IconSize = 32;
            btnSave.ImageAlign = ContentAlignment.MiddleLeft;
            btnSave.Location = new Point(171, 279);
            btnSave.Margin = new Padding(3, 3, 3, 0);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(133, 37);
            btnSave.TabIndex = 24;
            btnSave.Tag = "Happy with your changes? Tap this button to save them and apply to all managed presets. Ready to shine!";
            btnSave.Text = "Save Changes";
            btnSave.TextAlign = ContentAlignment.MiddleRight;
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            btnSave.MouseEnter += btnSave_MouseEnter;
            // 
            // btnCancel
            // 
            btnCancel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCancel.IconChar = FontAwesome.Sharp.IconChar.Close;
            btnCancel.IconColor = SystemColors.Highlight;
            btnCancel.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnCancel.IconSize = 32;
            btnCancel.ImageAlign = ContentAlignment.MiddleLeft;
            btnCancel.Location = new Point(171, 239);
            btnCancel.Margin = new Padding(3, 3, 3, 0);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(133, 37);
            btnCancel.TabIndex = 25;
            btnCancel.Tag = "This button is your 'undo' friend! Click it to forget all those changes you just made.";
            btnCancel.Text = "Cancel";
            btnCancel.TextAlign = ContentAlignment.MiddleRight;
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            btnCancel.MouseEnter += btnCancel_MouseEnter;
            // 
            // btnApplyAll
            // 
            btnApplyAll.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnApplyAll.IconChar = FontAwesome.Sharp.IconChar.Clone;
            btnApplyAll.IconColor = SystemColors.Highlight;
            btnApplyAll.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnApplyAll.IconSize = 32;
            btnApplyAll.ImageAlign = ContentAlignment.MiddleLeft;
            btnApplyAll.Location = new Point(171, 319);
            btnApplyAll.Margin = new Padding(3, 3, 3, 0);
            btnApplyAll.Name = "btnApplyAll";
            btnApplyAll.Size = new Size(133, 37);
            btnApplyAll.TabIndex = 26;
            btnApplyAll.Tag = "One click to rule them all! (Your presets, that is). Apply your current changes to every preset in your presets folder, including those not managed by Bundlingway.";
            btnApplyAll.Text = "Apply to All";
            btnApplyAll.TextAlign = ContentAlignment.MiddleRight;
            btnApplyAll.UseVisualStyleBackColor = true;
            btnApplyAll.MouseEnter += btnApplyAll_MouseEnter;
            // 
            // frmShortcuts
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(313, 364);
            Controls.Add(btnApplyAll);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(lblGrpShortcuts);
            Controls.Add(tableLayoutPanel1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "frmShortcuts";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Configuration";
            Load += fromShortcuts_Load;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Label label1;
        private Label label3;
        private Label label2;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label lblGrpShortcuts;
        private TextBox txtSrtOverlay;
        private TextBox txtSrtToggle;
        private TextBox txtSrtReload;
        private TextBox txtSrtScreenshot;
        private TextBox txtSrtTextures;
        private TextBox txtSrtPreviewer;
        private TextBox txtSrtRatio;
        private FontAwesome.Sharp.IconButton btnSave;
        private FontAwesome.Sharp.IconButton btnCancel;
        private FontAwesome.Sharp.IconButton btnApplyAll;
    }
}