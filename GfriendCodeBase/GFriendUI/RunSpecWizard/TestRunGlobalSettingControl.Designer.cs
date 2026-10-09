namespace HP.GFriend.UI.RunSpecWizard
{
    partial class TestRunGlobalSettingControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.flowLayoutPanelDetail = new System.Windows.Forms.FlowLayoutPanel();
            this.labelOutputFolder = new System.Windows.Forms.Label();
            this.textBoxOutputPath = new System.Windows.Forms.TextBox();
            this.pictureBoxSelectFolder = new System.Windows.Forms.PictureBox();
            this.labelRepeat = new System.Windows.Forms.Label();
            this.numericUpDownRepeat = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSelectFolder)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRepeat)).BeginInit();
            this.SuspendLayout();
            // 
            // flowLayoutPanelDetail
            // 
            this.flowLayoutPanelDetail.AutoScroll = true;
            this.flowLayoutPanelDetail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelDetail.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.flowLayoutPanelDetail.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanelDetail.Name = "flowLayoutPanelDetail";
            this.flowLayoutPanelDetail.Size = new System.Drawing.Size(441, 384);
            this.flowLayoutPanelDetail.TabIndex = 3;
            // 
            // labelOutputFolder
            // 
            this.labelOutputFolder.AutoSize = true;
            this.labelOutputFolder.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold);
            this.labelOutputFolder.Location = new System.Drawing.Point(450, 21);
            this.labelOutputFolder.Name = "labelOutputFolder";
            this.labelOutputFolder.Size = new System.Drawing.Size(96, 13);
            this.labelOutputFolder.TabIndex = 4;
            this.labelOutputFolder.Text = "Output Folder";
            // 
            // textBoxOutputPath
            // 
            this.textBoxOutputPath.Location = new System.Drawing.Point(453, 37);
            this.textBoxOutputPath.Name = "textBoxOutputPath";
            this.textBoxOutputPath.Size = new System.Drawing.Size(315, 21);
            this.textBoxOutputPath.TabIndex = 5;
            this.textBoxOutputPath.TextChanged += new System.EventHandler(this.TextBoxOutputPath_TextChanged);
            // 
            // pictureBoxSelectFolder
            // 
            this.pictureBoxSelectFolder.Image = global::HP.GFriend.UI.Properties.Resources.OpenFolder_16x;
            this.pictureBoxSelectFolder.Location = new System.Drawing.Point(774, 39);
            this.pictureBoxSelectFolder.Name = "pictureBoxSelectFolder";
            this.pictureBoxSelectFolder.Size = new System.Drawing.Size(16, 16);
            this.pictureBoxSelectFolder.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxSelectFolder.TabIndex = 6;
            this.pictureBoxSelectFolder.TabStop = false;
            this.pictureBoxSelectFolder.Click += new System.EventHandler(this.PictureBoxSelectFolder_Click);
            // 
            // labelRepeat
            // 
            this.labelRepeat.AutoSize = true;
            this.labelRepeat.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold);
            this.labelRepeat.Location = new System.Drawing.Point(450, 78);
            this.labelRepeat.Name = "labelRepeat";
            this.labelRepeat.Size = new System.Drawing.Size(130, 13);
            this.labelRepeat.TabIndex = 7;
            this.labelRepeat.Text = "Total Repeat Count";
            // 
            // numericUpDownRepeat
            // 
            this.numericUpDownRepeat.Location = new System.Drawing.Point(586, 76);
            this.numericUpDownRepeat.Name = "numericUpDownRepeat";
            this.numericUpDownRepeat.Size = new System.Drawing.Size(88, 21);
            this.numericUpDownRepeat.TabIndex = 8;
            this.numericUpDownRepeat.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownRepeat.ValueChanged += new System.EventHandler(this.NumericUpDownRepeat_ValueChanged);
            // 
            // TestRunGlobalSettingControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.numericUpDownRepeat);
            this.Controls.Add(this.labelRepeat);
            this.Controls.Add(this.pictureBoxSelectFolder);
            this.Controls.Add(this.textBoxOutputPath);
            this.Controls.Add(this.labelOutputFolder);
            this.Controls.Add(this.flowLayoutPanelDetail);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "TestRunGlobalSettingControl";
            this.Size = new System.Drawing.Size(800, 390);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSelectFolder)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRepeat)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelDetail;
        private System.Windows.Forms.Label labelOutputFolder;
        private System.Windows.Forms.TextBox textBoxOutputPath;
        private System.Windows.Forms.PictureBox pictureBoxSelectFolder;
        private System.Windows.Forms.Label labelRepeat;
        private System.Windows.Forms.NumericUpDown numericUpDownRepeat;
    }
}
