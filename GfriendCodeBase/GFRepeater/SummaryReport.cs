using HP.GFriend.Repeater.Properties;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HP.GFriend.Core
{
    public class SummaryReport
    {
        private int _pass;
        private int _fail;
        private int _error;
        private string _exeucitonId;
        private string _serverEndPoint;
        private string _resultBaseUrl;

        private string _rootFolder;
        private List<string> _outputXMLs;
        private List<Report> _reports;

        public SummaryReport(string rootFolder, string executionID = null, string serverEndPoint = null)
        {
            _rootFolder = rootFolder;
            _pass = 0;
            _fail = 0;
            _error = 0;
            _exeucitonId = executionID;
            _serverEndPoint = serverEndPoint;
            _resultBaseUrl = string.Join("/", _serverEndPoint, "result", _exeucitonId);
            _outputXMLs = new List<string>();
            _reports = new List<Report>();
        }

        public string Generate()
        {
            _outputXMLs = Directory.GetFiles(_rootFolder, "output.xml", SearchOption.AllDirectories).ToList();
            foreach (string outputXML in _outputXMLs)
            {
                Report report = new Report(outputXML);
                _pass += report.Pass;
                _fail += report.Fail;
                _error += report.Error;
                _reports.Add(report);
            }

            _reports = _reports.OrderBy(o => o.CreationTime).ToList();

            string summaryHTML = Path.Combine(_rootFolder, "SummaryReport.html");
            using (StreamWriter writer = new StreamWriter(summaryHTML, false, Encoding.UTF8))
            {
                writer.AutoFlush = true;
                writer.WriteLine(Resources.HTMLHeader);

                if(!string.IsNullOrEmpty(_exeucitonId))
                {
                    string reportUrl = $"{_serverEndPoint}/api/execution/{_exeucitonId}/testreport/";
                    writer.WriteLine($"Click <a href=\"{reportUrl}\">here</a> to download this report<br><br>");
                }

                writer.WriteLine("<h2>Test Summary</h2><br>");
                writer.WriteLine("<table class =\"table table - borderd\">");
                writer.WriteLine("<tr class=\"active\" style=\"width: 210px\">");
                writer.WriteLine("<th style='width: 70px'>Total</th>");
                writer.WriteLine("<th style='width: 70px'>Pass</th>");
                writer.WriteLine("<th style='width: 70px'>Fail</th>");
                writer.WriteLine("<th style='width: 70px'>Error</th>");
                writer.WriteLine("</tr>");


                // Summary
                writer.WriteLine("<TR>");
                writer.WriteLine($"<td class=warning>{_pass + _fail + _error}</td>");
                writer.WriteLine($"<td class=success>{_pass}</td>");
                writer.WriteLine($"<td class=danger>{_fail}</td>");
                writer.WriteLine($"<td class=danger>{_error}</td>");
                writer.WriteLine("</TR></TABLE><BR>");

                // Global Graphs
                string[] imgFiles = Directory.GetFiles(_rootFolder, "*.png");
                if(imgFiles.Length > 0)
                {
                    writer.WriteLine("<h2>Test Graphs</h2><br>");
                    writer.WriteLine("Click graph to download raw data(csv file.)<br>");
                    foreach (string img in imgFiles)
                    {
                        string chartName = Path.GetFileNameWithoutExtension(img);
                        chartName = chartName.Replace("Global_", "").Replace("_", " ");
                        writer.WriteLine($"<h3>{chartName}</h3><br>");
                        writer.WriteLine($"<a href=\"{_resultBaseUrl}/{Path.GetFileNameWithoutExtension(img)}.csv\"><img src=\"{_resultBaseUrl}/{Path.GetFileName(img)}\"></a><br><br>");
                    }
                }
                


                // For each Testsuites
                writer.WriteLine("<BR><h2>Test Details</h2><BR>");
                writer.WriteLine("<table class =\"table table - borderd\">");
                writer.WriteLine("<tr class=active><th>Test ID</th><th>Result</th><th>Pass</th><th>Fail</th><th>Error</th><th>Start Time</th><th>End Time</th><th>Duration</th><th>Report</th></tr>");
                foreach (Report report in _reports)
                {
                    writer.WriteLine("<TR>");
                    writer.WriteLine($"<TD>{report.ReportID}</TD>");
                    writer.WriteLine($"<TD>{report.Overall}</TD>");
                    writer.WriteLine($"<TD>{report.Pass}</TD>");
                    writer.WriteLine($"<TD>{report.Fail}</TD>");
                    writer.WriteLine($"<TD>{report.Error}</TD>");
                    writer.WriteLine($"<TD>{report.StartTime.ToString("yyyy-MM-dd HH:mm:ss")}</TD>");
                    writer.WriteLine($"<TD>{report.EndTime.ToString("yyyy-MM-dd HH:mm:ss")}</TD>");
                    writer.WriteLine($"<TD>{report.Duration.ToString()}</TD>");
                    writer.WriteLine($"<TD><A HREF=\"{report.ReportPath.Replace(_rootFolder, ".")}\" target=_blank>Link</a></TD>");
                    writer.WriteLine("</TR>");
                }

                writer.WriteLine("</TABLE></BODY></HTML>");
                writer.Flush();
            }

            return summaryHTML;
        }

    }
}
