using HP.GFriend.Core.Execution;
using HP.GFriend.GFLogger;
using System.Collections.Generic;
using System.Linq;

namespace HP.GFriend.Core.Custom
{
    public class CustomLibrary : TestSuite
    {
        public string FilePath { get; set; }
        public List<CustomKeyword> Keywords { get; internal set; }
        public List<string> FilesToUse { get; private set; }
        
        public Dictionary<string, string> Resources { get; set; }
        public CustomLibrary(string libraryFilePath, Dictionary<string, string> executionVariables = null)
        {
            Logger.Trace($"Creating custom library with {libraryFilePath}");
            Keywords = new List<CustomKeyword>();
            TestDataManager testDataManager = Parser.ParseTestSuite(libraryFilePath, executionVariables);
            FilesToUse = testDataManager.FilesToUse;
            Resources = testDataManager.Resources;
            TestSuite testSuite = testDataManager.TargetTestSuite;
            Logger.Trace("Parsing done");
            _testcases = testSuite._testcases;
            _usedLibrary = testSuite._usedLibrary;
            Name = testSuite.Name;
            FilePath = libraryFilePath;
            ConvertTestCaseToKeyword();
            FilePath = libraryFilePath;
            Description = testSuite.Description;
            Logger.Trace($"{Name} is crated");
        }

        private void ConvertTestCaseToKeyword()
        {
            foreach(TestCase c in _testcases)
            {
                Keywords.Add(new CustomKeyword(c));
            }
            _testcases = null;
        }

        public List<string> GetDependencies()
        {
            return _usedLibrary;
        }

        public CustomKeyword GetKeyword(string keywordName)
        {
            return Keywords.Where(s => s.Name.Equals(keywordName)).First();
        }

        public bool DutUsed()
        {
            bool dutUsed = false;
            Dictionary<string, Library> availableLib = LibraryUtils.GetAvailableLibraries();
            foreach(string lib in _usedLibrary)
            {
                dutUsed |= availableLib[lib].DutUsed();
            }
            return dutUsed;
        }

        public new void Run(TestDataManager testDataManager)
        {
            // Do Nothing
        }

        public new void Run(TestDataManager testDataManager, List<string> tcToRun)
        {
            // Do Nothing
        }
    }
}
