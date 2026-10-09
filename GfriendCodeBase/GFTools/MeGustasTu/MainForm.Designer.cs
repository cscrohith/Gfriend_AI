namespace HP.GFriend.Tool
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
            this.textBoxIpAddress = new System.Windows.Forms.TextBox();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.labelDeviceAddress = new System.Windows.Forms.Label();
            this.splitContainerScreen = new System.Windows.Forms.SplitContainer();
            this.loadingPictureBox = new System.Windows.Forms.PictureBox();
            this.pictureBoxScreenShot = new System.Windows.Forms.PictureBox();
            this.tabControlUiSelection = new System.Windows.Forms.TabControl();
            this.tabPageAndroid = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.treeViewXML = new System.Windows.Forms.TreeView();
            this.dataGridViewDescription = new System.Windows.Forms.DataGridView();
            this.colKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPageOmni = new System.Windows.Forms.TabPage();
            this.duneTreeView = new System.Windows.Forms.TreeView();
            this.listBoxOmniIds = new System.Windows.Forms.ListBox();
            this.toolStripTop = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonDump = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.labelMouse = new System.Windows.Forms.Label();
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.richTextBoxOutput = new System.Windows.Forms.RichTextBox();
            this.groupBoxLogFilter = new System.Windows.Forms.GroupBox();
            this.checkBoxError = new System.Windows.Forms.CheckBox();
            this.checkBoxWarn = new System.Windows.Forms.CheckBox();
            this.checkBoxDebug = new System.Windows.Forms.CheckBox();
            this.checkBoxTrace = new System.Windows.Forms.CheckBox();
            this.labelPassword = new System.Windows.Forms.Label();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.contextMenuStripPopupOnTree = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyXPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.duneSearch = new System.Windows.Forms.Label();
            this.duneSearchTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerScreen)).BeginInit();
            this.splitContainerScreen.Panel1.SuspendLayout();
            this.splitContainerScreen.Panel2.SuspendLayout();
            this.splitContainerScreen.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.loadingPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxScreenShot)).BeginInit();
            this.tabControlUiSelection.SuspendLayout();
            this.tabPageAndroid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDescription)).BeginInit();
            this.tabPageOmni.SuspendLayout();
            this.toolStripTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            this.groupBoxLogFilter.SuspendLayout();
            this.contextMenuStripPopupOnTree.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxIpAddress
            // 
            this.textBoxIpAddress.Location = new System.Drawing.Point(125, 41);
            this.textBoxIpAddress.Name = "textBoxIpAddress";
            this.textBoxIpAddress.Size = new System.Drawing.Size(131, 26);
            this.textBoxIpAddress.TabIndex = 0;
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(542, 41);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(99, 23);
            this.buttonConnect.TabIndex = 2;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.ButtonConnect_Click);
            // 
            // labelDeviceAddress
            // 
            this.labelDeviceAddress.AutoSize = true;
            this.labelDeviceAddress.Location = new System.Drawing.Point(15, 43);
            this.labelDeviceAddress.Name = "labelDeviceAddress";
            this.labelDeviceAddress.Size = new System.Drawing.Size(121, 18);
            this.labelDeviceAddress.TabIndex = 6;
            this.labelDeviceAddress.Text = "Device Address";
            // 
            // splitContainerScreen
            // 
            this.splitContainerScreen.BackColor = System.Drawing.SystemColors.Window;
            this.splitContainerScreen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerScreen.Location = new System.Drawing.Point(0, 0);
            this.splitContainerScreen.Name = "splitContainerScreen";
            // 
            // splitContainerScreen.Panel1
            // 
            this.splitContainerScreen.Panel1.Controls.Add(this.loadingPictureBox);
            this.splitContainerScreen.Panel1.Controls.Add(this.pictureBoxScreenShot);
            // 
            // splitContainerScreen.Panel2
            // 
            this.splitContainerScreen.Panel2.Controls.Add(this.tabControlUiSelection);
            this.splitContainerScreen.Size = new System.Drawing.Size(1225, 488);
            this.splitContainerScreen.SplitterDistance = 737;
            this.splitContainerScreen.SplitterWidth = 5;
            this.splitContainerScreen.TabIndex = 7;
            // 
            // loadingPictureBox
            // 
            this.loadingPictureBox.BackColor = System.Drawing.SystemColors.InfoText;
            this.loadingPictureBox.Image = global::HP.GFriend.Tool.Properties.Resources.Loading;
            this.loadingPictureBox.Location = new System.Drawing.Point(414, 219);
            this.loadingPictureBox.Name = "loadingPictureBox";
            this.loadingPictureBox.Size = new System.Drawing.Size(88, 55);
            this.loadingPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.loadingPictureBox.TabIndex = 1;
            this.loadingPictureBox.TabStop = false;
            // 
            // pictureBoxScreenShot
            // 
            this.pictureBoxScreenShot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxScreenShot.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxScreenShot.Name = "pictureBoxScreenShot";
            this.pictureBoxScreenShot.Size = new System.Drawing.Size(737, 488);
            this.pictureBoxScreenShot.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxScreenShot.TabIndex = 0;
            this.pictureBoxScreenShot.TabStop = false;
            this.pictureBoxScreenShot.Click += new System.EventHandler(this.PictureBoxScreenShot_Click);
            this.pictureBoxScreenShot.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBoxScreenShot_Paint);
            this.pictureBoxScreenShot.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictureBoxScreenShot_MouseMove);
            this.pictureBoxScreenShot.Resize += new System.EventHandler(this.PictureBoxScreenShot_Resize);
            // 
            // tabControlUiSelection
            // 
            this.tabControlUiSelection.Controls.Add(this.tabPageAndroid);
            this.tabControlUiSelection.Controls.Add(this.tabPageOmni);
            this.tabControlUiSelection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlUiSelection.Location = new System.Drawing.Point(0, 0);
            this.tabControlUiSelection.Name = "tabControlUiSelection";
            this.tabControlUiSelection.SelectedIndex = 0;
            this.tabControlUiSelection.Size = new System.Drawing.Size(483, 488);
            this.tabControlUiSelection.TabIndex = 1;
            // 
            // tabPageAndroid
            // 
            this.tabPageAndroid.Controls.Add(this.splitContainer1);
            this.tabPageAndroid.Location = new System.Drawing.Point(4, 27);
            this.tabPageAndroid.Name = "tabPageAndroid";
            this.tabPageAndroid.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAndroid.Size = new System.Drawing.Size(475, 457);
            this.tabPageAndroid.TabIndex = 0;
            this.tabPageAndroid.Text = "Android";
            this.tabPageAndroid.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeViewXML);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.dataGridViewDescription);
            this.splitContainer1.Size = new System.Drawing.Size(469, 451);
            this.splitContainer1.SplitterDistance = 245;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 1;
            // 
            // treeViewXML
            // 
            this.treeViewXML.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewXML.HideSelection = false;
            this.treeViewXML.Location = new System.Drawing.Point(0, 0);
            this.treeViewXML.Name = "treeViewXML";
            this.treeViewXML.Size = new System.Drawing.Size(469, 245);
            this.treeViewXML.TabIndex = 0;
            this.treeViewXML.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewXML_AfterSelect);
            this.treeViewXML.MouseClick += new System.Windows.Forms.MouseEventHandler(this.treeViewXML_MouseClick);
            // 
            // dataGridViewDescription
            // 
            this.dataGridViewDescription.AllowUserToAddRows = false;
            this.dataGridViewDescription.AllowUserToDeleteRows = false;
            this.dataGridViewDescription.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.dataGridViewDescription.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dataGridViewDescription.ColumnHeadersHeight = 29;
            this.dataGridViewDescription.ColumnHeadersVisible = false;
            this.dataGridViewDescription.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colKey,
            this.colVal});
            this.dataGridViewDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewDescription.GridColor = System.Drawing.SystemColors.Control;
            this.dataGridViewDescription.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewDescription.Name = "dataGridViewDescription";
            this.dataGridViewDescription.RowHeadersVisible = false;
            this.dataGridViewDescription.RowHeadersWidth = 51;
            this.dataGridViewDescription.RowTemplate.Height = 23;
            this.dataGridViewDescription.Size = new System.Drawing.Size(469, 201);
            this.dataGridViewDescription.TabIndex = 0;
            this.dataGridViewDescription.Resize += new System.EventHandler(this.DataGridViewDescription_Resize);
            // 
            // colKey
            // 
            this.colKey.HeaderText = "Key";
            this.colKey.MinimumWidth = 6;
            this.colKey.Name = "colKey";
            this.colKey.Width = 125;
            // 
            // colVal
            // 
            this.colVal.HeaderText = "Value";
            this.colVal.MinimumWidth = 6;
            this.colVal.Name = "colVal";
            this.colVal.Width = 125;
            // 
            // tabPageOmni
            // 
            this.tabPageOmni.Controls.Add(this.duneTreeView);
            this.tabPageOmni.Controls.Add(this.listBoxOmniIds);
            this.tabPageOmni.Location = new System.Drawing.Point(4, 27);
            this.tabPageOmni.Name = "tabPageOmni";
            this.tabPageOmni.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageOmni.Size = new System.Drawing.Size(475, 457);
            this.tabPageOmni.TabIndex = 1;
            this.tabPageOmni.Text = "Native UI";
            this.tabPageOmni.UseVisualStyleBackColor = true;
            // 
            // duneTreeView
            // 
            this.duneTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.duneTreeView.Location = new System.Drawing.Point(3, 3);
            this.duneTreeView.Name = "duneTreeView";
            this.duneTreeView.Size = new System.Drawing.Size(469, 451);
            this.duneTreeView.TabIndex = 1;
            this.duneTreeView.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.duneTreeView_BeforeSelect);
            this.duneTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.duneTreeView_AfterSelect);
            // 
            // listBoxOmniIds
            // 
            this.listBoxOmniIds.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxOmniIds.FormattingEnabled = true;
            this.listBoxOmniIds.ItemHeight = 18;
            this.listBoxOmniIds.Location = new System.Drawing.Point(3, 3);
            this.listBoxOmniIds.Name = "listBoxOmniIds";
            this.listBoxOmniIds.Size = new System.Drawing.Size(469, 451);
            this.listBoxOmniIds.TabIndex = 0;
            this.listBoxOmniIds.SelectedIndexChanged += new System.EventHandler(this.ListBoxOmniIds_SelectedIndexChanged);
            this.listBoxOmniIds.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ListBoxOmniIds_MouseClick);
            // 
            // toolStripTop
            // 
            this.toolStripTop.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStripTop.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonDump,
            this.toolStripSeparator1});
            this.toolStripTop.Location = new System.Drawing.Point(0, 0);
            this.toolStripTop.Name = "toolStripTop";
            this.toolStripTop.Size = new System.Drawing.Size(1264, 27);
            this.toolStripTop.TabIndex = 8;
            this.toolStripTop.Text = "toolStrip1";
            // 
            // toolStripButtonDump
            // 
            this.toolStripButtonDump.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonDump.Enabled = false;
            this.toolStripButtonDump.Image = global::HP.GFriend.Tool.Properties.Resources.capture;
            this.toolStripButtonDump.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonDump.Name = "toolStripButtonDump";
            this.toolStripButtonDump.Size = new System.Drawing.Size(29, 24);
            this.toolStripButtonDump.Text = "Dump";
            this.toolStripButtonDump.Click += new System.EventHandler(this.ToolStripButtonDump_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // labelMouse
            // 
            this.labelMouse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelMouse.AutoSize = true;
            this.labelMouse.Location = new System.Drawing.Point(1169, 55);
            this.labelMouse.Name = "labelMouse";
            this.labelMouse.Size = new System.Drawing.Size(52, 18);
            this.labelMouse.TabIndex = 9;
            this.labelMouse.Text = "(0, 0)";
            // 
            // splitContainerMain
            // 
            this.splitContainerMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerMain.Location = new System.Drawing.Point(19, 72);
            this.splitContainerMain.Name = "splitContainerMain";
            this.splitContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.splitContainerScreen);
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.richTextBoxOutput);
            this.splitContainerMain.Size = new System.Drawing.Size(1225, 623);
            this.splitContainerMain.SplitterDistance = 488;
            this.splitContainerMain.SplitterWidth = 5;
            this.splitContainerMain.TabIndex = 10;
            // 
            // richTextBoxOutput
            // 
            this.richTextBoxOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxOutput.Location = new System.Drawing.Point(0, 0);
            this.richTextBoxOutput.Name = "richTextBoxOutput";
            this.richTextBoxOutput.Size = new System.Drawing.Size(1225, 130);
            this.richTextBoxOutput.TabIndex = 6;
            this.richTextBoxOutput.Text = "";
            // 
            // groupBoxLogFilter
            // 
            this.groupBoxLogFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxLogFilter.Controls.Add(this.checkBoxError);
            this.groupBoxLogFilter.Controls.Add(this.checkBoxWarn);
            this.groupBoxLogFilter.Controls.Add(this.checkBoxDebug);
            this.groupBoxLogFilter.Controls.Add(this.checkBoxTrace);
            this.groupBoxLogFilter.Location = new System.Drawing.Point(877, 28);
            this.groupBoxLogFilter.Name = "groupBoxLogFilter";
            this.groupBoxLogFilter.Size = new System.Drawing.Size(274, 38);
            this.groupBoxLogFilter.TabIndex = 11;
            this.groupBoxLogFilter.TabStop = false;
            this.groupBoxLogFilter.Text = "Display Log";
            // 
            // checkBoxError
            // 
            this.checkBoxError.AutoSize = true;
            this.checkBoxError.Checked = true;
            this.checkBoxError.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxError.Location = new System.Drawing.Point(211, 15);
            this.checkBoxError.Name = "checkBoxError";
            this.checkBoxError.Size = new System.Drawing.Size(67, 22);
            this.checkBoxError.TabIndex = 3;
            this.checkBoxError.Text = "Error";
            this.checkBoxError.UseVisualStyleBackColor = true;
            // 
            // checkBoxWarn
            // 
            this.checkBoxWarn.AutoSize = true;
            this.checkBoxWarn.Checked = true;
            this.checkBoxWarn.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxWarn.Location = new System.Drawing.Point(145, 15);
            this.checkBoxWarn.Name = "checkBoxWarn";
            this.checkBoxWarn.Size = new System.Drawing.Size(68, 22);
            this.checkBoxWarn.TabIndex = 2;
            this.checkBoxWarn.Text = "Warn";
            this.checkBoxWarn.UseVisualStyleBackColor = true;
            // 
            // checkBoxDebug
            // 
            this.checkBoxDebug.AutoSize = true;
            this.checkBoxDebug.Checked = true;
            this.checkBoxDebug.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDebug.Location = new System.Drawing.Point(72, 15);
            this.checkBoxDebug.Name = "checkBoxDebug";
            this.checkBoxDebug.Size = new System.Drawing.Size(77, 22);
            this.checkBoxDebug.TabIndex = 1;
            this.checkBoxDebug.Text = "Debug";
            this.checkBoxDebug.UseVisualStyleBackColor = true;
            // 
            // checkBoxTrace
            // 
            this.checkBoxTrace.AutoSize = true;
            this.checkBoxTrace.Checked = true;
            this.checkBoxTrace.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxTrace.Location = new System.Drawing.Point(6, 15);
            this.checkBoxTrace.Name = "checkBoxTrace";
            this.checkBoxTrace.Size = new System.Drawing.Size(70, 22);
            this.checkBoxTrace.TabIndex = 0;
            this.checkBoxTrace.Text = "Trace";
            this.checkBoxTrace.UseVisualStyleBackColor = true;
            // 
            // labelPassword
            // 
            this.labelPassword.AutoSize = true;
            this.labelPassword.Location = new System.Drawing.Point(276, 44);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(131, 18);
            this.labelPassword.TabIndex = 13;
            this.labelPassword.Text = "Admin Password";
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(390, 42);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.Size = new System.Drawing.Size(131, 26);
            this.textBoxPassword.TabIndex = 1;
            // 
            // contextMenuStripPopupOnTree
            // 
            this.contextMenuStripPopupOnTree.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStripPopupOnTree.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyXPathToolStripMenuItem});
            this.contextMenuStripPopupOnTree.Name = "contextMenuStripPopupOnTree";
            this.contextMenuStripPopupOnTree.ShowImageMargin = false;
            this.contextMenuStripPopupOnTree.Size = new System.Drawing.Size(129, 28);
            // 
            // copyXPathToolStripMenuItem
            // 
            this.copyXPathToolStripMenuItem.Name = "copyXPathToolStripMenuItem";
            this.copyXPathToolStripMenuItem.Size = new System.Drawing.Size(128, 24);
            this.copyXPathToolStripMenuItem.Text = "Copy XPath";
            this.copyXPathToolStripMenuItem.Click += new System.EventHandler(this.copyXPathToolStripMenuItem_Click);
            // 
            // duneSearch
            // 
            this.duneSearch.AutoSize = true;
            this.duneSearch.Location = new System.Drawing.Point(678, 44);
            this.duneSearch.Name = "duneSearch";
            this.duneSearch.Size = new System.Drawing.Size(59, 18);
            this.duneSearch.TabIndex = 14;
            this.duneSearch.Text = "Search";
            // 
            // duneSearchTextBox
            // 
            this.duneSearchTextBox.Location = new System.Drawing.Point(744, 38);
            this.duneSearchTextBox.Name = "duneSearchTextBox";
            this.duneSearchTextBox.Size = new System.Drawing.Size(100, 26);
            this.duneSearchTextBox.TabIndex = 15;
            this.duneSearchTextBox.TextChanged += new System.EventHandler(this.duneSearchTextBox_TextChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 709);
            this.Controls.Add(this.duneSearchTextBox);
            this.Controls.Add(this.duneSearch);
            this.Controls.Add(this.labelPassword);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.groupBoxLogFilter);
            this.Controls.Add(this.splitContainerMain);
            this.Controls.Add(this.labelMouse);
            this.Controls.Add(this.toolStripTop);
            this.Controls.Add(this.labelDeviceAddress);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.textBoxIpAddress);
            this.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Me Gustas Tu";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TesterMainForm_FormClosed);
            this.splitContainerScreen.Panel1.ResumeLayout(false);
            this.splitContainerScreen.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerScreen)).EndInit();
            this.splitContainerScreen.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.loadingPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxScreenShot)).EndInit();
            this.tabControlUiSelection.ResumeLayout(false);
            this.tabPageAndroid.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDescription)).EndInit();
            this.tabPageOmni.ResumeLayout(false);
            this.toolStripTop.ResumeLayout(false);
            this.toolStripTop.PerformLayout();
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            this.groupBoxLogFilter.ResumeLayout(false);
            this.groupBoxLogFilter.PerformLayout();
            this.contextMenuStripPopupOnTree.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox textBoxIpAddress;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Label labelDeviceAddress;
        private System.Windows.Forms.SplitContainer splitContainerScreen;
        private System.Windows.Forms.PictureBox pictureBoxScreenShot;
        private System.Windows.Forms.ToolStrip toolStripTop;
        private System.Windows.Forms.ToolStripButton toolStripButtonDump;
        private System.Windows.Forms.Label labelMouse;
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.GroupBox groupBoxLogFilter;
        private System.Windows.Forms.CheckBox checkBoxError;
        private System.Windows.Forms.CheckBox checkBoxWarn;
        private System.Windows.Forms.CheckBox checkBoxDebug;
        private System.Windows.Forms.CheckBox checkBoxTrace;
        private System.Windows.Forms.TabControl tabControlUiSelection;
        private System.Windows.Forms.TabPage tabPageAndroid;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView treeViewXML;
        private System.Windows.Forms.DataGridView dataGridViewDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn colKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVal;
        private System.Windows.Forms.TabPage tabPageOmni;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.ListBox listBoxOmniIds;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripPopupOnTree;
        private System.Windows.Forms.ToolStripMenuItem copyXPathToolStripMenuItem;
        private System.Windows.Forms.Label duneSearch;
        private System.Windows.Forms.TextBox duneSearchTextBox;
        private System.Windows.Forms.TreeView duneTreeView;
        private System.Windows.Forms.PictureBox loadingPictureBox;
        private System.Windows.Forms.RichTextBox richTextBoxOutput;
    }
}

