using HP.GFriend.Keywords;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace HP.GFriend.Core.Execution
{
    public static class Reporter
    {
        private static StreamWriter _writer = null;
        private static string _fileName = null;
        private static readonly string CONTENTS = "$CONTENTS$";
        private static string _path;
        private static string _reportSummary;
        public static void InitReport(string path, string outputFileName = null)
        {
            _path = path;
            if(string.IsNullOrEmpty(outputFileName))
            {
                outputFileName = "output.xml";
            }
            _fileName = Path.Combine(path, outputFileName);
            if(File.Exists(_fileName))
            {
                File.Delete(_fileName);
            }
            _writer = new StreamWriter(_fileName, true, Encoding.UTF8);
            WriteToOutput("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        }
        public static void InitReport(string path, string filename,bool mergecondition)
        {
            if (mergecondition)
            {
                _path = path;
                string outputFileName = filename;
                _fileName = Path.Combine(path, outputFileName);
            }
        }

        public static void EndReport()
        {
            _writer.Flush();
            _writer.Close();
            Console.WriteLine("Log File : " + Path.Combine(_path, "log.txt"));
            Console.WriteLine("Output File : " + _fileName);
        }

        public static void WriteToOutput(string text)
        {
            _writer.WriteLine(text);
            _writer.Flush();
        }

        public static void WriteToOutput(string tag, Dictionary<string,string> attributes, bool singleElemet)
        {
            string text;
            string attrStr;

            attrStr = "";
            foreach(KeyValuePair<string,string>attribute in attributes)
            {
                string tmpStr;
                tmpStr = attribute.Value.Replace("&", "&amp;");
                tmpStr = tmpStr.Replace("<", "&lt;");
                tmpStr = tmpStr.Replace(">", "&gt;");
                tmpStr = tmpStr.Replace("\"", "&quot;");
                tmpStr = tmpStr.Replace("'", "&apos;");
                tmpStr = attribute.Key + "=\"" + tmpStr + "\"";
                attrStr = string.Join(" ", attrStr, tmpStr);
            }

            if(singleElemet)
            {
                text = "<" + tag + " " + attrStr + "/>";
            }
            else
            {
                text = "<" + tag + " " + attrStr + ">";
            }

            WriteToOutput(text);
        }

        public static void WriteToOutput(string tag, Dictionary<string, string> attributes, string value)
        {
            string text;
            string attrStr;
            attrStr = "";
            foreach (KeyValuePair<string, string> attribute in attributes)
            {
                string tmpStr;
                tmpStr = attribute.Value.Replace("&", "&amp;");
                tmpStr = tmpStr.Replace("<", "&lt;");
                tmpStr = tmpStr.Replace(">", "&gt;");
                tmpStr = tmpStr.Replace("\"", "&quot;");
                tmpStr = tmpStr.Replace("'", "&apos;");
                tmpStr = attribute.Key + "=\"" + tmpStr + "\"";
                attrStr = string.Join(" ", attrStr, tmpStr);
            }

            text = "<" + tag + " " + attrStr + ">";
            WriteToOutput(text);
            value = value.Replace("&", "&amp;");
            value = value.Replace("<", "&lt;");
            value = value.Replace(">", "&gt;");
            value = value.Replace("\"", "&quot;");
            value = value.Replace("'", "&apos;");
            WriteToOutput(value);
            WriteToOutput("</" + tag + ">");
        }

        public static void WriteToOutput(string tag, string value)
        {
            value = value.Replace("&", "&amp;");
            value = value.Replace("<", "&lt;");
            value = value.Replace(">", "&gt;");
            value = value.Replace("\"", "&quot;");
            value = value.Replace("'", "&apos;");

            WriteToOutput("<" + tag + ">");
            WriteToOutput(value);
            WriteToOutput("</" + tag + ">");
        }

        public static void WriteToOutput(string tag, bool open)
        {
            if(open)
            {
                WriteToOutput("<" + tag + ">");
            }
            else // Close
            {
                WriteToOutput("</" + tag + ">");
            }
        }

        private static string RemoveInvalidXmlChars(string text)
        {
            var validXmlChars = text.Where(ch => XmlConvert.IsXmlChar(ch)).ToArray();
            return new string(validXmlChars);
        }
        public static void GenerateSessionReport(DateTime? startTime, DateTime? endTime, TimeSpan? elapsedTime, String SessionId, List<string> ActivityIds)
        {
            string htmlContents;
            string outputHTML;

            outputHTML = Path.Combine(Path.GetDirectoryName(_fileName), $"report_{SessionId}.html");
            using (Stream strm = Assembly.GetExecutingAssembly().GetManifestResourceStream("HP.GFriend.Core.Execution.Report.html"))
            using (StreamReader streamReader = new StreamReader(strm))
            {
                htmlContents = streamReader.ReadToEnd();

                string body = string.Empty;
                XmlDocument outputXML = new XmlDocument();
                using (StreamReader xmlStreamReader = new StreamReader(_fileName))
                {
                    string xmlContents = xmlStreamReader.ReadToEnd();
                    outputXML.LoadXml(RemoveInvalidXmlChars(xmlContents));
                }

                // Merge remote report.
                XmlNodeList remoteNodes = outputXML.SelectNodes("//Remote");
                foreach (XmlNode remoteNode in remoteNodes)
                {
                    string blockId = remoteNode.Attributes["BlockID"].Value;
                    string remoteXML = Path.Combine(Path.GetDirectoryName(_fileName), $"output_{blockId}.xml");
                    if (File.Exists(remoteXML))
                    {
                        XmlDocument remoteXMLDoc = new XmlDocument();
                        using (StreamReader xmlStreamReader = new StreamReader(remoteXML))
                        {
                            string xmlContents = xmlStreamReader.ReadToEnd();
                            remoteXMLDoc.LoadXml(RemoveInvalidXmlChars(xmlContents));
                        }
                        XmlNode remoteResultNode = remoteXMLDoc.SelectSingleNode("//Remote");
                        foreach (XmlNode child in remoteResultNode.ChildNodes)
                        {
                            remoteNode.AppendChild(outputXML.ImportNode(child, true));
                        }
                    }
                }
                outputXML.Save(_fileName);


                XmlNode testSuite = outputXML.SelectSingleNode("//TestSuite");

                // Title and Summary
                body += $"<h2>Session :: {SessionId}</h2><br><br>";
                body += $@"<table class ='table table-borderd' width ='100%'>
                                <tr class='info'>
                                  <td>
                                    <h4>
                                      <b>Summary</b>
                                    </h4><br>
                                    Session Start Time : {startTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty}<br>
                                    Session End Time : {endTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty}<br>
                                    Session Elapsed Time : {elapsedTime?.ToString(@"hh\:mm\:ss") ?? string.Empty}<br><br>
                                    <table class ='table table-borderd'>
                                        <tr class='active' style='width: 210px'>
                                         <th style='width: 70px' id='TOTAL'><u>Total</u> <img src='{Directory.GetCurrentDirectory()}\res\Filter.png' alt=''></th>
                                         <th style='width: 70px' id='PASS'><u>Pass</u><img src='{Directory.GetCurrentDirectory()}\res\Filter.png' alt=''></th>
                                        <th style='width: 70px' id='FAIL'><u>Fail</u><img src='{Directory.GetCurrentDirectory()}\res\Filter.png' alt=''></th>
                                        <th style='width: 70px'id='ERROR'><u>Error</u><img src='{Directory.GetCurrentDirectory()}\res\Filter.png' alt=''></th>
                                        </tr>
                                        <tr style='color: green;'>To see only pass or fail or error test cases click on the filter symbols</tr>

                                      <tr>
                                        <td class ='warning'>{testSuite.SelectNodes("//TestCase")?.Count ?? 0}</td>
                                        <td class ='success'>{testSuite.SelectNodes("//TestCase/Result[contains(text(),'PASS')]")?.Count ?? 0}</td>
                                        <td class ='danger'>{testSuite.SelectNodes("//TestCase/Result[contains(text(),'FAIL')]")?.Count ?? 0}</td>
                                        <td class ='error'>{testSuite.SelectNodes("//TestCase/Result[contains(text(),'ERROR')]")?.Count ?? 0}</td>
                                      </tr>
            
                                  </table>
                                  </td>
                                </tr>
                             </table><br>";

               
                // Warnings
                if (testSuite.SelectNodes("//Deprecated").Count > 0)
                {
                    List<string> deprecatedstatements = new List<string>();
                    foreach (XmlNode deprecated in testSuite.SelectNodes("//Deprecated"))
                    {

                        string statement = "";
                        foreach (XmlAttribute attributes in deprecated.ParentNode.Attributes)
                        {
                            if (attributes.Name == "StatementName")
                            {
                                statement = deprecated.ParentNode.Attributes["StatementName"].Value + ":" + deprecated.InnerText;
                            }
                        }
                        if (!String.IsNullOrEmpty(statement))
                        {
                            bool IsDeprecated = LibraryUtils.IsKeywordDeprecated(statement.Trim());
                            if (IsDeprecated)
                            {
                                deprecatedstatements.Add(statement);
                            }
                        }

                    }

                    if (deprecatedstatements.Count > 0)
                    {
                        body += "<table class ='table table-borderd' width ='100%'><tr class='danger'><td class='danger'><font class='FAIL'>";
                        body += "<b>Warning : Deprecated Keywords</b><br>";
                        foreach (string deprecatedstatement in deprecatedstatements)
                        {
                            body += deprecatedstatement + "<br>";
                        }
                    }

                    body += "</font></td></tr></table><br>";
                }
                body += "<h3>Test Case Results</h3>";
                // Write Test case resultsint
                foreach (string activityId in ActivityIds)
                {
                    XmlNodeList testCasesForActivityId = testSuite.SelectNodes($"//TestCase[ActivityId='{activityId}']");
                    if (testCasesForActivityId.Count > 0)
                    {
                        string activityResult = "PASS";
                        foreach (XmlNode tc in testCasesForActivityId)
                        {
                            if (tc.SelectSingleNode("Result")?.InnerText.Trim() == "PASS")
                            {
                                continue;
                            }
                            else if (tc.SelectSingleNode("Result")?.InnerText.Trim() == "FAIL")
                            {
                                if (activityResult.Contains("ERROR"))
                                {
                                    activityResult = "PASS FAIL ERROR";
                                    continue;
                                }
                                else if (activityResult.Contains("PASS"))
                                {
                                    activityResult = "PASS FAIL";
                                    continue;
                                }
                                else
                                {
                                    activityResult = "FAIL";
                                }
                            }
                            else if (tc.SelectSingleNode("Result")?.InnerText.Trim() == "ERROR")
                            {
                                if (activityResult.Contains("FAIL"))
                                {
                                    activityResult = "PASS FAIL ERROR";
                                    continue;
                                }
                                else if (activityResult.Contains("PASS"))
                                {
                                    activityResult = "PASS ERROR";
                                    continue;
                                }
                                else
                                {
                                    activityResult = "ERROR";
                                }
                            }
                        }

                        body += $"<h4 class='{activityResult}' style='color:black'>Activity ID: {activityId}</h4>";
                        foreach (XmlNode tc in testCasesForActivityId)
                        {
                            startTime = Convert.ToDateTime(tc.SelectSingleNode("StartTime")?.InnerText.Trim());
                            endTime = Convert.ToDateTime(tc.SelectSingleNode("EndTime")?.InnerText.Trim());
                            elapsedTime = null;
                            if (startTime.HasValue && endTime.HasValue)
                            {
                                elapsedTime = endTime.Value - startTime.Value;
                            }

                            Stack<string> tagToClose = new Stack<string>();
                            body += $"<dl class='accordion {tc.SelectSingleNode("Result")?.InnerText.Trim() ?? string.Empty}' >";
                            tagToClose.Push("</dl>");
                            body += $@"<dt>
                        <a href=''>
                           <font class='{tc.SelectSingleNode("Result")?.InnerText.Trim() ?? string.Empty}'>{tc.Attributes["Name"]?.Value ?? string.Empty}</font>
                        </a>
                        <br>
                      </dt>
                      <dd>
                        Start Time : {tc.SelectSingleNode("StartTime")?.InnerText.Trim() ?? string.Empty} | End Time : {tc.SelectSingleNode("EndTime")?.InnerText.Trim() ?? string.Empty} |  Elapsed Time : {elapsedTime}";
                            tagToClose.Push("</dd>");
                            foreach (XmlNode runnable in tc.SelectNodes("Repeat|Block|If|While|ForEachRow|Statement|Remote|For|ForSelectedRow|SpreadSheetForEachRow"))
                            {
                                body += XMLNodetoHTML(runnable);
                            }
                            while (tagToClose.Count > 0)
                            {
                                string tag = tagToClose.Pop();
                                body += tag;
                            }
                        }
                    }
                }

                htmlContents = htmlContents.Replace(CONTENTS, body);
            }

            // Write report.html
            using (StreamWriter streamWriter = new StreamWriter(outputHTML))
            {
                streamWriter.Write(htmlContents);
                streamWriter.Flush();
            }


            Console.WriteLine("Report File : " + outputHTML);
        }

        public static void GenerateReport()
        {
            string htmlContents;
            string outputHTML;

            outputHTML = Path.Combine(Path.GetDirectoryName(_fileName), "report.html");
            using (Stream strm = Assembly.GetExecutingAssembly().GetManifestResourceStream("HP.GFriend.Core.Execution.Report.html"))
            using(StreamReader streamReader = new StreamReader(strm))
            {
                htmlContents = streamReader.ReadToEnd();

                string body = string.Empty;
                XmlDocument outputXML = new XmlDocument();
                using (StreamReader xmlStreamReader = new StreamReader(_fileName))
                {
                    string xmlContents = xmlStreamReader.ReadToEnd();
                    outputXML.LoadXml(RemoveInvalidXmlChars(xmlContents));
                }

                // Merge remote report.
                XmlNodeList remoteNodes = outputXML.SelectNodes("//Remote");
                foreach(XmlNode remoteNode in remoteNodes)
                {
                    string blockId = remoteNode.Attributes["BlockID"].Value;
                    string remoteXML = Path.Combine(Path.GetDirectoryName(_fileName), $"output_{blockId}.xml");
                    if(File.Exists(remoteXML))
                    {
                        XmlDocument remoteXMLDoc = new XmlDocument();
                        using (StreamReader xmlStreamReader = new StreamReader(remoteXML))
                        {
                            string xmlContents = xmlStreamReader.ReadToEnd();
                            remoteXMLDoc.LoadXml(RemoveInvalidXmlChars(xmlContents));
                        }
                        XmlNode remoteResultNode = remoteXMLDoc.SelectSingleNode("//Remote");
                        foreach(XmlNode child in remoteResultNode.ChildNodes)
                        {
                            remoteNode.AppendChild(outputXML.ImportNode(child, true));
                        }
                    }
                }
                outputXML.Save(_fileName);


                XmlNode testSuite = outputXML.SelectSingleNode("//TestSuite");
                DateTime? startTime = Convert.ToDateTime(testSuite.SelectSingleNode("StartTime")?.InnerText.Trim());
                DateTime? endTime = Convert.ToDateTime(testSuite.SelectSingleNode("EndTime")?.InnerText.Trim());
                TimeSpan? elapsedTime = null;
                if (startTime.HasValue && endTime.HasValue)
                {
                    elapsedTime = endTime.Value - startTime.Value;
                }

                // Title and Summary
                body += $"<h2>Test Suite :: {testSuite.Attributes["Name"]?.Value ?? string.Empty}</h2><br><br>";
                body += $@"<table class='table table-bordered' width='100%'>
                <tr class='info'>
                    <td>
                        <h4><b>Summary</b></h4><br>
                        Test Start Time : {testSuite.SelectSingleNode("/TestSuite/StartTime")?.InnerText.Trim() ?? string.Empty}<br>
                        Test End Time : {testSuite.SelectSingleNode("/TestSuite/EndTime")?.InnerText.Trim() ?? string.Empty}<br>
                        Test Elapsed Time : {elapsedTime}<br><br>
                        <table class='table table-bordered'>
                            <tr class='active' style='width: 210px'>
                                <th style='width: 70px' id='TOTAL'><u>Total</u> <img src='{Directory.GetCurrentDirectory()}\\res\\Filter.png' alt=''></th>
                                <th style='width: 70px' id='PASS'><u>Pass</u><img src='{Directory.GetCurrentDirectory()}\\res\\Filter.png' alt=''></th>
                                <th style='width: 70px' id='FAIL'><u>Fail</u><img src='{Directory.GetCurrentDirectory()}\\res\\Filter.png' alt=''></th>
                                <th style='width: 70px' id='ERROR'><u>Error</u><img src='{Directory.GetCurrentDirectory()}\\res\\Filter.png' alt=''></th>
                            </tr>
                            <tr style='color: green;'>To see only pass or fail or error test cases click on the filter symbols</tr>
                            <tr>
                                <td class='warning'>{testSuite.SelectNodes("//TestCase")?.Count ?? 0}</td>
                                <td class='success'>{testSuite.SelectNodes("//TestCase/Result[contains(text(),'PASS')]")?.Count ?? 0}</td>
                                <td class='danger'>{testSuite.SelectNodes("//TestCase/Result[contains(text(),'FAIL')]")?.Count ?? 0}</td>
                                <td class='error'>{testSuite.SelectNodes("//TestCase/Result[contains(text(),'ERROR')]")?.Count ?? 0}</td>
                            </tr>
                        </table>
                    </td>";


                _reportSummary = $@"<table class ='table table-borderd' width ='100%'>
                                <tr class='info'>
                                  <td>
                                    <h4>
                                      <b>Summary</b>
                                    </h4><br>
                                    Session Start Time : {startTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty}<br>
                                    Session End Time : {endTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty}<br>
                                    Session Elapsed Time : {elapsedTime?.ToString(@"hh\:mm\:ss") ?? string.Empty}<br><br>
                                    <table class ='table table-borderd'>
                                        <tr class='active' style='width: 210px'>
                                         <th style='width: 70px' id='TOTAL'><u>Total</u></th>
                                         <th style='width: 70px' id='PASS'><u>Pass</u></th>
                                        <th style='width: 70px' id='FAIL'><u>Fail</u></th>
                                        <th style='width: 70px'id='ERROR'><u>Error</u></th>
                                        </tr>
                                      <tr>
                                        <td class ='warning'>{testSuite.SelectNodes("//TestCase")?.Count ?? 0}</td>
                                        <td class ='success'>{testSuite.SelectNodes("//TestCase/Result[contains(text(),'PASS')]")?.Count ?? 0}</td>
                                        <td class ='danger'>{testSuite.SelectNodes("//TestCase/Result[contains(text(),'FAIL')]")?.Count ?? 0}</td>
                                        <td class ='error'>{testSuite.SelectNodes("//TestCase/Result[contains(text(),'ERROR')]")?.Count ?? 0}</td>
                                      </tr>
            
                                  </table>
                                  </td>
                                </tr>
                             </table><br>";
                CommonExecutionInfo.SetVariable("ReportSummary", _reportSummary);


                //<!-- Add System and Printer Information beside the summary -->
                if (BuiltInLibrary._enableSystemandPrinterInformation==true)
                {
                    // Execute system information methods and capture their outputs
                    string cpuInfo = "";
                    string ramInfo = "";
                    string windowsInfo = "";
                    string printerModelNumber = "";
                    string printerModelName = "";
                    string printerFirmwareVersion = "";
                    string printerDriverDetails = "";
                    string printerInfo = "";
                    string printerFirmwareDateCode = "";
                    string firmwareBundleVersion = "";
                    string bundleVersionInfo = "";


                    // execute the methods to get the system information and store them in variables.
                    KeywordResult cpuResult = new BuiltInLibrary().GetSystemInformation("cpuInfo");
                    if (cpuResult.Result == KeywordResults.Pass)
                    {
                        cpuInfo = cpuResult.Output;
                    }

                    KeywordResult ramResult = new BuiltInLibrary().GetRam("ramInfo");
                    if (ramResult.Result == KeywordResults.Pass)
                    {
                        ramInfo = ramResult.Output;
                    }

                    KeywordResult windowsResult = new BuiltInLibrary().GetWindowsVersion("windowsInfo");
                    if (windowsResult.Result == KeywordResults.Pass)
                    {
                        windowsInfo = windowsResult.Output;
                    }

                    KeywordResult printerResult = new BuiltInLibrary().GetPrinterDetails(CommonExecutionInfo.GetVariable(SystemVariables.DUT_ADDRESS), CommonExecutionInfo.GetVariable(SystemVariables.DUT_ADMINPW));
                    if (printerResult.Result == KeywordResults.Pass)
                    {
                        printerInfo = printerResult.Output;

                        // Extract information from printerInfo string using string manipulation or regular expressions
                        printerModelName = ExtractValue(printerInfo, "ModelName");
                        printerDriverDetails = ExtractValue(printerInfo, "Driver Info");
                        printerFirmwareVersion = ExtractValue(printerInfo, "Firmware");
                        printerModelNumber = ExtractValue(printerInfo, "ModelNumber");
                        printerFirmwareDateCode = ExtractValue(printerInfo, "Firmware Datecode");
                    }
                    KeywordResult bundleVersion = new BuiltInLibrary().GetFirmwareBundleVersion(CommonExecutionInfo.GetVariable(SystemVariables.DUT_ADDRESS));                    
                    if (bundleVersion.Result == KeywordResults.Pass)
                    {
                        bundleVersionInfo = bundleVersion.Output;
                        firmwareBundleVersion = ExtractValue(bundleVersionInfo, "Firmware Bundle Version");
                    }
                    string gfriendVersion = Executor.GetVersion();


                    body += $@"<td><br><br> <br><br> <br><br> <br><br> <h4><b>System Information</b></h4><p><b>CPU: </b>{cpuInfo}<br><b>RAM: </b>{ramInfo}<br><b>Windows Version: </b>{windowsInfo} <br><b>GFriend Version: </b>{gfriendVersion} </p><br></td><td><br><br><br><br><br><br><br><br> " +
                       $@"<h4><b>Printer Information</b></h4><p><b>Model Number: </b>{printerModelNumber}<br><b>Firmware Bundle Version: </b>{firmwareBundleVersion}<br><b>Model Name: </b>{printerModelName}<br><b>Firmware DateCode: </b>{printerFirmwareDateCode}<br><b>Firmware Version: </b>{printerFirmwareVersion}<br><b>Driver Details: </b>{printerDriverDetails}</p><br></td>";

                   BuiltInLibrary._enableSystemandPrinterInformation = false; //setting back to default
}
                body += $@"</tr></table><br>";


                // Warnings
                if (testSuite.SelectNodes("//Deprecated").Count > 0)
                {
                    List<string> deprecatedstatements = new List<string>();
                    foreach (XmlNode deprecated in testSuite.SelectNodes("//Deprecated"))
                    {

                        string statement = "";
                        foreach (XmlAttribute attributes in deprecated.ParentNode.Attributes)
                        {
                            if (attributes.Name == "StatementName")
                            {
                                statement = deprecated.ParentNode.Attributes["StatementName"].Value + ":" + deprecated.InnerText;
                            }
                        }
                        if (!String.IsNullOrEmpty(statement))
                        {
                            bool IsDeprecated = LibraryUtils.IsKeywordDeprecated(statement.Trim());
                            if (IsDeprecated)
                            {
                                deprecatedstatements.Add(statement);
                            }
                        }

                    }

                    if (deprecatedstatements.Count > 0)
                    {
                        body += "<table class ='table table-borderd' width ='100%'><tr class='danger'><td class='danger'><font class='FAIL'>";
                        body += "<b>Warning : Deprecated Keywords</b><br>";
                        foreach (string deprecatedstatement in deprecatedstatements)
                        {
                            body += deprecatedstatement + "<br>";
                        }
                    }

                    body += "</font></td></tr></table><br>";
                }

                body += "<h3>Test Case Results</h3>";
                // Write Test case results
                foreach(XmlNode tc in testSuite.SelectNodes("//TestCase"))
                {
                    startTime = Convert.ToDateTime(tc.SelectSingleNode("StartTime")?.InnerText.Trim());
                    endTime = Convert.ToDateTime(tc.SelectSingleNode("EndTime")?.InnerText.Trim());
                    elapsedTime = null;
                    if (startTime.HasValue && endTime.HasValue)
                    {
                        elapsedTime = endTime.Value - startTime.Value;
                    }
                    Stack<string> tagToClose = new Stack<string>();
                    body += $"<dl class='accordion {tc.SelectSingleNode("Result")?.InnerText.Trim() ?? string.Empty}' >";
                    tagToClose.Push("</dl>");
                    body += $@"<dt>
                        <a href=''>
                            Test Case :: <font class='{tc.SelectSingleNode("Result")?.InnerText.Trim() ?? string.Empty}'>{tc.Attributes["Name"]?.Value ?? string.Empty}</font>
                        </a>
                        <br>
                      </dt>
                      <dd>
                        Start Time : {tc.SelectSingleNode("StartTime")?.InnerText.Trim()??string.Empty} | End Time : {tc.SelectSingleNode("EndTime")?.InnerText.Trim() ?? string.Empty} | Elapsed Time : {elapsedTime}";
                    tagToClose.Push("</dd>");

                    foreach(XmlNode runnable in tc.SelectNodes("Repeat|Block|If|While|ForEachRow|Statement|Remote|For|ForSelectedRow|SpreadSheetForEachRow"))
                    {
                        body += XMLNodetoHTML(runnable);
                    }

                    while (tagToClose.Count >0)
                    {
                        string tag = tagToClose.Pop();
                        body += tag;
                    }
                }

                htmlContents = htmlContents.Replace(CONTENTS, body);
            }

            // Write report.html
            using (StreamWriter streamWriter = new StreamWriter(outputHTML))
            {
                streamWriter.Write(htmlContents);
                streamWriter.Flush();
            }


            Console.WriteLine("Report File : " + outputHTML);
        }

        private static string XMLNodetoHTML(XmlNode node)
        {
            Stack<string> tagToClose = new Stack<string>();
            string html = string.Empty;
            string alwaysPassIndicator = string.Empty;
            DateTime? startTime = null;
            DateTime? endTime = null;
            TimeSpan? elapsedTime = null;

            // Extract start and end times
            if (node.SelectSingleNode("StartTime") != null)
            {
                startTime = Convert.ToDateTime(node.SelectSingleNode("StartTime")?.InnerText.Trim());
            }
            if (node.SelectSingleNode("EndTime") != null)
            {
                endTime = Convert.ToDateTime(node.SelectSingleNode("EndTime")?.InnerText.Trim());
            }
            // Calculate elapsed time
            if (startTime.HasValue && endTime.HasValue)
            {
                elapsedTime = endTime.Value - startTime.Value;
            }

            if (node.SelectNodes("AlwaysPass")?.Count > 0)
            {
                alwaysPassIndicator = "@";
            }
            switch (node.Name)
            {
                case "Block":
                    break;
                case "Repeat":
                    if(!node.Attributes["RepeatCount"]?.Value.Equals("1") ?? false)
                    {
                        html = $@"<dl class ='accordion {node.SelectSingleNode("Result")?.InnerText.Trim().ToUpper() ?? string.Empty}'>
                                <dt>
                                  <a href=''>
                                    <font class='{node.SelectSingleNode("Result")?.InnerText.Trim() ?? string.Empty}'>
                                      {alwaysPassIndicator}Repeat : {node.Attributes["RepeatCount"]?.Value ?? string.Empty} - 
                                        Pass({node.SelectNodes("Loop/Result[contains(text(),'PASS')]")?.Count ?? 0}) 
                                        Fail({node.SelectNodes("Loop/Result[contains(text(),'FAIL')]")?.Count ?? 0}) 
                                        Error({node.SelectNodes("Loop/Result[contains(text(),'ERROR')]")?.Count ?? 0}) 
                                    </font>
                                  </a>
                                </dt>
                                <dd>
                                  Start Time : {node.SelectSingleNode("StartTime")?.InnerText.Trim() ?? string.Empty} | End Time :{node.SelectSingleNode("EndTime")?.InnerText.Trim() ?? string.Empty}| Elapsed Time: {elapsedTime?.ToString() ?? string.Empty}<br>";
                        tagToClose.Push("</dl>");
                        tagToClose.Push("</dd>");
                    }
                    break;
                case "Loop":
                    if (node.ParentNode.SelectNodes("Loop").Count > 1)
                    {
                        html = $@"<dl class ='accordion {node.SelectSingleNode("Result")?.InnerText.Trim().ToUpper() ?? string.Empty}'>
                                <dt>
                                  <a href=''>
                                    <font class='{node.SelectSingleNode("Result")?.InnerText.Trim() ?? string.Empty}'>
                                      Loop : {node.Attributes["LoopCount"]?.Value ?? string.Empty}
                                        {node.Attributes["DataSet"]?.Value ?? string.Empty}
                                    </font>
                                  </a>
                                </dt>
                                <dd>
                                  Start Time : {node.SelectSingleNode("StartTime")?.InnerText.Trim() ?? string.Empty} | End Time :{node.SelectSingleNode("EndTime")?.InnerText.Trim() ?? string.Empty}| Elapsed Time: {elapsedTime?.ToString() ?? string.Empty}<br>";
                        tagToClose.Push("</dl>");
                        tagToClose.Push("</dd>");
                    }
                    break;

                case "While":
                    html = $@"<dl class ='accordion {node.SelectSingleNode("Result")?.InnerText.Trim().ToUpper() ?? string.Empty}'>
                                <dt>
                                  <a href=''>
                                    <font class='{node.SelectSingleNode("Result")?.InnerText.Trim() ?? string.Empty}'>
                                      {alwaysPassIndicator}{node.Attributes["Condition"]?.Value ?? string.Empty}
                                    </font>
                                  </a>
                                </dt>
                                <dd>
                                  Start Time : {node.SelectSingleNode("StartTime")?.InnerText.Trim() ?? string.Empty} | End Time :{node.SelectSingleNode("EndTime")?.InnerText.Trim() ?? string.Empty}| Elapsed Time: {elapsedTime?.ToString() ?? string.Empty}<br>";
                    tagToClose.Push("</dl>");
                    tagToClose.Push("</dd>");
                    break;

                case "For":
                    html = $@"<dl class ='accordion {node.SelectSingleNode("Result")?.InnerText.Trim().ToUpper() ?? string.Empty}'>
                                <dt>
                                  <a href=''>
                                    <font class='{node.SelectSingleNode("Result")?.InnerText.Trim() ?? string.Empty}'>
                                      {alwaysPassIndicator}For : {node.Attributes["Listvalue"]?.Value ?? string.Empty}
                                    </font>
                                  </a>
                                </dt>
                                <dd>
                                  Start Time : {node.SelectSingleNode("StartTime")?.InnerText.Trim() ?? string.Empty} | End Time :{node.SelectSingleNode("EndTime")?.InnerText.Trim() ?? string.Empty}| Elapsed Time: {elapsedTime?.ToString() ?? string.Empty}<br>";
                    tagToClose.Push("</dl>");
                    tagToClose.Push("</dd>");
                    break;

                case "ForEachRow":
                    html = $@"<dl class ='accordion {node.SelectSingleNode("Result")?.InnerText.Trim().ToUpper() ?? string.Empty}'>
                                <dt>
                                  <a href=''>
                                    <font class='{node.SelectSingleNode("Result")?.InnerText.Trim() ?? string.Empty}'>
                                      For Each Row:{node.Attributes["TableName"]?.Value ?? string.Empty}
                                    </font>
                                  </a>
                                </dt>
                                <dd>
                                  Start Time : {node.SelectSingleNode("StartTime")?.InnerText.Trim() ?? string.Empty} | End Time :{node.SelectSingleNode("EndTime")?.InnerText.Trim() ?? string.Empty}| Elapsed Time: {elapsedTime?.ToString() ?? string.Empty}<br>";
                    tagToClose.Push("</dl>");
                    tagToClose.Push("</dd>");
                    break;
                case "SpreadSheetForEachRow":
                    html = $@"<dl class ='accordion {node.SelectSingleNode("Result")?.InnerText.Trim().ToUpper() ?? string.Empty}'>
                                <dt>
                                  <a href=''>
                                    <font class='{node.SelectSingleNode("Result")?.InnerText.Trim() ?? string.Empty}'>
                                      Spread Sheet For Each Row:{node.Attributes["TableName"]?.Value ?? string.Empty}
                                    </font>
                                  </a>
                                </dt>
                                <dd>
                                  Start Time : {node.SelectSingleNode("StartTime")?.InnerText.Trim() ?? string.Empty} | End Time :{node.SelectSingleNode("EndTime")?.InnerText.Trim() ?? string.Empty}| Elapsed Time: {elapsedTime?.ToString() ?? string.Empty}<br>";
                    tagToClose.Push("</dl>");
                    tagToClose.Push("</dd>");
                    break;

                case "ForSelectedRow":
                    html = $@"<dl class ='accordion {node.SelectSingleNode("Result")?.InnerText.Trim().ToUpper() ?? string.Empty}'>
                                <dt>
                                  <a href=''>
                                    <font class='{node.SelectSingleNode("Result")?.InnerText.Trim() ?? string.Empty}'>
                                      For Selected Row:{node.Attributes["TableName"]?.Value ?? string.Empty}
                                    </font>
                                  </a>
                                </dt>
                                <dd>
                                  Start Time : {node.SelectSingleNode("StartTime")?.InnerText.Trim() ?? string.Empty} | End Time :{node.SelectSingleNode("EndTime")?.InnerText.Trim() ?? string.Empty}| Elapsed Time: {elapsedTime?.ToString() ?? string.Empty}<br>";
                    tagToClose.Push("</dl>");
                    tagToClose.Push("</dd>");
                    break;

                case "If":
                    html = $@"<dl class ='accordion {node.SelectSingleNode("Result")?.InnerText.Trim().ToUpper() ?? string.Empty}'>
                                <dt>
                                  <a href=''>
                                    <font class='{node.SelectSingleNode("Result")?.InnerText.Trim() ?? string.Empty}'>
                                      {alwaysPassIndicator}If:{node.Attributes["Condition"]?.Value ?? string.Empty}
                                    </font>
                                  </a>
                                </dt>
                                <dd>
                                  Start Time : {node.SelectSingleNode("StartTime")?.InnerText.Trim() ?? string.Empty} | End Time :{node.SelectSingleNode("EndTime")?.InnerText.Trim() ?? string.Empty}| Elapsed Time: {elapsedTime?.ToString() ?? string.Empty}<br>";
                    tagToClose.Push("</dl>");
                    tagToClose.Push("</dd>");
                    break;
                case "StatementBlock":
                    html = $@"<dl class ='accordion {node.SelectSingleNode("Result")?.InnerText.Trim().ToUpper() ?? string.Empty}'>
                                <dt>
                                  <a href=''>
                                    <font class='{node.SelectSingleNode("Result")?.InnerText.Trim() ?? string.Empty}'>
                                      Executed Condition : {node.Attributes["Kind"]?.Value ?? string.Empty}
                                    </font>
                                  </a>
                                </dt>
                                <dd>
                                  Start Time : {node.SelectSingleNode("StartTime")?.InnerText.Trim() ?? string.Empty} | End Time :{node.SelectSingleNode("EndTime")?.InnerText.Trim() ?? string.Empty}| Elapsed Time: {elapsedTime?.ToString() ?? string.Empty}<br>";
                    tagToClose.Push("</dl>");
                    tagToClose.Push("</dd>");
                    break;

                case "Remote":
                    html = $@"<dl class ='accordion {node.SelectSingleNode("Result")?.InnerText.Trim().ToUpper() ?? string.Empty}'>
                                <dt>
                                  <a href=''>
                                    <font class='{node.SelectSingleNode("Result")?.InnerText.Trim() ?? string.Empty}'>
                                      {alwaysPassIndicator}Remote Async Run:{node.Attributes["RemoteID"]?.Value ?? string.Empty}
                                    </font>
                                  </a>
                                </dt>
                                <dd>
                                  Start Time : {node.SelectSingleNode("StartTime")?.InnerText.Trim() ?? string.Empty} | End Time :{node.SelectSingleNode("EndTime")?.InnerText.Trim() ?? string.Empty}| Elapsed Time: {elapsedTime?.ToString() ?? string.Empty}<br>";
                    tagToClose.Push("</dl>");
                    tagToClose.Push("</dd>");
                    break;

                case "Statement":
                    html = $@" <dl class ='accordion {node.SelectSingleNode("Result")?.InnerText.Trim().ToUpper() ?? string.Empty}'>
                                            <dt>
                                              <a href=''>
                                                <font class='{node.SelectSingleNode("Result")?.InnerText.Trim().ToUpper() ?? string.Empty}'>
                                                  {node.Attributes["StatementName"]?.Value.Trim() ?? string.Empty}
                                                </font>
                                              </a>
                                            </dt>
                                            <dd>
                                              Start Time : {node.SelectSingleNode("StartTime")?.InnerText.Trim() ?? string.Empty} | End Time :{node.SelectSingleNode("EndTime")?.InnerText.Trim() ?? string.Empty}| Elapsed Time: {elapsedTime?.ToString() ?? string.Empty}<br>";
                    tagToClose.Push("</dl>");
                    tagToClose.Push("</dd>");
                    break;
                case "ScreenShot":
                    html = $@"<img src ='{node.InnerText?.Trim() ?? string.Empty}'><br>";
                    break;
                case "Link":
                    html = $@"<a href ='{node.InnerText?.Trim() ?? string.Empty}'>{node.Attributes["DisplayName"]?.Value ?? "DataSet" }</a><br>";
                    break;
                case "Output":
                    html = $@"{node.InnerText?.Trim().Replace(Environment.NewLine, "<BR>") ?? string.Empty}<br>";
                    break;
                case "ErrorDescription":
                    html = $@"<font color='red'>{node.InnerText?.Trim().Replace(Environment.NewLine, "<BR>") ?? string.Empty}</font><br>";
                    break;
                case "AdditionalInfo":
                    html = $@"<dl class ='accordion'>
                                <dt><a href=''>Addtional Info</a></dt>
                                    <dd>{node.InnerText?.Trim().Replace(Environment.NewLine, "<BR>") ?? string.Empty}</dd>
                                </dl>";
                    break;
            }

            if(node.HasChildNodes)
            {
                foreach(XmlNode child in node.ChildNodes)
                {
                    html += XMLNodetoHTML(child);
                }
            }

            while (tagToClose.Count > 0)
            {
                string tag = tagToClose.Pop();
                html += tag;
            }

            return html;
        }

        public static void FixXmlOutput(string outputXml)
        {
            Stack<string> xmlTags = new Stack<string>();
            _fileName = outputXml;
            _path = Path.GetDirectoryName(outputXml);

            if(_writer != null)
            {
                _writer.Close();
            }

            // Step 1 : Read all tags and save un-closed tag to stack
            StreamReader reader = new StreamReader(_fileName);
            string line;
            string tagPattern = @"<(.|\n)*?>";
            Regex tagRegex = new Regex(tagPattern);

            while ((line = reader.ReadLine()) != null)
            {
                foreach(Match match in tagRegex.Matches(line))
                {
                    string tag = match.ToString();
                    if(!tag.EndsWith("/>") && !tag.StartsWith("<?"))
                    {
                        if(tag.StartsWith("</"))
                        {
                            if(xmlTags.Peek().Equals(GetTagName(tag)))
                            {
                                xmlTags.Pop();
                            }
                        }
                        else
                        {
                            xmlTags.Push(GetTagName(tag));
                        }
                    }
                }
            }

            reader.Close();

            // Step 2 : Close tags
            _writer = new StreamWriter(_fileName, true, Encoding.UTF8);
            string closeTag;
            while(xmlTags.Count > 0)
            {
                closeTag = xmlTags.Pop();
                closeTag = string.Format(@"</{0}>", closeTag);
                _writer.WriteLine(closeTag);
                _writer.Flush();
            }

            EndReport();
            GenerateReport();
        }

        public static string GetTagName(string tag)
        {
            string name;

            tag = tag.Replace("</", "<");
            tag = tag.Replace(">", "");
            name = tag.Split(' ')[0];
            name = name.Replace("<", "");
            return name;
        }
        // Static method to extract the value between labels and the next newline
        public static string ExtractValue(string input, string label)
        {
            int startIndex = input.IndexOf(label);
            if (startIndex == -1)
                return ""; // Return empty if the label is not found

            startIndex += label.Length + 2; // Move past the label and space after it
            int endIndex = input.IndexOf('\n', startIndex);
            if (endIndex == -1)
                endIndex = input.Length;

            return input.Substring(startIndex, endIndex - startIndex).Trim();
        }
    }
}
