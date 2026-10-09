namespace HP.GFriend.Tool
{
    partial class MainForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.flowLayoutPanelPackages = new System.Windows.Forms.FlowLayoutPanel();
            this.textBoxIp = new System.Windows.Forms.TextBox();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.textBoxPw = new System.Windows.Forms.TextBox();
            this.textBoxId = new System.Windows.Forms.TextBox();
            this.labelDeviceAddress = new System.Windows.Forms.Label();
            this.labelDeviceUserId = new System.Windows.Forms.Label();
            this.labelUserPw = new System.Windows.Forms.Label();
            this.textBoxUnknown = new System.Windows.Forms.TextBox();
            this.textBoxNotInstalled = new System.Windows.Forms.TextBox();
            this.textBoxInstalled = new System.Windows.Forms.TextBox();
            this.textBoxDeveloperID = new System.Windows.Forms.TextBox();
            this.labelDeveloperID = new System.Windows.Forms.Label();
            this.buttonInstallWDA = new System.Windows.Forms.Button();
            this.checkBoxPersonal = new System.Windows.Forms.CheckBox();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // flowLayoutPanelPackages
            // 
            this.flowLayoutPanelPackages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanelPackages.Location = new System.Drawing.Point(15, 77);
            this.flowLayoutPanelPackages.Name = "flowLayoutPanelPackages";
            this.flowLayoutPanelPackages.Size = new System.Drawing.Size(698, 10);
            this.flowLayoutPanelPackages.TabIndex = 0;
            // 
            // textBoxIp
            // 
            this.textBoxIp.Location = new System.Drawing.Point(91, 25);
            this.textBoxIp.Name = "textBoxIp";
            this.textBoxIp.Size = new System.Drawing.Size(100, 20);
            this.textBoxIp.TabIndex = 1;
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(561, 23);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(159, 23);
            this.buttonConnect.TabIndex = 4;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // textBoxPw
            // 
            this.textBoxPw.Location = new System.Drawing.Point(454, 25);
            this.textBoxPw.Name = "textBoxPw";
            this.textBoxPw.Size = new System.Drawing.Size(100, 20);
            this.textBoxPw.TabIndex = 3;
            // 
            // textBoxId
            // 
            this.textBoxId.Location = new System.Drawing.Point(258, 25);
            this.textBoxId.Name = "textBoxId";
            this.textBoxId.Size = new System.Drawing.Size(100, 20);
            this.textBoxId.TabIndex = 2;
            // 
            // labelDeviceAddress
            // 
            this.labelDeviceAddress.AutoSize = true;
            this.labelDeviceAddress.Location = new System.Drawing.Point(12, 28);
            this.labelDeviceAddress.Name = "labelDeviceAddress";
            this.labelDeviceAddress.Size = new System.Drawing.Size(73, 13);
            this.labelDeviceAddress.TabIndex = 5;
            this.labelDeviceAddress.Text = "AppiumServer";
            // 
            // labelDeviceUserId
            // 
            this.labelDeviceUserId.AutoSize = true;
            this.labelDeviceUserId.Location = new System.Drawing.Point(212, 28);
            this.labelDeviceUserId.Name = "labelDeviceUserId";
            this.labelDeviceUserId.Size = new System.Drawing.Size(40, 13);
            this.labelDeviceUserId.TabIndex = 6;
            this.labelDeviceUserId.Text = "UserID";
            // 
            // labelUserPw
            // 
            this.labelUserPw.AutoSize = true;
            this.labelUserPw.Location = new System.Drawing.Point(373, 28);
            this.labelUserPw.Name = "labelUserPw";
            this.labelUserPw.Size = new System.Drawing.Size(75, 13);
            this.labelUserPw.TabIndex = 7;
            this.labelUserPw.Text = "UserPassword";
            // 
            // textBoxUnknown
            // 
            this.textBoxUnknown.BackColor = System.Drawing.Color.Gray;
            this.textBoxUnknown.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxUnknown.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxUnknown.ForeColor = System.Drawing.Color.Black;
            this.textBoxUnknown.Location = new System.Drawing.Point(632, 4);
            this.textBoxUnknown.Name = "textBoxUnknown";
            this.textBoxUnknown.Size = new System.Drawing.Size(88, 13);
            this.textBoxUnknown.TabIndex = 14;
            this.textBoxUnknown.TabStop = false;
            this.textBoxUnknown.Text = "Unknown";
            this.textBoxUnknown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxNotInstalled
            // 
            this.textBoxNotInstalled.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.textBoxNotInstalled.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxNotInstalled.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxNotInstalled.ForeColor = System.Drawing.Color.Black;
            this.textBoxNotInstalled.Location = new System.Drawing.Point(539, 4);
            this.textBoxNotInstalled.Name = "textBoxNotInstalled";
            this.textBoxNotInstalled.Size = new System.Drawing.Size(88, 13);
            this.textBoxNotInstalled.TabIndex = 13;
            this.textBoxNotInstalled.TabStop = false;
            this.textBoxNotInstalled.Text = "Not Installed";
            this.textBoxNotInstalled.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxInstalled
            // 
            this.textBoxInstalled.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.textBoxInstalled.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxInstalled.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxInstalled.ForeColor = System.Drawing.Color.Black;
            this.textBoxInstalled.Location = new System.Drawing.Point(445, 4);
            this.textBoxInstalled.Name = "textBoxInstalled";
            this.textBoxInstalled.Size = new System.Drawing.Size(88, 13);
            this.textBoxInstalled.TabIndex = 12;
            this.textBoxInstalled.TabStop = false;
            this.textBoxInstalled.Text = "Installed";
            this.textBoxInstalled.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxDeveloperID
            // 
            this.textBoxDeveloperID.Location = new System.Drawing.Point(616, 53);
            this.textBoxDeveloperID.Name = "textBoxDeveloperID";
            this.textBoxDeveloperID.Size = new System.Drawing.Size(100, 20);
            this.textBoxDeveloperID.TabIndex = 15;
            this.textBoxDeveloperID.Visible = false;
            // 
            // labelDeveloperID
            // 
            this.labelDeveloperID.AutoSize = true;
            this.labelDeveloperID.Location = new System.Drawing.Point(451, 56);
            this.labelDeveloperID.Name = "labelDeveloperID";
            this.labelDeveloperID.Size = new System.Drawing.Size(156, 13);
            this.labelDeveloperID.TabIndex = 16;
            this.labelDeveloperID.Text = "DeveloperID (No email, only ID)";
            this.labelDeveloperID.Visible = false;
            // 
            // buttonInstallWDA
            // 
            this.buttonInstallWDA.Location = new System.Drawing.Point(15, 50);
            this.buttonInstallWDA.Name = "buttonInstallWDA";
            this.buttonInstallWDA.Size = new System.Drawing.Size(146, 23);
            this.buttonInstallWDA.TabIndex = 17;
            this.buttonInstallWDA.Text = "Install WDA";
            this.buttonInstallWDA.UseVisualStyleBackColor = true;
            this.buttonInstallWDA.Click += new System.EventHandler(this.buttonInstallWDA_Click);
            // 
            // checkBoxPersonal
            // 
            this.checkBoxPersonal.AutoSize = true;
            this.checkBoxPersonal.Location = new System.Drawing.Point(167, 52);
            this.checkBoxPersonal.Name = "checkBoxPersonal";
            this.checkBoxPersonal.Size = new System.Drawing.Size(191, 17);
            this.checkBoxPersonal.TabIndex = 18;
            this.checkBoxPersonal.Text = "Install WDA without register device";
            this.checkBoxPersonal.UseVisualStyleBackColor = true;
            this.checkBoxPersonal.CheckedChanged += new System.EventHandler(this.checkBoxPersonal_CheckedChanged);
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOutput.Location = new System.Drawing.Point(15, 91);
            this.textBoxOutput.Multiline = true;
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxOutput.Size = new System.Drawing.Size(700, 98);
            this.textBoxOutput.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(721, 201);
            this.Controls.Add(this.textBoxOutput);
            this.Controls.Add(this.checkBoxPersonal);
            this.Controls.Add(this.buttonInstallWDA);
            this.Controls.Add(this.labelDeveloperID);
            this.Controls.Add(this.textBoxDeveloperID);
            this.Controls.Add(this.textBoxUnknown);
            this.Controls.Add(this.textBoxNotInstalled);
            this.Controls.Add(this.textBoxInstalled);
            this.Controls.Add(this.labelUserPw);
            this.Controls.Add(this.labelDeviceUserId);
            this.Controls.Add(this.labelDeviceAddress);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.textBoxPw);
            this.Controls.Add(this.textBoxId);
            this.Controls.Add(this.textBoxIp);
            this.Controls.Add(this.flowLayoutPanelPackages);
            this.Name = "MainForm";
            this.Text = "Apple";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelPackages;
        private System.Windows.Forms.TextBox textBoxIp;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.TextBox textBoxPw;
        private System.Windows.Forms.TextBox textBoxId;
        private System.Windows.Forms.Label labelDeviceAddress;
        private System.Windows.Forms.Label labelDeviceUserId;
        private System.Windows.Forms.Label labelUserPw;
        private System.Windows.Forms.TextBox textBoxUnknown;
        private System.Windows.Forms.TextBox textBoxNotInstalled;
        private System.Windows.Forms.TextBox textBoxInstalled;
        private System.Windows.Forms.TextBox textBoxDeveloperID;
        private System.Windows.Forms.Label labelDeveloperID;
        private System.Windows.Forms.Button buttonInstallWDA;
        private System.Windows.Forms.CheckBox checkBoxPersonal;
        private System.Windows.Forms.TextBox textBoxOutput;
    }
}

