namespace HP.GFriend.UI
{
    partial class GetTouchEventForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GetTouchEventForm));
            this.textConsole = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textConsole
            // 
            this.textConsole.Font = new System.Drawing.Font("Verdana", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textConsole.Location = new System.Drawing.Point(3, 3);
            this.textConsole.Multiline = true;
            this.textConsole.Name = "textConsole";
            this.textConsole.Size = new System.Drawing.Size(519, 226);
            this.textConsole.TabIndex = 0;
            // 
            // GetTouchEventForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(523, 257);
            this.Controls.Add(this.textConsole);
            this.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "GetTouchEventForm";
            this.Text = "Get Touch Event";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.GetTouchEventForm_FormClosed);
            this.Load += new System.EventHandler(this.GetTouchEventForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textConsole;
    }
}