using HP.GFriend.Keywords;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace HP.GFriend.UI.Device
{
    [Serializable]
    public class DeviceInfo
    {
        private string _deviceInfoPath;
        [XmlElement]
        public List<DeviceUnderTest> Devices { get; set; }

        public DeviceInfo()
        {
            Devices = new List<DeviceUnderTest>();
        }

        public DeviceInfo(string xmlPath)
        {
            _deviceInfoPath = xmlPath;
            Devices = new List<DeviceUnderTest>();
        }

        public List<string> GetDeviceIds()
        {
            if(!string.IsNullOrEmpty(_deviceInfoPath) && File.Exists(_deviceInfoPath))
            {
                Load();
            }
            return Devices.Select(d => d.DeviceId).ToList();
        }

        public DeviceUnderTest GetDevice(string deviceId)
        {
            return Devices.Where(d => d.DeviceId.Equals(deviceId))?.FirstOrDefault() ?? null;
        }

        public void Update(DeviceUnderTest dut)
        {
            if(GetDevice(dut.DeviceId) == null)
            {
                Devices.Add(dut);
            }
            else
            {
                Devices[Devices.IndexOf(GetDevice(dut.DeviceId))] = dut;
            }
            Save(_deviceInfoPath);
        }

        public void Remove(string deviceId)
        {
            Devices.Remove(GetDevice(deviceId));
            Save(_deviceInfoPath);
        }

        public void Load()
        {
            using (XmlReader reader = XmlReader.Create(_deviceInfoPath))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(DeviceInfo));
                DeviceInfo deviceInfo = (DeviceInfo)xmlSerializer.Deserialize(reader);
                Devices = deviceInfo.Devices;
            }
        }
        public static DeviceInfo Load(string xmlFilePath)
        {
            using (XmlReader reader = XmlReader.Create(xmlFilePath))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(DeviceInfo));
                DeviceInfo deviceInfo = (DeviceInfo)xmlSerializer.Deserialize(reader);
                deviceInfo._deviceInfoPath = xmlFilePath;
                return deviceInfo;
            }
        }

        public void Save(string xmlFilePath)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(DeviceInfo));
            using(StreamWriter writer = new StreamWriter(xmlFilePath, false, Encoding.UTF8))
            {
                xmlSerializer.Serialize(writer, this);
                writer.Flush();
                writer.Close();
            }
        }
    }
}
