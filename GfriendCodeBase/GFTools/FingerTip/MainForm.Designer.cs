namespace HP.GFriend.Tool
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.flowLayoutPanelDrivers = new System.Windows.Forms.FlowLayoutPanel();
            this.textBoxBrowserName = new System.Windows.Forms.TextBox();
            this.textBoxBrowserVersion = new System.Windows.Forms.TextBox();
            this.textBoxDriverVersion = new System.Windows.Forms.TextBox();
            this.textBoxInstalled = new System.Windows.Forms.TextBox();
            this.textBoxNotInstalled = new System.Windows.Forms.TextBox();
            this.textBoxNeedUpdate = new System.Windows.Forms.TextBox();
            this.textBoxNA = new System.Windows.Forms.TextBox();
            this.textBoxUnknown = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // flowLayoutPanelDrivers
            // 
            this.flowLayoutPanelDrivers.Location = new System.Drawing.Point(1, 38);
            this.flowLayoutPanelDrivers.Name = "flowLayoutPanelDrivers";
            this.flowLayoutPanelDrivers.Size = new System.Drawing.Size(617, 10);
            this.flowLayoutPanelDrivers.TabIndex = 0;
            // 
            // textBoxBrowserName
            // 
            this.textBoxBrowserName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.textBoxBrowserName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxBrowserName.ForeColor = System.Drawing.Color.Black;
            this.textBoxBrowserName.Location = new System.Drawing.Point(8, 23);
            this.textBoxBrowserName.Name = "textBoxBrowserName";
            this.textBoxBrowserName.Size = new System.Drawing.Size(100, 13);
            this.textBoxBrowserName.TabIndex = 4;
            this.textBoxBrowserName.TabStop = false;
            this.textBoxBrowserName.Text = "Browser";
            // 
            // textBoxBrowserVersion
            // 
            this.textBoxBrowserVersion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.textBoxBrowserVersion.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxBrowserVersion.ForeColor = System.Drawing.Color.Black;
            this.textBoxBrowserVersion.Location = new System.Drawing.Point(114, 23);
            this.textBoxBrowserVersion.Name = "textBoxBrowserVersion";
            this.textBoxBrowserVersion.Size = new System.Drawing.Size(100, 13);
            this.textBoxBrowserVersion.TabIndex = 5;
            this.textBoxBrowserVersion.TabStop = false;
            this.textBoxBrowserVersion.Text = "Browser Version";
            // 
            // textBoxDriverVersion
            // 
            this.textBoxDriverVersion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.textBoxDriverVersion.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxDriverVersion.ForeColor = System.Drawing.Color.Black;
            this.textBoxDriverVersion.Location = new System.Drawing.Point(359, 23);
            this.textBoxDriverVersion.Name = "textBoxDriverVersion";
            this.textBoxDriverVersion.Size = new System.Drawing.Size(100, 13);
            this.textBoxDriverVersion.TabIndex = 6;
            this.textBoxDriverVersion.TabStop = false;
            this.textBoxDriverVersion.Text = "Web Driver Version";
            // 
            // textBoxInstalled
            // 
            this.textBoxInstalled.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.textBoxInstalled.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxInstalled.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxInstalled.ForeColor = System.Drawing.Color.Black;
            this.textBoxInstalled.Location = new System.Drawing.Point(164, 3);
            this.textBoxInstalled.Name = "textBoxInstalled";
            this.textBoxInstalled.Size = new System.Drawing.Size(88, 13);
            this.textBoxInstalled.TabIndex = 7;
            this.textBoxInstalled.TabStop = false;
            this.textBoxInstalled.Text = "Installed";
            this.textBoxInstalled.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxNotInstalled
            // 
            this.textBoxNotInstalled.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.textBoxNotInstalled.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxNotInstalled.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxNotInstalled.ForeColor = System.Drawing.Color.Black;
            this.textBoxNotInstalled.Location = new System.Drawing.Point(257, 3);
            this.textBoxNotInstalled.Name = "textBoxNotInstalled";
            this.textBoxNotInstalled.Size = new System.Drawing.Size(88, 13);
            this.textBoxNotInstalled.TabIndex = 8;
            this.textBoxNotInstalled.TabStop = false;
            this.textBoxNotInstalled.Text = "Not Installed";
            this.textBoxNotInstalled.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxNeedUpdate
            // 
            this.textBoxNeedUpdate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.textBoxNeedUpdate.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxNeedUpdate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxNeedUpdate.ForeColor = System.Drawing.Color.Black;
            this.textBoxNeedUpdate.Location = new System.Drawing.Point(349, 3);
            this.textBoxNeedUpdate.Name = "textBoxNeedUpdate";
            this.textBoxNeedUpdate.Size = new System.Drawing.Size(88, 13);
            this.textBoxNeedUpdate.TabIndex = 9;
            this.textBoxNeedUpdate.TabStop = false;
            this.textBoxNeedUpdate.Text = "Need Update";
            this.textBoxNeedUpdate.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxNA
            // 
            this.textBoxNA.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.textBoxNA.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxNA.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxNA.ForeColor = System.Drawing.Color.Black;
            this.textBoxNA.Location = new System.Drawing.Point(441, 3);
            this.textBoxNA.Name = "textBoxNA";
            this.textBoxNA.Size = new System.Drawing.Size(88, 13);
            this.textBoxNA.TabIndex = 10;
            this.textBoxNA.TabStop = false;
            this.textBoxNA.Text = "Not Applicable";
            this.textBoxNA.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxUnknown
            // 
            this.textBoxUnknown.BackColor = System.Drawing.Color.Gray;
            this.textBoxUnknown.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxUnknown.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxUnknown.ForeColor = System.Drawing.Color.Black;
            this.textBoxUnknown.Location = new System.Drawing.Point(532, 3);
            this.textBoxUnknown.Name = "textBoxUnknown";
            this.textBoxUnknown.Size = new System.Drawing.Size(88, 13);
            this.textBoxUnknown.TabIndex = 11;
            this.textBoxUnknown.TabStop = false;
            this.textBoxUnknown.Text = "Unknown";
            this.textBoxUnknown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.ClientSize = new System.Drawing.Size(621, 51);
            this.Controls.Add(this.textBoxUnknown);
            this.Controls.Add(this.textBoxNA);
            this.Controls.Add(this.textBoxNeedUpdate);
            this.Controls.Add(this.textBoxNotInstalled);
            this.Controls.Add(this.textBoxInstalled);
            this.Controls.Add(this.textBoxDriverVersion);
            this.Controls.Add(this.textBoxBrowserVersion);
            this.Controls.Add(this.textBoxBrowserName);
            this.Controls.Add(this.flowLayoutPanelDrivers);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Finger Tip - Web Test Preparation";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelDrivers;
        private System.Windows.Forms.TextBox textBoxBrowserName;
        private System.Windows.Forms.TextBox textBoxBrowserVersion;
        private System.Windows.Forms.TextBox textBoxDriverVersion;
        private System.Windows.Forms.TextBox textBoxInstalled;
        private System.Windows.Forms.TextBox textBoxNotInstalled;
        private System.Windows.Forms.TextBox textBoxNeedUpdate;
        private System.Windows.Forms.TextBox textBoxNA;
        private System.Windows.Forms.TextBox textBoxUnknown;
    }
}

