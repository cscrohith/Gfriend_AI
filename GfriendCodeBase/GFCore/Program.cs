using HP.GFriend.Core.Execution;
using HP.GFriend.Core.Execution.Spec;
using HP.GFriend.GFLogger;
using HP.GFriend.Keywords;
using HP.GFriend.Support;
using Microsoft.Office.Interop.Excel;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HP.GFriend.Core
{
    class Program
    {
        private static string _libfilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "libs");
        private static TestDataManager _testDataManager;

        [STAThread]
        static void Main(string[] args)
        {
            string tsPath = null;
            string ipAddress = null;
            string outputDir = null;
            string testCases = null;
            string outputXml = null;
            string adminId = null;
            string adminPw = null;
            string lanDebug = null;
            string deviceType = null;
            string deviceSpec = null;
            string appiumPort = null;
            string platformOrder = null;
            string testRunSpec = null;
            bool generateDoc = false;
            bool headlessMode = false;
            string version = null;
            bool dryrun = false;
            bool merge = false;

            List<string> tcToRun = null;

            // Execution Variables
            string executionVariableFile = null;
            Dictionary<string, string> executionVariables = new Dictionary<string, string>();

            var options = new OptionSet()
            {
                {"t|testsuite=","{TestSuite} to run",v => tsPath = v},
                {"c|testcase=","{TestCase} to run (surrounded with double quote (\"), comma sperated)",v => testCases = v},
                {"i|ipaddress=","{IPAddress} of target. For multi-device execution, separate multiple values with a pipe ('|'), e.g. \"192.168.1.1|192.168.1.2\"",v => ipAddress = v},
                {"a|adminId=","{Admin ID} of target. For multi-device execution, provide a pipe ('|') separated list matching the order of --ipaddress",v => adminId = v},
                {"p|adminPw=","{Admin PW} of target. For multi-device execution, provide a pipe ('|') separated list matching the order of --ipaddress",v => adminPw = v},
                {"l|landebug=","{LAN Debug Address} of target. For multi-device execution, provide a pipe ('|') separated list matching the order of --ipaddress",v => lanDebug = v},
                {"dt|deviceType=","{DeviceType}/platform of target (e.g. Dune, Jedi). Also used as DeviceId when one isn't otherwise specified. For multi-device execution, provide a pipe ('|') separated list matching the order of --ipaddress",v => deviceType = v},
                {"pt|port=","Appium server {Port} of target (required for Appium-based platforms such as Mac/iOS, e.g. 4723). For multi-device execution, provide a pipe ('|') separated list matching the order of --ipaddress",v => appiumPort = v},
                {"d|device=","Recommended for multi-device execution: one self-contained {DeviceSpec} per device, in the form \"IPAddress:AdminId:AdminPassword:LanDebugAddress:DeviceType:Port\" (trailing fields may be omitted, e.g. \"IPAddress:AdminId:AdminPassword\"). Separate multiple devices with a pipe ('|'), e.g. \"146.205.5.38:rdl:rdl@12345:: Dune:4723|146.205.4.221:rdl:135792468::Jedi\". When provided, this takes precedence over --ipaddress/--adminId/--adminPw/--landebug/--deviceType/--port and avoids any risk of misaligning IP/password across separate pipe-delimited lists.",v => deviceSpec = v},
                {"po|platformOrder=","Order in which configured devices are registered/initialized at startup, as a comma or pipe separated list of platform/device types (e.g. \"Jedi,Dune,Mac\"). Devices whose DeviceType is not listed are initialized last, in their original order. This only affects device registration/connection order; keyword execution still follows the order written in the script.",v => platformOrder = v},
                {"f|outputToFix=","{output.xml Path} to fix",v => outputXml = v},
                {"o|output=","{Output} folder of result",v => outputDir = v},
                {"s|testRunSpec=","{TestRunSpec} xml file",v => testRunSpec = v},
                {"k|keywordDoc", "Generate Keyword Documentation", v => {generateDoc = true; } },
                {"h|headless", "Running on headless environment", v => {headlessMode = true; } },
                {"v|variableFile=","{Path} for this execution variable file",v => executionVariableFile = v},
                {"e={:}",$"Execuation variables with VariableName:VariableValue.", (n,v) =>
                    {
                        string varName = n;
                        if(!n.StartsWith("${") && !n.EndsWith("}"))
                        {
                            varName = "${" + n + "}";
                        }

                        executionVariables[varName] = v;
                    }
                },
                {"ver|version", "Gfriend version", v => { version = Assembly.GetExecutingAssembly().GetName().Version.ToString(); } },
                {"dr|dryrun","GFriend dry run",v=>{ dryrun=true; } },
                {"m|merge","Merge the output files and generate a single report file",v=>{ merge=true; } },
            };
            List<string> extra;
            try
            {
                extra = options.Parse(args);
            }
            catch (OptionException)
            {
                Console.WriteLine("Usage:");
                options.WriteOptionDescriptions(Console.Out);
                Environment.Exit(1);
            }

            // Guard against stray leading/trailing whitespace in argument values (e.g. a script
            // or wrapper accidentally injecting a space before a path), which would otherwise
            // cause a FileNotFoundException when combined into a path like "...\scripts\ file.txt".
            tsPath = tsPath?.Trim();
            ipAddress = ipAddress?.Trim();
            outputDir = outputDir?.Trim();
            testCases = testCases?.Trim();
            outputXml = outputXml?.Trim();
            adminId = adminId?.Trim();
            adminPw = adminPw?.Trim();
            lanDebug = lanDebug?.Trim();
            deviceType = deviceType?.Trim();
            deviceSpec = deviceSpec?.Trim();
            appiumPort = appiumPort?.Trim();
            platformOrder = platformOrder?.Trim();
            testRunSpec = testRunSpec?.Trim();

            if (merge)
            {
                if (tsPath == null || outputDir == null)
                {
                    Console.WriteLine("Please mention the folder name / output folder to merge the output files into a single report file");
                    return;
                }
                else
                {
                    //Utils.MergeOutputFilesAndGenerateReport(tsPath, outputDir);
                    Runner.HandleMergeAndReportGeneration(tsPath, outputDir);
                    return;
                }
            }


            if (dryrun)
            {
                CommonExecutionInfo.Initialize();
                Utils.DryrunProcess(tsPath, outputDir);
                return;
            }

            if (version != null)
            {
                Console.WriteLine("GFriend version is: " + version);
                return;
            }
            if (generateDoc)
            {

                foreach (KeyValuePair<string, Library> libDic in LibraryUtils.GetAvailableLibraries())
                {
                    Console.WriteLine($"Generating Keyword Documentation :: {libDic.Key}");
                    libDic.Value.Load(libDic.Value.Name);
                    libDic.Value.GenerateKeywordDocumentation(Environment.CurrentDirectory, $"GF_Keywords_{libDic.Key}.html");
                }
                return;
            }

            if (outputXml != null)
            {
                Reporter.FixXmlOutput(outputXml);
                return;
            }

            if (executionVariableFile != null)
            {
                Parser.ParseVariableFile(executionVariableFile, executionVariables);
            }


            if (testRunSpec != null)
            {
                RunSpecRunner.Run(testRunSpec, headlessMode, _testDataManager, executionVariables);
                return;
            }

            if (ipAddress == null && deviceSpec == null)
            {
                Console.WriteLine("Error : Please specify IP Address of target (--ipaddress) or a --device specification\n\nUsage:");
                options.WriteOptionDescriptions(Console.Out);
                Environment.Exit(1);
            }

            if (tsPath != null && outputDir == null)
            {                
                Console.WriteLine("Error : Please Specify Output Folder of the test\n\nUsage:");
                options.WriteOptionDescriptions(Console.Out);
                Environment.Exit(1);
            }

            if (testCases != null)
            {
                Console.WriteLine(testCases);
                tcToRun = testCases.Split(',').ToList<string>();
            }

            // Multi-device support: build one DeviceUnderTest per device, either from the
            // self-contained --device specification (preferred, no cross-option alignment risk)
            // or, for backward compatibility, from -i/-a/-p/-l/-dt pipe ('|') separated lists
            // mapped by index. If fewer values are supplied for a given legacy option than for
            // -i, the last available value is reused (or ignored if none was supplied) so a
            // single value still applies uniformly to every device.
            List<DeviceUnderTest> devices = new List<DeviceUnderTest>();

            if (deviceSpec != null)
            {
                // "-d" format: one pipe ('|') separated entry per device, each entry being
                // "IPAddress:AdminId:AdminPassword:LanDebugAddress:DeviceType" (trailing fields
                // may be omitted). This keeps every device's fields together in a single token,
                // so there is no possibility of an IP being paired with the wrong password.
                string[] deviceSpecs = deviceSpec.Split('|').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
                foreach (string spec in deviceSpecs)
                {
                    string[] fields = spec.Split(':');
                    string specIp = fields.Length > 0 ? fields[0].Trim() : null;
                    if (string.IsNullOrEmpty(specIp))
                    {
                        Console.WriteLine($"Error : Invalid --device entry (missing IP address): \"{spec}\"");
                        Environment.Exit(1);
                    }

                    DeviceUnderTest specDut = new DeviceUnderTest(specIp);
                    if (fields.Length > 1 && !string.IsNullOrEmpty(fields[1].Trim()))
                        specDut.AdminId = fields[1].Trim();
                    if (fields.Length > 2 && !string.IsNullOrEmpty(fields[2].Trim()))
                        specDut.AdminPassword = fields[2].Trim();
                    if (fields.Length > 3 && !string.IsNullOrEmpty(fields[3].Trim()))
                        specDut.LanDebugAddress = fields[3].Trim();
                    if (fields.Length > 4 && !string.IsNullOrEmpty(fields[4].Trim()))
                    {
                        specDut.DeviceType = fields[4].Trim();
                        if (string.IsNullOrEmpty(specDut.DeviceId))
                        {
                            specDut.DeviceId = specDut.DeviceType;
                        }
                    }
                    if (fields.Length > 5 && !string.IsNullOrEmpty(fields[5].Trim()))
                    {
                        if (int.TryParse(fields[5].Trim(), out int specPort))
                        {
                            specDut.Port = specPort;
                        }
                        else
                        {
                            Console.WriteLine($"Error : Invalid --device entry (Port must be a number): \"{spec}\"");
                            Environment.Exit(1);
                        }
                    }

                    devices.Add(specDut);
                }
            }
            else
            {
                string[] ipList = ipAddress.Split('|').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
                string[] adminIdList = SplitOrEmpty(adminId);
                string[] adminPwList = SplitOrEmpty(adminPw);
                string[] lanDebugList = SplitOrEmpty(lanDebug);
                string[] deviceTypeList = SplitOrEmpty(deviceType);
                string[] portList = SplitOrEmpty(appiumPort);

                for (int i = 0; i < ipList.Length; i++)
                {
                    DeviceUnderTest listDut = new DeviceUnderTest(ipList[i]);

                    string mappedAdminId = GetMappedValue(adminIdList, i);
                    if (mappedAdminId != null)
                    {
                        listDut.AdminId = mappedAdminId;
                    }

                    string mappedAdminPw = GetMappedValue(adminPwList, i);
                    if (mappedAdminPw != null)
                    {
                        listDut.AdminPassword = mappedAdminPw;
                    }

                    string mappedLanDebug = GetMappedValue(lanDebugList, i);
                    if (mappedLanDebug != null)
                    {
                        listDut.LanDebugAddress = mappedLanDebug;
                    }

                    string mappedDeviceType = GetMappedValue(deviceTypeList, i);
                    if (mappedDeviceType != null)
                    {
                        listDut.DeviceType = mappedDeviceType;
                        if (string.IsNullOrEmpty(listDut.DeviceId))
                        {
                            listDut.DeviceId = mappedDeviceType;
                        }
                    }

                    string mappedPort = GetMappedValue(portList, i);
                    if (mappedPort != null)
                    {
                        if (int.TryParse(mappedPort, out int listPort))
                        {
                            listDut.Port = listPort;
                        }
                        else
                        {
                            Console.WriteLine($"Error : Invalid --port value (must be a number): \"{mappedPort}\"");
                            Environment.Exit(1);
                        }
                    }

                    devices.Add(listDut);
                }
            }

            // Optionally reorder the registered devices by platform/DeviceType (e.g. so that
            // connection/initialization is attempted for Jedi first, then Dune, then Mac, etc.).
            // This only controls the order devices are registered/connected in; it has no effect
            // on the order keywords execute in the script, since keyword-to-device routing is
            // resolved independently per keyword (see Executor.ResolveLibraryDut/EnsureLibraryInitialized).
            if (!string.IsNullOrEmpty(platformOrder))
            {
                string[] orderedPlatforms = platformOrder
                    .Split(new[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToArray();

                devices = devices
                    .OrderBy(d =>
                    {
                        int idx = Array.FindIndex(orderedPlatforms, p =>
                            string.Equals(p, d.DeviceType, StringComparison.OrdinalIgnoreCase));
                        return idx < 0 ? int.MaxValue : idx;
                    })
                    .ToList();

                Console.WriteLine($"===== Device registration order set to: {string.Join(" -> ", orderedPlatforms)} =====");
            }

            // IMPORTANT: The script is parsed and executed exactly ONCE, with every configured
            // device passed to the Executor together. Test cases within the script are not tied
            // to a single device loop iteration; instead, each keyword's target device/platform
            // is resolved independently at execution time by Executor.ResolveLibraryDut(...) and
            // Executor.EnsureLibraryInitialized(...), based on the "using <Library> with <DeviceId>"
            // clause (or platform/DeviceType match). This allows a single script containing test
            // cases for multiple platforms (e.g. Dune, Jedi, Mac) to run each test case against
            // its own correct device, without one device's state/connection ever being reused or
            // overwritten by another device's run.
            foreach (var d in devices)
            {
                Console.WriteLine($"===== Registered device :: Id={d.DeviceId} Type={d.DeviceType} IP={d.DeviceAddress} =====");
            }

            CommonExecutionInfo.Initialize();

            Runner.InitGFRunner(devices, outputDir, headlessMode);

            if (testCases == null)
            {
                Runner.Run(tsPath, out _testDataManager, null, _libfilePath, null, null, executionVariables);
            }
            else
            {
                Runner.Run(tsPath, out _testDataManager, tcToRun, _libfilePath, null, null, executionVariables);
            }

            bool anyFailure = _testDataManager.TargetTestSuite.Result == "FAIL" || _testDataManager.TargetTestSuite.Result == "ERROR";

            Environment.Exit(anyFailure ? 1 : 0);
        }

        /// <summary>
        /// Splits a pipe ('|') delimited option value into a trimmed array, or returns an empty
        /// array when the raw value is null/empty.
        /// </summary>
        private static string[] SplitOrEmpty(string rawValue)
        {
            if (string.IsNullOrEmpty(rawValue))
            {
                return new string[0];
            }
            return rawValue.Split('|').Select(s => s.Trim()).ToArray();
        }

        /// <summary>
        /// Returns the value at <paramref name="index"/> from <paramref name="values"/> when
        /// present, or the last available value when the option was supplied with fewer entries
        /// than the ip address list (so a single value applies to every device), or null when no
        /// values were supplied at all.
        /// </summary>
        private static string GetMappedValue(string[] values, int index)
        {
            if (values == null || values.Length == 0)
            {
                return null;
            }
            return index < values.Length ? values[index] : values[values.Length - 1];
        }
    }
}
