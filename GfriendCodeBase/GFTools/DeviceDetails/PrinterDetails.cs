using HP.GFriend.Keywords;
using HP.GFriend.Tool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Logger = HP.GFriend.GFLogger.Logger;

namespace DeviceDetails
{
    public partial class DeviceDetailsForm : Form
    {
        private string productNumber;
        private string _formatType = "";
     
        private Dictionary<Control, Rectangle> _controlOriginalRects = new Dictionary<Control, Rectangle>();
        StringBuilder allDeviceInfo = new StringBuilder();

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceDetailsForm"/> class.
        /// Sets up the form's UI properties, such as enabling maximize/minimize buttons and making the form resizable.
        /// </summary>      
        public DeviceDetailsForm()
        {
            InitializeComponent();
            this.MinimizeBox = true;
            this.FormBorderStyle = FormBorderStyle.Sizable;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceDetailsForm"/> class.
        /// Sets up the UI components and pre-fills the IP address and password fields with the provided values.
        /// Also registers the Load event handler.
        /// </summary>
        public DeviceDetailsForm(string deviceId, string adminPassword)
        {
            HP.GFriend.GFLogger.Logger.Debug("Initializing DeviceDetailsForm");
            InitializeComponent();
            HP.GFriend.GFLogger.Logger.Debug("InitializeComponent completed");
            textBoxIpAddress.Text = deviceId;
            textBoxPassword.Text = adminPassword;
            this.MinimizeBox = true;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.Load += DeviceDetailsForm_Load;
            HP.GFriend.GFLogger.Logger.Debug("DeviceDetailsForm initialization completed");
        }
        /// <summary>
        /// Handles the form's Load event. Displays a loading message, disables certain controls during initialization,
        /// and triggers the logic to view device details after a simulated delay.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private async void DeviceDetailsForm_Load(object sender, EventArgs e)
        {
            var newtextboxip = textBoxIpAddress;

            // Show loading message
            label2loadingmessage.Cursor = Cursors.WaitCursor;

            // Disable controls to prevent interaction during loading
            buttonViewDetails.Enabled = false;
            buttonCopyAll.Enabled = true;
            buttonCopyAll.Visible = true;
            button1Reload.Visible = false;

            // Simulate loading delay
            await Task.Delay(3500); // Adjust delay if needed

            // Execute logic to view details (you can call the method directly)
            buttonViewDetails_Click(null, null);

            // Hide loading message and enable controls
            label2loadingmessage.Visible = false;
            label2loadingmessage.Cursor = Cursors.Default;
            buttonViewDetails.Enabled = false;
            comboBoxCopyFormat.Items.Clear();
            comboBoxJobType.SelectedIndex = 0;        
        }

        /// <summary>
        /// Hides all UI elements related to device applications and job details to reset the view.
        /// </summary>
        #region UIVisibility
        private void DeviceUIVisible()
        {
            dataGridViewNativeApps.Visible = false;
            dataGridViewSolutions.Visible = false;
            labelSolutions.Visible = false;
            labelNativeApps.Visible = false;
            lblSolutionDetails.Visible = false;
            lblNativeAppDetails.Visible = false;
            labelJobs.Visible = false;
            labelJobDetails.Visible = false;
            dataGridViewJobDetails.Visible = false;
            comboBoxJobType.Visible = false;
            button1Reload.Visible = false;
        }
        #endregion UIVisibility

        /// <summary>
        /// Handles the TextChanged event for the IP address textbox. Resets the UI and disables view details if the field is empty.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        #region UserInput & Password
        private void textBoxIpAddress_TextChanged(object sender, EventArgs e)
        {
            if (textBoxIpAddress.Text == "")
            {
                buttonViewDetails.Enabled = true;
                richTextBoxAllInfo.Clear();
                richTextBoxAllInfo.Visible = false;
                comboBoxJobType.SelectedIndex = -1;
                DeviceUIVisible();            
            }
        }
        /// <summary>
        /// Handles the TextChanged event for the password textbox. Enables the view details button if the field is empty
        /// and reattaches the form load event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void textBoxPassword_TextChanged(object sender, EventArgs e)
        {
            if (textBoxPassword.Text == "")
            {
                buttonViewDetails.Enabled = true;
                this.Load += DeviceDetailsForm_Load;
            }
        }
        #endregion UserInput & Password

        /// <summary>
        /// Displays detailed information about a device, including printer information, firmware details,
        /// printer status, job queue data, native apps, and installed solutions, when the IP address and
        /// password are provided. Populates UI components like RichTextBox and DataGridViews with the retrieved data.
        /// </summary>
        private void DisplayDetails()
        {
            List<NativeAppData> nativeappdetails = new List<NativeAppData>();
            List<DevicePackageInfo> installedSolutions = new List<DevicePackageInfo>();
            List<JobQueueDetails> jobQueueDetails = new List<JobQueueDetails>();

            richTextBoxAllInfo.Clear();
            richTextBoxAllInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;

            DeviceUIVisible();

            string ip = textBoxIpAddress.Text?.Trim();
            string password = textBoxPassword.Text;

            //validate the Input for IPAddress and password
            string validationMessage = ValidateInputs(ip, password);
            if (!string.IsNullOrEmpty(validationMessage))
            {
                MessageBox.Show(validationMessage);
                return; // Stop further execution if validation fails
            }

            try
            {
                DeviceInformation deviceDetails = new DeviceDetails.DeviceInformation(textBoxIpAddress.Text, textBoxPassword.Text);

                if (deviceDetails == null)
                {
                    MessageBox.Show("Could not retrieve details for the device : " + textBoxIpAddress.Text + " , Please enter valid IpAddress and Password");
                    return;
                }

                //Display printer details
                richTextBoxAllInfo.Clear();
                richTextBoxAllInfo.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;

                richTextBoxAllInfo.SelectionFont = new Font("Stencil", 14, FontStyle.Bold);
                richTextBoxAllInfo.SelectionColor = Color.FromArgb(0, 171, 192);
                richTextBoxAllInfo.AppendText("Printer Information:\r\n \r\n");

                string printerInfo = $"IP Address: {textBoxIpAddress.Text}\r\n\r\n" +
                            $"Printer Name: {deviceDetails.GetPrinterName()}\r\n\r\n" +
                            $"Model Number: {deviceDetails.GetModelNumber()}\r\n\r\n" +
                            $"Mac Address: {deviceDetails.GetMacAddress()}\r\n\r\n" +
                            $"Printer Family: {deviceDetails.GetFamily(textBoxIpAddress.Text, textBoxPassword.Text)}\r\n\r\n" +
                            $"Product Number: {deviceDetails.GetDeviceProgram(productNumber)}\r\n\r\n";

                richTextBoxAllInfo.SelectionFont = new Font("Verdana", 11, FontStyle.Regular);
                richTextBoxAllInfo.SelectionColor = Color.Black;
                richTextBoxAllInfo.AppendText(printerInfo);
                // richTextBoxAllInfo.AppendText("IP Address: " + textBoxIpAddress.Text + "\r\n \r\n" + "Printer Name: " + deviceDetails.GetPrinterName() + "\r\n \r\n" + "Model Number: " + deviceDetails.GetModelNumber() + "\r\n \r\n" + "Mac Address: " + deviceDetails.GetMacAddress() + "\r\n \r\n" + "Printer Family: " + deviceDetails.GetFamily(textBoxIpAddress.Text, textBoxPassword.Text) + "\r\n \r\n" + "Product Number: " + deviceDetails.GetDeviceProgram(productNumber) + "\r\n \r\n");
                allDeviceInfo.AppendLine("=== Printer Information ===\n").AppendLine(printerInfo);

                richTextBoxAllInfo.SelectionFont = new Font("Stencil", 14, FontStyle.Bold);
                richTextBoxAllInfo.SelectionColor = Color.FromArgb(0, 171, 192);
                richTextBoxAllInfo.AppendText("\nFirmware Information:\r\n \r\n");

                string firmwareInfo = $"Firmware Version: {deviceDetails.GetModelNumber()}\r\n\r\n" +
                              $"Firmware Date: {deviceDetails.GetFirmwareDate()}\r\n\r\n";

                richTextBoxAllInfo.SelectionFont = new Font("Verdana", 11, FontStyle.Regular);
                richTextBoxAllInfo.SelectionColor = Color.Black;
                richTextBoxAllInfo.AppendText(firmwareInfo);
                allDeviceInfo.AppendLine("=== Firmware Information ===\n").AppendLine(firmwareInfo);

                // Add printer status details
                richTextBoxAllInfo.SelectionFont = new Font("Stencil", 14, FontStyle.Bold);
                richTextBoxAllInfo.SelectionColor = Color.FromArgb(0, 171, 192);
                richTextBoxAllInfo.AppendText("\nPrinter Status:\r\n \r\n");
                string statusInfo = $"Printer Status: {deviceDetails.GetPrinterStatus()}\r\n\r\n" +
                           $"Cartridge Status: {deviceDetails.GetCartridgeStatus()}\r\n\r\n" +
                           $"Tray Status: {deviceDetails.GetTrayStatus()}\r\n\r\n" +
                           $"Power Status: {deviceDetails.GetPowerStatus()}\r\n\r\n";

                richTextBoxAllInfo.SelectionFont = new Font("Verdana", 11, FontStyle.Regular);
                richTextBoxAllInfo.SelectionColor = Color.Black;
                richTextBoxAllInfo.AppendText(statusInfo);
                allDeviceInfo.AppendLine("=== Printer Status ===\n").AppendLine(statusInfo);

                // Calculate the height of the text inside RichTextBox
                int textHeight = richTextBoxAllInfo.GetPositionFromCharIndex(richTextBoxAllInfo.Text.Length).Y; // Adjust padding

                RichTextBox richTextBoxDisplay = new RichTextBox();
                richTextBoxDisplay.Dock = DockStyle.Fill;
                this.Controls.Add(richTextBoxDisplay);

                dataGridViewJobDetails.Rows.Clear();
                dataGridViewNativeApps.Rows.Clear();
                dataGridViewSolutions.Rows.Clear();

                //Display job Details         
                labelJobDetails.Visible = true;
                comboBoxJobType.Visible = true;
                button1Reload.Visible = true;

                AppLogger.Debug("Before Job log details: " + "jobQueueDetails");
                jobQueueDetails = deviceDetails.GetProcessingJobQueue();
                AppLogger.Debug(" After Job log details: " + "jobQueueDetails");

                if (jobQueueDetails.Count > 0)
                {
                    comboBoxJobType.Visible = true;
                    dataGridViewJobDetails.Rows.Clear();

                    // Set RowStyles for proper row spacing
                    for (int i = 0; i < jobQueueDetails.Count; i++)
                    {
                        dataGridViewJobDetails.Rows.Add(jobQueueDetails[i].JobId, jobQueueDetails[i].JobName, jobQueueDetails[i].JobType, jobQueueDetails[i].StartTime, jobQueueDetails[i].State, jobQueueDetails[i].UserName, jobQueueDetails[i].EndTime);
                    }
                    dataGridViewJobDetails.Visible = true;
                }
                else
                {
                    labelJobs.Visible = true;
                }

                //Display native apps
                lblNativeAppDetails.Visible = true;
                nativeappdetails = deviceDetails.GetNativeAplications(textBoxIpAddress.Text, textBoxPassword.Text);

                if (nativeappdetails.Count > 0)
                {
                    dataGridViewNativeApps.Rows.Clear();

                    // Set RowStyles for proper row spacing
                    for (int i = 0; i < nativeappdetails.Count; i++)
                    {
                        dataGridViewNativeApps.Rows.Add(nativeappdetails[i].Title, nativeappdetails[i].Description);
                    }
                }
                else
                {
                    labelNativeApps.Visible = true;
                    lblSolutionDetails.Location = new Point(671, 170);
                    dataGridViewSolutions.Location = new Point(671, 200);
                    labelSolutions.Location = new Point(671, 200);
                }

                //Display Solutions
                try
                {
                    lblSolutionDetails.Visible = true;
                    installedSolutions = deviceDetails.GetInstalledSolutions(textBoxIpAddress.Text, textBoxPassword.Text);

                    if (installedSolutions.Count == 0)
                    {
                        List<NativeAppData> solutionDerivedFromNativeAppDetails = deviceDetails.GetSolutionsFromNativeDetails(ref nativeappdetails);
                        foreach (NativeAppData data in solutionDerivedFromNativeAppDetails)
                        {
                            DevicePackageInfo solution = new DevicePackageInfo(data.Title, "NA", data.Id, "NA", data.Description);
                            installedSolutions.Add(solution);
                        }
                    }
                    List<InstalledSolution> Solutions = installedSolutions.Select(p => new InstalledSolution
                    {
                        PackageName = p.Name,
                        Version = p.Version,
                        UUID = p.Uuid,
                        InstalledFile = p.installedFileName,
                        Description = p.Description
                    }).ToList();

                    if (installedSolutions.Count > 0)
                    {
                        List<string> columns = new List<string>() { "Solution Name", "Version", "UUID", "Installed File", "Description" };

                        // Set RowStyles for proper row spacing
                        for (int i = 0; i < installedSolutions.Count; i++)
                        {
                            dataGridViewSolutions.Rows.Add(installedSolutions[i].Name, installedSolutions[i].Version, installedSolutions[i].Description);
                        }
                    }
                    else
                    {
                        labelSolutions.Visible = true;
                    }
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No Installed solution: " + ex.Message, "Error");
                    string logPath = ExceptionLogger.LogException(ex);
                }
            }

            catch (Exception ex)
            {
                Logger.Debug(("Error retrieving device details, No Installed solution: " + ex.Message));
                string logPath = ExceptionLogger.LogException(ex);
            }

            finally
            {
                label2loadingmessage.Visible = false;
                richTextBoxAllInfo.Visible = true;
                if (jobQueueDetails.Count > 0)
                {
                    dataGridViewJobDetails.Visible = true;
                }
                else
                {
                    labelJobs.Visible = true;
                }
                if (nativeappdetails.Count > 0)
                {
                    dataGridViewNativeApps.Visible = true;
                }
                else
                {
                    labelNativeApps.Visible = true;
                }
                if (installedSolutions.Count > 0)
                {
                    dataGridViewSolutions.Visible = true;
                }
                else
                {
                    labelSolutions.Visible = true;
                }
                buttonViewDetails.Enabled = false;
            }
            return;
        }

        /// <summary>
        /// Handles the click event for the Copy All button. Gathers data from UI grids,
        /// formats it based on the selected format (Notepad or Excel), and saves it to a file on the desktop.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        #region Controls
        private void buttonCopyAll_Click(object sender, EventArgs e)
        {
            try
            {
                comboBoxCopyFormat.SelectedIndexChanged += comboBoxCopyFormat_SelectedIndexChanged_1;

                if (comboBoxCopyFormat.SelectedItem == null)
                {
                    MessageBox.Show("Please select a format (Notepad or Excel) before copying.");
                    return;
                }
                _formatType = comboBoxCopyFormat.SelectedItem.ToString(); // "Notepad" or "Excel"
                buttonCopyAll.Enabled = false;

                allDeviceInfo.AppendLine("=== Job Queue Details ===");
                allDeviceInfo.AppendLine(GetDataFromGrid(dataGridViewJobDetails, _formatType));
                allDeviceInfo.AppendLine();

                allDeviceInfo.AppendLine("=== Native App Details ===");
                allDeviceInfo.AppendLine(GetDataFromGrid(dataGridViewNativeApps, _formatType));
                allDeviceInfo.AppendLine();

                allDeviceInfo.AppendLine("=== Solutions Details ===");
                allDeviceInfo.AppendLine(GetDataFromGrid(dataGridViewSolutions, _formatType));
                allDeviceInfo.AppendLine();

                string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "DeviceInfoExports");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileExtension = _formatType.Equals("Excel", StringComparison.OrdinalIgnoreCase) ? ".csv" : ".txt";
                string fileName = "DeviceInfo_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + fileExtension;
                string fullPath = Path.Combine(folderPath, fileName);

                File.WriteAllText(fullPath, allDeviceInfo.ToString(), Encoding.UTF8);
                 ShowFileLink(fullPath);        
            }         
            catch (Exception ex)
            {
                MessageBox.Show("Failed to copy device info: " + ex.Message);
            }
        }

        /// <summary>
        /// Triggers the logic to display device details when the View Details button is clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>.
        private void buttonViewDetails_Click(object sender, EventArgs e)
        {
            DisplayDetails();
        }
        /// <summary>
        /// Handles the Clear All button click event. 
        /// Resets the UI by enabling the View Details button, clearing and hiding the job details text box, 
        /// resetting the job type selection, and restoring the default device UI state.
        /// </summary>
        private void button1_ClearAll_Click_1(object sender, EventArgs e)
        {
            buttonViewDetails.Enabled = true;
            richTextBoxAllInfo.Clear();
            richTextBoxAllInfo.Visible = false;
            comboBoxJobType.SelectedIndex = -1;
            label1loadingJob_details_message.Visible = false;
            button1Reload.Visible = false;
            DeviceUIVisible();
        }

        /// <summary>
        /// Handles the Reload button click event. 
        /// Refreshes the job details by invoking the job type selection change logic 
        /// through the <see cref="comboBoxJobType_SelectedIndexChanged"/> method.
        /// </summary>
        private void button1Reload_Click(object sender, EventArgs e)
        {
            comboBoxJobType_SelectedIndexChanged(sender, e);
        }
        #endregion Controls

        /// <summary>
        /// Enables the Copy All button when a format is selected from the combo box.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        #region ComboBoxItems
        private void comboBoxCopyFormat_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            buttonCopyAll.Enabled = true;
        }
       
        /// <summary>
        /// Handles the event when a job type is selected from the combo box. Retrieves the processing job queue
        /// for the specified device and populates the DataGridView with job details filtered by the selected job type.
        /// </summary> 
        private async void comboBoxJobType_SelectedIndexChanged(object sender, EventArgs e)
        {
            label1loadingJob_details_message.Visible = true;
            string jobType = comboBoxJobType.SelectedItem?.ToString()?.Trim();

            if (string.IsNullOrEmpty(jobType)) return;
            dataGridViewJobDetails.Rows.Clear();

            DeviceInformation deviceDetails = new DeviceInformation(textBoxIpAddress.Text, textBoxPassword.Text);
            List<JobQueueDetails> jobQueueDetails = await Task.Run(() => deviceDetails.GetProcessingJobQueue());

            string deviceType = deviceDetails.DeviceType;
            bool jobFound = false;
            for (int i = 0; i < jobQueueDetails.Count; i++)
            {
                if (deviceType == "Dune")
                {
                    if (string.Equals(jobQueueDetails[i].JobType.Trim(), jobType.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        dataGridViewJobDetails.Rows.Add(jobQueueDetails[i].JobId, jobQueueDetails[i].JobName, jobQueueDetails[i].JobType, jobQueueDetails[i].StartTime, jobQueueDetails[i].State, jobQueueDetails[i].UserName, jobQueueDetails[i].PauseReason);
                        jobFound = true;
                    }
                }

                else if (deviceType == "Jedi")
                {
                    if (string.Equals(jobQueueDetails[i].JobName?.Trim(), jobType, StringComparison.OrdinalIgnoreCase))
                    {
                        dataGridViewJobDetails.Rows.Add(jobQueueDetails[i].JobId, jobQueueDetails[i].JobName, jobQueueDetails[i].JobType, jobQueueDetails[i].StartTime, jobQueueDetails[i].State, jobQueueDetails[i].UserName, jobQueueDetails[i].PauseReason);
                        jobFound = true;
                    }
                }
                if (jobType == "All Jobs")
                {
                    dataGridViewJobDetails.Rows.Add(jobQueueDetails[i].JobId, jobQueueDetails[i].JobName, jobQueueDetails[i].JobType, jobQueueDetails[i].StartTime, jobQueueDetails[i].State, jobQueueDetails[i].UserName, jobQueueDetails[i].PauseReason);
                    jobFound = true; // Display all jobs
                }
                label1loadingJob_details_message.Visible = false;
                dataGridViewJobDetails.Visible = jobFound;
                labelJobs.Visible = !jobFound;
            }
        }
        #endregion ComboBoxItems

        /// <summary>
        /// Validates the IP address and password inputs for basic format and security rules.
        /// </summary>
        /// <param name="ip">The IP address to validate.</param>
        /// <param name="password">The password to validate.</param>
        /// <returns>
        /// A validation error message as a <see cref="string"/> if validation fails; otherwise, <c>null</c> if all inputs are valid.
        /// </returns>
        #region MessageBox  
        private string ValidateInputs(string ip, string password)
        {
            //Validate both the IPAddress and Password inputs are entered 
            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(password))
                return "Please enter both IP address and password.";

            //Validate the input IPAddress and format (strict IPv4)
            else if (!IPAddress.TryParse(ip, out _) || (string.IsNullOrEmpty(ip) || (!Regex.IsMatch(ip, @"^(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)){3}$"))))
                return "Please enter a valid IPv4 address (e.g., 146.205.4.223).";

            //validate Input Password
            if (string.IsNullOrEmpty(password) || password.Any(char.IsWhiteSpace))
                return string.IsNullOrEmpty(password) ? "Please enter a valid password." :
                "Password should not contain spaces.";

            return null;
        }

        /// <summary>
        /// Extracts data from a DataGridView and returns it as a formatted string based on the specified format type.
        /// Supports tab-separated format for Excel and table-style format for Notepad.
        /// </summary>
        /// <param name="dgv">The DataGridView control containing the data.</param>
        /// <param name="formatType">The desired output format ("Excel" or "Notepad").</param>
        /// <returns>A formatted string representing the data in the specified format.</returns>
        private string GetDataFromGrid(DataGridView dgv, string formatType)
        {
            StringBuilder sb = new StringBuilder();

            // Excel
            if (formatType.Equals("Excel", StringComparison.OrdinalIgnoreCase))
            {
                // Tab-separated format for Excel
                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    if (col.Visible)
                        sb.Append(col.HeaderText + "\t");
                }
                sb.AppendLine();

                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        foreach (DataGridViewColumn col in dgv.Columns)
                        {
                            if (col.Visible)
                                sb.Append(row.Cells[col.Index].Value?.ToString() + "\t");
                        }
                        sb.AppendLine();
                    }
                }
            }
            else if (formatType.Equals("Notepad", StringComparison.OrdinalIgnoreCase))// "Notepad" or default
            {
                // Table-style format
                Dictionary<int, int> columnWidths = new Dictionary<int, int>();

                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    if (!col.Visible) continue;

                    int maxWidth = col.HeaderText.Length;
                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        if (!row.IsNewRow)
                        {
                            var val = row.Cells[col.Index].Value?.ToString() ?? "";
                            if (val.Length > maxWidth)
                                maxWidth = val.Length;
                        }
                    }
                    columnWidths[col.Index] = maxWidth + 2;
                }

                // Border line
                string border = "+";
                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    if (col.Visible)
                        border += new string('-', columnWidths[col.Index]) + "+";
                }
                sb.AppendLine(border);

                // Header row
                sb.Append("|");
                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    if (col.Visible)
                        sb.Append(" " + col.HeaderText.PadRight(columnWidths[col.Index] - 1) + "|");
                }
                sb.AppendLine();
                sb.AppendLine(border);

                // Rows
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.IsNewRow) continue;

                    sb.Append("|");
                    foreach (DataGridViewColumn col in dgv.Columns)
                    {
                        if (col.Visible)
                        {
                            string val = row.Cells[col.Index].Value?.ToString() ?? "";
                            sb.Append(" " + val.PadRight(columnWidths[col.Index] - 1) + "|");
                        }
                    }
                    sb.AppendLine();
                }

                sb.AppendLine(border);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Displays a custom dialog form showing the file save confirmation message, 
        /// along with the saved file path as a clickable hyperlink. 
        /// Provides OK and Cancel buttons for user interaction.
        /// </summary>
        /// <param name="fullPath">The full file path of the saved file. 
        /// Clicking the hyperlink opens the file location in Windows Explorer.</param>
        private void ShowFileLink(string fullPath)
        {
            Form linkForm = new Form
            {
                Text = "File Saved",
                Width = 470,
                Height = 190
            };

            Label lbl = new Label
            {
                Text = "Device information saved successfully at:",
                // Dock = DockStyle.Top,
                Location = new Point(9, 40),
                Height = 30,
                AutoSize = true,           
            };
            linkForm.Controls.Add(lbl);

            LinkLabel link = new LinkLabel
            {
                Text = fullPath,
                Location = new Point(9, 40),
                Height = 30,
                AutoSize = true
            };
            link.LinkClicked += (s, e) =>
            {
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{fullPath}\"");
            };
            linkForm.Controls.Add(link);

            Button ok = new Button
            {
                Text = "OK",
                Width = 80,
                Height = 30,
                Location = new Point(88, 100),
                DialogResult = DialogResult.OK
            };
            linkForm.Controls.Add(ok);

            //cancel button
            Button cancel = new Button
            {
                Text = "Cancel",
                //Dock = DockStyle.Left,
                Width = 80,
                Height = 30,
                Location = new Point(220, 100),
                DialogResult = DialogResult.Cancel
            };
            linkForm.Controls.Add(cancel);
            linkForm.AcceptButton = ok;
            linkForm.AcceptButton = cancel;
            linkForm.ShowDialog();
        }

        #endregion MessageBox 

        /// <summary>
        /// Resizes all child controls of a given parent control based on the specified width and height scaling ratios.
        /// Recursively adjusts the position and size of controls to maintain layout consistency when the form is resized.
        /// </summary>
        /// <param name="parent">The parent control containing the child controls to resize.</param>
        /// <param name="xRatio">The horizontal scaling ratio.</param>
        /// <param name="yRatio">The vertical scaling ratio.</param>
        #region FormResize
        private void ResizeControls(Control parent, float xRatio, float yRatio)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (_controlOriginalRects.TryGetValue(ctrl, out var original))
                {
                    ctrl.Left = (int)(original.Left * xRatio);
                    ctrl.Top = (int)(original.Top * yRatio);
                    ctrl.Width = (int)(original.Width * xRatio);
                    ctrl.Height = (int)(original.Height * yRatio);
                }
                if (ctrl.HasChildren)
                {
                    ResizeControls(ctrl, xRatio, yRatio);
                }
            }
        }
        #endregion FormResize

    }
}
