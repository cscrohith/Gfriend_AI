using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Printing;
using System.Drawing.Printing;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Diagnostics;
using HP.GFriend.GFLogger;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Management;
using System.Text.RegularExpressions;
using System.Globalization;

namespace HP.GFriend.Keywords
{
    public static class PrintQueueUtils
    {
        public static PrintQueue GetPrintQueue(string name)
        {
            LocalPrintServer localPrintServer = new LocalPrintServer();
            return localPrintServer.GetPrintQueue(name);
        }
   
        public static int GetJobCount(PrintQueue queue)
        {
            return queue.GetPrintJobInfoCollection().Count();
        }


        public static PrintSystemJobInfo GetJob(PrintQueue queue, string jobName, bool exactMatch)
        {
            foreach (PrintSystemJobInfo job in queue.GetPrintJobInfoCollection())
            {
                if (exactMatch && job.Name.Equals(jobName))
                {
                    return job;
                }
                else if (!exactMatch && job.Name.Contains(jobName))
                {
                    return job;
                }
            }
            return null;
        }

        public static PrintSystemJobInfo GetCurrentJob(PrintQueue queue)
        {
            try
            {
                return queue.GetPrintJobInfoCollection().First() ?? null;
            }
            catch (Exception)
            {
                return null;
            }

        }

        public static bool PauseJob(PrintSystemJobInfo job)
        {
            job.Pause();
            return job.IsPaused;
        }

        public static bool ResumeJob(PrintSystemJobInfo job)
        {
            job.Resume();
            return !job.IsPaused;
        }

        public static bool DeleteJob(PrintSystemJobInfo job)
        {
            PrintQueue printQueue = job.HostingPrintQueue;
            int jobId = job.JobIdentifier;
            job.Cancel();
            foreach (PrintSystemJobInfo queueJob in printQueue.GetPrintJobInfoCollection())
            {
                if (queueJob.JobIdentifier.Equals(jobId))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool EmptyQueue(PrintQueue queue)
        {
            foreach (PrintSystemJobInfo job in queue.GetPrintJobInfoCollection())
            {
                job.Cancel();
            }
            if (queue.GetPrintJobInfoCollection().Count() > 0)
            {
                return false;
            }
            return true;
        }

        #region PrintFileWithExtension
        /// <summary>
        ///Code to Print the file with given extension
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="printQueueName"></param>
        public static void PrintFile(string filePath, string printQueueName)
        {
            PrintFileWithExtension(filePath, printQueueName);      
        }
        /// <summary>
        /// Code to Print the file with given file extension
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="printQueueName"></param>
        private static void PrintFileWithExtension(string filePath, string printQueueName)
        {
            try
            {
                using (Process process = new Process())
                {
                    ProcessStartInfo processStartInfo = new ProcessStartInfo()
                    {
                        FileName = filePath,
                        Verb = "PrintTo".ToString().ToLower(CultureInfo.CurrentCulture),
                        Arguments = string.Format("\"{0}\"",printQueueName),
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = true,
                        ErrorDialog = false
                    };
                    process.StartInfo = processStartInfo;
                    process.Start();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}