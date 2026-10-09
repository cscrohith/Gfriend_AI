using FastColoredTextBoxNS;
using HP.DeviceAutomation.Jedi;
using HP.GFriend.Client;
using HP.GFriend.Client.Forms;
using HP.GFriend.Core;
using HP.GFriend.Core.Custom;
using HP.GFriend.Core.Execution;
using HP.GFriend.GFLogger;
using HP.GFriend.Keywords;
using HP.GFriend.Support;
using HP.GFriend.UI.Controls;
using HP.GFriend.UI.Device;
using HP.GFriend.UI.Tool;
using HP.GFriend.Utils.Git;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using HP.GFriend.UI.RunSpecWizard;
using HP.GFriend.Core.Execution.Spec;
using System.Configuration;
using Newtonsoft.Json.Linq;
using HP.GFriend.Core.BuiltIn;
using GFriendUI;
namespace HP.GFriend.UI
{
    public partial class MainForm : Form
    {
        private Thread _bgWorkerThread;

        #region readonlyVariables
        private readonly string Description = "Description";
        private readonly string DeviceAddress = "DeviceAddress";
        private readonly string LanDebugAddress = "LanDebugAddress";
        private readonly string Port = "Port";
        private readonly string AdminId = "AdminId";
        private readonly string AdminPassword = "AdminPassword";
        private readonly string DeviceType = "DeviceType";

        private readonly string ServerInfo = "Server Info";
        private readonly string AssetServerAddress = "Asset Server Address";
        private readonly string ScriptServerAddress = "Repository Server Address";
        private readonly string OpenFileInfo = "Open File Info";
        private readonly string OpenFolderinfo = "Open Folder Info";
        private readonly string OpenFolderPath = "Folder Path";
        private readonly string DeviceInfo = "Device Setting";
        private readonly string TargetDevice = "Target Device";
        private readonly string TextRunTargetDevice = "Text Run Target Device";
        private readonly string SelectedTSList = "Selected TS LIST";
        private readonly string SelectedTCList = "Selected TC LIST";

        private readonly List<string> SupportedExtension = new List<string>(new string[] { ".txt", ".gfscript", ".gflib", ".gfvar" });
        private readonly List<string> ExecuteSupportedExtension = new List<string>(new string[] { ".txt", ".gfscript" });


        public readonly string gflib = ".gflib";
        #endregion]
        private const string Url = "https://engage.cloud.microsoft/main/org/hp.com/groups/eyJfdHlwZSI6Ikdyb3VwIiwiaWQiOiIyMDAxMjc2ODQ2MDgifQ";
            // Old Yammer Link "https://engage.cloud.microsoft/main/org/hp.com/groups/eyJfdHlwZSI6Ikdyb3VwIiwiaWQiOiI0NTc2NzUyNDM1MiJ9/new?domainRedirect=true";


        private GFScriptTabPage _currentTab;

        private string _outputDir = null;
        private string _devicefilePathINI = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DeviceList.ini");
        private string _devicefilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DeviceList.xml");
        private string _libfilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "libs");
        private string _scriptsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "scripts");
        private string _externalToolsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "tools");
        private string _settingFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Settings.ini");
        private string _manualPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "manual", "README.html");

        private IniFile _deviceInfoINI;
        private DeviceInfo _deviceInfo;
        private static int newTabCount = 1;
        private bool _checkAll = true;

        private List<DeviceUnderTest> _dutList = new List<DeviceUnderTest>();
        private DeviceUnderTest _dut;

        // Store selected device IDs for multi-device support
        private List<string> _selectedDeviceIds = new List<string>();

        private string _scriptServerAddress = "http://130.31.193.61:8080";
        private List<string> _loadedCheckedNode = new List<string>();
        private List<string> _loadedTCList = new List<string>();
        private string _loadedSelectedTab = null;
        private string _assetServerAddress = "130.31.192.225";

        private List<string> _tabList = new List<string>();

        private Dictionary<ToolStripMenuItem, GFTool> _externalTools;

        private TextBoxStreamWriter _streamTextBoxWriter;

        private string _currentTs = "New Test Suite";
        private List<string> _testFileList = new List<string>();
        private bool _runAllTC = false;

        private List<string> _tcToRun = null;

        // Stable, 0-based declaration-order Ids of the test cases the user checked, matching
        // TestCase.Id. This (never Name, never on-screen row index/position) is what actually
        // identifies which test case gets executed.
        private List<int> _tcIdsToRun = null;

        // Static variables for options
        internal static string ScriptDescriptonTemplate = "<Description></Description>";
        internal static string LibraryDescriptionTemplate = "<Description></Description>\r\n<Parameters></Parameters>";
        internal static string OverallDescriptionTemplate = "<Description></Description>\r\n<Precondition></Precondition>";


        // Consts
        private const string _uel = "\x1B\x25\x2D\x31\x32\x33\x34\x35\x58";
        private const string _crlf = "\x0D\x0A";

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        private const int TCM_SETMINTABWIDTH = 0x1300 + 49;
        private const string ToEmail = "siva.kumar@hp.com";
        public WhatsNew whatsNew;
        //copilot
        private bool _isCopilot = false;
        private bool _isCopilotText = false;
        private string copilotText = "";
        private TabPage tabCoDeveloperToHideOrShow;
        private string fullText = "";
        private string coDeveloperSampleScript = "";
        private string previousKeyword = ""; //each time when user enters to next line from builtinkeyword , we are getting the same line again which will trigger codeveloper text . So storing the keyword in this variable to ignore triggering of codeveloper text again for the second time
        private TestDataManager testDataManager = new TestDataManager();
        private Dictionary<string, Library> availableLibs = null;
        private Library coDeveloperComboBoxlibraryDetails = null;
        private string jsonText = "";

        private bool _isTestCasePaused = false;
                private ManualResetEventSlim _pauseEvent = new ManualResetEventSlim(true);
public MainForm()
        {
			WhatsNewFormCode();
            _dutList = new List<DeviceUnderTest>();
            _dut = new DeviceUnderTest();

            CheckForIllegalCrossThreadCalls = false;

            InitializeComponent();
            menuMain.ShowItemToolTips = true;

            // The checkbox must be the only control that selects/deselects a test case.
            // ListView's default FullRowSelect behavior highlights/"selects" the whole row
            // (including the test case text) on a plain click, independent of the Checked
            // state. Clearing .Selected immediately after any selection change removes that
            // highlight without touching .Checked, so clicking/dragging/double-clicking the
            // test case text can never change which test case is checked.
            testCases_listView.ItemSelectionChanged += (s, args) =>
            {
                if (args.IsSelected)
                {
                    args.Item.Selected = false;
                }
            };

            yammerPageToolStripMenuItem.ToolTipText = "Click here to open the GFriend 2 Viva Engage page";
            _externalTools = new Dictionary<ToolStripMenuItem, GFTool>();

            
            if (!File.Exists(_devicefilePath))
            {
                _deviceInfo = new DeviceInfo(_devicefilePath);

                if (File.Exists(_devicefilePathINI))
                {
                    _deviceInfoINI = new IniFile(_devicefilePathINI);

                    foreach (string deviceName in _deviceInfoINI.GetSectionList())
                    {
                        DeviceUnderTest dut = GetDeviceInfoFromINI(deviceName);
                        _deviceInfo.Devices.Add(dut);
                    }
                    File.Delete(_devicefilePathINI);
                }
                _deviceInfo.Save(_devicefilePath);
            }
            else
            {
                _deviceInfo = HP.GFriend.UI.Device.DeviceInfo.Load(_devicefilePath);
            }


            PopulateDeviceSelectorFromDeviceInfo();

            gfInstantRun.SetDeviceList(_deviceInfo.GetDeviceIds());

            

            textEditor_tabControl.HandleCreated += textEditor_tabControl_HandleCreated;

            if(File.Exists(_settingFile))
            {
                IniFile appSettingsINI = new IniFile(_settingFile);
                _scriptsPath = appSettingsINI.GetValue(OpenFolderinfo, OpenFolderPath, _scriptsPath);
            }


            if (!Directory.Exists(_scriptsPath))
            {
                Directory.CreateDirectory(_scriptsPath);
            }

            gfScriptFileList.SetScriptPath(_scriptsPath);
            UpdateFolderlist();

            
            gfInstantRun.SetAllDuts(GetAllDuts());
            Application.ThreadException += Application_ThreadException;

            // Show git icon if 64bit os
            if(Environment.Is64BitOperatingSystem)
            {
                toolStripButtonGit.Visible = true;
            }

            //By default Copilot tab will be hidden 
            tabCoDeveloperToHideOrShow = tabsBottom.TabPages["tabCoDeveloper"];
            tabsBottom.TabPages.Remove(tabCoDeveloperToHideOrShow);

            availableLibs = LibraryUtils.GetAvailableLibraries();

            LoadReservedKeywordsData();

            HP.GFriend.Core.CommonExecutionInfo.Initialize();
            HP.GFriend.Core.CommonExecutionInfo.SetVariable(SystemVariables.CO_DEVELOPER_MODE, "false");

            BuiltInLibrary.OnLatencyChanged += (sender, e) =>
            {
                var args = e as LatencyStatusChangedEventArgs;
                if (args != null)
                {

                    if (args.IsNetworkDown)
                    {
                        Logger.Debug("Network is down.Pausing connection");
                        tcPause_ToolStripButton_Click(sender, e); // Pause
                    }
                    else if (!args.IsNetworkDown)
                    {
                        Logger.Debug("Network is restored. Resuming connection");
                        tcPause_ToolStripButton_Click(sender, e); // Resume
                    }
                }
            };
        }

     
        [STAThread]
        private void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            if (e.Exception.StackTrace.Contains("FastColoredTextBoxNS.FastColoredTextBox"))
            {
                ResetOutput();
            }
        }

        private delegate void ResetOutputCallback();
        private void ResetOutput()
        {
            if(textOutput.InvokeRequired || tabOutput.InvokeRequired)
            {
                ResetOutputCallback roc = new ResetOutputCallback(ResetOutput);
                Invoke(roc);
            }
            else
            {
                GFOutputBox newTextbox = new GFOutputBox();
                tabOutput.Controls.Remove(textOutput);
                textOutput.SetupTextboxOutput(newTextbox);
                textOutput.Dispose();
                textOutput = newTextbox;
                tabOutput.Controls.Add(textOutput);

                _streamTextBoxWriter.ChangeOutputTarget(textOutput);

                textOutput.Visible = true;
                tabOutput.Refresh();
            }
        }



        #region GeneralMethods

        /// <summary>
        /// Get real path from file list TreeView
        /// </summary>
        private string GetRealTestPath(string relativePath)
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
        /// It creates folder for libraries and scripts if there didn't exist.
        /// </summary>
        private void UpdateFolderlist()
        {
            DirectoryInfo libFolderPath = new DirectoryInfo(_libfilePath);
            DirectoryInfo scriptFolderPath = new DirectoryInfo(_scriptsPath);

            if (!libFolderPath.Exists)
            {
                libFolderPath.Create();
            }

            if (!scriptFolderPath.Exists)
            {
                scriptFolderPath.Create();
            }
            LoadKeywordListMenu();

        }

        /// <summary>
        /// Update Tab data after save or load file
        /// If _tabData dictionary is not contains the file info, add new object for it. (Load or Save As / Save new file)
        /// If not, change _tabData value to "false" - It means that it didn't changed at original. (Save)
        /// </summary>
        /// <param name="filePath">File path for new tab</param>
        private void UpdateTabData(string filePath)
        {
            if (!_tabList.Contains(filePath))
            {
                _tabList.Add(filePath);
                _currentTab.IsAdded = true;
            }
            else
            {
                GFScriptTabPage gfTab = (GFScriptTabPage)textEditor_tabControl.TabPages[filePath];
                gfTab.IsUpdated = false;
            }
        }

        /// <summary>
        /// Append text to output filed
        /// </summary>
        /// <param name="text">Text for appending to output field</param>
        delegate void AppendToOutputCallback(string text, bool timeStamp);
        private void AppendToOutput(string text, bool timeStamp = true)
        {
            try
            {
                if (this.textOutput.InvokeRequired)
                {
                    AppendToOutputCallback atc = new AppendToOutputCallback(AppendToOutput);
                    this.Invoke(atc, new object[] { text, timeStamp });
                }
                else
                {
                    if (timeStamp)
                    {
                        textOutput.AppendText("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + text + "\r\n");
                    }
                    else
                    {
                        textOutput.AppendText(text + "\r\n");
                    }

                    textOutput.GoEnd();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// It set object for DeviceunderTest.
        /// </summary>
        /// <param name="deviceId">Key for device</param>
        private DeviceUnderTest SetDeviceUnderTest(string deviceId)
        {
            return _deviceInfo.GetDevice(deviceId);
            
        }

        /// <summary>
        /// Get all duts list from local
        /// </summary>
        /// <returns>DUTs list</returns>
        public List<DeviceUnderTest> GetAllDuts()
        {
            return _deviceInfo.Devices;
        }

        /// <summary>
        /// Returns the DUTs corresponding to the devices the user selected in the
        /// Target Devices tree (one per platform).  The list preserves the order in
        /// which the user selected them so the executor initialises platforms lazily
        /// in that same order.
        /// </summary>
        private List<DeviceUnderTest> GetSelectedDuts()
        {
            if (_selectedDeviceIds == null || _selectedDeviceIds.Count == 0)
                return new List<DeviceUnderTest>();

            if (_deviceInfo == null || _deviceInfo.Devices == null)
                return new List<DeviceUnderTest>();

            // Preserve selection order
            var result = new List<DeviceUnderTest>();
            foreach (string deviceId in _selectedDeviceIds)
            {
                var dut = _deviceInfo.Devices.FirstOrDefault(d =>
                    d.DeviceId.Equals(deviceId, StringComparison.OrdinalIgnoreCase));
                if (dut != null)
                    result.Add(dut);
            }
            return result;
        }



        /// <summary>
        /// Update test case on list view. Preserves the checked state of the current items
        /// (keyed by its stable ordinal Id, not by name or row position) across the rebuild,
        /// so that a checkbox selection made by the user is never silently discarded by an
        /// unrelated refresh (e.g. triggered from a text-editor event such as double-click
        /// caret movement, auto-formatting, or a using-statement re-parse).
        /// </summary>
        private void UpdateListViewTestCases()
        {
            // Remember which stable Ids were checked before the rebuild.
            HashSet<int> previouslyCheckedIds = new HashSet<int>();
            foreach (ListViewItem existingItem in testCases_listView.Items)
            {
                if (existingItem.Checked && existingItem.Tag is int existingId)
                {
                    previouslyCheckedIds.Add(existingId);
                }
            }

            testCases_listView.Items.Clear();
            int ordinal = 0;
            foreach (KeyValuePair<int, string> tcInfo in _currentTab.GetTCList())
            {
                ListViewItem item = new ListViewItem(new string[] { "", tcInfo.Key.ToString(), tcInfo.Value });

                // Tag the item with its stable ordinal Id (0-based declaration order in the
                // script), matching the Id the Parser assigns to the corresponding TestCase.
                // Execution selection must use this Id - never the item's on-screen row
                // index/position - so that refreshing, re-sorting, or otherwise altering the
                // list's display order can never change which test case actually gets run,
                // and so that test cases with duplicate names are never ambiguous.
                item.Tag = ordinal;

                // Restore the checkbox state for this exact test case Id so a text-editor
                // triggered refresh (e.g. after a double-click on the test case text) cannot
                // wipe out or corrupt the user's current selection.
                if (previouslyCheckedIds.Contains(ordinal))
                {
                    item.Checked = true;
                }

                testCases_listView.Items.Add(item);
                ordinal++;
            }
        }

        /// <summary>
        /// Update for set AssetInventory Server
        /// </summary>
        /// <param name="serverAddress"> Asset server address </param>
        public void SetAssetInventoryServer(string serverAddress)
        {
            _assetServerAddress = serverAddress;
        }
        #endregion GeneralMethods



        #region TabControl
        private void TabPage_OnTextUpdate(object sender, TextChangedEventArgs e)
        {
         DisplayCoDeveloperText(e.ChangedRange.Text.Trim());
            UpdateListViewTestCases();
            textEditor_tabControl.Refresh();
        }

        /// <summary>
        /// SelectedIndexChanged event on textEditor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textEditor_tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (textEditor_tabControl.TabPages.Count > 0)
            {
                FastColoredTextBox textEditor = (FastColoredTextBox)textEditor_tabControl.SelectedTab.Controls[0];

                _currentTs = textEditor_tabControl.SelectedTab.Name;

                _currentTab = (GFScriptTabPage)textEditor_tabControl.SelectedTab;
                UpdateListViewTestCases();

            }
        }

        /// <summary>
        /// Change size of tabs title fit on their contents
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textEditor_tabControl_HandleCreated(object sender, EventArgs e)
        {
            SendMessage(this.textEditor_tabControl.Handle, TCM_SETMINTABWIDTH, IntPtr.Zero, (IntPtr)16);
        }

        /// <summary>
        /// Draw tab title with chekcing saved image, title text, and close image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textEditor_tabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            GFScriptTabPage tabPage = (GFScriptTabPage)textEditor_tabControl.TabPages[e.Index];
            var tabRect = textEditor_tabControl.GetTabRect(e.Index);

            SolidBrush sb = new SolidBrush(Color.FromArgb(240, 238, 233));

            if (textEditor_tabControl.SelectedIndex == e.Index)
            {
                sb.Color = Color.White;
            }
            g.FillRectangle(sb, e.Bounds);

            tabRect.Inflate(-3, -3);

            Bitmap closeImage = Properties.Resources.Cancel_7x_16x;
            Bitmap savedImage;

            if (_tabList.Contains(tabPage.Name) && tabPage.IsUpdated)
            {
                savedImage = Properties.Resources.UnSaved_16x;
            }
            else
            {
                savedImage = Properties.Resources.Saved_16x;
            }

            Point point = new Point((tabRect.Left + savedImage.Width + 2), tabRect.Top);

            e.Graphics.DrawImage(savedImage,
                (tabRect.Left + 2),
                tabRect.Top + (tabRect.Height - closeImage.Height) / 2);
            e.Graphics.DrawImage(closeImage,
                (tabRect.Right - closeImage.Width),
                tabRect.Top + (tabRect.Height - closeImage.Height) / 2);

            if (textEditor_tabControl.SelectedIndex == e.Index)
            {
                TextRenderer.DrawText(e.Graphics, tabPage.Text, tabPage.Font, point
                 , Color.Black, TextFormatFlags.Left);
            }
            else
            {
                TextRenderer.DrawText(e.Graphics, tabPage.Text, tabPage.Font, point
                 , Color.Gray, TextFormatFlags.Left);
            }
        }

        /// <summary>
        /// Close tab when close image on tab title is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textEditor_tabControl_MouseDown(object sender, MouseEventArgs e)
        {
            for (var i = 0; i < textEditor_tabControl.TabPages.Count; i++)
            {
                var tabRect = textEditor_tabControl.GetTabRect(i);
                tabRect.Inflate(-2, -2);
                var closeImage = Properties.Resources.Cancel_7x_16x;
                var imageRect = new Rectangle(
                    (tabRect.Right - closeImage.Width),
                    tabRect.Top + (tabRect.Height - closeImage.Height) / 2,
                    closeImage.Width,
                    closeImage.Height);

                if (imageRect.Contains(e.Location))
                {
                    GFScriptTabPage tabPage = (GFScriptTabPage)textEditor_tabControl.TabPages[textEditor_tabControl.TabPages[i].Name];

                    CloseTab(tabPage);
                    break;
                }
            }
        }

        /// <summary>
        /// Make context menu when right click on tab title.
        /// It include Save, Save As, Open Folder, Open Folder in File explorer, Close, Close All, and Close All But This.
        /// Tab is closed when mouse middle button is clicked on tab title.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textEditor_tabControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i < textEditor_tabControl.TabCount; i++)
                {
                    Rectangle r = textEditor_tabControl.GetTabRect(i);

                    if (r.Contains(e.Location))
                    {
                        textEditor_tabControl.SelectedIndex = i;

                        ContextMenuStrip tabContextmenu = new ContextMenuStrip();
                        ToolStripMenuItem tabContextmenuSave = new ToolStripMenuItem("Save");
                        ToolStripMenuItem tabContextmenuSaveAs = new ToolStripMenuItem("Save As");
                        ToolStripMenuItem tabContextmenuOpenFolder = new ToolStripMenuItem("Open Folder");
                        ToolStripMenuItem tabContextmenuOpenFolderInFileExplorer = new ToolStripMenuItem("Open Folder in File Explorer");
                        ToolStripSeparator tabContextmenuSeparator = new ToolStripSeparator();
                        ToolStripMenuItem tabContextmenuCloseFile = new ToolStripMenuItem("Close");
                        ToolStripMenuItem tabContextmenuCloseAll = new ToolStripMenuItem("Close All");
                        ToolStripMenuItem tabContextmenuCloseAllButThis = new ToolStripMenuItem("Close All But This");
                        tabContextmenuSave.Click += new EventHandler(tabContextmenuSave_Click);
                        tabContextmenuSaveAs.Click += new EventHandler(tabContextmenuSaveAs_Click);
                        tabContextmenuOpenFolder.Click += new EventHandler(tabContextmenuOpenFolder_Click);
                        tabContextmenuOpenFolderInFileExplorer.Click += new EventHandler(tabContextmenuOpenFolderInFileExplorer_Click);
                        tabContextmenuCloseFile.Click += new EventHandler(tabContextmenuCloseFile_Click);
                        tabContextmenuCloseAll.Click += new EventHandler(tabContextmenuCloseAll_Click);
                        tabContextmenuCloseAllButThis.Click += new EventHandler(tabContextmenuCloseAllButThis_Click);

                        if (File.Exists(textEditor_tabControl.SelectedTab.Name))
                        {
                            tabContextmenu.Items.AddRange(new ToolStripItem[] { tabContextmenuSave, tabContextmenuSaveAs, tabContextmenuOpenFolder, tabContextmenuOpenFolderInFileExplorer, tabContextmenuSeparator, tabContextmenuCloseFile, tabContextmenuCloseAll, tabContextmenuCloseAllButThis });
                        }
                        else
                        {
                            tabContextmenu.Items.AddRange(new ToolStripItem[] { tabContextmenuSave, tabContextmenuSaveAs, tabContextmenuSeparator, tabContextmenuCloseFile, tabContextmenuCloseAll, tabContextmenuCloseAllButThis });
                        }
                        tabContextmenu.Show(Cursor.Position);
                        break;
                    }
                }
            }
            if (e.Button == MouseButtons.Middle)
            {
                for (int i = 0; i < textEditor_tabControl.TabCount; i++)
                {
                    Rectangle r = textEditor_tabControl.GetTabRect(i);

                    if (r.Contains(e.Location))
                    {
                        GFScriptTabPage tab = (GFScriptTabPage)textEditor_tabControl.SelectedTab;
                        textEditor_tabControl.SelectedIndex = i;

                        GFScriptTabPage closeTab = (GFScriptTabPage)textEditor_tabControl.TabPages[i];

                        CloseTab(closeTab);

                        if (textEditor_tabControl.TabPages.Contains(tab))
                        {
                            textEditor_tabControl.SelectedTab = tab;
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Click save button on context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabContextmenuSave_Click(object sender, EventArgs e)
        {
            SaveTestSuite();
        }

        /// <summary>
        /// Click save as button on context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabContextmenuSaveAs_Click(object sender, EventArgs e)
        {
            SaveTestSuite(true);
        }

        /// <summary>
        /// Click clear button on text editor sides
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearTextOutput_button_Click(object sender, EventArgs e)
        {
            textOutput.Clear();
        }


        /// <summary>
        /// Click open Folder button on context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabContextmenuOpenFolder_Click(object sender, EventArgs e)
        {
            string directoryPath = Path.GetDirectoryName(textEditor_tabControl.SelectedTab.Name);

            if (!_scriptsPath.Equals(directoryPath))
            {
                gfScriptFileList.LoadFolder(directoryPath);
                _scriptsPath = directoryPath;
                textOutput.AppendText($"* Open folder: {_scriptsPath} \r\n");

            }
        }

        /// <summary>
        /// Click open folder in file explorer button on context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabContextmenuOpenFolderInFileExplorer_Click(object sender, EventArgs e)
        {
            string directoryPath = Path.GetDirectoryName(textEditor_tabControl.SelectedTab.Name);

            if (Directory.Exists(directoryPath))
            {
                Process process = Process.Start(directoryPath);
            }
        }

        /// <summary>
        /// Click close button on context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabContextmenuCloseFile_Click(object sender, EventArgs e)
        {
            CloseTab();
        }

        /// <summary>
        /// Click close all but this button on context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabContextmenuCloseAllButThis_Click(object sender, EventArgs e)
        {
            foreach (GFScriptTabPage tab in textEditor_tabControl.TabPages)
            {
                if (!tab.Equals(textEditor_tabControl.SelectedTab))
                {
                    CloseTab(tab);
                }
            }
        }

        /// <summary>
        /// Click close all button on context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabContextmenuCloseAll_Click(object sender, EventArgs e)
        {
            foreach (GFScriptTabPage tab in textEditor_tabControl.TabPages)
            {
                CloseTab(tab);
            }
        }

        /// <summary>
        /// It add new tab with blank. "New file" button use this method.
        /// </summary>
        private void AddNewTab()
        {
            GFScriptTabPage tabPage = new GFScriptTabPage($"new{newTabCount}     ", false);
            string filePath = tabPage.Text;
            tabPage.AccessibleName = filePath;
            tabPage.Name = filePath;
            _currentTs = filePath;
            AttachEditorEvents(tabPage.TextEditor);
            newTabCount++;

            textEditor_tabControl.TabPages.Add(tabPage);
            textEditor_tabControl.SelectedTab = tabPage;


            tabPage.InitStyles();
            tabPage.ApplyStyles();
            tabPage.TextEditor.Select();
            tabPage.OnTextUpdate += TabPage_OnTextUpdate;
            _currentTab = tabPage;
            UpdateTabData(filePath);
        }



        /// <summary>
        /// It add new tab with contents on loaded file. If exist tab is only one tab that not changed, not saved, and blank tab, the tab will be removed.
        /// If loaded tab is already exist on text editor, it didn't make new tab and it change focus to the tab.
        /// </summary>
        /// <param name="filePath">Path for loading file</param>
        private void AddNewTab(string filePath)
        {
            if (!File.Exists(filePath))
            {
                AppendToOutput($"\"{filePath}\" is not exist");
                return;
            }

            GFScriptTabPage tabPage = (GFScriptTabPage)textEditor_tabControl.TabPages[0];

            if ((textEditor_tabControl.TabCount == 1) && !File.Exists(tabPage.Name) && !tabPage.IsUpdated)
            {
                _tabList.Remove(tabPage.Name);
                textEditor_tabControl.TabPages.Remove(tabPage);
                tabPage.Dispose();
            }

            if (_tabList.Contains(filePath))
            {
                foreach (GFScriptTabPage tab in textEditor_tabControl.TabPages)
                {
                    if (tab.Name.Equals(filePath))
                    {
                        textEditor_tabControl.SelectedTab = tab;
                        return;
                    }
                }
            }

            tabPage = new GFScriptTabPage(Path.GetFileName(filePath) + "     ", false);

            string tsContent = File.ReadAllText(filePath);

            tabPage.AccessibleName = filePath;
            tabPage.Name = filePath;
            _currentTs = filePath;

            textEditor_tabControl.TabPages.Add(tabPage);
            textEditor_tabControl.SelectedTab = tabPage;

            tabPage.TextEditor.AppendText(tsContent);
            tabPage.LoadLibrary();

            UpdateListViewTestCases();
            tabPage.InitStyles();
            tabPage.ApplyStyles();
            tabPage.TextEditor.Select();
            AttachEditorEvents(tabPage.TextEditor);
            tabPage.BuildAutoCompleteMenu();

            _currentTab = tabPage;
            UpdateListViewTestCases();
            tabPage.OnTextUpdate += TabPage_OnTextUpdate;
            UpdateTabData(filePath);
        }

        private void RefreshTab(string filePath)
        {
            CloseTab(filePath);
            AddNewTab(filePath);
        }

        /// <summary>
        /// Close selected tab 
        /// If selected tab is last one, it makes new blank tab.
        /// </summary>        
        private void CloseTab()
        {
            GFScriptTabPage tabPage = (GFScriptTabPage)textEditor_tabControl.SelectedTab;
            CloseTab(tabPage);
        }

        private void CloseTab(string filePath)
        {
            if (!_tabList.Contains(filePath)) return;
            AddNewTab(filePath);
            CloseTab();
        }

        /// <summary>
        /// Close specific tab 
        /// If selected tab is last one, it makes new blank tab.
        /// </summary>
        /// <param name="tabPage">Tab page for closing</param>
        private void CloseTab(GFScriptTabPage tabPage)
        {
            if (tabPage.IsUpdated)
            {
                string fileName = Path.GetFileName(tabPage.Name);

                DialogResult dr = new DialogResult();

                if (!string.IsNullOrEmpty(fileName))
                {
                    dr = MessageBox.Show($"[{fileName.Trim()}] Unsaved changes. Do you want to Save with same name?", "Before close", MessageBoxButtons.YesNoCancel);
                }
                else
                {
                    dr = MessageBox.Show($"[{textEditor_tabControl.SelectedTab.Name.Trim()}] Unsaved changes. Do you want to Save with new name?", "Before close", MessageBoxButtons.YesNoCancel);
                }

                switch (dr)
                {
                    case DialogResult.Yes:
                        SaveTestSuite();
                        break;
                    case DialogResult.Cancel:
                        return;
                }
            }

            _tabList.Remove(tabPage.Name);

            tabPage.TabTestDataManager = null;
            textEditor_tabControl.TabPages.Remove(tabPage);
            tabPage.Dispose();

            if (textEditor_tabControl.TabPages.Count == 0)
            {
                AddNewTab();
            }
        }
        #endregion TabControl


        #region FileControl
        /// <summary>
        /// Make new file. It act same with "AddNewTab()" method.
        /// </summary>
        private void NewFile()
        {
            AddNewTab();
        }

        /// <summary>
        /// Open file to text editor
        /// </summary>
        /// <param name="filePath">Path for open file</param>
        private void OpenFile(string filePath)
        {
            if (filePath != null && !filePath.Equals(string.Empty))
            {
                _currentTs = filePath;
                AddNewTab(filePath);


                AppendToOutput($"* Open file: {filePath}");
            }
        }

        /// <summary>
        /// Save a file.
        /// If saveAs is true or file name is not exist, it saves new file name.
        /// </summary>
        /// <param name="saveAs">Check save with new file name</param>
        private void SaveTestSuite(bool saveAs = false)
        {
            if (saveAs || !File.Exists(_currentTs))
            {
                // Create
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.InitialDirectory = _scriptsPath;
                sfd.Filter = "GF Script|*.txt;*.gfscript|GF Custom Library|*.gflib|GF Variable File|*.gfvar";
                sfd.DefaultExt = "txt";
                sfd.ShowDialog();

                if (sfd.FileName != null && !sfd.FileName.Trim().Equals(string.Empty))
                {
                    _currentTs = sfd.FileName;

                    _tabList.Remove(textEditor_tabControl.SelectedTab.Name);
                    UpdateTabData(_currentTs);
                    textEditor_tabControl.SelectedTab.AccessibleName = _currentTs;
                    textEditor_tabControl.SelectedTab.Name = _currentTs;
                    textEditor_tabControl.SelectedTab.Text = Path.GetFileName(_currentTs) + "     ";
                    textEditor_tabControl.ResumeLayout();
                }
                else
                {
                    return;
                }
            }

            using (StreamWriter writer = new StreamWriter(_currentTs, false, Encoding.UTF8))
            {
                writer.Write(textEditor_tabControl.SelectedTab.Controls[0].Text);
                writer.Close();
            }

            GFScriptTabPage tabPage = (GFScriptTabPage)textEditor_tabControl.SelectedTab;

            if (tabPage.IsUpdated)
            {
                tabPage.IsUpdated = false;
                textEditor_tabControl.Refresh();
            }

            if (tabPage.AccessibleName.EndsWith(gflib))
            {
                CustomLibrary customLibrary = new CustomLibrary(tabPage.AccessibleName);
                LibraryUtils.AddCustomLibraryToAvailableLibraries(customLibrary);
                LoadKeywordListMenu();
            }
        }

        private void FolderTreeView_OnAppendOutput(object sender, GFGeneralEventArgs e)
        {
            AppendToOutput(e.Message);
        }

        private void FolderTreeView_OnFolderLoad(object sender, TreeViewEventArgs e)
        {
            filelist_dataGridView.Rows.Clear();
        }
        private void FolderTreeView_OnOpenFile(object sender, GFGeneralEventArgs e)
        {
            if(e != null)
            {
                OpenFile(e.Message);
            }
        }

        private void FolderTreeView_OnUpdateSelectedFile(object sender, TreeViewEventArgs e)
        {
            UpdateSelectFileList(e.Node.FullPath, e.Node.Checked);
        }

        private void FolderTreeView_OnLoadFolderException(object sender, GFGeneralEventArgs e)
        {
            InitScriptPathServerAddress();
            InitSettingsInfo();

            UpdateFolderlist();
            gfScriptFileList.LoadFolder(_scriptsPath);

            LoadSettingsInfo();
        }

        #endregion FileControl

        #region SettingsInfoControl

        /// <summary>
        /// Initialize settings info to INI file: Settings.ini
        /// It include 2 items.
        ///  "Asset Inventory server info at STB"
        ///  "GFriend script server info"        
        /// </summary>        
        private void InitSettingsInfo()
        {
            FileStream file = File.Create(_settingFile);
            file.Close();

            IniFile appSettingsINI = new IniFile(_settingFile);

            appSettingsINI.SetValue(ServerInfo, AssetServerAddress, _assetServerAddress);
            appSettingsINI.SetValue(ServerInfo, ScriptServerAddress, _scriptServerAddress);

            appSettingsINI.Flush();
        }

        /// <summary>
        /// Initialize script path and server addresses to default value        
        /// </summary>        
        private void InitScriptPathServerAddress()
        {
            _scriptsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "scripts");
            _assetServerAddress = "130.31.192.225";
            _scriptServerAddress = "http://130.31.193.61:8080";
        }

        /// <summary>
        /// Save settings info to INI file: Settings.ini
        /// It include 6 items.
        ///  "Asset Inventory server info at STB"
        ///  "GFriend script server info"
        ///  "Target device for TC/TS test"
        ///  "Target device for text run"
        ///  "Opened folder at tree view"
        ///  "Loaded files on editor text field"
        /// </summary>        
        private void SaveSettingsInfo()
        {
            if (!File.Exists(_settingFile))
            {
                FileStream file = File.Create(_settingFile);
                file.Close();
            }

            IniFile appSettingsINI = new IniFile(_settingFile);

            //appSettingsINI.RemoveValue(ServerInfo);
            appSettingsINI.RemoveValue(DeviceInfo);
            appSettingsINI.RemoveValue(OpenFolderinfo);
            appSettingsINI.RemoveValue(OpenFileInfo);
            appSettingsINI.RemoveValue(SelectedTSList);
            appSettingsINI.RemoveValue(SelectedTCList);

            if (_assetServerAddress != null)
            {
                appSettingsINI.SetValue(ServerInfo, AssetServerAddress, _assetServerAddress);
            }

            var selectedDevices = GetAllSelectedDeviceIds();
            if (selectedDevices.Count > 0)
            {
                appSettingsINI.SetValue(DeviceInfo, TargetDevice, GetSelectedDeviceId());
                appSettingsINI.SetValue(DeviceInfo, TextRunTargetDevice, gfInstantRun.GetSelectedDevice());
            }

            appSettingsINI.SetValue(OpenFolderinfo, OpenFolderPath, _scriptsPath);

            if (filelist_dataGridView.RowCount > 0)
            {
                
                foreach (TreeNode node in gfScriptFileList.Collect())
                {
                    if (SupportedExtension.Contains(Path.GetExtension(node.FullPath)) && node.Checked)
                    {
                        appSettingsINI.SetValue(SelectedTSList, node.FullPath, node.Checked);
                    }
                }
            }

            if (_tabList.Count > 0)
            {
                foreach (GFScriptTabPage tab in textEditor_tabControl.TabPages)
                {
                    if (!tab.IsUpdated && File.Exists(tab.Name))
                    {
                        string selectedTabName = textEditor_tabControl.SelectedTab.Name;

                        if (tab.Name.Equals(selectedTabName))
                        {
                            appSettingsINI.SetValue(OpenFileInfo, tab.Name, "true");
                        }
                        else
                        {
                            appSettingsINI.SetValue(OpenFileInfo, tab.Name, "false");
                        }
                    }
                }
            }

            if (testCases_listView.Items.Count > 0)
            {
                foreach (ListViewItem item in testCases_listView.Items)
                {
                    if (item.Checked)
                    {
                        appSettingsINI.SetValue(SelectedTCList, item.SubItems[2].Text + "|" + item.Index, item.Index);
                    }
                }
            }

            appSettingsINI.Flush();
        }

        /// <summary>
        /// Load settings info to INI file: Settings.ini
        /// It include 6 items.
        ///  "Asset Inventory server info at STB"
        ///  "GFriend script server info"
        ///  "Target device for TC/TS test"
        ///  "Target device for text run"
        ///  "Opened folder at tree view"
        ///  "Loaded files on editor text field"
        /// </summary>
        private void LoadSettingsInfo()
        {
            if (!File.Exists(_settingFile))
            {
                return;
            }
            try
            {
                IniFile appSettingsINI = new IniFile(_settingFile);

                if (appSettingsINI.GetSectionList().Count > 0)
                {
                    
                    foreach (string sectionName in appSettingsINI.GetSectionList())
                    {
                        if (sectionName.Equals(ServerInfo))
                        {
                            _assetServerAddress = appSettingsINI.GetValue(ServerInfo, AssetServerAddress, _assetServerAddress);
                            _scriptServerAddress = appSettingsINI.GetValue(ServerInfo, ScriptServerAddress, _scriptServerAddress);
                        }

                        if (sectionName.Equals(DeviceInfo))
                        {
                            foreach (KeyValuePair<string, string> item in appSettingsINI.GetValues(DeviceInfo))
                            {
                                if (item.Key.Equals(TargetDevice) && GetAllSelectedDeviceIds().Contains(item.Value))
                                {
                                    // Selection handled by multi-device selector
                                }

                                if (item.Key.Equals(TextRunTargetDevice) && GetAllSelectedDeviceIds().Contains(item.Value))
                                {
                                    gfInstantRun.SelectDevice(item.Value);
                                }
                            }
                        }
                        if (sectionName.Equals(OpenFolderinfo))
                        {
                            _scriptsPath = appSettingsINI.GetValue(OpenFolderinfo, OpenFolderPath, _scriptsPath);
                        }

                        if (sectionName.Equals(SelectedTSList))
                        {
                            foreach (string key in appSettingsINI.GetValues(SelectedTSList).Keys)
                            {
                                _loadedCheckedNode.Add(key);
                            }
                        }
                        if (sectionName.Equals(OpenFileInfo))
                        {
                            foreach (KeyValuePair<string, string> item in appSettingsINI.GetValues(OpenFileInfo))
                            {
                                if (item.Value.Equals("true"))
                                {
                                    _loadedSelectedTab = item.Key;
                                }
                                if (!File.Exists(item.Key))
                                {
                                    AppendToOutput($"\"{item.Key}\" is not exist");
                                    return;
                                }

                                AddNewTab(item.Key);
                            }
                        }
                        if (sectionName.Equals(SelectedTCList))
                        {
                            foreach (string key in appSettingsINI.GetValues(SelectedTCList).Keys)
                            {
                                _loadedTCList.Add(key);
                            }
                        }
                        
                        if(sectionName.Equals(OptionForm.DESCRIPTION_SECTION_NAME))
                        {
                            string tempStr = appSettingsINI.GetValue(OptionForm.DESCRIPTION_SECTION_NAME, OptionForm.DESCRIPTION_OVERALL_KEY, OverallDescriptionTemplate);
                            tempStr = tempStr.Replace(@"\n", Environment.NewLine);
                            OverallDescriptionTemplate = tempStr.Replace(Environment.NewLine, $"{Environment.NewLine}/// ");

                            tempStr = appSettingsINI.GetValue(OptionForm.DESCRIPTION_SECTION_NAME, OptionForm.DESCRIPTION_SCRIPT_KEY, ScriptDescriptonTemplate);
                            tempStr = tempStr.Replace(@"\n", Environment.NewLine);
                            ScriptDescriptonTemplate = tempStr.Replace(Environment.NewLine, $"{Environment.NewLine}/// ");

                            tempStr = appSettingsINI.GetValue(OptionForm.DESCRIPTION_SECTION_NAME, OptionForm.DESCRIPTION_LIBRARY_KEY, LibraryDescriptionTemplate);
                            tempStr = tempStr.Replace(@"\n", Environment.NewLine);
                            LibraryDescriptionTemplate = tempStr.Replace(Environment.NewLine, $"{Environment.NewLine}/// ");
                        }

                      
                    }
                }
            }
            catch (Exception)
            {
                AppendToOutput("* Invalid user environments settings. Create new environments settings file");
                InitScriptPathServerAddress();

                InitSettingsInfo();
                UpdateFolderlist();
                LoadSettingsInfo();
            }
        }

        /// <summary>
        /// Load checked file on treeview        
        /// </summary>
        private void LoadFileListTreeViewChecked()
        {
            
            if (_loadedCheckedNode != null)
            {
                foreach (TreeNode node in gfScriptFileList.Collect())
                {
                    if (SupportedExtension.Contains(Path.GetExtension(node.FullPath)) && _loadedCheckedNode.Contains(node.FullPath))
                    {
                        node.Checked = true;
                    }
                }
            }
            
        }

        /// <summary>
        /// Load checked TC on TC listview        
        /// </summary>
        private void LoadTCListChecked()
        {
            if (!string.IsNullOrEmpty(_loadedSelectedTab))
            {
                foreach (GFScriptTabPage tab in textEditor_tabControl.TabPages)
                {
                    if (_loadedSelectedTab.Equals(tab.Name))
                    {
                        textEditor_tabControl.SelectedTab = tab;
                        break;
                    }
                }
            }

            if (testCases_listView.Items.Count > 0)
            {
                foreach (ListViewItem item in testCases_listView.Items)
                {
                    if (_loadedTCList.Contains(item.SubItems[2].Text + "|" + item.Index))
                    {
                        item.Checked = true;
                    }
                }
            }
        }
        #endregion SettingsInfoControl

        #region UIEnabledControl
        /// <summary>
        /// Enable UI for TC Run
        /// </summary>        
        private void EnableUIForTCRun()
        {
            this.testSuites_toolStrip.Enabled = false;
            this.selectAll_toolStripButton.Enabled = false;
            this.tcStart_ToolStripButton.Enabled = false;
            this.tcPause_ToolStripButton.Enabled = true;
            this.tcStop_ToolStripButton.Enabled = true;
            this.refresh_toolStripButton.Enabled = false;
        }

        /// <summary>
        /// Enable UI for TS Run
        /// </summary>
        private void EnableUIForTSRun()
        {
            // Disable only the individual buttons that must not be used while a Test Suite
            // run is in progress. Disabling the whole TestCases_toolStrip container would
            // also force its child tcStop_ToolStripButton into a disabled visual/interactive
            // state (a ToolStripItem's effective Enabled state is the AND of its own Enabled
            // and its parent container's Enabled), even though tcStop is unrelated to a TS
            // run and must remain available/consistent.
            this.tcStart_ToolStripButton.Enabled = false;
            this.selectAll_toolStripButton.Enabled = false;
            this.openFolder_toolStripButton.Enabled = false;
            this.tsStart_toolStripButton.Enabled = false;
            this.tsStop_toolStripButton.Enabled = true;
            this.refresh_toolStripButton.Enabled = false;
        }

        /// <summary>
        /// Enable UI After TC/TS Run
        /// </summary>
        private void EnableUIAfterRun()
        {
            this.testSuites_toolStrip.Enabled = true;
            this.TestCases_toolStrip.Enabled = true;
            this.selectAll_toolStripButton.Enabled = true;
            this.tcStart_ToolStripButton.Enabled = true;
            this.tcPause_ToolStripButton.Enabled = false;
            this.tcStop_ToolStripButton.Enabled = false;
            this.openFolder_toolStripButton.Enabled = true;
            this.tsStart_toolStripButton.Enabled = true;
            this.tsStop_toolStripButton.Enabled = false;
            this.refresh_toolStripButton.Enabled = true;
        }

        /// <summary>
        /// Enable TC stop button
        /// </summary>
        private void EnableUIForStop()
        {
            this.tcStop_ToolStripButton.Enabled = true;
        }

        /// <summary>
        /// Disable TC stop button
        /// </summary>
        private void DisableUIForStop()
        {
            this.tcStop_ToolStripButton.Enabled = false;
        }


        #endregion UIEnabledControl

        #region FormControlEvnets
        /// <summary>
        /// Load mainform
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!EulaForm.IsEulaAccepted())
            {
                using (EulaForm eulaForm = new EulaForm())
                {
                    eulaForm.ShowDialog();
                    if (!eulaForm.DialogResult.Equals(DialogResult.OK))
                    {
                        Visible = false;
                        Application.Exit();
                    }
                }
            }
            this.WindowState = FormWindowState.Maximized;

            filelist_dataGridView.ColumnCount = 1;
            filelist_dataGridView.Columns[0].Name = "Test Suites";
            filelist_dataGridView.Columns[0].Width = filelist_dataGridView.Width - 2;
            filelist_dataGridView.ReadOnly = true;

            AddNewTab();

            if (!File.Exists(_settingFile))
            {
                InitSettingsInfo();
            }

            LoadSettingsInfo();

            LoadExternalTools(_externalToolsPath);
            LoadKeywordListMenu();
            //LoadExternalTools();

            if (externalTool_ToolStripMenuItem.DropDownItems.Count < 1)
            {
                externalTool_ToolStripMenuItem.Enabled = false;
            }

            
            gfScriptFileList.LoadFolder(_scriptsPath);
            

            LoadFileListTreeViewChecked();

                LoadTCListChecked();

                AppendToOutput($"* Open folder: {_scriptsPath}");

                         // Initialize multi-device selector
                         // InitializeMultiDeviceSelector();
                     }

                /// <summary>
                /// Initialize multi-device selector with available devices
                /// </summary>
                private void InitializeMultiDeviceSelector()
                {
                    // DEPRECATED: Placeholder now used instead of MultiDeviceSelector
                    // This method left for reference only
                    return;
                    /*
                    if (multiDeviceSelector == null) return;

                    try
                    {
                        // Load devices from the device info
                        var platformDevices = new Dictionary<string, List<string>>();

                        // Get device IDs and organize by platform
                        var deviceIds = _deviceInfo.GetDeviceIds();

                        foreach (var deviceId in deviceIds)
                        {
                            // Try to determine platform from device type or use default
                            string platform = GetPlatformFromDeviceId(deviceId);

                            if (!platformDevices.ContainsKey(platform))
                            {
                                platformDevices[platform] = new List<string>();
                            }

                            if (!platformDevices[platform].Contains(deviceId))
                            {
                                platformDevices[platform].Add(deviceId);
                            }
                        }

                        // Load into the multi-device selector
                        multiDeviceSelector.LoadTargetDevices(platformDevices);
                        multiDeviceSelector.SelectionChanged += MultiDeviceSelector_SelectionChanged;
                    }
                    catch (Exception ex)
                    {
                        AppendToOutput($"* Error initializing device selector: {ex.Message}");
                    }
                    */
                }

                /// <summary>
                /// Handle device selection change
                /// </summary>
                private void MultiDeviceSelector_SelectionChanged(object sender, EventArgs e)
                {
                    // DEPRECATED: This event handler is no longer used
                    return;
                    /*
                    try
                    {
                        var selectedDevices = multiDeviceSelector.GetSelectedDevices();

                        if (selectedDevices.Count > 0)
                        {
                            // Update device address for the first selected device
                            string firstDevice = selectedDevices[0].Device;
                            UpdateDeviceAddressLabel(firstDevice);
                        }
                        else
                        {
                            // Clear device address
                            deviceAddress_label.Text = "";
                        }
                    }
                    catch (Exception ex)
                    {
                        AppendToOutput($"* Error handling device selection: {ex.Message}");
                    }
                    */
                }

                /// <summary>
                /// Get platform name from device ID
                /// </summary>
                private string GetPlatformFromDeviceId(string deviceId)
                {
                    if (string.IsNullOrEmpty(deviceId)) return "Unknown";

                    // Extract platform from device ID
                    // Examples: "Mac 1" -> "Mac", "Android 2" -> "Android", etc.
                    string[] parts = deviceId.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0)
                    {
                        return parts[0]; // Return first word as platform
                    }
                    return "Unknown";
                }

                /// <summary>
                /// Update device address label
                /// </summary>
                private void UpdateDeviceAddressLabel(string deviceId)
                {
                    try
                    {
                        if (_deviceInfo != null)
                        {
                            var device = _deviceInfo.GetDevice(deviceId);
                            if (device != null)
                            {
                                deviceAddress_label.Text = device.DeviceAddress ?? "";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        AppendToOutput($"* Error updating device address: {ex.Message}");
                    }
                }

                /// <summary>
                /// Get currently selected device ID (first selected or default)
                /// </summary>
                private string GetSelectedDeviceId()
                {
                    if (_selectedDeviceIds.Count > 0)
                    {
                        return _selectedDeviceIds[0];
                    }
                    return null;
                }

                /// <summary>
                /// Get all selected device IDs
                /// </summary>
                private List<string> GetAllSelectedDeviceIds()
                {
                    return new List<string>(_selectedDeviceIds);
                }

                /// <summary>
                /// Populate device selector from device info
                /// </summary>
                private void PopulateDeviceSelectorFromDeviceInfo()
                {
                    // DEPRECATED: Placeholder now used instead of MultiDeviceSelector
                    return;
                }

                /// <summary>
                /// Legacy method for compatibility - wraps the new multi-device selector
                /// </summary>
                private void RefreshDeviceComboBox()
                {
                    PopulateDeviceSelectorFromDeviceInfo();
                }

                 /// <summary>
        /// Closing event for MainForm
        /// </summary>        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            for (int i = 0; i < textEditor_tabControl.TabCount; i++)
            {
                GFScriptTabPage tabPage = (GFScriptTabPage)textEditor_tabControl.TabPages[i];

                if (tabPage.IsUpdated)
                {
                    string fileName = Path.GetFileName(tabPage.Name);
                    DialogResult dr = new DialogResult();

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        dr = MessageBox.Show($"{fileName.Trim()}::Unsaved changes. Do you want to Save?", "Before close", MessageBoxButtons.YesNoCancel);
                    }
                    else
                    {
                        dr = MessageBox.Show($"{tabPage.Name.Trim()}::Unsaved changes. Do you want to Save?", "Before close", MessageBoxButtons.YesNoCancel);
                    }

                    switch (dr)
                    {
                        case DialogResult.Yes:
                            SaveTestSuite();
                            break;
                        case DialogResult.Cancel:
                            e.Cancel = true;
                            return;
                    }
                }
            }

            SaveSettingsInfo();
        }

        /// <summary>
        /// DragDrop event for btnHideShow
        /// </summary>        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files != null)
            {
                OpenFile(files[0]);
            }

        }

        /// <summary>
        /// DragEnter event for btnHideShow
        /// </summary>        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void Filelist_dataGridView_Resize(object sender, EventArgs e)
        {
            if (filelist_dataGridView.Columns.Count > 0)
            {
                filelist_dataGridView.Columns[0].Width = filelist_dataGridView.Width - 2;
            }
        }
        #endregion

        #region MenuItems
        /// <summary>
        /// Click event for openToolStripMenuItem button
        /// </summary>        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog odf = new OpenFileDialog();
            string filePath = null;
            odf.Multiselect = false;
            odf.Filter = "GF Script and Library|*.txt;*.gfscript;*.gflib;*.gfvar";
            odf.ShowDialog();

            filePath = odf.FileName;
            OpenFile(filePath);
        }

        /// <summary>
        /// Click event for findToolStripMenuItem button
        /// </summary>        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FastColoredTextBox textEditor = (FastColoredTextBox)textEditor_tabControl.SelectedTab.Controls[0];
            textEditor.ShowFindDialog();
        }

        /// <summary>
        /// Click event for replaceToolStripMenuItem button
        /// </summary>        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FastColoredTextBox textEditor = (FastColoredTextBox)textEditor_tabControl.SelectedTab.Controls[0];
            textEditor.ShowReplaceDialog();
        }

        /// <summary>
        /// Click event for undoToolStripMenuItem button
        /// </summary>        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FastColoredTextBox textEditor = (FastColoredTextBox)textEditor_tabControl.SelectedTab.Controls[0];
            textEditor.Undo();
        }

        /// <summary>
        /// Click event for gotoToolStripMenuItem button
        /// </summary>        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gotoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FastColoredTextBox textEditor = (FastColoredTextBox)textEditor_tabControl.SelectedTab.Controls[0];
            textEditor.ShowGoToDialog();
        }

        /// <summary>
        /// Click event for newToolStripMenuItem button
        /// </summary>        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewFile();
        }

        /// <summary>
        /// Click event for saveToolStripMenuItem button
        /// </summary>        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveTestSuite();
        }

        /// <summary>
        /// Click event for saveAsToolStripMenuItem button
        /// </summary>        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveTestSuite(true);
        }

        /// <summary>
        /// Click event for runTestToolStripMenuItem button
        /// </summary>        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void runTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _runAllTC = true;
            RunTest(TestType.AllTC);
        }

        /// <summary>
        /// Click event for tcStart_ToolStripButton button
        /// </summary>        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tcStart_ToolStripButton_Click(object sender, EventArgs e)
        {
            _runAllTC = false;
            RunTest(TestType.PartialTC);
        }

        /// <summary>
        /// Click event for tcStop_ToolStripButton button
        /// </summary>        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tcStop_ToolStripButton_Click(object sender, EventArgs e)
        {
            StopTest();
        }

        /// <summary>
        /// Click event for tsStart_toolStripButton button
        /// </summary>        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsStart_toolStripButton_Click(object sender, EventArgs e)
        {
            if (filelist_dataGridView.Rows.Count > 0)
            {
                _runAllTC = true;
                RunTest(TestType.FileList);
            }
            else
            {
                MessageBox.Show("Please select at least one test suite");
            }
        }

        /// <summary>
        /// Click event for tsStop_toolStripButton button
        /// </summary>        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsStop_toolStripButton_Click(object sender, EventArgs e)
        {
            StopTest();
        }

        /// <summary>
        /// Click event for stopTestToolStripMenuItem button
        /// </summary>        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stopTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopTest();
        }


        /// <summary>
        /// Click event for aboutToolStripMenuItem button
        /// </summary>        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm abf = new AboutForm(this);
            abf.Show();
        }

        /// <summary>
        /// Click event for getPositionToolStripMenuItem button
        /// </summary>        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void getPositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetTouchEventForm gtef = new GetTouchEventForm();
            gtef.Show();
        }

        /// <summary>
        /// Click event for deviceListToolStripMenuItem        
        /// </summary>
        private void deviceListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisplayDeviceList();
        }

        /// <summary>
        /// Click event ScriptRepositoryServerToolStripMenuItem
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScriptRepositoryServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                GFClientForm clientForm = new GFClientForm(this.Icon, _scriptsPath);
                clientForm.Show();
            }
            catch (GFServerException ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        /// <summary>
        /// Load external tools list when the file includes .exe and .bat
        /// </summary>
        /// <param name="folderPath">Location for external tools</param>
        private void LoadExternalTools(string folderPath)
        {
            if (!Directory.Exists(_externalToolsPath))
            {
                Directory.CreateDirectory(_externalToolsPath);
            }

            foreach (string file in Directory.GetFiles(_externalToolsPath))
            {
                if (Path.GetExtension(file).Equals(".gftool"))
                {
                    try
                    {
                        GFTool exTool = new GFTool(file);
                        ToolStripMenuItem item = new ToolStripMenuItem(exTool.DisplayName);
                        item.ToolTipText = exTool.Description;
                        item.AccessibleName = file;
                        externalTool_ToolStripMenuItem.DropDownItems.Add(item);
                        _externalTools.Add(item, exTool);
                        item.Click += new EventHandler(ExternalToolItems_Click);
                    }
                    catch (Exception ex)
                    {
                        AppendToOutput($"Can not load external tools with {file}");
                    }
                }
            }
        }

        /// <summary>
        /// Click event for External Tool items button
        /// Execute selected external tool
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExternalToolItems_Click(object sender, EventArgs e)
        {
            GFTool extTool = _externalTools[(ToolStripMenuItem)sender];
            ArgData data = new ArgData();
            data.ScriptRoot = _scriptsPath;
            var _deviceId = GetSelectedDeviceId();
            if (!string.IsNullOrEmpty(_deviceId))
            {
                _dut = SetDeviceUnderTest(_deviceId);

                data.DeviceAddress = _dut.DeviceAddress;
                data.AdminId = _dut.AdminId;
                data.AdminPassword = _dut.AdminPassword;
                data.Port = _dut.Port.ToString();
            }

            extTool.Run(data);
        }

        /// <summary>
        /// Make libraries list on menu for make help for each library
        /// </summary>
        private void LoadKeywordListMenu()
        {
            keywordListToolStripMenuItem.DropDownItems.Clear();
            
            foreach(string libName in LibraryUtils.GetAvailbleLibraryNames(_scriptsPath,true))
            {
                ToolStripMenuItem item = new ToolStripMenuItem(libName);
                keywordListToolStripMenuItem.DropDownItems.Add(item);
                item.Click += new EventHandler(KeyWordlist_Click);
            }

        }

        /// <summary>
        /// Click event keyword list
        /// Display keyword manual for each library
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyWordlist_Click(object sender, EventArgs e)
        {
            string libraryName = (sender as ToolStripMenuItem).Text;
            Library library = LibraryUtils.GetAvailableLibraries(_scriptsPath)[libraryName];
            string keywordDoc = Path.Combine(Environment.CurrentDirectory, $"GF_Keywords_{library.Name}.html");

            if (File.Exists(keywordDoc))
            {
                File.Delete(keywordDoc);
            }
            library.Load(library.Name);

            library.GenerateKeywordDocumentation(Environment.CurrentDirectory, $"GF_Keywords_{library.Name}.html");
            Process.Start(keywordDoc);
        }

        /// <summary>
        /// Click event manual
        /// Display manual for GFriend
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void manualToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(_manualPath))
            {
                Process.Start(_manualPath);
            }
        }
        

        private void TestRunSpecWizardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TestRunSpecWizardForm wizard = new TestRunSpecWizardForm(_scriptsPath, _deviceInfo.Devices);
            wizard.Show();
        }

        private void OptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IniFile appSettingsINI = new IniFile(_settingFile);
            OptionForm optionForm = new OptionForm(appSettingsINI);
            optionForm.Show();
        }
        private void toolStripButtonGit_Click(object sender, EventArgs e)
        {
            if(gitControl == null)
            {
                gitControl = new GitControl();
                gitControl.Changed += delegate (object gitControl, GitEvent gitEvent) 
                { 
                    AppendToOutput(gitEvent.Message); 
                    if(!string.IsNullOrEmpty(gitEvent.RepoPath))
                    {
                        gfScriptFileList.LoadFolder(gitEvent.RepoPath);
                    }
                };
                gitControl.OpenLocalGitRepository(_scriptsPath);
            }

            if(filelist_panel.Controls.Contains(gitControl))
            {
                filelist_panel.Controls.Remove(gitControl);
                toolStripButtonGit.Checked = false;
            }
            else
            {
                filelist_panel.Controls.Add(gitControl);
                
                gitControl.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right))));
                gitControl.BorderStyle = BorderStyle.FixedSingle;
                gitControl.Location = gfScriptFileList.Location;
                gitControl.Anchor = gfScriptFileList.Anchor;
                gitControl.Size = new Size(filelist_dataGridView.Width, filelist_dataGridView.Height + gfScriptFileList.Height);
                gitControl.BringToFront();
                gitControl.Visible = true;
                

                toolStripButtonGit.Checked = true;
            }


        }

        #endregion MenuItems

        #region TestControlMethods    

        private string AllowPJLAnyway()
        {
            using (JediOmniDevice device = new JediOmniDevice(_dut.DeviceAddress, _dut.AdminPassword))
            {
                string urn = "urn:hp:imaging:con:service:security:SecurityService";
                string endpoint = "security";

                WebServiceTicket tic = device.WebServices.GetDeviceTicket(endpoint, urn);
                string oldValue = tic.FindElement("PjlDeviceAccess").Value;
                if (oldValue == "disabled")
                {
                    tic.FindElement("PjlDeviceAccess").SetValue("enabled");
                    device.WebServices.PutDeviceTicket(endpoint, urn, tic);
                }
                return oldValue;
            }

        }   

        private void paperless_on_button_Click(object sender, EventArgs e)
        {
            var _deviceId = GetSelectedDeviceId();
            if (!string.IsNullOrEmpty(_deviceId))
            {
                KeywordResult result;
                _dut = SetDeviceUnderTest(_deviceId);
                AllowPJLAnyway();
                string paperlessPjl = string.Join(_crlf, _uel, "@PJL RDYMSG DISPLAY=\"Paperless Mode\"", "@PJL SET SERVICEMODE=HPBOISEID", "@PJL SET JOBMEDIA=OFF", "@PJL DEFAULT JOBMEDIA=OFF", _uel);
                Logger.Trace("Paperless Enable PJL");
                Logger.Trace(paperlessPjl);
                result = MFPUtils.IpSend(_dut, paperlessPjl);

                if (result.Equals(KeywordResults.Error))
                {
                    AppendToOutput($"Fail to enable paperless mode on {_deviceId}");
                }
                else
                {
                    AppendToOutput($"Enable paperless mode on {_deviceId}");
                }
            }
        }

        private void paperless_off_button_Click(object sender, EventArgs e)
        {
            var _deviceId = GetSelectedDeviceId();
            if (!string.IsNullOrEmpty(_deviceId))
            {
                KeywordResult result;
                _dut = SetDeviceUnderTest(_deviceId);
                AllowPJLAnyway();
                string paperlessPjl = string.Join(_crlf, _uel, "@PJL RDYMSG DISPLAY=\"\"", "@PJL SET SERVICEMODE=HPBOISEID", "@PJL SET JOBMEDIA=ON", "@PJL DEFAULT JOBMEDIA=ON", _uel);
                Logger.Trace("Paperless Disable PJL");
                Logger.Trace(paperlessPjl);
                result = MFPUtils.IpSend(_dut, paperlessPjl);

                if (result.Equals(KeywordResults.Error))
                {
                    AppendToOutput($"Fail to disable paperless mode on {_deviceId}");
                }
                else
                {
                    AppendToOutput($"Disable paperless mode on {_deviceId}");
                }
            }
        }

        /// <summary>
        /// Stop test for TC/TS 
        /// </summary>
        private void StopTest()
        {
            Logger.Trace("In StopTest");
            EnableUIAfterRun();
            if (_bgWorkerThread != null)
            {
                _bgWorkerThread.Abort();
                _currentTab.TabTestDataManager?.Executor?.Dispose();
            }
            bgWorkerRunTest.Dispose();
            _currentTab.TabTestDataManager?.Executor?.Dispose();
            Logger.Trace("### Stop Test Called by User ###");
            GFriendLoggerServices.Dispose();
            
        }

        /// <summary>
        /// Run test for TC/TS 
        /// TestType:
        ///  - TestType.FileList: Test by TS (file list)
        ///  - TestType.AllTC: Test by all TCs when selected start button on tool strip tool
        ///  - TestType.PartialTC: Test by selected TCs
        /// </summary>
        private void RunTest(TestType testType)
        {
            DialogResult dr = new DialogResult();
            FastColoredTextBox fastColoredTextBox = (FastColoredTextBox)textEditor_tabControl.SelectedTab.Controls[0];
            List<string> usedDevices = new List<string>();
            List<DeviceUnderTest> allDuts = new List<DeviceUnderTest>();


            if (gfInstantRun.IsConnected)
            {
                dr = MessageBox.Show($"Device connection should be disconnect before test.\r\nDo you want to Disconnect?", "Device Connection", MessageBoxButtons.YesNo);

                switch (dr)
                {
                    case DialogResult.Yes:
                        gfInstantRun.DisconnectTextRun();
                        break;
                    case DialogResult.No:
                        return;
                }
            }
            var _deviceId = GetSelectedDeviceId();
            if (!string.IsNullOrEmpty(_deviceId) && _currentTab.TabTestDataManager != null)
            {
                _currentTab.TabTestDataManager.ChangeDefaultDUTId(_deviceId);
            }

            try
            {
                switch (testType)
                {
                    case TestType.FileList:
                        List<string> usedTSDevice = new List<string>();
                        TestDataManager testDataManager = null;
                        _testFileList.Clear();
                        if (filelist_dataGridView.Rows.Count > 0)
                        {
                            foreach (DataGridViewRow row in filelist_dataGridView.Rows)
                            {
                                string testfilePath = row.Cells[0].Value.ToString();
                                testDataManager = Parser.ParseTestSuite(GetRealTestPath(testfilePath));
                                if (testDataManager.GetUsedDevice() != null)
                                {
                                    usedTSDevice.AddRange(testDataManager.GetUsedDevice());
                                }

                                _testFileList.Add(GetRealTestPath(testfilePath));
                            }
                        }
                        if (usedTSDevice.Count > 0)
                        {
                            allDuts = GetAllDuts();
                            List<string> missingDuts = new List<string>();
                            foreach (string deviceName in usedTSDevice)
                            {
                                if (!allDuts.Exists(r => deviceName.Equals(r.DeviceId)))
                                {
                                    missingDuts.Add(deviceName);
                                }
                            }

                            if (missingDuts.Count > 0)
                            {
                                MessageBox.Show($"Following DUT(s) need to be registered in your device list : \r\n{string.Join(",", missingDuts)}");
                                return;
                            }
                        }
                        EnableUIForTSRun();
                        break;
                    case TestType.AllTC:
                    case TestType.PartialTC:

                        // Defensive: TabTestDataManager can end up null (e.g. tab recreated,
                        // or overwritten by a previous Runner.Run(out _currentTab.TabTestDataManager, ...)
                        // that failed mid-flight) which would otherwise cause a NullReferenceException
                        // deep inside Parser.ParseUsing. Re-create it here instead of throwing, so the
                        // exception-swallowing catch below can never be reached with stale selection state.
                        if (_currentTab.TabTestDataManager == null)
                        {
                            _currentTab.TabTestDataManager = new TestDataManager();
                        }

                        for (int line = 0; line < fastColoredTextBox.LineInfos.Count(); line++)
                        {
                            if (fastColoredTextBox.GetLineText(line).StartsWith(GFScriptTabPage.USING_TEXT))
                            {
                                Parser.ParseUsing(fastColoredTextBox.GetLineText(line), textEditor_tabControl.SelectedTab.Name, _currentTab.TabTestDataManager);
                            }
                        }
                        _currentTab.TabTestDataManager.PostActionAfterParsingUsings();

                        var selectedDevice = GetSelectedDeviceId();
                        if (!string.IsNullOrEmpty(selectedDevice))
                        {
                            _currentTab.TabTestDataManager.ChangeDefaultDUTId(selectedDevice);
                        }

                        usedDevices = _currentTab.TabTestDataManager.GetUsedDevice();

                        if (usedDevices.Count > 0)
                        {
                            allDuts = GetAllDuts();
                            List<string> missingDuts = new List<string>();
                            foreach (string deviceName in usedDevices)
                            {
                                if (!allDuts.Exists(r => deviceName.Equals(r.DeviceId)))
                                {
                                    missingDuts.Add(deviceName);
                                }
                            }

                            if (missingDuts.Count > 0)
                            {
                                MessageBox.Show($"Following DUT(s) need to be registered in your device list : \r\n{string.Join(",", missingDuts)}");
                                return;
                            }
                        }

                        _testFileList.Clear();

                        GFScriptTabPage tabPage = (GFScriptTabPage)textEditor_tabControl.SelectedTab;
                        string currentTs = tabPage.Name;

                        if (!File.Exists(currentTs) || tabPage.IsUpdated)
                        {
                            dr = MessageBox.Show($"Unsaved changes. It would be saved before test.", "Do you want to Save?", MessageBoxButtons.YesNo);

                            switch (dr)
                            {
                                case DialogResult.Yes:
                                    SaveTestSuite();
                                    break;
                                case DialogResult.No:
                                    return;
                            }
                        }

                        if (!File.Exists(currentTs))
                        {
                            return;
                        }

                        if (!_runAllTC)
                        {
                            _tcToRun = new List<string>();
                            _tcIdsToRun = new List<int>();
                            foreach (ListViewItem lvi in testCases_listView.Items)
                            {
                                if (lvi.Checked)
                                {
                                    _tcToRun.Add(lvi.SubItems[2].Text);

                                    // Capture the stable ordinal Id tagged on this item at
                                    // list-build time (see UpdateListViewTestCases). This is
                                    // what actually identifies the test case to the executor -
                                    // never the Name text, which can collide when two test
                                    // cases share the same displayed name, and never the
                                    // item's current on-screen row index/position.
                                    if (lvi.Tag is int tcId)
                                    {
                                        _tcIdsToRun.Add(tcId);
                                    }
                                }
                            }

                            if (_tcToRun.Count == 0)
                            {
                                MessageBox.Show("Please select at least one test case");
                                return;
                            }
                        }

                        _testFileList.Add(currentTs);
                        EnableUIForTCRun();
                        break;
                }
                tabsBottom.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                // IMPORTANT: Do not fall through to launching the background worker below.
                // _tcToRun/_tcIdsToRun are only rebuilt from the *current* checkbox selection
                // further up in this try block. If an exception (e.g. a NullReferenceException
                // from Parser.ParseUsing when TabTestDataManager is unexpectedly null) happens
                // before that rebuild code runs, those fields still hold whatever was selected
                // on a PREVIOUS run. Continuing on here would silently execute that stale
                // selection instead of the test case(s) the user actually just checked.
                AppendToOutput($"Error: {ex.Message}");
                Logger.Error(ex);
                return;
            }

            try
            {
                bgWorkerRunTest = new BackgroundWorker();
                bgWorkerRunTest.DoWork += new DoWorkEventHandler(bgWorkerRunTest_DoWork);
                bgWorkerRunTest.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorkerRunTest_Completed);
                bgWorkerRunTest.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                if (bgWorkerRunTest != null)
                {
                    bgWorkerRunTest.Dispose();
                }

                _currentTab.TabTestDataManager?.Executor?.Dispose();
                AppendToOutput($"\n### Test aborted by exception ###\n");
                AppendToOutput($"Contact GFriend Team with log : {Path.Combine(_outputDir, "log.txt")}\r\n", false);
                Logger.Trace("### Test aborted by exception ###");
                Logger.Error(ex);

                EnableUIAfterRun();

                GFriendLoggerServices.Dispose();
            }
        }

        /// <summary>
        /// Start run by BackGroundWorker
        /// </summary>
        private void bgWorkerRunTest_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                _bgWorkerThread = Thread.CurrentThread;

                using (_streamTextBoxWriter = new TextBoxStreamWriter(textOutput))
                {
                    _streamTextBoxWriter.TextBoxDisposed += _streamTextBoxWriter_TextBoxDisposed;
                    System.Console.SetOut(_streamTextBoxWriter);

                    foreach (string testSuite in _testFileList)
                    {
                        AppendToOutput($"\n\n### Test Suite ### \n{testSuite}\n");
                        string file = Path.GetFileName(testSuite);

                        if (file.Contains('.'))
                        {
                            var parts = file.Split('.');
                            file = parts[0];
                        }

                        _outputDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "output", DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + file);
                        Directory.CreateDirectory(_outputDir);

                        // If no device is selected, keep the DUT list empty.
                        // Physical-device libraries require an explicitly selected device.
                        _dutList = GetSelectedDuts();

                        Runner.InitGFRunner(_dutList, _outputDir);

                        // Use first selected device as the default DUT (for legacy single-device scripts)
                        string defaultDutId = GetSelectedDeviceId();

                        if (_runAllTC)
                        {
                            Runner.Run(testSuite, out _currentTab.TabTestDataManager, null, _libfilePath, defaultDutId);
                        }
                        else
                        {
                            // Prefer the stable-Id based selection (tcIdsToRun) so the exact
                            // test cases the user checked are executed - never a different one
                            // that happens to share the same displayed Name.
                            Runner.Run(testSuite, out _currentTab.TabTestDataManager, _tcToRun, _libfilePath, defaultDutId, null, null, _tcIdsToRun);
                        }

                    }
                }
            }
            catch (ThreadAbortException ex)
            {
                bgWorkerRunTest.Dispose();
                _currentTab.TabTestDataManager?.Executor?.Dispose();
                AppendToOutput($"\n### Force closing by tester ###\n");

                string filename = Path.Combine(_outputDir, "output.xml");

                if (File.Exists(filename))
                {
                    Reporter.FixXmlOutput(filename);
                }
                else
                {
                    AppendToOutput("Output file is not created\n");
                    }

                Logger.Trace("### Test abort by tester ###");
                Logger.Trace(ex);

                EnableUIAfterRun();

                GFriendLoggerServices.Dispose();
                }
            catch(LibraryInitializeException liex)
            {
                bgWorkerRunTest.Dispose();

                _currentTab.TabTestDataManager?.Executor?.Dispose();
                AppendToOutput($"\n### Test aborted by exception during library initialize. ###\n");
                AppendToOutput($"Check if your target device have sufficient information to execute the test.");
                AppendToOutput($"Exception Message : \n{liex.Message}");
                AppendToOutput($"Log : {Path.Combine(_outputDir, "log.txt")}\r\n", false);
                Logger.Trace("### Test aborted by exception ###");
                Logger.Error(liex);

                EnableUIAfterRun();

                GFriendLoggerServices.Dispose();
            }
            catch (Exception ex)
            {
                bgWorkerRunTest.Dispose();

                _currentTab.TabTestDataManager?.Executor?.Dispose();
                AppendToOutput($"\n### Test aborted by exception. ###\n");
                AppendToOutput($"Contact GFriend Team with log : {Path.Combine(_outputDir, "log.txt")}\r\n", false);
                Logger.Trace("### Test aborted by exception ###");
                Logger.Error(ex);

                EnableUIAfterRun();

                GFriendLoggerServices.Dispose();
            }
            finally
            {
                EnableUIAfterRun();
        }
        }

        private void _streamTextBoxWriter_TextBoxDisposed(object sender, EventArgs e)
        {
            ResetOutput();
        }

        /// <summary>
        /// Enable UI after bgWorkerRun test
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bgWorkerRunTest_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            Logger.Trace("Background worker test completed");
            EnableUIAfterRun();
            bgWorkerRunTest.Dispose();
            _currentTab.TabTestDataManager?.Executor?.Dispose();
            Logger.Trace("### bgWorkerRunTest_Completed -end ###");
            GFriendLoggerServices.Dispose();
        }
        #endregion TestControlMethods

        #region DeviceListControl
        /// <summary>
        /// Click event for deviceList_button       
        /// </summary>
        private void deviceList_button_Click(object sender, EventArgs e)
        {
            DisplayDeviceList();
        }

        /// <summary>
        /// Click event for targetDevicePlaceholder - shows multi-device selection popup
        /// </summary>
        private void targetDevicePlaceholder_Click(object sender, EventArgs e)
        {
            try
            {
                AppendToOutput("* Opening device selection popup...");

                // Group devices by platform using DeviceType property
                var platformDevices = new Dictionary<string, List<DeviceUnderTest>>();

                if (_deviceInfo != null && _deviceInfo.Devices != null)
                {
                    foreach (var device in _deviceInfo.Devices)
                    {
                        string platform = string.IsNullOrEmpty(device.DeviceType) ? "Unknown" : device.DeviceType;

                        if (!platformDevices.ContainsKey(platform))
                        {
                            platformDevices[platform] = new List<DeviceUnderTest>();
                        }

                        platformDevices[platform].Add(device);
                    }
                }

                // If no devices found, show empty dialog
                if (platformDevices.Count == 0)
                {
                    AppendToOutput("* No devices found. Please add devices first.");
                    MessageBox.Show("No devices available.\n\nPlease add devices using the Device List button.", 
                        "No Devices", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Create popup form
                Form popupForm = new Form
                {
                    Text = "Select Target Devices",
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    ShowIcon = false,
                    Width = 520,
                    Height = 580,
                    BackColor = Color.White,
                    Owner = this,
                    Font = new Font("Segoe UI", 9F)
                };

                // Base font MUST match platform NodeFont size so WinForms measures node widths correctly
                Font platformFont = new Font("Segoe UI", 11F, FontStyle.Bold);
                Font deviceFont   = new Font("Segoe UI", 9.5F, FontStyle.Regular);
                Font addressFont  = new Font("Segoe UI", 8.5F, FontStyle.Regular);

                // Create TreeView for device selection
                TreeView deviceTreeView = new TreeView
                {
                    CheckBoxes    = false,
                    Dock          = DockStyle.Fill,
                    Font          = platformFont,
                    ItemHeight    = 28,
                    ShowLines     = false,
                    ShowPlusMinus = true,
                    ShowRootLines = false,
                    FullRowSelect = true,
                    HideSelection = true,
                    BorderStyle   = BorderStyle.FixedSingle,
                    Indent        = 20,
                    BackColor     = Color.White
                };

                // Background tint per platform — used for the platform header row
                var platformColors = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Mac",      Color.FromArgb(173, 216, 230) },  // Light blue
                    { "iOS",      Color.FromArgb(255, 239, 213) },  // Papaya whip
                    { "Android",  Color.FromArgb(144, 238, 144) },  // Light green
                    { "Android2", Color.FromArgb(144, 238, 144) },  // Light green
                    { "Windows",  Color.FromArgb(176, 196, 222) },  // Light steel blue
                    { "Web",      Color.FromArgb(255, 228, 181) },  // Moccasin
                    { "Jedi",     Color.FromArgb(255, 218, 185) },  // Peach
                    { "Dune",     Color.FromArgb(221, 160, 221) },  // Plum
                    { "Ares",     Color.FromArgb(255, 182, 193) },  // Light pink
                    { "Cytrine",  Color.FromArgb(255, 255, 180) },  // Light yellow
                    { "Unknown",  Color.FromArgb(211, 211, 211) }   // Light gray
                };

                // Representative emoji per platform — shown on the platform header node
                var platformIcons = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Mac",      "💻" },
                    { "iOS",      "📱" },
                    { "Android",  "🤖" },
                    { "Android2", "🤖" },
                    { "Windows",  "🖥️" },
                    { "Web",      "🌐" },
                    { "Jedi",     "⚡" },
                    { "Dune",     "🖨" },
                    { "Ares",     "🖨" },
                    { "Cytrine",  "🖨" },
                    { "Unknown",  "❓" }
                };

                // Build tree: platform header  ▶  device row (○/●)  ▶  address row
                foreach (var platform in platformDevices.OrderBy(p => p.Key))
                {
                    int deviceCount = platform.Value.Count;
                    string platformIcon = platformIcons.ContainsKey(platform.Key)
                        ? platformIcons[platform.Key]
                        : "📦";
                    Color platformColor = platformColors.ContainsKey(platform.Key)
                        ? platformColors[platform.Key]
                        : Color.FromArgb(230, 230, 250);

                    // Platform header: icon + name + plain count number
                    TreeNode platformNode = new TreeNode($"{platformIcon}  {platform.Key}  ({deviceCount})")
                    {
                        Tag       = new { Type = "Platform", Name = platform.Key },
                        NodeFont  = platformFont,
                        ForeColor = Color.FromArgb(0, 51, 102),
                        BackColor = platformColor
                    };

                    foreach (var device in platform.Value.OrderBy(d => d.DeviceId))
                    {
                        // ○ = unselected   ● = selected
                        bool wasSelected = _selectedDeviceIds.Contains(device.DeviceId);
                        string circle    = wasSelected ? "●" : "○";

                        TreeNode deviceNode = new TreeNode($"{circle}  {device.DeviceId}")
                        {
                            Tag       = new { Type = "Device", Device = device },
                            NodeFont  = deviceFont,
                            ForeColor = Color.FromArgb(40, 40, 40),
                            BackColor = Color.White
                        };

                        // Address on its own indented child row
                        if (!string.IsNullOrWhiteSpace(device.DeviceAddress))
                        {
                            TreeNode addressNode = new TreeNode($"   {device.DeviceAddress}")
                            {
                                Tag       = new { Type = "Address" },
                                NodeFont  = addressFont,
                                ForeColor = Color.FromArgb(120, 120, 120),
                                BackColor = Color.White
                            };
                            deviceNode.Nodes.Add(addressNode);
                        }

                        deviceNode.Expand(); // always show address beneath device name
                        platformNode.Nodes.Add(deviceNode);
                    }

                    platformNode.Expand();
                    deviceTreeView.Nodes.Add(platformNode);
                }

                // Suppress default selection highlight
                deviceTreeView.BeforeSelect += (s, args) => { args.Cancel = true; };

                // Click device row  → toggle ○↔●, enforce one selection per platform
                // Click address row → delegate to parent device node
                deviceTreeView.NodeMouseClick += (s, args) =>
                {
                    if (args.Node == null || args.Button != MouseButtons.Left) return;

                    var tag = args.Node.Tag as dynamic;
                    if (tag == null) return;

                    TreeNode deviceNode = null;
                    if (tag.Type == "Device")
                        deviceNode = args.Node;
                    else if (tag.Type == "Address" && args.Node.Parent != null)
                        deviceNode = args.Node.Parent;

                    if (deviceNode == null) return;

                    bool isNowSelected = deviceNode.Text.StartsWith("○");

                    // Toggle this device
                    deviceNode.Text = isNowSelected
                        ? deviceNode.Text.Replace("○", "●")
                        : deviceNode.Text.Replace("●", "○");

                    // If now selected, unselect all sibling devices under same platform
                    if (isNowSelected && deviceNode.Parent != null)
                    {
                        foreach (TreeNode sibling in deviceNode.Parent.Nodes)
                        {
                            if (sibling != deviceNode && sibling.Text.StartsWith("●"))
                                sibling.Text = sibling.Text.Replace("●", "○");
                        }
                    }
                };

                // Block all standard checkbox interactions (defensive — CheckBoxes = false)
                deviceTreeView.BeforeCheck += (s, args) => { args.Cancel = true; };

                // Create info panel
                Panel infoPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 45,
                    BackColor = Color.FromArgb(240, 248, 255),
                    Padding = new Padding(15, 10, 15, 10)
                };

                Label infoLabel = new Label
                {
                    Text = "Select one device per platform. Multiple platforms can each have one device selected.",
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.DarkSlateGray,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                infoPanel.Controls.Add(infoLabel);

                // Create button panel
                Panel buttonPanel = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 60,
                    BackColor = Color.FromArgb(245, 245, 245)
                };

                // Separator line above button panel
                Panel separator = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 1,
                    BackColor = Color.LightGray
                };

                Button selectAllButton = new Button
                {
                    Text = "Select All",
                    Location = new Point(10, 14),
                    Width = 90,
                    Height = 30,
                    Font = new Font("Segoe UI", 9F),
                    UseVisualStyleBackColor = true
                };

                Button clearAllButton = new Button
                {
                    Text = "Clear All",
                    Location = new Point(108, 14),
                    Width = 90,
                    Height = 30,
                    Font = new Font("Segoe UI", 9F),
                    UseVisualStyleBackColor = true
                };

                Button cancelButton = new Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(275, 14),
                    Width = 95,
                    Height = 30,
                    Font = new Font("Segoe UI", 9F),
                    UseVisualStyleBackColor = true
                };

                Button okButton = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Location = new Point(378, 14),
                    Width = 95,
                    Height = 30,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    UseVisualStyleBackColor = true,
                    BackColor = Color.FromArgb(0, 120, 215),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };

                okButton.FlatAppearance.BorderSize = 0;

                // Select All: mark every device node as selected (●)
                selectAllButton.Click += (s, args) =>
                {
                    foreach (TreeNode platformNode in deviceTreeView.Nodes)
                        foreach (TreeNode devNode in platformNode.Nodes)
                            if (devNode.Text.StartsWith("○"))
                                devNode.Text = devNode.Text.Replace("○", "●");
                };

                // Clear All: mark every device node as unselected (○)
                clearAllButton.Click += (s, args) =>
                {
                    foreach (TreeNode platformNode in deviceTreeView.Nodes)
                        foreach (TreeNode devNode in platformNode.Nodes)
                            if (devNode.Text.StartsWith("●"))
                                devNode.Text = devNode.Text.Replace("●", "○");
                };

                buttonPanel.Controls.Add(selectAllButton);
                buttonPanel.Controls.Add(clearAllButton);
                buttonPanel.Controls.Add(cancelButton);
                buttonPanel.Controls.Add(okButton);

                popupForm.Controls.Add(deviceTreeView);
                popupForm.Controls.Add(separator);
                popupForm.Controls.Add(buttonPanel);
                popupForm.Controls.Add(infoPanel);
                popupForm.AcceptButton = okButton;
                popupForm.CancelButton = cancelButton;

                if (popupForm.ShowDialog() == DialogResult.OK)
                {
                    // Collect selected devices — identified by ● prefix on device nodes
                    var selectedDevices = new List<DeviceUnderTest>();
                    _selectedDeviceIds.Clear();

                    foreach (TreeNode platformNode in deviceTreeView.Nodes)
                    {
                        foreach (TreeNode deviceNode in platformNode.Nodes)
                        {
                            if (deviceNode.Text.StartsWith("●"))
                            {
                                var tag = deviceNode.Tag as dynamic;
                                if (tag != null && tag.Type == "Device")
                                {
                                    selectedDevices.Add(tag.Device);
                                    _selectedDeviceIds.Add(tag.Device.DeviceId);
                                }
                            }
                        }
                    }

                    // Update UI based on selection
                    if (selectedDevices.Count > 0)
                    {
                        targetDevicePlaceholder.Text = $"{selectedDevices.Count} device(s) selected";
                        targetDevicePlaceholder.ForeColor = Color.Black;

                        // Update device address label with first selected device
                        deviceAddress_label.Text = selectedDevices[0].DeviceId;

                        AppendToOutput($"* Selected {selectedDevices.Count} device(s):");
                        foreach (var device in selectedDevices)
                        {
                            AppendToOutput($"  - {device.DeviceType}: {device.DeviceId}");
                        }
                    }
                    else
                    {
                        targetDevicePlaceholder.Text = "Select Target";
                        targetDevicePlaceholder.ForeColor = Color.Gray;
                        deviceAddress_label.Text = "";
                        AppendToOutput("* No devices selected");
                    }
                }

                popupForm.Dispose();
            }
            catch (Exception ex)
            {
                AppendToOutput($"* Error showing device selection popup: {ex.Message}");
                MessageBox.Show($"Error showing device selection popup:\n{ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// <summary>
        /// LEGACY: Event handler for old single-device ComboBox (now handled by MultiDeviceSelector_SelectionChanged)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deviceId_comboBox_SelectedIndexChanged_Legacy(object sender, EventArgs e)
        {
            // This method is no longer used - multi-device selector handles selection changes
            // See: MultiDeviceSelector_SelectionChanged()
        }


        /// <summary>
        /// Set Device list at multiDeviceSelector and textRun_DeviceList_comboBox
        /// </summary>
        private void DisplayDeviceList()
        {
            string beforeValue = null, textRunBeforeValue = null;
            DeviceListForm deviceListForm = new DeviceListForm(_assetServerAddress);

            deviceListForm.sendAssetServer += new DeviceListForm.sendAssetServerDelegate(SetAssetInventoryServer);

            var selectedDevices = GetAllSelectedDeviceIds();
            if (selectedDevices.Count > 0)
            {
                beforeValue = selectedDevices[0]; // Save first selected device
                textRunBeforeValue = gfInstantRun.GetSelectedDevice();
            }
            deviceListForm.ShowDialog();
            // Reload DeviceInfo because DeviceListForm may have
            // added/deleted devices in DeviceList.xml.
            if (File.Exists(_devicefilePath))
            {
                _deviceInfo = HP.GFriend.UI.Device.DeviceInfo.Load(_devicefilePath);
            }

            // Clear selections that may refer to deleted devices.
            _selectedDeviceIds.RemoveAll(id => !_deviceInfo.GetDeviceIds().Contains(id));

            // Refresh device selector with updated device list
            PopulateDeviceSelectorFromDeviceInfo();
            gfInstantRun.SetDeviceList(_deviceInfo.GetDeviceIds());

            if (!string.IsNullOrEmpty(beforeValue) && GetAllSelectedDeviceIds().Count > 0 && _deviceInfo.GetDeviceIds().Contains(beforeValue))
            {
                // Try to restore previous selection
                // Note: multi-device selector doesn't have a direct "selected item" like ComboBox
                // User will need to manually reselect if desired
            }

            gfInstantRun.SetAllDuts(GetAllDuts());
            if (!string.IsNullOrEmpty(textRunBeforeValue) && gfInstantRun.GetDeviceList().Count > 0 && _deviceInfo.GetDeviceIds().Contains(textRunBeforeValue))
            {
                gfInstantRun.SelectDevice(textRunBeforeValue);
            }
        }


        private DeviceUnderTest GetDeviceInfoFromINI(string deviceId)
        {
            DeviceUnderTest dut = new DeviceUnderTest();
            int devicePort = 0;

            dut.DeviceId = deviceId;

            foreach (KeyValuePair<string, string> item in _deviceInfoINI.GetValues(deviceId))
            {
                if (item.Key.Equals(DeviceAddress))
                {
                    dut.DeviceAddress = item.Value;
                }
                if (item.Key.Equals(LanDebugAddress))
                {
                    dut.LanDebugAddress = item.Value;
                }
                if (item.Key.Equals(Port))
                {
                    if (Int32.TryParse(item.Value, out devicePort))
                    {
                        dut.Port = devicePort;
                    }
                }
                if (item.Key.Equals(AdminId))
                {
                    dut.AdminId = item.Value;
                }
                if (item.Key.Equals(AdminPassword))
                {
                    dut.AdminPassword = item.Value;
                }
                if (item.Key.Equals(Description))
                {
                    dut.Description = item.Value;
                }
                if (item.Key.Equals(DeviceType))
                {
                    dut.DeviceType = item.Value;
                }
            }

            return dut;
        }

        /// <summary>
        /// Update device address label to selected device
        /// </summary>
        private void UpdateDeviceAddresslabel(string deviceId)
        {
            if (GetAllSelectedDeviceIds().Count == 0)
            {
                deviceAddress_label.Text = "none";
            }
            else
            {
                deviceAddress_label.Text = _deviceInfo.GetDevice(deviceId).DeviceAddress;
            }
        }
        #endregion DeviceListControl

        #region FileListControl

        /// <summary>
        /// Click event for open folder on file list ToolStripBar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openFolder_toolStripButton_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrEmpty(fbd.SelectedPath))
                {
                    _scriptsPath = fbd.SelectedPath;

                    gfScriptFileList.LoadFolder(_scriptsPath);
                    AppendToOutput($"* Open folder: {_scriptsPath}");
                    if (gitControl != null)
                    {
                        gitControl.OpenLocalGitRepository(_scriptsPath);
                    }
                }
            }
        }

        /// <summary>
        /// Click event for refresh button on file list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refresh_toolStripButton_Click(object sender, EventArgs e)
        {
            gfScriptFileList.LoadFolder(_scriptsPath);
            AppendToOutput($"* Refresh folder: {_scriptsPath}");
        }

        /// <summary>
        /// Click event for new file button on text editor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonNewFile_Click(object sender, EventArgs e)
        {
            AddNewTab();
        }

        /// <summary>
        /// Click event for save button on text editor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>        
        private void save_toolStripButton_Click(object sender, EventArgs e)
        {
            SaveTestSuite();
        }

        /// <summary>
        /// Click event for save as button on text editor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveAs_toolStripButton_Click(object sender, EventArgs e)
        {
            SaveTestSuite(true);
        }

        /// <summary>
        /// Click event for open file button on text editor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openFile_toolStripButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog odf = new OpenFileDialog();
            string filePath = null;
            odf.InitialDirectory = _scriptsPath;
            odf.Multiselect = false;
            odf.Filter = "GF Script and Library|*.txt;*.gfscript;*.gflib;*.gfvar";
            odf.ShowDialog();

            filePath = odf.FileName;
            OpenFile(filePath);
        }

        /// <summary>
        /// Update Selected file list on filelist DataGridView
        /// </summary>
        /// <param name="filepath">File path for each file on file list DataGridView</param>
        /// <param name="fileChecked">File Checked Info for each file on file list DataGridView</param>
        private void UpdateSelectFileList(string filepath, bool fileChecked)
        {
            if (ExecuteSupportedExtension.Contains(Path.GetExtension(filepath)))
            {
                if (fileChecked)
                {
                    foreach (DataGridViewRow row in filelist_dataGridView.Rows)
                    {
                        if (row.Cells[0].Value.ToString().Equals(filepath))
                        {
                            return;
                        }
                    }
                    filelist_dataGridView.Rows.Add(filepath);
                }
                else
                {
                    foreach (DataGridViewRow row in filelist_dataGridView.Rows)
                    {
                        if (filepath.Equals(row.Cells[0].Value.ToString()))
                        {
                            filelist_dataGridView.Rows.Remove(row);
                            break;
                        }
                    }
                }
            }
        }






        #endregion FileListControl

        #region TCListControl
        /// <summary>
        /// Click event for Select All button on TC list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectAll_toolStripButton_Click(object sender, EventArgs e)
        {
            if (_checkAll)
            {
                foreach (ListViewItem listitem in testCases_listView.Items)
                {
                    listitem.Checked = true;
                    _checkAll = false;
                }
            }
            else
            {
                foreach (ListViewItem listitem in testCases_listView.Items)
                {
                    listitem.Checked = false;
                    _checkAll = true;
                }
            }
        }

        /// <summary>
        /// Item checked event for Select All button on TC list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void testCases_listView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            foreach (ListViewItem listtiem in testCases_listView.Items)
            {
                if (listtiem.Checked == true)
                {
                    selectAll_toolStripButton.Image = Properties.Resources.CheckboxClearAll_16x;
                    selectAll_toolStripButton.Text = "Clear All";

                    _checkAll = false;
                    break;
                }
                else
                {
                    _checkAll = true;

                    continue;
                }
            }

            if (_checkAll)
            {
                selectAll_toolStripButton.Image = Properties.Resources.CheckboxCheckAll_16x;
                selectAll_toolStripButton.Text = "Check All";

                _checkAll = true;
            }
        }

        /// <summary>
        /// MouseDoubleClick event for list view testcases. This only navigates the editor to
        /// the double-clicked test case's line - it must never change the Checked state of
        /// any item, and it must never rely on FocusedItem (which can point at a stale/wrong
        /// row) to identify which test case was actually double-clicked.
        /// </summary>        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewTestCases_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem clickedItem = testCases_listView.GetItemAt(e.X, e.Y);
            if (clickedItem == null)
            {
                return;
            }

            int line = Int32.Parse(clickedItem.SubItems[1].Text);

            FastColoredTextBox textEditor = (FastColoredTextBox)textEditor_tabControl.SelectedTab.Controls[0];

            textEditor.Selection = new Range(textEditor, 0, line - 1, 0, line - 1);
            textEditor.Select();
            textEditor.DoSelectionVisible();

        }
        #endregion TCListControl

        #region RunatServerControl
        private void runTestAtServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var deviceId = GetSelectedDeviceId();
                if (!string.IsNullOrEmpty(deviceId))
                {
                    _dut = SetDeviceUnderTest(deviceId);
                }

                GFServerExecutionForm executionForm = new GFServerExecutionForm(this.Icon, _deviceInfo.Devices, _dut);

                executionForm.Show();
            }
            catch (GFServerException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void runatTestServer_toolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                var deviceId = GetSelectedDeviceId();
                if (!string.IsNullOrEmpty(deviceId))
                {
                    _dut = SetDeviceUnderTest(deviceId);
                }

                GFServerExecutionForm executionForm = new GFServerExecutionForm(this.Icon, _deviceInfo.Devices, _dut);
                executionForm.Show();
            }
            catch (GFServerException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void showTestResult_toolStripButton_Click(object sender, EventArgs e)
        {
            WaitingForm waitingForm = new WaitingForm();
            Thread.Sleep(200);
            waitingForm.Show();

            GFServerViewerForm gFServerViewerForm = new GFServerViewerForm(Icon);
            gFServerViewerForm.Show();

            waitingForm.Close();
            gFServerViewerForm.Focus();
        }

        private void comment_toolStripButton_Click(object sender, EventArgs e)
        {
            _currentTab.AccessibleName = _currentTs;
            _currentTab.Name = _currentTs;
            _currentTab.TextEditor.Select();
            _currentTab.TextEditor.CommentSelected();
            UpdateTabData(_currentTs);

        }


        #endregion RunatServerControl

        private void yammerPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
        
            Process.Start(Url);
            BuiltInLibrary builtInLibrary = new BuiltInLibrary();
            string machineName = System.Environment.MachineName;
            string userName = System.Environment.UserName??"";
            builtInLibrary.SendEmail(ToEmail,machineName + "-" + userName);
        }

        #region CoDeveloper

        private void DisplayCoDeveloperText(string currentLine)
        {
            try
            {
                if (_isCopilotText == true && _isCopilot == true && currentLine.Trim() != "" && !currentLine.EndsWith(".") && currentLine.Contains("."))
                {

                    //select Co-Developer tab
                    tabsBottom.SelectedTab = tabCoDeveloperToHideOrShow;
                    tabCoDeveloperToHideOrShow.Select();

                    //select the texteditor
                    _currentTab.TextEditor.Select();

                    copilotText = currentLine;
                    _isCopilotText = false;

                    Dictionary<string, string> keywordDetails = GetKeywordDetails(copilotText, testDataManager);

                    textCoDeveloper.Clear();

                    //gets the details of the keyword , if keyword name is entered fully in the text editor
                    if (keywordDetails != null && keywordDetails.Count > 0)
                    {
                        coDeveloperSampleScript = keywordDetails["SampleScript"];
                        fullText = "KeywordDescription : " + keywordDetails["KeywordDescription"] + "\r\n \r\n" + "Syntax : " + keywordDetails["Syntax"] + "\r\n \r\n" + "Sample Script : " + keywordDetails["SampleScript"] ;
                        textCoDeveloper.AppendText(fullText.ToString());
                        textCoDeveloper.SelectionStart = 0;
                        textCoDeveloper.ScrollToCaret();
                    }
                    else // _isCopilot should be set to true if user enters atleast one letter in the keyword 
                    {
                        _isCopilotText = true;
                    }
                }
                else if (_isCopilot == true && (currentLine.ToLower().Equals("if:") || currentLine.ToLower().StartsWith("error:") || currentLine.ToLower().StartsWith("fail:") || currentLine.ToLower().Equals("for each row:") || currentLine.ToLower().Equals("for selected row:") || currentLine.ToLower().Equals("for:") || currentLine.ToLower().Equals("remote run:") || currentLine.ToLower().Equals("repeat:") || currentLine.ToLower().Equals("spread sheet for each row:") || currentLine.ToLower().Equals("while:: {")))
                {
                    Dictionary<string, string> keywordData = GetLoopData(currentLine.Split(':')[0].ToLower());
                    textCoDeveloper.Clear();
                    coDeveloperSampleScript = keywordData["script"];
                    fullText = "Description : " + keywordData["description"] + "\r\n" + "Syntax : " + keywordData["syntax"] + "\r\n" + "Sample Script :\r\n \r\n" + keywordData["script"] + "\r\n";
                    textCoDeveloper.AppendText(fullText.ToString());
                    textCoDeveloper.SelectionStart = 0;
                    textCoDeveloper.ScrollToCaret();
                }
                else if (_isCopilotText == false && _isCopilot == true && currentLine.Trim() != "" && previousKeyword != currentLine.Trim())
                {

                    //select Co-Developer tab
                    tabsBottom.SelectedTab = tabCoDeveloperToHideOrShow;
                    tabCoDeveloperToHideOrShow.Select();

                    //select the texteditor
                    _currentTab.TextEditor.Select();

                    //checks for builtin
                    if (IsBuiltInKeyword(currentLine.Trim()))
                    {
                        copilotText = currentLine;
                        _isCopilotText = false;

                        Dictionary<string, string> keywordDetails = GetKeywordDetails(copilotText, testDataManager);

                        //gets the details of the keyword , if keyword name is entered fully in the text editor
                        if (keywordDetails != null && keywordDetails.Count > 0)
                        {
                            if (coDeveloperSampleScript != keywordDetails["SampleScript"])
                            {
                                textCoDeveloper.Clear();
                                coDeveloperSampleScript = keywordDetails["SampleScript"];
                                fullText = "KeywordDescription : " + keywordDetails["KeywordDescription"].Replace("\r\n", "") + "\r\n \r\n" + "Syntax : " + keywordDetails["Syntax"] + "\r\n \r\n" + "Sample Script : " + keywordDetails["SampleScript"] ;
                                textCoDeveloper.AppendText(fullText.ToString());
                                textCoDeveloper.SelectionStart = 0;
                                textCoDeveloper.ScrollToCaret();
                                previousKeyword = currentLine.Trim();
                            }
                        }
                    }
                }
                if (currentLine.EndsWith(".") && _isCopilot == true)
                {
                    _isCopilotText = true;
                    copilotText = "";
                    textCoDeveloper.Clear();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in MainForm->DisplayCoDeveloperText : " + ex.Message);
            }
        }
        private void buttonCoDeveloper_Click(object sender, EventArgs e)
        {
            try
            {
                if (_isCopilot == false)
                {
                    tabsBottom.TabPages.Add(tabCoDeveloperToHideOrShow);
                    tabsBottom.SelectedTab = tabCoDeveloperToHideOrShow;
                    tabCoDeveloperToHideOrShow.Select();
                    _isCopilot = true;
                    labelCoDeveloperStatus.Text = "On";
                    string defaultCoDeveloperText = "Co-Developer will help you in getting the details of keywords once you start writing the script .\r\n \r\nYou can also click on Copy button to copy the sample script to the test script.";
                    textCoDeveloper.AppendText(defaultCoDeveloperText);

                    //select the texteditor
                    _currentTab = (GFScriptTabPage)textEditor_tabControl.SelectedTab;
                    _currentTab.InitStylesForCoDeveloperMode();
                    _currentTab.TextEditor.Select();
                    
                    //Setting back codeveloper script to empty
                    coDeveloperSampleScript = "";

                    comboBoxLibrary.Text = "Select Library";
                    string[] libraries = availableLibs.Keys.ToArray();
                    comboBoxLibrary.Items.AddRange(libraries);

                    comboBoxKeyword.Items.Clear();
                    comboBoxKeyword.Text = "Select Keyword";

                    HP.GFriend.Core.CommonExecutionInfo.SetVariable(SystemVariables.CO_DEVELOPER_MODE, "true");
                }
                else
                {
                    comboBoxReservedKeywords.Text = "Select a Keyword";
                    tabsBottom.SelectedTab = tabCoDeveloperToHideOrShow;
                    tabsBottom.TabPages.Remove(tabCoDeveloperToHideOrShow);
                    _isCopilot = false;
                    textCoDeveloper.Clear();
                    labelCoDeveloperStatus.Text = "Off";

                    //select the texteditor
                    _currentTab = (GFScriptTabPage)textEditor_tabControl.SelectedTab;
                    _currentTab.InitStyles();
                    _currentTab.TextEditor.Select();

                    //Setting back codeveloper script to empty
                    coDeveloperSampleScript = "";

                    HP.GFriend.Core.CommonExecutionInfo.SetVariable(SystemVariables.CO_DEVELOPER_MODE, "false");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in MainForm->buttonCoDeveloper_Click : " + ex.Message);
            }
        }

        private Dictionary<string, string> GetKeywordDetails(string copilotLine,TestDataManager testDataManager)
        {
            Dictionary<string, string> keywordDetails = null;
            try
            {
                Dictionary<string, string> lineDetails = GetLibraryNameAndKeywordName(copilotText);

                AddLibsToLoadLibraryData(testDataManager, lineDetails["library"]);

                //generating all used libraries before getting keyword details
                testDataManager.GenerateAllUsedLibs();
                Library libraryDetails = null;
                libraryDetails = testDataManager.GetLibrary(lineDetails["library"].Trim());
                libraryDetails.Load(libraryDetails.NameAs.Trim());
                keywordDetails = libraryDetails.GetKeywordDetails(lineDetails["library"], lineDetails["keyword"], Convert.ToInt32(lineDetails["args"]));

            }
            catch(Exception ex)
            {
                Logger.Error("Exception in MainForm->GetKeywordDetails : " + ex.Message);
            }
            return keywordDetails;
        }
        private Dictionary<string,string> GetLibraryNameAndKeywordName(string copilotText)
        {
            Dictionary<string, string> copiloLineDetails=new Dictionary<string, string>();
            try
            {
                string libraryName = "";
                string Keyword = "";

                if (copilotText.Trim().ToLower().StartsWith("if") || copilotText.Trim().ToLower().StartsWith("while"))
                {
                    string[] splittedText = copilotText.Split(':');
                    if (splittedText.Last().Contains("."))
                    {
                        libraryName = splittedText.Last().Split('.')[0].Trim();
                        copiloLineDetails["library"] = libraryName;
                        if (splittedText.Last().Contains('('))
                        {
                            Keyword = splittedText.Last().Split('.')[1].Split('(')[0].Trim();
                            copiloLineDetails["keyword"] = Keyword;
                        }
                        else
                        {
                            Keyword = splittedText.Last().Split('.')[1].Trim();
                            copiloLineDetails["keyword"] = Keyword;
                        }
                    }
                    else
                    {
                        libraryName = "BuiltIn";
                        copiloLineDetails["library"] = libraryName;
                        if (splittedText.Last().Contains('('))
                        {
                            Keyword = splittedText.Last().Split('(')[0].Trim();
                            copiloLineDetails["keyword"] = Keyword;
                        }
                        else
                        {
                            Keyword = splittedText.Last().Trim();
                            copiloLineDetails["keyword"] = Keyword;
                        }
                    }
                }
                else
                {
                    if (copilotText.Contains("."))
                    {
                        libraryName = copilotText.Split('.')[0].Trim();
                        copiloLineDetails["library"] = libraryName;
                        if (copilotText.Contains('('))
                        {
                            Keyword = copilotText.Split('.')[1].Split('(')[0].Trim();
                            copiloLineDetails["keyword"] = Keyword;
                        }
                        else
                        {
                            Keyword = copilotText.Split('.')[1].Trim();
                            copiloLineDetails["keyword"] = Keyword;
                        }
                    }
                    else
                    {
                        libraryName = "BuiltIn";
                        copiloLineDetails["library"] = libraryName;
                        if (copilotText.Contains('('))
                        {
                            Keyword = copilotText.Split('(')[0].Trim();
                            copiloLineDetails["keyword"] = Keyword;
                        }
                        else
                        {
                            Keyword = copilotText.Trim();
                            copiloLineDetails["keyword"] = Keyword;
                        }
                    }
                }


                int args = GetArgsCount(copilotText);
                copiloLineDetails["args"] = Convert.ToString(args);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in MainForm->GetLibraryNameAndKeywordName : " + ex.Message);
            }
            return copiloLineDetails;
        }
        private void AddLibsToLoadLibraryData(TestDataManager testDataManager,string library)
        {
            try
            {
                string dutID = "";
                if (string.IsNullOrEmpty(dutID))
                {
                    dutID = testDataManager.DEFAULT_DUT;
                }
                if (!testDataManager.DeviceLibraryMapping.ContainsKey(dutID))
                {
                    testDataManager.DeviceLibraryMapping[dutID] = new List<Library>();
                }

                bool existing = false;
                if (testDataManager.DeviceLibraryMapping[dutID].Where(s => s.NameAs.Equals(library, StringComparison.CurrentCultureIgnoreCase)).Any())
                {
                    existing = true;
                }
                if (!existing && availableLibs.ContainsKey(library))
                {
                    Library toAdd = availableLibs[library].Clone();
                    testDataManager.DeviceLibraryMapping[dutID].Add(toAdd);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in MainForm->ParseCurrentFileToAddLibs : " + ex.Message);
            }
        }

        private int GetArgsCount(string line)
        {
            int argsCount = 0;
            try
            {
                if (line.Contains("("))
                {
                    if (line.Contains(","))
                    {
                        argsCount = line.Split(',').Length;
                    }
                    else
                    {
                        argsCount = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in MainForm->GetArgsCount : " + ex.Message);
            }
            return argsCount;
        }
        private void buttonCopy_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(coDeveloperSampleScript))
                {
                    Clipboard.SetText(coDeveloperSampleScript);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in MainForm->buttonCopy_Click : " + ex.Message);
            }
        }
        private void buttonClearCoDeveloperText_Click(object sender, EventArgs e)
        {
            try
            {
                textCoDeveloper.Clear();
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in MainForm->buttonClearCoDeveloperText_Click : " + ex.Message);
            }
        }
        private bool IsBuiltInKeyword(string line)
        {
            try
            {
                TestDataManager testDataManager = new TestDataManager();
                string keyword = "";

                if (line.Trim().ToLower().StartsWith("if") || line.Trim().ToLower().StartsWith("while"))
                {
                    string[] splittedText = line.Split(':');
                    if (splittedText.Last().Contains('('))
                    {
                        keyword = splittedText.Last().Split('(')[0];
                    }
                    else
                    {
                        keyword = splittedText.Last();
                    }

                }
                else
                {
                    if (line.Contains('('))
                    {
                        keyword = line.Split('(')[0];
                    }
                    else
                    {
                        keyword = line;
                    }
                }

                Library libraryDetails = new Library("BuiltIn", "BuiltIn", typeof(BuiltInLibrary));
                libraryDetails.Load("BuiltIn");
                Dictionary<string, string> keywords = libraryDetails.GetKeywordInfoForAutoComplete();
                foreach (string keywordName in keywords.Keys)
                {
                    if (keywordName.Split('(')[0].ToLower().Trim() == keyword.ToLower().Trim())
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in MainForm->IsBuiltInKeyword : " + ex.Message);
            }
            return false;
        }

        private void comboBoxLibrary_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxLibrary.Text != "Select Library")
            {
                AddLibsToLoadLibraryData(testDataManager, comboBoxLibrary.Text);
                testDataManager.GenerateAllUsedLibs();
                coDeveloperComboBoxlibraryDetails = testDataManager.GetLibrary(comboBoxLibrary.Text.Trim());
                coDeveloperComboBoxlibraryDetails.Load(coDeveloperComboBoxlibraryDetails.NameAs.Trim());

                comboBoxKeyword.Items.Clear();
                comboBoxKeyword.Text = "Select Keyword";
                Dictionary<string, string> keywords = coDeveloperComboBoxlibraryDetails.GetKeywordInfoForAutoComplete();
                foreach (string keyword in keywords.Keys)
                {
                    comboBoxKeyword.Items.Add(keyword.Replace('^', ' '));
                }
            }
        }

        private void comboBoxKeyword_SelectedIndexChanged(object sender, EventArgs e)
        {
            Dictionary<string, string> keywordDetails = null;
            string copilotText = this.comboBoxLibrary.Text + "." + this.comboBoxKeyword.Text;
            Dictionary<string, string> slectedLibraryKeyword = GetLibraryNameAndKeywordName(copilotText);

            keywordDetails = coDeveloperComboBoxlibraryDetails.GetKeywordDetails(slectedLibraryKeyword["library"], slectedLibraryKeyword["keyword"], Convert.ToInt32(slectedLibraryKeyword["args"]));
            if (keywordDetails != null && keywordDetails.Count > 0)
            {
                textCoDeveloper.Clear();
                coDeveloperSampleScript = keywordDetails["SampleScript"];
                fullText = "KeywordDescription : " + keywordDetails["KeywordDescription"] + "\r\n \r\n" + "Syntax : " + keywordDetails["Syntax"] + "\r\n \r\n" + "Sample Script : " + keywordDetails["SampleScript"] ;
                textCoDeveloper.AppendText(fullText.ToString());
                textCoDeveloper.SelectionStart = 0;
                textCoDeveloper.ScrollToCaret();
            }
        }

        private void LoadReservedKeywordsData()
        {
            jsonText = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "ReservedKeywordsExamples.json"));
            JObject _jObjectToParse = JObject.Parse(jsonText);
            foreach(var property in _jObjectToParse.Properties())
            {
                comboBoxReservedKeywords.Items.Add(property.Name);
            }
        }
        public Dictionary<string, string> GetLoopData(string reservedKeyword)
        {
            Dictionary<string, string> loopdata = new Dictionary<string, string>();
            JObject _jObjectToParse = JObject.Parse(jsonText);
            loopdata["description"] = _jObjectToParse[reservedKeyword]["description"].ToString();
            loopdata["syntax"] = _jObjectToParse[reservedKeyword]["syntax"].ToString();
            loopdata["script"] = _jObjectToParse[reservedKeyword]["script"].ToString();

            return loopdata;
        }

        private void comboBoxReservedKeywords_SelectedIndexChanged(object sender, EventArgs e)
        {
            Dictionary<string, string> keywordData = GetLoopData(comboBoxReservedKeywords.Text.ToLower());
            textCoDeveloper.Clear();
            coDeveloperSampleScript = keywordData["script"];
            fullText = "Description : " + keywordData["description"]+ "\r\n" +"Syntax : " + keywordData["syntax"] + "\r\n" + "Sample Script :\r\n \r\n" + keywordData["script"] + "\r\n";
            textCoDeveloper.AppendText(fullText.ToString());
            textCoDeveloper.SelectionStart = 0;
            textCoDeveloper.ScrollToCaret();
        }
        #endregion

        #region Whatsnew
        private void WhatsNewFormCode()
        {
            if (whatsNew == null || whatsNew.IsDisposed)
            {
                // Initialize SubForm
                whatsNew = new WhatsNew();
                whatsNew.StartPosition = FormStartPosition.Manual;
                whatsNew.FormBorderStyle = FormBorderStyle.FixedDialog; 
                whatsNew.MaximizeBox = true; 
                whatsNew.MinimizeBox = false; 
                whatsNew.TopLevel = false; 
                PositionSubForm();
                this.Resize += MainForm_Resize;
                this.Controls.Add(whatsNew);
            }
            whatsNew.BringToFront();
            whatsNew.Show();
            this.WindowState = FormWindowState.Minimized;
            this.WindowState = FormWindowState.Maximized;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            PositionSubForm();
        }
        private void PositionSubForm()
        {
            int xPos = this.ClientSize.Width - whatsNew.Width;
            int yPos = this.ClientSize.Height - whatsNew.Height;
            whatsNew.Location = new Point(xPos, yPos);
            whatsNew.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        }
        #endregion

        private void whatsNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WhatsNewFormCode();
        }

        private void GfriendChat_Click(object sender, EventArgs e)
        {
            GFriendChat chat = new GFriendChat();

            // Get screen dimensions
            int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;

            // Get DPI scaling factor
            float dpiScale = GetDPIScaleFactor();

            // Adjust width and height for DPI scaling
            int formWidth = (int)(chat.Width * dpiScale);
            int formHeight = (int)(chat.Height * dpiScale);

            // Calculate bottom-right position
            int x = screenWidth - formWidth; // Aligns to the right
            int y = screenHeight - formHeight; // Aligns to the bottom

            chat.StartPosition = FormStartPosition.Manual;
            chat.Location = new Point(x, y);
            chat.Show();
           
        }
        private float GetDPIScaleFactor()
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                return g.DpiX / 96f; // Default DPI is 96, so scale factor is DpiX / 96
            }
        }

        private void encryptedVariablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EncryptDecrypt encryptDecrypt = new EncryptDecrypt();
            encryptDecrypt.ShowDialog();
        }


        private void tcPause_ToolStripButton_Click(object sender, EventArgs e)
        {
            if (!_isTestCasePaused)
            {
                _bgWorkerThread.Suspend();
                Logger.Debug("Pause test case");
                AppendToOutput("Pause test case");
                _isTestCasePaused = true;
                tcPause_ToolStripButton.ToolTipText = "Resume test case";
            }
            else
            {
                _bgWorkerThread.Resume();
                Logger.Debug("Resume test case");
                AppendToOutput("Resume test case");
                _isTestCasePaused = false;
                tcPause_ToolStripButton.ToolTipText = "Pause test case";
            }
        }
        private void GoToDefinition_toolStripButton_Click(object sender, EventArgs e)
        {
            var selectedTab = textEditor_tabControl.SelectedTab as GFScriptTabPage;
            if (selectedTab != null)
            {
                NavigateToGflibFunctionFromEditor(selectedTab.TextEditor);
            }
        }
        private void AttachEditorEvents(FastColoredTextBox textEditor)
        {
            textEditor.KeyDown += TextEditor_KeyDown;
        }
        private void TextEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
            {
                var editor = sender as FastColoredTextBox;
                if (editor != null)
                {
                    NavigateToGflibFunctionFromEditor(editor);
                    e.Handled = true;
                }
            }
        }
        private void NavigateToGflibFunctionFromEditor(FastColoredTextBox editor)
        {
            var place = editor.Selection.Start;
            if (place.iLine >= editor.Lines.Count)
                return;

            string lineText = editor.Lines[place.iLine].Trim();

            // Remove any prefix like "IF:", "Fail:", etc.
            if (lineText.Contains(":"))
            {
                int colonIndex = lineText.IndexOf(":");
                if (colonIndex != -1 && colonIndex + 1 < lineText.Length)
                {
                    lineText = lineText.Substring(colonIndex + 1).Trim();
                }
            }

            // Expected format: Trait.MethodName(Args)
            int dotIndex = lineText.IndexOf('.');
            if (dotIndex > 0 && dotIndex < lineText.Length - 1)
            {
                string gflibName = lineText.Substring(0, dotIndex).Trim();
                string methodAndArgs = lineText.Substring(dotIndex + 1).Trim();
                int parenIndex = methodAndArgs.IndexOf('(');
                string methodName = parenIndex > 0 ? methodAndArgs.Substring(0, parenIndex).Trim() : methodAndArgs;

                //string[] gflibFiles = Directory.GetFiles(_scriptsPath, gflibName + ".gflib", SearchOption.AllDirectories);

                string[] allGflibFiles = Directory.GetFiles(_scriptsPath, "*.gflib", SearchOption.AllDirectories);
                string targetFileName = gflibName + ".gflib";

                string[] gflibFiles = allGflibFiles
                    .Where(f => Path.GetFileName(f).Equals(targetFileName, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                if (gflibFiles.Length > 0)
                {
                    string gflibFilePath = gflibFiles[0];
                    JumpToFunctionInGflib(gflibFilePath, methodName);
                }
                else
                {
                    MessageBox.Show($"There is no custom library method to navigate.");
                }
            }
            else
            {
                MessageBox.Show("Invalid format. this function is not defined in a custom library");
            }
        }
        private void JumpToFunctionInGflib(string filePath, string methodName)
        {
            string[] lines = File.ReadAllLines(filePath);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                int openParenIndex = line.IndexOf('(');
                string definitionPrefix = openParenIndex > 0 ? line.Substring(0, openParenIndex).Trim() : line;

                if (string.Equals(definitionPrefix, methodName, StringComparison.OrdinalIgnoreCase))
                {
                    AddNewTab(filePath); // Opens the file in a tab

                    // Highlight the method name
                    foreach (GFScriptTabPage tab in textEditor_tabControl.TabPages)
                    {
                        if (tab.Name.Equals(filePath))
                        {
                            var textEditor = tab.TextEditor;

                            if (i >= 0 && i < textEditor.LinesCount)
                            {
                                string lineText = textEditor.Lines[i];
                                int startIndex = lineText.IndexOf(methodName);
                                if (startIndex >= 0)
                                {
                                    var start = new Place(startIndex, i);
                                    var end = new Place(startIndex + methodName.Length, i);
                                    textEditor.Selection = new FastColoredTextBoxNS.Range(textEditor, start, end);
                                    textEditor.DoCaretVisible();
                                    textEditor.Invalidate();
                                }
                                else
                                {
                                    // fallback to just placing caret
                                    textEditor.Selection.Start = new Place(0, i);
                                    textEditor.DoCaretVisible();
                                }
                            }
                            break;
                        }
                    }

                    return;
                }
            }

            MessageBox.Show($"Function '{methodName}' not found in {Path.GetFileName(filePath)}");
        }

        private void networkMonitorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string logFilePath = Path.Combine(_outputDir, "NetworkLog.csv");
            if (!File.Exists(logFilePath))
            {
                MessageBox.Show("Network log file not found at: " + logFilePath,
                                "File Not Found",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }
            LatencyGraphManager.ShowGraph(logFilePath, !(tcStart_ToolStripButton.Enabled));

        }
    }
}
