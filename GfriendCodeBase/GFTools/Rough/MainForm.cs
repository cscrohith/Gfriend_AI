using HP.DeviceAutomation;
using HP.DeviceAutomation.Jedi;
using HP.DeviceAutomation.Jedi.OmniUserInteraction;
using HP.DeviceAutomation.Jedi.Oxpd.Test;
using HP.GFriend.Utils.Appium;
using HP.GFriend.Utils.XPath;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Extension;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Mac;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace HP.GFriend.Tool
{
    public partial class MainForm : Form
    {
        private IOSDriver _iosDriver;
        private MacDriver _macDriver;
        private AndroidDriver _androidDriver;

        private OmniTesterClient _omni;
        private OxpdTestClient _oxpd;
        private IDevice _device;
        private bool _omniUsed;
        private AppiumElement _macBaseElement;
        private bool _isConnected;
        private bool _iosUsed;
        private bool _androidUsed;
        private bool _macUsed;

        private bool _isInteraction = false;
        private int _pixelRatio = 1;

        private string _dumpXML = null;

        private string _userId;
        private string _userPw;

        private XmlDocument _xmlDocument;
        private XPathBuilder _xPathBuilder;
        private string _xPathFirstPriority = "identifier";

        private byte[] _dumpScreenShot = null;
        private byte[] _liveScreenShot = null;
        private Dictionary<TreeNode, Dictionary<string, string>> _nodeInfo;
        private Rectangle _redBox;
        private int _mouseX, _mouseY;
        // private MacElement _macBaseElement;
        private int _imgStartX, _imgStartY;
        private System.Drawing.Size _windowSize;
        private double _resizeFactor;

        private Point? _swipeStart;
        private Point? _swipeEnd;
        private DateTime _mouseDownTime;

        private List<char> _keyBuffer = new List<char>();
        private bool _isTyping;
        private System.Windows.Forms.Timer _typeTimer;

        private Image _screenShotImg;

        public MainForm()
        {
            InitializeComponent();
            _isConnected = false;
            _nodeInfo = new Dictionary<TreeNode, Dictionary<string, string>>();
            comboBoxXpathPriority.SelectedIndex = 0;
            _xPathBuilder = new XPathBuilder();
            _xPathBuilder.AddBlackListAttribute(new List<string>() { "x", "y", "width", "height" });
            _xPathBuilder.AddFirstPriorityAttribute(new List<string>() { _xPathFirstPriority });
            ResizeDataGridViewColumns();
            _redBox = new Rectangle(0, 0, 0, 0);
            CalculateResizeFactors(pictureBoxScreenShot);
            timerScreenRefresh.Interval = 1000;
            // toolStripComboBoxTarget.Text = "Mac";
        }

        #region Control Events : Button Click
        private void toolStripButtonConnect_Click(object sender, EventArgs e)
        {
            timerScreenRefresh.Stop();
            if (_isConnected)
            {
                timerScreenRefresh.Stop();
                if (_iosUsed && _iosDriver != null)
                {
                    _iosDriver.Quit();
                    _iosDriver.Dispose();
                    _iosDriver = null;
                    AppiumServerUtils.StopWDA(textBoxUdid.Text);
                }
                else if (_macDriver != null)
                {
                    _macDriver.Quit();
                    _macDriver.Dispose();
                    _macDriver = null;
                }
                else if (_androidUsed && _androidDriver != null)
                {
                    _androidDriver.Quit();
                    _androidDriver.Dispose();
                    _androidDriver = null;
                }

                toolStripButtonConnect.Text = "Connect";
                _isConnected = false;
                toolStripButtonDump.Enabled = false;
                AppendToOutput("Disconnected");
                return;

            }
            else
            {
                _userId = toolStripTextBoxID.Text;
                _userPw = toolStripTextBoxPassword.Text;
                if (textBoxUdid.Text.Length <= 12 || Regex.IsMatch(textBoxUdid.Text, @"^\d{1,3}(\.\d{1,3}){3}:\d+$"))
                {
                    toolStripComboBoxTarget.Text = "Android";
                    if (string.IsNullOrEmpty(textBoxAppiumServer.Text) || string.IsNullOrEmpty(toolStripTextBoxPort.Text) ||
                        string.IsNullOrEmpty(_userId) || string.IsNullOrEmpty(_userPw) ||
                        string.IsNullOrEmpty(textBoxUdid.Text) || string.IsNullOrEmpty(textBundleId.Text))
                    {
                        AppendToOutput("Appium server address, port, user id, password, UDID and Bundle ID should be given.");
                        return;
                    }
                    AppiumServerUtils.Initialize(textBoxAppiumServer.Text, 22, _userId, _userPw);
                    if (!AppiumServerUtils.ExecuteAppiumServer())
                    {
                        AppendToOutput("Appium server is not running.");
                        return;
                    }
                    AppiumOptions caps = new AppiumOptions();
                    caps.PlatformName = "Android";
                    caps.DeviceName = AppiumServerUtils.GetDeviceName(textBoxUdid.Text);
                    caps.AutomationName = "UiAutomator2";
                    caps.AddAdditionalAppiumOption("appium:newCommandTimeout", 60000);
                    caps.AddAdditionalAppiumOption("appium:ignoreHiddenApiPolicyError", true);

                    // in seconds

                    _androidDriver = new AndroidDriver(new Uri($"http://{textBoxAppiumServer.Text}:{toolStripTextBoxPort.Text}/wd/hub"), caps);
                    // _androidDriver.ActivateApp(PackageName);
                    _pixelRatio = 1;  // from helper I gave earlier

                    foreach (KeyValuePair<string, object> attr in _androidDriver.SessionDetails)
                    {
                        AppendToOutput($"{attr.Key} : {attr.Value}");
                    }
                    _windowSize = new System.Drawing.Size(0, 0);

                    // Prevent WDA hang out
                    if(!textBoxUdid.Text.Contains(":5555"))
                    {
                        _androidDriver.ActivateApp(textBundleId.Text);

                    }
                    //_androidDriver.GetScreenshot();
                    _androidUsed = true;
                    _macUsed = false;
                    _iosUsed = false;
                }
                else if (textBoxUdid.Text.Length >= 13 && textBoxUdid.Text.Length <= 26 && !textBoxUdid.Text.Contains(":5555"))
                {
                    _iosUsed = true;
                    toolStripComboBoxTarget.Text = "iOS";
                    if (string.IsNullOrEmpty(textBoxAppiumServer.Text) || string.IsNullOrEmpty(toolStripTextBoxPort.Text) ||
                        string.IsNullOrEmpty(_userId) || string.IsNullOrEmpty(_userPw) ||
                        string.IsNullOrEmpty(textBoxUdid.Text) || string.IsNullOrEmpty(textBundleId.Text))
                    {
                        AppendToOutput("Appium server address, port, user id, password, UDID and Bundle ID should be given.");
                        return;
                    }

                    AppiumServerUtils.Initialize(textBoxAppiumServer.Text, 22, _userId, _userPw);
                    if (!AppiumServerUtils.ExecuteAppiumServer())
                    {
                        AppendToOutput("Appium server is not running.");
                        return;
                    }

                    AppiumOptions caps = new AppiumOptions();
                    string xcodeOrgId = "TKM6D5Y743";
                    if (AppiumServerUtils.IsWdaEntSetup())
                    {
                        string wdaPrebuiltPath = AppiumServerUtils.GetPrebuiltWDAPath();
                        if (string.IsNullOrEmpty(wdaPrebuiltPath))
                        {
                            AppendToOutput("Error : Can not find prebuild WDA in the server");
                        }
                        xcodeOrgId = "TKM6D5Y743";
                        caps.AddAdditionalAppiumOption("appium:derivedDataPath", wdaPrebuiltPath);
                        caps.AddAdditionalAppiumOption("appium:usePrebuiltWDA", true);
                        caps.AddAdditionalAppiumOption("appium:updatedWDABundleId", "com.hp.GFriend.WDA");
                        caps.AddAdditionalAppiumOption("appium:newCommandTimeout", 6000); // in seconds

                        //caps.AddAdditionalAppiumOption("useXctestrunFile", false);
                    }
                    else
                    {
                        xcodeOrgId = AppiumServerUtils.ExecuteWDA(textBoxUdid.Text);
                    }

                    caps.PlatformName = "iOS";
                    caps.AutomationName = "XCUITest";
                    caps.DeviceName = AppiumServerUtils.GetDeviceName(textBoxUdid.Text);
                    caps.AddAdditionalAppiumOption("appium:noReset", true);
                    caps.AddAdditionalAppiumOption("appium:xcodeOrgId", xcodeOrgId);
                    caps.AddAdditionalAppiumOption("appium:xcodeSigningId", "iPhone Developer");
                    caps.AddAdditionalAppiumOption("appium:bundleId", textBundleId.Text);
                    caps.AddAdditionalAppiumOption("appium:udid", textBoxUdid.Text);
                    caps.AddAdditionalAppiumOption("appium:wdaStartupRetryInterval", 10000);
                    caps.AddAdditionalAppiumOption("appium:wdaLaunchTimeout", 60000);
                    caps.AddAdditionalAppiumOption("appium:wdaConnectionTimeout", 240000);
                    caps.AddAdditionalAppiumOption("appium:usePrebuiltWDA", true);
                    caps.AddAdditionalAppiumOption("newCommandTimeout", 6000); // in seconds

                    //caps.AddAdditionalAppiumOption("appium:autoAcceptAlerts", true); 

                    _iosDriver = new IOSDriver(new Uri($"http://{textBoxAppiumServer.Text}:{toolStripTextBoxPort.Text}/wd/hub"), caps);
                    AppendToOutput($"PlatformName : {caps.PlatformName}");
                    AppendToOutput($"DeviceName : {caps.DeviceName}");
                    AppendToOutput($"Udid:{textBoxUdid.Text}");

                    foreach (KeyValuePair<string, object> attr in _iosDriver.SessionDetails)
                    {
                        double plat_v;
                        if (attr.Key is "platformVersion")
                        {
                            if (double.TryParse(attr.Value.ToString(), out plat_v))
                            {
                                if (plat_v <= 17)
                                {
                                    _pixelRatio = 2;

                                }
                                else
                                {
                                    _pixelRatio = 3;
                                }
                            }
                            else
                            {
                                Debug.WriteLine("The platform version value is not a valid integer.");
                            }
                        }

                    }
                    _windowSize = new System.Drawing.Size(0, 0);

                    // Prevent WDA hang out
                    _iosDriver.ActivateApp(textBundleId.Text);
                    _iosDriver.GetScreenshot();
                    _androidUsed = false;
                    _macUsed = false;
                    _iosUsed = true;
                }

                else if (textBoxUdid.Text.Length > 30)
                {
                    _macUsed = true;
                    toolStripComboBoxTarget.Text = "Mac";
                    if (string.IsNullOrEmpty(textBoxAppiumServer.Text) || string.IsNullOrEmpty(toolStripTextBoxPort.Text) ||
                        string.IsNullOrEmpty(_userId) || string.IsNullOrEmpty(_userPw) ||
                        string.IsNullOrEmpty(textBundleId.Text))
                    {
                        AppendToOutput("Appium server address, port, user id, password and Bundle ID should be given.");
                        return;
                    }
                    AppiumServerUtils.Initialize(textBoxAppiumServer.Text, 22, _userId, _userPw);
                    if (!AppiumServerUtils.ExecuteAppiumServer())
                    {
                        AppendToOutput("Appium server is not running.");
                        return;
                    }
                    AppiumOptions caps = new AppiumOptions();
                    caps.PlatformName = "mac";
                    caps.AutomationName = "Mac2";
                    caps.AddAdditionalAppiumOption("appium:bundleId", textBundleId.Text);
                    caps.AddAdditionalAppiumOption("appium:noReset", true);
                    _macDriver = new MacDriver(new Uri($"http://{textBoxAppiumServer.Text}:{toolStripTextBoxPort.Text}/wd/hub"), caps);
                    _pixelRatio = _macDriver.GetPixelRatio();
                    if (_pixelRatio == 0) _pixelRatio = 1;
                    try
                    {
                        foreach (KeyValuePair<string, object> attr in _macDriver.SessionDetails)
                        {
                            AppendToOutput($"{attr.Key} : {attr.Value}");
                        }
                    }
                    catch (Exception ex)
                    {
                        AppendToOutput($"Error retrieving session details: {ex.Message}");
                    }
                    _macUsed = true;
                    _androidUsed = false;
                    _iosUsed = false;
                    _windowSize = _macDriver.Manage().Window.Size;
                }
                _isConnected = true;
            }
            if (_isConnected)
            {
                toolStripButtonDump.Enabled = true;
                toolStripButtonConnect.Text = "Disconnect";
            }
            else
            {
                AppendToOutput("Connection error");
            }
        }
        private void GetMacBaseElement()
        {
            _macBaseElement = _macDriver.FindElement("xpath", "//*[not(@width=0) and not(@height=0)]");
            AppendToOutput($"Set Mac Base element : {_macBaseElement.Location.X}, {_macBaseElement.Location.Y}");
        }


        private void ToolStripButtonDump_Click(object sender, EventArgs e)
        {
            var textBoxIpAddress = textBoxUdid.Text.Split(':')[0];

            var textBoxPassword = txtprinterpwd.Text;
            if (_macUsed)
            {
                _dumpScreenShot = _macDriver.GetScreenshot().AsByteArray;
                _dumpXML = _macDriver.PageSource;
            }
            else if (_androidUsed)
            {
                try
                {
                    if (textBoxUdid.Text.Contains(":5555"))
                    {

                        _device = DeviceFactory.Create(textBoxIpAddress, textBoxPassword);
                        if (_device is JediOmniDevice)
                        {
                            _oxpd = new OxpdTestClient(textBoxIpAddress);
                            _oxpd.AdminPassword = textBoxPassword;
                            NetworkConfiguration.ConfigureCertificateAcceptance();
                            _omni = new OmniTesterClient(textBoxIpAddress, TimeSpan.FromSeconds(10));
                            _omniUsed = true;
                        }
                    }
                    else
                    {
                        _dumpScreenShot = _androidDriver.GetScreenshot().AsByteArray;

                    }
                }
                catch (Exception ex)
                {
                    AppendToOutput($"connection fail : {ex.ToString()}");
                }

                if (_omniUsed)
                {
                    _dumpScreenShot = _oxpd.GetScreenCapture();
                }
                _dumpXML = _androidDriver.PageSource;
            }
            else if (_iosUsed)
            {
                _dumpScreenShot = _iosDriver.GetScreenshot().AsByteArray;
                _dumpXML = _iosDriver.PageSource;
            }
            UpdateUI();

        }

        #endregion

        #region From Event
        private void TesterMainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_isConnected)
            {
                toolStripButtonConnect_Click(null, null);
            }

        }

        #endregion

        #region Picturebox Related Events

        private void GetLiveScreenCapture()
        {
            if (_iosUsed && _iosDriver != null)
            {
                try
                {
                    _liveScreenShot = _iosDriver.GetScreenshot().AsByteArray;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("hang up"))
                    {
                        AppiumServerUtils.ExecuteWDA(textBoxUdid.Text);
                    }
                    else
                    {
                        AppendToOutput($"Unexpected Error :\r\n {ex.ToString()}");
                    }
                }

            }

            else if (_macDriver != null)
            {
                _liveScreenShot = _macDriver.GetScreenshot().AsByteArray;
            }
            CalculateResizeFactors(pictureBoxLive);
        }

        private void CalculateResizeFactors(PictureBox sender)
        {
            Image i = sender.Image;

            if (i == null) return;
            int oriWidth = (int)(i.PhysicalDimension.Width / _pixelRatio);
            int oriHeight = (int)(i.PhysicalDimension.Height / _pixelRatio);
            int dipWidth, dipHeight;


            var wfactor = (double)oriWidth / sender.ClientSize.Width;
            var hfactor = (double)oriHeight / sender.ClientSize.Height;

            _resizeFactor = Math.Max(wfactor, hfactor);
            dipWidth = (int)(oriWidth / _resizeFactor);
            dipHeight = (int)(oriHeight / _resizeFactor);
            _imgStartX = (sender.Width - dipWidth) / 2;
            _imgStartY = (sender.Height - dipHeight) / 2;

        }

        private void PictureBoxScreenShot_MouseMove(object sender, MouseEventArgs e)
        {
            Image i = ((PictureBox)sender).Image;
            if (i == null) return;

            _mouseX = (int)((e.X - _imgStartX) * _resizeFactor);
            _mouseY = (int)((e.Y - _imgStartY) * _resizeFactor);

            labelMouse.Text = $"({_mouseX} , {_mouseY})";
        }

        private void PictureBoxScreenShot_Resize(object sender, EventArgs e)
        {
            CalculateResizeFactors(pictureBoxScreenShot);
        }

        private void PictureBoxLive_Resize(object sender, EventArgs e)
        {
            CalculateResizeFactors(pictureBoxLive);
        }

        private void PictureBoxScreenShot_Click(object sender, EventArgs e)
        {

            int minRect = int.MaxValue;
            if (tabControlUiSelection.SelectedTab.Equals(tabPageXmlDump))
            {
                foreach (KeyValuePair<TreeNode, Dictionary<string, string>> kv in _nodeInfo)
                {
                    if (kv.Value.ContainsKey("x") && kv.Value.ContainsKey("y")
                        && kv.Value.ContainsKey("width") && kv.Value.ContainsKey("height"))
                    {
                        if (IsInBounds(_mouseX, _mouseY, int.Parse(kv.Value["x"]), int.Parse(kv.Value["y"]),
                            int.Parse(kv.Value["width"]), int.Parse(kv.Value["height"])))
                        {
                            int size = int.Parse(kv.Value["width"]) * int.Parse(kv.Value["height"]);
                            if (minRect > size)
                            {
                                treeViewXML.SelectedNode = kv.Key;
                                minRect = size;
                            }

                        }
                    }
                    else if (kv.Value.ContainsKey("bounds"))
                    {
                        string bounds = kv.Value["bounds"];
                        try
                        {
                            var match = System.Text.RegularExpressions.Regex.Match(bounds, @"\[(\d+),(\d+)]\[(\d+),(\d+)]");
                            if (match.Success)
                            {
                                int x = Convert.ToInt32(match.Groups[1].Value);
                                int y = Convert.ToInt32(match.Groups[2].Value);
                                int width = Convert.ToInt32(match.Groups[3].Value) - x;
                                int height = Convert.ToInt32(match.Groups[4].Value) - y;

                                // Replace the selected code block with the following for Android screen coordinate handling
                                if (IsPointInRect(_mouseX, _mouseY, x, y, width, height, _pixelRatio))
                                {
                                    int rectArea = width * height;
                                    if (rectArea < minRect)
                                    {
                                        treeViewXML.SelectedNode = kv.Key;
                                        minRect = rectArea;
                                    }
                                    DrawRedBox(x, y, width, height);
                                }
                            }

                        }
                        catch (Exception ex)
                        {

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


        private void DrawRedBox(int x, int y, int width, int height)
        {
            int x2 = x + width;
            int y2 = y + height;

            x = (int)(x / _resizeFactor) + _imgStartX;
            y = (int)(y / _resizeFactor) + _imgStartY;

            _redBox.X = x;
            _redBox.Y = y;
            _redBox.Width = (int)(width / _resizeFactor);
            _redBox.Height = (int)(height / _resizeFactor);
            pictureBoxScreenShot.Refresh();

        }

        private bool IsInBounds(int x, int y, int targetX, int targetY, int targetWidth, int targetHeight)
        {
            int targetX2, targetY2;

            targetX2 = targetX + targetWidth;
            targetY2 = targetY + targetHeight;

            return (targetX <= x) && (targetY <= y) && (targetX2 >= x) && (targetY2 >= y);

        }

        private bool IsPointInRect(int mouseX, int mouseY, int rectX, int rectY, int rectWidth, int rectHeight, double pixelRatio)
        {
            // Adjust for pixel density
            mouseX = (int)(mouseX / pixelRatio);
            mouseY = (int)(mouseY / pixelRatio);

            return mouseX >= rectX && mouseX <= rectX + rectWidth &&
                   mouseY >= rectY && mouseY <= rectY + rectHeight;
        }


        #endregion

        #region XML Dump Related
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


                    val = $"{xNode.Attributes["type"]?.Value ?? xNode.Name}";
                    TreeNode tr = new TreeNode(val);
                    inTreeNode.Nodes.Add(tr);
                    tr.Tag = xNode;
                    AddNodeInfo(xNode, tr);
                    tNode = inTreeNode.Nodes[i];
                    AddNode(xNode, tNode);
                }
            }
            else
            {
                string val = $"{inXmlNode.Attributes["type"]?.Value ?? inXmlNode.Name}";
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
                dataGridViewDescription.Rows.Add(kv.Key, kv.Value);
            }

            if (attr.ContainsKey("x") && attr.ContainsKey("y") && attr.ContainsKey("width") && attr.ContainsKey("height"))
            {
                DrawRedBox(int.Parse(attr["x"]), int.Parse(attr["y"]), int.Parse(attr["width"]), int.Parse(attr["height"]));
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




        #region Output Textbox Related
        private delegate void AppendToOutputCallback(object contents);

        private void AppendToOutput(object contents)
        {
            if (this.textBoxOutput.InvokeRequired)
            {
                AppendToOutputCallback atoc = new AppendToOutputCallback(AppendToOutput);
                this.Invoke(atoc, new object[] { contents });
            }
            else
            {
                textBoxOutput.AppendText($"{contents.ToString()}\r\n");
            }

        }

        #endregion



        private void toolStripComboBoxTarget_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (toolStripComboBoxTarget.Text.Equals("iOS"))
            {
                labelUdid.Visible = true;
                textBoxUdid.Visible = true;
                labelprinterpwd.Visible = false;
                txtprinterpwd.Visible = false;
                tabControlUtils.SelectedTab = tabPageiOS;
                _iosUsed = true;
                _androidUsed = false;
                _macUsed = false;
            }
            else if (toolStripComboBoxTarget.Text.Equals("Mac"))
            {
                labelUdid.Visible = false;
                textBoxUdid.Visible = false;
                labelprinterpwd.Visible = false;
                txtprinterpwd.Visible = false;

                _macUsed = true;
                _iosUsed = false;
                _androidUsed = false;
                tabControlUtils.SelectedTab = tabPageMac;
            }
            else if (toolStripComboBoxTarget.Text.Equals("Android"))
            {
                labelUdid.Visible = true;
                textBoxUdid.Visible = true;
                if(textBoxUdid.Text.Contains(":5555"))
                {
                    labelprinterpwd.Visible = true;
                    txtprinterpwd.Visible = true;
                }
                else
                {
                    labelprinterpwd.Visible = false;
                    txtprinterpwd.Visible = false;
                }

                _androidUsed = true;
                _iosUsed = false;
                _macUsed = false;
                tabControlUtils.SelectedTab = tabPageAndroid;
            }
        }


        public void SetDeviceInfo(string deviceId, string adminPassword)
        {
            textBoxAppiumServer.Text = deviceId;
            textBundleId.Text = adminPassword;
            if (!string.IsNullOrEmpty(deviceId))
            {
                try
                {
                    toolStripButtonConnect_Click(null, null);
                }
                catch (Exception ex)
                {
                    AppendToOutput(ex.ToString());
                }
            }
        }

        private void tabControlUpside_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlUpside.SelectedTab.Equals(tabPageLiveControl))
            {
                CalculateResizeFactors(pictureBoxLive);
                timerScreenRefresh.Start();
            }
            else
            {
                timerScreenRefresh.Stop();
            }
        }



        private void UpdateUI()
        {
            using (var ms = new MemoryStream(_dumpScreenShot))
            {
                _screenShotImg = Image.FromStream(ms);
            }
            if (_windowSize.Width > 0 && _windowSize.Height > 0)
            {
                _screenShotImg = (Image)(new Bitmap(_screenShotImg, _windowSize));
            }
            pictureBoxScreenShot.Image = _screenShotImg;

            CalculateResizeFactors(pictureBoxScreenShot);

            _nodeInfo.Clear();
            try
            {
                XMLtoTree();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("UI Dump fail. Please try again");
            }


            pictureBoxScreenShot.Refresh();
        }


        #region Live control
        private void pictureBoxLive_MouseDown(object sender, MouseEventArgs e)
        {
            if (_iosUsed && _iosDriver != null)
            {
                if (!_isInteraction)
                {
                    _swipeStart = new Point(_mouseX, _mouseY);
                    _mouseDownTime = DateTime.Now;
                    _isInteraction = true;
                }
            }
        }

        private void pictureBoxLive_MouseUp(object sender, MouseEventArgs e)
        {
            if (_iosUsed && _iosDriver != null)
            {
                if (_isInteraction && _swipeStart != null)
                {
                    _swipeEnd = new Point(_mouseX, _mouseY);
                    string message;
                    int waitTime = 0;
                    if (_swipeStart.Equals(_swipeEnd))
                    {
                        message = $"Clicking Point : ({_swipeStart.Value.X},{_swipeStart.Value.Y})";
                        if (_mouseDownTime.AddMilliseconds(1000) > DateTime.Now)
                        {
                            message = $"Clicking Point : ({_swipeStart.Value.X},{_swipeStart.Value.Y})";
                            waitTime = 100;
                        }
                        else
                        {
                            message = $"Long Clicking Point : ({_swipeStart.Value.X},{_swipeStart.Value.Y})";
                            waitTime = 1000;
                        }


                    }
                    else
                    {
                        message = $"Swipe : ({_swipeStart.Value.X},{_swipeStart.Value.Y}) to ({_swipeEnd.Value.X},{_swipeEnd.Value.Y})";
                        waitTime = 500;
                    }

                    //TouchAction swipe = new TouchAction(_iosDriver);
                    // swipe.Press(_swipeStart.Value.X, _swipeStart.Value.Y).Wait(waitTime).MoveTo(_swipeEnd.Value.X, _swipeEnd.Value.Y).Release().Perform();
                    AppendToOutput(message);

                    _swipeStart = null;
                    _swipeEnd = null;
                    _isInteraction = false;
                }
            }
        }

        private void PictureBoxLive_Click(object sender, EventArgs e)
        {
            if (!_iosUsed && _macDriver != null)
            {
                if (!_isInteraction)
                {
                    _isInteraction = true;

                    PointerInputDevice mouse = new PointerInputDevice(PointerKind.Mouse);
                    ActionSequence seq = new ActionSequence(mouse, 0);
                    //GetMacBaseElement();
                    //int calX = _mouseX - _macBaseElement.Location.X - _macBaseElement.Size.Width / 2;
                    //int calY = _mouseY - _macBaseElement.Location.Y - _macBaseElement.Size.Height / 2;
                    //AppendToOutput($"Clicking : {_mouseX}, {_mouseY}");
                    // seq.AddAction(mouse.CreatePointerMove(_macBaseElement, calX, calY, TimeSpan.FromMilliseconds(10)));
                    seq.AddAction(mouse.CreatePointerDown(MouseButton.Left));
                    seq.AddAction(mouse.CreatePause(TimeSpan.FromMilliseconds(100)));
                    seq.AddAction(mouse.CreatePointerUp(MouseButton.Left));
                    _macDriver.PerformActions(new List<ActionSequence>() { seq });
                    AppendToOutput("Clicking Done");


                    _isInteraction = false;
                }
            }

        }

        private void timerScreenRefresh_Tick(object sender, EventArgs e)
        {
            GetLiveScreenCapture();
            if (_liveScreenShot != null)
            {
                using (var ms = new MemoryStream(_liveScreenShot))
                {
                    Image liveImage = Image.FromStream(ms);
                    if (_windowSize.Width > 0 && _windowSize.Height > 0)
                    {
                        liveImage = (Image)(new Bitmap(liveImage, _windowSize));
                    }
                    pictureBoxLive.Image = liveImage;
                }
                pictureBoxLive.Refresh();
            }
        }

        private void buttonSendKeyIos_Click(object sender, EventArgs e)
        {
            _iosDriver.SwitchTo().ActiveElement().SendKeys(textBoxStrToSendios.Text);
        }

        private void buttonSendKeyMac_Click(object sender, EventArgs e)
        {
            _macDriver.SwitchTo().ActiveElement().SendKeys(textBoxStrToSendMac.Text);
        }


        private void tabControlUpside_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!tabControlUpside.SelectedTab.Equals(tabPageLiveControl)) return;
            if (textBoxStrToSendMac.Focused || textBoxXpathMac.Focused || textBoxStrToSendios.Focused) return;
            _isTyping = true;
            if (_typeTimer == null)
            {
                _typeTimer = new System.Windows.Forms.Timer();
                _typeTimer.Interval = 1000;
                _typeTimer.Tick += _typeTimer_Tick;
                _typeTimer.Start();
            }
            _keyBuffer.Add(e.KeyChar);

        }

        private void _typeTimer_Tick(object sender, EventArgs e)
        {
            if (!_isTyping)
            {
                string toSend = new string(_keyBuffer.ToArray());
                AppendToOutput($"Sending : {toSend}");
                IWebElement activeElement = null;
                if (_macDriver != null)
                {
                    activeElement = _macDriver.SwitchTo().ActiveElement();


                }
                else if (_iosDriver != null)
                {
                    activeElement = _iosDriver.SwitchTo().ActiveElement();
                }
                if (activeElement != null)
                {
                    activeElement.SendKeys(toSend);
                    _keyBuffer.Clear();

                }
                _typeTimer.Stop();
                _typeTimer = null;
            }
            _isTyping = false;
        }

        private void buttonClickMac_Click(object sender, EventArgs e)
        {
            if (_macDriver != null)
            {
                //_macDriver.FindElement(OpenQA.Selenium.By.XPath(textBoxXpathMac.Text)).Click();
                _macDriver.FindElement(By.XPath(textBoxXpathMac.Text)).Click();
            }
        }

        #endregion

        #region Utils
        private void buttonListDevice_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxAppiumServer.Text) || string.IsNullOrEmpty(toolStripTextBoxID.Text)
                || string.IsNullOrEmpty(toolStripTextBoxPassword.Text))
            {
                MessageBox.Show("Please fill out Appium server address, ID and password.");
                return;
            }
            toolStripComboBoxTarget.Text = "iOS";
            listViewResult.Items.Clear();
            listViewResult.Columns.Clear();
            listViewResult.Columns.Add("Item");
            listViewResult.Columns.Add("Name");
            listViewResult.Columns.Add("UDID");
            listViewResult.Columns.Add("Type");
            listViewResult.Columns.Add("OS Version");
            AppiumServerUtils.Initialize(textBoxAppiumServer.Text, 22, toolStripTextBoxID.Text, toolStripTextBoxPassword.Text);
            foreach (IOSDevice device in AppiumServerUtils.GetConnectedDevices())
            {
                ListViewItem lvi = new ListViewItem(new string[] { "device", device.DeviceName, device.Udid, device.DeviceType, device.OsVersion });
                if (device.DeviceType != "Simulators" && device.Udid != null && device.Udid != string.Empty)
                {
                    listViewResult.Items.Add(lvi);
                }
            }
            listViewResult.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

        }

        private void buttonListMacApps_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxAppiumServer.Text) || string.IsNullOrEmpty(toolStripTextBoxID.Text)
                || string.IsNullOrEmpty(toolStripTextBoxPassword.Text))
            {
                MessageBox.Show("Please fill out Appium server address, ID and password.");
                return;
            }
            toolStripComboBoxTarget.Text = "Mac";
            listViewResult.Items.Clear();
            listViewResult.Columns.Clear();
            listViewResult.Columns.Add("Item");
            listViewResult.Columns.Add("Name");
            listViewResult.Columns.Add("Bundle ID");

            AppiumServerUtils.Initialize(textBoxAppiumServer.Text, 22, toolStripTextBoxID.Text, toolStripTextBoxPassword.Text);
            foreach (App app in AppiumServerUtils.GetInstalledApps(textBoxAppiumServer.Text).OrderBy(o => o.AppName))
            {
                ListViewItem lvi = new ListViewItem(new string[] { "app", app.AppName, app.BundleId ?? string.Empty });
                listViewResult.Items.Add(lvi);
            }

            listViewResult.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }



        private void buttonListIOSApps_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxAppiumServer.Text) || string.IsNullOrEmpty(toolStripTextBoxID.Text)
                || string.IsNullOrEmpty(toolStripTextBoxPassword.Text) || string.IsNullOrEmpty(textBoxUdid.Text))
            {
                MessageBox.Show("Please fill out Appium server address, ID, password, and UDID.");
                return;
            }
            toolStripComboBoxTarget.Text = "iOS";
            listViewResult.Items.Clear();
            listViewResult.Columns.Clear();
            listViewResult.Columns.Add("Item");
            listViewResult.Columns.Add("Name");
            listViewResult.Columns.Add("Bundle ID");

            AppiumServerUtils.Initialize(textBoxAppiumServer.Text, 22, toolStripTextBoxID.Text, toolStripTextBoxPassword.Text);
            foreach (App app in AppiumServerUtils.GetInstalledApps(textBoxUdid.Text).OrderBy(o => o.AppName))
            {
                ListViewItem lvi = new ListViewItem(new string[] { "app", app.AppName, app.BundleId });
                listViewResult.Items.Add(lvi);
            }

            listViewResult.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void comboBoxXpathPriority_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_xPathBuilder != null)
            {
                _xPathFirstPriority = comboBoxXpathPriority.SelectedItem.ToString();
                _xPathBuilder.ClearFirstPriorityAttribute();
                _xPathBuilder.AddFirstPriorityAttribute(_xPathFirstPriority);
            }
        }

        private void BtnAndroidAppslist_lidt(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxAppiumServer.Text) || string.IsNullOrEmpty(toolStripTextBoxID.Text)
                  || string.IsNullOrEmpty(toolStripTextBoxPassword.Text) || string.IsNullOrEmpty(textBoxUdid.Text))
            {
                MessageBox.Show("Please fill out Appium server address, ID, password, and UDID.");
                return;
            }
            toolStripComboBoxTarget.Text = "Android";
            listViewResult.Items.Clear();
            listViewResult.Columns.Clear();
            listViewResult.Columns.Add("Item");
            listViewResult.Columns.Add("Name");
            listViewResult.Columns.Add("Package Name");

            AppiumServerUtils.Initialize(textBoxAppiumServer.Text, 22, toolStripTextBoxID.Text, toolStripTextBoxPassword.Text);
            
            // Fixed: Use GetInstalledApps with UDID parameter instead of GetInstalledAndroidApps
            foreach (App app in AppiumServerUtils.GetInstalledApps(textBoxUdid.Text).OrderBy(o => o.AppName))
            {
                switch (app.AppName)
                {
                    case string name when name.Contains("com.hp.printercontrol"):
                        app.AppName = "Hp Smart";
                        break;

                    case string name when name.Contains("com.google.android.apps.chromecast.app"):
                        app.AppName = "Chrome";
                        app.BundleId = "com.android.chrome";
                        break;

                    case string name when name.Contains("com.oneplus.note"):
                        app.AppName = "Note";
                        break;

                    case string name when name.Contains("com.easywork.easycast"):
                        app.AppName = "EasyCast";
                        break;
                    case string name when name.Contains("com.microsoft.emmx"):
                        app.AppName = "Edge";
                        break;
                    case string name when name.Contains("com.whatsapp"):
                        app.AppName = "Whatsapp";
                        break;

                    default:
                        break;
                }

                ListViewItem lvi = new ListViewItem(new string[] { "app", app.AppName, app.BundleId });
                listViewResult.Items.Add(lvi);
            }

            listViewResult.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }


        private void listViewResult_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            switch (listViewResult.SelectedItems[0].SubItems[0].Text)
            {
                case "device":
                    textBoxUdid.Text = listViewResult.SelectedItems[0].SubItems[2].Text;
                    break;
                case "app":
                    textBundleId.Text = listViewResult.SelectedItems[0].SubItems[2].Text;
                    break;
            }
        }

        #endregion


        private void buttonTest_Click(object sender, EventArgs e)
        {
            AppiumServerUtils.Initialize(textBoxAppiumServer.Text, 22, toolStripTextBoxID.Text, toolStripTextBoxPassword.Text);
            AppiumServerUtils.InstallWDA("homopsychos");
        }

    }
}

