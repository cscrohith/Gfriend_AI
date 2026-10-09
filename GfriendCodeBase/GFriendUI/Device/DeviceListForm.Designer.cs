namespace HP.GFriend.UI.Device
{
    partial class DeviceListForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeviceListForm));
            this.edit_button = new System.Windows.Forms.Button();
            this.remove_button = new System.Windows.Forms.Button();
            this.deviceList_dataGridView = new System.Windows.Forms.DataGridView();
            this.add_button = new System.Windows.Forms.Button();
            this.stbServer_button = new System.Windows.Forms.Button();
            this.stbServer_textBox = new System.Windows.Forms.TextBox();
            this.stbServer_label = new System.Windows.Forms.Label();
            this.assetInventory_dataGridView = new System.Windows.Forms.DataGridView();
            this.addfromSTB_button = new System.Windows.Forms.Button();
            this.deviceListSplitContainer = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.deviceList_dataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.assetInventory_dataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.deviceListSplitContainer)).BeginInit();
            this.deviceListSplitContainer.Panel1.SuspendLayout();
            this.deviceListSplitContainer.Panel2.SuspendLayout();
            this.deviceListSplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // edit_button
            // 
            this.edit_button.Location = new System.Drawing.Point(81, 0);
            this.edit_button.Name = "edit_button";
            this.edit_button.Size = new System.Drawing.Size(75, 23);
            this.edit_button.TabIndex = 1;
            this.edit_button.Text = "Edit";
            this.edit_button.UseVisualStyleBackColor = true;
            this.edit_button.Click += new System.EventHandler(this.edit_button_Click);
            // 
            // remove_button
            // 
            this.remove_button.Location = new System.Drawing.Point(162, 0);
            this.remove_button.Name = "remove_button";
            this.remove_button.Size = new System.Drawing.Size(75, 23);
            this.remove_button.TabIndex = 2;
            this.remove_button.Text = "Remove";
            this.remove_button.UseVisualStyleBackColor = true;
            this.remove_button.Click += new System.EventHandler(this.remove_button_Click);
            // 
            // deviceList_dataGridView
            // 
            this.deviceList_dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.deviceList_dataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.deviceList_dataGridView.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight;
            this.deviceList_dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.deviceList_dataGridView.Location = new System.Drawing.Point(0, 29);
            this.deviceList_dataGridView.Name = "deviceList_dataGridView";
            this.deviceList_dataGridView.Size = new System.Drawing.Size(856, 231);
            this.deviceList_dataGridView.TabIndex = 3;
            // 
            // add_button
            // 
            this.add_button.Location = new System.Drawing.Point(0, 0);
            this.add_button.Name = "add_button";
            this.add_button.Size = new System.Drawing.Size(75, 23);
            this.add_button.TabIndex = 0;
            this.add_button.Text = "Add";
            this.add_button.UseVisualStyleBackColor = true;
            this.add_button.Click += new System.EventHandler(this.add_button_Click);
            // 
            // stbServer_button
            // 
            this.stbServer_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.stbServer_button.Location = new System.Drawing.Point(239, 269);
            this.stbServer_button.Name = "stbServer_button";
            this.stbServer_button.Size = new System.Drawing.Size(75, 23);
            this.stbServer_button.TabIndex = 4;
            this.stbServer_button.Text = "Connect";
            this.stbServer_button.UseVisualStyleBackColor = true;
            this.stbServer_button.Click += new System.EventHandler(this.stbServer_button_Click);
            // 
            // stbServer_textBox
            // 
            this.stbServer_textBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.stbServer_textBox.Location = new System.Drawing.Point(123, 271);
            this.stbServer_textBox.Name = "stbServer_textBox";
            this.stbServer_textBox.Size = new System.Drawing.Size(110, 20);
            this.stbServer_textBox.TabIndex = 5;
            this.stbServer_textBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.stbServer_textBox_KeyDown);
            // 
            // stbServer_label
            // 
            this.stbServer_label.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.stbServer_label.AutoSize = true;
            this.stbServer_label.Location = new System.Drawing.Point(3, 274);
            this.stbServer_label.Name = "stbServer_label";
            this.stbServer_label.Size = new System.Drawing.Size(114, 13);
            this.stbServer_label.TabIndex = 6;
            this.stbServer_label.Text = "Asset Inventory Server";
            // 
            // assetInventory_dataGridView
            // 
            this.assetInventory_dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.assetInventory_dataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.assetInventory_dataGridView.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight;
            this.assetInventory_dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.assetInventory_dataGridView.Enabled = false;
            this.assetInventory_dataGridView.Location = new System.Drawing.Point(0, 0);
            this.assetInventory_dataGridView.MultiSelect = false;
            this.assetInventory_dataGridView.Name = "assetInventory_dataGridView";
            this.assetInventory_dataGridView.Size = new System.Drawing.Size(856, 168);
            this.assetInventory_dataGridView.TabIndex = 8;
            // 
            // addfromSTB_button
            // 
            this.addfromSTB_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.addfromSTB_button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.addfromSTB_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.addfromSTB_button.Image = global::HP.GFriend.UI.Properties.Resources.AddDevice;
            this.addfromSTB_button.Location = new System.Drawing.Point(346, 269);
            this.addfromSTB_button.Name = "addfromSTB_button";
            this.addfromSTB_button.Size = new System.Drawing.Size(93, 23);
            this.addfromSTB_button.TabIndex = 9;
            this.addfromSTB_button.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.addfromSTB_button.UseVisualStyleBackColor = true;
            this.addfromSTB_button.Visible = false;
            this.addfromSTB_button.Click += new System.EventHandler(this.addfromSTB_button_Click);
            // 
            // deviceListSplitContainer
            // 
            this.deviceListSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.deviceListSplitContainer.Location = new System.Drawing.Point(12, 12);
            this.deviceListSplitContainer.Name = "deviceListSplitContainer";
            this.deviceListSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // deviceListSplitContainer.Panel1
            // 
            this.deviceListSplitContainer.Panel1.Controls.Add(this.add_button);
            this.deviceListSplitContainer.Panel1.Controls.Add(this.addfromSTB_button);
            this.deviceListSplitContainer.Panel1.Controls.Add(this.edit_button);
            this.deviceListSplitContainer.Panel1.Controls.Add(this.remove_button);
            this.deviceListSplitContainer.Panel1.Controls.Add(this.stbServer_button);
            this.deviceListSplitContainer.Panel1.Controls.Add(this.stbServer_textBox);
            this.deviceListSplitContainer.Panel1.Controls.Add(this.stbServer_label);
            this.deviceListSplitContainer.Panel1.Controls.Add(this.deviceList_dataGridView);
            // 
            // deviceListSplitContainer.Panel2
            // 
            this.deviceListSplitContainer.Panel2.Controls.Add(this.assetInventory_dataGridView);
            this.deviceListSplitContainer.Size = new System.Drawing.Size(856, 466);
            this.deviceListSplitContainer.SplitterDistance = 294;
            this.deviceListSplitContainer.TabIndex = 10;
            // 
            // DeviceListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.ClientSize = new System.Drawing.Size(880, 490);
            this.Controls.Add(this.deviceListSplitContainer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DeviceListForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Device List";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DeviceListForm_FormClosing);
            this.Load += new System.EventHandler(this.DeviceListForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.deviceList_dataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.assetInventory_dataGridView)).EndInit();
            this.deviceListSplitContainer.Panel1.ResumeLayout(false);
            this.deviceListSplitContainer.Panel1.PerformLayout();
            this.deviceListSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.deviceListSplitContainer)).EndInit();
            this.deviceListSplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button edit_button;
        private System.Windows.Forms.Button remove_button;
        private System.Windows.Forms.DataGridView deviceList_dataGridView;
        private System.Windows.Forms.Button add_button;
        private System.Windows.Forms.Button stbServer_button;
        private System.Windows.Forms.TextBox stbServer_textBox;
        private System.Windows.Forms.Label stbServer_label;
        private System.Windows.Forms.DataGridView assetInventory_dataGridView;
        private System.Windows.Forms.Button addfromSTB_button;
        private System.Windows.Forms.SplitContainer deviceListSplitContainer;
    }
}