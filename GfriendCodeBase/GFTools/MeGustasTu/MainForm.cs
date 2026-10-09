using HP.DeviceAutomation;
using HP.DeviceAutomation.Jedi;
using HP.DeviceAutomation.Jedi.OmniUserInteraction;
using HP.DeviceAutomation.Jedi.Oxpd.Test;
using HP.DeviceAutomation.Sirius;
using HP.GFriend.Utils.XPath;
using HP.Automation.SES;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using HP.DeviceAutomation.Dune;

namespace HP.GFriend.Tool
{
    public partial class MainForm : Form
    {
        private const int _defaultSpaceForDuneHierarchy = 4;
        private IDevice _device;

        // Omni
        private OmniTesterClient _omni;
        private OxpdTestClient _oxpd;

        //dune
        private DuneDevice _dune;
        private ProSelectDialControlPanel _dialUI;
        private WorkflowControlPanel _workflowUI;

        // Sirius
        private SiriusUIv2ControlPanel _siriusV2;
        private SiriusUIv3ControlPanel _siriusV3;
        private WidgetCollection _widgets;
        private SESLib _android;

        private bool _isConnected;
        private bool _omniUsed;
        private bool _androidUsed;
        private bool _sirius2Used;
        private bool _sirius3Used;
        private bool _duneUsed;

        private string _dumpXML = null;
        private List<string> _omniIds;
        private List<string> _duneIds;
        private List<string> _duneNodes;
        private Dictionary<int, NativeElement> _nativeElements;
        private string _duneSearchedNode = null;

        private XmlDocument _xmlDocument;
        private XPathBuilder _xPathBuilder;

        private byte[] _screenShot = null;
        private Dictionary<TreeNode, Dictionary<string, string>> _nodeInfo;
        private Rectangle _redBox;
        private int _mouseX, _mouseY;
        private int _imgStartX, _imgStartY;
        private double _resizeFactor;

        private Image _screenShotImg;

        private string duneXmlFileName = Directory.GetCurrentDirectory() + "\\dunexml.xml";
        private const string XmlNodeTag = "node";
        private const string XmlNodeTextAtt = "text";
        private const string XmlNodeTagAtt = "tag";
        private const string XmlNodeImageIndexAtt = "imageindex";

        public MainForm()
        {
            InitializeComponent();
            _isConnected = false;
            _androidUsed = false;
            _omniUsed = false;
            _duneUsed = false;
            _nativeElements = new Dictionary<int, NativeElement>();
            _nodeInfo = new Dictionary<TreeNode, Dictionary<string, string>>();
            _omniIds = new List<string>();
            _duneIds = new List<string>();
            _duneNodes = new List<string>();
            ResizeDataGridViewColumns();
            _redBox = new Rectangle(0, 0, 0, 0);
            CalculateResizeFactors();

            _xPathBuilder = new XPathBuilder();
            _xPathBuilder.AddBlackListAttribute(new List<string>() { "bounds" });
            _xPathBuilder.AddFirstPriorityAttribute(new List<string>() { "resource-id" });
            DeviceAutomation.Logger.OnDebug += LogDebug;
            DeviceAutomation.Logger.OnError += LogError;
            DeviceAutomation.Logger.OnTrace += LogTrace;
            DeviceAutomation.Logger.OnWarn += LogWarn;
        }

        public MainForm(string deviceId, string adminPassword)
        {
            InitializeComponent();
            _isConnected = false;
            _omniUsed = false;
            _duneUsed = false;
            _nativeElements = new Dictionary<int, NativeElement>();
            _nodeInfo = new Dictionary<TreeNode, Dictionary<string, string>>();
            _omniIds = new List<string>();
            _duneIds = new List<string>();
            _duneNodes = new List<string>();
            ResizeDataGridViewColumns();
            _redBox = new Rectangle(0, 0, 0, 0);
            CalculateResizeFactors();

            _xPathBuilder = new XPathBuilder();
            _xPathBuilder.AddBlackListAttribute(new List<string>() { "bounds" });
            _xPathBuilder.AddFirstPriorityAttribute(new List<string>() { "resource-id" });

            DeviceAutomation.Logger.OnDebug += LogDebug;
            DeviceAutomation.Logger.OnError += LogError;
            DeviceAutomation.Logger.OnTrace += LogTrace;
            DeviceAutomation.Logger.OnWarn += LogWarn;

            textBoxIpAddress.Text = deviceId;
            textBoxPassword.Text = adminPassword;
            Show();
            if (!string.IsNullOrEmpty(deviceId))
            {
                try
                {
                    ButtonConnect_Click(null, null);
                }
                catch (Exception ex)
                {
                    AppendToOutput(ex.ToString());
                }
            }
        }

        #region Control Events : Button Click
        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            loadingPictureBox.Visible = false;
            duneSearch.Visible = false;
            duneSearchTextBox.Visible = false;
            if (_isConnected)
            {
                if (_androidUsed)
                {
                    _android.Disconnect();
                    _android.Dispose();
                }

                if (_omniUsed)
                {
                    try
                    {
                        _omni.Disconnect();
                    }
                    catch (Exception)
                    {
                        // ADB status can make this exception. It doesn't affect to use this tool.
                    }
                }
                if (_duneUsed)
                {
                    _dune.Dispose();
                }
                buttonConnect.Text = "Connect";
                _isConnected = false;
                toolStripButtonDump.Enabled = false;
                AppendToOutput("Disconnected");

            }
            else
            {
                try
                {
                    if (!string.IsNullOrEmpty(ADB.Connect(textBoxIpAddress.Text)))
                    {
                        _android = SESLib.Create(textBoxIpAddress.Text);

                        if (_android != null)
                        {
                            _android.Connect();
                            _androidUsed = true;
                            AppendToOutput("Android connected");
                        }
                    }
                    else
                    {
                        _android = null;
                    }

                }
                catch (Exception)
                {
                    // ADB status can make this exception. It doesn't affect to use this tool.
                }

                try
                {
                    if (!string.IsNullOrEmpty(textBoxPassword.Text))
                    {
                        _device = DeviceFactory.Create(textBoxIpAddress.Text, textBoxPassword.Text);

                        if (_device is JediOmniDevice)
                        {
                            _oxpd = new OxpdTestClient(textBoxIpAddress.Text);
                            _oxpd.AdminPassword = textBoxPassword.Text;
                            NetworkConfiguration.ConfigureCertificateAcceptance();
                            _omni = new OmniTesterClient(textBoxIpAddress.Text, TimeSpan.FromSeconds(10));
                            try
                            {
                                _omni.Connect();
                            }
                            catch (Exception)
                            {
                                WebInspector w;
                                w = new WebInspector(textBoxIpAddress.Text, 9222, TimeSpan.FromSeconds(30));
                                w.ForceDisconnect(w.DiscoverInspectablePages().ToList().First());
                                _omni.Connect();
                            }

                            _omniUsed = true;
                            AppendToOutput("Omni connected");
                        }
                        else if (_device is DuneDevice)
                        {
                            _dune = new DuneDevice(textBoxIpAddress.Text, textBoxPassword.Text);
                            if (_dune.ControlPanel.GetType().Equals(typeof(ProSelectDialControlPanel)))
                            {
                                _dialUI = _dune.ControlPanel as ProSelectDialControlPanel;
                            }
                            else if (_dune.ControlPanel.GetType().Equals(typeof(WorkflowControlPanel)))
                            {
                                _workflowUI = _dune.ControlPanel as WorkflowControlPanel;
                            }
                            _duneUsed = true;

                            duneSearch.Visible = true;
                            duneSearchTextBox.Visible = true;
                        }
                        else if (_device is SiriusUIv3Device)
                        {
                            _siriusV3 = ((SiriusUIv3Device)_device).ControlPanel;
                            _sirius3Used = true;
                        }
                        else if (_device is SiriusUIv2Device)
                        {
                            _siriusV2 = ((SiriusUIv2Device)_device).ControlPanel;
                            _sirius2Used = true;
                        }
                        else
                        {
                            AppendToOutput("Not supported device type!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    AppendToOutput($"connection error : {ex}");
                }

                _isConnected = _omniUsed || _sirius2Used || _sirius3Used || _androidUsed || _duneUsed;

                if (_isConnected)
                {
                    toolStripButtonDump.Enabled = true;
                    buttonConnect.Text = "Disconnect";
                }

                else
                {
                    AppendToOutput("Connection error");
                }
            }

        }


        private void ToolStripButtonDump_Click(object sender, EventArgs e)
        {
            duneTreeView.Visible = false;
            loadingPictureBox.Visible = false;
            duneSearch.Visible = false;
            duneSearchTextBox.Visible = false;
            try
            {
                if (_omniUsed)
                {
                    _screenShot = _oxpd.GetScreenCapture();
                    _omniIds = _omni.GetAllIdsOnPage().ToList();
                }
                else if (_duneUsed)
                {
                    _redBox.X = 0; _redBox.Y = 0; _redBox.Width = 0; _redBox.Height = 0;
                    _duneIds.Clear();
                    duneTreeView.Visible = true;
                    duneSearch.Visible = true;
                    duneSearchTextBox.Visible = true;
                    duneSearchTextBox.Text = "";
                    Image _duneImage = _dune.ControlPanel.ScreenCapture();
                    ImageConverter _imageConverter = new ImageConverter();
                    _screenShot = (byte[])_imageConverter.ConvertTo(_duneImage, typeof(byte[]));
                    _duneIds = _workflowUI.GetTreeHierarchyMethod().ToList();
                }
                else if (_sirius2Used)
                {
                    ImageConverter converter = new ImageConverter();
                    _screenShot = (byte[])converter.ConvertTo(_siriusV2.ScreenCapture(), typeof(byte[]));
                    _widgets = _siriusV2.GetScreenInfo().Widgets;
                }
                else if (_sirius3Used)
                {
                    ImageConverter converter = new ImageConverter();
                    _screenShot = (byte[])converter.ConvertTo(_siriusV3.ScreenCapture(), typeof(byte[]));
                    _widgets = _siriusV3.GetScreenInfo().Widgets;
                }
                else
                {
                    _screenShot = _android.GetScreenCapture();

                }
                if (_androidUsed)
                {
                    try
                    {
                        _dumpXML = _android.GetUIDump();
                    }
                    catch (Exception ex)
                    {
                        _androidUsed = false;
                        AppendToOutput($"Android UI dump error : {ex}");
                    }

                }

                UpdateUI();
            }
            catch (Exception ex)
            {
                AppendToOutput($"UI dump error : {ex}");
            }


        }

        #endregion

        #region From Event
        private void TesterMainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_isConnected)
            {
                if (_androidUsed)
                {
                    _android.Disconnect();
                    _android.Dispose();
                }


                if (_omniUsed)
                {
                    _omni.Disconnect();
                }
            }
        }

        #endregion

        #region Picturebox Related Events
        private void CalculateResizeFactors()
        {
            Image i = pictureBoxScreenShot.Image;
            if (i == null) return;
            int oriWidth = (int)(i.PhysicalDimension.Width);
            int oriHeight = (int)(i.PhysicalDimension.Height);
            int dipWidth, dipHeight;


            var wfactor = (double)i.Width / pictureBoxScreenShot.ClientSize.Width;
            var hfactor = (double)i.Height / pictureBoxScreenShot.ClientSize.Height;

            _resizeFactor = Math.Max(wfactor, hfactor);
            dipWidth = (int)(i.Width / _resizeFactor);
            dipHeight = (int)(i.Height / _resizeFactor);
            _imgStartX = (pictureBoxScreenShot.Width - dipWidth) / 2;
            _imgStartY = (pictureBoxScreenShot.Height - dipHeight) / 2;

        }

        private void PictureBoxScreenShot_MouseMove(object sender, MouseEventArgs e)
        {
            Image i = pictureBoxScreenShot.Image;
            if (i == null) return;

            _mouseX = (int)((e.X - _imgStartX) * _resizeFactor);
            _mouseY = (int)((e.Y - _imgStartY) * _resizeFactor);

            labelMouse.Text = $"({_mouseX} , {_mouseY})";
        }

        private void PictureBoxScreenShot_Resize(object sender, EventArgs e)
        {
            _redBox.X = 0; _redBox.Y = 0; _redBox.Width = 0; _redBox.Height = 0;
            CalculateResizeFactors();
        }

        private void PictureBoxScreenShot_Click(object sender, EventArgs e)
        {
            int minRect = int.MaxValue;
            if (tabControlUiSelection.SelectedTab.Equals(tabPageAndroid))
            {
                foreach (KeyValuePair<TreeNode, Dictionary<string, string>> kv in _nodeInfo)
                {
                    if (kv.Value.ContainsKey("bounds"))
                    {
                        if (IsInBounds(_mouseX, _mouseY, kv.Value["bounds"]))
                        {
                            int size = GetSize(kv.Value["bounds"]);
                            if (minRect > size)
                            {
                                treeViewXML.SelectedNode = kv.Key;
                                minRect = size;
                            }

                        }
                    }
                }
            }
            else
            {
                foreach (KeyValuePair<int, NativeElement> kv in _nativeElements)
                {
                    if (IsInBounds(_mouseX, _mouseY, kv.Value.GetBoundString()))
                    {
                        int size = kv.Value.GetSize();
                        if (minRect > size)
                        {
                            listBoxOmniIds.SelectedIndex = kv.Key;
                            minRect = size;
                        }

                    }
                }
            }

        }



        private void PictureBoxScreenShot_Paint(object sender, PaintEventArgs e)
        {
            if (_redBox.X + _redBox.Y + _redBox.Width + _redBox.Height > 0)
            {
                using (Pen pen = new Pen(Color.Red, 3))
                {
                    e.Graphics.DrawRectangle(pen, _redBox);
                }
            }
        }


        private void DrawRedBox(string bounds)
        {
            string start = bounds.Split(']')[0].Replace("[", "");
            string end = bounds.Split('[')[2].Replace("]", "");

            int x, y, w, h;
            int x2, y2;

            x = (int)(int.Parse(start.Split(',')[0]) / _resizeFactor) + _imgStartX;
            y = (int)(int.Parse(start.Split(',')[1]) / _resizeFactor) + _imgStartY;
            x2 = (int)(int.Parse(end.Split(',')[0]) / _resizeFactor) + _imgStartX;
            y2 = (int)(int.Parse(end.Split(',')[1]) / _resizeFactor) + _imgStartY;


            w = x2 - x;
            h = y2 - y;

            _redBox.X = x;
            _redBox.Y = y;
            _redBox.Width = w;
            _redBox.Height = h;
            pictureBoxScreenShot.Refresh();

        }

        private void DrawDuneRedBox(string bounds)
        {
            string start = bounds.Split(']')[0].Replace("[", "");
            string end = bounds.Split('[')[2].Replace("]", "");

            int x, y, w, h;

            x = (int)(int.Parse(start.Split(',')[0]) / _resizeFactor) + _imgStartX;
            y = (int)(int.Parse(start.Split(',')[1]) / _resizeFactor) + _imgStartY;
            w = (int)(int.Parse(end.Split(',')[0]) / _resizeFactor);
            h = (int)(int.Parse(end.Split(',')[1]) / _resizeFactor);

            _redBox.X = x;
            _redBox.Y = y;
            _redBox.Width = w;
            _redBox.Height = h;

            pictureBoxScreenShot.Refresh();
        }



        private int GetSize(string bounds)
        {
            string start = bounds.Split(']')[0].Replace("[", "");
            string end = bounds.Split('[')[2].Replace("]", "");

            int bx1, by1, bx2, by2;

            bx1 = int.Parse(start.Split(',')[0]);
            by1 = int.Parse(start.Split(',')[1]);
            bx2 = int.Parse(end.Split(',')[0]);
            by2 = int.Parse(end.Split(',')[1]);

            int width = bx2 - bx1;
            int height = by2 - by1;
            return width * height;
        }


        private bool IsInBounds(int x, int y, string bounds)
        {
            string start = bounds.Split(']')[0].Replace("[", "");
            string end = bounds.Split('[')[2].Replace("]", "");

            int bx1, by1, bx2, by2;

            bx1 = int.Parse(start.Split(',')[0]);
            by1 = int.Parse(start.Split(',')[1]);
            bx2 = int.Parse(end.Split(',')[0]);
            by2 = int.Parse(end.Split(',')[1]);

            return (bx1 <= x) && (by1 <= y) && (bx2 >= x) && (by2 >= y);

        }
        #endregion

        #region Anroid Dump Related
        private void XMLtoTree()
        {
            if (string.IsNullOrEmpty(_dumpXML))
            {
                return;
            }

            _xmlDocument = new XmlDocument();
            _xmlDocument.LoadXml(_dumpXML);

            treeViewXML.Nodes.Clear();
            treeViewXML.Nodes.Add(new TreeNode(_xmlDocument.DocumentElement.Name));
            TreeNode tNode = new TreeNode();
            tNode = treeViewXML.Nodes[0];
            tNode.Tag = _xmlDocument.DocumentElement;
            AddNode(_xmlDocument.DocumentElement, tNode);
            AddNodeInfo(_xmlDocument.DocumentElement, tNode);

            treeViewXML.ExpandAll();
            treeViewXML.SelectedNode = treeViewXML.Nodes[0];

        }

        private void AddNode(XmlNode inXmlNode, TreeNode inTreeNode)
        {
            XmlNode xNode;
            TreeNode tNode;
            XmlNodeList nodeList;
            int i;

            if (inXmlNode.HasChildNodes)
            {
                nodeList = inXmlNode.ChildNodes;
                for (i = 0; i <= nodeList.Count - 1; i++)
                {
                    string val;
                    xNode = inXmlNode.ChildNodes[i];


                    val = $"({xNode.Attributes["index"].Value}){xNode.Attributes["class"].Value}";
                    TreeNode tr = new TreeNode(val);
                    inTreeNode.Nodes.Add(tr);
                    AddNodeInfo(xNode, tr);
                    tNode = inTreeNode.Nodes[i];
                    tNode.Tag = xNode;
                    AddNode(xNode, tNode);
                }
            }
            else
            {
                string val = $"({inXmlNode.Attributes["index"].Value}){inXmlNode.Attributes["class"].Value}";
                inTreeNode.Text = val;
            }
        }

        private void AddNodeInfo(XmlNode inXmlNode, TreeNode inTreeNode)
        {
            Dictionary<string, string> nodeAttr = new Dictionary<string, string>();
            foreach (XmlAttribute xa in inXmlNode.Attributes)
            {
                nodeAttr.Add(xa.Name, xa.Value);
            }

            _nodeInfo.Add(inTreeNode, nodeAttr);
        }

        private void TreeViewXML_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Dictionary<string, string> attr = _nodeInfo[e.Node];

            dataGridViewDescription.Rows.Clear();


            foreach (KeyValuePair<string, string> kv in attr)
            {
                if (kv.Value.Contains("\n"))
                {
                    string value = kv.Value.Replace("\n", "\\n");
                    dataGridViewDescription.Rows.Add(kv.Key, value);
                }
                else
                {
                    dataGridViewDescription.Rows.Add(kv.Key, kv.Value);
                }
            }

            if (attr.ContainsKey("bounds"))
            {
                DrawRedBox(attr["bounds"]);
            }

        }

        private void ResizeDataGridViewColumns()
        {
            int totalW = dataGridViewDescription.Width;
            colKey.Width = (int)(totalW * 0.37);
            colVal.Width = (int)(totalW * 0.58);
        }

        private void DataGridViewDescription_Resize(object sender, EventArgs e)
        {
            ResizeDataGridViewColumns();
        }

        private void treeViewXML_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                XmlNode targetXmlNode = treeViewXML.SelectedNode.Tag as XmlNode;
                if (treeViewXML.SelectedNode == null || targetXmlNode == null)
                {
                    copyXPathToolStripMenuItem.Enabled = false;
                }
                else
                {
                    copyXPathToolStripMenuItem.Enabled = true;
                }

                contextMenuStripPopupOnTree.Show(treeViewXML, e.Location);
            }
        }

        private void copyXPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            XmlNode targetXmlNode = treeViewXML.SelectedNode.Tag as XmlNode;
            if (targetXmlNode == null)
            {
                MessageBox.Show("Select valid item in the tree.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string xPath = _xPathBuilder.GetXPath(targetXmlNode, _xmlDocument);
            Clipboard.SetText(xPath);
            AppendToOutput($"XPath value {xPath} is copied to clipboard.");
        }

        #endregion


        #region Omni Dump Related
        private void ListBoxOmniIds_SelectedIndexChanged(object sender, EventArgs e)
        {
            NativeElement oe = _nativeElements[listBoxOmniIds.SelectedIndex];
            DrawRedBox(oe.GetBoundString());
        }

        private void ListBoxOmniIds_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button.Equals(MouseButtons.Right) && listBoxOmniIds.SelectedItems.Count > 0)
            {
                Clipboard.SetText(listBoxOmniIds.SelectedItem.ToString());
                AppendToOutput($"'{listBoxOmniIds.SelectedItem.ToString()}' is copied to clipboard");
            }
        }
        #endregion

        #region Output Textbox Related
        private delegate void AppendToOutputCallback(object contents);

        private void AppendToOutput(object contents)
        {
            if (this.richTextBoxOutput.InvokeRequired)
            {
                AppendToOutputCallback atoc = new AppendToOutputCallback(AppendToOutput);
                this.Invoke(atoc, new object[] { contents });
                richTextBoxOutput.ScrollToCaret();
            }
            else
            {
                richTextBoxOutput.AppendText($"{contents.ToString()}\r\n");
                richTextBoxOutput.ScrollToCaret();
            }

        }



        private void LogDebug(object sender, EventArgs e)
        {
            if (!checkBoxDebug.Checked) return;
            if (_duneUsed)
            {
                AppendToOutput($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} [D] :: Dune :: {((LogEventArgs)e).Message.ToString()}");
            }
            else
            {
                AppendToOutput($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} [D] :: Omni :: {((LogEventArgs)e).Message.ToString()}");
            }

        }


        private void LogTrace(object sender, EventArgs e)
        {
            if (!checkBoxTrace.Checked) return;
            if (_duneUsed)
            {
                AppendToOutput($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} [T] :: Dune :: {((LogEventArgs)e).Message.ToString()}");
            }
            else
            {
                AppendToOutput($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} [T] :: Omni :: {((LogEventArgs)e).Message.ToString()}");
            }

        }

        private void LogWarn(object sender, EventArgs e)
        {
            if (!checkBoxWarn.Checked) return;
            if (_duneUsed)
            {
                AppendToOutput($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} [W] :: Dune :: {((LogEventArgs)e).Message.ToString()}");
            }
            else
            {
                AppendToOutput($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} [W] :: Omni :: {((LogEventArgs)e).Message.ToString()}");
            }

        }


        private void LogError(object sender, EventArgs e)
        {
            if (!checkBoxError.Checked) return;
            if (_duneUsed)
            {
                AppendToOutput($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} [E] :: Dune :: {((LogEventArgs)e).Message.ToString()}");
            }
            else
            {
                AppendToOutput($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} [E] :: Omni :: {((LogEventArgs)e).Message.ToString()}");
            }
            AppendToOutput(((LogEventArgs)e).Exception.ToString());

        }
        #endregion
        public void SetDeviceInfo(string deviceId, string adminPassword)
        {
            textBoxIpAddress.Text = deviceId;
            textBoxPassword.Text = adminPassword;
            if (!string.IsNullOrEmpty(deviceId))
            {
                try
                {
                    ButtonConnect_Click(null, null);
                }
                catch (Exception ex)
                {
                    AppendToOutput(ex.ToString());
                }
            }
        }

        private void UpdateUI()
        {
            using (var ms = new MemoryStream(_screenShot))
            {
                _screenShotImg = Image.FromStream(ms);
            }
            pictureBoxScreenShot.Image = _screenShotImg;

            CalculateResizeFactors();

            // Android
            _nodeInfo.Clear();
            try
            {
                XMLtoTree();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("UI Dump fail. Please try again");
            }

            // Native UI
            listBoxOmniIds.Items.Clear();
            _nativeElements.Clear();
            if (_omniUsed)
            {
                foreach (string s in _omniIds)
                {
                    if (_omni.Exists($"#{s}"))
                    {
                        NativeElement e = new NativeElement(s, _omni.GetLocation($"#{s}"));
                        _nativeElements.Add(listBoxOmniIds.Items.Add(s), e);
                    }

                }
            }
            else if (_duneUsed)
            {
                duneSearch.Visible = true;
                duneSearchTextBox.Visible = true;

                _duneNodes.Clear();
                foreach (string id in _duneIds)
                {
                    string duneid = GetFormattedDuneId(id);
                }

                duneTreeView.Nodes.Clear();
                DuneTree();
                SaveDuneTreeViewToXml();
            }
            else if (_sirius2Used || _sirius3Used)
            {
                foreach (Widget w in _widgets)
                {
                    if (w.Id != null)
                    {
                        NativeElement e = new NativeElement(w.Id, w.Location.X, w.Location.Y, w.Size.Width, w.Size.Height);
                        _nativeElements.Add(listBoxOmniIds.Items.Add(w.Id), e);
                    }
                }
            }
            pictureBoxScreenShot.Refresh();
        }

        /// <summary>
        /// The method DuneTree creates tree hierarchy 
        /// Root node is hierarchy
        /// Based on the spaces of the _duneNodes , appends the node by getting the parent node.
        /// </summary>
        private void DuneTree()
        {
            if (_duneNodes == null)
            {
                return;
            }

            string rootId = _duneNodes[0];

            duneTreeView.Nodes.Clear();
            duneTreeView.Nodes.Add(new TreeNode(rootId));

            TreeNode tNode = new TreeNode();
            tNode = duneTreeView.Nodes[0];

            int prevCount = 0;
            TreeNode currentNode = null;
            for (int i = 1; i < _duneNodes.Count; i++)
            {
                int splittedIdCount = _duneNodes[i].Split(' ').Length;
                if (splittedIdCount > prevCount)
                {
                    prevCount = splittedIdCount;
                    currentNode = AddDuneNode(_duneNodes[i], tNode);
                    tNode = currentNode;
                }
                else if (splittedIdCount == prevCount)
                {
                    prevCount = splittedIdCount;
                    currentNode = AddDuneNode(_duneNodes[i], tNode.Parent);
                    tNode = currentNode;
                }
                else if (splittedIdCount < prevCount)
                {
                    tNode = tNode.Parent;
                    for (int count = prevCount; count > splittedIdCount; count = count - _defaultSpaceForDuneHierarchy)
                    {
                        tNode = tNode.Parent;
                    }

                    currentNode = AddDuneNode(_duneNodes[i], tNode);
                    tNode = currentNode;
                    prevCount = splittedIdCount;
                }
            }

            duneTreeView.ExpandAll();

        }

        private TreeNode AddDuneNode(string node, TreeNode inTreeNode)
        {
            TreeNode tr;
            int i;
            string val;

            val = node.Trim();
            tr = new TreeNode(val);
            inTreeNode.Nodes.Add(tr);

            return tr;
        }

        private void duneSearchTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(duneSearchTextBox.Text))
                {

                    duneTreeView.Nodes.Clear();

                    string rootId = _duneNodes[0];
                    duneTreeView.Nodes.Add(new TreeNode(rootId));

                    TreeNode tNode = new TreeNode();
                    tNode = duneTreeView.Nodes[0];

                    foreach (var duneId in _duneIds)
                    {
                        string formattedduneid = GetFormattedDuneId(duneId);
                        if (formattedduneid.ToLower().Contains(duneSearchTextBox.Text.ToLower()))
                        {
                            AddDuneNode(formattedduneid + " (" + duneId + ")", tNode);
                        }
                    }
                    duneTreeView.ExpandAll();
                }
                else
                {
                    AppendToOutput("Please Wait... Loading the tree elements");
                    duneTreeView.Nodes.Clear();
                    DeserializeDuneTreeView(duneXmlFileName);
                    duneTreeView.ExpandAll();

                    if (!String.IsNullOrEmpty(_duneSearchedNode))
                    {
                        TreeNode selectedNode = GetNodeFromPath(duneTreeView.Nodes[0], _duneSearchedNode);
                        duneTreeView.SelectedNode = selectedNode;
                        duneTreeView.SelectedNode.BackColor = Color.CornflowerBlue;
                        _duneSearchedNode = "";
                    }
                }
            }
            catch (Exception ex)
            {
                AppendToOutput(ex.Message);
            }
        }

        public TreeNode GetNodeFromPath(TreeNode node, string path)
        {
            TreeNode foundNode = null;
            foreach (TreeNode tn in node.Nodes)
            {
                string nodePath = tn.FullPath.Replace('\\', '/');
                if(!nodePath.StartsWith("/"))
                {
                    nodePath = "/" + nodePath;
                }

                if (nodePath == path)
                {
                    return tn;
                }
                else if (tn.Nodes.Count > 0)
                {
                    foundNode = GetNodeFromPath(tn, path);
                }
                if (foundNode != null)
                    return foundNode;
            }
            return null;
        }

        private void duneTreeView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode selectedNode = GetNodeFromPath(duneTreeView.Nodes[0], _duneSearchedNode);

            if (duneTreeView.SelectedNode != null)
            {
                if (duneTreeView.SelectedNode == selectedNode)
                {
                    duneTreeView.SelectedNode.BackColor = Color.White;
                }
            }
        }

        private void duneTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                duneTreeView.Enabled = false;
                Cursor.Hide();
                _redBox.X = 0; _redBox.Y = 0; _redBox.Width = 0; _redBox.Height = 0;
                if (_duneUsed)
                {
                    if (String.IsNullOrEmpty(duneSearchTextBox.Text))
                    {
                        string index = duneTreeView.SelectedNode.FullPath;
                        string node = index.Replace('\\', '/');

                        loadingPictureBox.Visible = true;
                        AppendToOutput("Please Wait... Loading the properties of element Id : " + node);
                        List<string> positions = _workflowUI.GetPosition(node).ToList();
                        DrawDuneRedBox("[" + positions[0].Split('=')[1] + "," + positions[1].Split('=')[1] + "][" + positions[2].Split('=')[1] + "," + positions[3].Split('=')[1] + "]");

                        AppendToOutput(node);
                        richTextBoxOutput.SelectionStart = richTextBoxOutput.GetFirstCharIndexOfCurrentLine();
                        richTextBoxOutput.SelectionColor = Color.Red;
                        AppendToOutput("The outline and the properties of the element are loaded from your device.");
                        pictureBoxScreenShot.Refresh();
                        loadingPictureBox.Visible = false;
                    }
                    else
                    {
                        string index = duneTreeView.SelectedNode.FullPath;
                        string node = index.Split('(')[1].Split(')')[0];
                        _duneSearchedNode = index.Split('(')[1].Split(')')[0];

                        loadingPictureBox.Visible = true;
                        AppendToOutput("Please Wait... Loading the properties of element Id : " + node);
                        List<string> positions = _workflowUI.GetPosition(node).ToList();
                        DrawDuneRedBox("[" + positions[0].Split('=')[1] + "," + positions[1].Split('=')[1] + "][" + positions[2].Split('=')[1] + "," + positions[3].Split('=')[1] + "]");

                        AppendToOutput(node);
                        richTextBoxOutput.SelectionStart = richTextBoxOutput.GetFirstCharIndexOfCurrentLine();
                        richTextBoxOutput.SelectionColor = Color.Red;
                        AppendToOutput("The outline and the properties of the element are loaded from your device.");
                        pictureBoxScreenShot.Refresh();
                        loadingPictureBox.Visible = false;
                    }
                }

                duneTreeView.Enabled = true;
                Cursor.Show();
            }
            catch (Exception ex)
            {
                AppendToOutput($"Error: {ex.Message}");
            }
            finally
            {
                loadingPictureBox.Visible = false;
                duneTreeView.Enabled = true;
                Cursor.Show();
            }
        }

        /// <summary>
        /// The method GetGormattedDuneId formats the data received from DAT 
        /// Input data : /statusCenterServiceStackView/nativeStackView/SpiceView
        /// formatted Output data : "            SpiceView" -> will get the output with 12space+last element(SpiceView) in the input data 
        /// </summary>
        /// <param name="duneId"></param>
        /// <returns></returns>
        private string GetFormattedDuneId(string duneId)
        {
            string id = "";

            if (duneId.Contains("/"))
            {
                if (!duneId.StartsWith("/"))
                {
                    duneId = "/" + duneId;
                }

                string[] splittedId = duneId.Split('/');
                int space = _defaultSpaceForDuneHierarchy * (splittedId.Length - 2);
                for (int i = 0; i < space; i++)
                {
                    id += " ";
                }
                id += splittedId[splittedId.Length - 1];
            }
            else
            {
                id = duneId;
            }

            _duneNodes.Add(id);
            return id;
        }

        private void SaveDuneTreeViewToXml()
        {
            XmlTextWriter textWriter = new XmlTextWriter(duneXmlFileName, System.Text.Encoding.ASCII);
            // writing the xml declaration tag
            textWriter.WriteStartDocument();
            // writing the main tag that encloses all node tags
            textWriter.WriteStartElement("TreeView");

            // save the nodes, recursive method
            SaveNodes(duneTreeView.Nodes, textWriter);

            textWriter.WriteEndElement();

            textWriter.Close();
        }

        private void SaveNodes(TreeNodeCollection nodesCollection, XmlTextWriter textWriter)
        {
            for (int i = 0; i < nodesCollection.Count; i++)
            {
                TreeNode node = nodesCollection[i];
                textWriter.WriteStartElement(XmlNodeTag);
                textWriter.WriteAttributeString(XmlNodeTextAtt, node.Text);
                textWriter.WriteAttributeString(XmlNodeImageIndexAtt, node.ImageIndex.ToString());
                if (node.Tag != null)
                    textWriter.WriteAttributeString(XmlNodeTagAtt, node.Tag.ToString());
                // add other node properties to serialize here  
                if (node.Nodes.Count > 0)
                {
                    SaveNodes(node.Nodes, textWriter);
                }
                textWriter.WriteEndElement();
            }
        }

        public void DeserializeDuneTreeView(string fileName)
        {
            XmlTextReader reader = null;
            try
            {
                // disabling re-drawing of treeview till all nodes are added
                duneTreeView.BeginUpdate();
                reader = new XmlTextReader(fileName);
                TreeNode parentNode = null;
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == XmlNodeTag)
                        {
                            TreeNode newNode = new TreeNode();
                            bool isEmptyElement = reader.IsEmptyElement;

                            // loading node attributes
                            int attributeCount = reader.AttributeCount;
                            if (attributeCount > 0)
                            {
                                for (int i = 0; i < attributeCount; i++)
                                {
                                    reader.MoveToAttribute(i);
                                    SetAttributeValue(newNode,
                                                 reader.Name, reader.Value);
                                }
                            }
                            // add new node to Parent Node or TreeView
                            if (parentNode != null)
                                parentNode.Nodes.Add(newNode);
                            else
                                duneTreeView.Nodes.Add(newNode);

                            // making current node 'ParentNode' if its not empty
                            if (!isEmptyElement)
                            {
                                parentNode = newNode;
                            }
                        }
                    }
                    // moving up to in TreeView if end tag is encountered
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        if (reader.Name == XmlNodeTag)
                        {
                            parentNode = parentNode.Parent;
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.XmlDeclaration)
                    {
                        //Ignore Xml Declaration                    
                    }
                    else if (reader.NodeType == XmlNodeType.None)
                    {
                        return;
                    }
                    else if (reader.NodeType == XmlNodeType.Text)
                    {
                        parentNode.Nodes.Add(reader.Value);
                    }

                }
            }
            finally
            {
                // enabling redrawing of treeview after all nodes are added
                duneTreeView.EndUpdate();
                reader.Close();
            }
        }
        private void SetAttributeValue(TreeNode node, string propertyName, string value)
        {
            if (propertyName == XmlNodeTextAtt)
            {
                node.Text = value;
            }
            else if (propertyName == XmlNodeImageIndexAtt)
            {
                node.ImageIndex = int.Parse(value);
            }
            else if (propertyName == XmlNodeTagAtt)
            {
                node.Tag = value;
            }
        }
    }
}

