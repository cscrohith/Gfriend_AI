using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HP.GFriend.Core.Execution.Spec
{
    public class RunSpecEventArgs
    {
        public TestRuns TestRuns { get; }
        public TestRun TestRun { get; }
        public string TestSuite { get; }
        public string OutputPath { get; }

        public RunSpecEventArgs(TestRuns runs, TestRun run, string suite, string output)
        {
            TestRuns = runs;
            TestRun = run;
            TestSuite = suite;
            OutputPath = output;
        }
    }
}
