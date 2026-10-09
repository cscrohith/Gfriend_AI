using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HP.GFriend.Keywords
{
    public partial class UserInteractionForm : Form
    {
        private KeywordResult _result;
        public enum Type
        {
            Confirm,
            Verification
        }
        public UserInteractionForm()
        {
            InitializeComponent();
            _result = new KeywordResult(KeywordResults.Pass);
        }

        public UserInteractionForm(Type type, KeywordResult result, string message)
        {
            InitializeComponent();
            textBoxMessage.Text = message;
            _result = result;
            switch (type)
            {
                case Type.Confirm:
                    InitialzeConfirmForm();
                    break;
                case Type.Verification:
                    InitialzeVerificationForm();
                    break;
            }
        }

        private void InitialzeConfirmForm()
        {
            buttonOK.Visible = true;
            buttonPass.Visible = false;
            buttonFail.Visible = false;
            textBoxReason.Visible = false;
            labelReason.Visible = false;
            Text = "Confirm";
            
            buttonOK.Click += ButtonOK_Click;
            buttonOK.Focus();
        }
        
        private void InitialzeVerificationForm()
        {
            buttonOK.Visible = false;
            buttonPass.Visible = true;
            buttonFail.Visible = true;
            textBoxReason.Visible = true;
            labelReason.Visible = true;
            Text = "Verification";

            buttonPass.Click += ButtonPass_Click;
            buttonFail.Click += ButtonFail_Click;
            textBoxReason.Focus();
        }

        private void ButtonFail_Click(object sender, EventArgs e)
        {
            _result.Result = KeywordResults.Fail;
            if (!string.IsNullOrEmpty(textBoxReason.Text))
            {
                _result.Output = textBoxReason.Text;
            }
            Close();
        }

        private void ButtonPass_Click(object sender, EventArgs e)
        {
            _result.Result = KeywordResults.Pass;
            if(!string.IsNullOrEmpty(textBoxReason.Text))
            {
                _result.Output = textBoxReason.Text;
            }
            Close();
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            Close();
        }


    }
}
