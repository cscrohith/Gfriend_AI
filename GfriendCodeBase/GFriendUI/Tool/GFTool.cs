using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Xml;

namespace HP.GFriend.UI.Tool
{
    internal class GFTool
    {
        public string BasePath { get; set; }
        public string Executable { get; set; }
        public string ToolPath
        {
            get
            {
                return Path.Combine(BasePath, Executable);
            }
        }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public List<Arg> Arguments { get; set; }

        public GFTool(string gftoolFilePath)
        {
            BasePath = Path.GetDirectoryName(gftoolFilePath);

            XmlDocument gftool = new XmlDocument();
            gftool.Load(gftoolFilePath);
            Executable = gftool.GetElementsByTagName("executable").Item(0).InnerText.Replace(Environment.NewLine, "").Trim();
            DisplayName = gftool.GetElementsByTagName("displayname").Item(0).InnerText.Replace(Environment.NewLine, "").Trim();
            Description = gftool.GetElementsByTagName("description").Item(0).InnerText.Replace(Environment.NewLine, "").Trim();

            Arguments = new List<Arg>();
            foreach(XmlNode xn in gftool.SelectNodes("//args/arg"))
            {
                string sw = string.Empty;
                try
                {
                    sw = xn.ChildNodes.Cast<XmlNode>().Where(n => n.Name.Equals("switch"))?.First().InnerText.Replace(Environment.NewLine, "").Trim() ?? string.Empty;
                }
                catch (Exception)
                { }
                Arg a = new Arg()
                {
                    Name = xn.Attributes["name"].Value,
                    Mandatory = bool.Parse(xn.Attributes["mandatory"].Value),
                    Switch = sw,
                    TypeOfArg = (ArgType)Enum.Parse(typeof(ArgType), xn.ChildNodes.Cast<XmlNode>().Where(n => n.Name.Equals("value")).First().InnerText.Replace(Environment.NewLine, "").Trim())
                };
                Arguments.Add(a);

            }
        }

        public void Run(ArgData data)
        {
            if(data!=null)
            {
                foreach (Arg a in Arguments)
                {
                    a.Value = string.Empty;
                    switch (a.TypeOfArg)
                    {
                        case ArgType.DeviceAddress:
                            if (!string.IsNullOrEmpty(data.DeviceAddress))
                            {
                                a.Value = data.DeviceAddress;
                            }
                            break;
                        case ArgType.AdminId:
                            if (!string.IsNullOrEmpty(data.AdminId))
                            {
                                a.Value = data.AdminId;
                            }
                            break;
                        case ArgType.AdminPassword:
                            if (!string.IsNullOrEmpty(data.AdminPassword))
                            {
                                a.Value = data.AdminPassword;
                            }
                            break;
                        case ArgType.Port:
                            if (!string.IsNullOrEmpty(data.Port))
                            {
                                a.Value = data.Port;
                            }
                            break;
                        case ArgType.ScriptRoot:
                            if(!string.IsNullOrEmpty(data.ScriptRoot))
                            {
                                a.Value = "\"" + data.ScriptRoot + "\"";
                            }
                            break;
                        case ArgType.Custom:
                            using(InputArgForm inputDialog = new InputArgForm(a.Name))
                            {
                                inputDialog.ShowDialog();
                                a.Value = inputDialog.Value;
                            }
                            break;
                    }
                }
            }


            Process process = new Process();
            process.StartInfo.FileName = ToolPath;
            process.StartInfo.Arguments = string.Join(" ", Arguments.Select(s => s.ToString()));
            process.Start();

        }
    }
}
