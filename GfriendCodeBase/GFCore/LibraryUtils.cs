using HP.GFriend.Core.Custom;
using HP.GFriend.Keywords;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HP.GFriend.Core
{
    public static class LibraryUtils
    {
        public static string LibraryPath { get; set; }
        private static Dictionary<string, Library> _availableLibrary;
        private static List<string> _BuiltInAndGFLibraries = new List<string>();

        public static List<string> GetAvailbleLibraryNames(string scriptRoot = null, bool force = false)
        {
            if(_availableLibrary == null)
            {
                _availableLibrary = new Dictionary<string, Library>();
                SearchAvailableLibraries(scriptRoot);
            }
            return _availableLibrary.Keys.ToList<string>();
        }

        public static Dictionary<string, Library> GetAvailableLibraries(string scriptRoot = null, bool force = false)
        {
            if (_availableLibrary == null)
            {
                _availableLibrary = new Dictionary<string, Library>();
                SearchAvailableLibraries(scriptRoot);
            }
            return _availableLibrary;
        }

        public static void AddCustomLibraryToAvailableLibraries(CustomLibrary customLibrary)
        {
            if(_availableLibrary == null)
            {
                _availableLibrary = new Dictionary<string, Library>();
                SearchAvailableLibraries();
            }
            if (_availableLibrary.ContainsKey(customLibrary.Name))
            {
                _availableLibrary.Remove(customLibrary.Name);
            }
            _availableLibrary.Add(customLibrary.Name, new Library(customLibrary.Name, customLibrary.Name, customLibrary));

        }

        private static void SearchAvailableLibraries(string scriptRoot = null, bool force = false)
        {
            if (force || string.IsNullOrEmpty(LibraryPath))
            {
                LibraryPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "libs");
            }

            if (!Directory.Exists(LibraryPath))
            {
                Directory.CreateDirectory(LibraryPath);
            }

            // Add Built-in
            Type builtIn = typeof(BuiltInLibrary);
            if (builtIn.GetInterfaces().Where(i => i.FullName.Equals(typeof(IGFLibrary).FullName)).Count() > 0)
            {
                var activator = Activator.CreateInstance(builtIn);
                string libName = (string)builtIn.InvokeMember("GetName", BindingFlags.InvokeMethod, null, activator, null);
                Library aLib = new Library(libName, libName, builtIn);
                _availableLibrary.Add(libName, aLib);
                //LoadLibrary("BuiltIn");
            }

            foreach (string file in Directory.EnumerateFiles(LibraryPath,"GFK*.dll", SearchOption.TopDirectoryOnly))
            {
                Assembly dll = Assembly.LoadFrom(file);
                List<Type> types = dll.GetExportedTypes().ToList<Type>();

                foreach (Type t in types)
                {
                    if (t.GetInterfaces().Where(i => i.FullName.Equals(typeof(IGFLibrary).FullName)).Count()>0)
                    {
                        var activator = Activator.CreateInstance(t);
                        try
                        {
                            string libName = (string)t.InvokeMember("GetName", BindingFlags.InvokeMethod, null, activator, null);
                            Library aLib = new Library(libName, libName, t);
                            _availableLibrary.Add(libName, aLib);
                        }
                        catch (Exception) { }

                    }
                }
            }

            if(!string.IsNullOrEmpty(scriptRoot) && Directory.Exists(scriptRoot))
            {
                foreach (string file in Directory.EnumerateFiles(scriptRoot, "*.gflib", SearchOption.AllDirectories))
                {
                    CustomLibrary customLibrary = new CustomLibrary(file);
                    if (_availableLibrary.ContainsKey(customLibrary.Name))
                    {
                        _availableLibrary.Remove(customLibrary.Name);
                    }
                    _availableLibrary.Add(customLibrary.Name, new Library(customLibrary.Name, customLibrary.Name, customLibrary));
                }
            }
        }

        public static List<string> GetBuiltInAndGFLibraries()
        {
            _BuiltInAndGFLibraries = new List<string>();
            // Add Built-in
            Type builtIn = typeof(BuiltInLibrary);
            if (builtIn.GetInterfaces().Where(i => i.FullName.Equals(typeof(IGFLibrary).FullName)).Count() > 0)
            {
                var activator = Activator.CreateInstance(builtIn);
                string libName = (string)builtIn.InvokeMember("GetName", BindingFlags.InvokeMethod, null, activator, null);
                _BuiltInAndGFLibraries.Add(libName);
            }

            // GF Libraries
            foreach (string file in Directory.EnumerateFiles(LibraryPath, "GFK*.dll", SearchOption.TopDirectoryOnly))
            {
                Assembly dll = Assembly.LoadFrom(file);
                List<Type> types = dll.GetExportedTypes().ToList<Type>();

                foreach (Type t in types)
                {
                    if (t.GetInterfaces().Where(i => i.FullName.Equals(typeof(IGFLibrary).FullName)).Count() > 0)
                    {
                        var activator = Activator.CreateInstance(t);
                        try
                        {
                            string libName = (string)t.InvokeMember("GetName", BindingFlags.InvokeMethod, null, activator, null);
                            _BuiltInAndGFLibraries.Add(libName);
                        }
                        catch (Exception) { }

                    }
                }
            }

            return _BuiltInAndGFLibraries;
        }
        public static bool IsByPassedKeyword(string[] byPassKeywords, string line)
        {
            bool isByPassedKeyword = false;
            try
            {
                string[] byPassedStatements = { "Repeat:", "For:", "While:", "ForEachRow:", "SpraedSheetForEachRow:", "ForSelectedRow:", "RemoteRun:", "If:", "Fail:", "Error:" };
                foreach (string statement in byPassedStatements)
                {
                    if (line.Contains(statement))
                    {
                        return false;
                    }
                }
                foreach (string keyword in byPassKeywords)
                {
                    if (line.Contains(keyword))
                    {
                        if (line.Contains("("))
                        {
                            if ((line.Substring(0, line.IndexOf("(")).Trim()).Equals(keyword))
                            {
                                isByPassedKeyword = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GFLogger.Logger.Error("IsByPassedKeyword method failed ", ex);
                GFLogger.Logger.Error("Exception message : " + ex.Message);
                GFLogger.Logger.Error("Inner Exception : " + ex.InnerException);
                isByPassedKeyword = false;
            }
            return isByPassedKeyword;
        }
        public static ArrayList variablenamesInLine(string line)
        {

            ArrayList variablenames = new ArrayList();
            try
            {
                if (!String.IsNullOrEmpty(line) && line.Contains("$"))
                {
                    string[] splitline = line.Split('$');

                    foreach (var splittedline in splitline)
                    {
                        if (splittedline.StartsWith("{"))
                        {
                            string variablename = "$" + splittedline.Substring(splittedline.IndexOf('{'), splittedline.IndexOf('}') + 1);
                            if (!variablenames.Contains(variablename))
                            {
                                variablenames.Add(variablename);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GFLogger.Logger.Error("variablenamesInLine method failed ", ex);
            }
            return variablenames;
        }

        public static bool IsKeywordDeprecated(string statement)
        {
            bool isKeywordDeprecated = false;
            try
            {
            string library = statement.Split('.')[0] ?? "BuiltInLibrary";
            string keyword = statement.Split('.')[1].Split('(')[0].ToString();
            int arguments = 0;
            if (statement.Contains('('))
            {
                arguments = statement.Split('(')[1].Split(',').Length;
            }


            GFLogger.Logger.Trace($"Inside the method IsKeywordDeprecated");
            GFLogger.Logger.Trace($"Library : {library}");
            GFLogger.Logger.Trace($"Keyword : {keyword}");
            GFLogger.Logger.Trace($"No. of arguments : {arguments}");

            Dictionary<string, Library> availableLibraries = GetAvailableLibraries();
            bool libraryExists = availableLibraries.Keys.Any(key => key.Trim().Equals(library.Trim(), StringComparison.OrdinalIgnoreCase));

            if (libraryExists)
            {
                Type libtype = null;
                if (library == "BuiltInLibrary")
                {
                    libtype = typeof(BuiltInLibrary);
                }
                else
                {
                    string libraryPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "libs");
                    string file = Path.Combine(libraryPath, "GFK." + library + ".dll");
                    Assembly dll = Assembly.LoadFrom(file);

                    List<Type> types = dll.GetExportedTypes().ToList<Type>();
                    foreach (Type type in types)
                    {
                        if (type.Name.ToLower().Equals(library.ToLower()))
                        {
                            libtype = type;
                        }
                    }
                }

                MethodInfo[] methods = libtype.GetMethods();
                foreach (MethodInfo m in methods)
                {
                    if (m.ReturnType.FullName.Equals(typeof(KeywordResult).FullName))
                    {
                        Keyword aKeyword = new Keyword();
                        HP.GFriend.Support.Utils.GetKeywordFromMethod(m, aKeyword);

                        if (aKeyword.KeywordName.Trim().ToLower() == keyword.Trim().ToLower())
                        {
                            if (aKeyword.NumOfArgs == arguments)
                            {
                                if (aKeyword.Deprecated)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                }
            }
            catch (Exception ex)
            {
                GFLogger.Logger.Trace($"Exception in the method IsKeywordDeprecated");
                GFLogger.Logger.Trace($"Exception : "+ex.Message);
            }

            return isKeywordDeprecated;
        }
    }
}
