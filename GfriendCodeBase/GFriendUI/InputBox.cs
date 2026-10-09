using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HP.GFriend.UI
{
    public partial class InputBox : Form
    {
       public string textTextBox { get; set; }

        public InputBox(string title = "", string label = "")
        {
            InitializeComponent();

            this.Text = title;
            inputBox_label.Text = label;
        }

        private void inputBox_OK_button_Click(object sender, EventArgs e)
        {
            textTextBox = inputBox_textBox.Text;
            DialogResult = DialogResult.OK;            
            this.Close();
        }
        
        private void inputBox_textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                textTextBox = inputBox_textBox.Text;
                DialogResult = DialogResult.OK;
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                textTextBox = inputBox_textBox.Text;
                DialogResult = DialogResult.OK;
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void InputBox_Load(object sender, EventArgs e)
        {
            inputBox_textBox.Focus();
        }
    }
}
