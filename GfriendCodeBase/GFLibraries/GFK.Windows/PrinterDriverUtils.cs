using HP.GFriend.GFLogger;
using System;
using System.IO;
using System.Management;
using System.Printing;
using System.Runtime.InteropServices;

namespace HP.GFriend.Keywords
{
    public class PrinterDriverUtils
    {
        private static ManagementScope _managementScope = null;
        private static class Winspool
        {
            [StructLayout(LayoutKind.Sequential)]
            private class PRINTER_DEFAULTS
            {
                public string pDatatype;
                public IntPtr pDevMode;
                public int DesiredAccess;
            }

            [DllImport("winspool.drv", EntryPoint = "XcvDataW", SetLastError = true)]
            private static extern bool XcvData(
                IntPtr hXcv,
                [MarshalAs(UnmanagedType.LPWStr)] string pszDataName,
                IntPtr pInputData,
                uint cbInputData,
                IntPtr pOutputData,
                uint cbOutputData,
                out uint pcbOutputNeeded,
                out uint pwdStatus);

            [DllImport("winspool.drv", EntryPoint = "OpenPrinterA", SetLastError = true)]
            private static extern int OpenPrinter(
                string pPrinterName,
                ref IntPtr phPrinter,
                PRINTER_DEFAULTS pDefault);

            [DllImport("winspool.drv", EntryPoint = "ClosePrinter")]
            private static extern int ClosePrinter(IntPtr hPrinter);

            [DllImport("winspool.drv", EntryPoint = "DeletePrinter")]
            private static extern bool DeletePrinter(IntPtr hPrinter);


            public static int AddLocalPort(string portName)
            {
                PRINTER_DEFAULTS def = new PRINTER_DEFAULTS();

                def.pDatatype = null;
                def.pDevMode = IntPtr.Zero;
                def.DesiredAccess = 1; //Server Access Administer

                IntPtr hPrinter = IntPtr.Zero;

                int n = OpenPrinter(",XcvMonitor Local Port", ref hPrinter, def);
                if (n == 0)
                    return Marshal.GetLastWin32Error();

                if (!portName.EndsWith("\0"))
                    portName += "\0"; // Must be a null terminated string

                // Must get the size in bytes. Rememeber .NET strings are formed by 2-byte characters
                uint size = (uint)(portName.Length * 2);

                // Alloc memory in HGlobal to set the portName
                IntPtr portPtr = Marshal.AllocHGlobal((int)size);
                Marshal.Copy(portName.ToCharArray(), 0, portPtr, portName.Length);

                uint needed; // Not that needed in fact...
                uint xcvResult; // Will receive de result here

                XcvData(hPrinter, "AddPort", portPtr, size, IntPtr.Zero, 0, out needed, out xcvResult);

                ClosePrinter(hPrinter);
                Marshal.FreeHGlobal(portPtr);

                return (int)xcvResult;
            }
        }


        public class PrinterInstallationStatus
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public Exception Ex { get; set; }
        }

        private static void ConnectManagementScope()
        {
            ConnectionOptions wmiConnectionOptions = new ConnectionOptions();
            wmiConnectionOptions.EnablePrivileges = true;
            wmiConnectionOptions.Impersonation = ImpersonationLevel.Impersonate;
            wmiConnectionOptions.Authentication = AuthenticationLevel.Packet;
            
            _managementScope = new ManagementScope(ManagementPath.DefaultPath, wmiConnectionOptions);
            

            //_managementScope = new ManagementScope(ManagementPath.DefaultPath);
            //_managementScope.Connect();

        }

        #region AddPrinter

        public static PrinterInstallationStatus AddPrinter(string printerName, string printerIP, string driverPath, string driverName)
        {
            try
            {
                ConnectManagementScope();
                try
                {
                    if (!CreateTCPIPPrinterPort(printerIP))
                    {
                        return new PrinterInstallationStatus() { Success = false, Message = "Port creation fail" };
                    }
                }
                catch(Exception ex)
                {
                    Logger.Error("Error during printer port creation", ex);
                    return new PrinterInstallationStatus() { Success = false, Message = ex.ToString(), Ex = ex };
                }

                try
                {
                    if(!CreatePrinterDriver(driverPath, driverName))
                    {
                        return new PrinterInstallationStatus() { Success = false, Message = "Driver creation fail" };
                    }
                }
                catch(Exception ex)
                {
                    Logger.Error("Error during printer driver creation", ex);
                    return new PrinterInstallationStatus() { Success = false, Message = ex.ToString(), Ex = ex };
                }

                ManagementClass printerClass = new ManagementClass(_managementScope, new ManagementPath("Win32_Printer"), new ObjectGetOptions());
                printerClass.Get();
                ManagementObject printer = printerClass.CreateInstance();
                printer.SetPropertyValue("DriverName", driverName);
                printer.SetPropertyValue("PortName", printerIP);
                printer.SetPropertyValue("Name", printerName);
                printer.SetPropertyValue("DeviceID", printerName);
                printer.SetPropertyValue("Network", true);
                printer.SetPropertyValue("Shared", false);
                printer.Put();
                return new PrinterInstallationStatus() { Success = true };
            }
            catch(Exception ex)
            {
                Logger.Error("Error during adding printer", ex);
                return new PrinterInstallationStatus() { Success = false, Message = "Error during adding printer", Ex = ex };
            }
        }

        public static PrinterInstallationStatus AddLocalPrinter(string printerName, string portName, string driverPath, string driverName)
        {
            try
            {
                ConnectManagementScope();

                try
                {
                    if (!CreatePrinterDriver(driverPath, driverName))
                    {
                        return new PrinterInstallationStatus() { Success = false, Message = "Driver creation fail" };
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error during printer driver creation", ex);
                    return new PrinterInstallationStatus() { Success = false, Message = ex.ToString(), Ex = ex };
                }

                Winspool.AddLocalPort(portName);
                
                ManagementClass printerClass = new ManagementClass(_managementScope, new ManagementPath("Win32_Printer"), new ObjectGetOptions());
                printerClass.Get();
                ManagementObject printer = printerClass.CreateInstance();
                printer.SetPropertyValue("DriverName", driverName);
                printer.SetPropertyValue("PortName", portName);
                printer.SetPropertyValue("Name", printerName);
                printer.SetPropertyValue("DeviceID", printerName);
                printer.SetPropertyValue("Local", true);
                printer.Put();
                

                return new PrinterInstallationStatus() { Success = true };
            }
            catch (Exception ex)
            {
                Logger.Error("Error during adding printer", ex);
                return new PrinterInstallationStatus() { Success = false, Message = "Error during adding printer", Ex = ex };
            }
        }

        private static bool CreatePrinterDriver(string printerDriverInfPath, string driverName)
        {
            bool endResult = false;
            ManagementClass printerDriverClass = new ManagementClass(_managementScope, new ManagementPath("Win32_PrinterDriver"), new ObjectGetOptions());
            ManagementObject printerDriver = printerDriverClass.CreateInstance();
            printerDriver.SetPropertyValue("Name", driverName);
            printerDriver.SetPropertyValue("FilePath", Path.GetDirectoryName(printerDriverInfPath));
            printerDriver.SetPropertyValue("InfName", printerDriverInfPath);

            // Obtain in-parameters for the method
            using (ManagementBaseObject inParams = printerDriverClass.GetMethodParameters("AddPrinterDriver"))
            {
                
                inParams["DriverInfo"] = printerDriver;
                // Execute the method and obtain the return values.            

                using (ManagementBaseObject result = printerDriverClass.InvokeMethod("AddPrinterDriver", inParams, null))
                {
                    uint errorCode = (uint)result.Properties["ReturnValue"].Value;
                    switch (errorCode)
                    {
                        case 0:
                            endResult = true;
                            break;
                        case 5:
                            Logger.Error("Access Denied.");
                            break;
                        case 123:
                            Logger.Error("The filename, directory name, or volume label syntax is incorrect.");
                            break;
                        case 1801:
                            Logger.Error("Invalid Printer Name.");
                            break;
                        case 1930:
                            Logger.Error("Incompatible Printer Driver.");
                            break;
                        case 3019:
                            Logger.Error("The specified printer driver was not found on the system and needs to be downloaded.");
                            break;
                        default:
                            Logger.Error($"Unspecified error : {errorCode}");
                            break;
                    }
                }
            }
            return endResult;
        }

        private static bool CreateTCPIPPrinterPort(string printerIP)
        {

            if (CheckTCPIPPrinterPort(printerIP))
                return true;

            ManagementClass printerPortClass = new ManagementClass(_managementScope, new ManagementPath("Win32_TCPIPPrinterPort"), new ObjectGetOptions());
            printerPortClass.Get();
            ManagementObject newPrinterPort = printerPortClass.CreateInstance();
            newPrinterPort.SetPropertyValue("Name", printerIP);
            newPrinterPort.SetPropertyValue("Protocol", 1);
            newPrinterPort.SetPropertyValue("HostAddress", printerIP);
            newPrinterPort.SetPropertyValue("PortNumber", 9100);
            newPrinterPort.SetPropertyValue("SNMPEnabled", true);
            newPrinterPort.Put();
            return true;
        }

       
        private static bool CheckTCPIPPrinterPort(string printerPortName)
        {

            //Query system for Operating System information
            ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_TCPIPPrinterPort");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(_managementScope, query);

            ManagementObjectCollection queryCollection = searcher.Get();
            foreach (ManagementObject m in queryCollection)
            {
                if (m["Name"].ToString() == printerPortName)
                    return true;
            }
            return false;
        }

        #endregion

        public static bool DeletePrinter(string printerName)
        {
            using (PrintServer ps = new PrintServer())
            {
                using (PrintQueue pq = new PrintQueue(ps, printerName, PrintSystemDesiredAccess.AdministratePrinter))
                {
                    pq.Purge();
                }
            }

            ConnectManagementScope();

            SelectQuery oSelectQuery = new SelectQuery();
            oSelectQuery.QueryString = @"SELECT * FROM Win32_Printer WHERE Name = '" + printerName + "'";

            ManagementObjectSearcher oObjectSearcher = new ManagementObjectSearcher(_managementScope, oSelectQuery);
            ManagementObjectCollection oObjectCollection = oObjectSearcher.Get();

            if (oObjectCollection.Count != 0)
            {
                foreach (ManagementObject oItem in oObjectCollection)
                {
                    oItem.Delete();
                    return true;
                }
            }
            return false;
        }


        public static void SetDefaultPrinter(string sPrinterName)
        {
            ConnectManagementScope();

            SelectQuery oSelectQuery = new SelectQuery();
            oSelectQuery.QueryString = @"SELECT * FROM Win32_Printer WHERE Name = '" + sPrinterName;

            ManagementObjectSearcher oObjectSearcher = new ManagementObjectSearcher(_managementScope, oSelectQuery);
            ManagementObjectCollection oObjectCollection = oObjectSearcher.Get();

            if (oObjectCollection.Count != 0)
            {
                foreach (ManagementObject oItem in oObjectCollection)
                {
                    oItem.InvokeMethod("SetDefaultPrinter", new object[] { sPrinterName });
                    return;

                }
            }
        }
        //Gets the printer information
        public static void GetPrinterInfo(string sPrinterName)
        {
            ConnectManagementScope();

            SelectQuery oSelectQuery = new SelectQuery();
            //oSelectQuery.QueryString = @"SELECT * FROM Win32_Printer WHERE Name = '" + sPrinterName.Replace("\", "\\") + "'";
            oSelectQuery.QueryString = @"SELECT * FROM Win32_Printer WHERE Name = '" + sPrinterName;

            ManagementObjectSearcher oObjectSearcher = new ManagementObjectSearcher(_managementScope, @oSelectQuery);
            ManagementObjectCollection oObjectCollection = oObjectSearcher.Get();

            foreach (ManagementObject oItem in oObjectCollection)
            {
                Console.WriteLine("Name : " + oItem["Name"].ToString());
                Console.WriteLine("PortName : " + oItem["PortName"].ToString());
                Console.WriteLine("DriverName : " + oItem["DriverName"].ToString());
                Console.WriteLine("DeviceID : " + oItem["DeviceID"].ToString());
                Console.WriteLine("Shared : " + oItem["Shared"].ToString());
                Console.WriteLine("---------------------------------------------------------------");
            }
        }
        //Checks whether a printer is installed
        public bool IsPrinterInstalled(string sPrinterName)
        {
            ConnectManagementScope();

            SelectQuery oSelectQuery = new SelectQuery();
            //oSelectQuery.QueryString = @"SELECT * FROM Win32_Printer WHERE Name = '" + sPrinterName.Replace("\", "\\") + "'";
            oSelectQuery.QueryString = @"SELECT * FROM Win32_Printer WHERE Name = '" + sPrinterName;

            ManagementObjectSearcher oObjectSearcher = new ManagementObjectSearcher(_managementScope, oSelectQuery);
            ManagementObjectCollection oObjectCollection = oObjectSearcher.Get();

            return oObjectCollection.Count > 0;
        }


        
        public static void RenamePrinter(string sPrinterName, string newName)
        {
            ConnectManagementScope();

            SelectQuery oSelectQuery = new SelectQuery();
            //oSelectQuery.QueryString = @"SELECT * FROM Win32_Printer WHERE Name = '" + sPrinterName.Replace("\", "\\") + "'";
            oSelectQuery.QueryString = @"SELECT * FROM Win32_Printer WHERE Name = '" + sPrinterName;

            ManagementObjectSearcher oObjectSearcher = new ManagementObjectSearcher(_managementScope, oSelectQuery);
            ManagementObjectCollection oObjectCollection = oObjectSearcher.Get();

            if (oObjectCollection.Count != 0)
            {
                foreach (ManagementObject oItem in oObjectCollection)
                {
                    oItem.InvokeMethod("RenamePrinter", new object[] { newName });
                    return;
                }
            }
        }
    }
}


