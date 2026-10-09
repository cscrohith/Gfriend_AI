using HP.GFriend.Keywords;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace HP.GFriend.Core.Execution.Spec
{
    [Serializable()]
    [XmlRoot("TestSuite")]
    public class TestRun
    {
        [XmlElement]
        public string TestSuitePath { get; set; }

        [XmlElement("DeviceUnderTest")]
        public List<DeviceUnderTest> DeviceUnderTests { get; set; }

        [XmlAttribute("Repeat")]
        public int RepeatCount { get; set; } = 1;

        [XmlElement]
        public string DefaultDevice { get; set; }

        [XmlElement]
        public string OutputPath { get; set; }

        [XmlElement("TestCase")]
        public List<string> TestCaseToRun { get; set; }

        public TestRun()
        {
            TestCaseToRun = new List<string>();
        }

    }
}

