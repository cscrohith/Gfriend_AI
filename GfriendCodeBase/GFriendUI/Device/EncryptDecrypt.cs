using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace HP.GFriend.UI.Device
{
    public partial class EncryptDecrypt : Form
    {       
        private string xmlFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "EncryptedData.xml");
        public EncryptDecrypt()
        {
            InitializeComponent();
            InitializeDataGridView();
            // Set the textbox height same as DataGridView row height
            int rowHeight = dataGridView1.RowTemplate.Height;
            txtKey.Height = rowHeight+5;
            txtValue.Height = rowHeight+5;
            //btnEncrypt.Height=rowHeight+5; 
            if (File.Exists(xmlFilePath))
            {
                //dataList = LoadDataFromXml();
                LoadDataFromXml();
            }
        }
        private void InitializeDataGridView()
        {
            dataGridView1.ColumnCount = 2;
            dataGridView1.Columns[0].Name = "Variable Name";
            dataGridView1.Columns[1].Name = "Variable Value";
            dataGridView1.Columns[0].ReadOnly = true;
            dataGridView1.Columns[1].ReadOnly = true;
            // Set the font style for "Variable Value" column
            dataGridView1.Columns[1].DefaultCellStyle.Font = new System.Drawing.Font("Arial", 18);
            // Add a Button column for removing keys
            DataGridViewButtonColumn removeButtonColumn = new DataGridViewButtonColumn
            {
                Name = "Remove Variable",
                HeaderText = "Remove Variable",
                Text = "X",
                UseColumnTextForButtonValue = true
            };

            dataGridView1.Columns.Add(removeButtonColumn);
            dataGridView1.Columns["Remove Variable"].Width = 100;
            // Prevent extra empty row
            dataGridView1.AllowUserToAddRows = false;
            // Remove extra space below
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.ScrollBars = ScrollBars.Vertical; // Only show scrollbar when needed

            dataGridView1.ReadOnly = false; // Allow button clicks
            dataGridView1.CellClick += DataGridView1_CellClick;
        }
        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dataGridView1.Columns["Remove Variable"].Index)
            {
                string keyToRemove = dataGridView1.Rows[e.RowIndex].Cells[0].Value?.ToString();

                if (MessageBox.Show($"Are you sure you want to remove key '{keyToRemove}'?",
                    "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    dataGridView1.Rows.RemoveAt(e.RowIndex);
                    RemoveFromXml(keyToRemove);
                }
            }
        }
        private void RemoveFromXml(string keyToRemove)
        {
            if (!File.Exists(xmlFilePath))
                return;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<EncryptedData>));
                List<EncryptedData> dataList;

                using (TextReader reader = new StreamReader(xmlFilePath))
                {
                    dataList = (List<EncryptedData>)serializer.Deserialize(reader);
                }
                // Remove the key from the list
                dataList.RemoveAll(d => d.Key == keyToRemove);
                // Save back to XML
                using (TextWriter writer = new StreamWriter(xmlFilePath))
                {
                    serializer.Serialize(writer, dataList);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing key: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            string key = txtKey.Text;
            string value = txtValue.Text;
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                MessageBox.Show("Key and Value cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Check if the key already exists in the DataGridView
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[0].Value?.ToString() == key)
                {
                    MessageBox.Show("Duplicate key detected. Please enter a unique key.", "Duplicate Key", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            string encryptedValue = Encrypt(value, key);
            dataGridView1.Rows.Add(key, "....................");
            SaveToXml(key, value,encryptedValue);
            txtKey.Text = string.Empty; txtValue.Text= string.Empty;
        }
        private void DataGridView1_RowHeightChanged(object sender, DataGridViewRowEventArgs e)
        {
            txtKey.Height = e.Row.Height;
            txtValue.Height = e.Row.Height;
        }
        private string Encrypt(string value, string key)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = GenerateKey(key);
                    aes.IV = new byte[16]; // Zero IV for simplicity

                    using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    {
                        byte[] inputBytes = Encoding.UTF8.GetBytes(value);
                        byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                        return Convert.ToBase64String(encryptedBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Encryption failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }
        }

        private byte[] GenerateKey(string key)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(key)).Take(32).ToArray();
            }
        }
        private void SaveToXml(string key, string originalValue, string encryptedValue)
        {
            List<EncryptedData> dataList = new List<EncryptedData>();
            // Load existing data
            if (File.Exists(xmlFilePath))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<EncryptedData>));
                using (TextReader reader = new StreamReader(xmlFilePath))
                {
                    dataList = (List<EncryptedData>)serializer.Deserialize(reader);
                }
            }
            // Add new data
            dataList.Add(new EncryptedData
            {
                Key = key,
                //OriginalValue = originalValue,
                EncryptedValue = encryptedValue // Store actual encrypted value
            });
            // Save back to XML
            SaveDataToXml(dataList);
        }

        private void SaveDataToXml(List<EncryptedData> data)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<EncryptedData>));
                using (TextWriter writer = new StreamWriter(xmlFilePath))
                {
                    serializer.Serialize(writer, data);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save XML: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDataFromXml()
        {
            try
            {
                List<EncryptedData> xmldata = new List<EncryptedData>();
                if (!File.Exists(xmlFilePath))
                    return;
                XmlSerializer serializer = new XmlSerializer(typeof(List<EncryptedData>));
                using (TextReader reader = new StreamReader(xmlFilePath))
                {

                    xmldata = (List<EncryptedData>)serializer.Deserialize(reader);
                }
                if (xmldata.Count > 0)
                {
                    foreach (var data in xmldata)
                    {
                        //dataGridView1.Rows.Add(data.Key, data.OriginalValue, data.EncryptedValue);
                        dataGridView1.Rows.Add(data.Key, "....................");
                        //dataGridView1.Rows.Add(data.Key,data.EncryptedValue);
                    }
                }
            }
            catch (Exception)
            {
                return; // Return empty list if there's an error
            }
        }
        public static string DecryptByKey(string key, string xmlFilePath)
        {
            if (!File.Exists(xmlFilePath))
            {
                MessageBox.Show("XML file not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<EncryptedData>));
                List<EncryptedData> dataList;

                using (TextReader reader = new StreamReader(xmlFilePath))
                {
                    dataList = (List<EncryptedData>)serializer.Deserialize(reader);
                }
                var data = dataList.FirstOrDefault(d => d.Key == key);
                if (data != null)
                {
                    return Decrypt(data.EncryptedValue, key);
                }
                MessageBox.Show("Key not found in stored data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Decryption failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }
        }
        private static string Decrypt(string encryptedValue, string key)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = GenerateKey1(key);
                    aes.IV = new byte[16]; // Zero IV for simplicity

                    using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    {
                        byte[] encryptedBytes = Convert.FromBase64String(encryptedValue);
                        byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                        return Encoding.UTF8.GetString(decryptedBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Decryption failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }
        }
        private static byte[] GenerateKey1(string key)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(key)).Take(32).ToArray();
            }
        }
    }

    [Serializable]
    public class EncryptedData
    {
        public string Key { get; set; }
        //public string OriginalValue { get; set; }
        public string EncryptedValue { get; set; }
    }
}

