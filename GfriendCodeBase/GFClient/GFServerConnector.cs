using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace HP.GFriend.Client
{
    public class GFServerConnector
    {
        public string GFServer { get; set; }

        public GFServerConnector()
        {
            GFServer = "http://localhost:8000";
        }

        public GFServerConnector(string serverEndpoint)
        {
            GFServer = serverEndpoint.TrimEnd('/');
        }

        private async Task<GFServerResult> GetScriptListAsync()
        {
            JArray jsonArray = new JArray();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync($"{GFServer}/api/scripts");
                    string contents = await response.Content.ReadAsStringAsync();
                    jsonArray = JArray.Parse(contents);
                }

                GFServerResult ret = new GFServerResult(RequestStatus.Success);
                foreach (JObject j in jsonArray)
                {
                    ret.DataList.Add(new Script(j));
                }
                return ret;
            }
            catch (Exception)
            {
                return new GFServerResult(RequestStatus.ConnectionError, "Server-side connection error while getting all scripts list");
            }
        }

        public GFServerResult GetScriptList()
        {
            Task<GFServerResult> getTask = Task.Run(() => GetScriptListAsync());
            getTask.Wait();
            return getTask.Result;
        }

        private async Task<GFServerResult> GetScriptInfoAsync(int scriptId)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync($"{GFServer}/api/scripts/{scriptId}/info");
                    string contents = await response.Content.ReadAsStringAsync();
                    JObject script = JObject.Parse(contents);
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return new GFServerResult(RequestStatus.KeyError, "Can not find script with given key");
                    }
                    else
                    {
                        return new GFServerResult(new Script(script));
                    }
                }
            }
            catch(Exception)
            {
                return new GFServerResult(RequestStatus.ConnectionError, "Server-side connection error while getting script info");
            }
        }

        public GFServerResult GetScriptInfo(int scriptId)
        {
            Task<GFServerResult> getTask = Task.Run(() => GetScriptInfoAsync(scriptId));
            getTask.Wait(TimeSpan.FromSeconds(5));
            return getTask.Result;
        }

        private async Task<Byte[]> GetScriptAsync(int scriptId)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync($"{GFServer}/api/scripts/{scriptId}");
                    Byte[] contents = await response.Content.ReadAsByteArrayAsync();
                    return contents;
                }
            }
            catch(Exception)
            {
                throw new GFServerException("Server error");
            }
        }

        public GFServerResult GetScript(int scriptId, string localScriptRoot)
        {
            GFServerResult getResult = GetScriptInfo(scriptId);
            if(!getResult.Status.Equals(RequestStatus.Success))
            {
                return getResult;
            }
            Script script =(Script)getResult.Data;

            Task<Byte[]> getTask = Task.Run(() => GetScriptAsync(scriptId));
            try
            {
                getTask.Wait();
            }
            catch(GFServerException)
            {
                return new GFServerResult(RequestStatus.ServerError, "Server-side error while downloading script");
            }
            
            if(getTask.Result == null)
            {
                return new GFServerResult(RequestStatus.KeyError, "Can not find script with specified key");
            }

            try
            {
                script.AbsolutePath = Path.Combine(localScriptRoot, script.StoredPath.Replace('/', '\\'));
                Directory.CreateDirectory(Path.GetDirectoryName(script.AbsolutePath));
                using (FileStream f = new FileStream(script.AbsolutePath, FileMode.Create))
                {
                    f.Write(getTask.Result, 0, getTask.Result.Length);
                    f.Flush();
                }
                return new GFServerResult(script);
            }
            catch(Exception)
            {
                return new GFServerResult(RequestStatus.UnExpected, "Unexpeced error during file operation");
            }
            
        }

        private async Task<GFServerResult> UploadScriptAsync(Script toUpload)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // create data
                    StringContent content = new StringContent(toUpload.ToJson());
                    content.Headers.Remove("Content-Type");
                    content.Headers.Add("Content-Type", "application/json");
                    HttpResponseMessage response = await client.PostAsync($"{GFServer}/api/scripts/upload/", content);
                    if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                    {
                        return new GFServerResult(RequestStatus.ServerError, "Server-side error while creating meta-data in the server");
                    }
                    string result = await response.Content.ReadAsStringAsync();
                    Script ret = new Script(JObject.Parse(result));
                    ret.AbsolutePath = toUpload.AbsolutePath;

                    // upload file
                    GFServerResult uploadResult = await UploadFileAsync(ret);
                    return uploadResult;
                }
            }
            catch(Exception)
            {
                return new GFServerResult(RequestStatus.UnExpected, "Unexpected error while uploing script to the server");
            }
            
        }

        private async Task<GFServerResult> UploadFileAsync(Script toUpload)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    FileInfo info = new FileInfo(toUpload.AbsolutePath);
                    FileStream fs = info.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    StreamContent content = new StreamContent(fs);
                    content.Headers.Add("Content-Type", "text/plain");
                    content.Headers.Add("Content-Disposition", $"attachment; filename={Path.GetFileName(toUpload.AbsolutePath)}");

                    HttpResponseMessage response = await client.PutAsync($"{GFServer}/api/scripts/{toUpload.Id}/upload/", content);

                    if(response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                    {
                        return new GFServerResult(RequestStatus.ServerError, "Server-side error while uploading file to the server");
                    }
                    return new GFServerResult(toUpload);
                }
            }
            catch(Exception)
            {
                return new GFServerResult(RequestStatus.ConnectionError, "Server-side connection error while uploading file to the server");
            }
        }

        public GFServerResult UploadScript(Script toUpload)
        {
            Task<GFServerResult> uploadTask = Task.Run(() => UploadScriptAsync(toUpload));
            uploadTask.Wait();
            return uploadTask.Result;
        }

        public GFServerResult UpdateScriptFile(Script toUpdate)
        {
            Task<GFServerResult> UpdateScriptFileTask = Task.Run(() => UploadFileAsync(toUpdate));
            UpdateScriptFileTask.Wait();
            return UpdateScriptFileTask.Result;

        }

        private async Task<GFServerResult> UpdateScriptInfoAsync(Script toUpdate)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // create data
                    StringContent content = new StringContent(toUpdate.ToJson());
                    content.Headers.Remove("Content-Type");
                    content.Headers.Add("Content-Type", "application/json");
                    HttpResponseMessage response = await client.PutAsync($"{GFServer}/api/scripts/{toUpdate.Id}/update/", content);
                    if (response.IsSuccessStatusCode)
                    {
                        return new GFServerResult(RequestStatus.Success);
                    }
                    return new GFServerResult(RequestStatus.ServerError, "Server-side error while updating script info");

                }
            }
            catch(Exception)
            {
                return new GFServerResult(RequestStatus.ConnectionError, "Server-side connection error while updating script info");
            }
        }

        public GFServerResult UpdateScriptInfo(Script toUpdate)
        {
            Task<GFServerResult> updateTask = Task.Run(() => UpdateScriptInfoAsync(toUpdate));
            updateTask.Wait();
            return updateTask.Result;
        }

        // For Galaxy Project
        // Test Execution

        private async Task<GFServerResult> StartTestProjectAsync(TestProject testProject)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // create data
                    StringContent content = new StringContent(testProject.ToJson());
                    content.Headers.Remove("Content-Type");
                    content.Headers.Add("Content-Type", "application/json");
                    HttpResponseMessage response = await client.PostAsync($"{GFServer}/api/execution/start/", content);
                    if (response.IsSuccessStatusCode)
                    {
                        string contents = await response.Content.ReadAsStringAsync();
                        JObject jTestProject = JObject.Parse(contents);
                        TestProject tp = new TestProject(jTestProject);
                        if(string.IsNullOrEmpty(tp.JenkinsJobURL) || tp.JenkinsJobURL.Equals("None"))
                        {
                            return new GFServerResult(RequestStatus.ShortOfExecutor, "Test resource (Remote Test Executor) is not enough");
                        }
                        return new GFServerResult(tp);
                    }
                    else
                    {
                        return new GFServerResult(RequestStatus.ServerError, "Server-side error while starting test project");
                    }
                }
            }
            catch(Exception)
            {
                return new GFServerResult(RequestStatus.ConnectionError, "Server-side connection error while starting test project");
            }
        }
        public GFServerResult StartTestProject(TestProject testProject)
        {
            Task<GFServerResult> startTPTask = Task.Run(() => StartTestProjectAsync(testProject));
            startTPTask.Wait();
            
            return startTPTask.Result;
        }

        private async Task<GFServerResult> EndTestProjectAsync(TestProject testProject)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    StringContent content = new StringContent(testProject.ToJson());
                    content.Headers.Remove("Content-Type");
                    content.Headers.Add("Content-Type", "application/json");

                    HttpResponseMessage response = await client.PutAsync($"{GFServer}/api/execution/{testProject.ExecutionID}/end/", content);
                    if (response.IsSuccessStatusCode)
                    {
                        return new GFServerResult(RequestStatus.Success);
                    }
                    return new GFServerResult(RequestStatus.ServerError, "Server-side error while ending test project");

                }
            }
            catch(Exception)
            {
                return new GFServerResult(RequestStatus.ConnectionError, "Server-side connection error while ending test project");
            }
            
        }
        public GFServerResult EndTestProject(TestProject testProject)
        {
            Task<GFServerResult> endTPTask = Task.Run(() => EndTestProjectAsync(testProject));
            endTPTask.Wait();

            return endTPTask.Result;
        }

        private async Task<GFServerResult> UpdateTestResultAsync(TestResult testResult)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // create data
                    StringContent content = new StringContent(testResult.ToJson());
                    content.Headers.Remove("Content-Type");
                    content.Headers.Add("Content-Type", "application/json");
                    HttpResponseMessage response = await client.PostAsync($"{GFServer}/api/execution/{testResult.ExecutionID}/update/", content);
                    if (response.IsSuccessStatusCode)
                    {
                        string contents = await response.Content.ReadAsStringAsync();
                        JObject jTestResult = JObject.Parse(contents);
                        return new GFServerResult(new TestResult(jTestResult));
                    }
                    else
                    {
                        return new GFServerResult(RequestStatus.ServerError, "Server-side error while updating test result");
                    }
                    
                }
            }
            catch(Exception)
            {
                return new GFServerResult(RequestStatus.ConnectionError, "Server-side connection error while updating test result");
            }
        }

        public GFServerResult UpdateTestResult(TestResult testResult)
        {
            Task<GFServerResult> updateTask = Task.Run(() => UpdateTestResultAsync(testResult));
            updateTask.Wait();
            return updateTask.Result;
        }

        private async Task<GFServerResult> UploadSingleTestResult(string executionID, string filePathToUplaod, string outputDirPath)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    FileInfo info = new FileInfo(filePathToUplaod);
                    FileStream fs = info.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    StreamContent content = new StreamContent(fs);
                    content.Headers.Add("Content-Type", "text/plain");
                    content.Headers.Add("Content-Disposition", $"attachment; filename='{Path.GetFileName(filePathToUplaod)}'");
                    content.Headers.Add("FilePath", filePathToUplaod.Replace(outputDirPath, "").Replace(Path.DirectorySeparatorChar, '/').Trim('/'));
                    HttpResponseMessage response = await client.PutAsync($"{GFServer}/api/execution/{executionID}/upload/", content);
                    if (response.IsSuccessStatusCode)
                    {
                        return new GFServerResult(RequestStatus.Success);
                    }
                    else
                    {
                        return new GFServerResult(RequestStatus.ServerError, "Server-side error while uploading test result");
                    }
                    
                }
            }
            catch(Exception)
            {
                return new GFServerResult(RequestStatus.ConnectionError, "Server-side connection error while uploading test result");
            }
            
        }

        public GFServerResult UploadTestResult(string executionID, string outputDirPath)
        {
            string[] allFile = Directory.GetFiles(outputDirPath, "*.*", SearchOption.AllDirectories);
            GFServerResult result = new GFServerResult(RequestStatus.Success);
            foreach(string resultFile in allFile)
            {
                Task<GFServerResult> uploadTask = Task.Run(() => UploadSingleTestResult(executionID, resultFile, outputDirPath));
                uploadTask.Wait();
                if(uploadTask.Result.Status != RequestStatus.Success)
                {
                    result.Status = RequestStatus.UnExpected;
                    result.Message = "Fail to upload one or more files";
                }
            }
            return result;
        }


        // Result view and flow control
        private async Task<GFServerResult> GetTestProjectsAsync()
        {
            JArray jsonArray = new JArray();
            try
            {
                using (HttpClient client = new HttpClient())
                {

                    HttpResponseMessage response = await client.GetAsync($"{GFServer}/api/execution");
                    string contents = await response.Content.ReadAsStringAsync();
                    jsonArray = JArray.Parse(contents);
                }
            }
            catch(Exception)
            {
                return new GFServerResult(RequestStatus.ConnectionError, "Server-side connection error while getting list of test projects");
            }


            GFServerResult result = new GFServerResult(RequestStatus.Success);
            foreach (JObject j in jsonArray)
            {
                result.DataList.Add(new TestProject(j));
            }
            return result;
            
        }

        public GFServerResult GetTestProjects()
        {
            Task<GFServerResult> getTask = Task.Run(() => GetTestProjectsAsync());
            getTask.Wait();
            return getTask.Result;
        }

        private async Task<bool> IsTestProjectCancelingAsync(string executionID)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync($"{GFServer}/api/execution/{executionID}/canceling");
                string contents = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsTestProjectCanceling(string executionID)
        {
            Task<bool> getTask = Task.Run(() => IsTestProjectCancelingAsync(executionID));
            getTask.Wait();
            return getTask.Result;
        }


        private async Task<bool> IsTestProjectExistAsync(string executionID)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync($"{GFServer}/api/execution/{executionID}/exist");
                string contents = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsTestProjectExist(string executionID)
        {
            Task<bool> getTask = Task.Run(() => IsTestProjectExistAsync(executionID));
            getTask.Wait();
            return getTask.Result;
        }

        private async Task<GFServerResult> GetTestProjectAsync(string executionID)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync($"{GFServer}/api/execution/{executionID}");
                    string contents = await response.Content.ReadAsStringAsync();
                    if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return new GFServerResult(RequestStatus.KeyError, "Can not find test project with specified execution id");
                    }
                    if (!response.IsSuccessStatusCode)
                    {
                        return new GFServerResult(RequestStatus.ServerError, "Server error while getting test project");
                    }
                    
                    JObject proj = JObject.Parse(contents);
                    return new GFServerResult(new TestProject(proj));
                }
            }
            catch(Exception)
            {
                return new GFServerResult(RequestStatus.ConnectionError, "Server-side connection error while getting test project");
            }
            
        }

        public GFServerResult GetTestProject(string executionID)
        {
            Task<GFServerResult> getTask = Task.Run(() => GetTestProjectAsync(executionID));
            getTask.Wait(TimeSpan.FromSeconds(5));
            return getTask.Result;
        }


        private async Task<GFServerResult> GetTestResultsAsync(string executionID)
        {
            JArray jsonArray = new JArray();
            try
            {
                using (HttpClient client = new HttpClient())
                {

                    HttpResponseMessage response = await client.GetAsync($"{GFServer}/api/execution/{executionID}/testresults");
                    string contents = await response.Content.ReadAsStringAsync();
                    jsonArray = JArray.Parse(contents);
                }
            }
            catch(Exception)
            {
                return new GFServerResult(RequestStatus.ConnectionError, "Server-side connection error while getting test result");
            }

            GFServerResult result = new GFServerResult(RequestStatus.Success);
            foreach (JObject j in jsonArray)
            {
                result.DataList.Add(new TestResult(j));
            }
            return result;
        }

        public GFServerResult GetTestResults(string executionID)
        {
            Task<GFServerResult> getTask = Task.Run(() => GetTestResultsAsync(executionID));
            getTask.Wait(TimeSpan.FromSeconds(30));
            return getTask.Result;
        }

        private async Task<GFServerResult> GetTestProjectResultAsync(string executionID)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync($"{GFServer}/api/execution/{executionID}/counts");
                    string contents = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return new GFServerResult(RequestStatus.KeyError, "Can not find test project with specified execution id");
                    }
                    if (!response.IsSuccessStatusCode)
                    {
                        return new GFServerResult(RequestStatus.ServerError, "Server error while getting pass/fail/error count of test project");
                    }

                    JObject result = JObject.Parse(contents);
                    return new GFServerResult(new TestProjectResult(executionID, result));
                }
            }
            catch (Exception)
            {
                return new GFServerResult(RequestStatus.ConnectionError, "Server-side connection error while getting pass/fail/error count of test project");
            }
        }

        public GFServerResult GetTestProjectResult(string executionID)
        {
            Task<GFServerResult> getTask = Task.Run(() => GetTestProjectResultAsync(executionID));
            getTask.Wait(TimeSpan.FromSeconds(10));
            return getTask.Result;
        }
        
        private async Task<GFServerResult> CancelTestProjectAsync(TestProject testProject)
        {
            testProject.Status = ExecutionStatus.Canceling;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    StringContent content = new StringContent(testProject.ToJson());
                    content.Headers.Remove("Content-Type");
                    content.Headers.Add("Content-Type", "application/json");

                    HttpResponseMessage response = await client.PutAsync($"{GFServer}/api/execution/{testProject.ExecutionID}/cancel/", content);
                    if (response.IsSuccessStatusCode)
                    {
                        return new GFServerResult(RequestStatus.Success);
                    }
                    return new GFServerResult(RequestStatus.ServerError, "Server-side error while canceling test project");

                }
            }
            catch (Exception)
            {
                return new GFServerResult(RequestStatus.ConnectionError, "Server-side connection error while canceling test project");
            }

        }
        public GFServerResult CancelTestProject(TestProject testProject)
        {
            Task<GFServerResult> cancelTPTask = Task.Run(() => CancelTestProjectAsync(testProject));
            cancelTPTask.Wait();

            return cancelTPTask.Result;
        }

        // Management
        private async Task<string> GetSuperUser()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync($"{GFServer}/api/manage/superuser");
                    string contents = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        return null;
                    }
                    else
                    {
                        return contents;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        public bool IsSuperUser(string username)
        {
            Task<string> getTask = Task.Run(() => GetSuperUser());
            getTask.Wait(TimeSpan.FromSeconds(30));

            string superusers = getTask.Result;
            if(string.IsNullOrEmpty(superusers))
            {
                return false;
            }

            List<string> listSuperusers = superusers.Split(';').ToList();
            if (listSuperusers.Contains(username))
            {
                return true;
            }
            return false;
        }
    }
}
