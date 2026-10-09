using System;
using System.Collections.Generic;
using System.Linq;

namespace HP.GFriend.Keywords
{
    public class PrinterDriverOption
    {
        public string OptionName { get; set; }
        public ControlType OptionType { get; set; } = ControlType.UNKNOWN;
        public string AutomationId { get; set; }
        public Dictionary<string, string> Values { get; set; }

        public PrinterDriverOption()
        {
            Values = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            string valueStr = string.Join(Environment.NewLine, Values.Keys.ToList());
            string optionStr = $"[{OptionName} ({OptionType.ToString()} : {AutomationId})]{Environment.NewLine}{valueStr}";

            return optionStr;
        }

    }
}
