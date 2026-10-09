namespace HP.GFriend.UI.Controls
{
    partial class GFScriptFileList
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
            this.filelist_treeView = new System.Windows.Forms.TreeView();
            this.filelist_fileSystemWatcher = new System.IO.FileSystemWatcher();
            ((System.ComponentModel.ISupportInitialize)(this.filelist_fileSystemWatcher)).BeginInit();
            this.SuspendLayout();
            // 
            // filelist_treeView
            // 
            this.filelist_treeView.CheckBoxes = true;
            this.filelist_treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filelist_treeView.ItemHeight = 16;
            this.filelist_treeView.Location = new System.Drawing.Point(0, 0);
            this.filelist_treeView.Name = "filelist_treeView";
            this.filelist_treeView.Size = new System.Drawing.Size(266, 217);
            this.filelist_treeView.TabIndex = 16;
            this.filelist_treeView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.Filelist_treeView_AfterCheck);
            this.filelist_treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.Filelist_treeView_AfterSelect);
            this.filelist_treeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.Filelist_treeView_NodeMouseClick);
            this.filelist_treeView.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.Filelist_treeView_NodeMouseDoubleClick);
            // 
            // filelist_fileSystemWatcher
            // 
            this.filelist_fileSystemWatcher.EnableRaisingEvents = true;
            this.filelist_fileSystemWatcher.IncludeSubdirectories = true;
            this.filelist_fileSystemWatcher.SynchronizingObject = this;
            this.filelist_fileSystemWatcher.Changed += new System.IO.FileSystemEventHandler(this.Filelist_fileSystemWatcher_Changed);
            this.filelist_fileSystemWatcher.Created += new System.IO.FileSystemEventHandler(this.Filelist_fileSystemWatcher_Changed);
            this.filelist_fileSystemWatcher.Deleted += new System.IO.FileSystemEventHandler(this.Filelist_fileSystemWatcher_Changed);
            this.filelist_fileSystemWatcher.Renamed += new System.IO.RenamedEventHandler(this.Filelist_fileSystemWatcher_Changed);
            // 
            // GFScriptFileList
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.filelist_treeView);
            this.Name = "GFScriptFileList";
            this.Size = new System.Drawing.Size(266, 217);
            ((System.ComponentModel.ISupportInitialize)(this.filelist_fileSystemWatcher)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView filelist_treeView;
        private System.IO.FileSystemWatcher filelist_fileSystemWatcher;
    }
}
