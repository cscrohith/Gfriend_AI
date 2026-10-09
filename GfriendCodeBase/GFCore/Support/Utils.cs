using HP.GFriend.Core;
using HP.GFriend.Core.Custom;
using HP.GFriend.Core.Execution;
using HP.GFriend.Keywords;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static System.Net.WebRequestMethods;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Data.SqlTypes;
using HP.GFriend.GFLogger;
using System.Security.Cryptography;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace HP.GFriend.Support
{
    public static class Utils
    {
        private static List<string> usedPaths = new List<string>();
        private static List<string> validatedCustomPaths = new List<string>();
        private static string folderpath = "";
        private static List<string> variables = new List<string>();
        private static List<string> customLibraries = new List<string>();
        private static List<string> availableLibraries = new List<string>();
        private static Dictionary<string, int> customKeywords = new Dictionary<string, int>();
        private static int linedepth = 0;
        private static string testcasenameline = "";
        private static bool testcasevalidated = true;
        private static int totalTestCases = 0;
        private static int passTestCases = 0;
        private static int failTestCases = 0;
        private static int errorTestCases = 0;
        private static bool filecontainsAliasNameForLibrary = false;
        private static Dictionary<string, string> aliasLibrary = new Dictionary<string, string>();
        public static string GetTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        public static string WriteImageByteToFile(byte[] img)
        {
            return null;
        }

        public static void GetKeywordFromMethod(MethodInfo m, Keyword k)
        {
            try
            {


                k.KeywordName = ((KeywordDisplayName)(m.GetCustomAttributes().Where(a => a.GetType().FullName.Equals(typeof(KeywordDisplayName).FullName)).First())).DisplayName;
            }
            catch (Exception)
            {
                k.KeywordName = m.Name;
            }
            if (Attribute.IsDefined(m, typeof(SampleScript)))
            {
                try
                {
                    k.SampleScript = ((SampleScript)(m.GetCustomAttributes().Where(a => a.GetType().FullName.Equals(typeof(SampleScript).FullName)).First())).demoScript;
                }
                catch (Exception)
                {
                    k.SampleScript = string.Empty;
                }
            }

            string description;
            List<string> paramDescription = new List<string>();
            try
            {
                var desAttr = m.GetCustomAttributes().Where(a => a.GetType().FullName.Equals(typeof(KeywordDescription).FullName)).First();
                description = desAttr.ToString();
            }
            catch (Exception)
            {
                description = string.Empty;
            }
            try
            {
                foreach (var p in m.GetCustomAttributes().Where(a => a.GetType().FullName.Equals(typeof(KeywordParameters).FullName)))
                {
                    paramDescription.Add(p.ToString());
                }
            }
            catch (Exception) { }

            k.IsGetKeyword = Attribute.IsDefined(m, typeof(GetKeyword));
            k.Description = description;
            k.FunctionName = m.Name;
            k.NumOfArgs = m.GetParameters().Count();
            k.Method = m;
            k.Args = "";
            if (k.NumOfArgs > 0)
            {

                foreach (ParameterInfo p in m.GetParameters())
                {
                    k.Args += p.Name + ", ";
                }
                k.Args += "@@ZZ@@ZZ@@";
                k.Args = k.Args.Replace(", @@ZZ@@ZZ@@", "");
                //k.KeywordName += $" ({k.Args})";
                k.Args = string.Join("\r\n", paramDescription.ToArray());
            }
            else
            {
                k.Args = "NO Argument";
            }

            if (Attribute.IsDefined(m, typeof(Deprecated)))
            {
                k.Deprecated = true;
                var deprecatedAttr = m.GetCustomAttributes().Where(a => a.GetType().FullName.Equals(typeof(Deprecated).FullName)).First();
                k.Description = string.Join(Environment.NewLine, $"[Deprecated]{deprecatedAttr.ToString()}", k.Description);
                k.DeprecatedReason = deprecatedAttr.ToString();

            }
        }
        /// <summary>
        /// merge the output files of a folder into a single report file
        /// </summary>
        public static void MergeOutputFilesAndGenerateReport(string PreviousOutputFolderPath, string outputDir)
        {
            string[] pathparts = PreviousOutputFolderPath.Split(new char[] { '\\' });
            string SessionId = pathparts[pathparts.Length - 1];
            int outputFoldersCount = Directory.GetDirectories(PreviousOutputFolderPath).Length;
            Console.WriteLine("Total no. of directories in the path " + PreviousOutputFolderPath + " : " + outputFoldersCount);

            List<(string XmlFilePath, string ActivityId)> xmlFilesWithActivityIds = GetXmlFilesFromFolders(PreviousOutputFolderPath, outputFoldersCount);
            List<string> activityIds = xmlFilesWithActivityIds.Select(x => x.ActivityId).Distinct().ToList();
            DateTime? startTime, endTime;
            TimeSpan? elapsedTime;
            MergeXmlFiles(xmlFilesWithActivityIds, outputDir, SessionId, out startTime, out endTime, out elapsedTime);
            Console.WriteLine("Output File is generated in the path : " + outputDir);

            Reporter.InitReport(outputDir, "output_" + SessionId + ".xml", true);
            Reporter.GenerateSessionReport(startTime, endTime, elapsedTime, SessionId, activityIds);
        }

        private static List<(string XmlFilePath, string ActivityId)> GetXmlFilesFromFolders(string previousOutputFolderPath, int outputFoldersCount)
        {
            List<(string XmlFilePath, string ActivityId)> xmlFilesWithActivityIds = new List<(string, string)>();
            for (int i = 1; i <= outputFoldersCount; i++)
            {
                string folderPrefix = $"{i.ToString().PadLeft(6, '0')}_";
                var directories = Directory.EnumerateDirectories(previousOutputFolderPath, folderPrefix + "*", SearchOption.TopDirectoryOnly);

                foreach (var directory in directories)
                {
                    string[] pathparts = directory.Split(new char[] { '\\' });
                    string ActivityId = pathparts[pathparts.Length - 1];
                    string outputFolderPath = Path.Combine(directory, "output");

                    if (Directory.Exists(outputFolderPath))
                    {
                        var xmlFiles = Directory.GetFiles(outputFolderPath, "output.xml");
                        foreach (var xmlFile in xmlFiles)
                        {
                            xmlFilesWithActivityIds.Add((xmlFile, ActivityId));
                        }
                    }
                }
            }

            return xmlFilesWithActivityIds;
        }

        private static void MergeXmlFiles(List<(string XmlFilePath, string ActivityId)> xmlFilesWithActivityIds, string outputFolderPath, string SessionId, out DateTime? startTime, out DateTime? endTime, out TimeSpan? elapsedTime)
        {
            string _fileName = Path.Combine(outputFolderPath, "output_" + SessionId + ".xml");
            if (xmlFilesWithActivityIds.Count == 0)
            {
                throw new ArgumentException("At least one XML file must be provided for merging.");
            }

            XmlDocument mergedDoc = new XmlDocument();
            mergedDoc.LoadXml("<TestSuite></TestSuite>"); // Root element for merged data
            startTime = null;
            endTime = null;
            foreach (var (xmlFilePath, activityId) in xmlFilesWithActivityIds)
            {
                if (!IsFileOpen(xmlFilePath))
                {
                    try
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(xmlFilePath);

                        // Extract start and end times
                        var start = doc.SelectSingleNode("/TestSuite/StartTime")?.InnerText.Trim();
                        var end = doc.SelectSingleNode("/TestSuite/EndTime")?.InnerText.Trim();

                        if (DateTime.TryParse(start, out DateTime startTimeParsed))
                        {
                            if (startTime == null || startTimeParsed < startTime)
                            {
                                startTime = startTimeParsed;
                            }
                        }

                        if (DateTime.TryParse(end, out DateTime endTimeParsed))
                        {
                            if (endTime == null || endTimeParsed > endTime)
                            {
                                endTime = endTimeParsed;
                            }
                        }

                        // Merge suites and test cases
                        MergeXmlDocuments(mergedDoc, doc, activityId, Path.GetDirectoryName(xmlFilePath), outputFolderPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing {xmlFilePath}: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"Skipping {xmlFilePath} as it is already opened.");
                }
            }
            // Calculate elapsed time
            elapsedTime = null;
            if (startTime.HasValue && endTime.HasValue)
            {
                elapsedTime = endTime.Value - startTime.Value;
            }

            // Add elapsed time to the merged XML document
            if (elapsedTime.HasValue)
            {
                XmlElement elapsedTimeElement = mergedDoc.CreateElement("ElapsedTime");
                elapsedTimeElement.InnerText = elapsedTime.Value.ToString();
                mergedDoc.DocumentElement.AppendChild(elapsedTimeElement);
            }

            // Save merged XML to file
            if (System.IO.File.Exists(_fileName))
            {
                System.IO.File.Delete(_fileName);
            }

            using (StreamWriter writer = new StreamWriter(_fileName, true, Encoding.UTF8))
            {
                mergedDoc.Save(writer);
            }
        }

        private static void MergeXmlDocuments(XmlDocument mergedDoc, XmlDocument doc, string activityId, string originalDocPath, string outputFolderPath)
        {
            // Update PNG image paths to relative paths
            UpdatePngPaths(doc, originalDocPath, outputFolderPath);

            // Ensure that the root node exists in the merged document
            XmlNode mergedRoot = mergedDoc.DocumentElement;
            if (mergedRoot == null)
            {
                mergedRoot = mergedDoc.CreateElement("TestSuite");
                mergedDoc.AppendChild(mergedRoot);
            }

            // Select all test case nodes from the incoming document
            XmlNodeList testCaseNodes = doc.SelectNodes("/TestSuite/TestCase");

            foreach (XmlNode testCaseNode in testCaseNodes)
            {
                // Add or update the ActivityId for the current test case
                XmlNode activityIdNode = testCaseNode.SelectSingleNode("ActivityId");
                if (activityIdNode == null)
                {
                    activityIdNode = doc.CreateElement("ActivityId");
                    testCaseNode.AppendChild(activityIdNode);
                }
                activityIdNode.InnerText = activityId;

                // Find or create a corresponding suite node for the ActivityId in the merged document
                XmlNode activitySuiteNode = mergedDoc.SelectSingleNode($"/TestSuite/Suite[@ActivityId='{activityId}']");
                if (activitySuiteNode == null)
                {
                    // If no suite exists for this ActivityId, create it
                    activitySuiteNode = mergedDoc.CreateElement("Suite");

                    XmlAttribute activityIdAttr = mergedDoc.CreateAttribute("ActivityId");
                    activityIdAttr.Value = activityId;
                    activitySuiteNode.Attributes.Append(activityIdAttr);

                    mergedRoot.AppendChild(activitySuiteNode);
                }

                // Import and append the test case to the correct suite node
                XmlNode importedTestCase = mergedDoc.ImportNode(testCaseNode, true);
                activitySuiteNode.AppendChild(importedTestCase);
            }
        }
        private static bool IsFileOpen(string filePath)
        {
            try
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    // File is not open by another process
                    return false;
                }
            }
            catch (FileNotFoundException ex)
            {
                // Handle the case where the file doesn't exist
                Console.WriteLine($"File not found: {ex.Message}");
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                // Handle the case where you don't have permission to access the file
                Console.WriteLine($"Unauthorized access: {ex.Message}");
                return true;
            }
            catch (IOException ex)
            {
                // Handle other IO exceptions (file in use by another process)
                Console.WriteLine($"File is in use or another I/O issue occurred: {ex.Message}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception : {ex.Message}");
                return true;
            }
        }

        private static void UpdatePngPaths(XmlDocument doc, string basePath, string outputFolderPath)
        {
            // Find all image nodes (assuming images are within <ScreenShot> tags and the path is in the 'src' attribute)
            XmlNodeList imgNodes = doc.SelectNodes("//ScreenShot");
            foreach (XmlNode imgNode in imgNodes)
            {
                if (imgNode != null)
                {
                    string absolutePath = Path.Combine(basePath, imgNode.FirstChild.Value.Replace("\r\n", ""));
                    string relativePath = GetRelativePath(outputFolderPath, absolutePath);
                    imgNode.FirstChild.Value = relativePath;
                }
            }
        }


        private static string GetRelativePath(string basepath, string targetPath)
        {
            // Ensure that fromPath is an absolute path
            basepath = Path.GetFullPath(basepath);
            // Ensure that toPath is an absolute path
            targetPath = Path.GetFullPath(targetPath);

            // Ensure fromPath ends with a directory separator
            if (!basepath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                basepath += Path.DirectorySeparatorChar;
            }

            // Convert the paths to URIs
            Uri fromUri = new Uri(basepath);
            Uri toUri = new Uri(targetPath);

            if (fromUri.Scheme != toUri.Scheme)
            {
                // If they are not of the same scheme, return the absolute path
                return targetPath;
            }

            // Make the relative URI
            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            // Convert URI path separators to system path separators
            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }

        public static void DryrunProcess(string tsPath, string outputDir)
        {
            try
            {
                if (tsPath != null)
                {
                    if (outputDir == null)
                    {
                        Console.WriteLine("Please provide a directory for logging.");
                        return;
                    }


                    if (tsPath.Contains(".txt") || tsPath.Contains(".gflib"))
                    {
                        string fileName = Utils.GetFileNameFromTheDirectory(tsPath).Split('.')[0];
                        string outputPath = Path.Combine(outputDir, fileName) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        if (!Directory.Exists(outputPath))
                        {
                            Directory.CreateDirectory(outputPath);
                        }
                        GFriendLoggerServices.InitLogger(outputPath);
                        Reporter.InitReport(outputPath);
                        Logger.Trace("TestSuite : " + tsPath);
                        Reporter.WriteToOutput("TestSuite", new Dictionary<string, string>() { { "Name", fileName } }, false);
                        Reporter.WriteToOutput("StartTime", Utils.GetTime());
                        Utils.DryrunValidation(tsPath);
                        Reporter.WriteToOutput("EndTime", Utils.GetTime());
                        Reporter.WriteToOutput("TestSuite", false);
                        Logger.Trace("--------------------------------------------------------------------------------------------------------");

                        Reporter.EndReport();
                        Reporter.GenerateReport();
                    }
                    else
                    {
                        string folderName = tsPath.Split('\\')[tsPath.Split('\\').Length - 1];
                        string outputPath = Path.Combine(outputDir, folderName) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        GFriendLoggerServices.InitLogger(outputPath);
                        Reporter.InitReport(outputPath);

                        Logger.Trace("TestSuite : " + folderName);
                        Reporter.WriteToOutput("TestSuite", new Dictionary<string, string>() { { "Name", folderName } }, false);
                        Reporter.WriteToOutput("StartTime", Utils.GetTime());

                        string[] testFiles = Directory.GetFiles(tsPath, "*", SearchOption.AllDirectories);
                        usedPaths.Clear();
                        variables.Clear();

                        foreach (string testPath in testFiles)
                        {
                            //ignoring flowchart files
                            if (testPath.Contains(".txt") && !testPath.ToLower().Contains("flowchart"))
                            {
                                string fileName = Utils.GetFileNameFromTheDirectory(testPath).Split('.')[0];


                                if (!Directory.Exists(outputPath))
                                {
                                    Directory.CreateDirectory(outputPath);
                                }


                                if (fileName.ToLower() != "Readme".ToLower())
                                {
                                    Utils.DryrunValidation(testPath);
                                }
                            }
                        }

                        Reporter.WriteToOutput("EndTime", Utils.GetTime());
                        Reporter.WriteToOutput("TestSuite", false);
                        Reporter.EndReport();
                        Reporter.GenerateReport();
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Trace(e.Message);
            }
        }
        /// <summary>
        /// This method  
        /// 1.checks the test case path
        /// 2.validates test case
        /// </summary>
        /// <param name="testcase"></param>
        /// <returns></returns>
        public static void DryrunValidation(string testcase)
        {
            try
            {
                if (!CheckFileExist(testcase))
                {
                    Logger.Trace("test case file does not exist : " + testcase);
                    Console.WriteLine("test case file does not exist : " + testcase);
                }
                else
                {
                    usedPaths.Add(testcase);
                }

                folderpath = GetFolderFromTheFilePath(testcase);

                ValdiateTestCase(testcase);

                Console.WriteLine("Total : " + totalTestCases + "\tPass : " + passTestCases + "\tFail : " + failTestCases + "\tError : " + errorTestCases);
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Trace(e.Message);
            }
        }
        /// <summary>
        /// This method validates
        /// 1.Resources
        /// 2.Each line in the script
        /// 3.variables
        /// </summary>
        /// <param name="testcase"></param>
        /// <returns></returns>
        private static void ValdiateTestCase(string testcase)
        {
            try
            {
                //validates the using statements in txt and get the files used
                ValidateAndGetResources(testcase);

                foreach (string path in usedPaths)
                {
                    if (!path.Contains(".gfvar") && !validatedCustomPaths.Contains(path))
                    {
                        ValidateEachFile(path);

                        validatedCustomPaths.Add(path);
                        aliasLibrary.Clear();
                    }
                }

                //validate variables used in txt or gflib file exists in gfvar file or not
                if (variables.Count == 0)
                {
                }
                else
                {
                    if (testcase.Contains("gfvar"))
                    {
                        foreach (string variable in variables)
                        {
                            if (!ValidateVariable(variable))
                            {
                                Logger.Trace("The variable " + variable + " does not exist");
                            }
                        }
                    }
                }


            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Trace(e.Message);
            }
        }

        private static void ValidateAndGetResources(string testcase)
        {
            try
            {
                StreamReader tsFile = new StreamReader(testcase);
                string tsContents = tsFile.ReadToEnd();
                tsFile.Close();

                StringReader reader = new StringReader(tsContents);
                string line = "";
                while ((line = reader.ReadLine()) != null)
                {
                    if (!line.Trim().StartsWith("//"))
                    {
                        //reources validation
                        if (line.Trim().ToLower().StartsWith("using"))
                        {
                            int linelength = line.Split(' ').Length;
                            if (linelength <= 4)
                            {
                                if (!ValidateResources(testcase, line))
                                {
                                    Logger.Trace(line.Trim() + " : Fail");
                                }
                                else
                                {
                                    Logger.Trace(line.Trim() + " : Pass");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Trace(e.Message);
            }
        }

        private static void ValidateEachFile(string file)
        {
            try
            {
                StreamReader tsFile = new StreamReader(file);
                string tsContents = tsFile.ReadToEnd();
                tsFile.Close();

                StringReader reader = new StringReader(tsContents);
                string line;


                //validates each line other than using statements for .txt file
                if (file.Contains(".txt"))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!line.Trim().StartsWith("//"))
                        {
                            //reources validation
                            if (!line.Trim().ToLower().StartsWith("using"))
                            {
                                if (!ValidateLine(line))
                                {
                                    testcasevalidated = false;
                                }
                            }
                        }
                    }
                }
                if (!file.Contains(".txt"))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!line.Trim().StartsWith("//"))
                        {
                            //reources validation
                            if (line.Trim().ToLower().StartsWith("using"))
                            {
                                int linelength = line.Split(' ').Length;
                                if (linelength <= 4)
                                {
                                    if (!ValidateResources(file, line))
                                    {
                                        Logger.Trace(line.Trim() + " : Fail");
                                    }
                                    else
                                    {
                                        Logger.Trace(line.Trim() + " : Pass");
                                    }
                                }
                            }

                            else
                            {
                                if (!ValidateLine(line))
                                {
                                    testcasevalidated = false;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Trace(e.Message);
            }
        }
        /// <summary>
        /// This method validates reources which are written with using statements
        /// </summary>
        /// <param name="testcasepath"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        private static bool ValidateResources(string testcasepath, string line)
        {
            bool isreourcevalidated = false;
            try
            {
                if (line.Contains(".gfvar"))
                {

                    string gfvarlib = line.Split(new char[] { ' ' })[1].Trim();
                    string varFilePath = GetFilePath(line, gfvarlib, testcasepath);
                    if (CheckFileExist(varFilePath))
                    {
                        if (!usedPaths.Contains(varFilePath))
                        {
                            usedPaths.Add(varFilePath);
                        }
                        isreourcevalidated = true;
                    }
                    else
                    {
                        isreourcevalidated = false;
                    }
                }
                else if (line.Contains(".gflib"))
                {
                    string gflib = line.Split(new char[] { ' ' })[1].Trim();
                    string libfilePath = GetFilePath(line, gflib, testcasepath);
                    if (CheckFileExist(libfilePath))
                    {
                        if (!usedPaths.Contains(libfilePath))
                        {
                            usedPaths.Add(libfilePath);
                        }

                        if (line.Contains("\\"))
                        {
                            int lineLength = line.Split('\\').Length;
                            string libname = line.Split('\\')[lineLength - 1].Split('.')[0].Trim();
                            customLibraries.Add(libname);
                        }
                        else
                        {
                            customLibraries.Add(gflib.Split('.')[0]);
                        }

                        isreourcevalidated = true;
                    }
                    else
                    {
                        isreourcevalidated = false;
                    }
                }
                else if (!line.Contains("gf"))
                {
                    if (line.ToLower().Contains("as"))
                    {
                        string CustomlibName = line.Split(new string[] { "As" }, StringSplitOptions.None)[1];
                        string lib = line.Split(new string[] { "As" }, StringSplitOptions.None)[0].Split(' ')[1];
                        if (!aliasLibrary.ContainsKey(CustomlibName) && aliasLibrary.Count > 0)
                        {
                            aliasLibrary.Add(CustomlibName, lib);
                        }
                        else
                        {
                            aliasLibrary.Add(CustomlibName, lib);
                        }
                        filecontainsAliasNameForLibrary = true;
                    }

                    string libraryname = line.Split(' ')[1].Trim();
                    int lineLength = line.Split(' ').Length;
                    if (lineLength <= 4)
                    {
                        GetAvailabelLibraries();
                        if (!ValidateLibraries(libraryname))
                        {
                            isreourcevalidated = false;
                        }
                        else
                        {
                            isreourcevalidated = true;
                        }
                    }
                }
                else
                {
                    isreourcevalidated = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Trace(e.Message);
            }

            return isreourcevalidated;
        }
        /// <summary>
        /// This method validates the librarie used in using statement
        /// </summary>
        /// <param name="libraryname"></param>
        /// <returns></returns>
        private static bool ValidateLibraries(string libraryname)
        {
            bool libraryValdiated = false;
            try
            {
                foreach (var library in availableLibraries)
                {
                    if (libraryname == library)
                    {
                        libraryValdiated = true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Trace(e.Message);
            }
            return libraryValdiated;
        }
        /// <summary>
        /// This method validates library , keyword , parameters of each line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static bool ValidateLine(string line)
        {
            bool islineValidted = true;

            try
            {
                bool isBuiltin = true;

                string testcasename = "";

                foreach (var lib in aliasLibrary)
                {
                    if (line.Trim().Contains(lib.Key.Trim()))
                    {
                        line = line.Replace(lib.Key, lib.Value);
                    }
                }

                if (line.Contains("{") && !line.Contains("$"))
                {
                    if (line.ToLower().Contains("repeat:") || line.ToLower().Contains("for:") || line.ToLower().Contains("while:") || line.ToLower().Contains("if:") || line.ToLower().Contains("foreachrow:") || line.ToLower().Contains("remoterun:") || line.ToLower().Contains("fail:") || line.ToLower().Contains("error:") || line.ToLower().Contains("for each row:") || line.ToLower().Contains("remote run:"))
                    {
                    }
                    else
                    {
                        testcasename = testcasenameline.Trim();
                    }
                }
                else
                {
                    testcasenameline = line;
                }


                GetAvailabelLibraries();
                foreach (string lib in availableLibraries)
                {
                    if (line.Contains("."))
                    {
                        if (line.Split('.')[0].Contains(lib))
                        {
                            isBuiltin = false;
                            break;
                        }
                    }
                    if (!line.Contains(".") && line.Contains("("))
                    {
                        if (line.Split('(')[0].Contains(lib))
                        {
                            isBuiltin = false;
                            break;
                        }
                    }
                }
                foreach (string lib in customLibraries)
                {
                    if (line.Contains("."))
                    {
                        if (line.Split('.')[0].Contains(lib))
                        {
                            isBuiltin = false;
                            break;
                        }
                    }
                    if (!line.Contains(".") && line.Contains("("))
                    {
                        if (line.Split('(')[0].Contains(lib))
                        {
                            isBuiltin = false;
                            break;
                        }
                    }
                }

                if (line.Trim().StartsWith("{"))
                {
                    linedepth++; islineValidted = true;
                    if (linedepth == 1)
                    {
                        totalTestCases++;
                        testcasevalidated = true;
                        Console.WriteLine("\n::: Test Case :::: " + testcasename);
                        Logger.Trace("Test case " + totalTestCases + " : " + testcasename);
                        Reporter.WriteToOutput("TestCase", new Dictionary<string, string>() { { "Name", testcasename } }, false);
                        Reporter.WriteToOutput("StartTime", Utils.GetTime());
                    }
                }
                else if (line.Trim().StartsWith("}"))
                {
                    linedepth--; islineValidted = true;
                    if (linedepth == 0)
                    {
                        if (testcasevalidated)
                        {
                            Console.WriteLine();
                            Reporter.WriteToOutput("Result", "PASS");
                            Reporter.WriteToOutput("EndTime", Utils.GetTime());
                            Reporter.WriteToOutput("TestCase", false);
                            passTestCases++;
                        }
                        else
                        {
                            Console.WriteLine();
                            Reporter.WriteToOutput("Result", "FAIL");
                            Reporter.WriteToOutput("EndTime", Utils.GetTime());
                            Reporter.WriteToOutput("TestCase", false);
                            failTestCases++;
                        }

                        testcasevalidated = true;
                    }
                }
                else if (line.Trim().StartsWith("//"))
                {
                    islineValidted = true;
                }
                else if (line.Trim().Equals(""))
                {
                    islineValidted = true;
                }
                else if (isBuiltin == true && linedepth > 0)
                {

                    List<string> keywords = new List<string>();

                    if (line.Contains("$"))
                    {
                        int startindex = line.IndexOf("{");
                        int endindex = line.IndexOf("}");
                        string variable = "$" + line.Substring(startindex, endindex - startindex + 1);
                        if (!variables.Contains(variable))
                        {
                            variables.Add(variable);
                        }
                    }
                    if (line.Trim().StartsWith("@"))
                    {
                        line = line.Trim().TrimStart('@');
                    }

                    string library = "";
                    string keyword = "";
                    if (line.ToLower().Contains("builtin"))
                    {
                        if (line.ToLower().Contains("repeat:") || line.ToLower().Contains("for:") || line.ToLower().Contains("while:") || line.ToLower().Contains("if:") || line.ToLower().Contains("foreachrow:") || line.ToLower().Contains("remoterun:") || line.ToLower().Contains("fail:") || line.ToLower().Contains("error:") || line.ToLower().Contains("for each row:") || line.ToLower().Contains("remote run:"))
                        {
                            library = line.Split('.')[0].Split(':')[1].Trim();
                        }
                        if (library.Contains("("))
                        {
                            library = library.Split('(')[1];
                            keyword = line.Split(new char[] { '(' })[1].Split('.')[1].Trim();
                        }
                        else
                        {
                            keyword = line.Split('(')[0].Split('.')[1].Trim();
                        }
                    }
                    else
                    {
                        if (line.ToLower().Contains("repeat:") || line.ToLower().Contains("for:") || line.ToLower().Contains("while:") || line.ToLower().Contains("if:") || line.ToLower().Contains("foreachrow:") || line.ToLower().Contains("remoterun:") || line.ToLower().Contains("fail:") || line.ToLower().Contains("error:") || line.ToLower().Contains("for each row:") || line.ToLower().Contains("remote run:"))
                        {
                            keyword = line.Split('(')[0].Split(':')[1].Trim();
                            if (keyword.Contains("!"))
                            {
                                keyword = line.Split('(')[0].Split(':')[1].Trim().TrimStart('!');
                            }
                        }
                        else
                        {
                            keyword = line.Split(new char[] { '(' })[0].Trim();
                        }
                    }
                    string[] arguments = new string[line.Split(',').Length];
                    if (line.Contains(','))
                    {
                        arguments = new string[line.Split(',').Length];
                        arguments = line.Split(',');
                    }
                    else if (line.Contains("("))
                    {
                        string argumentValue = line.Split(new char[] { '(' })[1].Trim(new char[] { ')' });
                        if (!string.IsNullOrEmpty(argumentValue))
                        {
                            arguments = new string[1];
                            arguments[0] = argumentValue;
                        }
                        else
                        {
                            arguments = new string[0];
                        }
                    }
                    if (keyword != "")
                    {
                        Type type = typeof(BuiltInLibrary);
                        MethodInfo[] methods = type.GetMethods();
                        foreach (MethodInfo m in methods)
                        {
                            Keyword aKeyword = new Keyword();
                            Utils.GetKeywordFromMethod(m, aKeyword);
                            if (m.ReturnType == typeof(KeywordResult))
                            {
                                keywords.Add(aKeyword.KeywordName);
                            }
                            ParameterInfo[] parameterinfo = m.GetParameters();


                            if (keyword.Trim().ToLower() == aKeyword.KeywordName.Trim().ToLower())
                            {
                                if (parameterinfo.Length == arguments.Length)
                                {
                                    if (ValidateParameters(line, keyword.Replace(" ", ""), m))
                                    {
                                        islineValidted = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    islineValidted = false;
                                    break;
                                }
                                islineValidted = true;
                                break;
                            }
                            else
                            {
                                islineValidted = false;
                            }
                        }
                    }
                    else
                    {
                        islineValidted = true;
                    }

                    if (islineValidted == false)
                    {
                        if (line.ToLower().Contains("repeat:") || line.ToLower().Contains("for:") || line.ToLower().Contains("while:") || line.ToLower().Contains("if:") || line.ToLower().Contains("foreachrow:") || line.ToLower().Contains("remoterun:") || line.ToLower().Contains("fail:") || line.ToLower().Contains("error:") || line.ToLower().Contains("for each row:") || line.ToLower().Contains("remote run:"))
                        {
                            if (!line.Contains("("))
                            {
                                islineValidted = true;
                            }
                        }
                    }

                    if (islineValidted == true)
                    {
                        Reporter.WriteToOutput("Statement", new Dictionary<string, string>() { { "StatementName", line } }, false);
                        Reporter.WriteToOutput("StartTime", Utils.GetTime());
                        Reporter.WriteToOutput("Result", "Pass");
                        Reporter.WriteToOutput("EndTime", Utils.GetTime());
                        Reporter.WriteToOutput("Statement", false);
                        Console.WriteLine(line.Trim() + " :: Pass");
                        Logger.Trace(line.Trim() + " : Pass");
                    }
                    else
                    {
                        Reporter.WriteToOutput("Statement", new Dictionary<string, string>() { { "StatementName", line } }, false);
                        Reporter.WriteToOutput("StartTime", Utils.GetTime());
                        Reporter.WriteToOutput("Result", "Fail");
                        Reporter.WriteToOutput("AdditionalInfo", "Cannot find the keyword : " + keyword);
                        Reporter.WriteToOutput("EndTime", Utils.GetTime());
                        Reporter.WriteToOutput("Statement", false);
                        Console.WriteLine(line.Trim() + " :: Fail");
                        Logger.Trace(line.Trim() + " : Fail");
                    }
                }
                else if (isBuiltin == false && linedepth > 0)
                {
                    if (line.Contains("$"))
                    {
                        int startindex = line.IndexOf("{");
                        int endindex = line.IndexOf("}");
                        string variable = "$" + line.Substring(startindex, endindex - startindex + 1);
                        if (!variables.Contains(variable))
                        {
                            variables.Add(variable);
                        }
                    }

                    if (line.Trim().StartsWith("@"))
                    {
                        line = line.Trim().TrimStart('@');
                    }

                    string library = line.Split('.')[0].Trim();
                    string keyword = "";
                    if (library.Contains("("))
                    {
                        library = library.Split('(')[1];
                        keyword = line.Split(new char[] { '(' })[1].Split('.')[1].Trim();
                    }
                    else
                    {
                        keyword = line.Split('(')[0].Split('.')[1].Trim();
                    }

                    if (line.ToLower().Contains("repeat:") || line.ToLower().Contains("for:") || line.ToLower().Contains("while:") || line.ToLower().Contains("if:") || line.ToLower().Contains("foreachrow:") || line.ToLower().Contains("remoterun:") || line.ToLower().Contains("fail:") || line.ToLower().Contains("error:") || line.ToLower().Contains("for each row:") || line.ToLower().Contains("remote run:"))
                    {
                        library = line.Split('.')[0].Split(':')[1].Trim();
                        if (library.Contains("("))
                        {
                            library = library.Split('(')[1];
                        }
                        if (library.Contains("!"))
                        {
                            int startindex = library.IndexOf("!");
                            library = library.Substring(startindex + 1).Split('.')[0].Trim(); ;
                        }
                    }
                    string[] arguments = new string[line.Split(',').Length];
                    if (line.Contains(','))
                    {
                        arguments = line.Split(',');
                    }
                    else if (line.Contains("("))
                    {
                        string argumentValue = line.Split(new char[] { '(' })[1].Trim(new char[] { ')' });
                        if (!string.IsNullOrEmpty(argumentValue))
                        {
                            arguments = new string[1];
                            arguments[0] = argumentValue;
                        }
                        else
                        {
                            arguments = new string[0];
                        }
                    }
                    var lineContainsLibrary = availableLibraries.Where(l => l.Equals(library)).ToList();
                    var lineContainsCustomLibrary = customLibraries.Where(l => l.Equals(library)).ToList();

                    if (lineContainsLibrary.Count > 0)
                    {
                        string librarypath = Directory.GetCurrentDirectory() + "\\libs";
                        //for local debugging
                        //string librarypath = "C:\\source\\GFriend_Master_Latest_DryRun\\bin\\Debug\\GFriendUI\\libs";
                        string file = Path.Combine(librarypath, "GFK." + library + ".dll");

                        Assembly dll = Assembly.LoadFrom(file);
                        List<Type> types = dll.GetExportedTypes().ToList<Type>();
                        foreach (Type type in types)
                        {
                            int keywordCount = 0;
                            MethodInfo[] methods = type.GetMethods();
                            foreach (MethodInfo m in methods)
                            {
                                Keyword aKeyword = new Keyword();
                                Utils.GetKeywordFromMethod(m, aKeyword);
                                if (aKeyword.KeywordName.ToLower() == keyword.ToLower())
                                {
                                    if (arguments[0] != null)
                                    {
                                        foreach (string arg in arguments)
                                        {
                                            if (string.IsNullOrEmpty(arg))
                                            {
                                                islineValidted = false;
                                                break;
                                            }
                                        }
                                    }
                                    if (islineValidted != false)
                                    {
                                        if (m.GetParameters().Length == 0)
                                        {
                                            if (arguments[0] == null)
                                            {
                                                keywordCount++;
                                            }
                                        }
                                        else if (m.GetParameters().Length > 0 && arguments.Count() == m.GetParameters().Length)
                                        {
                                            keywordCount++;
                                        }
                                        islineValidted = true;
                                    }
                                }
                            }

                            if (keywordCount > 0)
                            {
                                islineValidted = true;
                                break;
                            }
                            else
                            {
                                islineValidted = false;
                            }
                        }


                        if (islineValidted == false)
                        {
                            if (line.ToLower().Contains("repeat:") || line.ToLower().Contains("for:") || line.ToLower().Contains("while:") || line.ToLower().Contains("if:") || line.ToLower().Contains("foreachrow:") || line.ToLower().Contains("remoterun:") || line.ToLower().Contains("fail:") || line.ToLower().Contains("error:") || line.ToLower().Contains("for each row:") || line.ToLower().Contains("remote run:"))
                            {
                                if (!line.Contains("("))
                                {
                                    islineValidted = true;
                                }
                            }
                        }

                        if (islineValidted == true)
                        {
                            Reporter.WriteToOutput("Statement", new Dictionary<string, string>() { { "StatementName", line } }, false);
                            Reporter.WriteToOutput("StartTime", Utils.GetTime());
                            Reporter.WriteToOutput("Result", "Pass");
                            Reporter.WriteToOutput("EndTime", Utils.GetTime());
                            Reporter.WriteToOutput("Statement", false);
                            Console.WriteLine(line.Trim() + " :: Pass");
                            Logger.Trace(line.Trim() + " : Pass");
                        }
                        else
                        {
                            Reporter.WriteToOutput("Statement", new Dictionary<string, string>() { { "StatementName", line } }, false);
                            Reporter.WriteToOutput("StartTime", Utils.GetTime());
                            Reporter.WriteToOutput("Result", "Fail");
                            Reporter.WriteToOutput("AdditionalInfo", "Cannot find the keyword : " + keyword);
                            Reporter.WriteToOutput("EndTime", Utils.GetTime());
                            Reporter.WriteToOutput("Statement", false);
                            Console.WriteLine(line.Trim() + " :: Fail");
                            Logger.Trace(line.Trim() + " :: Fail");
                        }
                    }
                    else if (lineContainsCustomLibrary.Count > 0)
                    {
                        if (ValidateCustomLibraries(line, library))
                        {
                            foreach (string file in usedPaths)
                            {
                                if (file.Contains("gflib"))
                                {
                                    GetCustomKeywords(file);
                                }
                            }
                            if (ValidateCustomKeywords(line, customKeywords))
                            {
                                islineValidted = true;
                            }
                            else
                            {
                                islineValidted = false;
                            }
                            //if (islineValidted == true)
                            //{
                            //    Reporter.WriteToOutput("Statement", new Dictionary<string, string>() { { "StatementName", line } }, false);
                            //    Reporter.WriteToOutput("StartTime", Utils.GetTime());
                            //    Reporter.WriteToOutput("Result", "Pass");
                            //    Reporter.WriteToOutput("EndTime", Utils.GetTime());
                            //    Reporter.WriteToOutput("Statement", false);
                            //    Console.WriteLine(line.Trim() + " :: Pass");
                            //    Logger.Trace(line.Trim() + " : Pass");
                            //}
                            //else
                            //{
                            //    Reporter.WriteToOutput("Statement", new Dictionary<string, string>() { { "StatementName", line } }, false);
                            //    Reporter.WriteToOutput("StartTime", Utils.GetTime());
                            //    Reporter.WriteToOutput("Result", "Fail");
                            //    Reporter.WriteToOutput("EndTime", Utils.GetTime());
                            //    Reporter.WriteToOutput("Statement", false);
                            //    Console.WriteLine(line.Trim() + " :: Fail");
                            //    Reporter.WriteToOutput("AdditionalInfo", "Cannot find the keyword : " + keyword);
                            //    Logger.Trace(line.Trim() + " : Fail");
                            //}
                        }
                        else
                        {
                            islineValidted = false;
                        }

                        if (islineValidted == true)
                        {
                            Reporter.WriteToOutput("Statement", new Dictionary<string, string>() { { "StatementName", line } }, false);
                            Reporter.WriteToOutput("StartTime", Utils.GetTime());
                            Reporter.WriteToOutput("Result", "Pass");
                            Reporter.WriteToOutput("EndTime", Utils.GetTime());
                            Reporter.WriteToOutput("Statement", false);
                            Console.WriteLine(line.Trim() + " :: Pass");
                            Logger.Trace(line.Trim() + " : Pass");
                        }
                        else
                        {
                            Reporter.WriteToOutput("Statement", new Dictionary<string, string>() { { "StatementName", line } }, false);
                            Reporter.WriteToOutput("StartTime", Utils.GetTime());
                            Reporter.WriteToOutput("Result", "Fail");
                            Reporter.WriteToOutput("AdditionalInfo", "Cannot find the keyword : " + keyword);
                            Reporter.WriteToOutput("EndTime", Utils.GetTime());
                            Reporter.WriteToOutput("Statement", false);
                            Console.WriteLine(line.Trim() + " :: Fail");
                            Logger.Trace(line.Trim() + " : Fail");
                        }
                    }
                    else
                    {
                        islineValidted = false;
                    }

                }
                else
                {
                    islineValidted = true;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Trace(e.Message);
            }
            return islineValidted;
        }
        /// <summary>
        /// this method validates parameters of builtin library
        /// </summary>
        /// <param name="line"></param>
        /// <param name="keyword"></param>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        private static bool ValidateParameters(string line, string keyword, MethodInfo methodInfo)
        {
            bool isParametersValid = false;
            try
            {
                ParameterInfo[] parameterinfo = methodInfo.GetParameters();
                int parametersCount = parameterinfo.Length;

                int keywordParametersCount = 0;
                string parameterstartline = line.Split('(')[1];
                string[] parameters = parameterstartline.Split(',');
                foreach (string parameter in parameters)
                {
                    if (String.IsNullOrEmpty(parameter))
                    {
                        return false;
                    }
                    keywordParametersCount++;
                }

                if (parametersCount != keywordParametersCount)
                {
                    isParametersValid = false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Trace(e.Message);
            }
            return isParametersValid;
        }
        /// <summary>
        /// The method validates the customlibraries
        /// </summary>
        /// <param name="line"></param>
        /// <param name="library"></param>
        /// <returns></returns>
        private static bool ValidateCustomLibraries(string line, string library)
        {
            bool libraryValdiated = false;
            try
            {
                string libraryname = "";

                if (line.Contains(library))
                {
                    if (line.ToLower().Contains("repeat:") || line.ToLower().Contains("for:") || line.ToLower().Contains("while:") || line.ToLower().Contains("if:") || line.ToLower().Contains("foreachrow:") || line.ToLower().Contains("remoterun:") || line.ToLower().Contains("fail:") || line.ToLower().Contains("error:") || line.ToLower().Contains("for each row:") || line.ToLower().Contains("remote run:"))
                    {
                        libraryname = line.Split('.')[0].Split(':')[1].Trim();
                        if (libraryname.Contains("("))
                        {
                            libraryname = libraryname.Split('(')[1];
                        }
                        List<string> libraries = customLibraries.Where(lib => lib.Equals(libraryname)).ToList();
                        if (libraries.Count > 0)
                        {
                            libraryValdiated = true;
                        }
                    }
                    else
                    {
                        libraryname = line.Split('.')[0].Trim();
                        if (libraryname.Contains("("))
                        {
                            libraryname = libraryname.Split('(')[1];
                        }
                        List<string> libraries = customLibraries.Where(lib => lib.Equals(libraryname)).ToList();
                        if (libraries.Count > 0)
                        {
                            libraryValdiated = true;
                        }
                    }
                }
                if (libraryValdiated == false)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Trace(e.Message);
            }
            return libraryValdiated;
        }
        /// <summary>
        /// The method gets the all the keyword in gflib
        /// </summary>
        /// <param name="testCase"></param>
        private static void GetCustomKeywords(string testCase)
        {
            try
            {
                if (customLibraries.Count == 0)
                {

                }
                else
                {
                    string previousline = "";
                    string currentline = "";
                    StreamReader tsFile = new StreamReader(testCase);
                    string tsContents = tsFile.ReadToEnd();
                    tsFile.Close();

                    StringReader reader = new StringReader(tsContents);
                    string line = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Contains("{") && !line.Contains("$"))
                        {
                            if (currentline.ToLower().Contains("repeat:") || currentline.ToLower().Contains("for:") || currentline.ToLower().Contains("while:") || currentline.ToLower().Contains("if:") || currentline.ToLower().Contains("foreachrow:") || currentline.ToLower().Contains("remoterun:") || currentline.ToLower().Contains("fail:") || currentline.ToLower().Contains("error:") || currentline.ToLower().Contains("for each row:") || currentline.ToLower().Contains("remote run:"))
                            {
                            }
                            else
                            {
                                previousline = currentline.Trim();

                                if (customKeywords.ContainsKey(previousline.Split('(')[0].ToLower().Trim()))
                                {

                                }
                                else
                                {
                                    if (previousline.Contains("("))
                                    {
                                        string customkeyword = previousline.Split('(')[0];
                                        string parameterstartline = previousline.Split('(')[1];
                                        string[] parameters = parameterstartline.Split(',');
                                        customKeywords.Add(customkeyword.Trim().ToLower(), parameters.Length);
                                    }
                                    else
                                    {
                                        customKeywords.Add(previousline.Trim().ToLower(), 0);
                                    }
                                }
                                continue;
                            }
                        }
                        else
                        {
                            currentline = line;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Trace(e.Message);
            }
        }
        /// <summary>
        /// The method validates the custom keyword in each line
        /// </summary>
        /// <param name="line"></param>
        /// <param name="customKeywords"></param>
        /// <returns></returns>
        private static bool ValidateCustomKeywords(string line, Dictionary<string, int> customKeywords)
        {
            bool isCustomKeywordExists = false;
            try
            {
                string currentkeyword = line.Split('.')[1].Split('(')[0].Trim();
                if (currentkeyword.Contains(")"))
                {
                    currentkeyword = currentkeyword.Split(')')[0];
                }
                foreach (string keyword in customKeywords.Keys)
                {
                    string libraryKeyword = keyword.Split('(')[0].Trim();
                    if (currentkeyword.ToLower().Equals(libraryKeyword.ToLower()))
                    {
                        string[] parameters = null;
                        if (line.Contains("("))
                        {
                            string parameterstartline = line.Split('(')[1];
                            parameters = parameterstartline.Split(',');
                        }

                        if (parameters != null)
                        {
                            foreach (string parameter in parameters)
                            {
                                if (string.IsNullOrEmpty(parameter))
                                {
                                    return false;
                                }
                            }

                            if (parameters.Length == customKeywords[libraryKeyword.Trim().ToLower()])
                            {
                                isCustomKeywordExists = true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (customKeywords[libraryKeyword.Trim().ToLower()] == 0)
                            {
                                isCustomKeywordExists = true;
                            }
                            else
                            {
                                return false;
                            }
                        }

                        isCustomKeywordExists = true;
                        break;
                    }
                    else
                    {
                        isCustomKeywordExists = false;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Trace(e.Message);
            }

            return isCustomKeywordExists;
        }
        /// <summary>
        /// The method validates the variables used , only static not dynamic variables
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        private static bool ValidateVariable(string variable)
        {
            string currentVariable = "";
            bool isValidVariables = false;
            try
            {
                foreach (string file in usedPaths)
                {
                    if (!file.Contains("gflib"))
                    {
                        StreamReader tsFile = new StreamReader(file);
                        string tsContents = tsFile.ReadToEnd();
                        tsFile.Close();

                        StringReader reader = new StringReader(tsContents);
                        string line = "";


                        while ((line = reader.ReadLine()) != null)
                        {
                            if (!line.Trim().StartsWith("//"))
                            {
                                if (line.Contains(variable))
                                {
                                    currentVariable = variable;
                                    if (line.Contains("="))
                                    {

                                        string[] splitline = line.Split('=');
                                        string variablename = splitline[0];
                                        string variablevalue = splitline[1];
                                        if (variable.Trim() == variablename.Trim())
                                        {
                                            isValidVariables = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Trace(e.Message);
            }
            return isValidVariables;
        }

        /// <summary>
        /// The method gets the all the libraries in lib folder
        /// </summary>
        /// <returns></returns>
        private static List<string> GetAvailabelLibraries()
        {
            try
            {
                availableLibraries = new List<string>();

                string librarypath = Directory.GetCurrentDirectory() + "\\libs";
                //for local debugging
                //string librarypath = "C:\\source\\GFriend_Master_Latest_DryRun\\bin\\Debug\\GFriendUI\\libs";
                List<string> files = Directory.EnumerateFiles(librarypath, "GFK*.dll", SearchOption.TopDirectoryOnly).ToList();

                foreach (string file in files)
                {
                    availableLibraries.Add(file.Split('.')[1].Split('.')[0]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Trace(e.Message);
            }
            return availableLibraries;
        }
        /// <summary>
        /// The method validates if file exist in particular path or not 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool CheckFileExist(string path)
        {
            bool fileexist = false;
            try
            {
                if (System.IO.File.Exists(path)) { fileexist = true; }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Trace(e.Message);
            }
            return fileexist;
        }
        /// <summary>
        /// The method gets the absoulte path of the file
        /// </summary>
        /// <param name="line"></param>
        /// <param name="fileName"></param>
        /// <param name="testcasepath"></param>
        /// <returns></returns>
        private static string GetFilePath(string line, string fileName, string testcasepath)
        {
            string path = "";
            try
            {
                if (line.Contains("\\"))
                {
                    path = Keywords.Support.Utils.GetAbsolutePath(fileName, Path.GetDirectoryName(testcasepath));
                }
                else
                {
                    path = folderpath + fileName;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Trace(e.Message);
            }
            return path;
        }
        /// <summary>
        /// the method gets the folder from file path
        /// </summary>
        /// <param name="testCase"></param>
        /// <returns></returns>
        private static string GetFolderFromTheFilePath(string testCase)
        {
            string testpath = "";
            try
            {
                string[] pathparts = testCase.Split(new char[] { '\\' });
                foreach (string path in pathparts)
                {
                    if (!path.Contains("."))
                    {
                        testpath += path + "\\";
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Trace(e.Message);
            }
            return testpath;
        }
        public static string GetFileNameFromTheDirectory(string testCase)
        {
            string filename = "";
            try
            {
                string[] pathparts = testCase.Split(new char[] { '\\' });
                foreach (string path in pathparts)
                {
                    if (path.Contains("."))
                    {
                        filename = path;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Trace(e.Message);
            }
            return filename;
        }
        public static List<EncryptedData> LoadDataFromXml(string xmlFilePath)
        {
            try
            {
                if (!System.IO.File.Exists(xmlFilePath))
                    return new List<EncryptedData>();

                XmlSerializer serializer = new XmlSerializer(typeof(List<EncryptedData>));

                using (TextReader reader = new StreamReader(xmlFilePath))
                {
                    return (List<EncryptedData>)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load XML: {ex.Message}");
                return new List<EncryptedData>();
            }
        }
        public static string Decryptxml(string encryptedValue, string key = "gfriend@123")
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = GenerateAESKey(key);
                    aes.IV = new byte[16]; // Use zero IV (must match encryption IV)

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
                Logger.Error($"Decryption failed: {ex.Message}");
                return string.Empty;
            }
        }
        public static byte[] GenerateAESKey(string key)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(key)).Take(32).ToArray();
            }
        }

        [Serializable]
        public class EncryptedData
        {
            public string Key { get; set; }
            public string OriginalValue { get; set; }
            public string EncryptedValue { get; set; }
        }
    }
}
