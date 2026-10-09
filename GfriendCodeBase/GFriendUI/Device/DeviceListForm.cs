using HP.GFriend.Keywords;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Windows.Forms;

namespace HP.GFriend.UI.Device
{
    public partial class DeviceListForm : Form
    {
        private string _devicefilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DeviceList.xml");
        private DeviceInfo _deviceInfo;
        

        private readonly string DeviceId = "DeviceId";
        private readonly string Description = "Description";
        private readonly string DeviceAddress = "DeviceAddress";
        private readonly string LanDebugAddress = "LanDebugAddress";
        private readonly string Port = "Port";
        private readonly string AdminId = "AdminId";
        private readonly string AdminPassword = "AdminPassword";
        private readonly string DeviceType = "DeviceType";

        private List<DeviceUnderTest> _dutFromSTB = new List<DeviceUnderTest>();

        public delegate void sendAssetServerDelegate(string serverAddress);
        public event sendAssetServerDelegate sendAssetServer;

        public DeviceListForm()
        {
            InitializeComponent();
            _deviceInfo = DeviceInfo.Load(_devicefilePath);
            deviceList_dataGridView.DataSource = _deviceInfo.Devices;
        }

        public DeviceListForm(string assetInventoryServer)
        {
            InitializeComponent();
            _deviceInfo = DeviceInfo.Load(_devicefilePath);
            deviceList_dataGridView.DataSource = _deviceInfo.Devices;
            stbServer_textBox.Text = assetInventoryServer;
        }

        private void InitDeviceList()
        {
            deviceList_dataGridView.DataSource = null;
            deviceList_dataGridView.DataSource = _deviceInfo.Devices;
        }

        private void DeviceListForm_Load(object sender, EventArgs e)
        {
            deviceListSplitContainer.Panel2Collapsed = true;

            deviceList_dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            deviceList_dataGridView.ReadOnly = true;

            assetInventory_dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            assetInventory_dataGridView.ReadOnly = true;

            InitDeviceList();
        }

        private void add_button_Click(object sender, EventArgs e)
        {            
            DeviceUnderTest deviceUnderTest = new DeviceUnderTest();
            using (var preferences = new UpdateDeviceForm())
            {
                if(preferences.ShowDialog() == DialogResult.OK)
                {
                    deviceUnderTest = preferences._deviceInfo;
                    _deviceInfo.Update(deviceUnderTest);
                    InitDeviceList();
                }
            }
            
        }
        
        private void edit_button_Click(object sender, EventArgs e)
        {
            if (deviceList_dataGridView.SelectedRows.Count > 0)
            {
                var selectedRow = deviceList_dataGridView.SelectedRows[0];
                var currentRow = selectedRow.Index;

                DeviceUnderTest deviceUnderTest = _deviceInfo.GetDevice(selectedRow.Cells[0].Value.ToString());
                using (var preferences = new UpdateDeviceForm(deviceUnderTest))
                {
                    if (preferences.ShowDialog() == DialogResult.OK)
                    {
                        deviceUnderTest = preferences._deviceInfo;
                    }
                }
                _deviceInfo.Update(deviceUnderTest);
                InitDeviceList();
                deviceList_dataGridView.Focus();
                deviceList_dataGridView.CurrentCell = deviceList_dataGridView.Rows[currentRow].Cells[0];
            }
        }

        private void remove_button_Click(object sender, EventArgs e)
        {
            if (deviceList_dataGridView.SelectedRows.Count > 0)
            {
                RemoveDevice();
            }
        }

        
        private void RemoveDevice()
        {
            if (deviceList_dataGridView.SelectedRows.Count > 0)
            {
                _deviceInfo.Remove(deviceList_dataGridView.SelectedRows[0].Cells[0].Value.ToString());
                InitDeviceList();
            }
        }

        private void stbServer_button_Click(object sender, EventArgs e)
        {
            if (stbServer_button.Text.Equals("Connect"))
            {
                ConnectToSTBServer();                
            }
            else
            {
                DisconnectToSTBServer();                
            }            
        }

        private void ConnectToSTBServer()
        {
            try
            {
                try
                {
                    Ping pingSender = new Ping();
                    PingReply pingReply = pingSender.Send(stbServer_textBox.Text);

                    if (!pingReply.Status.Equals(IPStatus.Success))
                    {
                        MessageBox.Show($"Connection to Asset Inventory server is failed.\r\nPlease check connection to Server.");
                        return;
                    }
                }
                catch (PingException)
                {
                    MessageBox.Show($"Connection to Asset Inventory is failed.\r\nPlease check connection to Server.");
                    return;
                }

                deviceListSplitContainer.Panel2Collapsed = false;
                assetInventory_dataGridView.Enabled = true;
                stbServer_textBox.Enabled = false;
                addfromSTB_button.Visible = true;

                _dutFromSTB = new List<DeviceUnderTest>();

                GetDeviceInfofromSTBServer();

                assetInventory_dataGridView.DataSource = _dutFromSTB;
                stbServer_button.Text = "Disconnect";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fail to connect to Asset Inventory server: {stbServer_textBox.Text}");
                GFLogger.Logger.Error(ex);
                DisconnectToSTBServer();
            }
        }

        private void DisconnectToSTBServer()
        {
            assetInventory_dataGridView.DataSource = null;

            stbServer_button.Text = "Connect";
            deviceListSplitContainer.Panel2Collapsed = true;
            assetInventory_dataGridView.Enabled = false;
            stbServer_textBox.Enabled = true;
            addfromSTB_button.Visible = false;
        }

        private void GetDeviceInfofromSTBServer(string database = "AssetInventory")
        {
            string assetInventoryServer = stbServer_textBox.Text;

            if (!string.IsNullOrEmpty(assetInventoryServer))
            {
                SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder()
                {
                    DataSource = assetInventoryServer,
                    InitialCatalog = database,
                    PersistSecurityInfo = true,
                    UserID = "asset_admin",
                    Password = "asset_admin",
                    MultipleActiveResultSets = true
                };

                using (SqlConnection connection = new SqlConnection(sqlBuilder.ToString()))
                {
                    connection.Open();
                    string sqlSelect = $"SELECT DISTINCT * FROM Printer p JOIN Asset a on p.AssetId=a.AssetId";

                    using (SqlCommand command = new SqlCommand(sqlSelect, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                _dutFromSTB.Add(new DeviceUnderTest()
                                {
                                    DeviceId = reader["Assetid"] as string,
                                    DeviceAddress = reader["Address1"] as string,
                                    LanDebugAddress = reader["Address2"] as string,
                                    AdminPassword = reader["Password"] as string,
                                    Description = reader["Description"] as string,
                                    DeviceType = Convert.ToString(reader["Capability"]),
                                    Port = Convert.ToInt32(reader["PortNumber"])
                                });
                            }
                        }
                    }
                }
            }            
        }

        private void addfromSTB_button_Click(object sender, EventArgs e)
        {            
            if (assetInventory_dataGridView.CurrentCell != null)
            {
                DeviceUnderTest dut = new DeviceUnderTest()
                {
                    DeviceId = assetInventory_dataGridView.Rows[assetInventory_dataGridView.CurrentCell.RowIndex].Cells[DeviceId].Value?.ToString(),
                    Description = assetInventory_dataGridView.Rows[assetInventory_dataGridView.CurrentCell.RowIndex].Cells[Description].Value?.ToString(),
                    DeviceAddress = assetInventory_dataGridView.Rows[assetInventory_dataGridView.CurrentCell.RowIndex].Cells[DeviceAddress].Value?.ToString(),
                    LanDebugAddress = assetInventory_dataGridView.Rows[assetInventory_dataGridView.CurrentCell.RowIndex].Cells[LanDebugAddress].Value?.ToString(),
                    Port = Convert.ToInt32(assetInventory_dataGridView.Rows[assetInventory_dataGridView.CurrentCell.RowIndex].Cells[Port].Value?.ToString()),
                    AdminId = assetInventory_dataGridView.Rows[assetInventory_dataGridView.CurrentCell.RowIndex].Cells[AdminId].Value?.ToString(),
                    AdminPassword = assetInventory_dataGridView.Rows[assetInventory_dataGridView.CurrentCell.RowIndex].Cells[AdminPassword].Value?.ToString(),
                    DeviceType = assetInventory_dataGridView.Rows[assetInventory_dataGridView.CurrentCell.RowIndex].Cells[DeviceType].Value?.ToString()
                };

                _deviceInfo.Update(dut);
                InitDeviceList();
            }
        }

        private void stbServer_textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ConnectToSTBServer();
            }
        }

        private void DeviceListForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!string.IsNullOrEmpty(stbServer_textBox.Text))
            {
                this.sendAssetServer(stbServer_textBox.Text);
            }
        }
    }
}
