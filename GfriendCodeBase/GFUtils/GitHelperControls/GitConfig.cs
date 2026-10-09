using Microsoft.Win32;
using System;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Management;

namespace HP.GFriend.Utils.Git
{
    [Serializable()]
    public class GitConfig
    {
        [XmlElement]
        public string UserName { get; set; }

        [XmlElement]
        public string UserEmail { get; set; }

        [XmlElement]
        public string EncryptedHttpToken { get; set; }


        [XmlIgnore]
        public string HttpToken
        {
            get
            {
                return Encrypt.StringCipher.Decrypt(EncryptedHttpToken, GetPassPharse());
            }

            set
            {
                EncryptedHttpToken = Encrypt.StringCipher.Encrypt(value, GetPassPharse());
            }
        }


        public GitConfig()
        {
            
        }

        public string GetPassPharse()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");

            ManagementObjectCollection information = searcher.Get();
            
            foreach (ManagementObject obj in information)
            {
                return obj.Properties["SerialNumber"].ToString();
            }
            return "___DEFAULT__PASS__PHARSE___";
        }

        public void Save(string filePath)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(GitConfig));
            using (FileStream savedFile = new FileStream(filePath, FileMode.Create))
            {
                xmlSerializer.Serialize(savedFile, this);
                savedFile.Flush();
                savedFile.Close();
            }
        }
        
        public static GitConfig Load(string filePath)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(GitConfig));
            using (XmlReader reader = XmlReader.Create(filePath))
            {
                GitConfig config = (GitConfig)xmlSerializer.Deserialize(reader);
                return config;
            }
        }
    }
}
