namespace DeviceDetails
{
    partial class DeviceDetailsForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeviceDetailsForm));
            this.labelDeviceAddress = new System.Windows.Forms.Label();
            this.labelPassword = new System.Windows.Forms.Label();
            this.textBoxIpAddress = new System.Windows.Forms.TextBox();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.buttonViewDetails = new System.Windows.Forms.Button();
            this.PackageName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Version = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UUID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.InstalledFile = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewSolutions = new System.Windows.Forms.DataGridView();
            this.Package_Name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Version_ = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Installedfile_ = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblSolutionDetails = new System.Windows.Forms.Label();
            this.lblNativeAppDetails = new System.Windows.Forms.Label();
            this.labelSolutions = new System.Windows.Forms.Label();
            this.labelNativeApps = new System.Windows.Forms.Label();
            this.labelJobDetails = new System.Windows.Forms.Label();
            this.dataGridViewJobDetails = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.JobEndTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UserName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PauseReason = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.labelJobs = new System.Windows.Forms.Label();
            this.comboBoxJobType = new System.Windows.Forms.ComboBox();
            this.richTextBoxAllInfo = new System.Windows.Forms.RichTextBox();
            this.label2loadingmessage = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button1_ClearAll = new System.Windows.Forms.Button();
            this.buttonCopyAll = new System.Windows.Forms.Button();
            this.dataGridViewNativeApps = new System.Windows.Forms.DataGridView();
            this.Title = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label1loadingJob_details_message = new System.Windows.Forms.Label();
            this.button1Reload = new System.Windows.Forms.Button();
            this.comboBoxCopyFormat = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSolutions)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewJobDetails)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewNativeApps)).BeginInit();
            this.SuspendLayout();
            // 
            // labelDeviceAddress
            // 
            this.labelDeviceAddress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(75)))), ((int)(((byte)(139)))));
            this.labelDeviceAddress.Font = new System.Drawing.Font("Verdana", 9.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDeviceAddress.ForeColor = System.Drawing.SystemColors.Control;
            this.labelDeviceAddress.Location = new System.Drawing.Point(15, 9);
            this.labelDeviceAddress.Name = "labelDeviceAddress";
            this.labelDeviceAddress.Size = new System.Drawing.Size(118, 26);
            this.labelDeviceAddress.TabIndex = 0;
            this.labelDeviceAddress.Text = "Device Address:";
            this.labelDeviceAddress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelPassword
            // 
            this.labelPassword.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(75)))), ((int)(((byte)(139)))));
            this.labelPassword.Font = new System.Drawing.Font("Verdana", 9.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPassword.ForeColor = System.Drawing.SystemColors.Control;
            this.labelPassword.Location = new System.Drawing.Point(413, 7);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(86, 31);
            this.labelPassword.TabIndex = 1;
            this.labelPassword.Text = " Password:";
            this.labelPassword.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBoxIpAddress
            // 
            this.textBoxIpAddress.Location = new System.Drawing.Point(139, 9);
            this.textBoxIpAddress.Multiline = true;
            this.textBoxIpAddress.Name = "textBoxIpAddress";
            this.textBoxIpAddress.Size = new System.Drawing.Size(198, 28);
            this.textBoxIpAddress.TabIndex = 2;
            this.textBoxIpAddress.TextChanged += new System.EventHandler(this.textBoxIpAddress_TextChanged);
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(505, 10);
            this.textBoxPassword.Multiline = true;
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.Size = new System.Drawing.Size(202, 28);
            this.textBoxPassword.TabIndex = 3;
            this.textBoxPassword.TextChanged += new System.EventHandler(this.textBoxPassword_TextChanged);
            // 
            // buttonViewDetails
            // 
            this.buttonViewDetails.BackColor = System.Drawing.SystemColors.Control;
            this.buttonViewDetails.Font = new System.Drawing.Font("Verdana", 9.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonViewDetails.ForeColor = System.Drawing.SystemColors.ControlText;
            this.buttonViewDetails.Location = new System.Drawing.Point(802, 7);
            this.buttonViewDetails.Name = "buttonViewDetails";
            this.buttonViewDetails.Size = new System.Drawing.Size(103, 34);
            this.buttonViewDetails.TabIndex = 4;
            this.buttonViewDetails.Text = "ViewDetails";
            this.buttonViewDetails.UseVisualStyleBackColor = false;
            this.buttonViewDetails.Click += new System.EventHandler(this.buttonViewDetails_Click);
            // 
            // PackageName
            // 
            this.PackageName.HeaderText = "PackageName";
            this.PackageName.MinimumWidth = 6;
            this.PackageName.Name = "PackageName";
            this.PackageName.Width = 200;
            // 
            // Version
            // 
            this.Version.HeaderText = "Version";
            this.Version.MinimumWidth = 6;
            this.Version.Name = "Version";
            this.Version.Width = 125;
            // 
            // UUID
            // 
            this.UUID.HeaderText = "UUID";
            this.UUID.MinimumWidth = 6;
            this.UUID.Name = "UUID";
            this.UUID.Width = 250;
            // 
            // InstalledFile
            // 
            this.InstalledFile.HeaderText = "InstalledFile";
            this.InstalledFile.MinimumWidth = 6;
            this.InstalledFile.Name = "InstalledFile";
            this.InstalledFile.Width = 200;
            // 
            // dataGridViewSolutions
            // 
            this.dataGridViewSolutions.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewSolutions.BackgroundColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Olive;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewSolutions.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewSolutions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewSolutions.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Package_Name,
            this.Version_,
            this.Installedfile_});
            this.dataGridViewSolutions.Location = new System.Drawing.Point(671, 614);
            this.dataGridViewSolutions.Name = "dataGridViewSolutions";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Verdana", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.DarkKhaki;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewSolutions.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewSolutions.RowHeadersWidth = 51;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.DodgerBlue;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
            this.dataGridViewSolutions.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridViewSolutions.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dataGridViewSolutions.Size = new System.Drawing.Size(793, 92);
            this.dataGridViewSolutions.TabIndex = 10;
            this.dataGridViewSolutions.Visible = false;
            // 
            // Package_Name
            // 
            this.Package_Name.HeaderText = "PackageName";
            this.Package_Name.MinimumWidth = 6;
            this.Package_Name.Name = "Package_Name";
            // 
            // Version_
            // 
            this.Version_.HeaderText = "Version";
            this.Version_.MinimumWidth = 6;
            this.Version_.Name = "Version_";
            // 
            // Installedfile_
            // 
            this.Installedfile_.HeaderText = "Description";
            this.Installedfile_.MinimumWidth = 6;
            this.Installedfile_.Name = "Installedfile_";
            // 
            // lblSolutionDetails
            // 
            this.lblSolutionDetails.Font = new System.Drawing.Font("Stencil", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSolutionDetails.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(192)))));
            this.lblSolutionDetails.Location = new System.Drawing.Point(667, 567);
            this.lblSolutionDetails.Name = "lblSolutionDetails";
            this.lblSolutionDetails.Size = new System.Drawing.Size(400, 20);
            this.lblSolutionDetails.TabIndex = 13;
            this.lblSolutionDetails.Text = "Partner Solutions Details";
            this.lblSolutionDetails.Visible = false;
            // 
            // lblNativeAppDetails
            // 
            this.lblNativeAppDetails.Font = new System.Drawing.Font("Stencil", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNativeAppDetails.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(192)))));
            this.lblNativeAppDetails.Location = new System.Drawing.Point(667, 69);
            this.lblNativeAppDetails.Name = "lblNativeAppDetails";
            this.lblNativeAppDetails.Size = new System.Drawing.Size(250, 23);
            this.lblNativeAppDetails.TabIndex = 14;
            this.lblNativeAppDetails.Text = "Native App Details";
            this.lblNativeAppDetails.Visible = false;
            // 
            // labelSolutions
            // 
            this.labelSolutions.Font = new System.Drawing.Font("SimSun", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSolutions.ForeColor = System.Drawing.Color.Red;
            this.labelSolutions.Location = new System.Drawing.Point(668, 596);
            this.labelSolutions.Name = "labelSolutions";
            this.labelSolutions.Size = new System.Drawing.Size(340, 15);
            this.labelSolutions.TabIndex = 15;
            this.labelSolutions.Text = "No Solutions installed in this device";
            this.labelSolutions.Visible = false;
            // 
            // labelNativeApps
            // 
            this.labelNativeApps.Font = new System.Drawing.Font("SimSun", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelNativeApps.ForeColor = System.Drawing.Color.Red;
            this.labelNativeApps.Location = new System.Drawing.Point(668, 92);
            this.labelNativeApps.Name = "labelNativeApps";
            this.labelNativeApps.Size = new System.Drawing.Size(358, 18);
            this.labelNativeApps.TabIndex = 16;
            this.labelNativeApps.Text = "No Native apps available in this device";
            this.labelNativeApps.Visible = false;
            // 
            // labelJobDetails
            // 
            this.labelJobDetails.Font = new System.Drawing.Font("Stencil", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelJobDetails.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(192)))));
            this.labelJobDetails.Location = new System.Drawing.Point(19, 735);
            this.labelJobDetails.Name = "labelJobDetails";
            this.labelJobDetails.Size = new System.Drawing.Size(149, 20);
            this.labelJobDetails.TabIndex = 17;
            this.labelJobDetails.Text = "Job Details";
            this.labelJobDetails.Visible = false;
            // 
            // dataGridViewJobDetails
            // 
            this.dataGridViewJobDetails.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewJobDetails.BackgroundColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewJobDetails.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dataGridViewJobDetails.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewJobDetails.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn5,
            this.dataGridViewTextBoxColumn6,
            this.dataGridViewTextBoxColumn7,
            this.dataGridViewTextBoxColumn8,
            this.JobEndTime,
            this.UserName,
            this.PauseReason});
            this.dataGridViewJobDetails.Location = new System.Drawing.Point(23, 782);
            this.dataGridViewJobDetails.Name = "dataGridViewJobDetails";
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewJobDetails.RowHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.dataGridViewJobDetails.RowHeadersWidth = 51;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle6.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.DodgerBlue;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.Color.White;
            this.dataGridViewJobDetails.RowsDefaultCellStyle = dataGridViewCellStyle6;
            this.dataGridViewJobDetails.Size = new System.Drawing.Size(1441, 102);
            this.dataGridViewJobDetails.TabIndex = 18;
            this.dataGridViewJobDetails.Visible = false;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.HeaderText = "JobId";
            this.dataGridViewTextBoxColumn5.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            // 
            // dataGridViewTextBoxColumn6
            // 
            this.dataGridViewTextBoxColumn6.HeaderText = "JobName";
            this.dataGridViewTextBoxColumn6.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            // 
            // dataGridViewTextBoxColumn7
            // 
            this.dataGridViewTextBoxColumn7.HeaderText = "JobType";
            this.dataGridViewTextBoxColumn7.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
            // 
            // dataGridViewTextBoxColumn8
            // 
            this.dataGridViewTextBoxColumn8.HeaderText = "JobStartTime";
            this.dataGridViewTextBoxColumn8.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
            // 
            // JobEndTime
            // 
            this.JobEndTime.HeaderText = "JobState";
            this.JobEndTime.MinimumWidth = 6;
            this.JobEndTime.Name = "JobEndTime";
            // 
            // UserName
            // 
            this.UserName.HeaderText = "UserName";
            this.UserName.MinimumWidth = 6;
            this.UserName.Name = "UserName";
            // 
            // PauseReason
            // 
            this.PauseReason.HeaderText = "EndTime";
            this.PauseReason.MinimumWidth = 6;
            this.PauseReason.Name = "PauseReason";
            // 
            // labelJobs
            // 
            this.labelJobs.AutoSize = true;
            this.labelJobs.Font = new System.Drawing.Font("SimSun", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelJobs.ForeColor = System.Drawing.Color.Red;
            this.labelJobs.Location = new System.Drawing.Point(20, 764);
            this.labelJobs.Name = "labelJobs";
            this.labelJobs.Size = new System.Drawing.Size(412, 15);
            this.labelJobs.TabIndex = 19;
            this.labelJobs.Text = "Currently No jobs are existing in this device";
            this.labelJobs.Visible = false;
            // 
            // comboBoxJobType
            // 
            this.comboBoxJobType.FormattingEnabled = true;
            this.comboBoxJobType.Items.AddRange(new object[] {
            "All Jobs",
            "Print",
            "Scan",
            "Fax",
            "PrintFromJobStorage",
            "Copy"});
            this.comboBoxJobType.Location = new System.Drawing.Point(173, 734);
            this.comboBoxJobType.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxJobType.Name = "comboBoxJobType";
            this.comboBoxJobType.Size = new System.Drawing.Size(132, 21);
            this.comboBoxJobType.TabIndex = 20;
            this.comboBoxJobType.Text = "All Jobs";
            this.comboBoxJobType.Visible = false;
            this.comboBoxJobType.SelectedIndexChanged += new System.EventHandler(this.comboBoxJobType_SelectedIndexChanged);
            // 
            // richTextBoxAllInfo
            // 
            this.richTextBoxAllInfo.Font = new System.Drawing.Font("SimSun", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxAllInfo.ForeColor = System.Drawing.Color.White;
            this.richTextBoxAllInfo.Location = new System.Drawing.Point(23, 89);
            this.richTextBoxAllInfo.Name = "richTextBoxAllInfo";
            this.richTextBoxAllInfo.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.richTextBoxAllInfo.Size = new System.Drawing.Size(615, 631);
            this.richTextBoxAllInfo.TabIndex = 23;
            this.richTextBoxAllInfo.Text = "";
            this.richTextBoxAllInfo.Visible = false;
            // 
            // label2loadingmessage
            // 
            this.label2loadingmessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2loadingmessage.ForeColor = System.Drawing.SystemColors.Highlight;
            this.label2loadingmessage.Location = new System.Drawing.Point(12, 61);
            this.label2loadingmessage.Name = "label2loadingmessage";
            this.label2loadingmessage.Size = new System.Drawing.Size(478, 25);
            this.label2loadingmessage.TabIndex = 28;
            this.label2loadingmessage.Text = "Please wait.. Printer information is loading!";
            this.label2loadingmessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label2loadingmessage.Visible = false;
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(75)))), ((int)(((byte)(139)))));
            this.panel1.Controls.Add(this.comboBoxCopyFormat);
            this.panel1.Controls.Add(this.button1_ClearAll);
            this.panel1.Controls.Add(this.buttonCopyAll);
            this.panel1.Controls.Add(this.labelDeviceAddress);
            this.panel1.Controls.Add(this.textBoxIpAddress);
            this.panel1.Controls.Add(this.labelPassword);
            this.panel1.Controls.Add(this.textBoxPassword);
            this.panel1.Controls.Add(this.buttonViewDetails);
            this.panel1.Location = new System.Drawing.Point(23, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1455, 44);
            this.panel1.TabIndex = 29;
            // 
            // button1_ClearAll
            // 
            this.button1_ClearAll.BackColor = System.Drawing.SystemColors.Control;
            this.button1_ClearAll.Font = new System.Drawing.Font("Verdana", 9.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1_ClearAll.ForeColor = System.Drawing.SystemColors.ControlText;
            this.button1_ClearAll.Location = new System.Drawing.Point(985, 7);
            this.button1_ClearAll.Name = "button1_ClearAll";
            this.button1_ClearAll.Size = new System.Drawing.Size(94, 32);
            this.button1_ClearAll.TabIndex = 39;
            this.button1_ClearAll.Text = "Clear All";
            this.button1_ClearAll.UseVisualStyleBackColor = false;
            this.button1_ClearAll.Click += new System.EventHandler(this.button1_ClearAll_Click_1);
            // 
            // buttonCopyAll
            // 
            this.buttonCopyAll.BackColor = System.Drawing.SystemColors.Control;
            this.buttonCopyAll.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCopyAll.Location = new System.Drawing.Point(1338, 5);
            this.buttonCopyAll.Name = "buttonCopyAll";
            this.buttonCopyAll.Size = new System.Drawing.Size(103, 34);
            this.buttonCopyAll.TabIndex = 33;
            this.buttonCopyAll.Text = "Export";
            this.buttonCopyAll.UseVisualStyleBackColor = false;
            this.buttonCopyAll.Click += new System.EventHandler(this.buttonCopyAll_Click);
            // 
            // dataGridViewNativeApps
            // 
            this.dataGridViewNativeApps.BackgroundColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewNativeApps.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.dataGridViewNativeApps.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewNativeApps.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Title,
            this.Description});
            this.dataGridViewNativeApps.Location = new System.Drawing.Point(671, 113);
            this.dataGridViewNativeApps.Name = "dataGridViewNativeApps";
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Verdana", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.Info;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewNativeApps.RowHeadersDefaultCellStyle = dataGridViewCellStyle8;
            dataGridViewCellStyle9.BackColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle9.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.Color.DodgerBlue;
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.Color.White;
            this.dataGridViewNativeApps.RowsDefaultCellStyle = dataGridViewCellStyle9;
            this.dataGridViewNativeApps.Size = new System.Drawing.Size(793, 417);
            this.dataGridViewNativeApps.TabIndex = 25;
            this.dataGridViewNativeApps.Visible = false;
            // 
            // Title
            // 
            this.Title.HeaderText = "Title";
            this.Title.Name = "Title";
            this.Title.Width = 280;
            // 
            // Description
            // 
            this.Description.HeaderText = "Description";
            this.Description.Name = "Description";
            this.Description.Width = 450;
            // 
            // label1loadingJob_details_message
            // 
            this.label1loadingJob_details_message.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1loadingJob_details_message.ForeColor = System.Drawing.SystemColors.Highlight;
            this.label1loadingJob_details_message.Location = new System.Drawing.Point(344, 731);
            this.label1loadingJob_details_message.Name = "label1loadingJob_details_message";
            this.label1loadingJob_details_message.Size = new System.Drawing.Size(136, 27);
            this.label1loadingJob_details_message.TabIndex = 30;
            this.label1loadingJob_details_message.Text = "Loading!........";
            this.label1loadingJob_details_message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label1loadingJob_details_message.Visible = false;
            // 
            // button1Reload
            // 
            this.button1Reload.BackColor = System.Drawing.SystemColors.Window;
            this.button1Reload.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.button1Reload.FlatAppearance.BorderSize = 0;
            this.button1Reload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1Reload.Font = new System.Drawing.Font("Verdana", 9.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1Reload.ForeColor = System.Drawing.SystemColors.ControlText;
            this.button1Reload.Image = ((System.Drawing.Image)(resources.GetObject("button1Reload.Image")));
            this.button1Reload.Location = new System.Drawing.Point(319, 731);
            this.button1Reload.Name = "button1Reload";
            this.button1Reload.Size = new System.Drawing.Size(19, 26);
            this.button1Reload.TabIndex = 41;
            this.button1Reload.UseVisualStyleBackColor = false;
            this.button1Reload.Visible = false;
            this.button1Reload.Click += new System.EventHandler(this.button1Reload_Click);
            // 
            // comboBoxCopyFormat
            // 
            this.comboBoxCopyFormat.FormattingEnabled = true;
            this.comboBoxCopyFormat.Items.AddRange(new object[] {
            "Excel",
            "Notepad"});
            this.comboBoxCopyFormat.Location = new System.Drawing.Point(1192, 13);
            this.comboBoxCopyFormat.Name = "comboBoxCopyFormat";
            this.comboBoxCopyFormat.Size = new System.Drawing.Size(127, 21);
            this.comboBoxCopyFormat.TabIndex = 40;
            this.comboBoxCopyFormat.Text = "Select File Type";
            this.comboBoxCopyFormat.SelectedIndexChanged += new System.EventHandler(this.comboBoxCopyFormat_SelectedIndexChanged_1);
            // 
            // DeviceDetailsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(1502, 1061);
            this.Controls.Add(this.button1Reload);
            this.Controls.Add(this.lblNativeAppDetails);
            this.Controls.Add(this.label1loadingJob_details_message);
            this.Controls.Add(this.richTextBoxAllInfo);
            this.Controls.Add(this.dataGridViewJobDetails);
            this.Controls.Add(this.dataGridViewSolutions);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.dataGridViewNativeApps);
            this.Controls.Add(this.labelNativeApps);
            this.Controls.Add(this.labelJobs);
            this.Controls.Add(this.label2loadingmessage);
            this.Controls.Add(this.lblSolutionDetails);
            this.Controls.Add(this.labelJobDetails);
            this.Controls.Add(this.labelSolutions);
            this.Controls.Add(this.comboBoxJobType);
            this.MaximizeBox = false;
            this.Name = "DeviceDetailsForm";
            this.Text = "Printer Details";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSolutions)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewJobDetails)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewNativeApps)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelDeviceAddress;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.TextBox textBoxIpAddress;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Button buttonViewDetails;
        private System.Windows.Forms.DataGridViewTextBoxColumn PackageName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Version;
        private System.Windows.Forms.DataGridViewTextBoxColumn UUID;
        private System.Windows.Forms.DataGridViewTextBoxColumn InstalledFile;
        private System.Windows.Forms.DataGridView dataGridViewSolutions;
        private System.Windows.Forms.Label lblSolutionDetails;
        private System.Windows.Forms.Label lblNativeAppDetails;
        private System.Windows.Forms.Label labelSolutions;
        private System.Windows.Forms.Label labelNativeApps;
        private System.Windows.Forms.Label labelJobDetails;
        private System.Windows.Forms.DataGridView dataGridViewJobDetails;
        private System.Windows.Forms.Label labelJobs;
        private System.Windows.Forms.ComboBox comboBoxJobType;
        private System.Windows.Forms.RichTextBox richTextBoxAllInfo;
        private System.Windows.Forms.Label label2loadingmessage;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonCopyAll;
        private System.Windows.Forms.DataGridView dataGridViewNativeApps;
        private System.Windows.Forms.DataGridViewTextBoxColumn Title;
        private System.Windows.Forms.DataGridViewTextBoxColumn Description;
        private System.Windows.Forms.DataGridViewTextBoxColumn Package_Name;
        private System.Windows.Forms.DataGridViewTextBoxColumn Version_;
        private System.Windows.Forms.DataGridViewTextBoxColumn Installedfile_;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
        private System.Windows.Forms.DataGridViewTextBoxColumn JobEndTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn UserName;
        private System.Windows.Forms.DataGridViewTextBoxColumn PauseReason;
        private System.Windows.Forms.Label label1loadingJob_details_message;
        private System.Windows.Forms.Button button1_ClearAll;
        private System.Windows.Forms.Button button1Reload;
        private System.Windows.Forms.ComboBox comboBoxCopyFormat;
    }
}

