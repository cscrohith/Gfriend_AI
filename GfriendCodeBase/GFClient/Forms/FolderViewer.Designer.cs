namespace HP.GFriend.Client.Forms
{
    partial class FolderViewer
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
            this.splitContainerBaseOfBase = new System.Windows.Forms.SplitContainer();
            this.splitContainerBase = new System.Windows.Forms.SplitContainer();
            this.splitContainerUpper = new System.Windows.Forms.SplitContainer();
            this.treeViewLocal = new System.Windows.Forms.TreeView();
            this.pictureBoxUpload = new System.Windows.Forms.PictureBox();
            this.pictureBoxDownload = new System.Windows.Forms.PictureBox();
            this.treeViewRemote = new System.Windows.Forms.TreeView();
            this.textBoxDetail = new System.Windows.Forms.TextBox();
            this.panelUpload = new System.Windows.Forms.Panel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonUpload = new System.Windows.Forms.Button();
            this.scriptInfo = new HP.GFriend.Client.Forms.ScriptInfo();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerBaseOfBase)).BeginInit();
            this.splitContainerBaseOfBase.Panel1.SuspendLayout();
            this.splitContainerBaseOfBase.Panel2.SuspendLayout();
            this.splitContainerBaseOfBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerBase)).BeginInit();
            this.splitContainerBase.Panel1.SuspendLayout();
            this.splitContainerBase.Panel2.SuspendLayout();
            this.splitContainerBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerUpper)).BeginInit();
            this.splitContainerUpper.Panel1.SuspendLayout();
            this.splitContainerUpper.Panel2.SuspendLayout();
            this.splitContainerUpper.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxUpload)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDownload)).BeginInit();
            this.panelUpload.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerBaseOfBase
            // 
            this.splitContainerBaseOfBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerBaseOfBase.Location = new System.Drawing.Point(0, 0);
            this.splitContainerBaseOfBase.Name = "splitContainerBaseOfBase";
            this.splitContainerBaseOfBase.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerBaseOfBase.Panel1
            // 
            this.splitContainerBaseOfBase.Panel1.Controls.Add(this.splitContainerBase);
            // 
            // splitContainerBaseOfBase.Panel2
            // 
            this.splitContainerBaseOfBase.Panel2.Controls.Add(this.textBoxDetail);
            this.splitContainerBaseOfBase.Size = new System.Drawing.Size(783, 563);
            this.splitContainerBaseOfBase.SplitterDistance = 461;
            this.splitContainerBaseOfBase.TabIndex = 0;
            // 
            // splitContainerBase
            // 
            this.splitContainerBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerBase.Location = new System.Drawing.Point(0, 0);
            this.splitContainerBase.Name = "splitContainerBase";
            // 
            // splitContainerBase.Panel1
            // 
            this.splitContainerBase.Panel1.Controls.Add(this.splitContainerUpper);
            // 
            // splitContainerBase.Panel2
            // 
            this.splitContainerBase.Panel2.Controls.Add(this.treeViewRemote);
            this.splitContainerBase.Size = new System.Drawing.Size(783, 461);
            this.splitContainerBase.SplitterDistance = 417;
            this.splitContainerBase.TabIndex = 1;
            // 
            // splitContainerUpper
            // 
            this.splitContainerUpper.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerUpper.Location = new System.Drawing.Point(0, 0);
            this.splitContainerUpper.Name = "splitContainerUpper";
            // 
            // splitContainerUpper.Panel1
            // 
            this.splitContainerUpper.Panel1.Controls.Add(this.treeViewLocal);
            // 
            // splitContainerUpper.Panel2
            // 
            this.splitContainerUpper.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.splitContainerUpper.Panel2.Controls.Add(this.pictureBoxUpload);
            this.splitContainerUpper.Panel2.Controls.Add(this.pictureBoxDownload);
            this.splitContainerUpper.Size = new System.Drawing.Size(417, 461);
            this.splitContainerUpper.SplitterDistance = 384;
            this.splitContainerUpper.TabIndex = 0;
            // 
            // treeViewLocal
            // 
            this.treeViewLocal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.treeViewLocal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewLocal.Location = new System.Drawing.Point(0, 0);
            this.treeViewLocal.Name = "treeViewLocal";
            this.treeViewLocal.Size = new System.Drawing.Size(384, 461);
            this.treeViewLocal.TabIndex = 0;
            this.treeViewLocal.Tag = "Local";
            this.treeViewLocal.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TreeView_SelectionChanged);
            this.treeViewLocal.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TreeViewLocal_NodeMouseDoubleClick);
            // 
            // pictureBoxUpload
            // 
            this.pictureBoxUpload.Image = global::HP.GFriend.Client.Properties.Resources.upload;
            this.pictureBoxUpload.Location = new System.Drawing.Point(3, 187);
            this.pictureBoxUpload.Name = "pictureBoxUpload";
            this.pictureBoxUpload.Size = new System.Drawing.Size(25, 25);
            this.pictureBoxUpload.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxUpload.TabIndex = 1;
            this.pictureBoxUpload.TabStop = false;
            this.pictureBoxUpload.Click += new System.EventHandler(this.PictureBoxUpload_Click);
            // 
            // pictureBoxDownload
            // 
            this.pictureBoxDownload.Image = global::HP.GFriend.Client.Properties.Resources.download;
            this.pictureBoxDownload.Location = new System.Drawing.Point(1, 127);
            this.pictureBoxDownload.Name = "pictureBoxDownload";
            this.pictureBoxDownload.Size = new System.Drawing.Size(25, 25);
            this.pictureBoxDownload.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxDownload.TabIndex = 0;
            this.pictureBoxDownload.TabStop = false;
            this.pictureBoxDownload.Click += new System.EventHandler(this.PictureBoxDownload_Click);
            // 
            // treeViewRemote
            // 
            this.treeViewRemote.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.treeViewRemote.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewRemote.Location = new System.Drawing.Point(0, 0);
            this.treeViewRemote.Name = "treeViewRemote";
            this.treeViewRemote.Size = new System.Drawing.Size(362, 461);
            this.treeViewRemote.TabIndex = 0;
            this.treeViewRemote.Tag = "Remote";
            this.treeViewRemote.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TreeView_SelectionChanged);
            this.treeViewRemote.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TreeViewRemote_NodeMouseDoubleClick);
            // 
            // textBoxDetail
            // 
            this.textBoxDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxDetail.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.textBoxDetail.Location = new System.Drawing.Point(0, 0);
            this.textBoxDetail.Multiline = true;
            this.textBoxDetail.Name = "textBoxDetail";
            this.textBoxDetail.Size = new System.Drawing.Size(783, 98);
            this.textBoxDetail.TabIndex = 0;
            // 
            // panelUpload
            // 
            this.panelUpload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.panelUpload.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelUpload.Controls.Add(this.buttonCancel);
            this.panelUpload.Controls.Add(this.buttonUpload);
            this.panelUpload.Controls.Add(this.scriptInfo);
            this.panelUpload.Location = new System.Drawing.Point(62, 120);
            this.panelUpload.Name = "panelUpload";
            this.panelUpload.Size = new System.Drawing.Size(616, 266);
            this.panelUpload.TabIndex = 3;
            this.panelUpload.Visible = false;
            // 
            // buttonCancel
            // 
            this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(192)))));
            this.buttonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(248)))), ((int)(((byte)(233)))));
            this.buttonCancel.Location = new System.Drawing.Point(483, 219);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(117, 31);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = false;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // buttonUpload
            // 
            this.buttonUpload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(192)))));
            this.buttonUpload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonUpload.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(248)))), ((int)(((byte)(233)))));
            this.buttonUpload.Location = new System.Drawing.Point(344, 219);
            this.buttonUpload.Name = "buttonUpload";
            this.buttonUpload.Size = new System.Drawing.Size(117, 31);
            this.buttonUpload.TabIndex = 1;
            this.buttonUpload.Text = "Upload";
            this.buttonUpload.UseVisualStyleBackColor = false;
            this.buttonUpload.Click += new System.EventHandler(this.ButtonUpload_Click);
            // 
            // scriptInfo
            // 
            this.scriptInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.scriptInfo.LocalScriptRoot = "";
            this.scriptInfo.Location = new System.Drawing.Point(35, 3);
            this.scriptInfo.Name = "scriptInfo";
            this.scriptInfo.Size = new System.Drawing.Size(547, 210);
            this.scriptInfo.TabIndex = 0;
            // 
            // FolderViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelUpload);
            this.Controls.Add(this.splitContainerBaseOfBase);
            this.Name = "FolderViewer";
            this.Size = new System.Drawing.Size(783, 563);
            this.splitContainerBaseOfBase.Panel1.ResumeLayout(false);
            this.splitContainerBaseOfBase.Panel2.ResumeLayout(false);
            this.splitContainerBaseOfBase.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerBaseOfBase)).EndInit();
            this.splitContainerBaseOfBase.ResumeLayout(false);
            this.splitContainerBase.Panel1.ResumeLayout(false);
            this.splitContainerBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerBase)).EndInit();
            this.splitContainerBase.ResumeLayout(false);
            this.splitContainerUpper.Panel1.ResumeLayout(false);
            this.splitContainerUpper.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerUpper)).EndInit();
            this.splitContainerUpper.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxUpload)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDownload)).EndInit();
            this.panelUpload.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerBaseOfBase;
        private System.Windows.Forms.SplitContainer splitContainerBase;
        private System.Windows.Forms.SplitContainer splitContainerUpper;
        private System.Windows.Forms.TreeView treeViewLocal;
        private System.Windows.Forms.TreeView treeViewRemote;
        private System.Windows.Forms.PictureBox pictureBoxUpload;
        private System.Windows.Forms.PictureBox pictureBoxDownload;
        private System.Windows.Forms.TextBox textBoxDetail;
        private System.Windows.Forms.Panel panelUpload;
        private System.Windows.Forms.Button buttonUpload;
        private ScriptInfo scriptInfo;
        private System.Windows.Forms.Button buttonCancel;
    }
}
