using System;
using System.IO;
using System.IO.Compression;
using System.Xml;

namespace HP.GFriend.Keywords
{
    public class HpkFileInfo
    {
        public string PackageName { get; set; }

        public string FilePath { get; set; }

        public string Uuid { get; set; }

        public string Version { get; set; }

        public HpkFileInfo(string fileName)
        {
            FilePath = fileName;
            LoadHpkXml(FilePath);
        }

        public void LoadHpkXml(string filepath)
        {
            using (FileStream hpkfile = new FileStream(filepath, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(hpkfile, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        {
                            XmlDocument xml = new XmlDocument();
                            Stream stream = entry.Open();
                            StreamReader reader = new StreamReader(stream);
                            xml.LoadXml(reader.ReadToEnd());
                            PackageName = xml.GetElementsByTagName("installFile")[0].InnerText;
                            Uuid = xml.GetElementsByTagName("uuid")[0].InnerText;
                        }
                    }
                }
            }
        }
    }
}
