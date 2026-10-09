using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HP.GFriend.UI.Controls
{
    public partial class GFScriptFileList : UserControl
    {
        private readonly List<string> ExecuteSupportedExtension = new List<string>(new string[] { ".txt", ".gfscript" });
        private readonly List<string> DefaultSupportedExtension = new List<string>(new string[] { ".txt", ".gfscript", ".gflib", ".gfvar" });
        private List<string> SupportedExtension;
        
        private string _scriptsPath;
        private ImageList _filelistImageList;
        private bool _filewathcherEnabled = true;

        public event EventHandler<TreeViewEventArgs> OnFolderLoad;
        public event EventHandler<TreeViewEventArgs> OnUpdateSelectedFile;
        public event EventHandler<GFGeneralEventArgs> OnAppendToOutput;
        public event EventHandler<GFGeneralEventArgs> OnOpenFile;
        public event EventHandler<GFGeneralEventArgs> OnLoadFolderException;
        


        // Variable for disabling checkbox about specific node on the tree view
        private const int TVIF_STATE = 0x8;
        private const int TVIS_STATEIMAGEMASK = 0xF000;
        private const int TV_FIRST = 0x1100;
        private const int TVM_SETITEM = TV_FIRST + 63;

        private bool _alwaysDisableCheckbox = false;

        [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Auto)]
        private struct TVITEM
        {
            public int mask;
            public IntPtr hItem;
            public int state;
            public int stateMask;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpszText;
            public int cchTextMax;
            public int iImage;
            public int iSelectedImage;
            public int cChildren;
            public IntPtr lParam;
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref TVITEM lParam);

        public GFScriptFileList()
        {
            InitializeComponent();
            SupportedExtension = DefaultSupportedExtension;
        }

        public GFScriptFileList(string scriptPath)
        {
            InitializeComponent();
            SupportedExtension = DefaultSupportedExtension;
            SetScriptPath(scriptPath);
        }

        public void SetScriptPath(string scriptPath, bool disableCheckbox = false, bool onlyDisplayExecutable = false)
        {
            _scriptsPath = scriptPath;
            _alwaysDisableCheckbox = disableCheckbox;
            if(onlyDisplayExecutable)
            {
                SupportedExtension = ExecuteSupportedExtension;
            }
            filelist_fileSystemWatcher.Path = _scriptsPath;
            _filelistImageList = new ImageList();
            _filelistImageList.Images.Add(global::HP.GFriend.UI.Properties.Resources._16);
            _filelistImageList.Images.Add(global::HP.GFriend.UI.Properties.Resources._8);
            filelist_treeView.ImageList = _filelistImageList;

            filelist_treeView.DrawMode = TreeViewDrawMode.OwnerDrawText;
            filelist_treeView.DrawNode += new DrawTreeNodeEventHandler(tree_DrawNode);
            LoadFolder(filelist_treeView, _scriptsPath);
        }

        public void LoadFolder(string scriptPath)
        {
            LoadFolder(filelist_treeView, scriptPath);
        }

        /// <summary>
        /// Load directory on file list treeview
        /// </summary>
        /// <param name="treeView">Entire treeview</param>
        /// <param name="path">Path for loading folder</param>
        private void LoadFolder(TreeView treeView, string path)
        {
            OnFolderLoad?.Invoke(this, null);
            treeView.Nodes.Clear();

            try
            {
                var stack = new Stack<TreeNode>();
                var rootDirectory = new DirectoryInfo(path);
                var node = new TreeNode(rootDirectory.Name) { Tag = rootDirectory };
                stack.Push(node);

                while (stack.Count > 0)
                {
                    var currentNode = stack.Pop();
                    var directoryInfo = (DirectoryInfo)currentNode.Tag;
                    foreach (var directory in directoryInfo.GetDirectories().Where(d => !d.Name.StartsWith(".")))
                    {
                        var childDirectoryNode = new TreeNode(directory.Name) { Tag = directory };
                        childDirectoryNode.ImageIndex = 0;
                        currentNode.Nodes.Add(childDirectoryNode);
                        stack.Push(childDirectoryNode);
                    }
                    foreach (var file in directoryInfo.GetFiles())
                    {
                        if (SupportedExtension.Contains(Path.GetExtension(file.Name)))
                        {
                            var childFileNode = new TreeNode(file.Name) { Tag = file };
                            childFileNode.ImageIndex = 1;
                            childFileNode.SelectedImageIndex = 1;

                            currentNode.Nodes.Add(childFileNode);
                        }
                    }
                }

                treeView.Nodes.Add(node);
                treeView.ExpandAll();

                _scriptsPath = path;
                filelist_fileSystemWatcher.Path = _scriptsPath;
            }
            catch (IOException)
            {
                OnAppendToOutput?.Invoke(null, new GFGeneralEventArgs("* Invalid user environments settings. Create new environments settings file"));
                treeView.Nodes.Clear();
                OnLoadFolderException?.Invoke(null, null);
            }
        }

        
        

        /// <summary>
        /// Get real path from file list TreeView
        /// </summary>
        public string GetRealTestPath(string relativePath)
        {
            if (relativePath.Contains(@"\"))
            {
                int separateIndex = relativePath.IndexOf(@"\");
                return _scriptsPath + relativePath.Substring(separateIndex);
            }
            else
            {
                return _scriptsPath;
            }
        }



        /// <summary>
        /// Update checkboxes at parent node if checkbox at child node is changed
        /// </summary>
        /// <param name="selectNode">Parent Node</param>
        private void ParentNodeChecking(TreeNode selectNode)
        {
            TreeNode t = selectNode.Parent;

            if (t != null)
            {
                t.Checked = true;
                foreach (TreeNode tn in t.Nodes)
                {
                    if (!tn.Checked)
                    {
                        var nodeExtension = Path.GetExtension(tn.FullPath);

                        if (nodeExtension.Equals(string.Empty) || ExecuteSupportedExtension.Contains(nodeExtension))
                        {
                            t.Checked = false;
                            break;
                        }

                    }
                }
                ParentNodeChecking(t);
            }
        }

        void tree_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            var nodeExtension = Path.GetExtension(e.Node.FullPath);
            if ( (!nodeExtension.Equals(string.Empty) && !ExecuteSupportedExtension.Contains(nodeExtension)) ||
                _alwaysDisableCheckbox)
            {
                HideCheckBox(filelist_treeView, e.Node);
                e.DrawDefault = true;
            }
            else
            {
                e.Graphics.DrawString(e.Node.Text, e.Node.TreeView.Font,
                   Brushes.Black, e.Node.Bounds.X, e.Node.Bounds.Y);
            }
        }



        /// <summary>
        /// Hides the checkbox for the specified node on a TreeView control.
        /// </summary>
        private void HideCheckBox(TreeView tvw, TreeNode node)
        {
            TVITEM tvi = new TVITEM();
            tvi.hItem = node.Handle;
            tvi.mask = TVIF_STATE;
            tvi.stateMask = TVIS_STATEIMAGEMASK;
            tvi.state = 0;
            SendMessage(tvw.Handle, TVM_SETITEM, IntPtr.Zero, ref tvi);
        }

        /// <summary>
        /// It makes TreeNode list for auto refresh when file on treeview is changed
        /// </summary>
        /// <param name="nodes">Get the nodes collection</param>
        IEnumerable<TreeNode> Collect(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                yield return node;

                foreach (var child in Collect(node.Nodes))
                    yield return child;
            }
        }

        public IEnumerable<TreeNode> Collect()
        {
            return Collect(filelist_treeView.Nodes);
        }

        #region EventHandlers

        private void Filelist_fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (!_filewathcherEnabled) return;
            List<string> nodeList = new List<string>();

            foreach (TreeNode node in Collect(filelist_treeView.Nodes))
            {
                if (node.Checked)
                {
                    nodeList.Add(node.FullPath);
                }
            }
            LoadFolder(filelist_treeView, _scriptsPath);

            foreach (TreeNode node in Collect(filelist_treeView.Nodes))
            {
                if (nodeList.Contains(node.FullPath))
                {
                    node.Checked = true;
                }
            }
        }


        /// <summary>
        /// Node Mouse Double Click event for file list Tree View
        /// Open selected file to text editor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Filelist_treeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (SupportedExtension.Contains(Path.GetExtension(e.Node.Text)))
            {
                OnOpenFile?.Invoke(this, new GFGeneralEventArgs(GetRealTestPath(e.Node.FullPath)));
            }
        }

        /// <summary>
        /// Node Right Click event for file list Tree View
        /// Open selected file to text editor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Filelist_treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ContextMenuStrip treeContextmenu = new ContextMenuStrip();
                ToolStripSeparator treeContextmenuSeparator = new ToolStripSeparator();
                ToolStripSeparator treeContextmenuSeparator2 = new ToolStripSeparator();
                ToolStripMenuItem treeContextmenuNewFile = new ToolStripMenuItem("New File");
                ToolStripMenuItem treeContextmenuOpenFile = new ToolStripMenuItem("Open File");
                ToolStripMenuItem treeContextmenuNewFolder = new ToolStripMenuItem("New Folder");
                ToolStripMenuItem treeContextmenuOpenFolder = new ToolStripMenuItem("Open Folder");
                ToolStripMenuItem treeContextmenuOpenFolderInFileExplorer = new ToolStripMenuItem("Open Folder in File Explorer");
                ToolStripMenuItem treeContextmenuRemoveFile = new ToolStripMenuItem("Remove File");
                ToolStripMenuItem treeContextmenuRemoveFolder = new ToolStripMenuItem("Remove Folder");
                treeContextmenuNewFile.Click += new EventHandler(treeContextmenuNewFile_Click);
                treeContextmenuOpenFile.Click += new EventHandler(treeContextmenuOpenFile_Click);
                treeContextmenuNewFolder.Click += new EventHandler(treeContextmenuNewFolder_Click);
                treeContextmenuOpenFolder.Click += new EventHandler(treeContextmenuOpenFolder_Click);
                treeContextmenuRemoveFile.Click += new EventHandler(treeContextmenuRemoveFile_Click);
                treeContextmenuRemoveFolder.Click += new EventHandler(treeContextmenuRemoveFolder_Click);
                treeContextmenuOpenFolderInFileExplorer.Click += new EventHandler(treeContextmenuOpenFolderInFileExplorer_Click);

                if (SupportedExtension.Contains(Path.GetExtension(e.Node.Text)))
                {
                    filelist_treeView.SelectedNode = e.Node;
                    treeContextmenu.Items.AddRange(new ToolStripItem[] { treeContextmenuOpenFile, treeContextmenuRemoveFile, treeContextmenuSeparator, treeContextmenuOpenFolder, treeContextmenuOpenFolderInFileExplorer });
                }

                if (Directory.Exists(GetRealTestPath(e.Node.FullPath)))
                {
                    filelist_treeView.SelectedNode = e.Node;
                    treeContextmenu.Items.AddRange(new ToolStripItem[] { treeContextmenuNewFile, treeContextmenuSeparator, treeContextmenuNewFolder, treeContextmenuRemoveFolder, treeContextmenuSeparator2, treeContextmenuOpenFolder, treeContextmenuOpenFolderInFileExplorer });
                }
                treeContextmenu.Show(Cursor.Position);

            }
        }

        /// <summary>
        /// After Select event for file list Tree View
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Filelist_treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Action != TreeViewAction.Unknown)
            {
                e.Node.Checked = !e.Node.Checked;
                filelist_treeView.SelectedNode = null;
            }
        }

        /// <summary>
        /// After Check event for file list Tree View
        /// Add items to file list DataGridView with their children
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Filelist_treeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            OnUpdateSelectedFile?.Invoke(this, e);

            filelist_treeView.AfterCheck -= Filelist_treeView_AfterCheck;
            ChildNodeChecking(e.Node);
            ParentNodeChecking(e.Node);
            filelist_treeView.AfterCheck += Filelist_treeView_AfterCheck;
        }

        /// <summary>
        /// Update checkboxes at all children node if checkbox at parent node is changed
        /// </summary>
        /// <param name="selectNode">Parent Node</param>
        private void ChildNodeChecking(TreeNode selectNode)
        {
            foreach (TreeNode tn in selectNode.Nodes)
            {
                tn.Checked = selectNode.Checked;
                TreeViewEventArgs args = new TreeViewEventArgs(tn);
                OnUpdateSelectedFile?.Invoke(this, args);

                ChildNodeChecking(tn);
            }
        }
        #endregion


        #region TreeContextMenuControl
        /// <summary>
        /// Click new file button on file list tree view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeContextmenuNewFile_Click(object sender, EventArgs e)
        {
            string directoryPath = GetRealTestPath(filelist_treeView.SelectedNode.FullPath);

            if (SupportedExtension.Contains(Path.GetExtension(directoryPath)))
            {
                return;
            }

            DialogResult dr = new DialogResult();

            InputBox newFile = new InputBox("New file", "File name: ");

            newFile.Location = Cursor.Position;
            dr = newFile.ShowDialog(this);

            switch (dr)
            {
                case DialogResult.OK:
                    if (!string.IsNullOrEmpty(newFile.textTextBox))
                    {
                        try
                        {
                            string filePath = null;

                            if (string.IsNullOrEmpty(Path.GetExtension(newFile.textTextBox)))
                            {
                                filePath = Path.Combine(directoryPath, newFile.textTextBox + ".txt");
                            }
                            else
                            {
                                filePath = Path.Combine(directoryPath, newFile.textTextBox);
                            }
                            if (File.Exists(filePath))
                            {
                                MessageBox.Show($"{Path.GetDirectoryName(filePath)} already exist. Fail to create new file.");
                            }

                            FileStream file = File.Create(filePath);
                            file.Close();

                            OnOpenFile?.Invoke(this, new GFGeneralEventArgs(filePath));
                            
                        }
                        catch (ArgumentException)
                        {
                            MessageBox.Show("File name is invalid. Fail to create new file.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("File name is empty. Fail to create new file.");
                    }
                    break;
            }
        }

        /// <summary>
        /// Click open file button on file list tree view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeContextmenuOpenFile_Click(object sender, EventArgs e)
        {
            OnOpenFile?.Invoke(this, new GFGeneralEventArgs(GetRealTestPath(filelist_treeView.SelectedNode.FullPath)));
        }

        /// <summary>
        /// Click new folder button on file list tree view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeContextmenuNewFolder_Click(object sender, EventArgs e)
        {
            string directoryPath = GetRealTestPath(filelist_treeView.SelectedNode.FullPath);

            if (SupportedExtension.Contains(Path.GetExtension(directoryPath)))
            {
                return;
            }

            DialogResult dr = new DialogResult();

            InputBox newFolder = new InputBox("New folder", "Folder name: ");

            newFolder.Location = Cursor.Position;
            dr = newFolder.ShowDialog(this);

            switch (dr)
            {
                case DialogResult.OK:
                    if (!string.IsNullOrEmpty(newFolder.textTextBox))
                    {
                        try
                        {
                            directoryPath = Path.Combine(directoryPath, newFolder.textTextBox);
                            Directory.CreateDirectory(directoryPath);
                        }
                        catch (ArgumentException)
                        {
                            MessageBox.Show("Foder name is invalid. Fail to create new folder");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Foder name is empty. Fail to create new folder");
                    }
                    break;
            }
        }

        /// <summary>
        /// Click open Folder button on context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeContextmenuOpenFolder_Click(object sender, EventArgs e)
        {
            string directoryPath = GetRealTestPath(filelist_treeView.SelectedNode.FullPath);

            if (SupportedExtension.Contains(Path.GetExtension(directoryPath)))
            {
                directoryPath = Path.GetDirectoryName(directoryPath);
            }

            if (!_scriptsPath.Equals(directoryPath))
            {
                LoadFolder(filelist_treeView, directoryPath);
                OnAppendToOutput?.Invoke(null, new GFGeneralEventArgs($" * Open folder: {_scriptsPath}"));
            }
        }

        /// <summary>
        /// Click open folder in file explorer button on context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeContextmenuOpenFolderInFileExplorer_Click(object sender, EventArgs e)
        {
            string directoryPath = GetRealTestPath(filelist_treeView.SelectedNode.FullPath);

            if (SupportedExtension.Contains(Path.GetExtension(directoryPath)))
            {
                directoryPath = Path.GetDirectoryName(directoryPath);
            }

            if (Directory.Exists(directoryPath))
            {
                Process process = Process.Start(directoryPath);
            }
        }

        /// <summary>
        /// Click remove file button on file list tree view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeContextmenuRemoveFile_Click(object sender, EventArgs e)
        {
            if (File.Exists(GetRealTestPath(filelist_treeView.SelectedNode.FullPath)))
            {
                File.Delete(GetRealTestPath(filelist_treeView.SelectedNode.FullPath));
            }

        }

        /// <summary>
        /// Click remove folder button on file list tree view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeContextmenuRemoveFolder_Click(object sender, EventArgs e)
        {
            if (filelist_treeView.SelectedNode == filelist_treeView.TopNode)
            {
                MessageBox.Show($"Remove denied. \"{filelist_treeView.SelectedNode.FullPath}\" is root to this file system.");
                return;
            }

            if (Directory.Exists(GetRealTestPath(filelist_treeView.SelectedNode.FullPath)))
            {
                Directory.Delete(GetRealTestPath(filelist_treeView.SelectedNode.FullPath), true);
            }
        }


        public void HoldFileSystemWatcher()
        {
            _filewathcherEnabled = false;
        }

        public void ResumeFileSystemWatcher()
        {
            _filewathcherEnabled = true;
        }
        #endregion
    }
}
