using HP.GFriend.Keywords;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace HP.GFriend.UI.RunSpecWizard
{
    public partial class TestRunSpecWizardForm : Form
    {
        private RunSpecWizardSequence _sequence;
        public TestRunSpecWizardForm()
        {
            InitializeComponent();
        }
        public TestRunSpecWizardForm(string scriptPath, List<DeviceUnderTest> deviceList)
        {
            InitializeComponent();
            _sequence = new RunSpecWizardSequence(scriptPath, deviceList);
            RefreshForm();
        }

        private void RefreshForm()
        {
            labelTitle.Text = _sequence.Title;
            panelMain.Controls.Clear();
            panelMain.Controls.Add(_sequence.GetCurrentControl());
            if(!_sequence.HaveNext)
            {
                buttonNext.Text = "Close";
            }
            else
            {
                buttonNext.Text = "Next >";
            }

            buttonPrevious.Visible = _sequence.HavePrevious;
        }

        private void ButtonNext_Click(object sender, EventArgs e)
        {
            if(buttonNext.Text.Equals("Close"))
            {
                Close();
                return;
            }

            _sequence.Next();
            _sequence.SetCurrentControl();
            RefreshForm();
        }

        private void ButtonPrevious_Click(object sender, EventArgs e)
        {
            _sequence.Previous();
            _sequence.SetCurrentControl();
            RefreshForm();
        }
    }
}
