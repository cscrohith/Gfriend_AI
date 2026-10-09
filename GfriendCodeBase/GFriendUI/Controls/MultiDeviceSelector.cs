using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace GFriendUI
{
    /// <summary>
    /// Custom control for multi-device selection with tree-view popup
    /// </summary>
    public class MultiDeviceSelector : Panel
    {
        private Label placeholderLabel;
        private Button browseButton;
        private Form popupForm;
        private TreeView deviceTreeView;
        private List<DeviceSelectionItem> selectedDevices;
        private Dictionary<string, List<string>> devicesByPlatform;

        public event EventHandler SelectionChanged;

        public MultiDeviceSelector()
        {
            selectedDevices = new List<DeviceSelectionItem>();
            devicesByPlatform = new Dictionary<string, List<string>>();

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Set panel properties
            this.BackColor = Color.White;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Height = 24;
            this.Padding = new Padding(1);

            // Create placeholder label with smaller font
            placeholderLabel = new Label
            {
                Text = "Select Target Device",
                ForeColor = Color.Gray,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0),
                Font = new Font("Segoe UI", 8F),
                Cursor = Cursors.Hand
            };
            placeholderLabel.Click += PlaceholderLabel_Click;
            this.Controls.Add(placeholderLabel);

            // Create browse button
            browseButton = new Button
            {
                Text = "...",
                Dock = DockStyle.Right,
                Width = 24,
                Height = 20,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            browseButton.FlatAppearance.BorderSize = 0;
            browseButton.Click += BrowseButton_Click;
            this.Controls.Add(browseButton);

            this.ResumeLayout(false);
        }

        /// <summary>
        /// Load target devices grouped by platform
        /// </summary>
        public void LoadTargetDevices(Dictionary<string, List<string>> platformDevices)
        {
            devicesByPlatform = new Dictionary<string, List<string>>(platformDevices);
        }

        /// <summary>
        /// Show the device selection popup
        /// </summary>
        private void ShowDeviceSelectionPopup()
        {
            if (popupForm != null && !popupForm.IsDisposed)
            {
                popupForm.Close();
            }

            popupForm = new Form
            {
                Text = "Select Target Devices",
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowIcon = false,
                Width = 350,
                Height = 400,
                BackColor = Color.White,
                TopMost = true
            };

            // Position popup below the selector
            Point screenLocation = this.Parent.PointToScreen(this.Location);
            popupForm.Location = new Point(screenLocation.X, screenLocation.Y + this.Height + 5);

            // Create TreeView
            deviceTreeView = new TreeView
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ForeColor = Color.Black,
                CheckBoxes = true,
                Font = new Font("Segoe UI", 9F),
                ItemHeight = 20
            };

            // Build tree structure
            BuildDeviceTree();

            // Add close button
            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 35,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Button OKButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(270, 5),
                Width = 70,
                Height = 25,
                FlatStyle = FlatStyle.System
            };

            Button CancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(190, 5),
                Width = 70,
                Height = 25,
                FlatStyle = FlatStyle.System
            };

            buttonPanel.Controls.Add(OKButton);
            buttonPanel.Controls.Add(CancelButton);

            popupForm.Controls.Add(deviceTreeView);
            popupForm.Controls.Add(buttonPanel);

            if (popupForm.ShowDialog() == DialogResult.OK)
            {
                CollectSelectedDevices();
                UpdateSelectedDeviceText();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Build the tree structure from platform devices
        /// </summary>
        private void BuildDeviceTree()
        {
            deviceTreeView.Nodes.Clear();

            foreach (var platform in devicesByPlatform)
            {
                TreeNode platformNode = new TreeNode(platform.Key)
                {
                    Tag = platform.Key,
                    ImageIndex = 0
                };

                foreach (var device in platform.Value)
                {
                    TreeNode deviceNode = new TreeNode(device)
                    {
                        Tag = $"{platform.Key}:{device}",
                        ImageIndex = 1
                    };

                    // Check if device is already selected
                    if (IsDeviceSelected(platform.Key, device))
                    {
                        deviceNode.Checked = true;
                    }

                    platformNode.Nodes.Add(deviceNode);
                }

                deviceTreeView.Nodes.Add(platformNode);
            }
        }

        /// <summary>
        /// Check if a device is in the selected list
        /// </summary>
        private bool IsDeviceSelected(string platform, string device)
        {
            return selectedDevices.Exists(d => d.Platform == platform && d.Device == device);
        }

        /// <summary>
        /// Collect checked devices from tree
        /// </summary>
        private void CollectSelectedDevices()
        {
            selectedDevices.Clear();

            foreach (TreeNode platformNode in deviceTreeView.Nodes)
            {
                foreach (TreeNode deviceNode in platformNode.Nodes)
                {
                    if (deviceNode.Checked)
                    {
                        string platform = platformNode.Text;
                        string device = deviceNode.Text;
                        selectedDevices.Add(new DeviceSelectionItem { Platform = platform, Device = device });
                    }
                }
            }
        }

        /// <summary>
        /// Update placeholder text based on selected devices
        /// </summary>
        private void UpdateSelectedDeviceText()
        {
            if (selectedDevices.Count == 0)
            {
                placeholderLabel.Text = "Select Target Device";
                placeholderLabel.ForeColor = Color.Gray;
            }
            else if (selectedDevices.Count == 1)
            {
                placeholderLabel.Text = selectedDevices[0].Device;
                placeholderLabel.ForeColor = Color.Black;
            }
            else if (selectedDevices.Count <= 3)
            {
                string deviceList = string.Join(", ", selectedDevices.ConvertAll(d => d.Device));
                placeholderLabel.Text = deviceList;
                placeholderLabel.ForeColor = Color.Black;
            }
            else
            {
                placeholderLabel.Text = $"{selectedDevices.Count} Devices Selected";
                placeholderLabel.ForeColor = Color.Black;
            }
        }

        /// <summary>
        /// Get list of selected devices
        /// </summary>
        public List<DeviceSelectionItem> GetSelectedDevices()
        {
            return new List<DeviceSelectionItem>(selectedDevices);
        }

        /// <summary>
        /// Get selected device platforms
        /// </summary>
        public List<string> GetSelectedPlatforms()
        {
            List<string> platforms = new List<string>();
            foreach (var device in selectedDevices)
            {
                if (!platforms.Contains(device.Platform))
                {
                    platforms.Add(device.Platform);
                }
            }
            return platforms;
        }

        /// <summary>
        /// Clear all selections
        /// </summary>
        public void ClearSelection()
        {
            selectedDevices.Clear();
            UpdateSelectedDeviceText();
        }

        private void PlaceholderLabel_Click(object sender, EventArgs e)
        {
            ShowDeviceSelectionPopup();
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            ShowDeviceSelectionPopup();
        }

        /// <summary>
        /// Data class for device selection
        /// </summary>
        public class DeviceSelectionItem
        {
            public string Platform { get; set; }
            public string Device { get; set; }

            public override string ToString()
            {
                return $"{Platform}:{Device}";
            }
        }
    }
}
