using HP.DeviceAutomation;
using HP.DeviceAutomation.Dune;
using HP.DeviceAutomation.Jedi;
using HP.GFriend.Keywords;
using HP.GFriend.Utils.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OXPd.Service.UIConfiguration;
using HP.GFriend.Tool;

namespace DeviceDetails
{
    #region PrinterDetails
    internal class DeviceInformation
    {
        private IDevice _device;
        private string _deviceType = "";
        private string _ipaddress = "";
        public string DeviceType => _deviceType;

        #region DUTAddress
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceInformation"/> class using the provided IP address and password.
        /// Determines the device type (printer family) during initialization.
        /// </summary>
        /// <param name="ipaddress">The IP address of the device.</param>
        /// <param name="password">The password used to authenticate with the device.</param>
        public DeviceInformation(string ipaddress,string password) 
        {
            _ipaddress = ipaddress;
            _deviceType = GetFamily(ipaddress, password);
        }
        #endregion DUTAddress

        /// <summary>
        /// Determines the printer family based on the provided IP address and password.
        /// </summary>
        /// <param name="ipaddress">The IP address of the device.</param>
        /// <param name="password">The password for device authentication.</param>
        /// <returns>The printer family ("Jedi" or "Dune"), or null if the family cannot be determined.</returns>

        // Method to retieve Printer Name using SNMP
        public string GetFamily(string ipaddress, string password)
        {
            try
            {
                _device = DeviceFactory.Create(_ipaddress, password);

                if (_device is JediOmniDevice)
                {
                    return "Jedi";
                }
                else if (_device is DuneDevice)
                {
                    return "Dune";
                }
            }
            catch (Exception ex)
            {
                HP.GFriend.GFLogger.Logger.Trace("Exception in the method GetFamily : " + ex.Message);
                return null;
            }

            return null;
        }

        /// <summary>
        /// Retrieves the printer name using SNMP.
        /// </summary>
        /// <returns>The printer name, or null if the name cannot be retrieved.</returns>
        public string GetPrinterName()
        {
            string oid = "1.3.6.1.2.1.25.3.2.1.3.1";
            string printerName = "";
            try
            {           
                if (_deviceType == "Jedi")
                {
                    JediOmniDevice jediDevice = (JediOmniDevice)_device;
                    SnmpOidValue modelNameSnmpOidValue = jediDevice.Snmp.GetRaw(oid);
                    printerName = modelNameSnmpOidValue.Value.ToString();
                }
                else if (_deviceType == "Dune")
                {
                    DuneDevice duneDevice = (DuneDevice)_device;
                    SnmpOidValue modelNameSnmpOidValue = duneDevice.Snmp.GetRaw(oid);
                    printerName = modelNameSnmpOidValue.Value.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving Printer Name: {ex.Message}");
            }

            return printerName != "Not found" ? printerName : null;      
        }

        /// <summary>
        /// Retrieves the model number of the device using SNMP.
        /// </summary>
        /// <returns>The model number of the device, or null if it cannot be retrieved.</returns>
        public string GetModelNumber()
        {
            // Hardcoded OID value for firmware date
            string modelNumOid = "1.3.6.1.4.1.11.2.3.9.4.2.1.1.3.1.0";
            string modelNumber = "";
            if (_deviceType == "Jedi")
            {
                JediOmniDevice jediDevice = (JediOmniDevice)_device;
                SnmpOidValue modelNameSnmpOidValue = jediDevice.Snmp.GetRaw(modelNumOid);
                modelNumber = modelNameSnmpOidValue.Value.ToString();
            }
            else if (_deviceType == "Dune")
            {
                DuneDevice duneDevice = (DuneDevice)_device;
                SnmpOidValue modelNameSnmpOidValue = duneDevice.Snmp.GetRaw(modelNumOid);
                modelNumber = modelNameSnmpOidValue.Value.ToString();
            }
            return modelNumber != "Not found" ? modelNumber : null;
        }

        /// <summary>
        /// Retrieves the power status of the device using SNMP.
        /// </summary>
        /// <returns>
        /// A string describing the power status, such as "Awake", "PowerSave", "Sleep", etc.
        /// Returns "None" if no status is available.
        /// </returns>
        public string GetPowerStatus()
        {
            // Hardcoded OID value for firmware date
            string powerStatusOid = "1.3.6.1.4.1.11.2.3.9.4.2.1.1.1.65.0";
            string powerStatus = "";
            if (_deviceType == "Jedi")
            {
                JediOmniDevice jediDevice = (JediOmniDevice)_device;
                SnmpOidValue modelNameSnmpOidValue = jediDevice.Snmp.GetRaw(powerStatusOid);
                powerStatus = modelNameSnmpOidValue.Value.ToString();
            }
            else if (_deviceType == "Dune")
            {
                DuneDevice duneDevice = (DuneDevice)_device;
                SnmpOidValue modelNameSnmpOidValue = duneDevice.Snmp.GetRaw(powerStatusOid);
                powerStatus = modelNameSnmpOidValue.Value.ToString();
            }
            switch (powerStatus)
            {
                case "0":
                    return "None: No status returned.";
                case "1":
                    return "Awake:  The device is awake (ready).";
                case "2":
                    return "PowerSave: The device is in a power-saving state (shallow suspend).";
                case "3":
                    return "Sleep: The device is asleep (suspend or A1W).";
                case "4":
                    return "Off: The device is off.";
                case "5":
                    return "TransitioningToAwake: The device is transitioning to Awake.";
                case "6":
                    return "TransitioningToPowerSave: The device is transitioning to PowerSave.";
                case "7":
                    return "TransitioningToSleep: The device is transitioning to Sleep.";
                case "8":
                    return "TransitioningToOff: The device is transitioning to Off.";
                case "9":
                    return "Reboot: The device is rebooting.";
                case "10":
                    return "AutoOff: The device is in an automatic off state.";
                case "11":
                    return "OneWatt: The device is in 1W state (deep suspend).";
                case "12":
                    return "DeepSleep: The device is in deep sleep.";
                default:
                    return "None: No status returned.";
            }
        }

        /// <summary>
        /// Retrieves the overall device status using SNMP.
        /// </summary>
        /// <returns>
        /// A string describing the device status, such as "Running", "Warning", "Testing", "Down", etc.
        /// Returns "None" if no status is available.
        /// </returns>
        public string GetDeviceStatus()
        {
            // Hardcoded OID value for firmware date
            string deviceStatusOid = "1.3.6.1.2.1.25.3.2.1.5.1";
            string deviceStatus = "";
            if (_deviceType == "Jedi")
            {
                JediOmniDevice jediDevice = (JediOmniDevice)_device;
                SnmpOidValue modelNameSnmpOidValue = jediDevice.Snmp.GetRaw(deviceStatusOid);
                deviceStatus = modelNameSnmpOidValue.Value.ToString();
            }
            else if (_deviceType == "Dune")
            {
                DuneDevice duneDevice = (DuneDevice)_device;
                SnmpOidValue modelNameSnmpOidValue = duneDevice.Snmp.GetRaw(deviceStatusOid);
                deviceStatus = modelNameSnmpOidValue.Value.ToString();
            }
            switch (deviceStatus)
            {
                case "0":
                    return "None: No Status is returned.";
                case "1":
                    return "Unknown: The current state of the device is unknown.";
                case "2":
                    return "Running:  The device is ready for use or is currently in use or maybe in power save mode.";
                case "3":
                    return "Warning: A condition exists that needs attention but is not preventing use. A non-critical alert is active.";
                case "4":
                    return "Testing: The device is not available for use because it is in the testing state.";
                case "5":
                    return "Down: The device is not available for use because it is offline or a critical alert is active. Human interaction is needed to bring the device to a ready state.";
                default:
                    return "None: No Status is returned.";
            }
        }

        /// <summary>
        /// Retrieves the printer-specific status using SNMP.
        /// </summary>
        /// <returns>
        /// A string describing the printer status, such as "Idle", "Printing", "Warmup", etc.
        /// Returns "None" if no status is available.
        /// </returns>
        public string GetPrinterStatus()
        {
            // Hardcoded OID value for firmware date
            string printerStatusOid = "1.3.6.1.2.1.25.3.5.1.1.1";
            string printerStatus = "";
            if (_deviceType == "Jedi")
            {
                JediOmniDevice jediDevice = (JediOmniDevice)_device;
                SnmpOidValue modelNameSnmpOidValue = jediDevice.Snmp.GetRaw(printerStatusOid);
                printerStatus = modelNameSnmpOidValue.Value.ToString();
            }
            else if (_deviceType == "Dune")
            {
                DuneDevice duneDevice = (DuneDevice)_device;
                SnmpOidValue modelNameSnmpOidValue = duneDevice.Snmp.GetRaw(printerStatusOid);
                printerStatus = modelNameSnmpOidValue.Value.ToString();
            }
            switch (printerStatus)
            {
                case "0":
                    return "None: No status returned.";
                case "1":
                    return "Other: The printer is offline or a critical alert is active.";
                case "2":
                    return "Unknown: The printer state is unknown.";
                case "3":
                    return "Idle: The printer is not performing any significant actions.";
                case "4":
                    return "Printing: A job is currently being processed or printed, or a PJL job is being processed.";
                case "5":
                    return "Warmup:  If the Device Status is Down, then the printer is currently offline but is resolving the condition that caused it to be offline. If the Device Status is Running, then the printer was in power save mode and is now ready to print.";
                default:
                    return "None: No status returned.";
            }
        }

        /// <summary>
        /// Retrieves the MAC address of the device using SNMP.
        /// </summary>
        /// <returns>The MAC address as a string, or null if not found.</returns>
        public string GetMacAddress()
        {
            // Hardcoded OID value for firmware date
            string macAddressOid = "1.3.6.1.2.1.2.2.1.6";
            string macAddress = "";
            if (_deviceType == "Jedi")
            {
                JediOmniDevice jediDevice = (JediOmniDevice)_device;
                SnmpOidValue modelNameSnmpOidValue = jediDevice.Snmp.GetRaw(macAddressOid);
                macAddress = modelNameSnmpOidValue.Value.ToString();
            }
            else if (_deviceType == "Dune")
            {
                DuneDevice duneDevice = (DuneDevice)_device;
                SnmpOidValue modelNameSnmpOidValue = duneDevice.Snmp.GetRaw(macAddressOid);
                macAddress = modelNameSnmpOidValue.Value.ToString();
            }
            return macAddress != "Not found" ? macAddress : null;
        }

        /// <summary>
        /// Retrieves the program name associated with a given product number.
        /// </summary>
        /// <param name="productNumber">The product number of the device.</param>
        /// <returns>The corresponding program name, or an empty string if not found.</returns>
        public string GetDeviceProgram(string productNumber)
        {
            string program = "";

            switch (productNumber)
            {
                case "1PS54A":
                case "1PS55A":
                case "1PV64A":
                case "1PV65A":
                case "1PV66A":
                case "1PV67A":
                    program = "Fairmont";
                    break;
                case "1PV89A":
                    program = "Kobe";
                    break;
                case "3GY14A":
                case "3GY15A":
                case "3GY16A":
                case "3GY17A":
                case "7PS95A":
                case "7PS96A":
                case "7PS97A":
                case "7PS98A":
                case "7PS99A":
                case "7PT00A":
                case "7PT01A":
                    program = "Ion";
                    break;
                case "7PS84A":
                    program = "Titanium";
                    break;
                case "7PS86A":
                    program = "Oxygen";
                    break;
                case "7ZU81A":
                    program = "Madrid";
                    break;
                case "A2W75A":
                case "D7P71A":
                    program = "Azalea";
                    break;
                case "CF066A":
                case "CF067A":
                case "CF068A":
                case "CF069A":
                    program = "Everest";
                    break;
                case "CZ244A":
                case "CZ245A":
                    program = "Tahiti";
                    break;
                case "D7P68A":
                    program = "Fiji";
                    break;
                case "CF236A":
                case "CF238A":
                    program = "Annapurna";
                    break;
                case "T3U43A":
                case "T3U44A":
                case "T3U64A":
                    program = "Banff";
                    break;

                case "T3U51A":
                case "T3U52A":
                case "T3U66A":
                    program = "Nagano";
                    break;

                case "3WT91A":
                case "T3U55A":
                case "T3U56A":
                    program = "Keystone";
                    break;

                case "L2762A":
                    program = "Arches";
                    break;

                case "L2763A":
                    program = "Yellowstone";
                    break;

                case "3PZ15A":
                    program = "Wrigley";
                    break;

                case "3PZ55A":
                    program = "Busch";
                    break;

                case "3PZ95A":
                    program = "Fenway";
                    break;

                case "3QA55A":
                    program = "Camden";
                    break;

                case "5QJ83A":
                    program = "Ammolite";
                    break;

                case "5QJ87A":
                    program = "Moganite";
                    break;

                case "5QJ90A":
                case "5QJ94A":
                    program = "Citrine";
                    break;

                case "5QJ98A":
                case "5QK02A":
                    program = "Pearl";
                    break;

                case "5QK03A":
                case "5QK08A":
                    program = "Jasper";
                    break;

                case "5QK09A":
                case "5QK13A":
                    program = "Moonstone";
                    break;

                case "9S183A":
                case "9S184A":
                    program = "Jasper T";
                    break;

                case "9S185A":
                case "9S186A":
                case "9S187A":
                case "AJ7J3A":
                    program = "Moonstone T";
                    break;

                case "AG8Z1A":
                    program = "MOONSTONE";
                    break;
            }
            return program;
        }

        #region FirmwareDetails
        /// <summary>
        /// Retrieves the firmware version of the device using SNMP.
        /// </summary>
        /// <returns>The firmware version, or null if the version cannot be retrieved.</returns>
        // Method to retrieve firmware version using SNMP
        public string GetFirmwareVersion()
        {
            // Hardcoded OID value for firmware name
            string versionOid = "1.3.6.1.4.1.11.2.3.9.4.2.1.1.3.6.0";
            string firmwareVersion = "";
            try
            {
                if (_deviceType == "Jedi")
                {
                    JediOmniDevice jediDevice = (JediOmniDevice)_device;
                    SnmpOidValue modelNameSnmpOidValue = jediDevice.Snmp.GetRaw(versionOid);
                    firmwareVersion = modelNameSnmpOidValue.Value.ToString();
                }
                else if (_deviceType == "Dune")
                {
                    DuneDevice duneDevice = (DuneDevice)_device;
                    SnmpOidValue modelNameSnmpOidValue = duneDevice.Snmp.GetRaw(versionOid);
                    firmwareVersion = modelNameSnmpOidValue.Value.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving Printer Name: {ex.Message}");
            }

            return firmwareVersion != "Not found" ? firmwareVersion : null;
        }

        /// <summary>
        /// Retrieves the firmware release date of the device using SNMP.
        /// </summary>
        /// <returns>The firmware release date, or null if the date cannot be retrieved.</returns>

        // Method to retrieve firmware date using SNMP
        public string GetFirmwareDate()
        {
            // Hardcoded OID value for firmware date
            string dateOid = "1.3.6.1.4.1.11.2.3.9.4.2.1.1.3.5.0";
            string firmwareDate = "";
            if (_deviceType == "Jedi")
            {
                JediOmniDevice jediDevice = (JediOmniDevice)_device;
                SnmpOidValue modelNameSnmpOidValue = jediDevice.Snmp.GetRaw(dateOid);
                firmwareDate = modelNameSnmpOidValue.Value.ToString();
            }
            else if (_deviceType == "Dune")
            {
                DuneDevice duneDevice = (DuneDevice)_device;
                SnmpOidValue modelNameSnmpOidValue = duneDevice.Snmp.GetRaw(dateOid);
                firmwareDate = modelNameSnmpOidValue.Value.ToString();
            }
            return firmwareDate != "Not found" ? firmwareDate : null;
        }

        /// <summary>
        /// Gets the device firmware date code for Dune.
        /// </summary>
        /// <param name="address">Is the Address of the Device.</param>
        /// <returns>The device firmware date code, or null if the information could not be retrieved.</returns>
        internal static string GetFirmwareDateCode(string address)
        {
            string Url = $"https://{address}/cdm/system/v1/identity";
            using (HttpClient clients = new HttpClient())
            {
                HttpResponseMessage response = clients.GetAsync(Url).Result;

                if (response.IsSuccessStatusCode)
                {
                    string responsedata = response.Content.ReadAsStringAsync().Result;
                    dynamic deviceData = JsonConvert.DeserializeObject(responsedata);

                    string firmwareDateCode = deviceData.firmwareDateCode;
                    return firmwareDateCode;
                }
                else
                {
                    HP.GFriend.GFLogger.Logger.Debug($"Unable to get the Firmware Version for {address} ");
                    return "Error";
                }
            }
        }

        /// <summary>
        /// Retrieves the firmware version and power status of the device using a telnet connection.
        /// </summary>
        /// <returns>A tuple containing the firmware version and power status, or null values if retrieval fails.</returns>
        public (string firmware, string powerStatus) GetFirmwareAndPowerStatus()
        {
            string server = _ipaddress;
            int port = 9104; // Default telnet port

            //UDW command for getting firmware version and firmware date
            string firmwareCommand = "DeviceIdentification PUB_getDeviceId";
            //UDW command for getting power status
            string powerStatusCommand = "SystemScheduler PUB_getSchedulerStats";
            string firmware = null;
            string powerStatus = null;

            try
            {
                using (TcpClient client = new TcpClient(server, port))
                {
                    using (NetworkStream stream = client.GetStream())
                    {
                        byte[] data = Encoding.ASCII.GetBytes(firmwareCommand + "\n");
                        stream.Write(data, 0, data.Length);

                        data = new byte[256];
                        StringBuilder response = new StringBuilder();
                        int bytes = 0;

                        Thread.Sleep(100); // Sleep for 100 milliseconds
                        do
                        {
                            bytes = stream.Read(data, 0, data.Length);
                            response.Append(Encoding.ASCII.GetString(data, 0, bytes));
                        }
                        while (stream.DataAvailable);

                        string[] parts = response.ToString().Split(';');
                        foreach (var part in parts)
                        {
                            if (part.Trim().StartsWith("FW:"))
                            {
                                firmware = part.Trim().Substring(3);
                                break;
                            }
                        }

                        // Now send the command for power status
                        data = Encoding.ASCII.GetBytes(powerStatusCommand + "\n");
                        stream.Write(data, 0, data.Length);

                        response.Clear();
                        do
                        {
                            bytes = stream.Read(data, 0, data.Length);
                            response.Append(Encoding.ASCII.GetString(data, 0, bytes));
                        }
                        while (stream.DataAvailable);

                        parts = response.ToString().Split(',');
                        foreach (var part in parts)
                        {
                            if (part.Contains("\"PowerLevel\":"))
                            {
                                powerStatus = part.Split(':')[1].Trim(new char[] { '"', ' ', '\n', '\r' });
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Handle exception as needed
            }

            return (firmware, powerStatus);
        }
        #endregion FirmwareDetails

        #region CartridgeStatus
        /// <summary>
        /// Retrieves the status of the printer's cartridges using SNMP.
        /// </summary>
        /// <returns>A string indicating which cartridges are empty, or "All cartridges are empty" if none are installed.</returns>
        public string GetCartridgeStatus()
        {
            string blackCartridgeOid = "1.3.6.1.2.1.43.11.1.1.6.1.1";
            string cyanCartridgeOid = "1.3.6.1.2.1.43.11.1.1.6.1.2";
            string magentaCartridgeOid = "1.3.6.1.2.1.43.11.1.1.6.1.3";
            string yellowCartridgeOid = "1.3.6.1.2.1.43.11.1.1.6.1.4";

            string blackCartridgeStatus = "";
            string cyanCartridgeStatus = "";
            string magentaCartridgeStatus = "";
            string yellowCartridgeStatus = "";

            if (_deviceType == "Jedi")
            {
                JediOmniDevice jediDevice = (JediOmniDevice)_device;
                SnmpOidValue blackCartridgeOidValue = jediDevice.Snmp.GetRaw(blackCartridgeOid);
                blackCartridgeStatus = blackCartridgeOidValue.Value.ToString();
                SnmpOidValue cyanCartridgeOidValue = jediDevice.Snmp.GetRaw(cyanCartridgeOid);
                cyanCartridgeStatus = blackCartridgeOidValue.Value.ToString();
                SnmpOidValue magentaCartridgeOidValue = jediDevice.Snmp.GetRaw(magentaCartridgeOid);
                magentaCartridgeStatus = blackCartridgeOidValue.Value.ToString();
                SnmpOidValue yellowCartridgeOidValue = jediDevice.Snmp.GetRaw(yellowCartridgeOid);
                yellowCartridgeStatus = blackCartridgeOidValue.Value.ToString();
            }
            else if (_deviceType == "Dune")
            {
                DuneDevice duneDevice = (DuneDevice)_device;
                SnmpOidValue blackCartridgeOidValue = duneDevice.Snmp.GetRaw(blackCartridgeOid);
                blackCartridgeStatus = blackCartridgeOidValue.Value.ToString();
                SnmpOidValue cyanCartridgeOidValue = duneDevice.Snmp.GetRaw(cyanCartridgeOid);
                cyanCartridgeStatus = blackCartridgeOidValue.Value.ToString();
                SnmpOidValue magentaCartridgeOidValue = duneDevice.Snmp.GetRaw(magentaCartridgeOid);
                magentaCartridgeStatus = blackCartridgeOidValue.Value.ToString();
                SnmpOidValue yellowCartridgeOidValue = duneDevice.Snmp.GetRaw(yellowCartridgeOid);
                yellowCartridgeStatus = blackCartridgeOidValue.Value.ToString();
            }

            if (blackCartridgeStatus.Contains("Install Black Cartridge") && cyanCartridgeStatus.Contains("Install Cyan Cartridge") && magentaCartridgeStatus.Contains("Install Magenta Cartridge") && yellowCartridgeStatus.Contains("Install Yellow Cartridge"))
            {
                return "All cartridges are empty";
            }
            else
            {
                List<string> emptyCartridges = new List<string>();
                if (blackCartridgeStatus.Contains("Install Black Cartridge"))
                    emptyCartridges.Add("Black");
                if (cyanCartridgeStatus.Contains("Install Cyan Cartridge"))
                    emptyCartridges.Add("Cyan");
                if (magentaCartridgeStatus.Contains("Install Magenta Cartridge"))
                    emptyCartridges.Add("Magenta");
                if (yellowCartridgeStatus.Contains("Install Yellow Cartridge"))
                    emptyCartridges.Add("Yellow");

                return string.Join(" and ", emptyCartridges) + " cartridge(s) are empty";
            }
        }

        /// <summary>
        /// Retrieves the status of the printer's paper trays using SNMP.
        /// </summary>
        /// <returns>A string indicating which trays are empty, or "No tray is empty" if all trays contain paper.</returns>
        public string GetTrayStatus()
        {
            string tray2Oid = "1.3.6.1.2.1.43.16.5.1.2.1.5";
            string tray3Oid = "1.3.6.1.2.1.43.18.1.1.8.1.18";
            string tray2Status = "";
            string tray3Status = "";
            if (_deviceType == "Jedi")
            {
                JediOmniDevice jediDevice = (JediOmniDevice)_device;
                SnmpOidValue tray2OidValue = jediDevice.Snmp.GetRaw(tray2Oid);
                tray2Status = tray2OidValue.Value.ToString();
                SnmpOidValue tray3OidValue = jediDevice.Snmp.GetRaw(tray3Oid);
                tray3Status = tray2OidValue.Value.ToString();
            }
            else if (_deviceType == "Dune")
            {
                DuneDevice duneDevice = (DuneDevice)_device;
                SnmpOidValue tray2OidValue = duneDevice.Snmp.GetRaw(tray2Oid);
                tray2Status = tray2OidValue.Value.ToString();
                SnmpOidValue tray3OidValue = duneDevice.Snmp.GetRaw(tray3Oid);
                tray3Status = tray2OidValue.Value.ToString();
            }
            if (tray2Status.Contains("Tray 2 empty") && tray3Status.Contains("Tray 3 empty"))
            {
                return "Tray 2 and 3 are empty";
            }
            else if (tray2Status.Contains("Tray 2 empty"))
            {
                return "Tray 2 is empty";
            }
            else if (tray3Status.Contains("Tray 3 empty"))
            {
                return "Tray 3 is empty";
            }
            else
            {
                return "No tray is empty";
            }
        }
        /// <summary>
        /// Retrieves the status of the printer's cartridges and trays using SNMP.
        /// </summary>
        /// <returns>A list of cartridge and tray statuses.</returns>
        public string GetCartridgeTrayStatus()
        {
            // Hardcoded OID value for firmware name
            string duneOID1 = ".1.3.6.1.2.1.43.18.1.1.8.1";
            string duneOID2 = ".1.3.6.1.2.1.43.18.1.1.8.2";
            string duneOID3 = ".1.3.6.1.2.1.43.18.1.1.8.3";
            string duneOID4 = ".1.3.6.1.2.1.43.18.1.1.8.4";
            string duneOID5 = ".1.3.6.1.2.1.43.18.1.1.8.5";
            string duneOID6 = ".1.3.6.1.2.1.43.18.1.1.8.6";
            string duneOID7 = ".1.3.6.1.2.1.43.18.1.1.8.7";
            string duneOID8 = ".1.3.6.1.2.1.43.18.1.1.8.8";
            string duneOID9 = ".1.3.6.1.2.1.43.18.1.1.8.9";
            List<string> statuses = new List<string>();
            if (_deviceType == "Jedi")
            {
                JediOmniDevice jediDevice = (JediOmniDevice)_device;
                SnmpOidValue duneOID1Value = jediDevice.Snmp.GetRaw(duneOID1);
                statuses.Add(duneOID1Value.Value.ToString());
                SnmpOidValue duneOID2Value = jediDevice.Snmp.GetRaw(duneOID2);
                statuses.Add(duneOID2Value.Value.ToString());
                SnmpOidValue duneOID3Value = jediDevice.Snmp.GetRaw(duneOID3);
                statuses.Add(duneOID3Value.Value.ToString());
                SnmpOidValue duneOID4Value = jediDevice.Snmp.GetRaw(duneOID4);
                statuses.Add(duneOID4Value.Value.ToString());
                SnmpOidValue duneOID5Value = jediDevice.Snmp.GetRaw(duneOID5);
                statuses.Add(duneOID5Value.Value.ToString());
                SnmpOidValue duneOID6Value = jediDevice.Snmp.GetRaw(duneOID6);
                statuses.Add(duneOID6Value.Value.ToString());
                SnmpOidValue duneOID7Value = jediDevice.Snmp.GetRaw(duneOID7);
                statuses.Add(duneOID7Value.Value.ToString());
            }
            else if (_deviceType == "Dune")
            {
                DuneDevice duneDevice = (DuneDevice)_device;
                SnmpOidValue duneOID1Value = duneDevice.Snmp.GetRaw(duneOID1);
                statuses.Add(duneOID1Value.Value.ToString());
                SnmpOidValue duneOID2Value = duneDevice.Snmp.GetRaw(duneOID2);
                statuses.Add(duneOID2Value.Value.ToString());
                SnmpOidValue duneOID3Value = duneDevice.Snmp.GetRaw(duneOID3);
                statuses.Add(duneOID3Value.Value.ToString());
                SnmpOidValue duneOID4Value = duneDevice.Snmp.GetRaw(duneOID4);
                statuses.Add(duneOID4Value.Value.ToString());
                SnmpOidValue duneOID5Value = duneDevice.Snmp.GetRaw(duneOID5);
                statuses.Add(duneOID5Value.Value.ToString());
                SnmpOidValue duneOID6Value = duneDevice.Snmp.GetRaw(duneOID6);
                statuses.Add(duneOID6Value.Value.ToString());
                SnmpOidValue duneOID7Value = duneDevice.Snmp.GetRaw(duneOID7);
                statuses.Add(duneOID7Value.Value.ToString());
            }
            
            List<string> uniqueStatuses = statuses.Distinct().ToList();

            List<string> messages = new List<string>();
            foreach (string status in uniqueStatuses)
            {
                switch (status)
                {
                    case "allTraysEmpty":
                        messages.Add("Empty Tray");
                        break;
                    case "cartridgeLow":
                        messages.Add("Cartridge Low");
                        break;
                    case "cartridgeMissing":
                        messages.Add("Cartridge Missing");
                        break;
                    case "cartridgeMemoryError":
                        messages.Add("Cartridge Problem");
                        break;
                    default:
                        break;
                }
            }

            if (messages.Count == 0)
            {
                return "No issues with Cartridge and Tray";
            }

            return string.Join(", ", messages);
        }
        #endregion CartridgeStatus

        /// <summary>
        /// Retrieves the CDM identity information of the device from a REST API.
        /// </summary>
        /// <returns>A <see cref="CDMIdentityData"/> object containing identity details, or an empty object if retrieval fails.</returns>
        public CDMIdentityData GetCDMIdentityInfo()
        {
            CDMIdentityData cDMIdentityData = new CDMIdentityData();
            try
            {
                string jsonUrl = $"https://{_ipaddress}/cdm/system/v1/identity";
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                using (HttpClient clients = new HttpClient())
                {
                    HttpResponseMessage response = clients.GetAsync(jsonUrl).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonData = response.Content.ReadAsStringAsync().Result;
                        ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => false;
                        cDMIdentityData = JsonConvert.DeserializeObject<CDMIdentityData>(jsonData);
                        return cDMIdentityData;
                    }
                    else
                    {
                        return cDMIdentityData;
                    }
                }
            }
            catch (Exception ex)
            {
                return cDMIdentityData;
            }
        }

        #region JobDetails
        /// <summary>
        /// Retrieves the list of processing job queue details from the device.
        /// </summary>
        /// <returns>A list of <see cref="JobQueueDetails"/> containing job queue information, or an empty list if retrieval fails.</returns>
        public List<JobQueueDetails> GetProcessingJobQueue()
        {
            List<JobQueueDetails> jobQueueDetails = new List<JobQueueDetails>();
            try
            {
                string jsonUrl = "";
                if (_deviceType == "Dune")
                {              
                    // jsonUrl = $"https://{_ipaddress}/cdm/jobManagement/v1/queue";
                    SetOAuth2StandardEnableTokenAuthFalse("9104", _ipaddress,"10");
                    jsonUrl = $"https://{_ipaddress}/cdm/jobManagement/v1/historyStats";
                    HP.GFriend.GFLogger.Logger.Debug("Job log details: " + jsonUrl);

                    ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                    using (HttpClient clients = new HttpClient())
                    {
                        HttpResponseMessage response = clients.GetAsync(jsonUrl).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            string jsonData = response.Content.ReadAsStringAsync().Result;
                            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => false;
                            dynamic jobDetails = JsonConvert.DeserializeObject(jsonData);
                            HP.GFriend.GFLogger.Logger.Debug("Job log details after deserializeObject: " + jobDetails);

                            //foreach (var job in jobDetails.jobList)
                            foreach (var job in jobDetails.historyStats)
                            {
                                JobQueueDetails queueDetails = new JobQueueDetails();
                                queueDetails.JobId = job.jobUuid;
                                queueDetails.State = job.jobInfo.state;
                                queueDetails.JobName = job?.jobInfo?.jobName ?? "NA";
                                queueDetails.JobType = job.jobInfo.jobCategory;
                                queueDetails.UserName = job.jobInfo.userName;
                                queueDetails.StartTime = job.jobInfo.startTime;
                                queueDetails.EndTime = job.jobInfo.endTime;
                                queueDetails.CompletionState = job.jobInfo.jobCompletionState;
                                jobQueueDetails.Add(queueDetails);
                                HP.GFriend.GFLogger.Logger.Debug("Job log details successfully: jobQueueDetails");
                            }
                        }
                    }
                }              
                else if (_deviceType == "Jedi")
                {
                    jsonUrl = $"https://{_ipaddress}/hp/device/JobLogReport/Index";
                    HP.GFriend.GFLogger.Logger.Debug("Job log details: " + jsonUrl);
                    AppLogger.Debug("Job log details: " + jsonUrl);

                    string timeOut = "60";
                    Browsers defaultBrowser = GetSystemDefaultBrowser();
                    HP.GFriend.GFLogger.Logger.Debug($"Default browser detected: {defaultBrowser}");
                    AppLogger.Debug($"Default browser detected: {defaultBrowser}");

                    DeviceUnderTest dut = new DeviceUnderTest();
                    string outputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs"); // or any valid path

                    AppLogger.Debug("Output Directory " + "outputDir");
                    CommonExecutionInfo.IsHeadlessMode = true; // This forces headless
                    Web web = new Web();
                    web.Initialize(dut, outputDir);

                    //Chrome browser
                    if (defaultBrowser == Browsers.Chrome)
                    {
                        HP.GFriend.GFLogger.Logger.Debug("Opening Job Log page in Chrome");
                        AppLogger.Debug("Opening Job Log page in Chrome");
                        KeywordResult res = web.OpenWithChrome(jsonUrl, timeOut);
                    }
                    //edge browser
                    else if (defaultBrowser == Browsers.Edge)
                    {
                        HP.GFriend.GFLogger.Logger.Debug("Opening Job Log page in Edge");
                        AppLogger.Debug("Opening Job Log page in Edge");
                        KeywordResult res = web.OpenWithEdge(jsonUrl, timeOut);
                    }

                    var driverField = web.GetType().GetField("_driver", BindingFlags.NonPublic | BindingFlags.Instance);
                    IWebDriver driver = (IWebDriver)driverField?.GetValue(web);

                    HP.GFriend.GFLogger.Logger.Debug("Attempting to retrieve WebDriver instance");
                    AppLogger.Debug("Attempting to retrieve WebDriver instance");
                    if (driver == null)
                    {
                        HP.GFriend.GFLogger.Logger.Error("WebDriver instance is NULL");
                        AppLogger.Debug("WebDriver instance is NULL");
                        throw new Exception("Failed to retrieve WebDriver instance from WebKeywords class.");
                    }

                    HP.GFriend.GFLogger.Logger.Debug("WebDriver instance successfully retrieved");
                    AppLogger.Debug("WebDriver instance successfully retrieved");
                    // Wait for login and table load    
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
                    wait.Until(d => ((OpenQA.Selenium.IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

                     // If "details-button" is present click it
                     var detailsButtons = driver.FindElements(By.Id("details-button"));
                     if (detailsButtons.Count > 0)
                     {
                        detailsButtons[0].Click();

                        var proceedLinks = driver.FindElements(By.Id("proceed-link"));
                        AppLogger.Debug("proceedLinks");
                        if (proceedLinks.Count > 0)
                        {
                             proceedLinks[0].Click();
                        }
                     }
                     Thread.Sleep(10000);

                     var passwordField = wait.Until(drv =>
                     {
                         var elems = drv.FindElements(By.Id("PasswordTextBox"));
                         return elems.Count > 0 ? elems[0] : null;
                     });

                     passwordField.SendKeys(_device.AdminPassword);

                     string typedValue = passwordField.GetAttribute("value");
                     Console.WriteLine($"Value typed into password box: {typedValue}");

                     wait.Until(drv =>
                     {
                         var elems = drv.FindElements(By.Id("signInOk"));
                         HP.GFriend.GFLogger.Logger.Debug("Login button clicked");
                         AppLogger.Debug("Login button clicked");
                         return elems.Count > 0 ? elems[0] : null;
                     }).Click();

                     Thread.Sleep(10000);
                     wait.Until(drv => drv.FindElement(By.Id("JobLogTable")));

                     IWebElement table = driver.FindElement(By.Id("JobLogTable"));

                     HP.GFriend.GFLogger.Logger.Debug("JobLogTable loaded successfully");
                     AppLogger.Debug("JobLogTable loaded successfully");
                     FileLogger.Debug("JobLogTable loaded");
                     IList<IWebElement> rows = table.FindElements(By.TagName("tr"));

                     for (int i = 1; i < rows.Count; i++)
                     {
                         IList<IWebElement> cells = rows[i].FindElements(By.TagName("td"));
                         if (cells.Count > 0)
                         {
                            //JobQueueDetails queueDetails = new JobQueueDetails
                            //{
                            //    JobId = "NA",
                            //    JobType = "NA",
                            //    PauseReason = "NA",
                            //    State = cells[3].Text,
                            //    JobName = cells[1].Text,
                            //    UserName = cells[2].Text,
                            //    StartTime = cells[4].Text,
                            //    EndTime = "NA"
                            //};
                            //jobQueueDetails.Add(queueDetails);
                            JobQueueDetails queueDetails = new JobQueueDetails
                            {
                                JobId = "NA",
                                JobType = "NA",
                                PauseReason = "NA",
                                JobName = cells[1].GetAttribute("innerText")?.Trim(),
                                UserName = cells[2].GetAttribute("innerText")?.Trim(),
                                State = cells[3].GetAttribute("innerText")?.Trim(),
                                StartTime = cells[4].GetAttribute("innerText")?.Trim(),
                                EndTime = "NA"
                            };
                            jobQueueDetails.Add(queueDetails);          
                            
                            AppLogger.Debug("Row HTML → " + rows[i].GetAttribute("innerHTML"));
                            AppLogger.Debug($"Total rows found in JobLogTable: {rows.Count}");
                            AppLogger.Debug($"Total JobQueueDetails added: {jobQueueDetails.Count}");
                            AppLogger.Debug($"Job[{jobQueueDetails.Count}] | Name={queueDetails.JobName}, " + $"User={queueDetails.UserName}, State={queueDetails.State}");
                        }
                    }
                     driver.Dispose();         
                }
                return jobQueueDetails;
            }
            catch (Exception ex)
            {
                HP.GFriend.GFLogger.Logger.Error("Exception occurred while fetching Job Log details", ex);
                AppLogger.Error("Exception occurred while fetching Job Log details", ex);
                string logPath = ExceptionLogger.LogException(ex);
                return jobQueueDetails;
            }
        }

        private Browsers GetBrowser()
        {
            try
            {
                return GetSystemDefaultBrowser();
            }
            catch
            {
                return Browsers.Edge; // SAFE fallback
            }
        }
        /// <summary>
        /// Retrieves the system's default web browser by checking the Windows registry.
        /// </summary>
        /// <returns>
        /// A <see cref="Browsers"/> enum value representing the default browser:
        /// Chrome, Edge, Firefox, IE, or Chrome as a fallback if not identifiable.
        /// </returns>
        public static Browsers GetSystemDefaultBrowser()
        {
            string defaultBrowserProgId = string.Empty;

            using (var userChoiceKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                       @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice"))
            {
                if (userChoiceKey != null)
                {
                    defaultBrowserProgId = userChoiceKey.GetValue("ProgId")?.ToString() ?? string.Empty;
                }
            }

            if (defaultBrowserProgId.IndexOf("Chrome", StringComparison.OrdinalIgnoreCase) >= 0)
                return Browsers.Chrome;
            else if (defaultBrowserProgId.IndexOf("Edge", StringComparison.OrdinalIgnoreCase) >= 0)
                return Browsers.Edge;
            else if (defaultBrowserProgId.IndexOf("Firefox", StringComparison.OrdinalIgnoreCase) >= 0)
                return Browsers.FireFox;
            else if (defaultBrowserProgId.IndexOf("IE.HTTP", StringComparison.OrdinalIgnoreCase) >= 0)
                return Browsers.IE;
            else
                return Browsers.Chrome; // fallback to Chrome
        }

        /// <summary>
        /// Retrieves the job history details from the device.
        /// </summary>
        /// <returns>A list of <see cref="JobHistoryDetails"/> containing job history information, or an empty list if retrieval fails.</returns>
        public List<JobHistoryDetails> GetHistoryJobDetails()
        {
            List<JobHistoryDetails> jobHistoryDetails = new List<JobHistoryDetails>();
            try
            {
                string jsonUrl = $"https://{_ipaddress}/cdm/jobManagement/v1/historyStats";
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                using (HttpClient clients = new HttpClient())
                {
                    HttpResponseMessage response = clients.GetAsync(jsonUrl).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonData = response.Content.ReadAsStringAsync().Result;
                        ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => false;
                        dynamic jobDetails = JsonConvert.DeserializeObject(jsonData);

                        foreach (var job in jobDetails.historyStats)
                        {
                            JobHistoryDetails historyDetails = new JobHistoryDetails();
                            historyDetails.JobId = job.jobUuid;
                            historyDetails.State = job.jobInfo.state;
                            historyDetails.JobName = job.jobInfo.jobName;
                            historyDetails.JobType = job.jobInfo.jobCategory;
                            historyDetails.UserName = job.jobInfo.userName;
                            historyDetails.StartTime = job.jobInfo.startTime;
                            historyDetails.EndTime = job.endTime;
                            jobHistoryDetails.Add(historyDetails);
                        }
                        return jobHistoryDetails;
                    }
                    else
                    {
                        return jobHistoryDetails;
                    }
                }
            }
            catch (Exception ex)
            {
                return jobHistoryDetails;
            }
        }
        #endregion JobDetails

        #region NativeAppDetails
        /// <summary>
        /// Retrieves a list of native applications available on the specified device.
        /// </summary>
        /// <param name="ipAddress">The IP address of the device.</param>
        /// <param name="password">The administrative password for authentication.</param>
        /// <returns>A list of native application details available on the device.</returns>
        public List<NativeAppData> GetNativeAplications(string ipAddress,string password)
        {
            List<NativeAppData> nativeDetails = new List<NativeAppData>();
            try
            {
                string jsonUrl = "";
                if (_deviceType == "Dune")
                {
                    SetOAuth2StandardEnableTokenAuthFalse("9104", ipAddress, "10");
                    jsonUrl = $"https://{ipAddress}/cdm/shortcut/v1/shortcuts";
                    ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                    using (HttpClient clients = new HttpClient())
                    {
                        HttpResponseMessage response = clients.GetAsync(jsonUrl).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            string jsonData = response.Content.ReadAsStringAsync().Result;

                            JObject jsonObject = JObject.Parse(jsonData);
                            JArray shortcuts = (JArray)jsonObject["shortcuts"];

                            foreach (JObject shortcut in shortcuts)
                            {
                                string type = shortcut["type"]?.ToString();

                                if (type == "nativeApp")
                                {
                                    NativeAppData nativeAppDetails = new NativeAppData
                                    {
                                        Id = shortcut["id"]?.ToString(),
                                        Title = shortcut["title"]?.ToString(),
                                        Name = shortcut["name"]?.ToString(),
                                        Description = shortcut["description"]?.ToString()
                                    };

                                    nativeDetails.Add(nativeAppDetails);
                                }
                            }
                            Console.WriteLine(jsonData); // Or use a breakpoint
                        }
                    }
                }    
                else
                {
                    DeviceNetworkAddress deviceNetworkAddress = new DeviceNetworkAddress(ipAddress);
                    UIConfigurationService uIConfigurationService = new UIConfigurationService(deviceNetworkAddress, password, 9222, "en-US");

                    GetApplicationAccessPointsResponse res = uIConfigurationService.GetApplicationAccessPoint();
                    foreach (var responsedata in res.GetApplicationAccessPointsResult)
                    {
                        NativeAppData Data = new NativeAppData
                        {
                            Id = responsedata.id,
                            Name = responsedata.name,
                            Title = ExtractValue(responsedata.title.ToString()),
                            Description = ExtractValue(responsedata.description.ToString())
                        };

                        nativeDetails.Add(Data);
                    }
                }
               return nativeDetails;                 
            }
            catch (Exception ex)
            {
                return nativeDetails;
            }
            return nativeDetails;
        }
        #endregion NativeAppDetails

        /// <summary>
        /// Sets OAuth2Standard PUB_testEnableTokenAuth false.
        /// </summary>
        /// <param name="time">wait time in seconds</param>  
        /// <param name="portNumber">server port number to be connected for auth2 standard disabling</param> 
        /// <returns><c>true, if device respond in specified sec</c><c>false</c> otherwise.</returns>
        public bool SetOAuth2StandardEnableTokenAuthFalse(string portNumber, string ipAddress, string time = "3")
        {
            if (!string.IsNullOrEmpty(portNumber))
            {
                try
                {
                    int timeOut = Convert.ToInt32(time);
                    int serverPort = Convert.ToInt32(portNumber);
                    // Define the Telnet server's IP address and port
                    string serverIp = ipAddress;

                    // Create a TCP client to connect to the Telnet server
                    using (TcpClient client = new TcpClient(serverIp, serverPort))
                    {
                        using (NetworkStream stream = client.GetStream())
                        using (StreamReader reader = new StreamReader(stream))
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            StringBuilder stringToStoreResponse = new StringBuilder();
                            int character;//to read the characters from reader
                            DateTime start = DateTime.Now;

                            //check the timeout in seconds
                            while (DateTime.Now - start < TimeSpan.FromSeconds(timeOut))
                            {
                                if (stream.DataAvailable)
                                {
                                    //The next character from the input stream represented as an System.Int32 object                               
                                    character = reader.Read();
                                    if (character == -1)
                                    {
                                        break;
                                    }
                                    stringToStoreResponse.Append((char)character);
                                }
                            }
                            string response = stringToStoreResponse.ToString();
                            if (response.Length == 0)
                            {
                                return false;
                            }
                            else
                            {
                                // Send Telnet commands
                                writer.WriteLine("OAuth2Standard PUB_testEnableTokenAuth false");
                                writer.WriteLine("exit");
                                writer.Flush();

                                var stringBuilderToReadResponse = new StringBuilder();
                                while ((character = reader.Read()) != -1)
                                {

                                    char c = (char)character;
                                    stringBuilderToReadResponse.Append(c);
                                }
                                return true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        #region SolutionsDetails
        /// <summary>
        /// Retrieves a list of installed solutions on the specified device.
        /// </summary>
        /// <param name="ipAddress">The IP address of the device.</param>
        /// <param name="password">The administrative password for authentication.</param>
        /// <returns>A list of installed solutions on the device, or an empty list if none are found.</returns>
        public List<DevicePackageInfo> GetInstalledSolutions(string ipAddress, string password)
        {
            List<DevicePackageInfo> installedSolutions = null;

            if (_deviceType == "Jedi")
            {
                HpkInstallData installData = new HpkInstallData();
                DeviceUnderTest deviceUnderTest = new DeviceUnderTest
                {
                    DeviceAddress = ipAddress,
                    AdminPassword = password
                };
                try
                {
                    installedSolutions = installData.GetPackages(deviceUnderTest);
                }
                catch (Exception ex)
                {
                    HP.GFriend.GFLogger.Logger.Debug($"Error retrieving device details: {ex.Message}" + "Error");
                }
            }
            else if (_deviceType == "Dune")
            {
                string jsonUrl = $"https://{ipAddress}/cdm/shortcut/v1/shortcuts";

                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                installedSolutions = new List<DevicePackageInfo>();

                using (HttpClient clients = new HttpClient())
                {
                    HttpResponseMessage response = clients.GetAsync(jsonUrl).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonData = response.Content.ReadAsStringAsync().Result;

                        JObject jsonObject = JObject.Parse(jsonData);
                        JArray shortcuts = (JArray)jsonObject["shortcuts"];

                        foreach (JObject shortcut in shortcuts)
                        {
                            string type = shortcut["type"]?.ToString();
                           // JToken iconsToken = shortcut["icons"];
                            if (type == "webApp")
                            {
                                string version = "";
                                if (shortcut["icons"] is JArray iconsArray)
                                {
                                    foreach (JObject icon in iconsArray)
                                    {
                                        if (icon["version"] != null)
                                        {
                                            version = icon["version"].ToString();
                                            break; // Stop after finding the first valid version
                                        }
                                    }
                                }
                                var devicepackageinfo = new DevicePackageInfo
                                {
                                    Name = shortcut["title"]?.ToString(),
                                    Version = version,
                                    Uuid = shortcut["id"]?.ToString(),
                                    Description = !string.IsNullOrEmpty(shortcut["description"]?.ToString()) ? "NA" : shortcut["description"].ToString(),
                                };
                                installedSolutions.Add(devicepackageinfo);
                            }
                        }
                    }  
                }
            }
            return installedSolutions ?? new List<DevicePackageInfo>();
        }

        /// <summary>
        /// Retrieves a list of installed solutions on the specified device.
        /// </summary>
        /// <param name="ipAddress">The IP address of the device.</param>
        /// <param name="password">The administrative password for authentication.</param>
        /// <returns>A list of installed solutions on the device, or an empty list if none are found.</returns>
        //public bool UnInstallSolution(string ipAddress,,string password,string solutionToUnInstall, ref List<HP.GFriend.Keywords.DevicePackageInfo> installedSolutions)
        //{
        //    try
        //    {
        //        DeviceUnderTest deviceUnderTest = new DeviceUnderTest
        //        {
        //            DeviceAddress = ipAddress,
        //            AdminPassword = password
        //        };
        //        foreach (HP.GFriend.Keywords.DevicePackageInfo devicePackageInfo in installedSolutions)
        //        {
        //            if(devicePackageInfo.Name.Trim().Equals(solutionToUnInstall))
        //            {
        //                HpkInstallData hpkInstallData = new HpkInstallData();
        //               // hpkInstallData.RemoveHpk(deviceUnderTest, devicePackageInfo.installedFileName);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Error retrieving device details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }

        //   // return installedSolutions ?? new List<HP.GFriend.Keywords.DevicePackageInfo>();
        //}
     

        /// <summary>
        /// Extracts solutions from a list of native application data by filtering entries where the ID matches the name.
        /// </summary>
        /// <param name="nativeAppData">A reference to the list of native application data.</param>
        /// <returns>A list of solutions extracted from the native application data.</returns>
        public List<NativeAppData> GetSolutionsFromNativeDetails(ref List<NativeAppData> nativeAppData)
        {
            List<NativeAppData> solutionData=new List<NativeAppData>();
            if (_deviceType == "Jedi")
            {
                solutionData = nativeAppData.Where(x => x.Id.Equals(x.Name)).ToList();

                foreach (var data in solutionData)
                {
                    nativeAppData.Remove(data);
                }
            }
            return solutionData;
        }
        #endregion SolutionsDetails

        /// <summary>
        /// Extracts the actual value from a formatted string containing a pattern like "value=ActualValue]".
        /// </summary>
        /// <param name="input">The input string potentially containing the value pattern.</param>
        /// <returns>
        /// The extracted value if the pattern is found; otherwise, returns the original input string.
        /// </returns>
        private string ExtractValue(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Match "value=ActualValue]"
            Match match = Regex.Match(input, @"value=(.*?)\]");
            if (match.Success)
            {
                return match.Groups[1].Value; // Extract only the actual value
            }
            return input; // Return original if pattern not found
        }
    }

    #endregion PrinterDetails
}
