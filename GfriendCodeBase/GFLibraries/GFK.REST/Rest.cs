using HP.GFriend.Keywords;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using HP.GFriend.GFLogger;
using System.Net;

namespace HP.GFirend.Keywords
{
    public class Rest : IGFLibrary
    {
        private HttpRequestMessage _request;
        private string _url;
        private HttpMethod _method;
        private Encoding _encoding;

        public void Dispose()
        {
            
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
            return "Rest";
        }

        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
            //ServicePointManager.ServerCertificateValidationCallback +=
            //    (sender, cert, chain, sslPolicyErrors) => true;

        }

        public bool IgnoreCeritificatteValidation(object sender, System.Security.Cryptography.X509Certificates.X509Certificate cert, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        internal async Task<GFRestResult> SendRequestAsync()
        {
            using(HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.SendAsync(_request);
                GFRestResult restResult = new GFRestResult();
                restResult.Status = response.StatusCode;
                restResult.ResponseBody = await response.Content.ReadAsStringAsync();
                restResult.IsSuccess = response.IsSuccessStatusCode;
                return restResult;
            }
        }

        [KeywordDescription("Initialize Request with default UTF-8 encoding. This keyword must be called in first place of REST API request.")]
        [KeywordDisplayName("Initialize Request")]
        [KeywordParameters("url", "request url")]
        [KeywordParameters("method", "One of GET, POST, PUT, and DELETE")]
        [SampleScript(" Rest.Initialize Request (https://dummyjson.com/auth/login,POST)")]
        public KeywordResult InitializeRequest(string url, string method)
        {
            return InitializeRequest(url, method, "UTF-8");
        }

        [KeywordDescription("Initialize Request. This keyword must be called in first place of REST API request.")]
        [KeywordDisplayName("Initialize Request")]
        [KeywordParameters("url", "request url")]
        [KeywordParameters("method", "One of GET, POST, PUT, and DELETE")]
        [KeywordParameters("encoding", "Encoding of to use in request and response. (ex. UTF-8)")]
        [SampleScript(" Rest.Initialize Request (https://dummyjson.com/auth/login,POST,UTF-8)")]
        public KeywordResult InitializeRequest(string url, string method, string encoding)
        {
            _url = url;
            switch(method.Trim().ToUpper())
            {
                case "GET":
                    _method = HttpMethod.Get;
                    break;
                case "POST":
                    _method = HttpMethod.Post;
                    break;
                case "DELETE":
                    _method = HttpMethod.Delete;
                    break;
                case "PUT":
                    _method = HttpMethod.Put;
                    break;
                default:
                    KeywordResult error = new KeywordResult(KeywordResults.Error);
                    error.Output = $"Can not recognize http method : {method}";
                    Logger.Error(error.Output);
                    return error;
            }
            _request = new HttpRequestMessage(_method, _url);

            try
            {
                _encoding = Encoding.GetEncoding(encoding);
            }
            catch(Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during set encoding. Please check if encoding is valid.";
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;
            }

            return new KeywordResult(KeywordResults.Pass);
        }
        [KeywordDescription("Ignore security warning for invalid certificate.\r\n" +
            "Take caution to use this keyword and use Enable Security Warning to rollback settings.")]
        [KeywordDisplayName("Disable Security Warning")]
        [SampleScript("Rest.Disable Security Warning()")]
        public KeywordResult DisableSecurityWarning()
        {
            ServicePointManager.ServerCertificateValidationCallback += IgnoreCeritificatteValidation;
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Set security warning for invalid certificate.\r\n" +
            "This is default behavior of request, you do not need call this except after calling Disable Security Warning")]
        [KeywordDisplayName("Enable Security Warning")]
        [SampleScript("Rest.Enable Security Warning()")]
        public KeywordResult EnableSecurityWarning()
        {
            ServicePointManager.ServerCertificateValidationCallback -= IgnoreCeritificatteValidation;
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Add HTTP header to request")]
        [KeywordDisplayName("Add Header")]
        [KeywordParameters("name", "name of header")]
        [KeywordParameters("value", "value of header")]
        [SampleScript("Rest.Add Header (Connection,keep-alive)")]
        public KeywordResult AddHeader(string name, string value)
        {
            _request.Headers.Add(name, value);
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Add basich authentication of request")]
        [KeywordDisplayName("Add Basic Auth")]
        [KeywordParameters("username", "username for request")]
        [KeywordParameters("password", "password for request")]
        [SampleScript("Rest.Add Basic Auth (${username},${password})")]
        public KeywordResult AddBasicAuth(string username, string password)
        {
            string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
            _request.Headers.Add("Authorization", "Basic " + encoded);
            return new KeywordResult(KeywordResults.Pass);
        }

        [GetKeyword]
        [KeywordDescription("Send request and get resonse to variable")]
        [KeywordDisplayName("Send Request")]
        [KeywordParameters("response", "variable name to save response")]
        [SampleScript("Rest.Send Request (${response})")]
        public KeywordResult SendRequest(string response)
        {
            Task<GFRestResult> httpTask = Task.Run(() => SendRequestAsync());
            httpTask.Wait();
            GFRestResult result = httpTask.Result;
            CommonExecutionInfo.SetVariable(response, result.ResponseBody);
            
            KeywordResult keywordResult = new KeywordResult(KeywordResults.Pass);
            keywordResult.Output = result.ResponseBody;

            if(!result.IsSuccess)
            {
                keywordResult.Result = KeywordResults.Fail;
            }

            return keywordResult;
        }

        [KeywordDescription("Add data to request body with json format")]
        [KeywordDisplayName("Add Json Data")]
        [KeywordParameters("jsonData", "json formatted data")]
        [SampleScript(" Rest.Add Json Data(${ jsonData}) \r\n Usgae - Read From File (${jsonpath},${orderid_json_output}\r\n Json.Load Json(${orderid_json_output})\r\n Json.Get Value(${ jsonData},$.[1].friendlySubscriptionId)\r\n Rest.Add Json Data (${jsonData})")]
        public KeywordResult AddJsonData(string jsonData)
        {
            StringContent content = new StringContent(jsonData);
            System.Net.Http.Headers.MediaTypeHeaderValue contentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            contentType.CharSet = _encoding.WebName;
            content.Headers.ContentType = contentType;
            _request.Content = content;

            return new KeywordResult(KeywordResults.Pass);
        }
    }
}
