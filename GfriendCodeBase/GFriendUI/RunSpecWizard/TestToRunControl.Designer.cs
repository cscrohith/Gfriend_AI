namespace HP.GFriend.UI.RunSpecWizard
{
    partial class TestToRunControl
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
            this.labelScriptPath = new System.Windows.Forms.Label();
            this.numericUpDownRepeat = new System.Windows.Forms.NumericUpDown();
            this.labelRepeat = new System.Windows.Forms.Label();
            this.labelDevice = new System.Windows.Forms.Label();
            this.comboBoxDevice = new System.Windows.Forms.ComboBox();
            this.labelSelectTC = new System.Windows.Forms.Label();
            this.listBoxTC = new System.Windows.Forms.ListBox();
            this.buttonDelete = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRepeat)).BeginInit();
            this.SuspendLayout();
            // 
            // labelScriptPath
            // 
            this.labelScriptPath.AutoSize = true;
            this.labelScriptPath.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelScriptPath.Location = new System.Drawing.Point(13, 11);
            this.labelScriptPath.Name = "labelScriptPath";
            this.labelScriptPath.Size = new System.Drawing.Size(47, 13);
            this.labelScriptPath.TabIndex = 0;
            this.labelScriptPath.Text = "label1";
            // 
            // numericUpDownRepeat
            // 
            this.numericUpDownRepeat.Font = new System.Drawing.Font("Verdana", 8.25F);
            this.numericUpDownRepeat.Location = new System.Drawing.Point(62, 36);
            this.numericUpDownRepeat.Name = "numericUpDownRepeat";
            this.numericUpDownRepeat.Size = new System.Drawing.Size(89, 21);
            this.numericUpDownRepeat.TabIndex = 1;
            this.numericUpDownRepeat.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownRepeat.ValueChanged += new System.EventHandler(this.NumericUpDownRepeat_ValueChanged);
            // 
            // labelRepeat
            // 
            this.labelRepeat.AutoSize = true;
            this.labelRepeat.Font = new System.Drawing.Font("Verdana", 8.25F);
            this.labelRepeat.Location = new System.Drawing.Point(13, 38);
            this.labelRepeat.Name = "labelRepeat";
            this.labelRepeat.Size = new System.Drawing.Size(47, 13);
            this.labelRepeat.TabIndex = 2;
            this.labelRepeat.Text = "Repeat";
            this.labelRepeat.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelDevice
            // 
            this.labelDevice.AutoSize = true;
            this.labelDevice.Font = new System.Drawing.Font("Verdana", 8.25F);
            this.labelDevice.Location = new System.Drawing.Point(181, 38);
            this.labelDevice.Name = "labelDevice";
            this.labelDevice.Size = new System.Drawing.Size(91, 13);
            this.labelDevice.TabIndex = 3;
            this.labelDevice.Text = "Default Device";
            this.labelDevice.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelDevice.Visible = false;
            // 
            // comboBoxDevice
            // 
            this.comboBoxDevice.Font = new System.Drawing.Font("Verdana", 8.25F);
            this.comboBoxDevice.FormattingEnabled = true;
            this.comboBoxDevice.Location = new System.Drawing.Point(276, 35);
            this.comboBoxDevice.Name = "comboBoxDevice";
            this.comboBoxDevice.Size = new System.Drawing.Size(123, 21);
            this.comboBoxDevice.TabIndex = 4;
            this.comboBoxDevice.Visible = false;
            this.comboBoxDevice.SelectedValueChanged += new System.EventHandler(this.ComboBoxDevice_SelectedValueChanged);
            // 
            // labelSelectTC
            // 
            this.labelSelectTC.AutoSize = true;
            this.labelSelectTC.Font = new System.Drawing.Font("Verdana", 8.25F);
            this.labelSelectTC.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(75)))), ((int)(((byte)(139)))));
            this.labelSelectTC.Location = new System.Drawing.Point(13, 72);
            this.labelSelectTC.Name = "labelSelectTC";
            this.labelSelectTC.Size = new System.Drawing.Size(324, 13);
            this.labelSelectTC.TabIndex = 5;
            this.labelSelectTC.Text = "To Select Testcase, Click Here (Default : All Test cases)";
            this.labelSelectTC.Click += new System.EventHandler(this.LabelSelectTC_Click);
            // 
            // listBoxTC
            // 
            this.listBoxTC.Font = new System.Drawing.Font("Verdana", 8.25F);
            this.listBoxTC.FormattingEnabled = true;
            this.listBoxTC.Location = new System.Drawing.Point(16, 72);
            this.listBoxTC.Name = "listBoxTC";
            this.listBoxTC.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.listBoxTC.Size = new System.Drawing.Size(383, 199);
            this.listBoxTC.TabIndex = 6;
            this.listBoxTC.Visible = false;
            this.listBoxTC.SelectedValueChanged += new System.EventHandler(this.ListBoxTC_SelectedValueChanged);
            // 
            // buttonDelete
            // 
            this.buttonDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonDelete.Font = new System.Drawing.Font("Verdana", 8.25F);
            this.buttonDelete.ForeColor = System.Drawing.Color.Red;
            this.buttonDelete.Location = new System.Drawing.Point(398, 3);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(24, 23);
            this.buttonDelete.TabIndex = 7;
            this.buttonDelete.Text = "X";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.ButtonDelete_Click);
            // 
            // TestToRunControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.listBoxTC);
            this.Controls.Add(this.labelSelectTC);
            this.Controls.Add(this.comboBoxDevice);
            this.Controls.Add(this.labelDevice);
            this.Controls.Add(this.labelRepeat);
            this.Controls.Add(this.numericUpDownRepeat);
            this.Controls.Add(this.labelScriptPath);
            this.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "TestToRunControl";
            this.Size = new System.Drawing.Size(425, 95);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRepeat)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelScriptPath;
        private System.Windows.Forms.NumericUpDown numericUpDownRepeat;
        private System.Windows.Forms.Label labelRepeat;
        private System.Windows.Forms.Label labelDevice;
        private System.Windows.Forms.ComboBox comboBoxDevice;
        private System.Windows.Forms.Label labelSelectTC;
        private System.Windows.Forms.ListBox listBoxTC;
        private System.Windows.Forms.Button buttonDelete;
    }
}
