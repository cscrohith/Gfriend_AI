using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;

namespace HP.GFriend.Updater
{
    public class GFTools
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "version")]
        public string Version { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        public override string ToString()
        {
            string[] lines = Description.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            string desc = string.Join(Environment.NewLine, lines.Where(w => !string.IsNullOrEmpty(w.Trim())).Select(s => "\t" + s).ToArray());
            string type = string.Empty;

            if (Type != null)
            {
                type = $"({Type})";
            }
            return $"{Name} : {Version} {type}{Environment.NewLine}{desc}";
        }
    }
}
