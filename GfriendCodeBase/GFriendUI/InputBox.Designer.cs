namespace HP.GFriend.UI
{
    partial class InputBox
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
            this.inputBox_label = new System.Windows.Forms.Label();
            this.inputBox_textBox = new System.Windows.Forms.TextBox();
            this.inputBox_OK_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // inputBox_label
            // 
            this.inputBox_label.AutoSize = true;
            this.inputBox_label.Location = new System.Drawing.Point(12, 15);
            this.inputBox_label.Name = "inputBox_label";
            this.inputBox_label.Size = new System.Drawing.Size(35, 13);
            this.inputBox_label.TabIndex = 0;
            this.inputBox_label.Text = "label1";
            // 
            // inputBox_textBox
            // 
            this.inputBox_textBox.Location = new System.Drawing.Point(104, 12);
            this.inputBox_textBox.Name = "inputBox_textBox";
            this.inputBox_textBox.Size = new System.Drawing.Size(111, 20);
            this.inputBox_textBox.TabIndex = 1;
            this.inputBox_textBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.inputBox_textBox_KeyDown);
            // 
            // inputBox_OK_button
            // 
            this.inputBox_OK_button.Location = new System.Drawing.Point(221, 10);
            this.inputBox_OK_button.Name = "inputBox_OK_button";
            this.inputBox_OK_button.Size = new System.Drawing.Size(33, 23);
            this.inputBox_OK_button.TabIndex = 2;
            this.inputBox_OK_button.Text = "OK";
            this.inputBox_OK_button.UseVisualStyleBackColor = true;
            this.inputBox_OK_button.Click += new System.EventHandler(this.inputBox_OK_button_Click);
            // 
            // InputBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.ClientSize = new System.Drawing.Size(266, 41);
            this.Controls.Add(this.inputBox_OK_button);
            this.Controls.Add(this.inputBox_textBox);
            this.Controls.Add(this.inputBox_label);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "InputBox";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "InputBox";
            this.Load += new System.EventHandler(this.InputBox_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.InputBox_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label inputBox_label;
        private System.Windows.Forms.TextBox inputBox_textBox;
        private System.Windows.Forms.Button inputBox_OK_button;
    }
}