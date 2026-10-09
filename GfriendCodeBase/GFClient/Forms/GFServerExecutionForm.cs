using HP.GFriend.Keywords;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HP.GFriend.Client.Forms
{
    public partial class GFServerExecutionForm : Form
    {
        private List<DeviceUnderTest> _deviceInfoList;
        private DeviceUnderTest _device = new DeviceUnderTest();

        public GFServerConnector GFServer
        {
            get; private set;
        }
        private string _serverAddress = null;
        private List<Script> _scriptInServer;        
        private List<int> _tsIdList;        
        private List<string> emailList = new List<string>();

        private GFClientUtil _gFClientUtil = new GFClientUtil();

        private readonly List<string> SupportedExtension = new List<string>(new string[] { ".txt", ".gfscript", ".gflib" });
        private readonly List<string> ExecuteSupportedExtension = new List<string>(new string[] { ".txt", ".gfscript", ".gflib" });

        // Variable for disabling checkbox about specific node on the tree view
        private const int TVIF_STATE = 0x8;
        private const int TVIS_STATEIMAGEMASK = 0xF000;
        private const int TV_FIRST = 0x1100;
        private const int TVM_SETITEM = TV_FIRST + 63;

        [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Auto)]
        private struct Tvitem
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
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam,
                                                 ref Tvitem lParam);

        public GFServerExecutionForm()
        {
            InitializeComponent();
        }

        public GFServerExecutionForm(Icon icon, List<DeviceUnderTest> deviceList, DeviceUnderTest dut)
        {
            Icon = icon;            

            InitializeComponent();
            GetCurrentUserEmail();
            userInfo_label.Text = Environment.UserName;

            _serverAddress = _gFClientUtil.LoadScriptServerInfoINI();
            gfServerAddr_textBox.Text = _serverAddress;

            _device = dut;
            _deviceInfoList = deviceList;
            SetDeviceIdComboBox();

            ConnectGFSever(_serverAddress);
            ImageList treeIcons = new ImageList();
            treeIcons.Images.Add(Properties.Resources.icoFolder);
            treeIcons.Images.Add(Properties.Resources.icoFile);
            serverList_treeView.ImageList = treeIcons;
            _scriptInServer = new List<Script>();
            _tsIdList = new List<int>();

            serverList_treeView.DrawMode = TreeViewDrawMode.OwnerDrawText;
            serverList_treeView.DrawNode += new DrawTreeNodeEventHandler(tree_DrawNode);

            tsList_dataGridView.ColumnCount = 1;
            tsList_dataGridView.Columns[0].Name = "Test Suites";
            tsList_dataGridView.Columns[0].Width = tsList_dataGridView.Width - 2;
            tsList_dataGridView.ReadOnly = true;

            fileInfo_dataGridView.ColumnCount = 2;            
            fileInfo_dataGridView.Columns[0].Width = 60;            
            fileInfo_dataGridView.Columns[1].Width = fileInfo_dataGridView.Width - 63;
            fileInfo_dataGridView.ReadOnly = true;

            eMail_dataGridView.ColumnCount = 1;            
            eMail_dataGridView.Columns[0].Width = tsList_dataGridView.Width - 2;
            eMail_dataGridView.ReadOnly = true;

            Refresh();
            ActiveControl = tsList_dataGridView;
        }

        public override void Refresh()
        {
            ListDirectory(serverList_treeView, GFServer);
        }

        #region DeviceList Control
        private void SetDeviceIdComboBox()
        {
            if (_deviceInfoList != null)
            {
                deviceId_comboBox.DataSource = GetDeviceIdList();

                if (_device != null)
                {
                    deviceId_comboBox.SelectedItem = _device.DeviceId;
                }
            }
        }

        private DeviceUnderTest GetDUTfromComboBox()
        {            
            foreach (DeviceUnderTest dut in _deviceInfoList)
            {
                if (dut.DeviceId.Equals(deviceId_comboBox.Text))
                {
                    deviceAddress_label.Text = dut.DeviceAddress;
                    return dut;
                }
            }
            return null;
        }

        private List<string> GetDeviceIdList()
        {
            List<string> deviceIdLIst = new List<string>();

            foreach (DeviceUnderTest device in _deviceInfoList)
            {
                deviceIdLIst.Add(device.DeviceId);
            }

            return deviceIdLIst;
        }

        private void deviceId_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (deviceId_comboBox.SelectedIndex >= 0 && deviceId_comboBox.Items.Count > 0)
            {
                var deviceId = deviceId_comboBox.SelectedItem.ToString();
                UpdateDeviceAddresslabel(deviceId);
            }
        }

        /// <summary>
        /// Update device address label to selected device
        /// </summary>
        private void UpdateDeviceAddresslabel(string deviceId)
        {

            if (deviceId_comboBox.SelectedIndex < 0 || deviceId_comboBox.Items.Count == 0)
            {
                deviceAddress_label.Text = "none";
            }
            else
            {
                foreach (DeviceUnderTest dut in _deviceInfoList)
                {
                    if (dut.DeviceId.Equals(deviceId))
                    {                        
                        deviceAddress_label.Text = dut.DeviceAddress;
                        break;
                    }
                }
            }
        }
        #endregion DeviceList Control

        #region ServerList Control
        private void ConnectGFSever(string serverAddr)
        {
            GFServer = new GFServerConnector(serverAddr);                    
        }

        private void Connect_button_Click(object sender, EventArgs e)
        {
            ConnectGFSever(gfServerAddr_textBox.Text);
            GFServerResult serverResult = ListDirectory(serverList_treeView, GFServer); 

            if(serverResult.Status == RequestStatus.Success)
            {
                _serverAddress = gfServerAddr_textBox.Text;
                _gFClientUtil.SaveScriptServerInfoINI(_serverAddress);                
            }
            
        }

        private GFServerResult ListDirectory(TreeView treeView, GFServerConnector gfServer)
        {
            tsList_dataGridView.Rows.Clear();
            treeView.Nodes.Clear();

            GFServerResult result = gfServer.GetScriptList();            

            if (result.Status != RequestStatus.Success)
            {
                MessageBox.Show(result.Message, result.GetStatusDescription());                
                return result;
            }

            _scriptInServer = result.DataList.ConvertAll(o => (Script)o);
            List<Tuple<int, string>> pathInfo = _scriptInServer.Select(c => new Tuple<int, string>(c.Id, c.StoredPath)).ToList();

            TreeNode root = new TreeNode("GF Server") { Tag = "Folder", Name = "Root" };

            pathInfo = pathInfo.OrderBy(o => o.Item2).ToList();

            foreach (Tuple<int, string> scriptInServer in pathInfo)
            {
                string path = scriptInServer.Item2;
                string[] splitted = path.Replace('\\', '/').Split('/');

                if(SupportedExtension.Contains(Path.GetExtension(splitted[0])))
                {
                    TreeNode added = root.Nodes.Add(scriptInServer.Item1.ToString(), path, 1, 1);
                    added.Tag = "File";
                    added.Name = scriptInServer.Item1.ToString();
                }
                else
                {
                    TreeNode currentNode = root;

                    foreach (string obj in splitted)
                    {
                        if (SupportedExtension.Contains(Path.GetExtension(obj)))
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

        private void serverlist_treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {            
            fileInfo_dataGridView.Rows.Clear();

            if (e.Node.Tag.ToString().Equals("File"))
            {
                GFServerResult result = GFServer.GetScriptInfo(int.Parse(e.Node.Name));
                if(result.Status != RequestStatus.Success)
                {
                    MessageBox.Show(result.Message, result.GetStatusDescription());
                }
                Script script = (Script)result.Data;
                fileInfo_dataGridView.Rows.Add("Name", script.ScriptName);
                fileInfo_dataGridView.Rows.Add("Path", script.StoredPath);
                fileInfo_dataGridView.Rows.Add("Author", script.Author);
                fileInfo_dataGridView.Rows.Add("Description", script.Description);
                
            }
        }

        private void serverList_treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Action != TreeViewAction.Unknown)
            {
                e.Node.Checked = !e.Node.Checked;
                serverList_treeView.SelectedNode = null;
            }
        }

        private void serverList_treeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag.ToString().Equals("File"))
            {
                UpdateSelectFileList(int.Parse(e.Node.Name), e.Node.Checked);
            }

            serverList_treeView.AfterCheck -= serverList_treeView_AfterCheck;
            ChildNodeChecking(e.Node);
            ParentNodeChecking(e.Node);
            serverList_treeView.AfterCheck += serverList_treeView_AfterCheck;                    
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

                if (tn.Tag.ToString().Equals("File"))
                {
                    UpdateSelectFileList(int.Parse(tn.Name), tn.Checked);
                }                

                ChildNodeChecking(tn);
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

                        if(string.IsNullOrEmpty(nodeExtension) || ExecuteSupportedExtension.Contains(nodeExtension))
                        {
                            t.Checked = false;
                            break;
                        }
                    }
                }
                ParentNodeChecking(t);
            }
        }

        /// <summary>
        /// Update Selected file list on filelist DataGridView
        /// </summary>
        /// <param name="filepath">File path for each file on file list DataGridView</param>
        /// <param name="fileChecked">File Checked Info for each file on file list DataGridView</param>
        private void UpdateSelectFileList(int id, bool fileChecked)
        {
            GFServerResult result = GFServer.GetScriptInfo(id);
            if (result.Status != RequestStatus.Success)
            {
                MessageBox.Show(result.Message, result.GetStatusDescription());
            }
            Script script = (Script)result.Data;

            if (ExecuteSupportedExtension.Contains(Path.GetExtension(script.StoredPath)))
            {
                if (fileChecked)
                {
                    foreach (DataGridViewRow row in tsList_dataGridView.Rows)
                    {
                        if (row.Cells[0].Value.ToString().Equals(script.StoredPath))
                        {
                            return;
                        }
                    }
                    tsList_dataGridView.Rows.Add(script.StoredPath);
                    _tsIdList.Add(script.Id);
                }
                else
                {
                    foreach (DataGridViewRow row in tsList_dataGridView.Rows)
                    {
                        if (script.StoredPath.Equals(row.Cells[0].Value.ToString()))
                        {
                            tsList_dataGridView.Rows.Remove(row);
                            _tsIdList.Remove(script.Id);
                            break;
                        }
                    }
                }
            }
        }

        void tree_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            var nodeExtension = Path.GetExtension(e.Node.FullPath);
            if (!string.IsNullOrEmpty(nodeExtension) && !ExecuteSupportedExtension.Contains(nodeExtension))
            {
                HideCheckBox(serverList_treeView, e.Node);
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
            Tvitem tvi = new Tvitem();
            tvi.hItem = node.Handle;
            tvi.mask = TVIF_STATE;
            tvi.stateMask = TVIS_STATEIMAGEMASK;
            tvi.state = 0;
            SendMessage(tvw.Handle, TVM_SETITEM, IntPtr.Zero, ref tvi);
        }
        #endregion ServerList Control

        #region Workflow option Control
        private void timeBased_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (timeBased_radioButton.Checked)
            {
                timeBased_maskedTextBox.Enabled = true;
                duration_label.Enabled = true;
                iterations_label.Enabled = false;
                countBased_textBox.Enabled = false;
                hhmm_label.Enabled = false;
            }
            else
            {
                timeBased_maskedTextBox.Enabled = false;
                duration_label.Enabled = false;
                iterations_label.Enabled = true;
                countBased_textBox.Enabled = true;
                hhmm_label.Enabled = true;
            }
        }

        private void countBased_textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
            {
                e.Handled = true;
            }
        }
        #endregion Workflow option Control

        #region Email option Control
        private void GetCurrentUserEmail()
        {
            BackgroundWorker gettingWorker = new BackgroundWorker();
            gettingWorker.DoWork += GettingWorker_DoWork;
            gettingWorker.RunWorkerCompleted += GettingWorker_RunWorkerCompleted;
            gettingWorker.RunWorkerAsync();
        }

        private void GettingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            panelEmailWorking.Visible = false;
        }

        private void GettingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string domain = Environment.UserDomainName;
            string username = Environment.UserName;

            PrincipalContext domainContext = new PrincipalContext(ContextType.Domain, domain);
            UserPrincipal user = UserPrincipal.FindByIdentity(domainContext, username);
            string email = user.EmailAddress;
            if(!string.IsNullOrEmpty(email))
            {
                eMail_dataGridView.Rows.Add(email);
                emailList.Add(email);
            }
        }

        private void email_button_Click(object sender, EventArgs e)
        {
            if(Email_textBox.Text != null)
            {
                eMail_dataGridView.Rows.Add(Email_textBox.Text);
                emailList.Add(Email_textBox.Text);
                Email_textBox.Text = "";
            }
        }

        private void emailRemove_button_Click(object sender, EventArgs e)
        {
            if (eMail_dataGridView.SelectedRows[0] != null)
            {
                emailList.Remove(eMail_dataGridView.SelectedRows[0].ToString());
                eMail_dataGridView.Rows.Remove(eMail_dataGridView.SelectedRows[0]);                
            }
        }

        private void Email_textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Enter) && (Email_textBox.Text != null))
            {
                eMail_dataGridView.Rows.Add(Email_textBox.Text);
                emailList.Add(Email_textBox.Text);
                Email_textBox.Text = "";                
            }
        }
        #endregion Email option Control

        #region Test Control
        private void runtoServer_toolStripButton_Click(object sender, EventArgs e)
        {
            if (_tsIdList.Count < 1)
            {
                MessageBox.Show("Select at least one test suites for test");
                return;
            }

            _device = GetDUTfromComboBox();

            if (_device == null)
            {
                MessageBox.Show("Select device");
                return;
            }
            
            TestProject testProject = new TestProject(_tsIdList, _device);

            testProject.Author = userInfo_label.Text;

            if (emailList.Count > 0)
            {
                testProject.EmailTo = String.Join(",", emailList);
            }
            
            if (timeBased_radioButton.Checked)
            {
                if (string.IsNullOrEmpty(timeBased_maskedTextBox.Text))
                {
                    MessageBox.Show("Duration field should have a value for time based testing");
                    timeBased_maskedTextBox.Focus();
                    return;
                }
                string[] time = timeBased_maskedTextBox.Text.ToString().Split(':');

                int testTime = 0;
                if (!string.IsNullOrEmpty(time[0]) && !time[0].Equals("0") && !time[0].Equals("00") && !time[0].Equals("000"))
                {
                    testTime = Int32.Parse(time[0]) * 60 + Int32.Parse(time[1]);
                }
                else
                {
                    if(string.IsNullOrEmpty(time[1]) || time[1].Equals("0") || time[1].Equals("00") || time[1].Equals("000"))
                    {
                        MessageBox.Show("Duration field should have a value for time based testing");
                        timeBased_maskedTextBox.Focus();
                        return;
                    }
                    testTime = Int32.Parse(time[1]);
                }

                TimeSpan testDuration = TimeSpan.FromMinutes(testTime);
                testProject.SetRepeat(testDuration);
            }
            else
            {
                if (string.IsNullOrEmpty(countBased_textBox.Text))
                {
                    MessageBox.Show("Iterations field should have a value for count based testing");
                    countBased_textBox.Focus();
                    return;
                }
                testProject.SetRepeat(Int32.Parse(countBased_textBox.Text));
            }

            WaitingForm waitingForm = new WaitingForm();
            waitingForm.Show();

            GFServerResult startResult = GFServer.StartTestProject(testProject);
            if(startResult.Status != RequestStatus.Success)
            {
                waitingForm.Close();
                MessageBox.Show(startResult.Message, startResult.GetStatusDescription());
                return;
            }
            testProject = (TestProject)startResult.Data;

            GFServerViewerForm gFServerViewerForm = new GFServerViewerForm(Icon, testProject);
            gFServerViewerForm.Show();

            waitingForm.Close();
            gFServerViewerForm.Focus();
        }
        #endregion Test control

        private void viewServer_toolStripButton_Click(object sender, EventArgs e)
        {
            WaitingForm waitingForm = new WaitingForm();
            waitingForm.Show();

            GFServerViewerForm gFServerViewerForm = new GFServerViewerForm(Icon);
            gFServerViewerForm.Show();

            waitingForm.Close();
            gFServerViewerForm.Focus();
        }
    }
}
