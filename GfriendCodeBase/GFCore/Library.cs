using HP.GFriend.GFLogger;
using HP.GFriend.Keywords;
using HP.GFriend.Support;
using HP.GFriend.Core.Custom;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using HP.GFriend.Core.Execution;

namespace HP.GFriend.Core
{

    public class Library : IDisposable
    {
        public enum LibraryTypes
        {
            Native,
            Custom
        }

        public string NameAs { get; internal set; }
        public string Name { get; }
        public string OutputDir { get; set; }
        public string Description { get; set; }
        public DeviceUnderTest Dut { get; set; }
        public List<Keyword> Keywords { get; set; }
        public LibraryTypes LibraryType { get; }
        public CustomLibrary CustomLibrary { get; set; }
        public IGFLibrary GFActivator { get; private set; } = null;
        private readonly Type _libType = null;

        // Tracks whether Initialize() actually completed successfully for this library instance.
        // GFActivator can be created as a side effect of calling DutUsed() (e.g. during device
        // resolution/dependency checks) before Initialize() is ever invoked, so GFActivator != null
        // is not sufficient to know the underlying library is safe to Dispose().
        private bool _isNativeInitialized = false;

        private PlatformType? _platform = null;

        /// <summary>
        /// Returns the platform type for this library. Auto-detected during Initialize() and cached.
        /// Can be overridden by setting the property.
        /// </summary>
        public virtual PlatformType Platform
        {
            get
            {
                if (_platform.HasValue)
                    return _platform.Value;
                _platform = DetectPlatform();
                return _platform.Value;
            }
            set { _platform = value; }
        }

        /// <summary>
        /// Detects the platform type based on the library type name or the library's registered name.
        /// Matches against known platform keywords such as "windows", "ios", "mac", and "android".
        /// </summary>
        /// <returns>
        /// A <see cref="PlatformType"/> value representing the detected platform,
        /// or <see cref="PlatformType.Unknown"/> if no platform can be determined.
        /// </returns>
        private PlatformType DetectPlatform()
        {
            string typeName = _libType?.Name?.ToLower() ?? Name?.ToLower() ?? string.Empty;

            if (typeName.Contains("windows") || typeName.Contains("win"))
                return PlatformType.Windows;
            if (typeName.Contains("ios") || typeName.Contains("iphone") || typeName.Contains("ipad"))
                return PlatformType.iOS;
            if (typeName.Contains("mac") || typeName.Contains("macos"))
                return PlatformType.Mac;
            if (typeName.Contains("android"))
                return PlatformType.Android;

            return PlatformType.Unknown;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Library"/> class for a native library.
        /// Automatically reads the <see cref="LibraryDescription"/> attribute from the provided type if present.
        /// </summary>
        /// <param name="name">The canonical name of the library.</param>
        /// <param name="nameAs">The alias name used to reference this library.</param>
        /// <param name="GFLibrary">The <see cref="Type"/> of the native GFriend library implementation.</param>
        public Library(string name, string nameAs, Type GFLibrary)
        {
            Name = name;
            NameAs = nameAs;
            _libType = GFLibrary;

            var desAttr = GFLibrary.GetCustomAttributes().Where(a => a.GetType().FullName.Equals(typeof(LibraryDescription).FullName)) ?? null;
            if (desAttr.Count() > 0)
            {
                Description = desAttr.First().ToString();
            }

            CustomLibrary = null;
            LibraryType = LibraryTypes.Native;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Library"/> class for a custom library.
        /// Reads the description directly from the provided <see cref="CustomLibrary"/> instance.
        /// </summary>
        /// <param name="name">The canonical name of the library.</param>
        /// <param name="nameAs">The alias name used to reference this library.</param>
        /// <param name="customLibrary">The <see cref="CustomLibrary"/> definition containing keywords and metadata.</param>
        public Library(string name, string nameAs, CustomLibrary customLibrary)
        {
            Name = name;
            NameAs = nameAs;
            _libType = null;
            CustomLibrary = customLibrary;
            Description = customLibrary.Description;
            LibraryType = LibraryTypes.Custom;
        }

        /// <summary>
        /// Creates a shallow clone of the current <see cref="Library"/> instance,
        /// preserving the DUT, output directory, and platform if already resolved.
        /// </summary>
        /// <returns>
        /// A new <see cref="Library"/> instance with the same configuration,
        /// or <c>null</c> if the library type is unrecognized.
        /// </returns>
        public Library Clone()
        {
            Library clonned = null;
            switch (LibraryType)
            {
                case LibraryTypes.Custom:
                    clonned = new Library(Name, NameAs, CustomLibrary);
                    break;
                case LibraryTypes.Native:
                    clonned = new Library(Name, NameAs, _libType);
                    break;
            }

            if (clonned == null) return null;

            clonned.Dut = Dut;
            clonned.OutputDir = OutputDir;
            if (_platform.HasValue)
                clonned.Platform = _platform.Value;

            return clonned;
        }

        /// <summary>
        /// Loads the library under the specified alias and populates the <see cref="Keywords"/> list.
        /// For native libraries, keywords are discovered via reflection on methods returning <see cref="KeywordResult"/>.
        /// For custom libraries, keywords are sourced directly from the <see cref="CustomLibrary"/> definition.
        /// </summary>
        /// <param name="nameAs">The alias name to register this library under during loading.</param>
        public void Load(string nameAs)
        {
            Logger.Trace($"Loading Library : {Name} -> {nameAs}");
            Keywords = new List<Keyword>();
            NameAs = nameAs;
            if (LibraryType.Equals(LibraryTypes.Native))
            {
                Logger.Trace("Library type is native library.");
                MethodInfo[] methods = _libType.GetMethods();
                foreach (MethodInfo m in methods)
                {
                    if (m.ReturnType.FullName.Equals(typeof(KeywordResult).FullName))
                    {
                        Keyword aKeyword = new Keyword();
                        Utils.GetKeywordFromMethod(m, aKeyword);
                        Keywords.Add(aKeyword);
                    }
                }
            }
            else if (LibraryType.Equals(LibraryTypes.Custom))
            {
                Logger.Trace("Library type is custom library.");
                foreach (CustomKeyword cKeyword in CustomLibrary.Keywords)
                {
                    Keyword aKeyword = new Keyword();
                    aKeyword.Args = cKeyword.Args;
                    aKeyword.NumOfArgs = cKeyword.NumOfArgs;
                    aKeyword.Description = cKeyword.Description;
                    aKeyword.FunctionName = cKeyword.Name;
                    aKeyword.KeywordName = cKeyword.Name;
                    Keywords.Add(aKeyword);
                }
            }
        }

        /// <summary>
        /// Initializes the native library by creating an activator instance and invoking its
        /// <c>Initialize</c> method with the provided device and output directory.
        /// Also detects and caches the platform type.
        /// </summary>
        /// <param name="dut">
        /// The <see cref="DeviceUnderTest"/> to associate with the library.
        /// May be <c>null</c> only if the library reports that a DUT is not required.
        /// </param>
        /// <param name="outputDir">The directory path where output files should be written.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="dut"/> is <c>null</c> and the library requires a DUT.
        /// </exception>
        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
            if (LibraryType.Equals(LibraryTypes.Native))
            {
                if (GFActivator == null)
                {
                    GFActivator = (IGFLibrary)System.Activator.CreateInstance(_libType);
                }

                bool dutRequired = (bool)_libType.InvokeMember("DutUsed", BindingFlags.InvokeMethod, null, GFActivator, new object[] { });

                if (dut == null && dutRequired)
                {
                    Logger.Error($"Library '{Name}': DeviceUnderTest is null during Initialize.");
                    throw new ArgumentNullException(nameof(dut), "DeviceUnderTest cannot be null for native library initialization.");
                }

                Dut = dut;
                OutputDir = outputDir;

                _platform = DetectPlatform();
                Logger.Trace($"Library '{Name}' platform detected as: {_platform.Value}");

                try
                {
                    if (dut == null && !dutRequired)
                    {
                        Logger.Trace($"Library '{Name}': DUT not required, initializing with null DUT for platform '{_platform.Value}'.");
                        _libType.InvokeMember("Initialize", BindingFlags.InvokeMethod, null, GFActivator, new object[] { dut, outputDir });
                    }
                    else
                    {
                        _libType.InvokeMember("Initialize", BindingFlags.InvokeMethod, null, GFActivator, new object[] { dut, outputDir });
                    }

                    _isNativeInitialized = true;
                }
                catch (TargetInvocationException tie) when (tie.InnerException != null)
                {
                    // InvokeMember wraps any exception thrown by the library's own Initialize()
                    // method (e.g. a device connection/timeout failure) in a TargetInvocationException
                    // whose own message is just "Exception has been thrown by the target of an
                    // invocation." Unwrap it (preserving the original stack trace) so the real
                    // root cause is logged/reported and callers (e.g.
                    // Executor.EnsureLibraryInitialized) see the actual failure reason.
                    ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
                }
            }
        }

        /// <summary>
        /// Executes a named method on the native library activator with the supplied parameters.
        /// </summary>
        /// <param name="methodName">The name of the method to invoke on the native library.</param>
        /// <param name="param">An array of arguments to pass to the method.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown when this method is called on a non-native (custom) library.
        /// </exception>
        public void ExecuteMethod(string methodName, object[] param)
        {
            if (LibraryType.Equals(LibraryTypes.Native))
            {
                if (GFActivator == null)
                {
                    GFActivator = (IGFLibrary)System.Activator.CreateInstance(_libType);
                }
                _libType.InvokeMember(methodName, BindingFlags.InvokeMethod, null, GFActivator, param);
            }
            else
            {
                throw new NotSupportedException("Only works with native library");
            }
        }

        /// <summary>
        /// Retrieves the list of dependency names declared by this library.
        /// For native libraries, invokes <c>GetDependencies</c> via reflection.
        /// For custom libraries, delegates to <see cref="CustomLibrary.GetDependencies"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="List{T}"/> of dependency name strings,
        /// or <c>null</c> if the library type is unrecognized.
        /// </returns>
        public List<string> GetDependencies()
        {
            if (LibraryType.Equals(LibraryTypes.Native))
            {
                if (GFActivator == null)
                {
                    GFActivator = (IGFLibrary)System.Activator.CreateInstance(_libType);
                }
                return (List<string>)_libType.InvokeMember("GetDependencies", BindingFlags.InvokeMethod, null, GFActivator, new object[] { });
            }
            else if (LibraryType.Equals(LibraryTypes.Custom))
            {
                return CustomLibrary.GetDependencies();
            }
            return null;
        }

        /// <summary>
        /// Determines whether this library requires a <see cref="DeviceUnderTest"/> to function.
        /// For native libraries, invokes <c>DutUsed</c> via reflection.
        /// For custom libraries, delegates to <see cref="CustomLibrary.DutUsed"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if a DUT is required; otherwise <c>false</c>.
        /// Defaults to <c>true</c> for unrecognized library types.
        /// </returns>
        public bool DutUsed()
        {
            if (LibraryType.Equals(LibraryTypes.Native))
            {
                if (GFActivator == null)
                {
                    GFActivator = (IGFLibrary)System.Activator.CreateInstance(_libType);
                }
                return (bool)_libType.InvokeMember("DutUsed", BindingFlags.InvokeMethod, null, GFActivator, new object[] { });
            }
            else if (LibraryType.Equals(LibraryTypes.Custom))
            {
                return CustomLibrary.DutUsed();
            }
            return true;
        }

        /// <summary>
        /// Releases resources held by this library by invoking <c>Dispose</c> on the
        /// native library activator instance, if one has been created.
        /// </summary>
        public void Dispose()
        {
            if (LibraryType.Equals(LibraryTypes.Native))
            {
                // Only dispose libraries that actually completed Initialize(). GFActivator can be
                // non-null even when Initialize() was never called (e.g. DutUsed() lazily creates
                // it), so relying on GFActivator alone can invoke Dispose() on a library instance
                // whose internal state (e.g. the DUT reference) was never set, causing NREs.
                if (GFActivator != null && _isNativeInitialized)
                {
                    _libType.InvokeMember("Dispose", BindingFlags.InvokeMethod, null, GFActivator, new object[] { });
                }
            }
        }

        /// <summary>
        /// Builds a dictionary of keyword entries formatted for use in standard auto-complete suggestions.
        /// Each entry maps a completion token (including argument placeholders) to a descriptive tooltip string.
        /// Deprecated keywords are excluded.
        /// </summary>
        /// <returns>
        /// A <see cref="Dictionary{TKey, TValue}"/> where the key is the auto-complete token
        /// and the value is the corresponding description string.
        /// </returns>
        public Dictionary<string, string> GetKeywordInfoForAutoComplete()
        {
            Dictionary<string, string> retVal = new Dictionary<string, string>();

            foreach (Keyword aKeyword in Keywords)
            {
                if (!aKeyword.Deprecated)
                {
                    string kwd;
                    string des;

                    kwd = aKeyword.KeywordName;
                    if (aKeyword.NumOfArgs > 0)
                    {
                        des = aKeyword.KeywordName + " (" + aKeyword.Args + ")\n" + aKeyword.Description;
                        kwd += " (^" + new string(',', aKeyword.NumOfArgs - 1) + ")";
                    }
                    else
                    {
                        des = aKeyword.KeywordName + "\n" + aKeyword.Description;
                        kwd += "^";
                    }
                    retVal[kwd] = des;
                }
            }

            return retVal;
        }

        /// <summary>
        /// Builds a dictionary of keyword entries formatted for auto-complete in Co-Developer mode.
        /// When a <see cref="Keyword.SampleScript"/> is available, it is used as the completion token
        /// with library prefix stripping applied. Falls back to the standard keyword name and argument
        /// placeholder format when no sample script is present. Deprecated keywords are excluded.
        /// </summary>
        /// <returns>
        /// A <see cref="Dictionary{TKey, TValue}"/> where the key is the Co-Developer auto-complete token
        /// and the value is the corresponding description string.
        /// </returns>
        public Dictionary<string, string> GetKeywordInfoForAutoCompleteForCoDeveloperMode()
        {
            Dictionary<string, string> retVal = new Dictionary<string, string>();

            foreach (Keyword aKeyword in Keywords)
            {
                if (!aKeyword.Deprecated)
                {
                    string kwd = "";
                    string des = "";
                    if (!string.IsNullOrEmpty(aKeyword.SampleScript))
                    {
                        if (!_libType.Name.ToLower().Equals("builtinlibrary"))
                        {
                            if (aKeyword.SampleScript.Contains(_libType.Name))
                            {
                                kwd = aKeyword.SampleScript.EndsWith(".") ? aKeyword.SampleScript.Substring(0, aKeyword.SampleScript.LastIndexOf(".")).Substring(_libType.Name.Length + 1) + "^" : aKeyword.SampleScript.Substring(_libType.Name.Length + 1) + "^";
                            }
                        }
                        else
                        {
                            kwd = aKeyword.SampleScript.EndsWith(".") ? aKeyword.SampleScript.Substring(0, aKeyword.SampleScript.LastIndexOf(".")) + "^" : aKeyword.SampleScript + "^";
                        }
                        des = kwd + "\n" + aKeyword.Description;
                    }
                    else
                    {
                        kwd = aKeyword.KeywordName;
                        if (aKeyword.NumOfArgs > 0)
                        {
                            des = aKeyword.KeywordName + " (" + aKeyword.Args + ")\n" + aKeyword.Description;
                            kwd += " (^" + new string(',', aKeyword.NumOfArgs - 1) + ")";
                        }
                        else
                        {
                            des = aKeyword.KeywordName + "\n" + aKeyword.Description;
                            kwd += "^";
                        }
                    }
                    retVal[kwd] = des;
                }
            }

            return retVal;
        }

        /// <summary>
        /// Finds and returns the first <see cref="Keyword"/> whose <see cref="Keyword.FunctionName"/>
        /// matches the specified name, ignoring case and whitespace.
        /// </summary>
        /// <param name="keywordName">The function name of the keyword to locate.</param>
        /// <returns>The matching <see cref="Keyword"/> instance.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no matching keyword is found in the <see cref="Keywords"/> list.
        /// </exception>
        public Keyword GetKeyword(string keywordName)
        {
            return Keywords.Where(k => k.FunctionName.Replace(" ", "").Equals(keywordName, StringComparison.OrdinalIgnoreCase)).First();
        }

        /// <summary>
        /// Generates an HTML keyword documentation file for this library at the specified location.
        /// The file is built from the embedded <c>GF_Keywords</c> template resource and lists all
        /// keywords with their names, arguments, descriptions, and sample scripts.
        /// Any existing file at the target path is deleted before generation.
        /// </summary>
        /// <param name="outputDir">The directory in which to create the documentation file.</param>
        /// <param name="fileName">
        /// The name of the output HTML file. Defaults to <c>"GF_Keywords.html"</c>.
        /// </param>
        public void GenerateKeywordDocumentation(string outputDir, string fileName = "GF_Keywords.html")
        {
            string keywordDoc = Path.Combine(outputDir, fileName);
            StreamWriter writer;
            Byte[] keywordTemplateByte = Encoding.UTF8.GetBytes(Properties.Resources.GF_Keywords);

            if (File.Exists(keywordDoc))
            {
                File.Delete(keywordDoc);
            }

            using (FileStream fs = new FileStream(keywordDoc, FileMode.Create))
            {
                fs.Write(keywordTemplateByte, 0, keywordTemplateByte.Length);
            }

            writer = new StreamWriter(keywordDoc, true, Encoding.UTF8);

            writer.WriteLine($" {Name}");

            // Adding description if it is not null or empty
            if (!string.IsNullOrEmpty(Description))
            {
                writer.WriteLine($"{Description} </h5></th>\r\n</tr>\r\n</table></br>");
            }
            else
            {
                writer.WriteLine($"</h5></th>\r\n</tr>\r\n</table></br>");
            }

            writer.WriteLine("<div>List of keywords</div><div class=\"search-container\">\r\n<input type=\"text\" id=\"searchInput\" placeholder=\"Search Keywords...\" oninput=\"delay(searchLinks, 150)\">\r\n</div>");
            writer.WriteLine("<div class=\"sidebar\" id=\"myTable\">");

            // Write Keyword List
            foreach (Keyword aKeyword in Keywords.OrderBy(i => i.KeywordName))
            {
                string args = "";
                string description = "";
                string sampleScript = "";
                if (!string.IsNullOrEmpty(aKeyword.Args))
                {
                    args = aKeyword.Args.Replace("\r\n", " , ");
                    args = args.Replace("\n", " ");
                    args = args.Replace("'", "&#92;&#39;");//Replacing with \'
                    args = args.Replace("\\", "\\\\");
                    args = args.Replace("\"", "&#34;");
                }
                if (!string.IsNullOrEmpty(aKeyword.Description))
                {
                    description = aKeyword.Description.Replace("\r\n", " ");
                    description = description.Replace("\n", " ");
                    description = description.Replace("'", "&#92;&#39;");
                    description = description.Replace("\\", "\\\\");
                    description = description.Replace("\"", "&#34;");
                }
                if (!string.IsNullOrEmpty(aKeyword.SampleScript))
                {
                    sampleScript = aKeyword.SampleScript.Replace("\r\n", " ");
                    sampleScript = sampleScript.Replace("\n", " ");
                    sampleScript = sampleScript.Replace("'", "&#92;&#39;");
                    sampleScript = sampleScript.Replace("\\", "\\\\");
                    sampleScript = sampleScript.Replace("\"", "&#34;");
                }

                string line = $"<a id=\"mylink\" href = \"#\" onclick=\"displayContent(['{aKeyword.KeywordName}','{args}','{description}','{sampleScript}'])\">{aKeyword.KeywordName}</a> </br> ";
                writer.WriteLine(line);
            }
            writer.WriteLine("</div><div class=\"content\" id=\"mainContent\"><p id=\"Keywordname\">Keyword Information</p><p id=\"des\"></p><p id=\"args\"></p><p id=\"syntax\"></p><p id=\"script\"></p></div>");

            writer.WriteLine("</div></div>");
            writer.WriteLine("</body>");
            writer.WriteLine("</html>");
            writer.Flush();
            writer.Close();
        }

        /// <summary>
        /// Retrieves detailed information for a keyword matching the given name and argument count.
        /// The result includes the keyword name, argument count, argument list, description,
        /// sample script, and generated call syntax.
        /// </summary>
        /// <param name="library">The library alias used when constructing the syntax string.</param>
        /// <param name="keyword">The display name of the keyword to look up (case-insensitive).</param>
        /// <param name="args">The expected number of arguments; used to disambiguate overloaded keyword names.</param>
        /// <returns>
        /// A <see cref="Dictionary{TKey, TValue}"/> containing the keys
        /// <c>KeywordName</c>, <c>NumOfArgs</c>, <c>Arguments</c>, <c>KeywordDescription</c>,
        /// <c>SampleScript</c>, and <c>Syntax</c>, or an empty dictionary if no match is found
        /// or an exception occurs.
        /// </returns>
        public Dictionary<string, string> GetKeywordDetails(string library, string keyword, int args)
        {
            Dictionary<string, string> retVal = new Dictionary<string, string>();
            try
            {
                foreach (Keyword aKeyword in Keywords)
                {
                    if (aKeyword.KeywordName.ToLower().Trim().Equals(keyword.ToLower().Trim()) && args == aKeyword.NumOfArgs)
                    {
                        retVal["KeywordName"] = aKeyword.KeywordName;
                        retVal["NumOfArgs"] = Convert.ToString(aKeyword.NumOfArgs);
                        retVal["Arguments"] = aKeyword.Args;
                        retVal["KeywordDescription"] = aKeyword.Description;
                        retVal["SampleScript"] = aKeyword.SampleScript;

                        string syntax = GetSyntax(library, aKeyword, args);
                        retVal["Syntax"] = syntax;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Library->GetKeywordDetails : " + ex.Message);
            }
            return retVal;
        }

        /// <summary>
        /// Constructs a human-readable call syntax string for the specified keyword,
        /// including the library alias, keyword name, and parameter names derived from
        /// the keyword's argument list.
        /// </summary>
        /// <param name="library">The library alias prepended to the syntax string.</param>
        /// <param name="keyword">The <see cref="Keyword"/> for which to build the syntax.</param>
        /// <param name="args">
        /// The argument count used to select the correct formatting branch.
        /// Values of 0 or 1 are handled individually; 2 or more trigger multi-parameter formatting.
        /// </param>
        /// <returns>
        /// A syntax string in the form <c>library.KeywordName(param1, param2, ...)</c>,
        /// or a partial syntax string if an exception occurs during formatting.
        /// </returns>
        private string GetSyntax(string library, Keyword keyword, int args)
        {
            string[] parameters = keyword.Args.Split('\r');
            string syntax = library.Trim() + "." + keyword.KeywordName + "(";

            try
            {
                if (args >= 2)
                {
                    if (keyword.NumOfArgs == args)
                    {
                        for (int i = 0; i < keyword.NumOfArgs; i++)
                        {
                            if (keyword.NumOfArgs == i + 1)
                            {
                                syntax = syntax + parameters[i].Split(':')[0].Trim('\n').Trim() + ")";
                            }
                            else
                            {
                                syntax = syntax + parameters[i].Split(':')[0].Trim('\n').Trim() + ",";
                            }
                        }
                        return syntax;
                    }
                }
                else
                {
                    if (keyword.NumOfArgs == 0)
                    {
                        syntax = syntax + ")";
                        return syntax;
                    }
                    else if (keyword.NumOfArgs == 1)
                    {
                        syntax = syntax + keyword.Args.Split(':')[0] + ")";
                        return syntax;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Library->GetSyntax : " + ex.Message);
                return syntax;
            }
            return syntax;
        }
    }
}
