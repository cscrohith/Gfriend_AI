using HP.GFriend.Keywords;
using CoreCommonInfo = HP.GFriend.Core.CommonExecutionInfo;
using LegacyCommonInfo = HP.GFriend.Keywords.CommonExecutionInfo;
using HP.GFriend.GFLogger;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using HP.GFriend.Core.Custom;
using System.Text.RegularExpressions;
using HP.GFriend.Core.Remote;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace HP.GFriend.Core.Execution
{
    public class Executor : IDisposable
    {
        private static readonly Regex VarRegex = new Regex(@"\$\{[a-zA-Z0-9_-]+\}", RegexOptions.Compiled);
        private static readonly string[] ByPassKeywords = { "Calculate And Assign", "ReAssign Value To Variable" };
        
        private readonly List<DeviceUnderTest> _duts;
        private TestDataManager _testDataManager;
        private string _outputDir;
        private string _tsPath;
        private bool _initialized;
        private bool _disposed;
        private bool disposedValue;
        private ExecutionContext _executionContext;
        private static bool TryGetRuntimeVariable(string variableName, out string value)
        {
            value = null;

            if (CoreCommonInfo.HasVariable(variableName))
            {
                value = CoreCommonInfo.GetVariable(variableName);
                return true;
            }
            if (LegacyCommonInfo.HasVariable(variableName))
            {
                value = LegacyCommonInfo.GetVariable(variableName);
                return true;
            }

            return false;
        }

        // Lazy-init: stores the resolved DUT for each device-mapping key.
        // Key = DeviceLibraryMapping key (device ID or NO_DUT / DEFAULT_DUT constant).
        // Value = (DUT to use for initialization, whether libs in this group are already initialized).
        private readonly Dictionary<string, (DeviceUnderTest Dut, bool LibsInitialized)> _dutAssignments
            = new Dictionary<string, (DeviceUnderTest, bool)>();

        // Lookup: Library.NameAs → device-mapping key, built during InitExecutor for fast EnsureLibraryInitialized calls.
        private readonly Dictionary<string, string> _libraryToDeviceKey
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Tracks libraries that failed to initialize so that subsequent keywords on the same
        // platform are reported as ERROR without stopping execution of other platforms.
        private readonly Dictionary<string, string> _libraryInitErrors
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Tracks the last device/platform combination for which the "Switching to device"
        // message was logged, so it is only shown once per switch rather than for every keyword.
        private string _lastActiveDeviceContext;

    public Executor(List<DeviceUnderTest> duts, TestDataManager testDataManager)
        {
            _duts = duts ?? throw new ArgumentNullException(nameof(duts));
            _testDataManager = testDataManager ?? throw new ArgumentNullException(nameof(testDataManager));
        }

        /// <summary>
        /// Gets the local machine's IP address from the network adapter.
        /// Returns the first active IPv4 address that is not a loopback address.
        /// </summary>
        /// <returns>Local IP address as string, or "127.0.0.1" if no network adapter found</returns>
        private static string GetLocalIPAddress()
        {
            try
            {
                // Get all network interfaces
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface networkInterface in interfaces)
                {
                    // Skip loopback and non-operational interfaces
                    if (networkInterface.OperationalStatus != OperationalStatus.Up ||
                        networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    {
                        continue;
                    }

                    // Get IP properties
                    IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();

                    // Get all unicast addresses
                    foreach (UnicastIPAddressInformation ip in ipProperties.UnicastAddresses)
                    {
                        // Return the first IPv4 address that is not loopback
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork &&
                            !IPAddress.IsLoopback(ip.Address))
                        {
                            Logger.Debug($"Detected local IP address: {ip.Address}");
                        }
                    }
                }

                // Fallback to localhost if no suitable IP found
                Logger.Warn("No network adapter found with active IPv4 address. Using localhost.");
                return "127.0.0.1";
            }
            catch (Exception ex)
            {
                Logger.Error($"Error detecting local IP address: {ex.Message}");
                return "127.0.0.1";
            }
        }

        public void InitExecutor(string outputDir = null, string tsPath = null)
        {
            if (_initialized)
                throw new InvalidOperationException("Executor already initialized");

            _outputDir = outputDir;
            _tsPath = tsPath;

            _executionContext = new ExecutionContext();
            Logger.Debug("ExecutionContext created for multi-device execution");

            CoreCommonInfo.Initialize();
            CoreCommonInfo.SetExecutionContext(_executionContext, PlatformType.Unknown);
            Logger.Debug("ExecutionContext wired into Core CommonExecutionInfo");

            foreach (KeyValuePair<string, List<Library>> kv in _testDataManager.DeviceLibraryMapping)
            {
                DeviceUnderTest targetDut = null;
                bool eagerInit; // true = initialize now; false = defer until first keyword use

                if (kv.Key.Equals(TestDataManager.NO_DUT))
                {
                    // Check if this is a Windows or Web library that needs a default DUT with local IP
                    bool needsLocalIP = kv.Value.Any(lib =>
                        lib.Name.Equals("Windows", StringComparison.OrdinalIgnoreCase) ||
                        lib.Name.Equals("Web",     StringComparison.OrdinalIgnoreCase));

                    if (needsLocalIP && (_duts == null || _duts.Count == 0))
                    {
                        string localIP = GetLocalIPAddress();
                        targetDut = new DeviceUnderTest(localIP)
                        {
                            DeviceId    = "LocalMachine",
                            Description = "Auto-generated local machine device",
                            DeviceType  = "Local"
                        };
                        Logger.Debug($"Created default DUT with local IP {localIP} for Windows/Web library execution");
                    }
                    else
                    {
                        targetDut = null;
                    }

                    // NO_DUT group (BuiltIn, Web, Windows) — initialize eagerly, they don't connect to physical devices
                    eagerInit = true;
                }
                else
                {
                    // DUT-requiring groups are initialized lazily, per library.
                    // DUT resolution is deferred to ResolveLibraryDut() so each library
                    // is independently matched to the correct platform device.
                    eagerInit = false;
                }

                if (eagerInit)
                {
                    // Eager path: initialize immediately (BuiltIn, no-DUT libs)
                    if (targetDut != null)
                    {
                        CoreCommonInfo.SetVariable(SystemVariables.DUT_ADDRESS, targetDut.DeviceAddress);
                        CoreCommonInfo.SetVariable(SystemVariables.DUT_ADMINID, targetDut.AdminId);
                        CoreCommonInfo.SetVariable(SystemVariables.DUT_ADMINPW, targetDut.AdminPassword);
                        CoreCommonInfo.SetVariable(SystemVariables.DUT_ID,      targetDut.DeviceId);
                        CoreCommonInfo.SetVariable(SystemVariables.DUT_TYPE,    targetDut.DeviceType);
                        foreach (DeviceUnderTest.Capability cap in targetDut.AdditionalCapabilites)
                            CoreCommonInfo.SetVariable("${DUT_" + cap.Key.ToUpper() + "}", cap.Value);
                    }
                    foreach (Library lib in kv.Value)
                        lib.Initialize(targetDut, outputDir);
                }
                else
                {
                    // Lazy path: resolve the correct DUT for each library individually.
                    // This ensures Mac libraries get the Mac device, Dune libraries get the
                    // Dune device, etc., even when all are in the same device group.
                    foreach (Library lib in kv.Value)
                    {
                        DeviceUnderTest libDut = ResolveLibraryDut(lib, kv.Key);
                        string libKey = kv.Key + "|" + lib.NameAs;
                        _dutAssignments[libKey] = (libDut, false);
                        _libraryToDeviceKey[lib.NameAs] = libKey;

                        if (libDut != null)
                        {
                           PlatformType libPlatform = PlatformDetector.DetectFromTypeName(lib.Name);
                            if (libPlatform != PlatformType.Unknown && _executionContext != null)
                            {
                                try
                                {
                                    _executionContext.RegisterDevice(libPlatform, libDut);
                                    Logger.Debug($"Registered device '{libDut.DeviceId}' for platform '{libPlatform}'");
                                }
                                catch (InvalidOperationException ex)
                                {
                                    Logger.Debug($"Device already registered for platform '{libPlatform}': {ex.Message}");
                                }
                            }
                        }
                        else if (lib.DutUsed())
                        {
                            string errorMsg =
                                $"No selected device is available for platform '{lib.Name}'. " +
                                $"Please select a {lib.Name} device before running the test.";
                            Logger.Error(errorMsg);
                             _libraryInitErrors[lib.NameAs] = errorMsg;
                        }

                        Logger.Debug($"[LazyInit] Deferred '{lib.Name}' " + $"(alias '{lib.NameAs}') → device '{libDut?.DeviceId ?? "null"}'");
                    }
                }
            }

            CoreCommonInfo.SetVariable(SystemVariables.EMPTY,           string.Empty);
            CoreCommonInfo.SetVariable(SystemVariables.OUTPUT_FOLDER,   outputDir);
            CoreCommonInfo.SetVariable(SystemVariables.CARRIAGE_RETUREN, "\r");
            CoreCommonInfo.SetVariable(SystemVariables.LINE_FEED,       "\n");

            // Copy dataset files to output folder
            if (_testDataManager.DataSets != null && _testDataManager.DataSets.Count > 0)
            {
                Dictionary<string, string> newDataSet = new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> kv in _testDataManager.DataSets)
                {
                    string filesInOutputPath = Path.Combine(outputDir, Path.GetFileName(kv.Value));
                    File.Copy(kv.Value, filesInOutputPath);
                    newDataSet[kv.Key] = filesInOutputPath;
                }
                _testDataManager.DataSets = newDataSet;
            }

            // Initialize Remote Executor
            foreach (KeyValuePair<string, RemoteExecutor> kv in _testDataManager.RemoteExecutors)
            {
                kv.Value.Initialize(_duts, tsPath, outputDir);
                CoreCommonInfo.SetRemoteExecutor(kv.Key, kv.Value);
            }

            _initialized = true;
        }

        /// <summary>
        /// Lazily initializes a library the first time one of its keywords is about to execute.
        /// Libraries that do not require a DUT (e.g. BuiltIn) are already initialized eagerly in
        /// InitExecutor and this method is a no-op for them.
        /// </summary>
        private void EnsureLibraryInitialized(Library library)
        {
            if (!_libraryToDeviceKey.TryGetValue(library.NameAs, out string deviceKey))
                return; // Not tracked (already eagerly initialized)

            if (!_dutAssignments.TryGetValue(deviceKey, out var entry))
                return;

            DeviceUnderTest dut = entry.Dut;

            PlatformType activePlatform = PlatformDetector.DetectFromTypeName(library.Name);
            if (_executionContext != null)
            {
                if (activePlatform != PlatformType.Unknown)
                {
                    CoreCommonInfo.SetExecutionContext(_executionContext, activePlatform);
                    Logger.Trace($"Set execution context for platform '{activePlatform}'");
                }
                else
                {
                    CoreCommonInfo.ClearExecutionContext();
                }
            }

            // Always switch the device context to the correct device for this platform,
            // so that every platform change in the script picks up the right DUT details.
            if (dut != null)
            {
                // Only log/print the switch message when actually changing to a different
                // device/platform combination, not on every keyword execution for the same one.
                string switchIdentity = $"{dut.DeviceId}|{library.Name}";
                if (!string.Equals(_lastActiveDeviceContext, switchIdentity, StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Debug($"[DeviceContext] Switching to device '{dut.DeviceId}' for platform '{library.Name}' (alias '{library.NameAs}')");
                    //Console.WriteLine($"[DeviceContext] Switching to device: {dut.DeviceId} for platform: {library.Name}");
                    _lastActiveDeviceContext = switchIdentity;
                }

                CoreCommonInfo.SetVariable(SystemVariables.DUT_ADDRESS, dut.DeviceAddress);
                CoreCommonInfo.SetVariable(SystemVariables.DUT_ADMINID, dut.AdminId);
                CoreCommonInfo.SetVariable(SystemVariables.DUT_ADMINPW, dut.AdminPassword);
                CoreCommonInfo.SetVariable(SystemVariables.DUT_ID,      dut.DeviceId);
                CoreCommonInfo.SetVariable(SystemVariables.DUT_TYPE,    dut.DeviceType);

                foreach (DeviceUnderTest.Capability cap in dut.AdditionalCapabilites)
                    CoreCommonInfo.SetVariable("${DUT_" + cap.Key.ToUpper() + "}", cap.Value);
            }
            else
            {
                string switchIdentity = $"NO_DUT|{library.Name}";
                if (!string.Equals(_lastActiveDeviceContext, switchIdentity, StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Debug($"[DeviceContext] Platform '{library.Name}' (alias '{library.NameAs}') has no DUT");
                    //Console.WriteLine($"[DeviceContext] Platform: {library.Name} (no DUT required)");
                    _lastActiveDeviceContext = switchIdentity;
                }
            }

            // Initialize this library only once
            if (!entry.LibsInitialized)
            {
                Logger.Debug($"[LazyInit] Initializing '{library.Name}' (alias '{library.NameAs}') for device '{dut?.DeviceId ?? "null"}'");
                try
                {
                    library.Initialize(dut, _outputDir);
                    _dutAssignments[deviceKey] = (dut, true);
                }
                catch (Exception ex)
                {
                    string errorMsg = $"Platform '{library.Name}' (alias '{library.NameAs}') failed to initialize: {ex.Message}";
                    Logger.Error(errorMsg);
                    Logger.Error(ex.ToString());
                    Console.WriteLine($"[LazyInit] ERROR - {errorMsg}");
                    _libraryInitErrors[library.NameAs] = errorMsg;
                    // Mark as initialized so we do not retry on every keyword call.
                    _dutAssignments[deviceKey] = (dut, true);
                }
            }
        }

        /// <summary>
        /// Resolves the best matching <see cref="DeviceUnderTest"/> for a single library.
        /// Resolution order:
        ///   1. Exact DeviceId match with the device group key (explicit 'with' clause).
        ///   2. DeviceType of an available DUT matches the library name (platform-based).
        ///   3. Auto-generate a local-machine DUT when no physical devices are configured.
        ///   4. Fallback to the first registered device with a warning.
        /// </summary>
        private DeviceUnderTest ResolveLibraryDut(Library lib, string groupKey)
        {
            // No selected/available DUT.
            // Do not create a fake LocalMachine DUT for physical platforms.
            if (_duts == null || _duts.Count == 0)
            {
                if (!lib.DutUsed())
                    return null;

                Logger.Error($"[DeviceMapping] No selected DUT available for platform '{lib.Name}'.");

                return null;
         }
        

            // 1. Platform-based match: resolve both the library name and the device's DeviceType
            //    to a PlatformType using PlatformDetector so that, e.g., a library named
            //    "JediOmni" (→ PlatformType.Jedi) correctly matches a DUT whose DeviceType is
            //    "Jedi" (also → PlatformType.Jedi). Falls back to exact string comparison when
            //    either side resolves to Unknown (e.g. custom/utility libraries).
            var libPlatform = PlatformDetector.DetectFromTypeName(lib.Name);
            var byPlatform = _duts.FirstOrDefault(d =>
            {
                if (string.Equals(d.DeviceType, lib.Name, StringComparison.OrdinalIgnoreCase))
                    return true;
                if (libPlatform != PlatformType.Unknown)
                {
                    var dutPlatform = PlatformDetector.DetectFromTypeName(d.DeviceType);
                    return dutPlatform == libPlatform;
                }
                return false;
            });
            if (byPlatform != null) return byPlatform;

            // 2. Exact DeviceId match with the group key.
            //    Used for libraries whose DeviceType does not directly match any registered
            //    device (e.g. utility/dependency libraries using an explicit 'with' clause).
            var byId = _duts.FirstOrDefault(d =>
                string.Equals(d.DeviceId, groupKey, StringComparison.OrdinalIgnoreCase));
            if (byId != null) return byId;

            // 3. Only fall back to "the" device when there is exactly one registered DUT,
            //    since in that case there is no ambiguity about which device was intended.
            //    When multiple devices of different platforms are registered, silently picking
            //    _duts[0] would wrongly bind this library to an unrelated device/platform
            //    (e.g. routing JediOmni keywords to an iOS device just because it was
            //    registered first). In that case, fail the resolution instead so the caller
            //    can report a clear error rather than silently executing on the wrong device.
            Logger.Error( $"[DeviceMapping] No selected device matches platform '{lib.Name}' " +$"(group key '{groupKey}'). Refusing to execute on another platform.");

            return null;
        }

        public string Execute(Statement aStatement, int repeat = -1, int stackLevel = 0, Dictionary<string, string> arguments = null)
        {
            try
            {
                return ExecuteInternal(aStatement, repeat, stackLevel, arguments);
            }
            catch (Exception ex)
            {
                // Never let an unexpected exception (including "Executor not initialized")
                // escape and abort the whole run. Mark the keyword as Failed, log the real
                // error, and let normal failure handling / report generation continue.
                return HandleError(aStatement, stackLevel, "Unexpected error during keyword execution", ex);
            }
        }

        private string ExecuteInternal(Statement aStatement, int repeat = -1, int stackLevel = 0, Dictionary<string, string> arguments = null)
        {
            if (!_initialized)
                throw new InvalidOperationException("Executor not initialized");

            object[] parameter = null;

            // Only for Turkish Language: Unicode conversion for comparison
            if (CultureInfo.CurrentCulture.Parent.Name.Equals("tr", StringComparison.OrdinalIgnoreCase))
            {
                aStatement.Library = Parser.EncodingDecodingLine(aStatement.Library);
                aStatement.Keyword = Parser.EncodingDecodingLine(aStatement.Keyword);
            }

            Console.Write("{0}{1}", new string('\t', stackLevel), aStatement.OriginalStatement);
            Logger.Trace($"Line Number :{aStatement.LineNumber} {aStatement.OriginalStatement} is called");

            string keyword = aStatement.Keyword.Replace(" ", "").ToUpper().Replace("()", "");

            Library library;
            Keyword keywordObj;
            try
            {
                library = _testDataManager.GetLibrary(aStatement.Library);
                keywordObj = library.GetKeyword(keyword);
            }
            catch (Exception ex)
            {
                return HandleError(aStatement, stackLevel, "Can not find specified keyword", ex);
            }

            if (keywordObj.Deprecated)
            {
                aStatement.DeprecatedReason = keywordObj.DeprecatedReason;
            }

            // Resolve parameters
            if (aStatement.Parameters != null)
            {
                parameter = (object[])aStatement.Parameters.Clone();
                for (int i = 0; i < parameter.Length; i++)
                {
                    foreach (Match var in VarRegex.Matches(parameter[i].ToString()))
                    {
                        if (var.Value.Equals(SystemVariables.FORLOOP_ITERATION, StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (CoreCommonInfo.HasforloopItem(var.Value))
                            {
                                parameter[i] = parameter[i].ToString().Replace(var.Value, CoreCommonInfo.GetforloopItem(var.Value));
                            }
                        }

                        if (var.Value.Equals(SystemVariables.REPEAT_COUNT, StringComparison.CurrentCultureIgnoreCase))
                        {
                            parameter[i] = parameter[i].ToString().Replace(var.Value, repeat.ToString());
                        }
                        else if (!keywordObj.IsGetKeyword)
                        {
                            if (!LibraryUtils.IsByPassedKeyword(ByPassKeywords, aStatement.OriginalStatement) &&
                                TryGetRuntimeVariable(var.Value, out string resolvedValue))
                            {
                                parameter[i] = parameter[i].ToString().Replace(var.Value, resolvedValue);
                            }
                        }
                    }

                    if (_testDataManager.Resources.ContainsKey(parameter[i].ToString()))
                    {
                        parameter[i] = _testDataManager.Resources[parameter[i].ToString()];
                    }
                }

                if (arguments != null)
                {
                    for (int i = 0; i < parameter.Length; i++)
                    {
                        foreach (KeyValuePair<string, string> arg in arguments)
                        {
                            parameter[i] = parameter[i].ToString().Replace(arg.Key, arg.Value);
                        }
                    }
                }
            }

            // Lazily initialize the library (no-op if already done or if it's a non-DUT library)
            EnsureLibraryInitialized(library);

            // If the library failed to initialize, report an error for this keyword but continue
            // execution so that subsequent platforms defined in the test script are still run.
            if (_libraryInitErrors.TryGetValue(library.NameAs, out string initError))
                return HandleError(aStatement, stackLevel, initError, new InvalidOperationException(initError));

            // Execute Native Library
            if (library.LibraryType.Equals(Library.LibraryTypes.Native))
            {
                KeywordResult result = ExecuteNativeKeyword(aStatement, library, keywordObj, parameter, stackLevel);
                aStatement.Result = result;

                if (result.ScreenShot != null && result.ScreenShot.Length > 0)
                {
                    string screenShotFileName = Path.Combine(library.OutputDir, $"{library.Name}_Screenshot_{DateTime.Now.ToString("yyMMdd_HHmmssff")}.png");
                    File.WriteAllBytes(screenShotFileName, result.ScreenShot);
                    aStatement.ScreenShotFilePath = screenShotFileName.Replace(library.OutputDir, ".");
                }
                else
                {
                    aStatement.ScreenShotFilePath = string.Empty;
                }
            }
            // Execute Custom Library
            else if (library.LibraryType.Equals(Library.LibraryTypes.Custom))
            {
                KeywordResult result = ExecuteCustomKeyword(aStatement, library, keywordObj, parameter, repeat);
                aStatement.Result = result;
                ApplyAlwaysPass(aStatement);
                Console.WriteLine(" :: " + aStatement.Result.Result);
                return result.Result.ToString();
            }

            ApplyAlwaysPass(aStatement);
            Console.WriteLine(" :: " + aStatement.Result.Result);
            Logger.Trace($"Keyword executed with result = {aStatement.Result.Result}");
            return aStatement.Result.Result.ToString();
        }

        private KeywordResult ExecuteNativeKeyword(Statement aStatement, Library library, Keyword keywordObj, object[] parameter, int stackLevel)
        {
            Type targetClass = keywordObj.Method.DeclaringType;
            var activator = library.GFActivator;
            KeywordResult result;

            try
            {
                result = InvokeNativeMethod(targetClass, keywordObj.Method.Name, activator, parameter);
            }
            catch (InvalidOperationException ioEx)
            {
                return CreateErrorResult($"Can not resolve name : {keywordObj.Method.Name}", "Could not find Keyword", ioEx);
            }
            catch (Exception ex)
            {
                if (IsSocketException(ex))
                {
                    if (library.Name.Equals("Android"))
                    {
                        Logger.Trace("Reset Android for connection issue");
                        string tmpOutputDir = library.OutputDir;
                        DeviceUnderTest tmpDut = library.Dut;
                        library.Dispose();
                        library.Initialize(tmpDut, tmpOutputDir);
                    }

                    try
                    {
                        result = InvokeNativeMethod(targetClass, keywordObj.Method.Name, activator, parameter);
                    }
                    catch (Exception retryEx)
                    {
                        return CreateErrorResult(
                            retryEx is InvalidOperationException ? $"Can not resolve name : {keywordObj.Method.Name}" : retryEx.ToString(),
                            retryEx is InvalidOperationException ? "Could not find Keyword" : "Unexpected exception",
                            retryEx);
                    }
                }
                else
                {
                    return CreateErrorResult(ex.ToString(), "Unexpected exception", ex);
                }
            }

            return result;
        }

        private KeywordResult InvokeNativeMethod(Type targetClass, string methodName, object activator, object[] parameter)
        {
            try
            {
                return (KeywordResult)targetClass.InvokeMember(methodName, BindingFlags.InvokeMethod, null, activator, parameter);
            }
            catch (MissingMethodException)
            {
                return (KeywordResult)targetClass.InvokeMember(methodName, BindingFlags.InvokeMethod, null, activator, new object[] { parameter });
            }
        }

        private KeywordResult ExecuteCustomKeyword(Statement aStatement, Library library, Keyword keywordObj, object[] parameter, int repeat)
        {
            CustomKeyword customKeyword;
            try
            {
                customKeyword = library.CustomLibrary.GetKeyword(keywordObj.KeywordName);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return CreateErrorResult($"Error getting keyword name : {keywordObj.KeywordName}", "Could not find Keyword", ex);
            }

            if (customKeyword == null)
            {
                Logger.Trace($"Error getting keyword name : {keywordObj.KeywordName}");
                return new KeywordResult(KeywordResults.Error) { Output = "Could not find Keyword", AdditionalInfo = $"Error getting keyword name : {keywordObj.KeywordName}" };
            }

            return customKeyword.Run(_testDataManager, parameter, repeat);
        }

        private bool IsSocketException(Exception ex)
        {
            return ex.InnerException?.GetType().Equals(typeof(System.Net.Sockets.SocketException)) == true
                || ex.ToString().Contains("System.Net.Sockets.SocketException");
        }

        private KeywordResult CreateErrorResult(string additionalInfo, string output, Exception ex)
        {
            Logger.Error(additionalInfo, ex);
            return new KeywordResult(KeywordResults.Error) { Output = output, AdditionalInfo = additionalInfo };
        }

        private string HandleError(Statement aStatement, int stackLevel, string message, Exception ex)
        {
            aStatement.Result = new KeywordResult(KeywordResults.Error) { Output = message };
            Console.WriteLine("{0} :: {1}", new string('\t', stackLevel), aStatement.Result.Result);
            Logger.Trace(message);
            Logger.Error(ex.ToString());
            ApplyAlwaysPass(aStatement);
            return aStatement.Result.Result.ToString();
        }

        private void ApplyAlwaysPass(Statement aStatement)
        {
            if (aStatement.IsAlwaysPass)
            {
                aStatement.Result.Result = KeywordResults.Pass;
            }
        }

        public static string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            if (_executionContext != null)
            {
                CoreCommonInfo.ClearExecutionContext();
                _executionContext.Dispose();
                _executionContext = null;
                Logger.Debug("ExecutionContext disposed and cleared");
            }

            if (_testDataManager == null)
                return;

            foreach (KeyValuePair<string, RemoteExecutor> kv in _testDataManager.RemoteExecutors)
            {
                try
                {
                    Logger.Trace($"Close remote executor of {kv.Key}");
                    kv.Value.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error disposing remote executor '{kv.Key}': {ex.Message}", ex);
                }
            }

            foreach (Library lib in _testDataManager.AllUsedLib)
            {
                try
                {
                    lib.Dispose();
                }
                catch (Exception ex)
                {
                    // A failure while disposing one library (e.g. a library that was never
                    // initialized, or whose native Dispose() throws) must not prevent the
                    // remaining libraries from being cleaned up, and must not propagate out
                    // of Dispose() and abort report generation/merging in Runner.Run().
                    Logger.Error($"Error disposing library '{lib.Name}' (alias '{lib.NameAs}'): {ex.Message}", ex);
                }
            }
        }
    }
    }