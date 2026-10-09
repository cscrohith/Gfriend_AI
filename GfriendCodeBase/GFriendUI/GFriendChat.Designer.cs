namespace HP.GFriend.UI
{
    partial class GFriendChat
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GFriendChat));
            this.messagePanel = new System.Windows.Forms.FlowLayoutPanel();
            this.userInputPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.userInputTextBox = new System.Windows.Forms.TextBox();
            this.sendButton = new System.Windows.Forms.Button();
            this.userInputPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // messagePanel
            // 
            this.messagePanel.AutoScroll = true;
            this.messagePanel.BackColor = System.Drawing.SystemColors.Window;
            this.messagePanel.Location = new System.Drawing.Point(6, 3);
            this.messagePanel.Name = "messagePanel";
            this.messagePanel.Size = new System.Drawing.Size(584, 574);
            this.messagePanel.TabIndex = 6;
            // 
            // userInputPanel
            // 
            this.userInputPanel.BackColor = System.Drawing.SystemColors.Window;
            this.userInputPanel.Controls.Add(this.userInputTextBox);
            this.userInputPanel.Controls.Add(this.sendButton);
            this.userInputPanel.Location = new System.Drawing.Point(0, 583);
            this.userInputPanel.Name = "userInputPanel";
            this.userInputPanel.Size = new System.Drawing.Size(590, 65);
            this.userInputPanel.TabIndex = 7;
            // 
            // userInputTextBox
            // 
            this.userInputTextBox.AutoCompleteCustomSource.AddRange(new string[] {
            "Manual",
            "Keywords"});
            this.userInputTextBox.Location = new System.Drawing.Point(3, 3);
            this.userInputTextBox.Multiline = true;
            this.userInputTextBox.Name = "userInputTextBox";
            this.userInputTextBox.Size = new System.Drawing.Size(528, 55);
            this.userInputTextBox.TabIndex = 4;
            this.userInputTextBox.Click += new System.EventHandler(this.userTextBoxInput_Click);
            this.userInputTextBox.Enter += new System.EventHandler(this.userInputTextBox_Enter);
            this.userInputTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.userInputTextBox_KeyDown);
            // 
            // sendButton
            // 
            this.sendButton.BackColor = System.Drawing.SystemColors.Window;
            this.sendButton.Image = ((System.Drawing.Image)(resources.GetObject("sendButton.Image")));
            this.sendButton.Location = new System.Drawing.Point(537, 3);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(50, 55);
            this.sendButton.TabIndex = 5;
            this.sendButton.UseVisualStyleBackColor = false;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // GFriendChat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(593, 649);
            this.Controls.Add(this.messagePanel);
            this.Controls.Add(this.userInputPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "GFriendChat";
            this.Text = "GFriend Chat";
            this.Load += new System.EventHandler(this.GFriendChat_load);
            this.userInputPanel.ResumeLayout(false);
            this.userInputPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.FlowLayoutPanel messagePanel;
        private System.Windows.Forms.FlowLayoutPanel userInputPanel;
        private System.Windows.Forms.TextBox userInputTextBox;
        private System.Windows.Forms.Button sendButton;
    }
}