using System;
using System.Windows.Forms;

namespace HP.GFriend.UI.Tool
{
    public partial class InputArgForm : Form
    {
        public string Value { get; private set; }
        public InputArgForm()
        {
            InitializeComponent();
        }

        public InputArgForm(string name)
        {
            InitializeComponent();
            labelInput.Text += name;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            Value = textBoxValue.Text;
            Close();
        }
    }
}
