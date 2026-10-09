using System.Xml;
using System.IO;
using System;
using HP.GFriend.Core.Execution;

namespace HP.GFriend.Core
{
    public class Report
    {
        public int Pass { get; set; }
        public int Fail { get; set; }
        public int Error { get; set; }
        public string Overall { get; set; }
        public string ReportPath { get; }
        public string ReportID { get; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }


        public DateTime CreationTime { get; }



        private string _outputPath;

        public Report(string outputXML)
        {
            _outputPath = outputXML;
            ReportPath = outputXML.Replace("output.xml", "report.html");
            ReportID = Path.GetDirectoryName(_outputPath);
            ReportID = ReportID.Replace(Directory.GetParent(ReportID).FullName, string.Empty).Replace("\\", string.Empty);
            CreationTime = File.GetCreationTime(outputXML);
            Pass = 0;
            Fail = 0;
            Error = 0;
            Overall = "PASS";
            Load();

        }

        private void Load()
        {
            if (!File.Exists(ReportPath))
            {
                Reporter.FixXmlOutput(_outputPath);
            }
            XmlDocument output = new XmlDocument();
            output.Load(_outputPath);
            XmlNodeList tcResults = output.SelectNodes("//TestCase/Result");
            foreach (XmlNode tcResult in tcResults)
            {
                string result = tcResult.InnerText.Trim().ToUpper();
                switch (result)
                {
                    case "PASS":
                        Pass++;
                        break;
                    case "FAIL":
                        Fail++;
                        if (!Overall.Equals("ERROR"))
                        {
                            Overall = "FAIL";
                        }
                        break;
                    case "ERROR":
                        Error++;
                        Overall = "ERROR";
                        break;
                }
            }

            // Getting Start and Endtime
            XmlNode timeInfo = output.SelectSingleNode("//TestSuite/StartTime");
            StartTime = DateTime.Parse(timeInfo.InnerText.Trim());
            timeInfo = output.SelectSingleNode("//TestSuite/EndTime");
            EndTime = DateTime.Parse(timeInfo.InnerText.Trim());
            Duration = EndTime.Subtract(StartTime);
        }
    }
}
