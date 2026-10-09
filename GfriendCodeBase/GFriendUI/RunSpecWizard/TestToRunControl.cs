using HP.GFriend.Core.Execution;
using HP.GFriend.Core.Execution.Spec;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HP.GFriend.UI.RunSpecWizard
{
    public partial class TestToRunControl : UserControl
    {
        private List<string> TestCases;
        public bool Selected { get; set; } = false;
        public event EventHandler<EventArgs> CloseClick;
        public TestRun TargetTest { get; set; }
        public TestToRunControl()
        {
            InitializeComponent();
        }
        public TestToRunControl(TestRun testRun)
        {
            InitializeComponent();
            listBoxTC.SelectedValueChanged -= ListBoxTC_SelectedValueChanged;
            TestCases = new List<string>();
            TargetTest = testRun;
            labelScriptPath.Text = Path.GetFileName(testRun.TestSuitePath);
            TestCases = Parser.ParseTestSuite(testRun.TestSuitePath).TargetTestSuite.TestCases.Select(s => s.Name).ToList();
            numericUpDownRepeat.Value = testRun.RepeatCount;
            comboBoxDevice.Text = testRun.DefaultDevice;
            if(TargetTest.TestCaseToRun.Count > 0)
            {
                LabelSelectTC_Click(null, null);
                for(int i =0; i<listBoxTC.Items.Count; i++)
                {
                    if(TargetTest.TestCaseToRun.Contains(listBoxTC.Items[i].ToString()))
                    {
                        listBoxTC.SetSelected(i, true);
                    }
                }
            }
            listBoxTC.SelectedValueChanged += ListBoxTC_SelectedValueChanged;
        }

        public void PreventEditingDefaultValue()
        {
            buttonDelete.Visible = false;
            labelSelectTC.Visible = false;
            listBoxTC.Enabled = false;
            numericUpDownRepeat.Enabled = false;
        }
        
        public void ShowDeviceSelection()
        {
            labelDevice.Visible = true;
            comboBoxDevice.Visible = true;
        }

        public void PreventEditingDeviceSelection()
        {
            comboBoxDevice.Enabled = false;
        }

        public void DeviceSelectionChanged(object sender, EventArgs e)
        {
            ListView lv = (ListView)sender;
            comboBoxDevice.Items.Clear();
            foreach(ListViewItem lvi in lv.CheckedItems)
            {
                comboBoxDevice.Items.Add(lvi.Text);
            }
            if(comboBoxDevice.Items.Count > 0)
            {
                comboBoxDevice.SelectedIndex = 0;
            }
        }

        private void LabelSelectTC_Click(object sender, EventArgs e)
        {
            int targetHeight = 20 * TestCases.Count;
            if(targetHeight < 40)
            {
                targetHeight = 40;
            }
            if(targetHeight > 300)
            {
                targetHeight = 300;
            }

            Height += targetHeight;
            listBoxTC.Height = targetHeight;
            listBoxTC.Items.AddRange(TestCases.ToArray());
            listBoxTC.Visible = true;
        }

        private void ListBoxTC_SelectedValueChanged(object sender, EventArgs e)
        {
            TargetTest.TestCaseToRun = listBoxTC.SelectedItems.OfType<string>().ToList();
        }

        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            CloseClick?.Invoke(this, null);
        }

        private void NumericUpDownRepeat_ValueChanged(object sender, EventArgs e)
        {
            TargetTest.RepeatCount = (int)numericUpDownRepeat.Value;
        }

        private void ComboBoxDevice_SelectedValueChanged(object sender, EventArgs e)
        {
            TargetTest.DefaultDevice = comboBoxDevice.SelectedItem.ToString();
        }
    }
}
