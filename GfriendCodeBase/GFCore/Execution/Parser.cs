using HP.GFriend.Core.Custom;
using HP.GFriend.Core.Remote;
using HP.GFriend.GFLogger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms.VisualStyles;
using System.Xml;

namespace HP.GFriend.Core.Execution
{
    public static class Parser
    {
        static Dictionary<int, IfBlock> ifBlocks = new Dictionary<int, IfBlock>();

        public static TestDataManager ParseTestSuite(string tsPath, Dictionary<string, string> executionVariables = null)
        {
            // ifBlocks is a static field shared across every ParseTestSuite call in the same
            // process/session. If it is not cleared here, IfBlock references left over from a
            // previous parse (e.g. after Check All / re-run / platform switch) can be reused by
            // this parse for a different depth collision, causing Fail:/Error: blocks to attach
            // to the wrong (stale) IfBlock and, ultimately, the wrong test case logic to execute.
            ifBlocks.Clear();

            string version = Executor.GetVersion();
            Logger.Trace("GFriend version : " + version);
            Logger.Trace("Parsing start");
            StreamReader tsFile = new StreamReader(tsPath);
            TestDataManager testDataManager = new TestDataManager();
            TestSuite aTestSuite = new TestSuite();
            TestCase aTestCase = null;
            Dictionary<string, string> variables = new Dictionary<string, string>();
            StringReader reader;
            Stack<IGFRunnable> blocks = new Stack<IGFRunnable>();

            List<string> tcMetadata = new List<string>();
            List<string> suiteMetadata = new List<string>();
            IGFRunnable currentBlock = null;
            string line;
            string tsContents;
            int depth;
            int lineNumber = 0;
            aTestSuite._scriptFilePath = tsPath;
            testDataManager.TestSuitePath = tsPath;

            // Delete All Comments First
            string pattern = @"//.{0,}";
            string argPattenWithSlash = @"\(.{0,}(//.{0,}).{0,}\)";
            tsContents = tsFile.ReadToEnd();
            tsContents = tsContents.Replace("\uFEFF", string.Empty);
            tsFile.Close();
            // Handling arguments which contains "//"
            foreach (Match m in Regex.Matches(tsContents, argPattenWithSlash))
            {
                tsContents = tsContents.Replace(m.Value, m.Value.Replace("//","@@@/@@/@@@"));
            }

            // Keep Metadata comment (starts with "///")
            tsContents = tsContents.Replace("///", "###/###/###/###");

            tsContents = Regex.Replace(tsContents, pattern, string.Empty);
            tsContents = tsContents.Replace("@@@/@@/@@@", "//");
            tsContents = tsContents.Replace("###/###/###/###", "///");
            reader = new StringReader(tsContents);

            // Variable patterns
            pattern = @"^\$\{[0-9a-zA-Z_-]+\}";


            // Testsuite overall metadata parsing
            suiteMetadata = new List<string>();
            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;
                // overall metadata parsing
                if (line.StartsWith("///"))
                {
                    suiteMetadata.Add(line.TrimStart('/'));
                }
                else
                {
                    if (suiteMetadata.Count > 0)
                    {
                        aTestSuite.MetaData = string.Join("\r\n", suiteMetadata);
                        XmlDocument meta = new XmlDocument();
                        meta.LoadXml("<meta>" + aTestSuite.MetaData + "</meta>");

                        foreach (XmlNode node in meta.SelectSingleNode("//meta").ChildNodes)
                        {
                            if (aTestSuite.MetaDataDic.ContainsKey(node.Name.ToLower()) && !string.IsNullOrEmpty(node.InnerText.Trim()))
                            {
                                aTestSuite.MetaDataDic[node.Name.ToLower()] += "\n" + node.InnerText.Trim();
                            }
                            else
                            {
                                aTestSuite.MetaDataDic.Add(node.Name.ToLower(), node.InnerText);
                            }

                        }

                        aTestSuite.Description = string.Empty;
                        foreach(KeyValuePair<string, string> kv in aTestSuite.MetaDataDic)
                        {
                            aTestSuite.Description += $"[{char.ToUpper(kv.Key[0])+kv.Key.Substring(1)}]\r\n{kv.Value}\r\n\r\n";
                        }
                        aTestSuite.Description = aTestSuite.Description.TrimEnd('\r', '\n');
                    }
                    break;
                }
            }

            tsContents = line + Environment.NewLine + reader.ReadToEnd();

            reader = new StringReader(tsContents);
            while ((line = reader.ReadLine())!=null)
            {
                lineNumber++;
                line = line.Trim();


                if (Regex.IsMatch(line, pattern) || string.IsNullOrEmpty(line))
                {
                    // Variable parsing
                    if (Regex.IsMatch(line, pattern))
                    {
                        string var = line.Split('=')[0].Trim();
                        string value = line.Split(new char[] {'='}, 2)[1].Trim();
                        variables.Add(var, value);
                    }

                }

                // "using" parsing
                else if (line.ToUpperInvariant().StartsWith("USING"))
                {
                    // Can use execution variable in using statement
                    if (executionVariables != null)
                    {
                        foreach (KeyValuePair<string, string> variable in executionVariables)
                        {
                            line = line.Replace(variable.Key, variable.Value);
                        }
                    }

                    ParseUsing(line, tsPath, testDataManager, variables, executionVariables);
                }

                // "resource" parsing
                else if (line.ToUpperInvariant().StartsWith("RESOURCE"))
                {
                    // Can use execution variable in resource statement
                    if (executionVariables != null)
                    {
                        foreach (KeyValuePair<string, string> variable in executionVariables)
                        {
                            line = line.Replace(variable.Key, variable.Value);
                        }
                    }
                    ParseResource(line, tsPath, testDataManager);
                }

                // "dataset" parsing
                else if(line.ToUpper().StartsWith("DATASET"))
                {
                    // Can use execution variable in dataset statement
                    if (executionVariables != null)
                    {
                        foreach (KeyValuePair<string, string> variable in executionVariables)
                        {
                            line = line.Replace(variable.Key, variable.Value);
                        }
                    }
                    ParseDataset(line, tsPath, testDataManager);
                }

                else
                {
                    break;
                }
            }

            testDataManager.PostActionAfterParsingUsings();

            foreach(Library lib in testDataManager.AllUsedLib)
            {
                aTestSuite.UsingLibrary(lib.NameAs);
            }
            tsContents = line + "\n" + reader.ReadToEnd();

            if (executionVariables != null)
            {
                foreach (KeyValuePair<string, string> variable in executionVariables)
                {
                    tsContents = tsContents.Replace(variable.Key, variable.Value);
                }
            }

            //to bypass Calculate And Assign , ReAssign Value To Variable , Pass , Fail by not replacing variable name with value 
            string[] byPassKeywords = { "Calculate And Assign", "ReAssign Value To Variable", "Pass", "Fail" };
            StringReader contentreader = new StringReader(tsContents);
            StringBuilder modifiedTSContents = new StringBuilder(tsContents);

            foreach (KeyValuePair<string, string> variable in variables)
            {
                while ((line = contentreader.ReadLine()) != null)
                {
                    ArrayList variablenamesInLine = LibraryUtils.variablenamesInLine(line);
                    foreach (string variablenameInLine in variablenamesInLine)
                    {
                        if (variablenameInLine.Equals(variable.Key))
                        {
                            string newline = line;
                            if (LibraryUtils.IsByPassedKeyword(byPassKeywords, line))
                            {
                                if (!testDataManager.Variables.ContainsKey(variable.Key))
                                {
                                    testDataManager.Variables.Add(variable.Key, variable.Value);
                                }
                                continue;
                            }
                            else
                            {
                                newline = line.Replace(variable.Key, variable.Value);
                                modifiedTSContents.Replace(line, newline);
                            }
                        }
                    }
                }
                contentreader = new StringReader(modifiedTSContents.ToString());
            }
            lineNumber--;
            tsContents = modifiedTSContents.ToString();

            // Start Parsing

            // Handling arguments before re-formating line
            pattern = @"\$\{[0-9a-zA-Z_-]+\}";
            Dictionary<string, string> replaceDict = new Dictionary<string, string>();
            foreach (Match m in Regex.Matches(tsContents, pattern))
            {
                string replacement = $"@@##@@{m.Value}@@##@@";
                replaceDict[m.Value] = replacement;
            }

            // Perform replacements in a single pass
            StringBuilder sb = new StringBuilder(tsContents);
            foreach (KeyValuePair<string, string> kvp in replaceDict)
            {
                sb.Replace(kvp.Key, kvp.Value);
            }

            // Re-format line for { and }
            string formattedContents = sb.ToString().Replace("\r\n", "\n")
                .Replace("{\n", "\n{\n")
                .Replace("}\n", "\n}\n")
                .Replace("} \n", "\n}\n")
                .Replace("{ \n", "\n{\n");

            // Restore original replacements
            foreach (KeyValuePair<string, string> kvp in replaceDict)
            {
                formattedContents = formattedContents.Replace(kvp.Value, kvp.Key);
            }

            lineNumber--;

            depth = 0;
            reader = new StringReader(formattedContents);
            while ((line = reader.ReadLine())!=null)
            {
                lineNumber++;
                line = line.Trim();
                if(line.EndsWith("{"))
                {
                    lineNumber--;
                    if (aTestCase != null)
                    {
                        aTestCase.OriginalStatement += Environment.NewLine + new string(' ', depth*4) + line;
                    }
                    line = line.Remove(line.Length - 1);
                    depth++;
                    if(currentBlock != null)
                    {
                        blocks.Push(currentBlock);
                    }

                }
                if(line.EndsWith("}") && !Regex.IsMatch(line, pattern))
                {
                    lineNumber--;
                    depth--;
                    if(depth < 0 ) depth = 0;
                    if (aTestCase != null)
                    {
                        aTestCase.OriginalStatement += Environment.NewLine + new string(' ', depth * 4) + line;
                    }
                    line = line.Remove(line.Length - 1);

                    try
                    {
                        blocks.Pop();
                    }
                    catch (Exception)
                    {
                        // ignore it
                    }
                    if(blocks.Count > 0)
                    {
                        currentBlock = blocks.Peek();
                    }
                    else
                    {
                        currentBlock = null;
                    }

                }

                // TC Metadata
                if (line.StartsWith("///"))
                {
                    line = line.Replace("&", "&amp;");
                    tcMetadata.Add(line.TrimStart('/'));
                }

                else if(!string.IsNullOrEmpty(line))
                {
                    if (depth == 0) // Get TC Name
                    {
                        aTestCase = new TestCase();
                        aTestCase.Name = line;
                        aTestCase.OriginalStatement = line;
                        if(tcMetadata.Count > 0)
                        {
                            try
                            {
                                aTestCase.MetaData = string.Join("\r\n", tcMetadata);
                                XmlDocument meta = new XmlDocument();

                                meta.LoadXml("<meta>" + aTestCase.MetaData + "</meta>");

                                foreach (XmlNode node in meta.SelectSingleNode("//meta").ChildNodes)
                                {
                                    if (aTestCase.MetaDataDic.ContainsKey(node.Name.ToLower()) && !string.IsNullOrEmpty(node.InnerText.Trim()))
                                    {
                                        aTestCase.MetaDataDic[node.Name.ToLower()] += "\n" + node.InnerText.Trim();
                                    }
                                    else
                                    {
                                        aTestCase.MetaDataDic.Add(node.Name.ToLower(), node.InnerText);
                                    }

                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Trace(aTestCase.MetaData);
                                throw ex;
                            }
                            tcMetadata.Clear();
                        }
                        aTestSuite.AddTestCase(aTestCase);
                    }
                    else
                    {
                        aTestCase.OriginalStatement += Environment.NewLine + new string(' ', depth*4) + line;
                        if(currentBlock != null && currentBlock.GetType().Equals(typeof(RemoteBlock)))
                        {
                            ((RemoteBlock)currentBlock).AddContents(depth, line);
                        }
                        else
                        {
                            currentBlock = ParseLine(depth, line, currentBlock, aTestCase, lineNumber);
                        }

                    }
                }
            }


            aTestSuite.Name = Path.GetFileNameWithoutExtension(tsPath);

            testDataManager.TargetTestSuite = aTestSuite;
            testDataManager.FilesToUse = testDataManager.FilesToUse.Distinct().ToList();

            return testDataManager;

        }

        public static IGFRunnable ParseLine(int depth, string line, IGFRunnable currentBlock, TestCase aTestCase, int lineNumber = 0)
        {
            string pattern = @"\$\{[0-9a-zA-Z_-]+\}";
            string formattedLine = line.Replace(" ", string.Empty);
            bool ignoreErrorFail = false;

            // #321 use '@' for run keyword and ignore error/fail
            if (formattedLine.StartsWith("@"))
            {
                ignoreErrorFail = true;
                formattedLine = formattedLine.TrimStart('@');
            }

            if (formattedLine.StartsWith("Repeat:", StringComparison.CurrentCultureIgnoreCase)) // Repeat
            {
                RepeatBlock aRepeter = new RepeatBlock();
                string repeatPart = line.Split(':')[1].Trim();
                if (!int.TryParse(repeatPart, out int repeatCount))
                {
                    if (repeatPart.EndsWith("h", StringComparison.CurrentCultureIgnoreCase))
                    {
                        repeatPart = repeatPart.Substring(0, repeatPart.Length - 1);
                        int timeInterval = int.Parse(repeatPart);
                        aRepeter.Duration = TimeSpan.FromHours(timeInterval);
                        aRepeter.IsTimeBase = true;
                    }
                    else if (repeatPart.EndsWith("m", StringComparison.CurrentCultureIgnoreCase))
                    {
                        repeatPart = repeatPart.Substring(0, repeatPart.Length - 1);
                        int timeInterval = int.Parse(repeatPart);
                        aRepeter.Duration = TimeSpan.FromMinutes(timeInterval);
                        aRepeter.IsTimeBase = true;
                    }
                    else if (repeatPart.EndsWith("s", StringComparison.CurrentCultureIgnoreCase))
                    {
                        repeatPart = repeatPart.Substring(0, repeatPart.Length - 1);
                        int timeInterval = int.Parse(repeatPart);
                        aRepeter.Duration = TimeSpan.FromSeconds(timeInterval);
                        aRepeter.IsTimeBase = true;
                    }

                    else // repeat with variable
                    {
                        repeatCount = -1;
                        if (Regex.IsMatch(line, pattern))
                        {
                            aRepeter.CountArgument = Regex.Match(line, pattern).Value;
                        }
                    }
                }

                aRepeter.OriginalStatement = line;
                aRepeter.IsAlwaysPass = ignoreErrorFail;

                if (currentBlock == null)
                {
                    aTestCase.AddSubBlock(aRepeter);
                }
                else
                {
                    currentBlock.AddSubBlock(aRepeter);
                }
                aRepeter.RepeatCount = repeatCount;
                currentBlock = aRepeter;
            }

            else if (formattedLine.StartsWith("For:", StringComparison.CurrentCultureIgnoreCase)) // for loop
            {
                ForBlock forBlock = new ForBlock();
                forBlock.IsAlwaysPass = ignoreErrorFail;
                forBlock.OriginalStatement = line;
                forBlock.OriginalStatement = line.Split(':')[1].Trim();
                forBlock.IsAlwaysPass = ignoreErrorFail;

                if (currentBlock == null)
                {
                    aTestCase.AddSubBlock(forBlock);
                }
                else
                {
                    currentBlock.AddSubBlock(forBlock);
                }
                currentBlock = forBlock;
            }

            else if (formattedLine.StartsWith("While:", StringComparison.CurrentCultureIgnoreCase)) // While Loop
            {
                WhileBlock aWhileBlock = new WhileBlock();
                aWhileBlock.IsAlwaysPass = ignoreErrorFail;
                aWhileBlock.OriginalStatement = line;
                string[] splitted = line.Split(':');
                string condition;
                int maxLoop = 0;
                if (int.TryParse(splitted.Last(), out maxLoop))
                {
                    aWhileBlock.MaxLoop = maxLoop;
                    condition = line.Replace(splitted[0], string.Empty).Replace(splitted.Last(), string.Empty).Trim().Trim(':');
                }
                else if (splitted.Length > 2 && Regex.IsMatch(splitted.Last(), pattern))
                {
                    aWhileBlock.MaxLoop = -1;
                    condition = line.Replace(splitted[0], string.Empty).Replace(splitted.Last(), string.Empty).Trim().Trim(':');
                    aWhileBlock.CountArgument = Regex.Match(splitted.Last(), pattern).Value;
                }
                else
                {
                    condition = line.Replace(splitted[0], string.Empty).Trim().Trim(':');
                }

                if (condition.StartsWith("!"))
                {
                    aWhileBlock.IsTrueLoop = false;
                    condition = condition.TrimStart('!');
                }
                aWhileBlock.Condition = ParseStatemet(condition, lineNumber); Statement aStatement = ParseStatemet(line);


                if (currentBlock == null)
                {
                    aTestCase.AddSubBlock(aWhileBlock);
                }
                else
                {
                    currentBlock.AddSubBlock(aWhileBlock);
                }
                currentBlock = aWhileBlock;
            }

            else if (formattedLine.StartsWith("ForEachRow:", StringComparison.CurrentCultureIgnoreCase)) // ForEachRow Block
            {
                ForEachRowBlock aFERBlock = new ForEachRowBlock();
                aFERBlock.IsAlwaysPass = ignoreErrorFail;
                aFERBlock.OriginalStatement = line;

                string dataSet = line.Split(':')[1];
                string[] splitted = dataSet.Split('.');
                if (splitted.Length != 3)
                {
                    throw new InvalidDataException("DataSet name, sheet name and table name must be given in For Each Row");
                }
                aFERBlock.DataSetName = splitted[0];
                aFERBlock.SheetName = splitted[1];
                aFERBlock.TableName = splitted[2];

                if (currentBlock == null)
                {
                    aTestCase.AddSubBlock(aFERBlock);
                }
                else
                {
                    currentBlock.AddSubBlock(aFERBlock);
                }
                currentBlock = aFERBlock;
            }
            else if (formattedLine.StartsWith("SpreadSheetForEachRow:", StringComparison.CurrentCultureIgnoreCase)) // SpreadSheetForEachRow Block
            {
                SpreadSheetForEachRowBlock SSFERBlock = new SpreadSheetForEachRowBlock();
                SSFERBlock.IsAlwaysPass = ignoreErrorFail;
                SSFERBlock.OriginalStatement = line;

                string dataSet = line.Split(':')[1];
                string[] splitted = dataSet.Split('.');
                if (splitted.Length != 3)
                {
                    throw new InvalidDataException("DataSet name, sheet name and table name must be given in Spread Sheet For Each Row");
                }
                SSFERBlock.DataSetName = splitted[0];
                SSFERBlock.SheetName = splitted[1];
                SSFERBlock.TableName = splitted[2];

                if (currentBlock == null)
                {
                    aTestCase.AddSubBlock(SSFERBlock);
                }
                else
                {
                    currentBlock.AddSubBlock(SSFERBlock);
                }
                currentBlock = SSFERBlock;
            }

            else if (formattedLine.StartsWith("ForSelectedRow:", StringComparison.CurrentCultureIgnoreCase))
            {
                ForSelectedRowBlock aFSRBlock = new ForSelectedRowBlock();
                aFSRBlock.IsAlwaysPass = ignoreErrorFail;
                aFSRBlock.OriginalStatement = line;

                string dataSet = line.Split(':')[1];
                string[] splitted = dataSet.Split('.');
                if (splitted.Length < 3)
                {
                    throw new InvalidDataException("DataSet name, sheet name, and table name must be given in For Each Row");
                }

                aFSRBlock.DataSetName = splitted[0];
                aFSRBlock.SheetName = splitted[1];
                aFSRBlock.TableName = splitted[2];

                // Parse filter criteria if provided
                if (splitted.Length == 4)
                {
                    string condtion = splitted[3];

                    aFSRBlock.ColumnFilter = new Dictionary<string, List<(string Operator, string Value)>>();

                    string[] conditionalOperators= new[] { ">=", "<=", "!=" ,"=", ">", "<" };

                        string[] parts = condtion.Split(conditionalOperators, StringSplitOptions.None);
                        if (parts.Length == 2)
                        {
                            string column = parts[0].Trim();
                            string conditionOperator = null;

                            // Inline operator extraction
                            if (condtion.Contains(">=")) conditionOperator = ">=";
                            else if (condtion.Contains("<=")) conditionOperator = "<=";
                            else if (condtion.Contains("!=")) conditionOperator = "!=";
                            else if (condtion.Contains(">")) conditionOperator = ">";
                            else if (condtion.Contains("<")) conditionOperator = "<";
                            else conditionOperator = "=="; // Default to equality if no operator found

                            string[] values = parts[1].Split(',').Select(v => v.Trim()).ToArray();

                            var operatorValueList = values.Select(v => (conditionOperator, v)).ToList();
                            if (aFSRBlock.ColumnFilter.ContainsKey(column))
                            {
                                aFSRBlock.ColumnFilter[column].AddRange(operatorValueList);
                            }
                            else
                            {
                                aFSRBlock.ColumnFilter[column] = operatorValueList;
                            }
                        }
                    
                }



                if (currentBlock == null)
                {
                    aTestCase.AddSubBlock(aFSRBlock);
                }
                else
                {
                    currentBlock.AddSubBlock(aFSRBlock);
                }
                currentBlock = aFSRBlock;
            }

            else if (formattedLine.StartsWith("RemoteRun:", StringComparison.CurrentCultureIgnoreCase)) // Remote exeuction
            {
                RemoteBlock aRemoteBlock = new RemoteBlock();
                aRemoteBlock.IsAlwaysPass = ignoreErrorFail;
                aRemoteBlock.OriginalStatement = line;

                string[] splitted = line.Split(':');
                string remoteId = splitted.Last().Trim();
                aRemoteBlock.RemoteId = remoteId;

                if (currentBlock == null)
                {
                    aTestCase.AddSubBlock(aRemoteBlock);
                }
                else
                {
                    currentBlock.AddSubBlock(aRemoteBlock);
                }
                currentBlock = aRemoteBlock;
            }

            else if (formattedLine.StartsWith("If:", StringComparison.CurrentCultureIgnoreCase)) // Condition block if
            {
                IfBlock aCondition = new IfBlock();
                aCondition.IsAlwaysPass = ignoreErrorFail;
                aCondition.OriginalStatement = line;

                string[] splitted = line.Split(':');
                string condition = line.Replace(splitted[0], string.Empty).Trim().Trim(':');
                aCondition.Condition = ParseStatemet(condition);
                if (currentBlock == null)
                {
                    aTestCase.AddSubBlock(aCondition);
                }
                else
                {
                    currentBlock.AddSubBlock(aCondition);
                }
                ifBlocks[depth] = aCondition;
                CommonBlock passBlock = new CommonBlock();
                aCondition.StatementPass = passBlock;
                currentBlock = passBlock;

            }

            else if (formattedLine.StartsWith("Fail:", StringComparison.CurrentCultureIgnoreCase)) // Condition block fail
            {
                if (!ifBlocks.ContainsKey(depth))
                {
                    throw new InvalidDataException("Fail block should be declared after If statement" + "\r\n TeseCase or Method Name=" + aTestCase.Name + "\r\n Line=" + line);
                }
                IfBlock aCondition = ifBlocks[depth];
                if (aCondition?.StatementFail != null)
                {
                    throw new InvalidDataException("Fail block is already declared" + "\r\nTeseCase or Method Name=" + aTestCase.Name + "\r\n Line=" + line);
                }

                CommonBlock failBlock = new CommonBlock();
                failBlock.IsAlwaysPass = ignoreErrorFail;
                failBlock.OriginalStatement = line;

                aCondition.StatementFail = failBlock;
                currentBlock = failBlock;
            }

            else if (formattedLine.StartsWith("Error:", StringComparison.CurrentCultureIgnoreCase)) // Condition block error
            {
                if (!ifBlocks.ContainsKey(depth))
                {
                    throw new InvalidDataException("Error block should be declared after If statement" + "\r\nTeseCase or Method Name=" + aTestCase.Name + "\r\n Line=" + line);
                }
                IfBlock aCondition = ifBlocks[depth];
                if (aCondition?.StatementError != null)
                {
                    throw new InvalidDataException("Error block is already declared" + "\r\nTeseCase or Method Name=" + aTestCase.Name + "\r\n Line=" + line);
                }

                CommonBlock errorBlock = new CommonBlock();
                errorBlock.IsAlwaysPass = ignoreErrorFail;
                errorBlock.OriginalStatement = line;

                aCondition.StatementError = errorBlock;
                currentBlock = errorBlock;
            }

            else // Statement
            {
                Statement aStatement = ParseStatemet(line,lineNumber);
                if (currentBlock == null)
                {
                    aTestCase.AddSubBlock(aStatement);
                }
                else
                {
                    currentBlock.AddSubBlock(aStatement);
                }
            }
            return currentBlock;
        }

        public static void
            ParseUsing(string line, string tsPath, TestDataManager testDataManager, Dictionary<string, string> variables = null, Dictionary<string, string> executionVariables = null)
        {
            if (testDataManager == null)
            {
                throw new ArgumentNullException(nameof(testDataManager),
                    "ParseUsing was called with a null TestDataManager. This previously surfaced as a " +
                    "NullReferenceException around the DEFAULT_DUT fallback further down and, if silently " +
                    "swallowed by the caller, could leave stale test-case selection state in place for the " +
                    "next run.");
            }

            string tmpStr = line;

           if(CultureInfo.CurrentCulture.Parent.CultureTypes.ToString()=="tr")
            {
                tmpStr = EncodingDecodingLine(line);
            }

            tmpStr = line.ToUpperInvariant();

            string dutID = string.Empty;
            bool isVariableFile = false;
            Dictionary<string, Library> availableLibs = LibraryUtils.GetAvailableLibraries();

            // using xxx as yyy with ddd at kkk
            if(tmpStr.Contains(" AT "))
            {
                Regex regex = new Regex(@"\sAT\s[^\s]+");
                string remoteInfo = regex.Match(tmpStr).Value;
                string[] splitted = remoteInfo.Split(' ');
                string remoteId = splitted.Last().Trim();
                remoteId = line.Substring(line.IndexOf(remoteId, StringComparison.CurrentCultureIgnoreCase), remoteId.Length);

                RemoteExecutor remoteExecutor;
                if(testDataManager.RemoteExecutors.ContainsKey(remoteId))
                {
                    remoteExecutor = testDataManager.RemoteExecutors[remoteId];
                }
                else
                {
                    remoteExecutor = new RemoteExecutor(remoteId);
                    testDataManager.RemoteExecutors.Add(remoteId, remoteExecutor);
                }
                TestDataManager remoteTestDataManager = remoteExecutor.RemoteTestDataManager;
                string originalRemoteInfo = line.Substring(line.IndexOf(remoteInfo, StringComparison.CurrentCultureIgnoreCase), remoteInfo.Length);
                string processedLine = line.Replace(originalRemoteInfo, string.Empty);
                ParseUsing(processedLine, tsPath, remoteTestDataManager, variables, executionVariables);
                remoteExecutor.Usings.Add(processedLine);
            }

            if (tmpStr.Contains(" WITH "))
            {
                Regex regex = new Regex(@"\sWITH\s[^\s]+");
                string withInfo = regex.Match(tmpStr).Value;
                string[] splitted = withInfo.Split(' ');
                string deviceId = splitted.Last().Trim();
                deviceId = line.Substring(line.IndexOf(deviceId, StringComparison.CurrentCultureIgnoreCase), deviceId.Length);
                if (!testDataManager.GetUsedDevice().Contains(deviceId))
                {
                    testDataManager.DutDictionary.Add(deviceId, null);
                }
                tmpStr = tmpStr.Substring(0, tmpStr.IndexOf("WITH")).Trim();
                dutID = deviceId;
            }
            string libAs = string.Empty;
            if (tmpStr.Contains(" AS "))
            {
                string[] splitted = tmpStr.Split(' ');
                libAs = splitted.Last().Trim();
                libAs = line.Substring(line.IndexOf(libAs, StringComparison.CurrentCultureIgnoreCase), libAs.Length);
                tmpStr = tmpStr.Substring(0, tmpStr.IndexOf(" AS ")).Trim();
            }

            tmpStr = line.Substring(line.IndexOf(tmpStr, StringComparison.OrdinalIgnoreCase), tmpStr.Length);
            string lib = tmpStr.Split(new char[] { ' ' }, 2)[1].Trim();
            string libPath = Keywords.Support.Utils.GetAbsolutePath(lib, Path.GetDirectoryName(tsPath));
            lib = Path.GetFileName(libPath);

            if (lib.EndsWith(".gflib"))
            {
                testDataManager.FilesToUse.Add(libPath);
                CustomLibrary customLibrary = new CustomLibrary(libPath, executionVariables);

                testDataManager.FilesToUse.AddRange(customLibrary.FilesToUse);
                testDataManager.Resources = testDataManager.Resources.Concat(customLibrary.Resources.Where(x => !testDataManager.Resources.Keys.Contains(x.Key))).ToDictionary(k => k.Key, v => v.Value);

                LibraryUtils.AddCustomLibraryToAvailableLibraries(customLibrary);
                availableLibs = LibraryUtils.GetAvailableLibraries();
                lib = lib.Replace(".gflib", string.Empty);
            }

            if (lib.EndsWith(".gfvar"))
            {
                testDataManager.FilesToUse.Add(libPath);
                if(variables != null && File.Exists(libPath))
                {
                    ParseVariableFile(libPath, variables);
                }

                isVariableFile = true;
            }



            if (string.IsNullOrEmpty(libAs))
            {
                libAs = lib;
            }

            if (string.IsNullOrEmpty(dutID))
            {
                dutID = testDataManager.DEFAULT_DUT ?? string.Empty;
            }

            if (!isVariableFile)
            {
                if (!testDataManager.DeviceLibraryMapping.ContainsKey(dutID))
                {
                    testDataManager.DeviceLibraryMapping[dutID] = new List<Library>();
                }

                bool existing = false;
                if (testDataManager.DeviceLibraryMapping[dutID].Where(s => s.NameAs.Equals(libAs, StringComparison.CurrentCultureIgnoreCase)).Any())
                {
                    existing = true;
                }
                if (!existing && availableLibs.ContainsKey(lib))
                {
                    Library toAdd = availableLibs[lib].Clone();
                    toAdd.NameAs = libAs;
                    testDataManager.DeviceLibraryMapping[dutID].Add(toAdd);
                }


            }
        }

        public static string EncodingDecodingLine(string line)
        {
            //--------------------------------------------------------------------------------------------------
            Encoding srcEncoding = Encoding.UTF8;
            Encoding destEncoding = Encoding.GetEncoding(1252); // Latin alphabet

            return destEncoding.GetString(Encoding.Convert(srcEncoding, destEncoding, srcEncoding.GetBytes(line)));

            //--------------------------------------------------------------------------------------------------------
        }

        public static void ParseVariableFile(string varPath, Dictionary<string, string> variables)
        {
            Logger.Trace($"Parsing Variable File : {varPath}");
            using (StreamReader reader = new StreamReader(varPath))
            {
                string pattern = @"^\$\{[0-9a-zA-Z_-]+\}";
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    // Variable parsing
                    if (Regex.IsMatch(line, pattern))
                    {
                        string var = line.Split('=')[0].Trim();
                        string value = line.Split(new char[] { '=' }, 2)[1].Trim();
                        variables[var]= value;
                    }
                }
            }
        }

        public static void ParseResource(string line, string tsPath, TestDataManager testDataManager)
        {
            string tmpStr = line.ToUpper();
            string resName = "NoName";
            if (tmpStr.Contains(" AS "))
            {
                string[] splitted = tmpStr.Split(' ');
                resName = splitted.Last().Trim();
                resName = line.Substring(line.IndexOf($" AS {resName}", StringComparison.CurrentCultureIgnoreCase) + 4, resName.Length);
                tmpStr = tmpStr.Substring(0, tmpStr.IndexOf(" AS ")).Trim();
            }
            else
            {
                Logger.Error("Resource file does not have name.");
                return;
            }


            string resPath = tmpStr.Split(' ')[0].Trim();
            resPath = tmpStr.Replace(resPath, string.Empty).Trim();
            resPath = line.Substring(line.IndexOf(resPath, StringComparison.CurrentCultureIgnoreCase), resPath.Length);

            if (string.IsNullOrEmpty(Path.GetPathRoot(resPath)))
            {
                string strResPath = Path.Combine(Path.GetDirectoryName(tsPath), resPath);
                Uri uri = new Uri(strResPath);
                resPath = Path.GetFullPath(uri.LocalPath);
            }

            testDataManager.Resources[resName] = resPath;
            testDataManager.FilesToUse.Add(resPath);
        }

        public static void ParseDataset(string line, string tsPath, TestDataManager testDataManager)
        {
            string tmpStr = line.ToUpper();
            string dataSetName = "NoName";
            if (tmpStr.Contains(" AS "))
            {
                string[] splitted = tmpStr.Split(' ');
                dataSetName = splitted.Last().Trim();
                dataSetName = line.Substring(line.IndexOf($" AS {dataSetName}", StringComparison.CurrentCultureIgnoreCase) + 4, dataSetName.Length);
                tmpStr = tmpStr.Substring(0, tmpStr.IndexOf(" AS ")).Trim();
            }
            else
            {
                Logger.Error("Dataset does not have name.");
                return;
            }


            string dataSetPath = tmpStr.Split(' ')[0].Trim();
            dataSetPath = tmpStr.Replace(dataSetPath, string.Empty).Trim();
            dataSetPath = line.Substring(line.IndexOf(dataSetPath, StringComparison.CurrentCultureIgnoreCase), dataSetPath.Length);

            if (string.IsNullOrEmpty(Path.GetPathRoot(dataSetPath)))
            {
                string strResPath = Path.Combine(Path.GetDirectoryName(tsPath), dataSetPath);
                Uri uri = new Uri(strResPath);
                dataSetPath = Path.GetFullPath(uri.LocalPath);
            }

            testDataManager.DataSets[dataSetName] = dataSetPath;
            testDataManager.FilesToUse.Add(dataSetPath);
        }

        public static Statement ParseStatemet(string line, int lineNumber = 0)
        {
            Statement aStatement = new Statement();
            string keyword = null;
            List<string> arguments = new List<string>();

            aStatement.OriginalStatement = line;
            aStatement.LineNumber = lineNumber;

            if (line.StartsWith("@"))
            {
                aStatement.IsAlwaysPass = true;
                line = line.TrimStart('@');
            }

            // Remove unnessary "()"
            if(line.TrimEnd().EndsWith("()"))
            {
                line = line.Substring(0, line.LastIndexOf("()"));
            }
            // Handling Escape Character
            // Remove Last )
            line += "@@@END@@@";
            line = line.Replace(")@@@END@@@", string.Empty);
            line = line.Replace("@@@END@@@", string.Empty);
            line = line.Replace(@"\,", "@@COMMA@@");
            line = line.Replace(@"\(", "@@LEFTBRACKET@@");
            line = line.Replace(@"\)", "@@RIGHTBRACKET@@");

            if (line.Contains('('))
            {
                string[] splitted = line.Split(new char[] { '(' }, 2);
                string tmpArgs;
                keyword = splitted[0].Trim().Replace(" ", "").ToUpper();
                tmpArgs = splitted[1];

                // Handling Escape Character
                tmpArgs = tmpArgs.Replace(@"\,", "@@COMMA@@");
                tmpArgs = tmpArgs.Replace("@@LEFTBRACKET@@", "(");
                tmpArgs = tmpArgs.Replace("@@RIGHTBRACKET@@", ")");


                foreach (string arg in tmpArgs.Split(','))
                {
                    arguments.Add(arg.Replace("@@COMMA@@", ","));
                }
                aStatement.Keyword = keyword;
                aStatement.Parameters = arguments.ToArray();
            }
            else
            {
                keyword = line.Trim().Replace(" ", "").ToUpper();
                aStatement.Keyword = keyword;
                aStatement.Parameters = null;
            }

            if(aStatement.Keyword.Split('.').Count() == 1)
            {
                aStatement.Library = "BuiltIn";
            }
            else
            {
                aStatement.Library = aStatement.Keyword.Split('.')[0];
                aStatement.Keyword = aStatement.Keyword.Split('.')[1];
            }

            return aStatement;
        }
    }
}
