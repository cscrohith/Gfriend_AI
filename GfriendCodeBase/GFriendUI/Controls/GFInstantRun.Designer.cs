namespace HP.GFriend.UI.Controls
{
    partial class GFInstantRun
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GFInstantRun));
            this.buttonConnect = new System.Windows.Forms.Button();
            this.comboBoxDeviceList = new System.Windows.Forms.ComboBox();
            this.buttonRun = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxRun = new FastColoredTextBoxNS.FastColoredTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.clearTextRunOutput_button = new System.Windows.Forms.Button();
            this.textRun_panel = new System.Windows.Forms.Panel();
            this.checkedListBoxLibraries = new System.Windows.Forms.CheckedListBox();
            this.labelLibrariesSelection = new System.Windows.Forms.Label();
            this.gfOutput = new HP.GFriend.UI.Controls.GFOutputBox();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxRun)).BeginInit();
            this.textRun_panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gfOutput)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonConnect
            // 
            this.buttonConnect.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.buttonConnect.FlatAppearance.BorderSize = 0;
            this.buttonConnect.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.buttonConnect.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.buttonConnect.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonConnect.Image = global::HP.GFriend.UI.Properties.Resources.Connect_16x;
            this.buttonConnect.Location = new System.Drawing.Point(256, 3);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(26, 23);
            this.buttonConnect.TabIndex = 15;
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.TextRun_Connect_button_Click);
            // 
            // comboBoxDeviceList
            // 
            this.comboBoxDeviceList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDeviceList.FormattingEnabled = true;
            this.comboBoxDeviceList.Location = new System.Drawing.Point(113, 4);
            this.comboBoxDeviceList.Name = "comboBoxDeviceList";
            this.comboBoxDeviceList.Size = new System.Drawing.Size(138, 21);
            this.comboBoxDeviceList.TabIndex = 15;
            this.comboBoxDeviceList.SelectedIndexChanged += new System.EventHandler(this.TextRun_DeviceList_comboBox_SelectedIndexChanged);
            // 
            // buttonRun
            // 
            this.buttonRun.Enabled = false;
            this.buttonRun.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.buttonRun.FlatAppearance.BorderSize = 0;
            this.buttonRun.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.buttonRun.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.buttonRun.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonRun.Image = global::HP.GFriend.UI.Properties.Resources.Run_16x;
            this.buttonRun.Location = new System.Drawing.Point(951, 4);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(57, 23);
            this.buttonRun.TabIndex = 17;
            this.buttonRun.Text = "Run";
            this.buttonRun.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.TextRun_run_button_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.label1.Location = new System.Drawing.Point(9, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "Target Device";
            // 
            // textBoxRun
            // 
            this.textBoxRun.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
            this.textBoxRun.AutoIndent = false;
            this.textBoxRun.AutoIndentChars = false;
            this.textBoxRun.AutoIndentCharsPatterns = "";
            this.textBoxRun.AutoIndentExistingLines = false;
            this.textBoxRun.AutoScrollMargin = new System.Drawing.Size(1, 1);
            this.textBoxRun.AutoScrollMinSize = new System.Drawing.Size(0, 14);
            this.textBoxRun.BackBrush = null;
            this.textBoxRun.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxRun.CharHeight = 14;
            this.textBoxRun.CharWidth = 8;
            this.textBoxRun.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBoxRun.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.textBoxRun.Enabled = false;
            this.textBoxRun.ImeMode = System.Windows.Forms.ImeMode.Hangul;
            this.textBoxRun.IsReplaceMode = false;
            this.textBoxRun.Location = new System.Drawing.Point(627, 4);
            this.textBoxRun.Multiline = false;
            this.textBoxRun.Name = "textBoxRun";
            this.textBoxRun.Paddings = new System.Windows.Forms.Padding(0);
            this.textBoxRun.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            //this.textBoxRun.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("textBoxRun.ServiceColors")));
            this.textBoxRun.ShowLineNumbers = false;
            this.textBoxRun.ShowScrollBars = false;
            this.textBoxRun.Size = new System.Drawing.Size(318, 23);
            this.textBoxRun.TabIndex = 19;
            this.textBoxRun.WordWrap = true;
            this.textBoxRun.Zoom = 100;
            this.textBoxRun.TextChanged += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.TextRun_fastColoredTextBox_TextChanged);
            this.textBoxRun.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextRun_fastColoredTextBox_KeyDown);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.label3.Location = new System.Drawing.Point(558, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.TabIndex = 19;
            this.label3.Text = "Keyword";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.label2.Location = new System.Drawing.Point(304, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "Libraries";
            // 
            // clearTextRunOutput_button
            // 
            this.clearTextRunOutput_button.BackColor = System.Drawing.Color.White;
            this.clearTextRunOutput_button.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.clearTextRunOutput_button.FlatAppearance.BorderSize = 0;
            this.clearTextRunOutput_button.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.clearTextRunOutput_button.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.clearTextRunOutput_button.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.clearTextRunOutput_button.Image = global::HP.GFriend.UI.Properties.Resources.ClearText_16x;
            this.clearTextRunOutput_button.Location = new System.Drawing.Point(1020, 4);
            this.clearTextRunOutput_button.Name = "clearTextRunOutput_button";
            this.clearTextRunOutput_button.Size = new System.Drawing.Size(26, 23);
            this.clearTextRunOutput_button.TabIndex = 22;
            this.clearTextRunOutput_button.UseVisualStyleBackColor = false;
            this.clearTextRunOutput_button.Click += new System.EventHandler(this.ClearTextRunOutput_button_Click);
            // 
            // textRun_panel
            // 
            this.textRun_panel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textRun_panel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(75)))), ((int)(((byte)(139)))));
            this.textRun_panel.Controls.Add(this.checkedListBoxLibraries);
            this.textRun_panel.Controls.Add(this.labelLibrariesSelection);
            this.textRun_panel.Controls.Add(this.gfOutput);
            this.textRun_panel.Controls.Add(this.clearTextRunOutput_button);
            this.textRun_panel.Controls.Add(this.label2);
            this.textRun_panel.Controls.Add(this.label3);
            this.textRun_panel.Controls.Add(this.textBoxRun);
            this.textRun_panel.Controls.Add(this.label1);
            this.textRun_panel.Controls.Add(this.buttonRun);
            this.textRun_panel.Controls.Add(this.comboBoxDeviceList);
            this.textRun_panel.Controls.Add(this.buttonConnect);
            this.textRun_panel.Location = new System.Drawing.Point(0, 0);
            this.textRun_panel.Name = "textRun_panel";
            this.textRun_panel.Size = new System.Drawing.Size(1055, 147);
            this.textRun_panel.TabIndex = 20;
            // 
            // checkedListBoxLibraries
            // 
            this.checkedListBoxLibraries.CheckOnClick = true;
            this.checkedListBoxLibraries.FormattingEnabled = true;
            this.checkedListBoxLibraries.Location = new System.Drawing.Point(375, 3);
            this.checkedListBoxLibraries.Name = "checkedListBoxLibraries";
            this.checkedListBoxLibraries.Size = new System.Drawing.Size(170, 124);
            this.checkedListBoxLibraries.TabIndex = 23;
            this.checkedListBoxLibraries.Visible = false;
            this.checkedListBoxLibraries.Leave += new System.EventHandler(this.CheckedListBoxLibraries_Leave);
            // 
            // labelLibrariesSelection
            // 
            this.labelLibrariesSelection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.labelLibrariesSelection.Font = new System.Drawing.Font("Verdana", 8F);
            this.labelLibrariesSelection.Location = new System.Drawing.Point(375, 4);
            this.labelLibrariesSelection.Name = "labelLibrariesSelection";
            this.labelLibrariesSelection.Size = new System.Drawing.Size(135, 21);
            this.labelLibrariesSelection.TabIndex = 24;
            this.labelLibrariesSelection.Text = "Select Libraries";
            this.labelLibrariesSelection.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelLibrariesSelection.Click += new System.EventHandler(this.LabelLibrariesSelection_Click);
            // 
            // gfOutput
            // 
            this.gfOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gfOutput.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
            this.gfOutput.AutoIndent = false;
            this.gfOutput.AutoIndentChars = false;
            this.gfOutput.AutoIndentCharsPatterns = "";
            this.gfOutput.AutoIndentExistingLines = false;
            this.gfOutput.AutoScrollMargin = new System.Drawing.Size(1, 1);
            this.gfOutput.AutoScrollMinSize = new System.Drawing.Size(0, 14);
            this.gfOutput.BackBrush = null;
            this.gfOutput.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.gfOutput.CharHeight = 14;
            this.gfOutput.CharWidth = 8;
            this.gfOutput.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.gfOutput.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.gfOutput.Font = new System.Drawing.Font("Courier New", 9.75F);
            this.gfOutput.ImeMode = System.Windows.Forms.ImeMode.Hangul;
            this.gfOutput.IsReplaceMode = false;
            this.gfOutput.Location = new System.Drawing.Point(0, 31);
            this.gfOutput.Name = "gfOutput";
            this.gfOutput.Paddings = new System.Windows.Forms.Padding(0);
            this.gfOutput.ReadOnly = true;
            this.gfOutput.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            //this.gfOutput.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("gfOutput.ServiceColors")));
            this.gfOutput.ShowLineNumbers = false;
            this.gfOutput.Size = new System.Drawing.Size(1055, 116);
            this.gfOutput.TabIndex = 1;
            this.gfOutput.WordWrap = true;
            this.gfOutput.Zoom = 100;
            this.gfOutput.TextChanged += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.TextRunOutput_fastColoredTextBox_TextChanged);
            // 
            // GFInstantRun
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textRun_panel);
            this.Name = "GFInstantRun";
            this.Size = new System.Drawing.Size(1055, 147);
            ((System.ComponentModel.ISupportInitialize)(this.textBoxRun)).EndInit();
            this.textRun_panel.ResumeLayout(false);
            this.textRun_panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gfOutput)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private GFOutputBox gfOutput;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.ComboBox comboBoxDeviceList;
        private System.Windows.Forms.Button buttonRun;
        private System.Windows.Forms.Label label1;
        private FastColoredTextBoxNS.FastColoredTextBox textBoxRun;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button clearTextRunOutput_button;
        private System.Windows.Forms.Panel textRun_panel;
        private System.Windows.Forms.CheckedListBox checkedListBoxLibraries;
        private System.Windows.Forms.Label labelLibrariesSelection;
    }
}
