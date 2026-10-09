namespace HP.GFriend.UI.Device
{
    partial class UpdateDeviceForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateDeviceForm));
            this.deviceId_label = new System.Windows.Forms.Label();
            this.deviceId_textBox = new System.Windows.Forms.TextBox();
            this.description_label = new System.Windows.Forms.Label();
            this.description_textBox = new System.Windows.Forms.TextBox();
            this.adminId_textBox = new System.Windows.Forms.TextBox();
            this.adminID_label = new System.Windows.Forms.Label();
            this.adminPassword_textBox = new System.Windows.Forms.TextBox();
            this.adminPassword_label = new System.Windows.Forms.Label();
            this.oK_button = new System.Windows.Forms.Button();
            this.cancel_button = new System.Windows.Forms.Button();
            this.port_textBox = new System.Windows.Forms.TextBox();
            this.port_label = new System.Windows.Forms.Label();
            this.deviceAddress_textBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.deviceType_textBox = new System.Windows.Forms.TextBox();
            this.deviceType_label = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lanDebugAddress_textBox = new System.Windows.Forms.TextBox();
            this.debugAddress_label = new System.Windows.Forms.Label();
            this.Capability_label = new System.Windows.Forms.Label();
            this.dataGridViewCapabilities = new System.Windows.Forms.DataGridView();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewCapabilities)).BeginInit();
            this.SuspendLayout();
            // 
            // deviceId_label
            // 
            this.deviceId_label.AutoSize = true;
            this.deviceId_label.Location = new System.Drawing.Point(12, 9);
            this.deviceId_label.Name = "deviceId_label";
            this.deviceId_label.Size = new System.Drawing.Size(56, 13);
            this.deviceId_label.TabIndex = 0;
            this.deviceId_label.Text = "Device Id:";
            // 
            // deviceId_textBox
            // 
            this.deviceId_textBox.Location = new System.Drawing.Point(103, 6);
            this.deviceId_textBox.Name = "deviceId_textBox";
            this.deviceId_textBox.Size = new System.Drawing.Size(183, 20);
            this.deviceId_textBox.TabIndex = 1;
            // 
            // description_label
            // 
            this.description_label.AutoSize = true;
            this.description_label.Location = new System.Drawing.Point(12, 90);
            this.description_label.Name = "description_label";
            this.description_label.Size = new System.Drawing.Size(63, 13);
            this.description_label.TabIndex = 2;
            this.description_label.Text = "Description:";
            // 
            // description_textBox
            // 
            this.description_textBox.Location = new System.Drawing.Point(103, 87);
            this.description_textBox.Name = "description_textBox";
            this.description_textBox.Size = new System.Drawing.Size(183, 20);
            this.description_textBox.TabIndex = 5;
            // 
            // adminId_textBox
            // 
            this.adminId_textBox.Location = new System.Drawing.Point(103, 113);
            this.adminId_textBox.Name = "adminId_textBox";
            this.adminId_textBox.Size = new System.Drawing.Size(183, 20);
            this.adminId_textBox.TabIndex = 6;
            // 
            // adminID_label
            // 
            this.adminID_label.AutoSize = true;
            this.adminID_label.Location = new System.Drawing.Point(12, 116);
            this.adminID_label.Name = "adminID_label";
            this.adminID_label.Size = new System.Drawing.Size(51, 13);
            this.adminID_label.TabIndex = 8;
            this.adminID_label.Text = "Admin Id:";
            // 
            // adminPassword_textBox
            // 
            this.adminPassword_textBox.Location = new System.Drawing.Point(103, 140);
            this.adminPassword_textBox.Name = "adminPassword_textBox";
            this.adminPassword_textBox.Size = new System.Drawing.Size(183, 20);
            this.adminPassword_textBox.TabIndex = 7;
            // 
            // adminPassword_label
            // 
            this.adminPassword_label.AutoSize = true;
            this.adminPassword_label.Location = new System.Drawing.Point(12, 143);
            this.adminPassword_label.Name = "adminPassword_label";
            this.adminPassword_label.Size = new System.Drawing.Size(88, 13);
            this.adminPassword_label.TabIndex = 11;
            this.adminPassword_label.Text = "Admin Password:";
            // 
            // oK_button
            // 
            this.oK_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.oK_button.Location = new System.Drawing.Point(82, 347);
            this.oK_button.Name = "oK_button";
            this.oK_button.Size = new System.Drawing.Size(75, 23);
            this.oK_button.TabIndex = 9;
            this.oK_button.Text = "OK";
            this.oK_button.UseVisualStyleBackColor = true;
            this.oK_button.Click += new System.EventHandler(this.oK_button_Click);
            // 
            // cancel_button
            // 
            this.cancel_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancel_button.Location = new System.Drawing.Point(187, 347);
            this.cancel_button.Name = "cancel_button";
            this.cancel_button.Size = new System.Drawing.Size(75, 23);
            this.cancel_button.TabIndex = 10;
            this.cancel_button.Text = "Cancel";
            this.cancel_button.UseVisualStyleBackColor = true;
            this.cancel_button.Click += new System.EventHandler(this.cancel_button_Click);
            // 
            // port_textBox
            // 
            this.port_textBox.Location = new System.Drawing.Point(244, 33);
            this.port_textBox.Name = "port_textBox";
            this.port_textBox.Size = new System.Drawing.Size(42, 20);
            this.port_textBox.TabIndex = 3;
            // 
            // port_label
            // 
            this.port_label.AutoSize = true;
            this.port_label.Location = new System.Drawing.Point(209, 36);
            this.port_label.Name = "port_label";
            this.port_label.Size = new System.Drawing.Size(29, 13);
            this.port_label.TabIndex = 17;
            this.port_label.Text = "Port:";
            // 
            // deviceAddress_textBox
            // 
            this.deviceAddress_textBox.Location = new System.Drawing.Point(103, 33);
            this.deviceAddress_textBox.Name = "deviceAddress_textBox";
            this.deviceAddress_textBox.Size = new System.Drawing.Size(100, 20);
            this.deviceAddress_textBox.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Device Address:";
            // 
            // deviceType_textBox
            // 
            this.deviceType_textBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.deviceType_textBox.Location = new System.Drawing.Point(103, 169);
            this.deviceType_textBox.Name = "deviceType_textBox";
            this.deviceType_textBox.ReadOnly = true;
            this.deviceType_textBox.Size = new System.Drawing.Size(183, 20);
            this.deviceType_textBox.TabIndex = 8;
            this.deviceType_textBox.Click += new System.EventHandler(this.DeviceType_TextBox_Click);
            this.deviceType_textBox.TextChanged += new System.EventHandler(this.DeviceType_TextBox_TextChanged);
            // 
            // deviceType_label
            // 
            this.deviceType_label.AutoSize = true;
            this.deviceType_label.Location = new System.Drawing.Point(12, 172);
            this.deviceType_label.Name = "deviceType_label";
            this.deviceType_label.Size = new System.Drawing.Size(71, 13);
            this.deviceType_label.TabIndex = 19;
            this.deviceType_label.Text = "Device Type:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Location = new System.Drawing.Point(3, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(227, 338);
            this.label2.TabIndex = 20;
            this.label2.Text = resources.GetString("label2.Text");
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox1.Location = new System.Drawing.Point(316, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(228, 358);
            this.groupBox1.TabIndex = 21;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "* Requirement Values";
            // 
            // lanDebugAddress_textBox
            // 
            this.lanDebugAddress_textBox.Location = new System.Drawing.Point(103, 59);
            this.lanDebugAddress_textBox.Name = "lanDebugAddress_textBox";
            this.lanDebugAddress_textBox.Size = new System.Drawing.Size(183, 20);
            this.lanDebugAddress_textBox.TabIndex = 4;
            // 
            // debugAddress_label
            // 
            this.debugAddress_label.AutoSize = true;
            this.debugAddress_label.Location = new System.Drawing.Point(12, 62);
            this.debugAddress_label.Name = "debugAddress_label";
            this.debugAddress_label.Size = new System.Drawing.Size(83, 13);
            this.debugAddress_label.TabIndex = 22;
            this.debugAddress_label.Text = "Debug Address:";
            // 
            // Capability_label
            // 
            this.Capability_label.AutoSize = true;
            this.Capability_label.Location = new System.Drawing.Point(12, 208);
            this.Capability_label.Name = "Capability_label";
            this.Capability_label.Size = new System.Drawing.Size(109, 13);
            this.Capability_label.TabIndex = 23;
            this.Capability_label.Text = "Additional Capabilities";
            // 
            // dataGridViewCapabilities
            // 
            this.dataGridViewCapabilities.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewCapabilities.Location = new System.Drawing.Point(12, 227);
            this.dataGridViewCapabilities.Name = "dataGridViewCapabilities";
            this.dataGridViewCapabilities.Size = new System.Drawing.Size(274, 114);
            this.dataGridViewCapabilities.TabIndex = 24;
            this.dataGridViewCapabilities.RowValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewCapabilities_RowValidated);
            // 
            // UpdateDeviceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(551, 377);
            this.Controls.Add(this.dataGridViewCapabilities);
            this.Controls.Add(this.Capability_label);
            this.Controls.Add(this.lanDebugAddress_textBox);
            this.Controls.Add(this.debugAddress_label);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.deviceType_textBox);
            this.Controls.Add(this.deviceType_label);
            this.Controls.Add(this.port_textBox);
            this.Controls.Add(this.port_label);
            this.Controls.Add(this.deviceAddress_textBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cancel_button);
            this.Controls.Add(this.oK_button);
            this.Controls.Add(this.adminPassword_textBox);
            this.Controls.Add(this.adminPassword_label);
            this.Controls.Add(this.adminId_textBox);
            this.Controls.Add(this.adminID_label);
            this.Controls.Add(this.description_textBox);
            this.Controls.Add(this.description_label);
            this.Controls.Add(this.deviceId_textBox);
            this.Controls.Add(this.deviceId_label);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "UpdateDeviceForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add device";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewCapabilities)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label deviceId_label;
        private System.Windows.Forms.TextBox deviceId_textBox;
        private System.Windows.Forms.Label description_label;
        private System.Windows.Forms.TextBox description_textBox;
        private System.Windows.Forms.TextBox adminId_textBox;
        private System.Windows.Forms.Label adminID_label;
        private System.Windows.Forms.TextBox adminPassword_textBox;
        private System.Windows.Forms.Label adminPassword_label;
        private System.Windows.Forms.Button oK_button;
        private System.Windows.Forms.Button cancel_button;
        private System.Windows.Forms.TextBox port_textBox;
        private System.Windows.Forms.Label port_label;
        private System.Windows.Forms.TextBox deviceAddress_textBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox deviceType_textBox;
        private System.Windows.Forms.Label deviceType_label;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox lanDebugAddress_textBox;
        private System.Windows.Forms.Label debugAddress_label;
        private System.Windows.Forms.Label Capability_label;
        private System.Windows.Forms.DataGridView dataGridViewCapabilities;
    }
}