using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace HP.GFriend.Core.Execution.Spec
{
    [Serializable()]
    [XmlRoot("TestRuns")]
    public class TestRuns
    {
        public TestRuns()
        {
            TestsToRun = new List<TestRun>();
        }

        [XmlAttribute("Repeat")]
        public int RepeatCount { get; set; } = 1;
        [XmlElement("TestSuite")]
        public List<TestRun> TestsToRun { get; set; }

        [XmlElement]
        public string OutputPath { get; set; }


    }
}
