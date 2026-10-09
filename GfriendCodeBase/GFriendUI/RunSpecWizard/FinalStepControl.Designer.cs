namespace HP.GFriend.UI.RunSpecWizard
{
    partial class FinalStepControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FinalStepControl));
            this.fastColoredTextBoxXML = new FastColoredTextBoxNS.FastColoredTextBox();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonExecute = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.fastColoredTextBoxXML)).BeginInit();
            this.SuspendLayout();
            // 
            // fastColoredTextBoxXML
            // 
            this.fastColoredTextBoxXML.AutoCompleteBracketsList = new char[] {
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
            this.fastColoredTextBoxXML.AutoIndentCharsPatterns = "";
            this.fastColoredTextBoxXML.AutoScrollMinSize = new System.Drawing.Size(2, 14);
            this.fastColoredTextBoxXML.BackBrush = null;
            this.fastColoredTextBoxXML.CharHeight = 14;
            this.fastColoredTextBoxXML.CharWidth = 8;
            this.fastColoredTextBoxXML.CommentPrefix = null;
            this.fastColoredTextBoxXML.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.fastColoredTextBoxXML.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.fastColoredTextBoxXML.Font = new System.Drawing.Font("Courier New", 9.75F);
            this.fastColoredTextBoxXML.IsReplaceMode = false;
            this.fastColoredTextBoxXML.Language = FastColoredTextBoxNS.Language.XML;
            this.fastColoredTextBoxXML.LeftBracket = '<';
            this.fastColoredTextBoxXML.LeftBracket2 = '(';
            this.fastColoredTextBoxXML.Location = new System.Drawing.Point(3, 3);
            this.fastColoredTextBoxXML.Name = "fastColoredTextBoxXML";
            this.fastColoredTextBoxXML.Paddings = new System.Windows.Forms.Padding(0);
            this.fastColoredTextBoxXML.ReadOnly = true;
            this.fastColoredTextBoxXML.RightBracket = '>';
            this.fastColoredTextBoxXML.RightBracket2 = ')';
            this.fastColoredTextBoxXML.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.fastColoredTextBoxXML.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("fastColoredTextBoxXML.ServiceColors")));
            this.fastColoredTextBoxXML.ShowLineNumbers = false;
            this.fastColoredTextBoxXML.Size = new System.Drawing.Size(794, 336);
            this.fastColoredTextBoxXML.TabIndex = 0;
            this.fastColoredTextBoxXML.Zoom = 100;
            // 
            // buttonSave
            // 
            this.buttonSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(192)))));
            this.buttonSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSave.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSave.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.buttonSave.Location = new System.Drawing.Point(3, 345);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(129, 32);
            this.buttonSave.TabIndex = 1;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = false;
            this.buttonSave.Click += new System.EventHandler(this.ButtonSave_Click);
            // 
            // buttonExecute
            // 
            this.buttonExecute.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(192)))));
            this.buttonExecute.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonExecute.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonExecute.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(238)))), ((int)(((byte)(233)))));
            this.buttonExecute.Location = new System.Drawing.Point(154, 345);
            this.buttonExecute.Name = "buttonExecute";
            this.buttonExecute.Size = new System.Drawing.Size(129, 32);
            this.buttonExecute.TabIndex = 2;
            this.buttonExecute.Text = "Execute Now";
            this.buttonExecute.UseVisualStyleBackColor = false;
            this.buttonExecute.Click += new System.EventHandler(this.ButtonExecute_Click);
            // 
            // FinalStepControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonExecute);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.fastColoredTextBoxXML);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "FinalStepControl";
            this.Size = new System.Drawing.Size(800, 390);
            ((System.ComponentModel.ISupportInitialize)(this.fastColoredTextBoxXML)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private FastColoredTextBoxNS.FastColoredTextBox fastColoredTextBoxXML;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonExecute;
    }
}
