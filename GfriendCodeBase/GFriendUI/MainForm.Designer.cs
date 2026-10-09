using HP.GFriend.UI.Controls;

namespace HP.GFriend.UI
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.deviceAddress_label = new System.Windows.Forms.Label();
            this.deviceAddress_Caption_label = new System.Windows.Forms.Label();
            this.deviceList_button = new System.Windows.Forms.Button();
            this.targetDevicePlaceholder = new System.Windows.Forms.Label();
            this.deviceId_caption_label = new System.Windows.Forms.Label();
            this.menuMain = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gotoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runTestToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopTestToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.getPositionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.externalTool_ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.deviceListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.repositoryBrowserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runTestAtServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.encryptedVariablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.keywordListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manualToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.whatsNewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.networkMonitorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.yammerPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTipDescriptions = new System.Windows.Forms.ToolTip(this.components);
            this.tabsBottom = new System.Windows.Forms.TabControl();
            this.tabOutput = new System.Windows.Forms.TabPage();
            this.GfriendChat = new System.Windows.Forms.Button();
            this.clearTextOutput_button = new System.Windows.Forms.Button();
            this.textOutput = new HP.GFriend.UI.Controls.GFOutputBox();
            this.tabRun = new System.Windows.Forms.TabPage();
            this.gfInstantRun = new HP.GFriend.UI.Controls.GFInstantRun();
            this.tabCoDeveloper = new System.Windows.Forms.TabPage();
            this.textCoDeveloper = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.comboBoxReservedKeywords = new System.Windows.Forms.ComboBox();
            this.labelReservedKeywords = new System.Windows.Forms.Label();
            this.comboBoxKeyword = new System.Windows.Forms.ComboBox();
            this.buttonClearCoDeveloperText = new System.Windows.Forms.Button();
            this.labelKeyword = new System.Windows.Forms.Label();
            this.buttonCopy = new System.Windows.Forms.Button();
            this.labelLibrary = new System.Windows.Forms.Label();
            this.comboBoxLibrary = new System.Windows.Forms.ComboBox();
            this.buttonCoDeveloper = new System.Windows.Forms.Button();
            this.bgWorkerRunTest = new System.ComponentModel.BackgroundWorker();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.label2 = new System.Windows.Forms.Label();
            this.TargetDevice_panel = new System.Windows.Forms.Panel();
            this.paperless_panel = new System.Windows.Forms.Panel();
            this.labelCoDeveloperStatus = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.paperless_on_button = new System.Windows.Forms.Button();
            this.paperless_off_button = new System.Windows.Forms.Button();
            this.RunatTestServer_panel = new System.Windows.Forms.Panel();
            this.RunatTestServer_toolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_scriptRepository = new System.Windows.Forms.ToolStripButton();
            this.runatTestServer_toolStripButton = new System.Windows.Forms.ToolStripButton();
            this.showTestResult_toolStripButton = new System.Windows.Forms.ToolStripButton();
            this.label6 = new System.Windows.Forms.Label();
            this.splitUpDown = new System.Windows.Forms.SplitContainer();
            this.splitLeftRight = new System.Windows.Forms.SplitContainer();
            this.filelist_panel = new System.Windows.Forms.Panel();
            this.gfScriptFileList = new HP.GFriend.UI.Controls.GFScriptFileList();
            this.testSuites_toolStrip = new System.Windows.Forms.ToolStrip();
            this.openFolder_toolStripButton = new System.Windows.Forms.ToolStripButton();
            this.refresh_toolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.tsStart_toolStripButton = new System.Windows.Forms.ToolStripButton();
            this.tsStop_toolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonGit = new System.Windows.Forms.ToolStripButton();
            this.testByTestSuites_panel = new System.Windows.Forms.Panel();
            this.testByTestSuites_label = new System.Windows.Forms.Label();
            this.filelist_dataGridView = new System.Windows.Forms.DataGridView();
            this.testbyTestcases_splitContainer = new System.Windows.Forms.SplitContainer();
            this.textEditor_tabControl = new System.Windows.Forms.TabControl();
            this.toolStripToolBar = new System.Windows.Forms.ToolStrip();
            this.newFile_toolStripButton = new System.Windows.Forms.ToolStripButton();
            this.openFile_toolStripButton = new System.Windows.Forms.ToolStripButton();
            this.save_toolStripButton = new System.Windows.Forms.ToolStripButton();
            this.saveAs_toolStripButton = new System.Windows.Forms.ToolStripButton();
            this.comment_toolStripButton = new System.Windows.Forms.ToolStripButton();
            this.goToDefinition_toolStripButton = new System.Windows.Forms.ToolStripButton();
            this.TestCases_panel = new System.Windows.Forms.Panel();
            this.testCases_listView = new System.Windows.Forms.ListView();
            this.headerChkbox = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.headerLine = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.headerTCName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TestCases_toolStrip = new System.Windows.Forms.ToolStrip();
            this.tcStart_ToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.tcPause_ToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.tcStop_ToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.selectAll_toolStripButton = new System.Windows.Forms.ToolStripButton();
            this.testbyTestCases_panel = new System.Windows.Forms.Panel();
            this.testbyTestCases_label = new System.Windows.Forms.Label();
            this.menuMain.SuspendLayout();
            this.tabsBottom.SuspendLayout();
            this.tabOutput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textOutput)).BeginInit();
            this.tabRun.SuspendLayout();
            this.tabCoDeveloper.SuspendLayout();
            this.panel1.SuspendLayout();
            this.TargetDevice_panel.SuspendLayout();
            this.paperless_panel.SuspendLayout();
            this.RunatTestServer_panel.SuspendLayout();
            this.RunatTestServer_toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitUpDown)).BeginInit();
            this.splitUpDown.Panel1.SuspendLayout();
            this.splitUpDown.Panel2.SuspendLayout();
            this.splitUpDown.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitLeftRight)).BeginInit();
            this.splitLeftRight.Panel1.SuspendLayout();
            this.splitLeftRight.Panel2.SuspendLayout();
            this.splitLeftRight.SuspendLayout();
            this.filelist_panel.SuspendLayout();
            this.testSuites_toolStrip.SuspendLayout();
            this.testByTestSuites_panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.filelist_dataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.testbyTestcases_splitContainer)).BeginInit();
            this.testbyTestcases_splitContainer.Panel1.SuspendLayout();
            this.testbyTestcases_splitContainer.Panel2.SuspendLayout();
            this.testbyTestcases_splitContainer.SuspendLayout();
            this.toolStripToolBar.SuspendLayout();
            this.TestCases_panel.SuspendLayout();
            this.TestCases_toolStrip.SuspendLayout();
            this.testbyTestCases_panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // deviceAddress_label
            // 
            this.deviceAddress_label.AutoSize = true;
            this.deviceAddress_label.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.deviceAddress_label.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.deviceAddress_label.Location = new System.Drawing.Point(463, 8);
            this.deviceAddress_label.Name = "deviceAddress_label";
            this.deviceAddress_label.Size = new System.Drawing.Size(35, 13);
            this.deviceAddress_label.TabIndex = 14;
            this.deviceAddress_label.Text = "none";
            // 
            // deviceAddress_Caption_label
            // 
            this.deviceAddress_Caption_label.AutoSize = true;
            this.deviceAddress_Caption_label.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.deviceAddress_Caption_label.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.deviceAddress_Caption_label.Location = new System.Drawing.Point(335, 8);
            this.deviceAddress_Caption_label.Name = "deviceAddress_Caption_label";
            this.deviceAddress_Caption_label.Size = new System.Drawing.Size(112, 13);
            this.deviceAddress_Caption_label.TabIndex = 13;
            this.deviceAddress_Caption_label.Text = "Device Address:";
            // 
            // deviceList_button
            // 
            this.deviceList_button.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.deviceList_button.FlatAppearance.BorderSize = 0;
            this.deviceList_button.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.deviceList_button.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.deviceList_button.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.deviceList_button.Image = global::HP.GFriend.UI.Properties.Resources.ListViewTable_16x;
            this.deviceList_button.Location = new System.Drawing.Point(299, 3);
            this.deviceList_button.Name = "deviceList_button";
            this.deviceList_button.Size = new System.Drawing.Size(26, 23);
            this.deviceList_button.TabIndex = 12;
            this.deviceList_button.UseVisualStyleBackColor = true;
            this.deviceList_button.Click += new System.EventHandler(this.deviceList_button_Click);
            // 
            // targetDevicePlaceholder
            // 
            this.targetDevicePlaceholder.AutoSize = false;
            this.targetDevicePlaceholder.BackColor = System.Drawing.Color.White;
            this.targetDevicePlaceholder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.targetDevicePlaceholder.ForeColor = System.Drawing.Color.Gray;
            this.targetDevicePlaceholder.Location = new System.Drawing.Point(131, 8);
            this.targetDevicePlaceholder.Name = "targetDevicePlaceholder";
            this.targetDevicePlaceholder.Size = new System.Drawing.Size(150, 20);
            this.targetDevicePlaceholder.TabIndex = 11;
            this.targetDevicePlaceholder.Text = "Select Target";
            this.targetDevicePlaceholder.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.targetDevicePlaceholder.Padding = new System.Windows.Forms.Padding(2, 0, 0, 0);
            this.targetDevicePlaceholder.Cursor = System.Windows.Forms.Cursors.Hand;
            this.targetDevicePlaceholder.Click += new System.EventHandler(this.targetDevicePlaceholder_Click);
            // 
            // deviceId_caption_label
            // 
            this.deviceId_caption_label.AutoSize = true;
            this.deviceId_caption_label.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.deviceId_caption_label.Location = new System.Drawing.Point(21, 60);
            this.deviceId_caption_label.Name = "deviceId_caption_label";
            this.deviceId_caption_label.Size = new System.Drawing.Size(165, 14);
            this.deviceId_caption_label.TabIndex = 4;
            this.deviceId_caption_label.Text = "Test Target (Device ID)";
            // 
            // menuMain
            // 
            this.menuMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.menuMain.ImageScalingSize = new System.Drawing.Size(20, 16);
            this.menuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.toolToolStripMenuItem,
            this.helpToolStripMenuItem,
            this.yammerPageToolStripMenuItem});
            this.menuMain.Location = new System.Drawing.Point(0, 0);
            this.menuMain.Name = "menuMain";
            this.menuMain.Size = new System.Drawing.Size(1408, 24);
            this.menuMain.TabIndex = 2;
            this.menuMain.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.newToolStripMenuItem.Text = "&New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.saveToolStripMenuItem.Text = "&Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.saveAsToolStripMenuItem.Text = "Save &As";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.findToolStripMenuItem,
            this.replaceToolStripMenuItem,
            this.gotoToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+Z";
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.undoToolStripMenuItem.Text = "&Undo";
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.undoToolStripMenuItem_Click);
            // 
            // findToolStripMenuItem
            // 
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            this.findToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+F";
            this.findToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.findToolStripMenuItem.Text = "&Find";
            this.findToolStripMenuItem.Click += new System.EventHandler(this.findToolStripMenuItem_Click);
            // 
            // replaceToolStripMenuItem
            // 
            this.replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            this.replaceToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+H";
            this.replaceToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.replaceToolStripMenuItem.Text = "&Replace";
            this.replaceToolStripMenuItem.Click += new System.EventHandler(this.replaceToolStripMenuItem_Click);
            // 
            // gotoToolStripMenuItem
            // 
            this.gotoToolStripMenuItem.Name = "gotoToolStripMenuItem";
            this.gotoToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+G";
            this.gotoToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.gotoToolStripMenuItem.Text = "&Goto";
            this.gotoToolStripMenuItem.Click += new System.EventHandler(this.gotoToolStripMenuItem_Click);
            // 
            // toolToolStripMenuItem
            // 
            this.toolToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runTestToolStripMenuItem,
            this.stopTestToolStripMenuItem,
            this.getPositionToolStripMenuItem,
            this.externalTool_ToolStripMenuItem,
            this.toolStripSeparator3,
            this.deviceListToolStripMenuItem,
            this.repositoryBrowserToolStripMenuItem,
            this.runTestAtServerToolStripMenuItem,
            this.toolStripSeparator2,
            this.toolStripMenuItem1,
            this.optionsToolStripMenuItem,
            this.encryptedVariablesToolStripMenuItem});
            this.toolToolStripMenuItem.Name = "toolToolStripMenuItem";
            this.toolToolStripMenuItem.Size = new System.Drawing.Size(42, 20);
            this.toolToolStripMenuItem.Text = "&Tool";
            // 
            // runTestToolStripMenuItem
            // 
            this.runTestToolStripMenuItem.Name = "runTestToolStripMenuItem";
            this.runTestToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.runTestToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.runTestToolStripMenuItem.Text = "&Run Test";
            this.runTestToolStripMenuItem.Click += new System.EventHandler(this.runTestToolStripMenuItem_Click);
            // 
            // stopTestToolStripMenuItem
            // 
            this.stopTestToolStripMenuItem.Name = "stopTestToolStripMenuItem";
            this.stopTestToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.stopTestToolStripMenuItem.Text = "&Stop Test";
            this.stopTestToolStripMenuItem.Click += new System.EventHandler(this.stopTestToolStripMenuItem_Click);
            // 
            // getPositionToolStripMenuItem
            // 
            this.getPositionToolStripMenuItem.Enabled = false;
            this.getPositionToolStripMenuItem.Name = "getPositionToolStripMenuItem";
            this.getPositionToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.getPositionToolStripMenuItem.Text = "Get Touch Position";
            this.getPositionToolStripMenuItem.Click += new System.EventHandler(this.getPositionToolStripMenuItem_Click);
            // 
            // externalTool_ToolStripMenuItem
            // 
            this.externalTool_ToolStripMenuItem.Name = "externalTool_ToolStripMenuItem";
            this.externalTool_ToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.externalTool_ToolStripMenuItem.Text = "&External Tools";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(195, 6);
            // 
            // deviceListToolStripMenuItem
            // 
            this.deviceListToolStripMenuItem.Name = "deviceListToolStripMenuItem";
            this.deviceListToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.deviceListToolStripMenuItem.Text = "Device list";
            this.deviceListToolStripMenuItem.Click += new System.EventHandler(this.deviceListToolStripMenuItem_Click);
            // 
            // repositoryBrowserToolStripMenuItem
            // 
            this.repositoryBrowserToolStripMenuItem.Name = "repositoryBrowserToolStripMenuItem";
            this.repositoryBrowserToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.repositoryBrowserToolStripMenuItem.Text = "Script Repository Server";
            this.repositoryBrowserToolStripMenuItem.Visible = false;
            this.repositoryBrowserToolStripMenuItem.Click += new System.EventHandler(this.ScriptRepositoryServerToolStripMenuItem_Click);
            // 
            // runTestAtServerToolStripMenuItem
            // 
            this.runTestAtServerToolStripMenuItem.Name = "runTestAtServerToolStripMenuItem";
            this.runTestAtServerToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.runTestAtServerToolStripMenuItem.Text = "Run Test at Server";
            this.runTestAtServerToolStripMenuItem.Visible = false;
            this.runTestAtServerToolStripMenuItem.Click += new System.EventHandler(this.runTestAtServerToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(195, 6);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(198, 22);
            this.toolStripMenuItem1.Text = "Test Run Spec Wizard";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.TestRunSpecWizardToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.optionsToolStripMenuItem.Text = "Options";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.OptionsToolStripMenuItem_Click);
            // 
            // encryptedVariablesToolStripMenuItem
            // 
            this.encryptedVariablesToolStripMenuItem.Name = "encryptedVariablesToolStripMenuItem";
            this.encryptedVariablesToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.encryptedVariablesToolStripMenuItem.Text = "Encrypted Variables";
            this.encryptedVariablesToolStripMenuItem.Click += new System.EventHandler(this.encryptedVariablesToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.keywordListToolStripMenuItem,
            this.manualToolStripMenuItem,
            this.aboutToolStripMenuItem,
            this.whatsNewToolStripMenuItem,
            this.networkMonitorToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // keywordListToolStripMenuItem
            // 
            this.keywordListToolStripMenuItem.Name = "keywordListToolStripMenuItem";
            this.keywordListToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.K)));
            this.keywordListToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.keywordListToolStripMenuItem.Text = "&Keyword List";
            // 
            // manualToolStripMenuItem
            // 
            this.manualToolStripMenuItem.Name = "manualToolStripMenuItem";
            this.manualToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F1)));
            this.manualToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.manualToolStripMenuItem.Text = "Manual";
            this.manualToolStripMenuItem.Click += new System.EventHandler(this.manualToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // whatsNewToolStripMenuItem
            // 
            this.whatsNewToolStripMenuItem.Name = "whatsNewToolStripMenuItem";
            this.whatsNewToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.whatsNewToolStripMenuItem.Text = "What\'s New";
            this.whatsNewToolStripMenuItem.Click += new System.EventHandler(this.whatsNewToolStripMenuItem_Click);
            // 
            // networkMonitorToolStripMenuItem
            // 
            this.networkMonitorToolStripMenuItem.Name = "networkMonitorToolStripMenuItem";
            this.networkMonitorToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.networkMonitorToolStripMenuItem.Text = "NetworkMonitor";
            this.networkMonitorToolStripMenuItem.Click += new System.EventHandler(this.networkMonitorToolStripMenuItem_Click);
            // 
            // yammerPageToolStripMenuItem
            // 
            this.yammerPageToolStripMenuItem.Image = global::HP.GFriend.UI.Properties.Resources.VivaEngageIcon;
            this.yammerPageToolStripMenuItem.Name = "yammerPageToolStripMenuItem";
            this.yammerPageToolStripMenuItem.Size = new System.Drawing.Size(32, 20);
            this.yammerPageToolStripMenuItem.Click += new System.EventHandler(this.yammerPageToolStripMenuItem_Click);
            // 
            // tabsBottom
            // 
            this.tabsBottom.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabsBottom.Controls.Add(this.tabOutput);
            this.tabsBottom.Controls.Add(this.tabRun);
            this.tabsBottom.Controls.Add(this.tabCoDeveloper);
            this.tabsBottom.Location = new System.Drawing.Point(4, 27);
            this.tabsBottom.Name = "tabsBottom";
            this.tabsBottom.SelectedIndex = 0;
            this.tabsBottom.Size = new System.Drawing.Size(1394, 139);
            this.tabsBottom.TabIndex = 0;
            this.toolTipDescriptions.SetToolTip(this.tabsBottom, "Clear all the text in the Output layer");
            // 
            // tabOutput
            // 
            this.tabOutput.Controls.Add(this.GfriendChat);
            this.tabOutput.Controls.Add(this.clearTextOutput_button);
            this.tabOutput.Controls.Add(this.textOutput);
            this.tabOutput.Location = new System.Drawing.Point(4, 23);
            this.tabOutput.Name = "tabOutput";
            this.tabOutput.Padding = new System.Windows.Forms.Padding(3);
            this.tabOutput.Size = new System.Drawing.Size(1386, 112);
            this.tabOutput.TabIndex = 0;
            this.tabOutput.Text = "Output";
            this.tabOutput.UseVisualStyleBackColor = true;
            // 
            // GfriendChat
            // 
            this.GfriendChat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.GfriendChat.BackColor = System.Drawing.Color.White;
            this.GfriendChat.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.GfriendChat.FlatAppearance.BorderSize = 0;
            this.GfriendChat.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.GfriendChat.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.GfriendChat.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GfriendChat.Image = ((System.Drawing.Image)(resources.GetObject("GfriendChat.Image")));
            this.GfriendChat.Location = new System.Drawing.Point(1339, 1);
            this.GfriendChat.Name = "GfriendChat";
            this.GfriendChat.Size = new System.Drawing.Size(44, 43);
            this.GfriendChat.TabIndex = 21;
            this.toolTipDescriptions.SetToolTip(this.GfriendChat, "Ask Gfriend");
            this.GfriendChat.UseVisualStyleBackColor = false;
            this.GfriendChat.Click += new System.EventHandler(this.GfriendChat_Click);
            // 
            // clearTextOutput_button
            // 
            this.clearTextOutput_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.clearTextOutput_button.BackColor = System.Drawing.Color.White;
            this.clearTextOutput_button.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.clearTextOutput_button.FlatAppearance.BorderSize = 0;
            this.clearTextOutput_button.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.clearTextOutput_button.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.clearTextOutput_button.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.clearTextOutput_button.Image = global::HP.GFriend.UI.Properties.Resources.ClearText_16x;
            this.clearTextOutput_button.Location = new System.Drawing.Point(1357, 50);
            this.clearTextOutput_button.Name = "clearTextOutput_button";
            this.clearTextOutput_button.Size = new System.Drawing.Size(26, 23);
            this.clearTextOutput_button.TabIndex = 20;
            this.toolTipDescriptions.SetToolTip(this.clearTextOutput_button, "Clear all the text in the Output layer");
            this.clearTextOutput_button.UseVisualStyleBackColor = false;
            this.clearTextOutput_button.Click += new System.EventHandler(this.clearTextOutput_button_Click);
            // 
            // textOutput
            // 
            this.textOutput.AllowDrop = false;
            this.textOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textOutput.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
            this.textOutput.AutoIndent = false;
            this.textOutput.AutoIndentChars = false;
            this.textOutput.AutoIndentCharsPatterns = "";
            this.textOutput.AutoIndentExistingLines = false;
            this.textOutput.AutoScrollMargin = new System.Drawing.Size(1, 1);
            this.textOutput.AutoScrollMinSize = new System.Drawing.Size(0, 14);
            this.textOutput.BackBrush = null;
            this.textOutput.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.textOutput.CharHeight = 14;
            this.textOutput.CharWidth = 8;
            this.textOutput.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textOutput.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.textOutput.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textOutput.IsReplaceMode = false;
            this.textOutput.Location = new System.Drawing.Point(0, 0);
            this.textOutput.Name = "textOutput";
            this.textOutput.Paddings = new System.Windows.Forms.Padding(0);
            this.textOutput.ReadOnly = true;
            this.textOutput.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.textOutput.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("textOutput.ServiceColors")));
            this.textOutput.ShowLineNumbers = false;
            this.textOutput.Size = new System.Drawing.Size(1386, 44);
            this.textOutput.TabIndex = 0;
            this.textOutput.WordWrap = true;
            this.textOutput.Zoom = 100;
            // 
            // tabRun
            // 
            this.tabRun.Controls.Add(this.gfInstantRun);
            this.tabRun.Location = new System.Drawing.Point(4, 22);
            this.tabRun.Name = "tabRun";
            this.tabRun.Padding = new System.Windows.Forms.Padding(3);
            this.tabRun.Size = new System.Drawing.Size(1386, 113);
            this.tabRun.TabIndex = 1;
            this.tabRun.Text = "Run";
            this.tabRun.UseVisualStyleBackColor = true;
            // 
            // gfInstantRun
            // 
            this.gfInstantRun.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gfInstantRun.Location = new System.Drawing.Point(3, 3);
            this.gfInstantRun.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gfInstantRun.Name = "gfInstantRun";
            this.gfInstantRun.Size = new System.Drawing.Size(1380, 107);
            this.gfInstantRun.TabIndex = 20;
            // 
            // tabCoDeveloper
            // 
            this.tabCoDeveloper.Controls.Add(this.textCoDeveloper);
            this.tabCoDeveloper.Controls.Add(this.panel1);
            this.tabCoDeveloper.Location = new System.Drawing.Point(4, 22);
            this.tabCoDeveloper.Name = "tabCoDeveloper";
            this.tabCoDeveloper.Size = new System.Drawing.Size(1386, 113);
            this.tabCoDeveloper.TabIndex = 2;
            this.tabCoDeveloper.Text = "Co-Developer";
            this.tabCoDeveloper.UseVisualStyleBackColor = true;
            // 
            // textCoDeveloper
            // 
            this.textCoDeveloper.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textCoDeveloper.Font = new System.Drawing.Font("Courier New", 9.75F);
            this.textCoDeveloper.Location = new System.Drawing.Point(0, 38);
            this.textCoDeveloper.Multiline = true;
            this.textCoDeveloper.Name = "textCoDeveloper";
            this.textCoDeveloper.ReadOnly = true;
            this.textCoDeveloper.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textCoDeveloper.Size = new System.Drawing.Size(1383, 49);
            this.textCoDeveloper.TabIndex = 25;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(75)))), ((int)(((byte)(139)))));
            this.panel1.Controls.Add(this.comboBoxReservedKeywords);
            this.panel1.Controls.Add(this.labelReservedKeywords);
            this.panel1.Controls.Add(this.comboBoxKeyword);
            this.panel1.Controls.Add(this.buttonClearCoDeveloperText);
            this.panel1.Controls.Add(this.labelKeyword);
            this.panel1.Controls.Add(this.buttonCopy);
            this.panel1.Controls.Add(this.labelLibrary);
            this.panel1.Controls.Add(this.comboBoxLibrary);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1386, 38);
            this.panel1.TabIndex = 24;
            // 
            // comboBoxReservedKeywords
            // 
            this.comboBoxReservedKeywords.FormattingEnabled = true;
            this.comboBoxReservedKeywords.Location = new System.Drawing.Point(923, 4);
            this.comboBoxReservedKeywords.Name = "comboBoxReservedKeywords";
            this.comboBoxReservedKeywords.Size = new System.Drawing.Size(187, 22);
            this.comboBoxReservedKeywords.TabIndex = 25;
            this.comboBoxReservedKeywords.Text = "Select a Keyword";
            this.comboBoxReservedKeywords.SelectedIndexChanged += new System.EventHandler(this.comboBoxReservedKeywords_SelectedIndexChanged);
            // 
            // labelReservedKeywords
            // 
            this.labelReservedKeywords.AutoSize = true;
            this.labelReservedKeywords.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Bold);
            this.labelReservedKeywords.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.labelReservedKeywords.Location = new System.Drawing.Point(752, 9);
            this.labelReservedKeywords.Name = "labelReservedKeywords";
            this.labelReservedKeywords.Size = new System.Drawing.Size(168, 13);
            this.labelReservedKeywords.TabIndex = 24;
            this.labelReservedKeywords.Text = "Reserved GFriend Words";
            // 
            // comboBoxKeyword
            // 
            this.comboBoxKeyword.FormattingEnabled = true;
            this.comboBoxKeyword.Location = new System.Drawing.Point(416, 6);
            this.comboBoxKeyword.Name = "comboBoxKeyword";
            this.comboBoxKeyword.Size = new System.Drawing.Size(187, 22);
            this.comboBoxKeyword.TabIndex = 3;
            this.comboBoxKeyword.SelectedIndexChanged += new System.EventHandler(this.comboBoxKeyword_SelectedIndexChanged);
            // 
            // buttonClearCoDeveloperText
            // 
            this.buttonClearCoDeveloperText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClearCoDeveloperText.BackColor = System.Drawing.Color.White;
            this.buttonClearCoDeveloperText.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.buttonClearCoDeveloperText.FlatAppearance.BorderSize = 0;
            this.buttonClearCoDeveloperText.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.buttonClearCoDeveloperText.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.buttonClearCoDeveloperText.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonClearCoDeveloperText.Image = global::HP.GFriend.UI.Properties.Resources.ClearText_16x;
            this.buttonClearCoDeveloperText.Location = new System.Drawing.Point(1352, 6);
            this.buttonClearCoDeveloperText.Name = "buttonClearCoDeveloperText";
            this.buttonClearCoDeveloperText.Size = new System.Drawing.Size(26, 23);
            this.buttonClearCoDeveloperText.TabIndex = 23;
            this.toolTipDescriptions.SetToolTip(this.buttonClearCoDeveloperText, "Clears all the text");
            this.buttonClearCoDeveloperText.UseVisualStyleBackColor = false;
            this.buttonClearCoDeveloperText.Click += new System.EventHandler(this.buttonClearCoDeveloperText_Click);
            // 
            // labelKeyword
            // 
            this.labelKeyword.AutoSize = true;
            this.labelKeyword.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Bold);
            this.labelKeyword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.labelKeyword.Location = new System.Drawing.Point(325, 11);
            this.labelKeyword.Name = "labelKeyword";
            this.labelKeyword.Size = new System.Drawing.Size(63, 13);
            this.labelKeyword.TabIndex = 2;
            this.labelKeyword.Text = "Keyword";
            // 
            // buttonCopy
            // 
            this.buttonCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCopy.Image = ((System.Drawing.Image)(resources.GetObject("buttonCopy.Image")));
            this.buttonCopy.Location = new System.Drawing.Point(1310, 7);
            this.buttonCopy.Name = "buttonCopy";
            this.buttonCopy.Size = new System.Drawing.Size(30, 23);
            this.buttonCopy.TabIndex = 22;
            this.toolTipDescriptions.SetToolTip(this.buttonCopy, "Samplescript will be copied to clipboard . You can paste it into your test script" +
        " .");
            this.buttonCopy.UseVisualStyleBackColor = true;
            this.buttonCopy.Click += new System.EventHandler(this.buttonCopy_Click);
            // 
            // labelLibrary
            // 
            this.labelLibrary.AutoSize = true;
            this.labelLibrary.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Bold);
            this.labelLibrary.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.labelLibrary.Location = new System.Drawing.Point(25, 9);
            this.labelLibrary.Name = "labelLibrary";
            this.labelLibrary.Size = new System.Drawing.Size(54, 13);
            this.labelLibrary.TabIndex = 1;
            this.labelLibrary.Text = "Library";
            // 
            // comboBoxLibrary
            // 
            this.comboBoxLibrary.FormattingEnabled = true;
            this.comboBoxLibrary.Location = new System.Drawing.Point(91, 6);
            this.comboBoxLibrary.Name = "comboBoxLibrary";
            this.comboBoxLibrary.Size = new System.Drawing.Size(187, 22);
            this.comboBoxLibrary.TabIndex = 0;
            this.comboBoxLibrary.SelectedIndexChanged += new System.EventHandler(this.comboBoxLibrary_SelectedIndexChanged);
            // 
            // buttonCoDeveloper
            // 
            this.buttonCoDeveloper.Image = global::HP.GFriend.UI.Properties.Resources.Co_Developer;
            this.buttonCoDeveloper.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonCoDeveloper.Location = new System.Drawing.Point(464, 3);
            this.buttonCoDeveloper.Name = "buttonCoDeveloper";
            this.buttonCoDeveloper.Size = new System.Drawing.Size(182, 26);
            this.buttonCoDeveloper.TabIndex = 17;
            this.buttonCoDeveloper.Text = "Co-Developer mode";
            this.toolTipDescriptions.SetToolTip(this.buttonCoDeveloper, "This will help you in writing test cases easier");
            this.buttonCoDeveloper.UseVisualStyleBackColor = true;
            this.buttonCoDeveloper.Click += new System.EventHandler(this.buttonCoDeveloper_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.label2.Location = new System.Drawing.Point(11, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(114, 17);
            this.label2.TabIndex = 0;
            this.label2.Text = "Target Devices";
            // 
            // TargetDevice_panel
            // 
            this.TargetDevice_panel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(75)))), ((int)(((byte)(139)))));
            this.TargetDevice_panel.Controls.Add(this.paperless_panel);
            this.TargetDevice_panel.Controls.Add(this.label2);
            this.TargetDevice_panel.Controls.Add(this.targetDevicePlaceholder);
            this.TargetDevice_panel.Controls.Add(this.deviceAddress_label);
            this.TargetDevice_panel.Controls.Add(this.deviceId_caption_label);
            this.TargetDevice_panel.Controls.Add(this.deviceList_button);
            this.TargetDevice_panel.Controls.Add(this.deviceAddress_Caption_label);
            this.TargetDevice_panel.Dock = System.Windows.Forms.DockStyle.Top;
            this.TargetDevice_panel.Location = new System.Drawing.Point(0, 24);
            this.TargetDevice_panel.Name = "TargetDevice_panel";
            this.TargetDevice_panel.Size = new System.Drawing.Size(1408, 31);
            this.TargetDevice_panel.TabIndex = 1;
            // 
            // paperless_panel
            // 
            this.paperless_panel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(75)))), ((int)(((byte)(139)))));
            this.paperless_panel.Controls.Add(this.labelCoDeveloperStatus);
            this.paperless_panel.Controls.Add(this.buttonCoDeveloper);
            this.paperless_panel.Controls.Add(this.label5);
            this.paperless_panel.Controls.Add(this.paperless_on_button);
            this.paperless_panel.Controls.Add(this.paperless_off_button);
            this.paperless_panel.Location = new System.Drawing.Point(561, 0);
            this.paperless_panel.Name = "paperless_panel";
            this.paperless_panel.Size = new System.Drawing.Size(716, 31);
            this.paperless_panel.TabIndex = 3;
            // 
            // labelCoDeveloperStatus
            // 
            this.labelCoDeveloperStatus.AutoSize = true;
            this.labelCoDeveloperStatus.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Bold);
            this.labelCoDeveloperStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.labelCoDeveloperStatus.Location = new System.Drawing.Point(654, 10);
            this.labelCoDeveloperStatus.Name = "labelCoDeveloperStatus";
            this.labelCoDeveloperStatus.Size = new System.Drawing.Size(26, 13);
            this.labelCoDeveloperStatus.TabIndex = 18;
            this.labelCoDeveloperStatus.Text = "Off";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.label5.Location = new System.Drawing.Point(12, 8);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(115, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Paperless mode:";
            // 
            // paperless_on_button
            // 
            this.paperless_on_button.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.paperless_on_button.Location = new System.Drawing.Point(133, 4);
            this.paperless_on_button.Name = "paperless_on_button";
            this.paperless_on_button.Size = new System.Drawing.Size(40, 23);
            this.paperless_on_button.TabIndex = 15;
            this.paperless_on_button.Text = "On";
            this.paperless_on_button.UseVisualStyleBackColor = true;
            this.paperless_on_button.Click += new System.EventHandler(this.paperless_on_button_Click);
            // 
            // paperless_off_button
            // 
            this.paperless_off_button.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.paperless_off_button.Location = new System.Drawing.Point(179, 4);
            this.paperless_off_button.Name = "paperless_off_button";
            this.paperless_off_button.Size = new System.Drawing.Size(40, 23);
            this.paperless_off_button.TabIndex = 16;
            this.paperless_off_button.Text = "Off";
            this.paperless_off_button.UseVisualStyleBackColor = true;
            this.paperless_off_button.Click += new System.EventHandler(this.paperless_off_button_Click);
            // 
            // RunatTestServer_panel
            // 
            this.RunatTestServer_panel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RunatTestServer_panel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(75)))), ((int)(((byte)(139)))));
            this.RunatTestServer_panel.Controls.Add(this.RunatTestServer_toolStrip);
            this.RunatTestServer_panel.Controls.Add(this.label6);
            this.RunatTestServer_panel.Location = new System.Drawing.Point(1253, 26);
            this.RunatTestServer_panel.Name = "RunatTestServer_panel";
            this.RunatTestServer_panel.Size = new System.Drawing.Size(208, 31);
            this.RunatTestServer_panel.TabIndex = 17;
            this.RunatTestServer_panel.Visible = false;
            // 
            // RunatTestServer_toolStrip
            // 
            this.RunatTestServer_toolStrip.BackColor = System.Drawing.Color.Transparent;
            this.RunatTestServer_toolStrip.CanOverflow = false;
            this.RunatTestServer_toolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.RunatTestServer_toolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.RunatTestServer_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_scriptRepository,
            this.runatTestServer_toolStripButton,
            this.showTestResult_toolStripButton});
            this.RunatTestServer_toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.RunatTestServer_toolStrip.Location = new System.Drawing.Point(125, 4);
            this.RunatTestServer_toolStrip.Name = "RunatTestServer_toolStrip";
            this.RunatTestServer_toolStrip.Size = new System.Drawing.Size(73, 27);
            this.RunatTestServer_toolStrip.TabIndex = 16;
            this.RunatTestServer_toolStrip.Text = "Run at Test Server";
            // 
            // toolStripButton_scriptRepository
            // 
            this.toolStripButton_scriptRepository.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_scriptRepository.Image = global::HP.GFriend.UI.Properties.Resources.ServerReport_16x;
            this.toolStripButton_scriptRepository.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_scriptRepository.Name = "toolStripButton_scriptRepository";
            this.toolStripButton_scriptRepository.Size = new System.Drawing.Size(24, 24);
            this.toolStripButton_scriptRepository.Text = "Script Repository Server";
            this.toolStripButton_scriptRepository.Click += new System.EventHandler(this.ScriptRepositoryServerToolStripMenuItem_Click);
            // 
            // runatTestServer_toolStripButton
            // 
            this.runatTestServer_toolStripButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.runatTestServer_toolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.runatTestServer_toolStripButton.Image = global::HP.GFriend.UI.Properties.Resources.ServerRunTest_16x;
            this.runatTestServer_toolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.runatTestServer_toolStripButton.Name = "runatTestServer_toolStripButton";
            this.runatTestServer_toolStripButton.Size = new System.Drawing.Size(24, 24);
            this.runatTestServer_toolStripButton.Text = "Run at Test Server";
            this.runatTestServer_toolStripButton.Click += new System.EventHandler(this.runatTestServer_toolStripButton_Click);
            // 
            // showTestResult_toolStripButton
            // 
            this.showTestResult_toolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.showTestResult_toolStripButton.Image = global::HP.GFriend.UI.Properties.Resources.WebServer_16x;
            this.showTestResult_toolStripButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.showTestResult_toolStripButton.Name = "showTestResult_toolStripButton";
            this.showTestResult_toolStripButton.Size = new System.Drawing.Size(24, 24);
            this.showTestResult_toolStripButton.Text = "Show Test Result from Server";
            this.showTestResult_toolStripButton.Click += new System.EventHandler(this.showTestResult_toolStripButton_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.label6.Location = new System.Drawing.Point(12, 8);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(102, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "Server Control";
            // 
            // splitUpDown
            // 
            this.splitUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitUpDown.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitUpDown.Location = new System.Drawing.Point(0, 61);
            this.splitUpDown.Name = "splitUpDown";
            this.splitUpDown.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitUpDown.Panel1
            // 
            this.splitUpDown.Panel1.Controls.Add(this.splitLeftRight);
            // 
            // splitUpDown.Panel2
            // 
            this.splitUpDown.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.splitUpDown.Panel2.Controls.Add(this.tabsBottom);
            this.splitUpDown.Size = new System.Drawing.Size(1408, 523);
            this.splitUpDown.SplitterDistance = 359;
            this.splitUpDown.TabIndex = 1;
            // 
            // splitLeftRight
            // 
            this.splitLeftRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitLeftRight.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitLeftRight.Location = new System.Drawing.Point(0, 0);
            this.splitLeftRight.Name = "splitLeftRight";
            // 
            // splitLeftRight.Panel1
            // 
            this.splitLeftRight.Panel1.Controls.Add(this.filelist_panel);
            // 
            // splitLeftRight.Panel2
            // 
            this.splitLeftRight.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.splitLeftRight.Panel2.Controls.Add(this.testbyTestcases_splitContainer);
            this.splitLeftRight.Panel2.Controls.Add(this.testbyTestCases_panel);
            this.splitLeftRight.Size = new System.Drawing.Size(1404, 355);
            this.splitLeftRight.SplitterDistance = 324;
            this.splitLeftRight.TabIndex = 0;
            // 
            // filelist_panel
            // 
            this.filelist_panel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filelist_panel.Controls.Add(this.gfScriptFileList);
            this.filelist_panel.Controls.Add(this.testSuites_toolStrip);
            this.filelist_panel.Controls.Add(this.testByTestSuites_panel);
            this.filelist_panel.Controls.Add(this.filelist_dataGridView);
            this.filelist_panel.Location = new System.Drawing.Point(0, 0);
            this.filelist_panel.Name = "filelist_panel";
            this.filelist_panel.Size = new System.Drawing.Size(323, 351);
            this.filelist_panel.TabIndex = 17;
            // 
            // gfScriptFileList
            // 
            this.gfScriptFileList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gfScriptFileList.Location = new System.Drawing.Point(4, 55);
            this.gfScriptFileList.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gfScriptFileList.Name = "gfScriptFileList";
            this.gfScriptFileList.Size = new System.Drawing.Size(319, 132);
            this.gfScriptFileList.TabIndex = 19;
            this.gfScriptFileList.OnFolderLoad += new System.EventHandler<System.Windows.Forms.TreeViewEventArgs>(this.FolderTreeView_OnFolderLoad);
            this.gfScriptFileList.OnUpdateSelectedFile += new System.EventHandler<System.Windows.Forms.TreeViewEventArgs>(this.FolderTreeView_OnUpdateSelectedFile);
            this.gfScriptFileList.OnAppendToOutput += new System.EventHandler<HP.GFriend.UI.Controls.GFGeneralEventArgs>(this.FolderTreeView_OnAppendOutput);
            this.gfScriptFileList.OnOpenFile += new System.EventHandler<HP.GFriend.UI.Controls.GFGeneralEventArgs>(this.FolderTreeView_OnOpenFile);
            this.gfScriptFileList.OnLoadFolderException += new System.EventHandler<HP.GFriend.UI.Controls.GFGeneralEventArgs>(this.FolderTreeView_OnLoadFolderException);
            // 
            // testSuites_toolStrip
            // 
            this.testSuites_toolStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.testSuites_toolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.testSuites_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openFolder_toolStripButton,
            this.refresh_toolStripButton,
            this.toolStripSeparator4,
            this.tsStart_toolStripButton,
            this.tsStop_toolStripButton,
            this.toolStripButtonGit});
            this.testSuites_toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.testSuites_toolStrip.Location = new System.Drawing.Point(0, 29);
            this.testSuites_toolStrip.Name = "testSuites_toolStrip";
            this.testSuites_toolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.testSuites_toolStrip.Size = new System.Drawing.Size(323, 27);
            this.testSuites_toolStrip.TabIndex = 17;
            this.testSuites_toolStrip.Text = "toolStrip1";
            // 
            // openFolder_toolStripButton
            // 
            this.openFolder_toolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.openFolder_toolStripButton.Image = global::HP.GFriend.UI.Properties.Resources.OpenFolder_16x;
            this.openFolder_toolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openFolder_toolStripButton.Name = "openFolder_toolStripButton";
            this.openFolder_toolStripButton.Size = new System.Drawing.Size(24, 24);
            this.openFolder_toolStripButton.Text = "Open Folder";
            this.openFolder_toolStripButton.Click += new System.EventHandler(this.openFolder_toolStripButton_Click);
            // 
            // refresh_toolStripButton
            // 
            this.refresh_toolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.refresh_toolStripButton.Image = global::HP.GFriend.UI.Properties.Resources.Refresh_16x;
            this.refresh_toolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.refresh_toolStripButton.Name = "refresh_toolStripButton";
            this.refresh_toolStripButton.Size = new System.Drawing.Size(24, 24);
            this.refresh_toolStripButton.Text = "Refresh";
            this.refresh_toolStripButton.Click += new System.EventHandler(this.refresh_toolStripButton_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 23);
            // 
            // tsStart_toolStripButton
            // 
            this.tsStart_toolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsStart_toolStripButton.Image = global::HP.GFriend.UI.Properties.Resources.Run_16x;
            this.tsStart_toolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsStart_toolStripButton.Name = "tsStart_toolStripButton";
            this.tsStart_toolStripButton.Size = new System.Drawing.Size(24, 24);
            this.tsStart_toolStripButton.Text = "Run Test Suites";
            this.tsStart_toolStripButton.Click += new System.EventHandler(this.tsStart_toolStripButton_Click);
            // 
            // tsStop_toolStripButton
            // 
            this.tsStop_toolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsStop_toolStripButton.Enabled = false;
            this.tsStop_toolStripButton.Image = global::HP.GFriend.UI.Properties.Resources.stop;
            this.tsStop_toolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsStop_toolStripButton.Name = "tsStop_toolStripButton";
            this.tsStop_toolStripButton.Size = new System.Drawing.Size(24, 24);
            this.tsStop_toolStripButton.Text = "toolStripButton1";
            this.tsStop_toolStripButton.ToolTipText = "Stop Test";
            this.tsStop_toolStripButton.Click += new System.EventHandler(this.tsStop_toolStripButton_Click);
            // 
            // toolStripButtonGit
            // 
            this.toolStripButtonGit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonGit.Image = global::HP.GFriend.UI.Properties.Resources.gitIcon;
            this.toolStripButtonGit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonGit.Name = "toolStripButtonGit";
            this.toolStripButtonGit.Size = new System.Drawing.Size(24, 24);
            this.toolStripButtonGit.Text = "toolStripButton1";
            this.toolStripButtonGit.ToolTipText = "Git Version Control";
            this.toolStripButtonGit.Visible = false;
            this.toolStripButtonGit.Click += new System.EventHandler(this.toolStripButtonGit_Click);
            // 
            // testByTestSuites_panel
            // 
            this.testByTestSuites_panel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(192)))));
            this.testByTestSuites_panel.Controls.Add(this.testByTestSuites_label);
            this.testByTestSuites_panel.Dock = System.Windows.Forms.DockStyle.Top;
            this.testByTestSuites_panel.Location = new System.Drawing.Point(0, 0);
            this.testByTestSuites_panel.Name = "testByTestSuites_panel";
            this.testByTestSuites_panel.Size = new System.Drawing.Size(323, 29);
            this.testByTestSuites_panel.TabIndex = 18;
            // 
            // testByTestSuites_label
            // 
            this.testByTestSuites_label.AutoSize = true;
            this.testByTestSuites_label.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.testByTestSuites_label.Location = new System.Drawing.Point(5, 6);
            this.testByTestSuites_label.Name = "testByTestSuites_label";
            this.testByTestSuites_label.Size = new System.Drawing.Size(155, 17);
            this.testByTestSuites_label.TabIndex = 0;
            this.testByTestSuites_label.Text = "Test by Test Suites";
            // 
            // filelist_dataGridView
            // 
            this.filelist_dataGridView.AllowUserToAddRows = false;
            this.filelist_dataGridView.AllowUserToDeleteRows = false;
            this.filelist_dataGridView.AllowUserToResizeRows = false;
            this.filelist_dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filelist_dataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.filelist_dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.filelist_dataGridView.Location = new System.Drawing.Point(0, 193);
            this.filelist_dataGridView.Name = "filelist_dataGridView";
            this.filelist_dataGridView.ReadOnly = true;
            this.filelist_dataGridView.RowHeadersVisible = false;
            this.filelist_dataGridView.RowHeadersWidth = 51;
            this.filelist_dataGridView.RowTemplate.Height = 18;
            this.filelist_dataGridView.RowTemplate.ReadOnly = true;
            this.filelist_dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.filelist_dataGridView.Size = new System.Drawing.Size(322, 151);
            this.filelist_dataGridView.TabIndex = 16;
            this.filelist_dataGridView.Resize += new System.EventHandler(this.Filelist_dataGridView_Resize);
            // 
            // testbyTestcases_splitContainer
            // 
            this.testbyTestcases_splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.testbyTestcases_splitContainer.Location = new System.Drawing.Point(22, 38);
            this.testbyTestcases_splitContainer.Name = "testbyTestcases_splitContainer";
            // 
            // testbyTestcases_splitContainer.Panel1
            // 
            this.testbyTestcases_splitContainer.Panel1.Controls.Add(this.textEditor_tabControl);
            this.testbyTestcases_splitContainer.Panel1.Controls.Add(this.toolStripToolBar);
            // 
            // testbyTestcases_splitContainer.Panel2
            // 
            this.testbyTestcases_splitContainer.Panel2.Controls.Add(this.TestCases_panel);
            this.testbyTestcases_splitContainer.Size = new System.Drawing.Size(1056, 317);
            this.testbyTestcases_splitContainer.SplitterDistance = 819;
            this.testbyTestcases_splitContainer.SplitterWidth = 1;
            this.testbyTestcases_splitContainer.TabIndex = 20;
            // 
            // textEditor_tabControl
            // 
            this.textEditor_tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textEditor_tabControl.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.textEditor_tabControl.Location = new System.Drawing.Point(24, 0);
            this.textEditor_tabControl.Name = "textEditor_tabControl";
            this.textEditor_tabControl.Padding = new System.Drawing.Point(12, 4);
            this.textEditor_tabControl.SelectedIndex = 0;
            this.textEditor_tabControl.Size = new System.Drawing.Size(796, 317);
            this.textEditor_tabControl.TabIndex = 12;
            this.textEditor_tabControl.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.textEditor_tabControl_DrawItem);
            this.textEditor_tabControl.SelectedIndexChanged += new System.EventHandler(this.textEditor_tabControl_SelectedIndexChanged);
            this.textEditor_tabControl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.textEditor_tabControl_MouseDown);
            this.textEditor_tabControl.MouseUp += new System.Windows.Forms.MouseEventHandler(this.textEditor_tabControl_MouseUp);
            // 
            // toolStripToolBar
            // 
            this.toolStripToolBar.Dock = System.Windows.Forms.DockStyle.Left;
            this.toolStripToolBar.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStripToolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newFile_toolStripButton,
            this.openFile_toolStripButton,
            this.save_toolStripButton,
            this.saveAs_toolStripButton,
            this.comment_toolStripButton,
            this.goToDefinition_toolStripButton});
            this.toolStripToolBar.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Table;
            this.toolStripToolBar.Location = new System.Drawing.Point(0, 0);
            this.toolStripToolBar.Name = "toolStripToolBar";
            this.toolStripToolBar.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripToolBar.Size = new System.Drawing.Size(25, 317);
            this.toolStripToolBar.TabIndex = 3;
            this.toolStripToolBar.Text = "toolStrip1";
            // 
            // newFile_toolStripButton
            // 
            this.newFile_toolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.newFile_toolStripButton.Image = global::HP.GFriend.UI.Properties.Resources.NewFile_16x;
            this.newFile_toolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newFile_toolStripButton.Name = "newFile_toolStripButton";
            this.newFile_toolStripButton.Size = new System.Drawing.Size(24, 24);
            this.newFile_toolStripButton.Text = "New File";
            this.newFile_toolStripButton.Click += new System.EventHandler(this.toolStripButtonNewFile_Click);
            // 
            // openFile_toolStripButton
            // 
            this.openFile_toolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.openFile_toolStripButton.Image = global::HP.GFriend.UI.Properties.Resources.OpenFile_16x;
            this.openFile_toolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openFile_toolStripButton.Name = "openFile_toolStripButton";
            this.openFile_toolStripButton.Size = new System.Drawing.Size(24, 24);
            this.openFile_toolStripButton.Text = "Open File";
            this.openFile_toolStripButton.Click += new System.EventHandler(this.openFile_toolStripButton_Click);
            // 
            // save_toolStripButton
            // 
            this.save_toolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.save_toolStripButton.Image = global::HP.GFriend.UI.Properties.Resources.Save_16x;
            this.save_toolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.save_toolStripButton.Name = "save_toolStripButton";
            this.save_toolStripButton.Size = new System.Drawing.Size(24, 24);
            this.save_toolStripButton.Text = "Save";
            this.save_toolStripButton.Click += new System.EventHandler(this.save_toolStripButton_Click);
            // 
            // saveAs_toolStripButton
            // 
            this.saveAs_toolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveAs_toolStripButton.Image = global::HP.GFriend.UI.Properties.Resources.SaveAs_16x;
            this.saveAs_toolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveAs_toolStripButton.Name = "saveAs_toolStripButton";
            this.saveAs_toolStripButton.Size = new System.Drawing.Size(24, 24);
            this.saveAs_toolStripButton.Text = "Save as";
            this.saveAs_toolStripButton.Click += new System.EventHandler(this.saveAs_toolStripButton_Click);
            // 
            // comment_toolStripButton
            // 
            this.comment_toolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.comment_toolStripButton.Image = global::HP.GFriend.UI.Properties.Resources.Comment;
            this.comment_toolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.comment_toolStripButton.Name = "comment_toolStripButton";
            this.comment_toolStripButton.Size = new System.Drawing.Size(24, 24);
            this.comment_toolStripButton.Text = "Comment";
            this.comment_toolStripButton.ToolTipText = "Comment / Uncomment";
            this.comment_toolStripButton.Click += new System.EventHandler(this.comment_toolStripButton_Click);
            // 
            // goToDefinition_toolStripButton
            // 
            this.goToDefinition_toolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.goToDefinition_toolStripButton.Image = global::HP.GFriend.UI.Properties.Resources.GoToImplementation;
            this.goToDefinition_toolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.goToDefinition_toolStripButton.Name = "goToDefinition_toolStripButton";
            this.goToDefinition_toolStripButton.Size = new System.Drawing.Size(24, 24);
            this.goToDefinition_toolStripButton.Text = "GoToDefinition";
            this.goToDefinition_toolStripButton.ToolTipText = "Go To Implementation";
            this.goToDefinition_toolStripButton.Click += new System.EventHandler(this.GoToDefinition_toolStripButton_Click);
            // 
            // TestCases_panel
            // 
            this.TestCases_panel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TestCases_panel.Controls.Add(this.testCases_listView);
            this.TestCases_panel.Controls.Add(this.TestCases_toolStrip);
            this.TestCases_panel.Location = new System.Drawing.Point(6, 0);
            this.TestCases_panel.Name = "TestCases_panel";
            this.TestCases_panel.Size = new System.Drawing.Size(590, 317);
            this.TestCases_panel.TabIndex = 11;
            // 
            // testCases_listView
            // 
            this.testCases_listView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.testCases_listView.CheckBoxes = true;
            this.testCases_listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.headerChkbox,
            this.headerLine,
            this.headerTCName});
            this.testCases_listView.FullRowSelect = true;
            this.testCases_listView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.testCases_listView.HideSelection = false;
            this.testCases_listView.Location = new System.Drawing.Point(0, 22);
            this.testCases_listView.Name = "testCases_listView";
            this.testCases_listView.Size = new System.Drawing.Size(590, 295);
            this.testCases_listView.TabIndex = 9;
            this.testCases_listView.UseCompatibleStateImageBehavior = false;
            this.testCases_listView.View = System.Windows.Forms.View.Details;
            this.testCases_listView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.testCases_listView_ItemChecked);
            this.testCases_listView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listViewTestCases_MouseDoubleClick);
            // 
            // headerChkbox
            // 
            this.headerChkbox.Text = "";
            this.headerChkbox.Width = 25;
            // 
            // headerLine
            // 
            this.headerLine.Width = 0;
            // 
            // headerTCName
            // 
            this.headerTCName.Text = "Test Cases";
            this.headerTCName.Width = 220;
            // 
            // TestCases_toolStrip
            // 
            this.TestCases_toolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.TestCases_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tcStart_ToolStripButton,
            this.tcPause_ToolStripButton,
            this.tcStop_ToolStripButton,
            this.toolStripSeparator1,
            this.selectAll_toolStripButton});
            this.TestCases_toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.TestCases_toolStrip.Location = new System.Drawing.Point(0, 0);
            this.TestCases_toolStrip.Name = "TestCases_toolStrip";
            this.TestCases_toolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.TestCases_toolStrip.Size = new System.Drawing.Size(590, 27);
            this.TestCases_toolStrip.TabIndex = 10;
            this.TestCases_toolStrip.Text = "toolStrip1";
            // 
            // tcStart_ToolStripButton
            // 
            this.tcStart_ToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tcStart_ToolStripButton.Image = global::HP.GFriend.UI.Properties.Resources.Run_16x;
            this.tcStart_ToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tcStart_ToolStripButton.Name = "tcStart_ToolStripButton";
            this.tcStart_ToolStripButton.Size = new System.Drawing.Size(24, 24);
            this.tcStart_ToolStripButton.Text = "Run Test Cases";
            this.tcStart_ToolStripButton.Click += new System.EventHandler(this.tcStart_ToolStripButton_Click);
            // 
            // tcPause_ToolStripButton
            // 
            this.tcPause_ToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tcPause_ToolStripButton.Image = global::HP.GFriend.UI.Properties.Resources.PauseImage;
            this.tcPause_ToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tcPause_ToolStripButton.Name = "tcPause_ToolStripButton";
            this.tcPause_ToolStripButton.Size = new System.Drawing.Size(24, 24);
            this.tcPause_ToolStripButton.Text = "tcPause_ToolStripButton";
            this.tcPause_ToolStripButton.ToolTipText = "Pause testcase\r\n";
            this.tcPause_ToolStripButton.Click += new System.EventHandler(this.tcPause_ToolStripButton_Click);
            // 
            // tcStop_ToolStripButton
            // 
            this.tcStop_ToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tcStop_ToolStripButton.Enabled = false;
            this.tcStop_ToolStripButton.Image = global::HP.GFriend.UI.Properties.Resources.stop;
            this.tcStop_ToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tcStop_ToolStripButton.Name = "tcStop_ToolStripButton";
            this.tcStop_ToolStripButton.Size = new System.Drawing.Size(24, 24);
            this.tcStop_ToolStripButton.Text = "Stop Test";
            this.tcStop_ToolStripButton.Click += new System.EventHandler(this.tcStop_ToolStripButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 23);
            // 
            // selectAll_toolStripButton
            // 
            this.selectAll_toolStripButton.Image = global::HP.GFriend.UI.Properties.Resources.CheckboxCheckAll_16x;
            this.selectAll_toolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.selectAll_toolStripButton.Name = "selectAll_toolStripButton";
            this.selectAll_toolStripButton.Size = new System.Drawing.Size(79, 24);
            this.selectAll_toolStripButton.Text = "Select All";
            this.selectAll_toolStripButton.Click += new System.EventHandler(this.SelectAll_toolStripButton_Click);
            // 
            // testbyTestCases_panel
            // 
            this.testbyTestCases_panel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(192)))));
            this.testbyTestCases_panel.Controls.Add(this.testbyTestCases_label);
            this.testbyTestCases_panel.Dock = System.Windows.Forms.DockStyle.Top;
            this.testbyTestCases_panel.Location = new System.Drawing.Point(0, 0);
            this.testbyTestCases_panel.Name = "testbyTestCases_panel";
            this.testbyTestCases_panel.Size = new System.Drawing.Size(1076, 29);
            this.testbyTestCases_panel.TabIndex = 19;
            // 
            // testbyTestCases_label
            // 
            this.testbyTestCases_label.AutoSize = true;
            this.testbyTestCases_label.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.testbyTestCases_label.Location = new System.Drawing.Point(5, 6);
            this.testbyTestCases_label.Name = "testbyTestCases_label";
            this.testbyTestCases_label.Size = new System.Drawing.Size(153, 17);
            this.testbyTestCases_label.TabIndex = 0;
            this.testbyTestCases_label.Text = "Test by Test Cases";
            // 
            // encryptedVariablesToolStripMenuItem
            // 
            this.encryptedVariablesToolStripMenuItem.Name = "encryptedVariablesToolStripMenuItem";
            this.encryptedVariablesToolStripMenuItem.Size = new System.Drawing.Size(250, 26);
            this.encryptedVariablesToolStripMenuItem.Text = "Encrypted Variables";
            this.encryptedVariablesToolStripMenuItem.Click += new System.EventHandler(this.encryptedVariablesToolStripMenuItem_Click);
            // 
            // gfScriptFileList
            // 
            this.gfScriptFileList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gfScriptFileList.Location = new System.Drawing.Point(4, 55);
            this.gfScriptFileList.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gfScriptFileList.Name = "gfScriptFileList";
            this.gfScriptFileList.Size = new System.Drawing.Size(319, 132);
            this.gfScriptFileList.TabIndex = 19;
            this.gfScriptFileList.OnFolderLoad += new System.EventHandler<System.Windows.Forms.TreeViewEventArgs>(this.FolderTreeView_OnFolderLoad);
            this.gfScriptFileList.OnUpdateSelectedFile += new System.EventHandler<System.Windows.Forms.TreeViewEventArgs>(this.FolderTreeView_OnUpdateSelectedFile);
            this.gfScriptFileList.OnAppendToOutput += new System.EventHandler<HP.GFriend.UI.Controls.GFGeneralEventArgs>(this.FolderTreeView_OnAppendOutput);
            this.gfScriptFileList.OnOpenFile += new System.EventHandler<HP.GFriend.UI.Controls.GFGeneralEventArgs>(this.FolderTreeView_OnOpenFile);
            this.gfScriptFileList.OnLoadFolderException += new System.EventHandler<HP.GFriend.UI.Controls.GFGeneralEventArgs>(this.FolderTreeView_OnLoadFolderException);
            // 
            // textOutput
            // 
            this.textOutput.AllowDrop = false;
            this.textOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textOutput.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
            this.textOutput.AutoIndent = false;
            this.textOutput.AutoIndentChars = false;
            this.textOutput.AutoIndentCharsPatterns = "";
            this.textOutput.AutoIndentExistingLines = false;
            this.textOutput.AutoScrollMargin = new System.Drawing.Size(1, 1);
            this.textOutput.AutoScrollMinSize = new System.Drawing.Size(0, 18);
            this.textOutput.BackBrush = null;
            this.textOutput.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.textOutput.CharHeight = 18;
            this.textOutput.CharWidth = 10;
            this.textOutput.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textOutput.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.textOutput.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textOutput.IsReplaceMode = false;
            this.textOutput.Location = new System.Drawing.Point(0, 0);
            this.textOutput.Name = "textOutput";
            this.textOutput.Paddings = new System.Windows.Forms.Padding(0);
            this.textOutput.ReadOnly = true;
            this.textOutput.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.textOutput.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("textOutput.ServiceColors")));
            this.textOutput.ShowLineNumbers = false;
            this.textOutput.Size = new System.Drawing.Size(1386, 44);
            this.textOutput.TabIndex = 0;
            this.textOutput.WordWrap = true;
            this.textOutput.Zoom = 100;
            // 
            // gfInstantRun
            // 
            this.gfInstantRun.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gfInstantRun.Location = new System.Drawing.Point(3, 3);
            this.gfInstantRun.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gfInstantRun.Name = "gfInstantRun";
            this.gfInstantRun.Size = new System.Drawing.Size(1380, 104);
            this.gfInstantRun.TabIndex = 20;
            // 

            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1408, 584);
            this.Controls.Add(this.RunatTestServer_panel);
            this.Controls.Add(this.TargetDevice_panel);
            this.Controls.Add(this.splitUpDown);
            this.Controls.Add(this.menuMain);
            this.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuMain;
            this.Name = "MainForm";
            this.Text = "GFriend";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.menuMain.ResumeLayout(false);
            this.menuMain.PerformLayout();
            this.tabsBottom.ResumeLayout(false);
            this.tabOutput.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.textOutput)).EndInit();
            this.tabRun.ResumeLayout(false);
            this.tabCoDeveloper.ResumeLayout(false);
            this.tabCoDeveloper.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.TargetDevice_panel.ResumeLayout(false);
            this.TargetDevice_panel.PerformLayout();
            this.paperless_panel.ResumeLayout(false);
            this.paperless_panel.PerformLayout();
            this.RunatTestServer_panel.ResumeLayout(false);
            this.RunatTestServer_panel.PerformLayout();
            this.RunatTestServer_toolStrip.ResumeLayout(false);
            this.RunatTestServer_toolStrip.PerformLayout();
            this.splitUpDown.Panel1.ResumeLayout(false);
            this.splitUpDown.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitUpDown)).EndInit();
            this.splitUpDown.ResumeLayout(false);
            this.splitLeftRight.Panel1.ResumeLayout(false);
            this.splitLeftRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitLeftRight)).EndInit();
            this.splitLeftRight.ResumeLayout(false);
            this.filelist_panel.ResumeLayout(false);
            this.filelist_panel.PerformLayout();
            this.testSuites_toolStrip.ResumeLayout(false);
            this.testSuites_toolStrip.PerformLayout();
            this.testByTestSuites_panel.ResumeLayout(false);
            this.testByTestSuites_panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.filelist_dataGridView)).EndInit();
            this.testbyTestcases_splitContainer.Panel1.ResumeLayout(false);
            this.testbyTestcases_splitContainer.Panel1.PerformLayout();
            this.testbyTestcases_splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.testbyTestcases_splitContainer)).EndInit();
            this.testbyTestcases_splitContainer.ResumeLayout(false);
            this.toolStripToolBar.ResumeLayout(false);
            this.toolStripToolBar.PerformLayout();
            this.TestCases_panel.ResumeLayout(false);
            this.TestCases_panel.PerformLayout();
            this.TestCases_toolStrip.ResumeLayout(false);
            this.TestCases_toolStrip.PerformLayout();
            this.testbyTestCases_panel.ResumeLayout(false);
            this.testbyTestCases_panel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.SplitContainer splitUpDown;
        private System.Windows.Forms.SplitContainer splitLeftRight;
        private System.Windows.Forms.TabControl tabsBottom;
        private System.Windows.Forms.TabPage tabOutput;
        private System.Windows.Forms.MenuStrip menuMain;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private HP.GFriend.UI.Controls.GFOutputBox textOutput;
        private System.Windows.Forms.ToolStripMenuItem toolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gotoToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStripToolBar;
        private System.Windows.Forms.Label deviceId_caption_label;
        private System.Windows.Forms.ToolTip toolTipDescriptions;
        private System.Windows.Forms.ToolStripMenuItem runTestToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopTestToolStripMenuItem;
        private System.ComponentModel.BackgroundWorker bgWorkerRunTest;
        private System.Windows.Forms.ListView testCases_listView;
        private System.Windows.Forms.ColumnHeader headerLine;
        private System.Windows.Forms.ColumnHeader headerTCName;
        private System.Windows.Forms.ToolStripMenuItem externalTool_ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem keywordListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader headerChkbox;
        private System.Windows.Forms.ToolStripMenuItem getPositionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deviceListToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.Label targetDevicePlaceholder;
        private System.Windows.Forms.Button deviceList_button;
        private System.Windows.Forms.Label deviceAddress_Caption_label;
        private System.Windows.Forms.Label deviceAddress_label;
        private System.Windows.Forms.DataGridView filelist_dataGridView;
        private System.Windows.Forms.Panel filelist_panel;
        private System.Windows.Forms.ToolStrip testSuites_toolStrip;
        private System.Windows.Forms.ToolStripButton tsStart_toolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton openFolder_toolStripButton;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.Panel TestCases_panel;
        private System.Windows.Forms.Panel testByTestSuites_panel;
        private System.Windows.Forms.Label testByTestSuites_label;
        private System.Windows.Forms.Panel testbyTestCases_panel;
        private System.Windows.Forms.Label testbyTestCases_label;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel TargetDevice_panel;
        private System.Windows.Forms.ToolStripButton newFile_toolStripButton;
        private System.Windows.Forms.ToolStripButton save_toolStripButton;
        private System.Windows.Forms.ToolStripButton saveAs_toolStripButton;
        private System.Windows.Forms.ToolStripButton tsStop_toolStripButton;
        private System.Windows.Forms.ToolStripButton openFile_toolStripButton;
        private System.Windows.Forms.ToolStrip TestCases_toolStrip;
        private System.Windows.Forms.ToolStripButton selectAll_toolStripButton;
        private System.Windows.Forms.ToolStripButton tcStart_ToolStripButton;
        private System.Windows.Forms.ToolStripButton tcStop_ToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton refresh_toolStripButton;
        private System.Windows.Forms.ToolStripMenuItem repositoryBrowserToolStripMenuItem;
        private System.Windows.Forms.Button clearTextOutput_button;
        private System.Windows.Forms.ToolStripMenuItem manualToolStripMenuItem;
        private System.Windows.Forms.Button paperless_off_button;
        private System.Windows.Forms.Button paperless_on_button;
        private System.Windows.Forms.Panel paperless_panel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.SplitContainer testbyTestcases_splitContainer;
        private System.Windows.Forms.TabControl textEditor_tabControl;
        private System.Windows.Forms.ToolStripMenuItem runTestAtServerToolStripMenuItem;
        private System.Windows.Forms.Panel RunatTestServer_panel;
        private System.Windows.Forms.ToolStrip RunatTestServer_toolStrip;
        private System.Windows.Forms.ToolStripButton runatTestServer_toolStripButton;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ToolStripButton showTestResult_toolStripButton;
        private System.Windows.Forms.ToolStripButton toolStripButton_scriptRepository;
        private System.Windows.Forms.TabPage tabRun;
        private GFInstantRun gfInstantRun;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private GFScriptFileList gfScriptFileList;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripButton toolStripButtonGit;
        private HP.GFriend.Utils.Git.GitControl gitControl;
        private System.Windows.Forms.ToolStripButton comment_toolStripButton; 
        private System.Windows.Forms.ToolStripButton goToDefinition_toolStripButton;
        private System.Windows.Forms.ToolStripMenuItem yammerPageToolStripMenuItem;
        private System.Windows.Forms.Label labelCoDeveloperStatus;
        private System.Windows.Forms.Button buttonCoDeveloper;
        private System.Windows.Forms.TabPage tabCoDeveloper;
        private System.Windows.Forms.Button buttonCopy;
        private System.Windows.Forms.Button buttonClearCoDeveloperText;
		private System.Windows.Forms.ToolStripMenuItem whatsNewToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox comboBoxKeyword;
        private System.Windows.Forms.Label labelKeyword;
        private System.Windows.Forms.Label labelLibrary;
        private System.Windows.Forms.ComboBox comboBoxLibrary;
        private System.Windows.Forms.ComboBox comboBoxReservedKeywords;
        private System.Windows.Forms.Label labelReservedKeywords;
        private System.Windows.Forms.TextBox textCoDeveloper;
        private System.Windows.Forms.Button GfriendChat;
        private System.Windows.Forms.ToolStripMenuItem encryptedVariablesToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton tcPause_ToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem networkMonitorToolStripMenuItem;

    }
}

