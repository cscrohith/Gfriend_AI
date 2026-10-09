using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace HP.GFriend.UI
{
    public partial class GetTouchEventForm : Form
    {
        private string _adbLocation;
        private Point displayXY;
        private Point relativeXY;
        private Point currentXY;
        private Point minXY;
        private Point maxXY;
        private Point displaySize;
        
        private Thread monitoringThread;
        private Process monitoringProc;
        private bool exitCondition;

        public GetTouchEventForm()
        {
            InitializeComponent();
            _adbLocation = Path.Combine(Environment.CurrentDirectory, @"tools/adb.exe");
        }

        delegate void AppendToOutputCallback(string text);
        private void AppendToOutput(string text)
        {
            if (this.textConsole.InvokeRequired)
            {
                AppendToOutputCallback atc = new AppendToOutputCallback(AppendToOutput);
                this.Invoke(atc, new object[] { text });
            }
            else
            {
                textConsole.AppendText(text + "\r\n");
            }
        }

        private void GetTouchEventForm_Load(object sender, EventArgs e)
        {
            AppendToOutput("Initializing....");
            GetDisplaySize();
            AppendToOutput(string.Format("Display Size : {0}x{1}", displaySize.X, displaySize.Y));
            GetMinMaxXY();
            AppendToOutput(string.Format("Min X,Y Position : ({0},{1})", minXY.X, minXY.Y));
            AppendToOutput(string.Format("Max X,Y Position : ({0},{1})", maxXY.X, maxXY.Y));
            exitCondition = false;

            AppendToOutput("Touch on target device then position will displayed...");
            monitoringThread = new Thread(new ThreadStart(MonitorTouchEvent));
            monitoringThread.Start();
        }


        private void GetTouchEventForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            exitCondition = true;
            monitoringProc.Kill();
            monitoringThread.Join();
        }

        private void GetDisplaySize()
        {
            Process adbProc;
            ProcessStartInfo adbInfo = new ProcessStartInfo();
            string stdOutput;
            adbInfo.FileName = _adbLocation;
            adbInfo.Arguments = "shell dumpsys window";
            adbInfo.RedirectStandardOutput = true;
            adbInfo.CreateNoWindow = true;
            adbInfo.UseShellExecute = false;

            using (adbProc = Process.Start(adbInfo))
            {
                while ((stdOutput = adbProc.StandardOutput.ReadLine()) != null)
                {
                    if(stdOutput.Contains("mUnrestrictedScreen"))
                    {
                        string tmpStr = stdOutput.Split(' ').Last();
                        displaySize.X = Int32.Parse(tmpStr.Split('x')[0]);
                        displaySize.Y = Int32.Parse(tmpStr.Split('x')[1]);
                        break;
                    }
                }
            }
        }

        private void GetMinMaxXY()
        {
            Process adbProc;
            ProcessStartInfo adbInfo = new ProcessStartInfo();
            string stdOutput;
            adbInfo.FileName = _adbLocation;
            adbInfo.Arguments = "shell getevent -p -l";
            adbInfo.RedirectStandardOutput = true;
            adbInfo.CreateNoWindow = true;
            adbInfo.UseShellExecute = false;

            using (adbProc = Process.Start(adbInfo))
            {
                while ((stdOutput = adbProc.StandardOutput.ReadLine()) != null)
                {
                    if (stdOutput.Contains("ABS_MT_POSITION_X"))
                    {
                        foreach(string tmpStr in stdOutput.Split(','))
                        {
                            if(tmpStr.Contains("min"))
                            {
                                minXY.X = Int32.Parse(tmpStr.Split(' ').Last());
                            }
                            else if(tmpStr.Contains("max"))
                            {
                                maxXY.X = Int32.Parse(tmpStr.Split(' ').Last());
                            }
                        }

                    }
                    else if(stdOutput.Contains("ABS_MT_POSITION_Y"))
                    {
                        foreach (string tmpStr in stdOutput.Split(','))
                        {
                            if (tmpStr.Contains("min"))
                            {
                                minXY.Y = Int32.Parse(tmpStr.Split(' ').Last());
                            }
                            else if (tmpStr.Contains("max"))
                            {
                                maxXY.Y = Int32.Parse(tmpStr.Split(' ').Last());
                            }
                        }
                    }
                }
            }

        }

        public void MonitorTouchEvent()
        {
            bool getX;
            bool getY;


            ProcessStartInfo adbInfo = new ProcessStartInfo();
            string stdOutput;
            adbInfo.FileName = _adbLocation;
            adbInfo.Arguments = "shell getevent -l";
            adbInfo.RedirectStandardOutput = true;
            adbInfo.CreateNoWindow = true;
            adbInfo.UseShellExecute = false;
            
            getX = false;
            getY = false;
            
            using (monitoringProc = Process.Start(adbInfo))
            {
                while (true)
                {
                    if((stdOutput = monitoringProc.StandardOutput.ReadLine()) != null)
                    {
                        if (stdOutput.Contains("ABS_MT_POSITION_X"))
                        {
                            getX = true;
                            string tmpStr = stdOutput.Trim().Split(' ').Last();
                            currentXY.X = Int32.Parse(tmpStr, System.Globalization.NumberStyles.HexNumber);
                        }
                        else if (stdOutput.Contains("ABS_MT_POSITION_Y"))
                        {
                            getY = true;
                            string tmpStr = stdOutput.Trim().Split(' ').Last();
                            currentXY.Y = Int32.Parse(tmpStr, System.Globalization.NumberStyles.HexNumber);
                        }

                        if (getX && getY)
                        {
                            getX = false;
                            getY = false;

                            // Calculate Display Coordinate and Relative Position
                            displayXY.X = (currentXY.X - minXY.X) * displaySize.X / (maxXY.X - minXY.X + 1);
                            displayXY.Y = (currentXY.Y - minXY.Y) * displaySize.Y / (maxXY.Y - minXY.Y + 1);
                            relativeXY.X = (int)((double)displayXY.X / (double)displaySize.X * 100);
                            relativeXY.Y = (int)((double)displayXY.Y / (double)displaySize.Y * 100);
                            AppendToOutput(string.Format("Absolute Position : ({0},{1})", displayXY.X, displayXY.Y));
                            AppendToOutput(string.Format("Relative Position (%) : ({0},{1})", relativeXY.X, relativeXY.Y));
                            AppendToOutput("");
                        }

                    }

                    if (exitCondition)
                    {
                        try
                        {
                            monitoringProc.Kill();
                        }
                        catch (Exception) { }
                        
                        break;
                    }

                }
            }
        }



    }
}
