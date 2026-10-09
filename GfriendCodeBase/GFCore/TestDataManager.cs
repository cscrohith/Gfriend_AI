using HP.GFriend.Core.Execution;
using HP.GFriend.Core.Remote;
using HP.GFriend.GFLogger;
using HP.GFriend.Keywords;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace HP.GFriend.Core
{
    [DataContract]
    public class TestDataManager
    {
        public string DEFAULT_DUT { get; private set; } = "__DEFUALT__DUT__";
        public const string NO_DUT = "__NO__DUT__";

        public Dictionary<string, DeviceUnderTest> DutDictionary { get; internal set; }
        public Dictionary<string, List<Library>> DeviceLibraryMapping { get; internal set; }
        public TestSuite TargetTestSuite { get; internal set; }
        public List<string> TestCaseToRun { get; internal set; }
        public List<string> FilesToUse { get; internal set; }

        public List<Library> AllUsedLib { get; private set; }
        public Dictionary<string, string> Resources { get; internal set; }
        public Dictionary<string, string> Variables { get; internal set; }
        public Dictionary<string, string> DataSets { get; internal set; }

        // For remote execution
        public Dictionary<string, RemoteExecutor> RemoteExecutors { get; internal set; }

        public Executor Executor { get; internal set; }

        public string TestSuitePath { get; internal set; }

        public TestDataManager()
        {
            DeviceLibraryMapping = new Dictionary<string, List<Library>>();
            DutDictionary = new Dictionary<string, DeviceUnderTest>();
            TestCaseToRun = new List<string>();
            FilesToUse = new List<string>();
            Resources = new Dictionary<string, string>();
            Variables = new Dictionary<string, string>();
            DataSets = new Dictionary<string, string>();
            RemoteExecutors = new Dictionary<string, RemoteExecutor>();

            List<Library> NonDutLibs = new List<Library>();
            NonDutLibs.Add(LibraryUtils.GetAvailableLibraries()["BuiltIn"]);
            DeviceLibraryMapping.Add(NO_DUT, NonDutLibs);
        }

        public void PostActionAfterParsingUsings()
        {
            Logger.Debug("Post Action");


            // Adding Dependencies
            Dictionary<string, Library> avaliableLibs = LibraryUtils.GetAvailableLibraries();
            foreach (KeyValuePair<string, List<Library>> dutLibDic in DeviceLibraryMapping)
            {
                List<string> dependencies = new List<string>();
                foreach(Library lib in dutLibDic.Value)
                {
                    List<string> libDependencies = lib.GetDependencies();
                    if(libDependencies != null)
                    {
                        dependencies = dependencies.Concat(libDependencies).ToList();
                    }

                }
                dependencies = dependencies.Distinct().ToList();

                foreach(string libName in dependencies)
                {
                    bool existing = false;
                    if (dutLibDic.Value.Where(s => s.Name.Equals(libName, StringComparison.CurrentCultureIgnoreCase)).Any())
                    {
                        existing = true;
                    }
                    if(!existing)
                    {
                        dutLibDic.Value.Add(avaliableLibs[libName].Clone());
                    }
                }
            }

            // Create Non-DUT library set and Add built-in as default
            List<Library> NonDutLibs = DeviceLibraryMapping[NO_DUT];
            if(NonDutLibs.Count == 0)
            {
                NonDutLibs.Add(avaliableLibs["BuiltIn"]);
            }

            if (DeviceLibraryMapping.ContainsKey(DEFAULT_DUT))
            {
                List<Library> DefaultDutLibs = DeviceLibraryMapping[DEFAULT_DUT];
                List<Library> toDelete = new List<Library>();
                foreach (Library lib in DefaultDutLibs)
                {
                    if(!lib.DutUsed())
                    {
                        bool existing = false;
                        if (NonDutLibs.Where(s => s.NameAs.Equals(lib.NameAs, StringComparison.CurrentCultureIgnoreCase)).Any())
                        {
                            existing = true;
                        }
                        if (!existing)
                        {
                            NonDutLibs.Add(lib);
                        }
                        toDelete.Add(lib);
                    }
                }
                if(toDelete.Count > 0)
                {
                    foreach(Library lib in toDelete)
                    {
                        DefaultDutLibs.Remove(lib);
                    }
                }
            }


            if (DeviceLibraryMapping.ContainsKey(DEFAULT_DUT) && DeviceLibraryMapping[DEFAULT_DUT].Count == 0)
            {
                DeviceLibraryMapping.Remove(DEFAULT_DUT);
            }

            // Create All Used Lib for searching
            GenerateAllUsedLibs();

        }

        public void GenerateAllUsedLibs()
        {
            AllUsedLib = new List<Library>();
            AllUsedLib.AddRange(DeviceLibraryMapping.Values.SelectMany(x => x.Where(o=>!AllUsedLib.Contains(o))));
            Logger.Trace($"All Used Libraries::{TestSuitePath}");
            foreach(string deviceId in DeviceLibraryMapping.Keys)
            {
                Logger.Trace($"Device ID : {deviceId}");
                foreach (Library lib in DeviceLibraryMapping[deviceId])
                {
                    Logger.Trace($"{lib.Name} is used {lib.NameAs}");
                }
            }
        }

        public void ChangeDefaultDUTId(string dutID)
        {
            // Do nothing if default DUT is already set.
            if (dutID.Equals(DEFAULT_DUT)) return;


            if(DutDictionary.ContainsKey(DEFAULT_DUT) && !DutDictionary.ContainsKey(dutID))
            {
                DutDictionary.Add(dutID, DutDictionary[DEFAULT_DUT]);
                DutDictionary.Remove(DEFAULT_DUT);
            }
            if(DeviceLibraryMapping.ContainsKey(DEFAULT_DUT))
            {
                if(DeviceLibraryMapping.ContainsKey(dutID))
                {
                    DeviceLibraryMapping[dutID].AddRange(DeviceLibraryMapping[DEFAULT_DUT]);
                    DeviceLibraryMapping[dutID] = DeviceLibraryMapping[dutID].Distinct().ToList();

                }
                else
                {
                    DeviceLibraryMapping.Add(dutID, DeviceLibraryMapping[DEFAULT_DUT]);
                }
                DeviceLibraryMapping.Remove(DEFAULT_DUT);

            }
            DEFAULT_DUT = dutID;

        }

        public List<string> GetUsedDevice()
        {
            return DutDictionary.Keys.ToList();
        }

        public void SetDevice(string deviceID, DeviceUnderTest dut)
        {
            DutDictionary[deviceID] = dut;
        }


        public Library GetLibrary(string libNameAs)
        {
                return AllUsedLib.Where(l => l.NameAs.Equals(libNameAs, StringComparison.OrdinalIgnoreCase))?.First();
        }
    }
}
