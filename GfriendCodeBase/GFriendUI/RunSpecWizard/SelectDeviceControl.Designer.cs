namespace HP.GFriend.UI.RunSpecWizard
{
    partial class SelectDeviceControl
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
            this.flowLayoutPanelDetail = new System.Windows.Forms.FlowLayoutPanel();
            this.listViewDevices = new System.Windows.Forms.ListView();
            this.columnHeaderID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // flowLayoutPanelDetail
            // 
            this.flowLayoutPanelDetail.AutoScroll = true;
            this.flowLayoutPanelDetail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelDetail.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.flowLayoutPanelDetail.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanelDetail.Name = "flowLayoutPanelDetail";
            this.flowLayoutPanelDetail.Size = new System.Drawing.Size(460, 384);
            this.flowLayoutPanelDetail.TabIndex = 2;
            // 
            // listViewDevices
            // 
            this.listViewDevices.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listViewDevices.CheckBoxes = true;
            this.listViewDevices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderID,
            this.columnHeaderAddress});
            this.listViewDevices.FullRowSelect = true;
            this.listViewDevices.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewDevices.HideSelection = false;
            this.listViewDevices.Location = new System.Drawing.Point(469, 3);
            this.listViewDevices.Name = "listViewDevices";
            this.listViewDevices.Size = new System.Drawing.Size(328, 384);
            this.listViewDevices.TabIndex = 3;
            this.listViewDevices.UseCompatibleStateImageBehavior = false;
            this.listViewDevices.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderID
            // 
            this.columnHeaderID.Text = "Device ID";
            this.columnHeaderID.Width = 151;
            // 
            // columnHeaderAddress
            // 
            this.columnHeaderAddress.Text = "Device Address";
            this.columnHeaderAddress.Width = 144;
            // 
            // SelectDeviceControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listViewDevices);
            this.Controls.Add(this.flowLayoutPanelDetail);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "SelectDeviceControl";
            this.Size = new System.Drawing.Size(800, 390);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelDetail;
        private System.Windows.Forms.ListView listViewDevices;
        private System.Windows.Forms.ColumnHeader columnHeaderID;
        private System.Windows.Forms.ColumnHeader columnHeaderAddress;
    }
}
