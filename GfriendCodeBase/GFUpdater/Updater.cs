using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using HP.GFriend.GFLogger;
using System.Net;

namespace HP.GFriend.Updater
{
    public class GFUpdaterOnAws
    {
        private string _gfServerEndpoint;
        private List<string> _usedFiles;

        public GFUpdaterOnAws()
        {
            _usedFiles = new List<string>();
        }

        public GFUpdaterOnAws(string gfServerEndpoint)
        {
            _gfServerEndpoint = gfServerEndpoint;
            //_versionInServer = GetCurrentVersionOnAws();
            _usedFiles = new List<string>();
        }

        #region UPDATE_LOGIC
        
        public List<string> CopyBinary(string binaryPath, string targetPath = null)
        {
            Logger.Trace("CopyBinary-binaryPath="+ binaryPath);
            
            if (targetPath == null)
            {
                targetPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
            Logger.Trace("CopyBinary-targetPath=" + targetPath);
            

            foreach (string dirPath in Directory.GetDirectories(binaryPath, "*", SearchOption.AllDirectories))
            {
                Logger.Trace("CopyBinary-dirPath=" + dirPath);
                

                string newPath = dirPath.Replace(binaryPath, targetPath);
                Logger.Trace("CopyBinary-newPath=" + newPath);
                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }
            }

            foreach (string filePath in Directory.GetFiles(binaryPath, "*.*", SearchOption.AllDirectories))
            {
                try
                {
                    File.Copy(filePath, filePath.Replace(binaryPath, targetPath), true);
                    Logger.Trace("file copied successfully : " + filePath);
                }
                catch (Exception)
                {
                    _usedFiles.Add(filePath);
                    Logger.Trace("file giving exception to copy : " + filePath);
                }

            }
            return _usedFiles;
        }
        #endregion

        #region REST-API
        private async Task<List<GFTools>> GetCurrentVersionOnAwsAsync(string name)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-API-Key", HP.GFriend.Updater.Const.API_KEY);
                    HttpResponseMessage response = await client.GetAsync($"{_gfServerEndpoint}/binary?name={name}");
                    string contents = await response.Content.ReadAsStringAsync();
                    Logger.Trace("GetCurrentVersionOnAwsAsync->" + response.StatusCode);
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return null;
                    }
                    else
                    {
                        return JsonConvert.DeserializeObject<List<GFTools>>(contents);

                        //contents = contents.Trim('\"').Split(new string[] { "::" }, StringSplitOptions.None)[0].Trim();
                        //return contents;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Trace("GetCurrentVersionOnAwsAsync->Exception->" + ex.Message);
                Logger.Trace("GetCurrentVersionOnAwsAsync->Exception->" + ex.InnerException);
                return null;
            }
        }

        public List<GFTools> GetCurrentVersionOnAws(string name, int timeOutSeconds  = 10)
        {
            Task<List<GFTools>> getTask = Task.Run(() => GetCurrentVersionOnAwsAsync(name));
            getTask.Wait(TimeSpan.FromSeconds(timeOutSeconds));
            if (getTask.IsCompleted)
            {
                return getTask.Result;
            }
            return null;
        }

        private async Task<Byte[]> GetGFBinaryOnAwsAsync(string url)
        {
            try
            {
                var handler = new HttpClientHandler();
                using (HttpClient client = new HttpClient(handler))
                {
                    //timeout of 2 minutes
                    client.Timeout = TimeSpan.FromSeconds(120);
                    Logger.Trace("TImeout for GetGFBinaryOnAwsAsync : " + client.Timeout.ToString());
                    HttpResponseMessage response = await client.GetAsync(url);
                    
                    Logger.Trace("Response from the method GetGFBinaryOnAwsAsync : " + response.StatusCode);
                    Byte[] contents = await response.Content.ReadAsByteArrayAsync();


                    // Check if a proxy is being used
                    IWebProxy proxy = handler.Proxy;

                    // If proxy is not null, print proxy details
                    if (proxy != null)
                    {
                        Uri proxyUri = proxy.GetProxy(new Uri(url));
                        Logger.Trace("Proxy Address: " + proxyUri.ToString());

                        // Check if default credentials or custom credentials are used
                        if (handler.UseDefaultCredentials)
                        {
                            Logger.Trace("Using default credentials for proxy.");
                        }
                        else if (handler.Credentials != null)
                        {
                            NetworkCredential credentials = handler.Credentials as NetworkCredential;
                            if (credentials != null)
                            {
                                Logger.Trace("Proxy Username: " + credentials.UserName);
                            }
                        }
                        else
                        {
                            Logger.Trace("No proxy credentials used.");
                        }
                    }
                    else
                    {
                        Logger.Trace("No proxy configured.");
                    }
                    return contents;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("task was canceled"))
                {
                    Logger.Trace("Inside exception block task was canceled");
                    Logger.Trace("GetGFBinaryOnAwsAsync->ExceptionMessage->" + ex.Message);
                    if (ex.InnerException != null)
                    {
                        Logger.Trace("GetGFBinaryOnAwsAsync->InnerException->" + ex.InnerException.Message);
                    }
                    Logger.Trace("Unable to download GFriend Binary from AWS in 2 mins. So , retrying the download for the second time.");

                    Task<Byte[]> getTask = Task.Run(() => RetryDownloadingGFBinaryOnAwsAsync(url));
                    return getTask.Result;
                }
                else
                {
                    Logger.Trace("Not inside exception block task was canceled");
                    Logger.Trace("GetGFBinaryOnAwsAsync->ExceptionMessage->" + ex.Message); 
                    if (ex.InnerException != null)
                    {
                        Logger.Trace("GetGFBinaryOnAwsAsync->InnerException->" + ex.InnerException.Message);
                    }
                }

                return null;
            }
        }

        private async Task<Byte[]> RetryDownloadingGFBinaryOnAwsAsync(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    //timeout of 5 minutes
                    client.Timeout = TimeSpan.FromSeconds(300);
                    Logger.Trace("TImeout for RetryDownloadingGFBinaryOnAwsAsync : " + client.Timeout.ToString());
                    HttpResponseMessage response = await client.GetAsync(url);
                    Logger.Trace("Response from the method RetryDownloadingGFBinaryOnAwsAsync : " + response.StatusCode);
                    Byte[] contents = await response.Content.ReadAsByteArrayAsync();
                    return contents;
                }
            }
            catch (Exception ex)
            {
                Logger.Trace("RetryDownloadingGFBinaryOnAwsAsync->ExceptionMessage->" + ex.Message);
                if (ex.InnerException != null)
                {
                    Logger.Trace("RetryDownloadingGFBinaryOnAwsAsync->InnerException->" + ex.InnerException.Message);
                }
                Logger.Trace("Unable to download GFriend Binary from AWS in 5 mins. Please check your network and try again.");
                return null;
            }
        }

        public string GetGFBinaryOnAws(string url, string name)
        {
            Task<Byte[]> getTask = Task.Run(() => GetGFBinaryOnAwsAsync(url));
            getTask.Wait();

            if (getTask.Result == null)
            {
                throw new HttpRequestException("Download result is null");
            }
            Logger.Trace("GetGFBinaryOnAws" + getTask.Result.Length.ToString());

            using (MemoryStream mStream = new MemoryStream(getTask.Result))
            {
                using (ZipArchive archive = new ZipArchive(mStream, ZipArchiveMode.Read))
                {
                    string basePath = Path.Combine(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory), name);
                    Console.WriteLine($"Saving download file to {basePath}");
                    if (Directory.Exists(basePath))
                    {
                        Directory.Delete(basePath, true);
                    }
                    Directory.CreateDirectory(basePath);
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string destinationPath = Path.Combine(basePath, entry.FullName);
                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            Directory.CreateDirectory(destinationPath);
                        }
                        else
                        {

                            destinationPath = destinationPath.Replace('/', Path.DirectorySeparatorChar);
                            FileStream fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
                            entry.Open().CopyTo(fileStream);
                            fileStream.Flush();
                            fileStream.Close();
                        }
                    }
                    return basePath;
                }
            }
        }

        private async Task<string> GetGFBinaryDownloadUrlOnAwsAsync(string name, string version)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-API-Key", Const.API_KEY);
                    HttpResponseMessage response = await client.GetAsync($"{_gfServerEndpoint}/binary/download?name={name}&version={version}");
                    string contents = await response.Content.ReadAsStringAsync();
                    return contents;
                }
            }
            catch (Exception ex)
            {
                Logger.Trace("GetGFBinaryDownloadUrlOnAwsAsync->Exception=" + ex.Message);
                return null;
            }
        }

        public string GetGFBinaryDownloadUrlOnAws(string name, string version)
        {
            Task<string> getTask = Task.Run(() => GetGFBinaryDownloadUrlOnAwsAsync(name, version));
            getTask.Wait();

            if (getTask.Result == null)
            {
                throw new HttpRequestException("Download url result is null");
            }
            return getTask.Result.Replace("\"", "");
        }

        private async Task<string> GetGFBinaryUploadUrlOnAwsAsync(string name, string version)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-API-Key", Const.API_KEY);
                    HttpResponseMessage response = await client.GetAsync($"{_gfServerEndpoint}/binary/upload?name={name}&version={version}");
                    string contents = await response.Content.ReadAsStringAsync();
                    return contents;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("GetGFBinaryUploadUrlOnAwsAsync->Exception" + ex.Message);
                return null;
            }
        }

        public string GetGFBinaryUploadUrlOnAws(string name, string version)
        {
            Task<string> getTask = Task.Run(() => GetGFBinaryUploadUrlOnAwsAsync(name, version));
            getTask.Wait();

            if (getTask.Result == null)
            {
                throw new HttpRequestException("Upload url result is null");
            }
            return getTask.Result.Replace("\"", "");
        }
            
        private async Task<bool> UploadBinaryToAwsAsync(string url, string name, string version, string filePathToUplaod)
        {
            Console.WriteLine("Uploading binary");
            //try
            //{
                using (HttpClient client = new HttpClient())
                {
                client.Timeout = TimeSpan.FromSeconds(460000);
                    FileInfo info = new FileInfo(filePathToUplaod);
                    FileStream fs = info.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    StreamContent content = new StreamContent(fs);
                    content.Headers.Add("Content-Type", "application/zip");
                    HttpResponseMessage response = await client.PutAsync(url, content);
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Upload Binary Successful");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"Request Failed with error :: {response.StatusCode}");
                        return false;
                    }

                }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.ToString());
            //    return false;
            //}
        }

        public bool UploadBinaryToAws(string url, string name, string version, string filePathToUpload)
        {
            Task<bool> uploadTask = Task.Run(() => UploadBinaryToAwsAsync(url, name, version, filePathToUpload));
            uploadTask.Wait();
            return uploadTask.Result;
        }

        private async Task<bool> UploadReleaseNoteToAwsAsync(string name, string version, string type, string filePathToUplaod)
        {
            Console.WriteLine("Uploading release-note");
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-API-Key", Const.API_KEY);
                    FileInfo info = new FileInfo(filePathToUplaod);
                    FileStream fs = info.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    var sr = new StreamReader(fs, Encoding.UTF8);
                    string description = sr.ReadToEnd();

                    GFTools tool = new GFTools()
                    {
                        Name = name,
                        Version = version,
                        Type = type,
                        Description = description
                    };
                    
                    var stringContent = new StringContent(JsonConvert.SerializeObject(tool), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PutAsync($"{_gfServerEndpoint}/binary/update", stringContent);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Upload Release-note Successful");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"Request Failed with error :: {response.StatusCode}");
                        return false;
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public bool UploadReleaseNoteToAws(string name, string version, string type, string filePathToUpload)
        {
            Task<bool> uploadTask = Task.Run(() => UploadReleaseNoteToAwsAsync(name, version, type, filePathToUpload));
            uploadTask.Wait();
            return uploadTask.Result;
        }
        #endregion
    }
}
