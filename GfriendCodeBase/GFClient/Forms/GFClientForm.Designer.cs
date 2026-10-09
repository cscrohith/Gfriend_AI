namespace HP.GFriend.Client.Forms
{
    partial class GFClientForm
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
            this.panelFolderview = new System.Windows.Forms.Panel();
            this.folderViewerGF = new HP.GFriend.Client.Forms.FolderViewer();
            this.textBoxServerAddr = new System.Windows.Forms.TextBox();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.pictureBoxOpen = new System.Windows.Forms.PictureBox();
            this.panelFolderview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOpen)).BeginInit();
            this.SuspendLayout();
            // 
            // panelFolderview
            // 
            this.panelFolderview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelFolderview.Controls.Add(this.folderViewerGF);
            this.panelFolderview.Location = new System.Drawing.Point(1, 42);
            this.panelFolderview.Name = "panelFolderview";
            this.panelFolderview.Size = new System.Drawing.Size(815, 554);
            this.panelFolderview.TabIndex = 0;
            // 
            // folderViewerGF
            // 
            this.folderViewerGF.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.folderViewerGF.Dock = System.Windows.Forms.DockStyle.Fill;
            this.folderViewerGF.GFServer = null;
            this.folderViewerGF.LocalScriptDirectory = null;
            this.folderViewerGF.Location = new System.Drawing.Point(0, 0);
            this.folderViewerGF.Name = "folderViewerGF";
            this.folderViewerGF.Size = new System.Drawing.Size(815, 554);
            this.folderViewerGF.TabIndex = 0;
            // 
            // textBoxServerAddr
            // 
            this.textBoxServerAddr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxServerAddr.Location = new System.Drawing.Point(440, 8);
            this.textBoxServerAddr.Name = "textBoxServerAddr";
            this.textBoxServerAddr.Size = new System.Drawing.Size(302, 20);
            this.textBoxServerAddr.TabIndex = 2;
            // 
            // buttonConnect
            // 
            this.buttonConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConnect.Location = new System.Drawing.Point(748, 7);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(64, 22);
            this.buttonConnect.TabIndex = 3;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.ButtonConnect_Click);
            // 
            // pictureBoxOpen
            // 
            this.pictureBoxOpen.Image = global::HP.GFriend.Client.Properties.Resources.OpenFolder_16x;
            this.pictureBoxOpen.Location = new System.Drawing.Point(12, 7);
            this.pictureBoxOpen.Name = "pictureBoxOpen";
            this.pictureBoxOpen.Size = new System.Drawing.Size(20, 21);
            this.pictureBoxOpen.TabIndex = 4;
            this.pictureBoxOpen.TabStop = false;
            this.pictureBoxOpen.Click += new System.EventHandler(this.PictureBoxOpen_Click);
            // 
            // GFClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(817, 593);
            this.Controls.Add(this.pictureBoxOpen);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.textBoxServerAddr);
            this.Controls.Add(this.panelFolderview);
            this.Name = "GFClientForm";
            this.Text = "GF Server Viewer";
            this.panelFolderview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOpen)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelFolderview;
        private FolderViewer folderViewerGF;
        private System.Windows.Forms.TextBox textBoxServerAddr;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.PictureBox pictureBoxOpen;
    }
}