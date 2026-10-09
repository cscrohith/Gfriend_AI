namespace HP.GFriend.Keywords
{
    partial class UserInteractionForm
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
            this.textBoxMessage = new System.Windows.Forms.TextBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonPass = new System.Windows.Forms.Button();
            this.buttonFail = new System.Windows.Forms.Button();
            this.textBoxReason = new System.Windows.Forms.TextBox();
            this.labelReason = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBoxMessage
            // 
            this.textBoxMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxMessage.Location = new System.Drawing.Point(12, 12);
            this.textBoxMessage.Multiline = true;
            this.textBoxMessage.Name = "textBoxMessage";
            this.textBoxMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxMessage.Size = new System.Drawing.Size(477, 73);
            this.textBoxMessage.TabIndex = 0;
            this.textBoxMessage.TabStop = false;
            this.textBoxMessage.Text = "message";
            // 
            // buttonOK
            // 
            this.buttonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(192)))));
            this.buttonOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonOK.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.buttonOK.Location = new System.Drawing.Point(413, 125);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(76, 29);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = false;
            // 
            // buttonPass
            // 
            this.buttonPass.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPass.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(192)))));
            this.buttonPass.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonPass.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.buttonPass.Location = new System.Drawing.Point(413, 125);
            this.buttonPass.Name = "buttonPass";
            this.buttonPass.Size = new System.Drawing.Size(76, 29);
            this.buttonPass.TabIndex = 2;
            this.buttonPass.Text = "Pass";
            this.buttonPass.UseVisualStyleBackColor = false;
            // 
            // buttonFail
            // 
            this.buttonFail.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFail.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(75)))), ((int)(((byte)(139)))));
            this.buttonFail.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonFail.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.buttonFail.Location = new System.Drawing.Point(331, 125);
            this.buttonFail.Name = "buttonFail";
            this.buttonFail.Size = new System.Drawing.Size(76, 29);
            this.buttonFail.TabIndex = 3;
            this.buttonFail.Text = "Fail";
            this.buttonFail.UseVisualStyleBackColor = false;
            // 
            // textBoxReason
            // 
            this.textBoxReason.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxReason.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.textBoxReason.Location = new System.Drawing.Point(85, 91);
            this.textBoxReason.Name = "textBoxReason";
            this.textBoxReason.Size = new System.Drawing.Size(404, 26);
            this.textBoxReason.TabIndex = 0;
            // 
            // labelReason
            // 
            this.labelReason.AutoSize = true;
            this.labelReason.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.labelReason.Location = new System.Drawing.Point(14, 94);
            this.labelReason.Name = "labelReason";
            this.labelReason.Size = new System.Drawing.Size(65, 20);
            this.labelReason.TabIndex = 5;
            this.labelReason.Text = "Reason";
            // 
            // UserInteractionForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.ClientSize = new System.Drawing.Size(501, 166);
            this.ControlBox = false;
            this.Controls.Add(this.labelReason);
            this.Controls.Add(this.textBoxReason);
            this.Controls.Add(this.buttonFail);
            this.Controls.Add(this.buttonPass);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.textBoxMessage);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(517, 205);
            this.Name = "UserInteractionForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "UserInteractionForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxMessage;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonPass;
        private System.Windows.Forms.Button buttonFail;
        private System.Windows.Forms.TextBox textBoxReason;
        private System.Windows.Forms.Label labelReason;
    }
}