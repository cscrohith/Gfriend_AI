namespace HP.GFriend.UI
{
    partial class OptionForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionForm));
            this.flowLayoutPanelDefault = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBoxDescriptionOption = new System.Windows.Forms.GroupBox();
            this.textBoxDescriptionLibrary = new System.Windows.Forms.TextBox();
            this.labelCustomLibrary = new System.Windows.Forms.Label();
            this.textBoxDescriptionTestScript = new System.Windows.Forms.TextBox();
            this.labelScript = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.textBoxDescriptionOverall = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.flowLayoutPanelDefault.SuspendLayout();
            this.groupBoxDescriptionOption.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanelDefault
            // 
            this.flowLayoutPanelDefault.Controls.Add(this.groupBoxDescriptionOption);
            this.flowLayoutPanelDefault.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanelDefault.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanelDefault.Name = "flowLayoutPanelDefault";
            this.flowLayoutPanelDefault.Size = new System.Drawing.Size(466, 454);
            this.flowLayoutPanelDefault.TabIndex = 0;
            // 
            // groupBoxDescriptionOption
            // 
            this.groupBoxDescriptionOption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxDescriptionOption.Controls.Add(this.textBoxDescriptionOverall);
            this.groupBoxDescriptionOption.Controls.Add(this.label1);
            this.groupBoxDescriptionOption.Controls.Add(this.textBoxDescriptionLibrary);
            this.groupBoxDescriptionOption.Controls.Add(this.labelCustomLibrary);
            this.groupBoxDescriptionOption.Controls.Add(this.textBoxDescriptionTestScript);
            this.groupBoxDescriptionOption.Controls.Add(this.labelScript);
            this.groupBoxDescriptionOption.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxDescriptionOption.Location = new System.Drawing.Point(3, 3);
            this.groupBoxDescriptionOption.Name = "groupBoxDescriptionOption";
            this.groupBoxDescriptionOption.Size = new System.Drawing.Size(457, 431);
            this.groupBoxDescriptionOption.TabIndex = 0;
            this.groupBoxDescriptionOption.TabStop = false;
            this.groupBoxDescriptionOption.Text = "Description Comments";
            // 
            // textBoxDescriptionLibrary
            // 
            this.textBoxDescriptionLibrary.Font = new System.Drawing.Font("Verdana", 8.25F);
            this.textBoxDescriptionLibrary.Location = new System.Drawing.Point(9, 254);
            this.textBoxDescriptionLibrary.Multiline = true;
            this.textBoxDescriptionLibrary.Name = "textBoxDescriptionLibrary";
            this.textBoxDescriptionLibrary.Size = new System.Drawing.Size(439, 121);
            this.textBoxDescriptionLibrary.TabIndex = 3;
            // 
            // labelCustomLibrary
            // 
            this.labelCustomLibrary.AutoSize = true;
            this.labelCustomLibrary.Font = new System.Drawing.Font("Verdana", 8.25F);
            this.labelCustomLibrary.Location = new System.Drawing.Point(6, 238);
            this.labelCustomLibrary.Name = "labelCustomLibrary";
            this.labelCustomLibrary.Size = new System.Drawing.Size(298, 13);
            this.labelCustomLibrary.TabIndex = 2;
            this.labelCustomLibrary.Text = "Custom Keyword (type \"/// \" in custom library file)";
            // 
            // textBoxDescriptionTestScript
            // 
            this.textBoxDescriptionTestScript.Font = new System.Drawing.Font("Verdana", 8.25F);
            this.textBoxDescriptionTestScript.Location = new System.Drawing.Point(9, 133);
            this.textBoxDescriptionTestScript.Multiline = true;
            this.textBoxDescriptionTestScript.Name = "textBoxDescriptionTestScript";
            this.textBoxDescriptionTestScript.Size = new System.Drawing.Size(439, 102);
            this.textBoxDescriptionTestScript.TabIndex = 1;
            // 
            // labelScript
            // 
            this.labelScript.AutoSize = true;
            this.labelScript.Font = new System.Drawing.Font("Verdana", 8.25F);
            this.labelScript.Location = new System.Drawing.Point(9, 117);
            this.labelScript.Name = "labelScript";
            this.labelScript.Size = new System.Drawing.Size(230, 13);
            this.labelScript.TabIndex = 0;
            this.labelScript.Text = "Test Case (type \"/// \" in test script file)";
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(373, 460);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(292, 460);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // textBoxDescriptionOverall
            // 
            this.textBoxDescriptionOverall.Font = new System.Drawing.Font("Verdana", 8.25F);
            this.textBoxDescriptionOverall.Location = new System.Drawing.Point(9, 37);
            this.textBoxDescriptionOverall.Multiline = true;
            this.textBoxDescriptionOverall.Name = "textBoxDescriptionOverall";
            this.textBoxDescriptionOverall.Size = new System.Drawing.Size(439, 77);
            this.textBoxDescriptionOverall.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Verdana", 8.25F);
            this.label1.Location = new System.Drawing.Point(6, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(203, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Test Script (type \"/// \" in first line)";
            // 
            // OptionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.ClientSize = new System.Drawing.Size(466, 486);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.flowLayoutPanelDefault);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "OptionForm";
            this.Text = "Options";
            this.flowLayoutPanelDefault.ResumeLayout(false);
            this.groupBoxDescriptionOption.ResumeLayout(false);
            this.groupBoxDescriptionOption.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelDefault;
        private System.Windows.Forms.GroupBox groupBoxDescriptionOption;
        private System.Windows.Forms.TextBox textBoxDescriptionLibrary;
        private System.Windows.Forms.Label labelCustomLibrary;
        private System.Windows.Forms.TextBox textBoxDescriptionTestScript;
        private System.Windows.Forms.Label labelScript;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TextBox textBoxDescriptionOverall;
        private System.Windows.Forms.Label label1;
    }
}