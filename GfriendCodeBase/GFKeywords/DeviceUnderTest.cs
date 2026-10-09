using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace HP.GFriend.Keywords
{
    [Serializable()]
    [XmlRoot("DeviceUnderTest")]
    public class DeviceUnderTest
    {
        [Serializable]
        public class Capability
        {
            [XmlElement]
            public string Key { get; set; }
            [XmlElement]
            public string Value { get; set; }

            public Capability()
            { }
        }

        [XmlElement]
        public string DeviceId { get; set; }

        [XmlElement]
        public string Description { get; set; }

        [XmlElement]
        public string DeviceAddress { get; set; }

        [XmlElement]
        public string LanDebugAddress { get; set; }

        [XmlElement]
        public int Port { get; set; }

        [XmlElement]
        public string AdminId { get; set; }

        [XmlElement]
        public string AdminPassword { get; set; }

        [XmlElement]
        public string DeviceType { get; set; }

        [XmlElement]
        public List<Capability> AdditionalCapabilites { get; set; }
        
        public DeviceUnderTest() 
        {
            AdditionalCapabilites = new List<Capability>();
        }

        public DeviceUnderTest(string deviceAddress)
        {
            DeviceAddress = deviceAddress;
            AdditionalCapabilites = new List<Capability>();
        }

        public string GetAdditionalCapability(string key)
        {
            return AdditionalCapabilites.Where(c => c.Key.Equals(key))?.FirstOrDefault()?.Value ?? string.Empty;
        }
    }
}
