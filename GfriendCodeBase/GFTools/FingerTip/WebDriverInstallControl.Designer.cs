namespace HP.GFriend.Tool
{
    partial class WebDriverInstallControl
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
            this.textBoxBrowserName = new System.Windows.Forms.TextBox();
            this.textBoxBrowserVersion = new System.Windows.Forms.TextBox();
            this.textBoxDriverVersion = new System.Windows.Forms.TextBox();
            this.buttonDownload = new System.Windows.Forms.Button();
            this.labelNote = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBoxBrowserName
            // 
            this.textBoxBrowserName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(192)))));
            this.textBoxBrowserName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxBrowserName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxBrowserName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.textBoxBrowserName.Location = new System.Drawing.Point(3, 3);
            this.textBoxBrowserName.Name = "textBoxBrowserName";
            this.textBoxBrowserName.Size = new System.Drawing.Size(100, 20);
            this.textBoxBrowserName.TabIndex = 0;
            this.textBoxBrowserName.TabStop = false;
            // 
            // textBoxBrowserVersion
            // 
            this.textBoxBrowserVersion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.textBoxBrowserVersion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxBrowserVersion.ForeColor = System.Drawing.Color.Black;
            this.textBoxBrowserVersion.Location = new System.Drawing.Point(109, 3);
            this.textBoxBrowserVersion.Name = "textBoxBrowserVersion";
            this.textBoxBrowserVersion.Size = new System.Drawing.Size(239, 20);
            this.textBoxBrowserVersion.TabIndex = 1;
            this.textBoxBrowserVersion.TabStop = false;
            // 
            // textBoxDriverVersion
            // 
            this.textBoxDriverVersion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.textBoxDriverVersion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxDriverVersion.ForeColor = System.Drawing.Color.Black;
            this.textBoxDriverVersion.Location = new System.Drawing.Point(354, 2);
            this.textBoxDriverVersion.Name = "textBoxDriverVersion";
            this.textBoxDriverVersion.Size = new System.Drawing.Size(145, 20);
            this.textBoxDriverVersion.TabIndex = 2;
            this.textBoxDriverVersion.TabStop = false;
            // 
            // buttonDownload
            // 
            this.buttonDownload.Location = new System.Drawing.Point(505, 0);
            this.buttonDownload.Name = "buttonDownload";
            this.buttonDownload.Size = new System.Drawing.Size(75, 39);
            this.buttonDownload.TabIndex = 3;
            this.buttonDownload.Text = "Download";
            this.buttonDownload.UseVisualStyleBackColor = true;
            this.buttonDownload.Click += new System.EventHandler(this.buttonDownload_Click);
            // 
            // labelNote
            // 
            this.labelNote.AutoSize = true;
            this.labelNote.ForeColor = System.Drawing.Color.Red;
            this.labelNote.Location = new System.Drawing.Point(106, 26);
            this.labelNote.Name = "labelNote";
            this.labelNote.Size = new System.Drawing.Size(0, 13);
            this.labelNote.TabIndex = 4;
            // 
            // WebDriverInstallControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.Controls.Add(this.labelNote);
            this.Controls.Add(this.buttonDownload);
            this.Controls.Add(this.textBoxDriverVersion);
            this.Controls.Add(this.textBoxBrowserVersion);
            this.Controls.Add(this.textBoxBrowserName);
            this.Name = "WebDriverInstallControl";
            this.Size = new System.Drawing.Size(583, 42);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxBrowserName;
        private System.Windows.Forms.TextBox textBoxBrowserVersion;
        private System.Windows.Forms.TextBox textBoxDriverVersion;
        private System.Windows.Forms.Button buttonDownload;
        private System.Windows.Forms.Label labelNote;
    }
}
