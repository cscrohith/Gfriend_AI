using HP.GFriend.Core.Execution;
using HP.GFriend.Keywords;
using HP.GFriend.Support;
using System;
using System.Linq;
using System.Collections.Generic;


namespace HP.GFriend.Core.Custom
{
    public class CustomKeyword : TestCase
    {
        private Dictionary<string, string> _arguments = null;
        public int NumOfArgs { get; set; } = 0;
        public string Args = string.Empty;
        public string Description { get; }
        public CustomKeyword(TestCase parsedTC)
        {
            parsedTC.Name = parsedTC.Name.Replace("()", "");
            Statement tempStatement = Parser.ParseStatemet(parsedTC.Name);
            _arguments = new Dictionary<string, string>();
            if (tempStatement.Parameters != null)
            {
                NumOfArgs = tempStatement.Parameters.Length;
                foreach(string s in tempStatement.Parameters)
                {
                    _arguments.Add(s.Trim(), string.Empty);
                }

                if(parsedTC.MetaDataDic.ContainsKey("parameters"))
                {
                    Args = parsedTC.MetaDataDic["parameters"];
                }
            }
            else
            {
                Args = "NO Argument";
            }
            Name = parsedTC.Name.Split('(')[0].Trim();
            
            if(parsedTC.MetaDataDic.ContainsKey("description"))
            {
                Description = parsedTC.MetaDataDic["description"];
            }
            
            subBlocks = parsedTC.subBlocks;
            tagName = "CustomKeyword";
        }


        public KeywordResult Run(TestDataManager testDataManager, object[] parameters, int repeat = -1)
        {
            if(parameters != null)
            {
                if(parameters.Length != _arguments.Count)
                {
                    KeywordResult error = new KeywordResult(KeywordResults.Error);
                    error.AdditionalInfo = "Parameter count is not matched";
                    return error;
                }
                for(int i =0; i<_arguments.Count; i++)
                {
                    string key = _arguments.Keys.ElementAt(i);
                    _arguments[key] = parameters[i].ToString().Trim();
                }
            }

            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            Reporter.WriteToOutput("StartTime", Utils.GetTime());
            Console.WriteLine("");
            foreach (IGFRunnable subBlock in subBlocks)
            {
                subBlock.Run(testDataManager, repeat,  1, _arguments);
                if (subBlock.Result.ToUpper().Equals("FAIL"))
                {
                    result.Result = KeywordResults.Fail;
                }
                else if (subBlock.Result.ToUpper().Equals("ERROR"))
                {
                    result.Result = KeywordResults.Error;
                }
                if (BuiltInLibrary.ExitTC || BuiltInLibrary.ExitTS ||  BuiltInLibrary.ExitCustomKeyword || BuiltInLibrary.Break)
                {
                    if(BuiltInLibrary.ExitCustomKeyword)
                    {
                        BuiltInLibrary.ExitCustomKeyword = false;
                        result.Result = BuiltInLibrary.ForcedCustomKeywordResult;
                        result.Output = CommonExecutionInfo.GetVariable("${KEYWORD_OUTPUT}");
                    }
                    
                    break;
                }
                else if (BuiltInLibrary.Continue)
                {
                    BuiltInLibrary.Continue = false;
                    break;
                }
            }

            Reporter.WriteToOutput("Result", result.Result.ToString().ToUpper());
            Reporter.WriteToOutput("EndTime", Utils.GetTime());
            return result;
        }
    }
}
