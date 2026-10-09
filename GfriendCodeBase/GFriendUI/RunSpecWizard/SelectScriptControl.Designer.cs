namespace HP.GFriend.UI.RunSpecWizard
{
    partial class SelectScriptControl
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
            this.gfScriptFileList = new HP.GFriend.UI.Controls.GFScriptFileList();
            this.SuspendLayout();
            // 
            // flowLayoutPanelDetail
            // 
            this.flowLayoutPanelDetail.AutoScroll = true;
            this.flowLayoutPanelDetail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelDetail.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.flowLayoutPanelDetail.Location = new System.Drawing.Point(337, 3);
            this.flowLayoutPanelDetail.Name = "flowLayoutPanelDetail";
            this.flowLayoutPanelDetail.Size = new System.Drawing.Size(460, 384);
            this.flowLayoutPanelDetail.TabIndex = 1;
            // 
            // gfScriptFileList
            // 
            this.gfScriptFileList.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gfScriptFileList.Location = new System.Drawing.Point(3, 3);
            this.gfScriptFileList.Name = "gfScriptFileList";
            this.gfScriptFileList.Size = new System.Drawing.Size(328, 384);
            this.gfScriptFileList.TabIndex = 0;
            this.gfScriptFileList.OnUpdateSelectedFile += new System.EventHandler<System.Windows.Forms.TreeViewEventArgs>(this.GfScriptFileList_OnUpdateSelectedFile);
            // 
            // SelectScriptControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.flowLayoutPanelDetail);
            this.Controls.Add(this.gfScriptFileList);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "SelectScriptControl";
            this.Size = new System.Drawing.Size(800, 390);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.GFScriptFileList gfScriptFileList;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelDetail;
    }
}
