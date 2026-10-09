using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HP.GFriend.Client.Forms
{
    [Docking(DockingBehavior.Ask)]
    public partial class FolderViewer : UserControl
    {
        public string LocalScriptDirectory { get; set; }
        public GFServerConnector GFServer { get; set; }

        private readonly List<string> SupportedExtension = new List<string>(new string[] { ".txt", ".gfscript", ".gflib" });

        private string _selectedLocal;
        private List<int> _toDownload;

        private List<Script> _scriptInServer;

        public FolderViewer()
        {
            InitializeComponent();
            ImageList treeIcons = new ImageList();
            treeIcons.Images.Add(Properties.Resources.icoFolder);
            treeIcons.Images.Add(Properties.Resources.icoFile);
            treeViewLocal.ImageList = treeIcons;
            treeViewRemote.ImageList = treeIcons;
            _toDownload = new List<int>();
            _scriptInServer = new List<Script>();
        }

        public FolderViewer(string localScriptDirectory, GFServerConnector gfServer) : this()
        {
            LocalScriptDirectory = localScriptDirectory;
            scriptInfo.LocalScriptRoot = localScriptDirectory;
            GFServer = gfServer;
        }

        public override void Refresh()
        {
            ListDirectory(treeViewLocal, LocalScriptDirectory);
            ListDirectory(treeViewRemote, GFServer);
        }

        public void RefreshLocal()
        {
            ListDirectory(treeViewLocal, LocalScriptDirectory);
        }

        public GFServerResult RefreshRemote()
        {
            return ListDirectory(treeViewRemote, GFServer);
        }

        private GFServerResult ListDirectory(TreeView treeView, GFServerConnector gfServer)
        {
            treeView.Nodes.Clear();
            GFServerResult result = gfServer.GetScriptList();
            if(result.Status != RequestStatus.Success)
            {
                MessageBox.Show(result.Message, result.GetStatusDescription());
                return result;
            }

            _scriptInServer = result.DataList.ConvertAll(o => (Script)o);
            List<Tuple<int, string>> pathInfo = _scriptInServer.Select(c => new Tuple<int, string>(c.Id, c.StoredPath)).ToList();
            TreeNode root = new TreeNode("GF Server") { Tag = "Folder", Name = "Root" };

            pathInfo = pathInfo.OrderBy(o => o.Item2).ToList();
            foreach(Tuple<int,string> scriptInServer in pathInfo)
            {
                string path = scriptInServer.Item2;
                string[] splitted = path.Replace('\\', '/').Split('/');

                // handing files in root
                if(SupportedExtension.Any(c=> splitted[0].Contains(c)))
                {
                    TreeNode added = root.Nodes.Add(scriptInServer.Item1.ToString(), path, 1, 1);
                    added.Tag = "File";
                    added.Name = scriptInServer.Item1.ToString();

                }
                else
                {
                    TreeNode currentNode = root;
                    
                    foreach(string obj in splitted)
                    {
                        if (SupportedExtension.Any(c => obj.Contains(c)))
                        {
                            TreeNode added = currentNode.Nodes.Add(scriptInServer.Item1.ToString(), obj, 1, 1);
                            added.Tag = "File";
                            added.Name = scriptInServer.Item1.ToString();
                        }
                        else
                        {
                            if (currentNode.Nodes.ContainsKey(obj))
                            {
                                currentNode = currentNode.Nodes[obj];
                            }
                            else
                            {
                                currentNode = currentNode.Nodes.Add(obj, obj, 0);
                                currentNode.Tag = "Folder";
                                currentNode.Name = obj;

                            }
                        }
                        

                    }
                }
            }
            treeView.Nodes.Add(root);
            treeView.ExpandAll();

            return result;
        }

        private void ListDirectory(TreeView treeView, string path)
        {
            scriptInfo.LocalScriptRoot = path;
            treeView.Nodes.Clear();
            try
            {
                Stack <TreeNode> stack = new Stack<TreeNode>();
                DirectoryInfo rootDirectory = new DirectoryInfo(path);
                TreeNode node = new TreeNode(rootDirectory.Name) { Tag = rootDirectory };
                stack.Push(node);

                while (stack.Count > 0)
                {
                    var currentNode = stack.Pop();
                    var directoryInfo = (DirectoryInfo)currentNode.Tag;
                    foreach (var directory in directoryInfo.GetDirectories())
                    {
                        var childDirectoryNode = new TreeNode(directory.Name) { Tag = directory };
                        childDirectoryNode.ImageIndex = 0;
                        currentNode.Nodes.Add(childDirectoryNode);
                        stack.Push(childDirectoryNode);
                    }
                    foreach (var file in directoryInfo.GetFiles())
                    {
                        if (SupportedExtension.Any(c => file.Name.Contains(c)))
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
            }
            catch (IOException e)
            {
                MessageBox.Show(e.ToString());
                treeView.Nodes.Clear();
            }
        }

        private void TreeView_SelectionChanged(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeView t = (TreeView)sender;
            if(t.Tag.ToString().Equals("Local"))
            {
                _selectedLocal = ((FileSystemInfo)(e.Node.Tag)).FullName;
            }
            else if (t.Tag.ToString().Equals("Remote"))
            {
                if(e.Node.Tag.ToString().Equals("Folder"))
                {
                    _toDownload.Clear();
                    AddSubItemsToDownloadList(e.Node);
                    textBoxDetail.Clear();
                }
                else if(e.Node.Tag.ToString().Equals("File"))
                {
                    _toDownload.Clear();
                    _toDownload.Add(int.Parse(e.Node.Name));
                    textBoxDetail.Text = GFServer.GetScriptInfo(int.Parse(e.Node.Name)).Data.ToString();
                }
            }
        }

        private void AddSubItemsToDownloadList(TreeNode node)
        {
            if(node.Tag.ToString().Equals("File"))
            {
                _toDownload.Add(int.Parse(node.Name));
                return;
            }

            foreach(TreeNode child in node.Nodes)
            {
                AddSubItemsToDownloadList(child);
            }
        }

        private void DownloadFromServer()
        {
            foreach(int id in _toDownload)
            {
                GFServer.GetScript(id, LocalScriptDirectory);
            }
            RefreshLocal();
        }

        private void PictureBoxDownload_Click(object sender, EventArgs e)
        {
            DownloadFromServer();
        }


        private void PictureBoxUpload_Click(object sender, EventArgs e)
        {
            if (SupportedExtension.Any(c => _selectedLocal.Contains(c)))
            {
                scriptInfo.SetData(_selectedLocal);
                panelUpload.Visible = true;
                buttonUpload.Text = "Upload";
            }
            else
            {
                MessageBox.Show("Please select script file to upload");
            }
        }

        private void ButtonUpload_Click(object sender, EventArgs e)
        {

            if(buttonUpload.Text.Equals("Upload"))
            {
                Script script = scriptInfo.Get();
                if (_scriptInServer.Where(s => s.StoredPath.Equals(script.StoredPath)).Count() > 0)
                {
                    // Update
                    int id = _scriptInServer.Where(s => s.StoredPath.Equals(script.StoredPath)).First().Id;
                    script.Id = id;
                    GFServer.UpdateScriptInfo(script);
                    GFServer.UpdateScriptFile(script);
                }
                else
                {
                    //Upload
                    GFServer.UploadScript(script);
                }

                RefreshRemote();
                panelUpload.Visible = false;
            }
            else if(buttonUpload.Text.Equals("Update"))
            {
                Script script = scriptInfo.Get();
                GFServer.UpdateScriptInfo(script);
                panelUpload.Visible = false;
            }
            
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            panelUpload.Visible = false;
        }

        private void TreeViewLocal_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (SupportedExtension.Any(c => _selectedLocal.Contains(c)))
            {
                scriptInfo.SetData(_selectedLocal);
                panelUpload.Visible = true;
                buttonUpload.Text = "Upload";
            }
         
        }

        private void TreeViewRemote_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag.ToString().Equals("File"))
            {
                GFServerResult result = GFServer.GetScriptInfo(int.Parse(e.Node.Name));
                if(result.Status != RequestStatus.Success)
                {
                    MessageBox.Show(result.Message, result.GetStatusDescription());
                    return;
                }
                Script s = (Script)result.Data;
                scriptInfo.SetData(s);
                panelUpload.Visible = true;
                buttonUpload.Text = "Update";
            }
        }
    }
}
