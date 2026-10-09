
namespace HP.GFriend.Utils.Git
{
    partial class GitControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBoxCommitMessage = new System.Windows.Forms.TextBox();
            this.checkedListBoxFileChanged = new System.Windows.Forms.CheckedListBox();
            this.labelChangeList = new System.Windows.Forms.Label();
            this.buttonCommitPush = new System.Windows.Forms.Button();
            this.labelBranch = new System.Windows.Forms.Label();
            this.labelBranchName = new System.Windows.Forms.Label();
            this.pictureBoxSetting = new System.Windows.Forms.PictureBox();
            this.pictureBoxPull = new System.Windows.Forms.PictureBox();
            this.pictureBoxSelectAll = new System.Windows.Forms.PictureBox();
            this.pictureBoxRefresh = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSetting)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPull)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSelectAll)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxRefresh)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxCommitMessage
            // 
            this.textBoxCommitMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCommitMessage.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxCommitMessage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(75)))), ((int)(((byte)(139)))));
            this.textBoxCommitMessage.Location = new System.Drawing.Point(3, 46);
            this.textBoxCommitMessage.Multiline = true;
            this.textBoxCommitMessage.Name = "textBoxCommitMessage";
            this.textBoxCommitMessage.Size = new System.Drawing.Size(274, 65);
            this.textBoxCommitMessage.TabIndex = 0;
            this.textBoxCommitMessage.Text = "Enter change description";
            this.textBoxCommitMessage.Click += new System.EventHandler(this.textBoxCommitMessage_Click);
            // 
            // checkedListBoxFileChanged
            // 
            this.checkedListBoxFileChanged.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedListBoxFileChanged.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkedListBoxFileChanged.FormattingEnabled = true;
            this.checkedListBoxFileChanged.Location = new System.Drawing.Point(3, 173);
            this.checkedListBoxFileChanged.Name = "checkedListBoxFileChanged";
            this.checkedListBoxFileChanged.Size = new System.Drawing.Size(274, 244);
            this.checkedListBoxFileChanged.TabIndex = 1;
            // 
            // labelChangeList
            // 
            this.labelChangeList.AutoSize = true;
            this.labelChangeList.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelChangeList.Location = new System.Drawing.Point(3, 157);
            this.labelChangeList.Name = "labelChangeList";
            this.labelChangeList.Size = new System.Drawing.Size(82, 13);
            this.labelChangeList.TabIndex = 2;
            this.labelChangeList.Text = "Change List";
            // 
            // buttonCommitPush
            // 
            this.buttonCommitPush.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCommitPush.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(192)))));
            this.buttonCommitPush.Enabled = false;
            this.buttonCommitPush.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCommitPush.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCommitPush.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(75)))), ((int)(((byte)(139)))));
            this.buttonCommitPush.Location = new System.Drawing.Point(131, 117);
            this.buttonCommitPush.Name = "buttonCommitPush";
            this.buttonCommitPush.Size = new System.Drawing.Size(146, 23);
            this.buttonCommitPush.TabIndex = 3;
            this.buttonCommitPush.Text = "Upload to Github";
            this.buttonCommitPush.UseVisualStyleBackColor = false;
            this.buttonCommitPush.Click += new System.EventHandler(this.buttonCommitPush_Click);
            // 
            // labelBranch
            // 
            this.labelBranch.AutoSize = true;
            this.labelBranch.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelBranch.Location = new System.Drawing.Point(0, 11);
            this.labelBranch.Name = "labelBranch";
            this.labelBranch.Size = new System.Drawing.Size(64, 13);
            this.labelBranch.TabIndex = 4;
            this.labelBranch.Text = "Branch : ";
            // 
            // labelBranchName
            // 
            this.labelBranchName.AutoSize = true;
            this.labelBranchName.Font = new System.Drawing.Font("Verdana", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelBranchName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(192)))));
            this.labelBranchName.Location = new System.Drawing.Point(56, 11);
            this.labelBranchName.Name = "labelBranchName";
            this.labelBranchName.Size = new System.Drawing.Size(53, 13);
            this.labelBranchName.TabIndex = 5;
            this.labelBranchName.Text = "master";
            this.labelBranchName.Click += new System.EventHandler(this.labelBranchName_Click);
            // 
            // pictureBoxSetting
            // 
            this.pictureBoxSetting.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxSetting.Image = global::HP.GFriend.Utils.Git.Properties.Resources.setting;
            this.pictureBoxSetting.Location = new System.Drawing.Point(250, 2);
            this.pictureBoxSetting.Name = "pictureBoxSetting";
            this.pictureBoxSetting.Size = new System.Drawing.Size(23, 23);
            this.pictureBoxSetting.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxSetting.TabIndex = 10;
            this.pictureBoxSetting.TabStop = false;
            this.pictureBoxSetting.Click += new System.EventHandler(this.pictureBoxSetting_Click);
            this.pictureBoxSetting.MouseHover += new System.EventHandler(this.Icon_MouseOver);
            // 
            // pictureBoxPull
            // 
            this.pictureBoxPull.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxPull.Image = global::HP.GFriend.Utils.Git.Properties.Resources.Pull;
            this.pictureBoxPull.Location = new System.Drawing.Point(205, 6);
            this.pictureBoxPull.Name = "pictureBoxPull";
            this.pictureBoxPull.Size = new System.Drawing.Size(16, 16);
            this.pictureBoxPull.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxPull.TabIndex = 9;
            this.pictureBoxPull.TabStop = false;
            this.pictureBoxPull.Click += new System.EventHandler(this.pictureBoxPull_Click);
            this.pictureBoxPull.MouseHover += new System.EventHandler(this.Icon_MouseOver);
            // 
            // pictureBoxSelectAll
            // 
            this.pictureBoxSelectAll.Image = global::HP.GFriend.Utils.Git.Properties.Resources.CheckboxCheckAll_16x;
            this.pictureBoxSelectAll.Location = new System.Drawing.Point(91, 154);
            this.pictureBoxSelectAll.Name = "pictureBoxSelectAll";
            this.pictureBoxSelectAll.Size = new System.Drawing.Size(24, 16);
            this.pictureBoxSelectAll.TabIndex = 8;
            this.pictureBoxSelectAll.TabStop = false;
            this.pictureBoxSelectAll.Click += new System.EventHandler(this.buttonSelectAll_Click);
            this.pictureBoxSelectAll.MouseHover += new System.EventHandler(this.Icon_MouseOver);
            // 
            // pictureBoxRefresh
            // 
            this.pictureBoxRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxRefresh.Image = global::HP.GFriend.Utils.Git.Properties.Resources.refresh;
            this.pictureBoxRefresh.Location = new System.Drawing.Point(229, 5);
            this.pictureBoxRefresh.Name = "pictureBoxRefresh";
            this.pictureBoxRefresh.Size = new System.Drawing.Size(18, 18);
            this.pictureBoxRefresh.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxRefresh.TabIndex = 7;
            this.pictureBoxRefresh.TabStop = false;
            this.pictureBoxRefresh.Click += new System.EventHandler(this.pictureBox1_Click);
            this.pictureBoxRefresh.MouseHover += new System.EventHandler(this.Icon_MouseOver);
            // 
            // GitControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.Controls.Add(this.pictureBoxSetting);
            this.Controls.Add(this.pictureBoxPull);
            this.Controls.Add(this.pictureBoxSelectAll);
            this.Controls.Add(this.pictureBoxRefresh);
            this.Controls.Add(this.labelBranchName);
            this.Controls.Add(this.labelBranch);
            this.Controls.Add(this.buttonCommitPush);
            this.Controls.Add(this.labelChangeList);
            this.Controls.Add(this.checkedListBoxFileChanged);
            this.Controls.Add(this.textBoxCommitMessage);
            this.ForeColor = System.Drawing.Color.Black;
            this.Name = "GitControl";
            this.Size = new System.Drawing.Size(280, 420);
            this.Enter += new System.EventHandler(this.GitControl_Enter);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSetting)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPull)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSelectAll)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxRefresh)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxCommitMessage;
        private System.Windows.Forms.CheckedListBox checkedListBoxFileChanged;
        private System.Windows.Forms.Label labelChangeList;
        private System.Windows.Forms.Button buttonCommitPush;
        private System.Windows.Forms.Label labelBranch;
        private System.Windows.Forms.Label labelBranchName;
        private System.Windows.Forms.PictureBox pictureBoxRefresh;
        private System.Windows.Forms.PictureBox pictureBoxSelectAll;
        private System.Windows.Forms.PictureBox pictureBoxPull;
        private System.Windows.Forms.PictureBox pictureBoxSetting;
    }
}
