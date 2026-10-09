using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HP.GFriend.GFLogger;
using HP.GFriend.Keywords.Support;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HP.GFriend.Keywords
{
    [LibraryDescription("Many keywords in Json library work with Json Path to specify element within json formatted text.\n" +
        "Please refer https://restfulapi.net/json-jsonpath/ for more detail of json path syntax.")]
    public class Json : IGFLibrary
    {
        private JObject _jObjectToParse;
        private JArray _jArrayObjectToParse;

        public void Dispose()
        {
            _jObjectToParse = null;
            _jArrayObjectToParse = null;
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
            return "Json";
        }

        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
            
        }

        [KeywordDescription("Load Json from json formatted text")]
        [KeywordDisplayName("Load Json")]
        [KeywordParameters("jsonText", "json formatted text")]
        [SampleScript("Json.Load Json (C:\\Users\\ReVi345\\Downloads\\675\\orderid.json)")]
        public KeywordResult LoadJson(string jsonText)
        {
            try
            {
                if (jsonText.StartsWith("["))
                {
                    _jArrayObjectToParse = JArray.Parse(jsonText);
                    _jObjectToParse = null;
                }
                else
                {
                    _jObjectToParse = JObject.Parse(jsonText);
                }
            }
            catch(Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Json parsing error.";
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                _jObjectToParse = null;
                return error;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [GetKeyword]
        [KeywordDescription("Get value of given json path")]
        [KeywordDisplayName("Get Value")]
        [KeywordParameters("saveTo", "varialbe name to save value")]
        [KeywordParameters("jsonPath", "path of value to get")]
        [SampleScript("Json.Get Value (${first_name},$.customer_firstname)")]
        public KeywordResult GetValue(string saveTo, string jsonPath)
        {
            jsonPath = Utils.GetVariablevalueIfExist(jsonPath);
            if(_jObjectToParse == null && _jArrayObjectToParse == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Json is not loaded. Use Load Json keyword before using Get Value.";
                Logger.Error(error.Output);
                return error;
            }
            JToken token = null;
            if (_jObjectToParse != null)
            {
                token = _jObjectToParse.SelectToken(jsonPath);
            }
            else if (_jArrayObjectToParse.Any())
            {
                token = _jArrayObjectToParse.SelectToken(jsonPath);
            }

            if (token == null)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Error);
                fail.Output = $"Can not find item with given json path ({jsonPath}) in json string ({_jObjectToParse})";
                Logger.Error(fail.Output);
                return fail;
            }
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            result.Output = token.ToString();
            CommonExecutionInfo.SetVariable(saveTo, token.ToString());
            return result;
        }

        [GetKeyword]
        [KeywordDescription("Add json element to root of target variable with given name and value")]
        [KeywordDisplayName("Add Json Element")]
        [KeywordParameters("target", "varialbe name which stores json formatted text")]
        [KeywordParameters("name", "name of element to add")]
        [KeywordParameters("value", "value of element to add")]
        [SampleScript("Json.Add Json Element (${body},text,Hello GFriend)")]
        public KeywordResult AddJsonElement(string target, string name, string value)
        {
            name = Utils.GetVariablevalueIfExist(name);
            value = Utils.GetVariablevalueIfExist(value);
            bool isNew = false;
            JObject jResult;
            string result = CommonExecutionInfo.GetVariable(target);
            if(string.IsNullOrEmpty(result))
            {
                jResult = new JObject();
                isNew = true;
            }
            else
            {
                try
                {
                    jResult = JObject.Parse(result);
                }
                catch(Exception ex)
                {
                    KeywordResult error = new KeywordResult(KeywordResults.Error);
                    error.Output = $"Target varialbe is not json formatted string : {result}";
                    error.AdditionalInfo = ex.ToString();
                    Logger.Error(error.Output, ex);
                    return error;
                }
            }


            JToken toAdd = null;

            try
            {
                JToken addValue = JToken.Parse(value);
                toAdd = new JProperty(name, addValue);
            }
            catch (Exception) 
            {
                toAdd = new JProperty(name, value);
            }

            jResult.Add(toAdd);

            KeywordResult pass = new KeywordResult(KeywordResults.Pass);
            pass.Output = jResult.ToString();

            CommonExecutionInfo.SetVariable(target, jResult.ToString());
            return pass;

        }
    }
}
