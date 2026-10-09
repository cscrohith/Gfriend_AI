using HP.GFriend.Keywords;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace HP.GFriend.UI.Device
{
    public partial class UpdateDeviceForm : Form
    {
        public DeviceUnderTest _deviceInfo { get; private set; }
        private BindingList<DeviceUnderTest.Capability> _bindingList;

        public UpdateDeviceForm()
        {
            _deviceInfo = new DeviceUnderTest();

            InitializeComponent();
            _bindingList = new BindingList<DeviceUnderTest.Capability>();
            dataGridViewCapabilities.DataSource = _bindingList;

            // Set placeholder text for Device Type field
            SetDeviceTypePlaceholder();

            // Disable OK button initially until Device Type is selected
            UpdateOkButtonState();
        }

        public UpdateDeviceForm(DeviceUnderTest deviceInfo)
        {
            _deviceInfo = new DeviceUnderTest();

            InitializeComponent();            
            UpdateDevice(deviceInfo);

            // Update OK button state after loading device info
            UpdateOkButtonState();
        }

        private void oK_button_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(deviceId_textBox.Text))
            {
                MessageBox.Show("Please enter value to Device Id field", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                deviceId_textBox.Focus();
            }
            else if(string.IsNullOrEmpty(deviceAddress_textBox.Text))
            {
                MessageBox.Show("Please enter value to Device Address field", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                deviceAddress_textBox.Focus();
            }
            else if(string.IsNullOrEmpty(deviceType_textBox.Text))
            {
                MessageBox.Show("Please select a Device Type", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                deviceType_textBox.Focus();
            }
            else
            {
                UpdateDeviceInfo();
                DialogResult = DialogResult.OK;
            }            
        }
        
        private void cancel_button_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        public void UpdateDevice(DeviceUnderTest deviceInfo)
        {
            this.Name = "Edit device";
            deviceId_textBox.Enabled = false;

            deviceId_textBox.Text = deviceInfo.DeviceId;
            deviceAddress_textBox.Text = deviceInfo.DeviceAddress;

            if (!string.IsNullOrEmpty(deviceInfo.LanDebugAddress))
            {
                lanDebugAddress_textBox.Text = deviceInfo.LanDebugAddress;
            }
            if (!string.IsNullOrEmpty(deviceInfo.Description))
            {
                description_textBox.Text = deviceInfo.Description;
            }
            if (deviceInfo.Port != 0)
            {
                port_textBox.Text = deviceInfo.Port.ToString();
            }
            if (!string.IsNullOrEmpty(deviceInfo.AdminId))
            {
                adminId_textBox.Text = deviceInfo.AdminId;
            }
            if (!string.IsNullOrEmpty(deviceInfo.AdminPassword))
            {
                adminPassword_textBox.Text = deviceInfo.AdminPassword;
            }
            if (!string.IsNullOrEmpty(deviceInfo.DeviceType))
            {
                deviceType_textBox.Text = deviceInfo.DeviceType;
            }
            _bindingList = new BindingList<DeviceUnderTest.Capability>(deviceInfo.AdditionalCapabilites);
            _bindingList.AllowNew = true;
            dataGridViewCapabilities.DataSource = _bindingList;
        }

        private void UpdateDeviceInfo()
        {

            _deviceInfo.DeviceId = deviceId_textBox.Text;
            _deviceInfo.DeviceAddress = deviceAddress_textBox.Text;

            if(lanDebugAddress_textBox.Text != null)
            {
                _deviceInfo.LanDebugAddress = lanDebugAddress_textBox.Text;
            }

            if (description_textBox.Text != null)
            {
                _deviceInfo.Description = description_textBox.Text;
            }

            int devicePort;
            if(port_textBox.Text != null && Int32.TryParse(port_textBox.Text, out devicePort))
            {
                _deviceInfo.Port = devicePort;
            }
            
            if(adminId_textBox.Text != null)
            {
                _deviceInfo.AdminId = adminId_textBox.Text;
            }
            
            if(adminPassword_textBox.Text != null)
            {
                _deviceInfo.AdminPassword = adminPassword_textBox.Text;
            }

            if(deviceType_textBox.Text != null)
            {
                _deviceInfo.DeviceType = deviceType_textBox.Text;
            }
        }

        private void dataGridViewCapabilities_RowValidated(object sender, DataGridViewCellEventArgs e)
        {
            _deviceInfo.AdditionalCapabilites = _bindingList.ToList();
            dataGridViewCapabilities.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
        }

        /// <summary>
        /// Handles the Click event for the Device Type textbox.
        /// Opens the PlatformTypeSelectionForm dialog to allow the user to select a platform type.
        /// </summary>
        private void DeviceType_TextBox_Click(object sender, EventArgs e)
        {
            // Remove placeholder text before opening dialog
            string currentValue = deviceType_textBox.Text;
            if (currentValue == "Select the platform")
            {
                currentValue = string.Empty;
            }

            // Create the platform type selection form with the current value
            using (var selectionForm = new PlatformTypeSelectionForm(currentValue))
            {
                // Show the dialog and check if the user clicked OK
                if (selectionForm.ShowDialog(this) == DialogResult.OK)
                {
                    // Update the textbox with the selected platform type
                    if (selectionForm.SelectedPlatformType.HasValue)
                    {
                        deviceType_textBox.ForeColor = System.Drawing.Color.Black;
                        deviceType_textBox.Text = selectionForm.SelectedPlatformType.Value.ToString();
                        // Update OK button state after selection
                        UpdateOkButtonState();
                    }
                }
                else
                {
                    // If cancelled and still empty, restore placeholder
                    if (string.IsNullOrEmpty(deviceType_textBox.Text))
                    {
                        SetDeviceTypePlaceholder();
                    }
                }
            }
        }

        /// <summary>
        /// Sets the placeholder text for the Device Type field.
        /// </summary>
        private void SetDeviceTypePlaceholder()
        {
            if (string.IsNullOrEmpty(deviceType_textBox.Text))
            {
                deviceType_textBox.ForeColor = System.Drawing.Color.Gray;
                deviceType_textBox.Text = "Select the platform";
            }
        }

        /// <summary>
        /// Handles the TextChanged event for the Device Type textbox.
        /// Updates the OK button state when the device type changes.
        /// </summary>
        private void DeviceType_TextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateOkButtonState();
        }

        /// <summary>
        /// Updates the enabled state of the OK button based on required field validation.
        /// The OK button is enabled only when Device Type is selected (not placeholder).
        /// </summary>
        private void UpdateOkButtonState()
        {
            bool hasValidDeviceType = !string.IsNullOrEmpty(deviceType_textBox.Text) 
                                     && deviceType_textBox.Text != "Select the platform";
            oK_button.Enabled = hasValidDeviceType;
        }

    }
}
