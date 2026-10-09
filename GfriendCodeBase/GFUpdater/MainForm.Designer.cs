namespace HP.GFriend.Updater
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
            this.labelStatus = new System.Windows.Forms.Label();
            this.progressBarUpdate = new System.Windows.Forms.ProgressBar();
            this.releseNoteRichTextBox = new System.Windows.Forms.RichTextBox();
            this.textBoxGFBasePath = new System.Windows.Forms.TextBox();
            this.serverLabel = new System.Windows.Forms.Label();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.panelDone = new System.Windows.Forms.Panel();
            this.timerClose = new System.Windows.Forms.Timer(this.components);
            this.labelClose = new System.Windows.Forms.Label();
            this.buttonClose = new System.Windows.Forms.Button();
            this.panelDone.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatus.Location = new System.Drawing.Point(12, 41);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(128, 13);
            this.labelStatus.TabIndex = 0;
            this.labelStatus.Text = "Downloading GFriend";
            // 
            // progressBarUpdate
            // 
            this.progressBarUpdate.Location = new System.Drawing.Point(14, 57);
            this.progressBarUpdate.Name = "progressBarUpdate";
            this.progressBarUpdate.Size = new System.Drawing.Size(856, 10);
            this.progressBarUpdate.Step = 20;
            this.progressBarUpdate.TabIndex = 1;
            // 
            // releseNoteRichTextBox
            // 
            this.releseNoteRichTextBox.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.releseNoteRichTextBox.Location = new System.Drawing.Point(14, 73);
            this.releseNoteRichTextBox.Name = "releseNoteRichTextBox";
            this.releseNoteRichTextBox.ReadOnly = true;
            this.releseNoteRichTextBox.Size = new System.Drawing.Size(856, 244);
            this.releseNoteRichTextBox.TabIndex = 2;
            this.releseNoteRichTextBox.Text = "";
            // 
            // textBoxGFBasePath
            // 
            this.textBoxGFBasePath.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxGFBasePath.Location = new System.Drawing.Point(98, 13);
            this.textBoxGFBasePath.Name = "textBoxGFBasePath";
            this.textBoxGFBasePath.Size = new System.Drawing.Size(728, 21);
            this.textBoxGFBasePath.TabIndex = 3;
            // 
            // serverLabel
            // 
            this.serverLabel.AutoSize = true;
            this.serverLabel.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.serverLabel.Location = new System.Drawing.Point(12, 16);
            this.serverLabel.Name = "serverLabel";
            this.serverLabel.Size = new System.Drawing.Size(80, 13);
            this.serverLabel.TabIndex = 4;
            this.serverLabel.Text = "GFriend Path";
            // 
            // timer
            // 
            this.timer.Tick += new System.EventHandler(this.Timer_Tick);
            // 
            // panelDone
            // 
            this.panelDone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(192)))));
            this.panelDone.Controls.Add(this.buttonClose);
            this.panelDone.Controls.Add(this.labelClose);
            this.panelDone.Location = new System.Drawing.Point(173, 160);
            this.panelDone.Name = "panelDone";
            this.panelDone.Size = new System.Drawing.Size(558, 96);
            this.panelDone.TabIndex = 5;
            this.panelDone.Visible = false;
            // 
            // timerClose
            // 
            this.timerClose.Tick += new System.EventHandler(this.TimerClose_Tick);
            // 
            // labelClose
            // 
            this.labelClose.AutoSize = true;
            this.labelClose.Font = new System.Drawing.Font("Verdana", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelClose.Location = new System.Drawing.Point(115, 11);
            this.labelClose.Name = "labelClose";
            this.labelClose.Size = new System.Drawing.Size(333, 42);
            this.labelClose.TabIndex = 0;
            this.labelClose.Text = "Update Finished";
            // 
            // buttonClose
            // 
            this.buttonClose.Font = new System.Drawing.Font("Verdana", 9F);
            this.buttonClose.Location = new System.Drawing.Point(132, 60);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(299, 23);
            this.buttonClose.TabIndex = 1;
            this.buttonClose.Text = "Close Updater and Launch GFriend (5)";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.ButtonClose_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(884, 323);
            this.Controls.Add(this.panelDone);
            this.Controls.Add(this.serverLabel);
            this.Controls.Add(this.textBoxGFBasePath);
            this.Controls.Add(this.releseNoteRichTextBox);
            this.Controls.Add(this.progressBarUpdate);
            this.Controls.Add(this.labelStatus);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "GF Updater";
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.panelDone.ResumeLayout(false);
            this.panelDone.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ProgressBar progressBarUpdate;
        private System.Windows.Forms.RichTextBox releseNoteRichTextBox;
        private System.Windows.Forms.TextBox textBoxGFBasePath;
        private System.Windows.Forms.Label serverLabel;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.Panel panelDone;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Label labelClose;
        private System.Windows.Forms.Timer timerClose;
    }
}