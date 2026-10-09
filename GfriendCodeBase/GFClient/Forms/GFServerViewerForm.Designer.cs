namespace HP.GFriend.Client.Forms
{
    partial class GFServerViewerForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GFServerViewerForm));
            this.testProjects_panel = new System.Windows.Forms.Panel();
            this.testProjects_dataGridView = new System.Windows.Forms.DataGridView();
            this.gfServerAddr_label = new System.Windows.Forms.Label();
            this.Connect_button = new System.Windows.Forms.Button();
            this.gfServerAddr_textBox = new System.Windows.Forms.TextBox();
            this.page_toolStrip = new System.Windows.Forms.ToolStrip();
            this.gotoFirst_toolStripButton = new System.Windows.Forms.ToolStripButton();
            this.back_toolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton4 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton5 = new System.Windows.Forms.ToolStripButton();
            this.forward_toolStripButton = new System.Windows.Forms.ToolStripButton();
            this.gotoLast_toolStripButton = new System.Windows.Forms.ToolStripButton();
            this.refresh_button = new System.Windows.Forms.Button();
            this.testProjects_panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.testProjects_dataGridView)).BeginInit();
            this.page_toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // testProjects_panel
            // 
            this.testProjects_panel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.testProjects_panel.Controls.Add(this.testProjects_dataGridView);
            this.testProjects_panel.Location = new System.Drawing.Point(13, 40);
            this.testProjects_panel.Name = "testProjects_panel";
            this.testProjects_panel.Size = new System.Drawing.Size(1078, 320);
            this.testProjects_panel.TabIndex = 0;
            // 
            // testProjects_dataGridView
            // 
            this.testProjects_dataGridView.AllowUserToAddRows = false;
            this.testProjects_dataGridView.AllowUserToDeleteRows = false;
            this.testProjects_dataGridView.AllowUserToResizeColumns = false;
            this.testProjects_dataGridView.AllowUserToResizeRows = false;
            this.testProjects_dataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.testProjects_dataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.testProjects_dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.testProjects_dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.testProjects_dataGridView.Location = new System.Drawing.Point(0, 0);
            this.testProjects_dataGridView.Name = "testProjects_dataGridView";
            this.testProjects_dataGridView.ReadOnly = true;
            this.testProjects_dataGridView.RowTemplate.Height = 30;
            this.testProjects_dataGridView.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.testProjects_dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.testProjects_dataGridView.Size = new System.Drawing.Size(1078, 320);
            this.testProjects_dataGridView.TabIndex = 0;
            this.testProjects_dataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.testProjects_dataGridView_CellContentClick);
            // 
            // gfServerAddr_label
            // 
            this.gfServerAddr_label.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gfServerAddr_label.AutoSize = true;
            this.gfServerAddr_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gfServerAddr_label.Location = new System.Drawing.Point(774, 17);
            this.gfServerAddr_label.Name = "gfServerAddr_label";
            this.gfServerAddr_label.Size = new System.Drawing.Size(82, 13);
            this.gfServerAddr_label.TabIndex = 9;
            this.gfServerAddr_label.Text = "Server Address:";
            // 
            // Connect_button
            // 
            this.Connect_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Connect_button.BackColor = System.Drawing.Color.Transparent;
            this.Connect_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Connect_button.Location = new System.Drawing.Point(1016, 11);
            this.Connect_button.Name = "Connect_button";
            this.Connect_button.Size = new System.Drawing.Size(73, 23);
            this.Connect_button.TabIndex = 8;
            this.Connect_button.Text = "Connect";
            this.Connect_button.UseVisualStyleBackColor = false;
            this.Connect_button.Click += new System.EventHandler(this.Connect_button_Click);
            // 
            // gfServerAddr_textBox
            // 
            this.gfServerAddr_textBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gfServerAddr_textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gfServerAddr_textBox.Location = new System.Drawing.Point(862, 12);
            this.gfServerAddr_textBox.Name = "gfServerAddr_textBox";
            this.gfServerAddr_textBox.Size = new System.Drawing.Size(147, 21);
            this.gfServerAddr_textBox.TabIndex = 7;
            // 
            // page_toolStrip
            // 
            this.page_toolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.page_toolStrip.AutoSize = false;
            this.page_toolStrip.BackColor = System.Drawing.Color.Transparent;
            this.page_toolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.page_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gotoFirst_toolStripButton,
            this.back_toolStripButton,
            this.toolStripButton1,
            this.toolStripButton2,
            this.toolStripButton3,
            this.toolStripButton4,
            this.toolStripButton5,
            this.forward_toolStripButton,
            this.gotoLast_toolStripButton});
            this.page_toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.page_toolStrip.Location = new System.Drawing.Point(440, 367);
            this.page_toolStrip.Name = "page_toolStrip";
            this.page_toolStrip.Size = new System.Drawing.Size(443, 23);
            this.page_toolStrip.TabIndex = 10;
            this.page_toolStrip.Text = "toolStrip1";
            // 
            // gotoFirst_toolStripButton
            // 
            this.gotoFirst_toolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.gotoFirst_toolStripButton.Image = global::HP.GFriend.Client.Properties.Resources.PreviousFrame_16x;
            this.gotoFirst_toolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.gotoFirst_toolStripButton.Name = "gotoFirst_toolStripButton";
            this.gotoFirst_toolStripButton.Size = new System.Drawing.Size(23, 20);
            this.gotoFirst_toolStripButton.Text = "toolStripButton6";
            this.gotoFirst_toolStripButton.Click += new System.EventHandler(this.ToolStripButtonClick);
            // 
            // back_toolStripButton
            // 
            this.back_toolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.back_toolStripButton.Image = global::HP.GFriend.Client.Properties.Resources.GlyphLeft_16x;
            this.back_toolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.back_toolStripButton.Name = "back_toolStripButton";
            this.back_toolStripButton.Size = new System.Drawing.Size(23, 20);
            this.back_toolStripButton.Text = "back_toolStripButton";
            this.back_toolStripButton.Click += new System.EventHandler(this.ToolStripButtonClick);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 19);
            this.toolStripButton1.Text = "1";
            this.toolStripButton1.Click += new System.EventHandler(this.ToolStripButtonClick);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton2.Image")));
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(23, 19);
            this.toolStripButton2.Text = "2";
            this.toolStripButton2.Click += new System.EventHandler(this.ToolStripButtonClick);
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton3.Image")));
            this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(23, 19);
            this.toolStripButton3.Text = "3";
            this.toolStripButton3.Click += new System.EventHandler(this.ToolStripButtonClick);
            // 
            // toolStripButton4
            // 
            this.toolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton4.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton4.Image")));
            this.toolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton4.Name = "toolStripButton4";
            this.toolStripButton4.Size = new System.Drawing.Size(23, 19);
            this.toolStripButton4.Text = "4";
            this.toolStripButton4.Click += new System.EventHandler(this.ToolStripButtonClick);
            // 
            // toolStripButton5
            // 
            this.toolStripButton5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton5.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton5.Image")));
            this.toolStripButton5.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton5.Name = "toolStripButton5";
            this.toolStripButton5.Size = new System.Drawing.Size(23, 19);
            this.toolStripButton5.Text = "5";
            this.toolStripButton5.Click += new System.EventHandler(this.ToolStripButtonClick);
            // 
            // forward_toolStripButton
            // 
            this.forward_toolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.forward_toolStripButton.Image = global::HP.GFriend.Client.Properties.Resources.GlyphRight_16x;
            this.forward_toolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.forward_toolStripButton.Name = "forward_toolStripButton";
            this.forward_toolStripButton.Size = new System.Drawing.Size(23, 20);
            this.forward_toolStripButton.Text = "forward_toolStripButton";
            this.forward_toolStripButton.Click += new System.EventHandler(this.ToolStripButtonClick);
            // 
            // gotoLast_toolStripButton
            // 
            this.gotoLast_toolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.gotoLast_toolStripButton.Image = global::HP.GFriend.Client.Properties.Resources.NextFrameArrow_16x;
            this.gotoLast_toolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.gotoLast_toolStripButton.Name = "gotoLast_toolStripButton";
            this.gotoLast_toolStripButton.Size = new System.Drawing.Size(23, 20);
            this.gotoLast_toolStripButton.Text = "toolStripButton7";
            this.gotoLast_toolStripButton.Click += new System.EventHandler(this.ToolStripButtonClick);
            // 
            // refresh_button
            // 
            this.refresh_button.Image = global::HP.GFriend.Client.Properties.Resources.Refresh_server_16x;
            this.refresh_button.Location = new System.Drawing.Point(13, 12);
            this.refresh_button.Name = "refresh_button";
            this.refresh_button.Size = new System.Drawing.Size(75, 23);
            this.refresh_button.TabIndex = 11;
            this.refresh_button.Text = "Refresh";
            this.refresh_button.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.refresh_button.UseVisualStyleBackColor = true;
            this.refresh_button.Click += new System.EventHandler(this.refresh_button_Click);
            // 
            // GFServerViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.ClientSize = new System.Drawing.Size(1103, 399);
            this.Controls.Add(this.refresh_button);
            this.Controls.Add(this.page_toolStrip);
            this.Controls.Add(this.gfServerAddr_label);
            this.Controls.Add(this.Connect_button);
            this.Controls.Add(this.gfServerAddr_textBox);
            this.Controls.Add(this.testProjects_panel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GFServerViewerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "GFreind Server";
            this.Load += new System.EventHandler(this.GFServerViewerForm_Load);
            this.testProjects_panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.testProjects_dataGridView)).EndInit();
            this.page_toolStrip.ResumeLayout(false);
            this.page_toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel testProjects_panel;
        private System.Windows.Forms.DataGridView testProjects_dataGridView;
        private System.Windows.Forms.Label gfServerAddr_label;
        private System.Windows.Forms.Button Connect_button;
        private System.Windows.Forms.TextBox gfServerAddr_textBox;
        private System.Windows.Forms.ToolStrip page_toolStrip;
        private System.Windows.Forms.ToolStripButton back_toolStripButton;
        private System.Windows.Forms.ToolStripButton forward_toolStripButton;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.ToolStripButton toolStripButton4;
        private System.Windows.Forms.ToolStripButton toolStripButton5;
        private System.Windows.Forms.ToolStripButton gotoFirst_toolStripButton;
        private System.Windows.Forms.ToolStripButton gotoLast_toolStripButton;
        private System.Windows.Forms.Button refresh_button;
    }
}