namespace HP.GFriend.Client.Forms
{
    partial class GFServerExecutionForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.ServerExecution_splitContainer = new System.Windows.Forms.SplitContainer();
            this.serverInfo_groupBox = new System.Windows.Forms.GroupBox();
            this.tSInfo_label = new System.Windows.Forms.Label();
            this.tSList_label = new System.Windows.Forms.Label();
            this.fileInfo_dataGridView = new System.Windows.Forms.DataGridView();
            this.tsList_dataGridView = new System.Windows.Forms.DataGridView();
            this.serverList_treeView = new System.Windows.Forms.TreeView();
            this.gfServerAddr_label = new System.Windows.Forms.Label();
            this.Connect_button = new System.Windows.Forms.Button();
            this.gfServerAddr_textBox = new System.Windows.Forms.TextBox();
            this.TestControl_toolStrip = new System.Windows.Forms.ToolStrip();
            this.runtoServer_toolStripButton = new System.Windows.Forms.ToolStripButton();
            this.viewServer_toolStripButton = new System.Windows.Forms.ToolStripButton();
            this.userInfo_groupBox = new System.Windows.Forms.GroupBox();
            this.userInfo_label = new System.Windows.Forms.Label();
            this.userId_Caption_label = new System.Windows.Forms.Label();
            this.email_groupBox = new System.Windows.Forms.GroupBox();
            this.panelEmailWorking = new System.Windows.Forms.Panel();
            this.labelEmailGetting = new System.Windows.Forms.Label();
            this.emailRemove_button = new System.Windows.Forms.Button();
            this.eMail_dataGridView = new System.Windows.Forms.DataGridView();
            this.emailAdd_button = new System.Windows.Forms.Button();
            this.Email_textBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.workflow_groupBox = new System.Windows.Forms.GroupBox();
            this.timeBased_maskedTextBox = new System.Windows.Forms.MaskedTextBox();
            this.hhmm_label = new System.Windows.Forms.Label();
            this.countBased_textBox = new System.Windows.Forms.TextBox();
            this.iterations_label = new System.Windows.Forms.Label();
            this.duration_label = new System.Windows.Forms.Label();
            this.countBased_radioButton = new System.Windows.Forms.RadioButton();
            this.timeBased_radioButton = new System.Windows.Forms.RadioButton();
            this.DeviceInfo_groupBox = new System.Windows.Forms.GroupBox();
            this.deviceId_comboBox = new System.Windows.Forms.ComboBox();
            this.deviceAddress_label = new System.Windows.Forms.Label();
            this.deviceAddress_Caption_label = new System.Windows.Forms.Label();
            this.deviceId_Caption_label = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.ServerExecution_splitContainer)).BeginInit();
            this.ServerExecution_splitContainer.Panel1.SuspendLayout();
            this.ServerExecution_splitContainer.Panel2.SuspendLayout();
            this.ServerExecution_splitContainer.SuspendLayout();
            this.serverInfo_groupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fileInfo_dataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tsList_dataGridView)).BeginInit();
            this.TestControl_toolStrip.SuspendLayout();
            this.userInfo_groupBox.SuspendLayout();
            this.email_groupBox.SuspendLayout();
            this.panelEmailWorking.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.eMail_dataGridView)).BeginInit();
            this.workflow_groupBox.SuspendLayout();
            this.DeviceInfo_groupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // ServerExecution_splitContainer
            // 
            this.ServerExecution_splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ServerExecution_splitContainer.Location = new System.Drawing.Point(0, 0);
            this.ServerExecution_splitContainer.Name = "ServerExecution_splitContainer";
            // 
            // ServerExecution_splitContainer.Panel1
            // 
            this.ServerExecution_splitContainer.Panel1.Controls.Add(this.serverInfo_groupBox);
            // 
            // ServerExecution_splitContainer.Panel2
            // 
            this.ServerExecution_splitContainer.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.ServerExecution_splitContainer.Panel2.Controls.Add(this.TestControl_toolStrip);
            this.ServerExecution_splitContainer.Panel2.Controls.Add(this.userInfo_groupBox);
            this.ServerExecution_splitContainer.Panel2.Controls.Add(this.email_groupBox);
            this.ServerExecution_splitContainer.Panel2.Controls.Add(this.workflow_groupBox);
            this.ServerExecution_splitContainer.Panel2.Controls.Add(this.DeviceInfo_groupBox);
            this.ServerExecution_splitContainer.Size = new System.Drawing.Size(800, 450);
            this.ServerExecution_splitContainer.SplitterDistance = 460;
            this.ServerExecution_splitContainer.TabIndex = 0;
            this.ServerExecution_splitContainer.TabStop = false;
            // 
            // serverInfo_groupBox
            // 
            this.serverInfo_groupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.serverInfo_groupBox.AutoSize = true;
            this.serverInfo_groupBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.serverInfo_groupBox.Controls.Add(this.tSInfo_label);
            this.serverInfo_groupBox.Controls.Add(this.tSList_label);
            this.serverInfo_groupBox.Controls.Add(this.fileInfo_dataGridView);
            this.serverInfo_groupBox.Controls.Add(this.tsList_dataGridView);
            this.serverInfo_groupBox.Controls.Add(this.serverList_treeView);
            this.serverInfo_groupBox.Controls.Add(this.gfServerAddr_label);
            this.serverInfo_groupBox.Controls.Add(this.Connect_button);
            this.serverInfo_groupBox.Controls.Add(this.gfServerAddr_textBox);
            this.serverInfo_groupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.serverInfo_groupBox.Location = new System.Drawing.Point(3, 0);
            this.serverInfo_groupBox.Name = "serverInfo_groupBox";
            this.serverInfo_groupBox.Size = new System.Drawing.Size(455, 461);
            this.serverInfo_groupBox.TabIndex = 0;
            this.serverInfo_groupBox.TabStop = false;
            this.serverInfo_groupBox.Text = "Server Info";
            // 
            // tSInfo_label
            // 
            this.tSInfo_label.AutoSize = true;
            this.tSInfo_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tSInfo_label.Location = new System.Drawing.Point(246, 54);
            this.tSInfo_label.Name = "tSInfo_label";
            this.tSInfo_label.Size = new System.Drawing.Size(79, 13);
            this.tSInfo_label.TabIndex = 12;
            this.tSInfo_label.Text = "Test Suite Info:";
            // 
            // tSList_label
            // 
            this.tSList_label.AutoSize = true;
            this.tSList_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tSList_label.Location = new System.Drawing.Point(242, 184);
            this.tSList_label.Name = "tSList_label";
            this.tSList_label.Size = new System.Drawing.Size(130, 13);
            this.tSList_label.TabIndex = 11;
            this.tSList_label.Text = "Test Suites for Server run:";
            // 
            // fileInfo_dataGridView
            // 
            this.fileInfo_dataGridView.AllowUserToAddRows = false;
            this.fileInfo_dataGridView.AllowUserToDeleteRows = false;
            this.fileInfo_dataGridView.AllowUserToResizeColumns = false;
            this.fileInfo_dataGridView.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.White;
            this.fileInfo_dataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.fileInfo_dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.fileInfo_dataGridView.ColumnHeadersVisible = false;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.fileInfo_dataGridView.DefaultCellStyle = dataGridViewCellStyle2;
            this.fileInfo_dataGridView.Location = new System.Drawing.Point(249, 70);
            this.fileInfo_dataGridView.Name = "fileInfo_dataGridView";
            this.fileInfo_dataGridView.RowHeadersVisible = false;
            this.fileInfo_dataGridView.Size = new System.Drawing.Size(200, 97);
            this.fileInfo_dataGridView.TabIndex = 3;
            // 
            // tsList_dataGridView
            // 
            this.tsList_dataGridView.AllowUserToAddRows = false;
            this.tsList_dataGridView.AllowUserToDeleteRows = false;
            this.tsList_dataGridView.AllowUserToResizeColumns = false;
            this.tsList_dataGridView.AllowUserToResizeRows = false;
            this.tsList_dataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.ActiveBorder;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.tsList_dataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.tsList_dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tsList_dataGridView.Location = new System.Drawing.Point(249, 203);
            this.tsList_dataGridView.MultiSelect = false;
            this.tsList_dataGridView.Name = "tsList_dataGridView";
            this.tsList_dataGridView.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.tsList_dataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.tsList_dataGridView.RowHeadersVisible = false;
            this.tsList_dataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.tsList_dataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tsList_dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.tsList_dataGridView.Size = new System.Drawing.Size(200, 238);
            this.tsList_dataGridView.TabIndex = 4;
            // 
            // serverList_treeView
            // 
            this.serverList_treeView.CheckBoxes = true;
            this.serverList_treeView.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.serverList_treeView.Location = new System.Drawing.Point(6, 54);
            this.serverList_treeView.Name = "serverList_treeView";
            this.serverList_treeView.Size = new System.Drawing.Size(230, 387);
            this.serverList_treeView.TabIndex = 2;
            this.serverList_treeView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.serverList_treeView_AfterCheck);
            this.serverList_treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.serverList_treeView_AfterSelect);
            this.serverList_treeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.serverlist_treeView_NodeMouseClick);
            // 
            // gfServerAddr_label
            // 
            this.gfServerAddr_label.AutoSize = true;
            this.gfServerAddr_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gfServerAddr_label.Location = new System.Drawing.Point(134, 28);
            this.gfServerAddr_label.Name = "gfServerAddr_label";
            this.gfServerAddr_label.Size = new System.Drawing.Size(82, 13);
            this.gfServerAddr_label.TabIndex = 6;
            this.gfServerAddr_label.Text = "Server Address:";
            // 
            // Connect_button
            // 
            this.Connect_button.BackColor = System.Drawing.Color.Transparent;
            this.Connect_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Connect_button.Location = new System.Drawing.Point(375, 22);
            this.Connect_button.Name = "Connect_button";
            this.Connect_button.Size = new System.Drawing.Size(73, 23);
            this.Connect_button.TabIndex = 1;
            this.Connect_button.Text = "Connect";
            this.Connect_button.UseVisualStyleBackColor = false;
            this.Connect_button.Click += new System.EventHandler(this.Connect_button_Click);
            // 
            // gfServerAddr_textBox
            // 
            this.gfServerAddr_textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gfServerAddr_textBox.Location = new System.Drawing.Point(222, 23);
            this.gfServerAddr_textBox.Name = "gfServerAddr_textBox";
            this.gfServerAddr_textBox.Size = new System.Drawing.Size(147, 21);
            this.gfServerAddr_textBox.TabIndex = 0;
            // 
            // TestControl_toolStrip
            // 
            this.TestControl_toolStrip.BackColor = System.Drawing.Color.Transparent;
            this.TestControl_toolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.TestControl_toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.TestControl_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runtoServer_toolStripButton,
            this.viewServer_toolStripButton});
            this.TestControl_toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.TestControl_toolStrip.Location = new System.Drawing.Point(4, 9);
            this.TestControl_toolStrip.Name = "TestControl_toolStrip";
            this.TestControl_toolStrip.Size = new System.Drawing.Size(225, 25);
            this.TestControl_toolStrip.TabIndex = 9;
            this.TestControl_toolStrip.Text = "toolStrip1";
            // 
            // runtoServer_toolStripButton
            // 
            this.runtoServer_toolStripButton.Image = global::HP.GFriend.Client.Properties.Resources.StartWeb_16x;
            this.runtoServer_toolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.runtoServer_toolStripButton.Name = "runtoServer_toolStripButton";
            this.runtoServer_toolStripButton.Size = new System.Drawing.Size(99, 22);
            this.runtoServer_toolStripButton.Text = "Run at Server";
            this.runtoServer_toolStripButton.Click += new System.EventHandler(this.runtoServer_toolStripButton_Click);
            // 
            // viewServer_toolStripButton
            // 
            this.viewServer_toolStripButton.Image = global::HP.GFriend.Client.Properties.Resources.WebServer_16x;
            this.viewServer_toolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.viewServer_toolStripButton.Name = "viewServer_toolStripButton";
            this.viewServer_toolStripButton.Size = new System.Drawing.Size(123, 22);
            this.viewServer_toolStripButton.Text = "Show Test Results";
            this.viewServer_toolStripButton.Click += new System.EventHandler(this.viewServer_toolStripButton_Click);
            // 
            // userInfo_groupBox
            // 
            this.userInfo_groupBox.Controls.Add(this.userInfo_label);
            this.userInfo_groupBox.Controls.Add(this.userId_Caption_label);
            this.userInfo_groupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.userInfo_groupBox.Location = new System.Drawing.Point(3, 45);
            this.userInfo_groupBox.Name = "userInfo_groupBox";
            this.userInfo_groupBox.Size = new System.Drawing.Size(329, 56);
            this.userInfo_groupBox.TabIndex = 8;
            this.userInfo_groupBox.TabStop = false;
            this.userInfo_groupBox.Text = "User Info";
            // 
            // userInfo_label
            // 
            this.userInfo_label.AutoSize = true;
            this.userInfo_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.userInfo_label.Location = new System.Drawing.Point(166, 28);
            this.userInfo_label.Name = "userInfo_label";
            this.userInfo_label.Size = new System.Drawing.Size(33, 13);
            this.userInfo_label.TabIndex = 5;
            this.userInfo_label.Text = "None";
            // 
            // userId_Caption_label
            // 
            this.userId_Caption_label.AutoSize = true;
            this.userId_Caption_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.userId_Caption_label.Location = new System.Drawing.Point(23, 28);
            this.userId_Caption_label.Name = "userId_Caption_label";
            this.userId_Caption_label.Size = new System.Drawing.Size(52, 13);
            this.userId_Caption_label.TabIndex = 1;
            this.userId_Caption_label.Text = "User Id:";
            // 
            // email_groupBox
            // 
            this.email_groupBox.Controls.Add(this.panelEmailWorking);
            this.email_groupBox.Controls.Add(this.emailRemove_button);
            this.email_groupBox.Controls.Add(this.eMail_dataGridView);
            this.email_groupBox.Controls.Add(this.emailAdd_button);
            this.email_groupBox.Controls.Add(this.Email_textBox);
            this.email_groupBox.Controls.Add(this.label2);
            this.email_groupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.email_groupBox.Location = new System.Drawing.Point(3, 290);
            this.email_groupBox.Name = "email_groupBox";
            this.email_groupBox.Size = new System.Drawing.Size(329, 151);
            this.email_groupBox.TabIndex = 7;
            this.email_groupBox.TabStop = false;
            this.email_groupBox.Text = "Notify to";
            // 
            // panelEmailWorking
            // 
            this.panelEmailWorking.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelEmailWorking.Controls.Add(this.labelEmailGetting);
            this.panelEmailWorking.Location = new System.Drawing.Point(36, 48);
            this.panelEmailWorking.Name = "panelEmailWorking";
            this.panelEmailWorking.Size = new System.Drawing.Size(269, 62);
            this.panelEmailWorking.TabIndex = 14;
            // 
            // labelEmailGetting
            // 
            this.labelEmailGetting.AutoSize = true;
            this.labelEmailGetting.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelEmailGetting.Location = new System.Drawing.Point(6, 21);
            this.labelEmailGetting.Name = "labelEmailGetting";
            this.labelEmailGetting.Size = new System.Drawing.Size(257, 16);
            this.labelEmailGetting.TabIndex = 0;
            this.labelEmailGetting.Text = "Getting current user\'s email address";
            // 
            // emailRemove_button
            // 
            this.emailRemove_button.Location = new System.Drawing.Point(269, 50);
            this.emailRemove_button.Name = "emailRemove_button";
            this.emailRemove_button.Size = new System.Drawing.Size(24, 23);
            this.emailRemove_button.TabIndex = 13;
            this.emailRemove_button.Text = "-";
            this.emailRemove_button.UseVisualStyleBackColor = true;
            this.emailRemove_button.Click += new System.EventHandler(this.emailRemove_button_Click);
            // 
            // eMail_dataGridView
            // 
            this.eMail_dataGridView.AllowUserToAddRows = false;
            this.eMail_dataGridView.AllowUserToDeleteRows = false;
            this.eMail_dataGridView.AllowUserToResizeRows = false;
            this.eMail_dataGridView.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight;
            this.eMail_dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.eMail_dataGridView.ColumnHeadersVisible = false;
            this.eMail_dataGridView.GridColor = System.Drawing.SystemColors.ButtonHighlight;
            this.eMail_dataGridView.Location = new System.Drawing.Point(119, 50);
            this.eMail_dataGridView.MultiSelect = false;
            this.eMail_dataGridView.Name = "eMail_dataGridView";
            this.eMail_dataGridView.ReadOnly = true;
            this.eMail_dataGridView.RowHeadersVisible = false;
            this.eMail_dataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.eMail_dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.eMail_dataGridView.Size = new System.Drawing.Size(144, 95);
            this.eMail_dataGridView.TabIndex = 12;
            // 
            // emailAdd_button
            // 
            this.emailAdd_button.Location = new System.Drawing.Point(269, 21);
            this.emailAdd_button.Name = "emailAdd_button";
            this.emailAdd_button.Size = new System.Drawing.Size(24, 23);
            this.emailAdd_button.TabIndex = 11;
            this.emailAdd_button.Text = "+";
            this.emailAdd_button.UseVisualStyleBackColor = true;
            this.emailAdd_button.Click += new System.EventHandler(this.email_button_Click);
            // 
            // Email_textBox
            // 
            this.Email_textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Email_textBox.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.Email_textBox.Location = new System.Drawing.Point(119, 22);
            this.Email_textBox.Name = "Email_textBox";
            this.Email_textBox.Size = new System.Drawing.Size(144, 20);
            this.Email_textBox.TabIndex = 10;
            this.Email_textBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Email_textBox_KeyDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label2.Location = new System.Drawing.Point(23, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Email Address:";
            // 
            // workflow_groupBox
            // 
            this.workflow_groupBox.Controls.Add(this.timeBased_maskedTextBox);
            this.workflow_groupBox.Controls.Add(this.hhmm_label);
            this.workflow_groupBox.Controls.Add(this.countBased_textBox);
            this.workflow_groupBox.Controls.Add(this.iterations_label);
            this.workflow_groupBox.Controls.Add(this.duration_label);
            this.workflow_groupBox.Controls.Add(this.countBased_radioButton);
            this.workflow_groupBox.Controls.Add(this.timeBased_radioButton);
            this.workflow_groupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.workflow_groupBox.Location = new System.Drawing.Point(3, 203);
            this.workflow_groupBox.Name = "workflow_groupBox";
            this.workflow_groupBox.Size = new System.Drawing.Size(329, 81);
            this.workflow_groupBox.TabIndex = 6;
            this.workflow_groupBox.TabStop = false;
            this.workflow_groupBox.Text = "Workflow Options";
            // 
            // timeBased_maskedTextBox
            // 
            this.timeBased_maskedTextBox.Enabled = false;
            this.timeBased_maskedTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.timeBased_maskedTextBox.Location = new System.Drawing.Point(222, 44);
            this.timeBased_maskedTextBox.Mask = "900:00";
            this.timeBased_maskedTextBox.Name = "timeBased_maskedTextBox";
            this.timeBased_maskedTextBox.Size = new System.Drawing.Size(51, 20);
            this.timeBased_maskedTextBox.TabIndex = 9;
            this.timeBased_maskedTextBox.Text = "00100";
            this.timeBased_maskedTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // hhmm_label
            // 
            this.hhmm_label.AutoSize = true;
            this.hhmm_label.Enabled = false;
            this.hhmm_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.hhmm_label.Location = new System.Drawing.Point(279, 49);
            this.hhmm_label.Name = "hhmm_label";
            this.hhmm_label.Size = new System.Drawing.Size(50, 13);
            this.hhmm_label.TabIndex = 12;
            this.hhmm_label.Text = "(hhh:mm)";
            // 
            // countBased_textBox
            // 
            this.countBased_textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.countBased_textBox.Location = new System.Drawing.Point(222, 21);
            this.countBased_textBox.Name = "countBased_textBox";
            this.countBased_textBox.Size = new System.Drawing.Size(51, 20);
            this.countBased_textBox.TabIndex = 7;
            this.countBased_textBox.Text = "1";
            this.countBased_textBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.countBased_textBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.countBased_textBox_KeyPress);
            // 
            // iterations_label
            // 
            this.iterations_label.AutoSize = true;
            this.iterations_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.iterations_label.Location = new System.Drawing.Point(166, 26);
            this.iterations_label.Name = "iterations_label";
            this.iterations_label.Size = new System.Drawing.Size(53, 13);
            this.iterations_label.TabIndex = 7;
            this.iterations_label.Text = "Iterations:";
            // 
            // duration_label
            // 
            this.duration_label.AutoSize = true;
            this.duration_label.Enabled = false;
            this.duration_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.duration_label.Location = new System.Drawing.Point(166, 49);
            this.duration_label.Name = "duration_label";
            this.duration_label.Size = new System.Drawing.Size(50, 13);
            this.duration_label.TabIndex = 6;
            this.duration_label.Text = "Duration:";
            // 
            // countBased_radioButton
            // 
            this.countBased_radioButton.AutoSize = true;
            this.countBased_radioButton.Checked = true;
            this.countBased_radioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.countBased_radioButton.Location = new System.Drawing.Point(27, 22);
            this.countBased_radioButton.Name = "countBased_radioButton";
            this.countBased_radioButton.Size = new System.Drawing.Size(124, 19);
            this.countBased_radioButton.TabIndex = 6;
            this.countBased_radioButton.TabStop = true;
            this.countBased_radioButton.Text = "Count Based Flow";
            this.countBased_radioButton.UseVisualStyleBackColor = true;
            // 
            // timeBased_radioButton
            // 
            this.timeBased_radioButton.AutoSize = true;
            this.timeBased_radioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.timeBased_radioButton.Location = new System.Drawing.Point(27, 45);
            this.timeBased_radioButton.Name = "timeBased_radioButton";
            this.timeBased_radioButton.Size = new System.Drawing.Size(120, 19);
            this.timeBased_radioButton.TabIndex = 8;
            this.timeBased_radioButton.Text = "Time Based Flow";
            this.timeBased_radioButton.UseVisualStyleBackColor = true;
            this.timeBased_radioButton.CheckedChanged += new System.EventHandler(this.timeBased_radioButton_CheckedChanged);
            // 
            // DeviceInfo_groupBox
            // 
            this.DeviceInfo_groupBox.Controls.Add(this.deviceId_comboBox);
            this.DeviceInfo_groupBox.Controls.Add(this.deviceAddress_label);
            this.DeviceInfo_groupBox.Controls.Add(this.deviceAddress_Caption_label);
            this.DeviceInfo_groupBox.Controls.Add(this.deviceId_Caption_label);
            this.DeviceInfo_groupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.DeviceInfo_groupBox.Location = new System.Drawing.Point(4, 107);
            this.DeviceInfo_groupBox.Name = "DeviceInfo_groupBox";
            this.DeviceInfo_groupBox.Size = new System.Drawing.Size(329, 90);
            this.DeviceInfo_groupBox.TabIndex = 3;
            this.DeviceInfo_groupBox.TabStop = false;
            this.DeviceInfo_groupBox.Text = "Device Info";
            // 
            // deviceId_comboBox
            // 
            this.deviceId_comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.deviceId_comboBox.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.deviceId_comboBox.FormattingEnabled = true;
            this.deviceId_comboBox.Location = new System.Drawing.Point(168, 23);
            this.deviceId_comboBox.Name = "deviceId_comboBox";
            this.deviceId_comboBox.Size = new System.Drawing.Size(121, 19);
            this.deviceId_comboBox.TabIndex = 5;
            this.deviceId_comboBox.SelectedIndexChanged += new System.EventHandler(this.deviceId_comboBox_SelectedIndexChanged);
            // 
            // deviceAddress_label
            // 
            this.deviceAddress_label.AutoSize = true;
            this.deviceAddress_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.deviceAddress_label.Location = new System.Drawing.Point(166, 57);
            this.deviceAddress_label.Name = "deviceAddress_label";
            this.deviceAddress_label.Size = new System.Drawing.Size(33, 13);
            this.deviceAddress_label.TabIndex = 4;
            this.deviceAddress_label.Text = "None";
            // 
            // deviceAddress_Caption_label
            // 
            this.deviceAddress_Caption_label.AutoSize = true;
            this.deviceAddress_Caption_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.deviceAddress_Caption_label.Location = new System.Drawing.Point(23, 57);
            this.deviceAddress_Caption_label.Name = "deviceAddress_Caption_label";
            this.deviceAddress_Caption_label.Size = new System.Drawing.Size(100, 13);
            this.deviceAddress_Caption_label.TabIndex = 3;
            this.deviceAddress_Caption_label.Text = "Device Address:";
            // 
            // deviceId_Caption_label
            // 
            this.deviceId_Caption_label.AutoSize = true;
            this.deviceId_Caption_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.deviceId_Caption_label.Location = new System.Drawing.Point(23, 28);
            this.deviceId_Caption_label.Name = "deviceId_Caption_label";
            this.deviceId_Caption_label.Size = new System.Drawing.Size(66, 13);
            this.deviceId_Caption_label.TabIndex = 1;
            this.deviceId_Caption_label.Text = "Device Id:";
            // 
            // GFServerExecutionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.ServerExecution_splitContainer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GFServerExecutionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Server Execution";
            this.ServerExecution_splitContainer.Panel1.ResumeLayout(false);
            this.ServerExecution_splitContainer.Panel1.PerformLayout();
            this.ServerExecution_splitContainer.Panel2.ResumeLayout(false);
            this.ServerExecution_splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ServerExecution_splitContainer)).EndInit();
            this.ServerExecution_splitContainer.ResumeLayout(false);
            this.serverInfo_groupBox.ResumeLayout(false);
            this.serverInfo_groupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fileInfo_dataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tsList_dataGridView)).EndInit();
            this.TestControl_toolStrip.ResumeLayout(false);
            this.TestControl_toolStrip.PerformLayout();
            this.userInfo_groupBox.ResumeLayout(false);
            this.userInfo_groupBox.PerformLayout();
            this.email_groupBox.ResumeLayout(false);
            this.email_groupBox.PerformLayout();
            this.panelEmailWorking.ResumeLayout(false);
            this.panelEmailWorking.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.eMail_dataGridView)).EndInit();
            this.workflow_groupBox.ResumeLayout(false);
            this.workflow_groupBox.PerformLayout();
            this.DeviceInfo_groupBox.ResumeLayout(false);
            this.DeviceInfo_groupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer ServerExecution_splitContainer;
        private System.Windows.Forms.GroupBox DeviceInfo_groupBox;
        private System.Windows.Forms.Label deviceId_Caption_label;
        private System.Windows.Forms.Label deviceAddress_Caption_label;
        private System.Windows.Forms.Label deviceAddress_label;
        private System.Windows.Forms.GroupBox workflow_groupBox;
        private System.Windows.Forms.RadioButton countBased_radioButton;
        private System.Windows.Forms.RadioButton timeBased_radioButton;
        private System.Windows.Forms.GroupBox email_groupBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox Email_textBox;
        private System.Windows.Forms.Button emailAdd_button;
        private System.Windows.Forms.GroupBox userInfo_groupBox;
        private System.Windows.Forms.Label userInfo_label;
        private System.Windows.Forms.Label userId_Caption_label;
        private System.Windows.Forms.Label duration_label;
        private System.Windows.Forms.Label iterations_label;
        private System.Windows.Forms.Label hhmm_label;
        private System.Windows.Forms.TextBox countBased_textBox;
        private System.Windows.Forms.ToolStrip TestControl_toolStrip;
        private System.Windows.Forms.ToolStripButton runtoServer_toolStripButton;
        private System.Windows.Forms.ToolStripButton viewServer_toolStripButton;
        private System.Windows.Forms.GroupBox serverInfo_groupBox;
        private System.Windows.Forms.Button Connect_button;
        private System.Windows.Forms.TextBox gfServerAddr_textBox;
        private System.Windows.Forms.Label gfServerAddr_label;
        private System.Windows.Forms.TreeView serverList_treeView;
        private System.Windows.Forms.DataGridView tsList_dataGridView;
        private System.Windows.Forms.ComboBox deviceId_comboBox;
        private System.Windows.Forms.DataGridView fileInfo_dataGridView;
        private System.Windows.Forms.Label tSList_label;
        private System.Windows.Forms.Label tSInfo_label;
        private System.Windows.Forms.MaskedTextBox timeBased_maskedTextBox;
        private System.Windows.Forms.DataGridView eMail_dataGridView;
        private System.Windows.Forms.Button emailRemove_button;
        private System.Windows.Forms.Panel panelEmailWorking;
        private System.Windows.Forms.Label labelEmailGetting;
    }
}