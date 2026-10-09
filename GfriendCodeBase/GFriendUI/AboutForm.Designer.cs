namespace HP.GFriend.UI
{
    partial class AboutForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
            this.pictureLogo = new System.Windows.Forms.PictureBox();
            this.labelVerion = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonUpdate = new System.Windows.Forms.Button();
            this.linkLabelHomepage = new System.Windows.Forms.LinkLabel();
            this.labelServerVersion = new System.Windows.Forms.Label();
            this.labelContact = new System.Windows.Forms.Label();
            this.labelName = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureLogo)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureLogo
            // 
            this.pictureLogo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureLogo.Image = global::HP.GFriend.UI.Properties.Resources.About;
            this.pictureLogo.Location = new System.Drawing.Point(54, 12);
            this.pictureLogo.Name = "pictureLogo";
            this.pictureLogo.Size = new System.Drawing.Size(294, 301);
            this.pictureLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureLogo.TabIndex = 0;
            this.pictureLogo.TabStop = false;
            // 
            // labelVerion
            // 
            this.labelVerion.Font = new System.Drawing.Font("Verdana", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelVerion.Location = new System.Drawing.Point(3, 50);
            this.labelVerion.Name = "labelVerion";
            this.labelVerion.Size = new System.Drawing.Size(389, 21);
            this.labelVerion.TabIndex = 1;
            this.labelVerion.Text = "Version Here";
            this.labelVerion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonUpdate);
            this.panel1.Controls.Add(this.linkLabelHomepage);
            this.panel1.Controls.Add(this.labelServerVersion);
            this.panel1.Controls.Add(this.labelContact);
            this.panel1.Controls.Add(this.labelName);
            this.panel1.Controls.Add(this.labelVerion);
            this.panel1.Location = new System.Drawing.Point(0, 334);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(395, 207);
            this.panel1.TabIndex = 2;
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.Location = new System.Drawing.Point(142, 108);
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(81, 32);
            this.buttonUpdate.TabIndex = 5;
            this.buttonUpdate.Text = "Update";
            this.buttonUpdate.UseVisualStyleBackColor = true;
            this.buttonUpdate.Visible = false;
            //this.buttonUpdate.Click += new System.EventHandler(this.buttonUpdate_Click);
            // 
            // linkLabelHomepage
            // 
            this.linkLabelHomepage.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabelHomepage.Location = new System.Drawing.Point(-2, 163);
            this.linkLabelHomepage.Name = "linkLabelHomepage";
            this.linkLabelHomepage.Size = new System.Drawing.Size(395, 27);
            this.linkLabelHomepage.TabIndex = 4;
            this.linkLabelHomepage.TabStop = true;
            this.linkLabelHomepage.Text = "Link Here";
            this.linkLabelHomepage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabelHomepage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelHomepage_LinkClicked);
            // 
            // labelServerVersion
            // 
            this.labelServerVersion.Font = new System.Drawing.Font("Verdana", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelServerVersion.Location = new System.Drawing.Point(3, 73);
            this.labelServerVersion.Name = "labelServerVersion";
            this.labelServerVersion.Size = new System.Drawing.Size(389, 37);
            this.labelServerVersion.TabIndex = 6;
            //this.labelServerVersion.Text = "ServerVersion";
            this.labelServerVersion.Text = "Auto update feature is disabled now. Please contact GFriend Team for latest version of GFriend.";
            this.labelServerVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            
            // 
            // labelContact
            // 
            this.labelContact.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelContact.Location = new System.Drawing.Point(-7, 137);
            this.labelContact.Name = "labelContact";
            this.labelContact.Size = new System.Drawing.Size(392, 26);
            this.labelContact.TabIndex = 2;
            this.labelContact.Text = "Contact Here";
            this.labelContact.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelName
            // 
            this.labelName.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelName.Location = new System.Drawing.Point(2, 9);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(390, 30);
            this.labelName.TabIndex = 5;
            this.labelName.Text = "GFriend Volume 2 (Flowerbud)";
            this.labelName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(397, 564);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pictureLogo);
            this.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AboutForm";
            this.Text = "About...";
            this.Load += new System.EventHandler(this.AboutForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureLogo)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureLogo;
        private System.Windows.Forms.Label labelVerion;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelContact;
        private System.Windows.Forms.LinkLabel linkLabelHomepage;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelServerVersion;
        private System.Windows.Forms.Button buttonUpdate;
    }
}