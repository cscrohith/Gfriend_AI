using HP.GFriend.GFLogger;
using HP.GFriend.Keywords;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace HP.GFriend.Support
{
    public static class MFPUtils
    {
        public class TempWriter : TextWriter
        {
            public string output = string.Empty;
            public override Encoding Encoding { get { return Encoding.UTF8; } }
            public override void Write(string value)
            {
                output += value;
            }

            public override void WriteLine(string value)
            {
                output += (value + Environment.NewLine);
            }
        }
        public static KeywordResult IpSend(DeviceUnderTest dut, FileInfo fileToSend, int port = 9100)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                byte[] hpnpf = HP.GFriend.Keywords.Properties.Resources.hpnpf;

                Assembly hpnpfAssembly = Assembly.Load(hpnpf);
                MethodInfo methodInfo = hpnpfAssembly.EntryPoint;
                string[] parameters = new string[] { fileToSend.FullName, "-x", dut.DeviceAddress, "-p", port.ToString() };


                // Set console output to temp writer
                TextWriter consoleOut = Console.Out;
                TextWriter consoleError = Console.Error;
                TempWriter tempWriter = new TempWriter();
                Console.SetOut(tempWriter);
                Console.SetError(tempWriter);

                methodInfo.Invoke(null, new[] { parameters });
                
                // Restore console output
                Console.SetOut(consoleOut);
                Console.SetError(consoleError);
                
                
                if (tempWriter.output.Contains("Error"))
                {
                    result.Result = KeywordResults.Error;
                    result.Output = tempWriter.output;
                    Logger.Error(tempWriter.output);
                }
                
            }
            catch(Exception ex)
            {
                Logger.Error("Can not send via IP send", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during IP Send";
            }
            return result;
        }

        public static KeywordResult IpSend(DeviceUnderTest dut, string stringToSend, int port = 9100)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                IpSend(dut.DeviceAddress, Encoding.UTF8.GetBytes(stringToSend), port);
            }
            catch(Exception ex)
            {
                Logger.Error("Can not send via IP send", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during IP Send";
            }
            return result;
        }

        private static void IpSend(string targetIP, byte[] bytesToSend, int port = 9100)
        {

            TcpClient client = new TcpClient();
            client.Connect(targetIP, port);

            var stream = client.GetStream();
            stream.Write(bytesToSend, 0, bytesToSend.Length);

            client.Close();
        }

        private static void IpSend_old(string targetIP, byte[] bytesToSend, int port = 9100)
        {
            Socket rawPrtSoc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            rawPrtSoc.NoDelay = true;

            IPAddress ip = IPAddress.Parse(targetIP);
            IPEndPoint ipep = new IPEndPoint(ip, port);
            rawPrtSoc.Connect(ipep);

            Logger.Debug($"To Send : {bytesToSend.Length}");
            int sent = rawPrtSoc.Send(bytesToSend);
            Logger.Debug($"Sent : {sent}");
            Thread.Sleep(10000);
            
            rawPrtSoc.Close();
        }

        public static void Send(Socket socket, byte[] buffer, int offset, int size, int timeout = 100000)
        {
            int startTickCount = Environment.TickCount;
            int sent = 0;  // how many bytes is already sent
            do
            {
                if (Environment.TickCount > startTickCount + timeout)
                    throw new Exception("Timeout.");
                try
                {
                    sent += socket.Send(buffer, offset + sent, size - sent, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably full, wait and try again
                        Thread.Sleep(30);
                    }
                    else
                        throw ex;  // any serious error occurr
                }
            } while (sent < size);
        }
    }
}
