
namespace HP.GFriend.Utils.Git
{
    partial class BranchControl
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
            this.treeViewBranches = new System.Windows.Forms.TreeView();
            this.textBoxBranch = new System.Windows.Forms.TextBox();
            this.pictureBoxCancel = new System.Windows.Forms.PictureBox();
            this.pictureBoxCheckOut = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCancel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCheckOut)).BeginInit();
            this.SuspendLayout();
            // 
            // treeViewBranches
            // 
            this.treeViewBranches.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewBranches.Location = new System.Drawing.Point(3, 32);
            this.treeViewBranches.Name = "treeViewBranches";
            this.treeViewBranches.Size = new System.Drawing.Size(169, 227);
            this.treeViewBranches.TabIndex = 0;
            this.treeViewBranches.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewBranches_NodeMouseClick);
            // 
            // textBoxBranch
            // 
            this.textBoxBranch.Location = new System.Drawing.Point(3, 6);
            this.textBoxBranch.Name = "textBoxBranch";
            this.textBoxBranch.Size = new System.Drawing.Size(115, 20);
            this.textBoxBranch.TabIndex = 3;
            // 
            // pictureBoxCancel
            // 
            this.pictureBoxCancel.Image = global::HP.GFriend.Utils.Git.Properties.Resources.Cancel;
            this.pictureBoxCancel.Location = new System.Drawing.Point(149, 5);
            this.pictureBoxCancel.Name = "pictureBoxCancel";
            this.pictureBoxCancel.Size = new System.Drawing.Size(19, 25);
            this.pictureBoxCancel.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxCancel.TabIndex = 4;
            this.pictureBoxCancel.TabStop = false;
            this.pictureBoxCancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // pictureBoxCheckOut
            // 
            this.pictureBoxCheckOut.Image = global::HP.GFriend.Utils.Git.Properties.Resources.Checkout;
            this.pictureBoxCheckOut.Location = new System.Drawing.Point(120, 5);
            this.pictureBoxCheckOut.Name = "pictureBoxCheckOut";
            this.pictureBoxCheckOut.Size = new System.Drawing.Size(23, 25);
            this.pictureBoxCheckOut.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxCheckOut.TabIndex = 2;
            this.pictureBoxCheckOut.TabStop = false;
            this.pictureBoxCheckOut.Click += new System.EventHandler(this.Checkout_Click);
            // 
            // BranchControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.Controls.Add(this.pictureBoxCancel);
            this.Controls.Add(this.textBoxBranch);
            this.Controls.Add(this.pictureBoxCheckOut);
            this.Controls.Add(this.treeViewBranches);
            this.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "BranchControl";
            this.Size = new System.Drawing.Size(175, 262);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCancel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCheckOut)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeViewBranches;
        private System.Windows.Forms.PictureBox pictureBoxCheckOut;
        private System.Windows.Forms.TextBox textBoxBranch;
        private System.Windows.Forms.PictureBox pictureBoxCancel;
    }
}
