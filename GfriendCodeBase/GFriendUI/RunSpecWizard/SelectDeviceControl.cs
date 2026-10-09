using HP.GFriend.Core.Execution.Spec;
using HP.GFriend.Keywords;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace HP.GFriend.UI.RunSpecWizard
{
    internal partial class SelectDeviceControl : UserControl, IRunSpecWizardControl
    {
        private RunSpecWizardSequence _sequence;
        private Dictionary<ListViewItem, DeviceUnderTest> _listviewItemMapping;
        public SelectDeviceControl()
        {
            InitializeComponent();
        }

        public UserControl GetUserControl()
        {
            return this;
        }

        public void SetSequence(RunSpecWizardSequence sqeuence)
        {
            listViewDevices.ItemChecked -= ListViewDevices_ItemChecked;
            _sequence = sqeuence;

            // Tests
            flowLayoutPanelDetail.Controls.Clear();
            foreach(TestRun testRun in _sequence.TestRunSpec.TestsToRun)
            {
                TestToRunControl tControl = new TestToRunControl(testRun);
                tControl.PreventEditingDefaultValue();
                tControl.ShowDeviceSelection();
                
                flowLayoutPanelDetail.Controls.Add(tControl);
            }

            // Devices
            _listviewItemMapping = new Dictionary<ListViewItem, DeviceUnderTest>();
            listViewDevices.Items.Clear();
            foreach (DeviceUnderTest dut in _sequence.DeviceList)
            {
                ListViewItem lvItem = new ListViewItem(dut.DeviceId);
                lvItem.SubItems.Add(new ListViewItem.ListViewSubItem(lvItem, dut.DeviceAddress));
                _listviewItemMapping.Add(lvItem, dut);
                listViewDevices.Items.Add(lvItem);
            }
            
            foreach(DeviceUnderTest dut in _sequence.DeviceUnderTests)
            {
                ListViewItem lvi = _listviewItemMapping.FirstOrDefault(x => x.Value == dut).Key;
                lvi.Checked = true;
            }

            listViewDevices.ItemChecked += ListViewDevices_ItemChecked;
            foreach(TestToRunControl tControl in flowLayoutPanelDetail.Controls)
            {
                listViewDevices.ItemChecked += tControl.DeviceSelectionChanged;
            }
            

        }

        private void ListViewDevices_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            List<DeviceUnderTest> deviceUnderTests = new List<DeviceUnderTest>();
            foreach(ListViewItem lvi in listViewDevices.CheckedItems)
            {
                deviceUnderTests.Add(_listviewItemMapping[lvi]);
            }
            foreach (TestRun testRun in _sequence.TestRunSpec.TestsToRun)
            {
                testRun.DeviceUnderTests = deviceUnderTests;
            }
            _sequence.DeviceUnderTests = deviceUnderTests;
        }
    }
}
