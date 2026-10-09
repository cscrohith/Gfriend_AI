namespace HP.GFriend.Tool
{
    partial class MacEnvSetupControl
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

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBoxPackageName = new System.Windows.Forms.TextBox();
            this.textBoxPackageVersion = new System.Windows.Forms.TextBox();
            this.buttonInstall = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxPackageName
            // 
            this.textBoxPackageName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(192)))));
            this.textBoxPackageName.Location = new System.Drawing.Point(3, 3);
            this.textBoxPackageName.Name = "textBoxPackageName";
            this.textBoxPackageName.Size = new System.Drawing.Size(121, 20);
            this.textBoxPackageName.TabIndex = 0;
            // 
            // textBoxPackageVersion
            // 
            this.textBoxPackageVersion.Location = new System.Drawing.Point(130, 3);
            this.textBoxPackageVersion.Name = "textBoxPackageVersion";
            this.textBoxPackageVersion.Size = new System.Drawing.Size(195, 20);
            this.textBoxPackageVersion.TabIndex = 1;
            // 
            // buttonInstall
            // 
            this.buttonInstall.Location = new System.Drawing.Point(331, 3);
            this.buttonInstall.Name = "buttonInstall";
            this.buttonInstall.Size = new System.Drawing.Size(144, 36);
            this.buttonInstall.TabIndex = 2;
            this.buttonInstall.Text = "Install";
            this.buttonInstall.UseVisualStyleBackColor = true;
            this.buttonInstall.Click += new System.EventHandler(this.button1_Click);
            // 
            // MacEnvSetupControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonInstall);
            this.Controls.Add(this.textBoxPackageVersion);
            this.Controls.Add(this.textBoxPackageName);
            this.Name = "MacEnvSetupControl";
            this.Size = new System.Drawing.Size(478, 52);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxPackageName;
        private System.Windows.Forms.TextBox textBoxPackageVersion;
        private System.Windows.Forms.Button buttonInstall;
    }
}
