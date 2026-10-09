using HP.GFriend.GFLogger;
using HP.GFriend.Keywords;
using HP.GFriend.Keywords.Support;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GFK.Regex
{
    [LibraryDescription("<p><br>To start scripting with Regex library , first use Regex.Set Pattern (pattern) and then use the other keywords of Regex Library . If Regex.Set Pattern (pattern) is not used at first then the other keywords thrws error as \"Pattern is not set. Use Set Pattern keyword first.\" <div style=\"color:black\"><p>Sample Test cases : <p>${message}=Inc-123 is cretaed successfully.Please reach out to the team with the incident number Inc-123 <p>tc_MatchWord <p>{ <p>Regex.Set Pattern (\\bInc-\\d{3}\\b) //The regex \\bInc-\\d{3}\\b will match the string \"Inc-any 3 digits\" Ex : Inc-123 , Inc-789 <p>Regex.Get Match (${message},${saveto}) // outputs the match value in the entire string : Inc-123 <p>Regex.Get Match (${message},0,${saveto}) // outputs the match value in the entire string for a particular index : Inc-123  <p>Regex.Get Match Count (${message},${matchcount}) // outputs the match count in the entire string : Matched count : 2 <p>Regex.Is Match (${message}) // outputs the match value in the entire string : Inc-123 <p>}<p>tc_VersionPattern <p>{ <p>Regex.Set Pattern (^[1-9]\\d{0\\,1}\\.[1-9]\\d{0\\,2}\\.[1-9]\\d{0\\,1}\\.[1-9]\\d{0\\,2}$) //the pattern can be like [1-9][1or2digits].[1-9][1,2or3digits].[1-9][1or2digits].[1-9][1,2or3digits] Ex:2.55.1.24 <p>Regex.Get Match (2.55.1.6,${saveto}) // outputs the match value in the entire string : 2.55.1.6  <p>Regex.Get Match (2.55.1.6,0,${saveto}) // outputs the match value in the entire string : 2.55.1.6 <p>Regex.Get Match Count (2.55.1.6,${matchcount}) // outputs the match count in the entire string : 1 <p>Regex.Is Match (2.55.1.6) // outputs the match value in the entire string : 2.55.1.6<p>}</div>")]
    public class Regex : IGFLibrary
    {
        private System.Text.RegularExpressions.Regex _regex;
        private string _pattern = string.Empty;

        public void Dispose()
        {
            _regex = null;
        }

        public bool DutUsed()
        {
            return false;
        }

        public List<string> GetDependencies()
        {
            return null;
        }

        public string GetName()
        {
            return "Regex";
        }

        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
        }

        [KeywordDisplayName("Set Pattern")]
        [KeywordDescription("Set regular expression pattern")]
        [KeywordParameters("pattern", "regular expression pattern")]
        [SampleScript("<p><b>Sample Pattern1 - Pass sceanrio : </b>Regex.Set Pattern(\\bInc-\\d{3}\\b) <p> <b>Explanation of the pattern : </b><p>\\b: This is a word boundary anchor. It asserts a position where a word starts or ends. <p>bInc-\\d{3}: This is the literal string that you want to match. <p>The regex \\bInc-\\d{3}\\b will match the string \"Inc-any 3 digits\" . <p>Example : Inc-123 is cretaed successfully.Please reach out to the team with the incident number Inc-123 .<p>Here there are 2 occurances of the word Inc-123 at the start and end of the line . <p><br><b>Sample Pattern2 - Pass sceanrio : </b>Regex.Set Pattern (^[1-9]\\d{0\\,1}\\.[1-9]\\d{0\\,2}\\.[1-9]\\d{0\\,1}\\.[1-9]\\d{0\\,2}$)  <p>The actual pattern is ^[1-9]\\d{0,1}\\.[1-9]\\d{0,2}\\.[1-9]\\d{0,1}\\.[1-9]\\d{0,2}$ , added \\ before \",\" to escape the character \",\" . <p><b>Explanation of pattern : </b>^[1-9]\\d{0,1}\\.[1-9]\\d{0,2}\\.[1-9]\\d{0,1}\\.[1-9]\\d{0,2}$ <p>[1-9]\\d{0,1}-allows 1 or 2 digits which can be from 1-9 <p> \\.-this is to have \".\" <p>Example pattern : 2.54.1.29 <p> </br><b>Sample Pattern 3 - Fail sceanrio :</b> <p>Regex.Set Pattern (.*+123) // output : Can not initialize regex.")]
        public KeywordResult SetPattern(string pattern)
        {
            try
            {
                _pattern = pattern;
                _regex = new System.Text.RegularExpressions.Regex(_pattern);
            }
            catch(Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Can not initialize regex.";
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDisplayName("Is Match")]
        [KeywordDescription("Check if pattern is found in given input.")]
        [KeywordParameters("input", "input text to check")]
        [SampleScript("<p>Pass scenario : Regex.Is Match (2.55.1.6) // output : 2.55.1.6 <p>Fail sceario : Regex.Is Match (123.3445.355.35553) // output : Can not find string with given input and pattern.")]
        public KeywordResult IsMatch(string input)
        {
            if (_regex == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Pattern is not set. Use Set Pattern keyword first.";
                Logger.Error(error.Output);
                return error;
            }
            input = Utils.GetVariablevalueIfExist(input);

            Match match = _regex.Match(input);
            if (match.Success)
            {
                KeywordResult pass = new KeywordResult(KeywordResults.Pass);
                pass.Output = match.Value;
                Logger.Debug($"String matched : {match.Value}");
                return pass;
            }
            else
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Can not find string with given input and pattern.";
                Logger.Error(fail.Output);
                return fail;
            }

        }
    

        [GetKeyword]
        [KeywordDisplayName("Get Match")]
        [KeywordDescription("Searches the specified input string for the first occurrence of the regular expression")]
        [KeywordParameters("input", "input text to check")]
        [KeywordParameters("saveTo", "variable name to save value")]
        [SampleScript("<p>Pass scenario : Regex.Get Match (2.55.1.4,${saveto}) // output :2.55.1.4 <p>Fail sceario : Regex.Get Match (123.3445.355.35553,${saveto}) // output : Can not find string with given input and pattern.")]
        public KeywordResult GetMatch(string input, string saveTo)
        {
            if (_regex == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Pattern is not set. Use Set Pattern keyword first.";
                Logger.Error(error.Output);
                return error;
            }
            input = Utils.GetVariablevalueIfExist(input);

            Match match = _regex.Match(input);
            if(match.Success)
            {
                CommonExecutionInfo.SetVariable(saveTo, match.Value);
                KeywordResult pass = new KeywordResult(KeywordResults.Pass);
                pass.Output = match.Value;
                Logger.Debug($"String matched : {match.Value}");
                return pass;
            }
            else
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Can not find string with given input and pattern.";
                Logger.Error(fail.Output);
                return fail;
            }
            
        }

        [GetKeyword]
        [KeywordDisplayName("Get Match")]
        [KeywordDescription("Searches the specified input string for the occurrence at given index of the regular expression")]
        [KeywordParameters("input", "input text to check")]
        [KeywordParameters("index", "index of match. Start with 0.")]
        [KeywordParameters("saveTo", "variable name to save value")]
        [SampleScript("<p>Pass scenario : Regex.Get Match (2.54.1.7,0,${saveto}) // output : 2.54.1.7 <p>Fail sceario : Regex.Get Match (123.3445.355.35553,0,${saveto}) // output : Can not find string with given input and pattern.")]
        public KeywordResult GetMatch(string input, string index, string saveTo)
        {
            index = Utils.GetVariablevalueIfExist(index);
            if (!int.TryParse(index, out int iIdx))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Index must be a number";
                Logger.Error(error.Output);
                return error;
            }

            if (_regex == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Pattern is not set. Use Set Pattern keyword first.";
                Logger.Error(error.Output);
                return error;
            }
            input = Utils.GetVariablevalueIfExist(input);
            

            MatchCollection matchCollection = _regex.Matches(input);
            
            if (matchCollection.Count > 0)
            {
                CommonExecutionInfo.SetVariable(saveTo, matchCollection[iIdx].Value);
                KeywordResult pass = new KeywordResult(KeywordResults.Pass);
                pass.Output = matchCollection[iIdx].Value;
                Logger.Debug($"String matched : {matchCollection[iIdx].Value}");
                return pass;
            }
            else
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Can not find string with given input and pattern.";
                Logger.Error(fail.Output);
                return fail;
            }

        }

        [GetKeyword]
        [KeywordDisplayName("Get Match Count")]
        [KeywordDescription("Searches the specified input string for the occurrence of the regular expression and return count of matches.")]
        [KeywordParameters("input", "input text to check")]
        [KeywordParameters("saveTo", "variable name to save value")]
        [SampleScript("<p>Pass scenario : Regex.Get Match Count (2.55.1.8,${matchcount}) // output : \"Matched count : 1\" <p>Fail sceario : Regex.Get Match Count (123.3445.355.35553,${saveto}) // output : \"Matched count : 0\"")]
        public KeywordResult GetMatchCount(string input, string saveTo)
        {

            if (_regex == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Pattern is not set. Use Set Pattern keyword first.";
                Logger.Error(error.Output);
                return error;
            }
            input = Utils.GetVariablevalueIfExist(input);

            MatchCollection matchCollection = _regex.Matches(input);

            CommonExecutionInfo.SetVariable(saveTo, matchCollection.Count.ToString());
            KeywordResult pass = new KeywordResult(KeywordResults.Pass);
            pass.Output = $"Matched count : {matchCollection.Count}";
            Logger.Debug(pass.Output);
            return pass;

        }
    }
}
