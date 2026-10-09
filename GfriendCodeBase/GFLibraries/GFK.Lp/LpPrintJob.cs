using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HP.GFriend.Keywords
{
    public class LpPrintJob
    {
        /// <summary>
        /// File to Print
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// Destination of print job (Printer)
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// Number of copies
        /// </summary>
        public int NumCopies { get; set; } = -1;

        /// <summary>
        /// Options
        /// </summary>
        public List<Tuple<string, string>> Options { get; private set; }


        /// <summary>
        /// Create new instance of LpPrintJob
        /// </summary>
        public LpPrintJob()
        {
            Options = new List<Tuple<string, string>>();
        }

        /// <summary>
        /// Add option with value
        /// </summary>
        /// <param name="option">option name</param>
        /// <param name="value">option value</param>
        public void AddOption(string option, string value)
        {
            Options.Add(new Tuple<string, string>(option, value));
        }

        /// <summary>
        /// Add option which do not have value
        /// </summary>
        /// <param name="option">option name</param>
        public void AddOption(string option)
        {
            Options.Add(new Tuple<string, string>(option, string.Empty));
        }

        /// <summary>
        /// Get command-line lp command
        /// </summary>
        /// <returns>lp command</returns>
        public string GetLpCommand()
        {
            string command = "lp ";
            if(!string.IsNullOrEmpty(Destination))
            {
                command += $"-d '{Destination}' ";
            }

            if (NumCopies > 0)
            {
                command += $"-n {NumCopies} ";
            }

            if (Options.Count > 0)
            {
                foreach(Tuple<string, string> option in Options)
                {
                    string optionArg;
                    if(string.IsNullOrEmpty(option.Item2))
                    {
                        optionArg = $"-o '{option.Item1}' ";
                    }
                    else
                    {
                        optionArg = $"-o '{option.Item1}={option.Item2}' ";
                    }
                    command += optionArg;
                }

            }

            command += File;

            return command;
        }
    }
}
