using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HP.GFriend.Utils.Appium
{
    internal class AWSConnector
    {
        public static byte[] GetProvisioningProfile()
        {
            string downloadUrl = GetProvisioningProfileDownloadUrl();
            Task<byte[]> downloadTask = Task.Run(() => GetProvisioningProfile(downloadUrl));
            downloadTask.Wait();
            return downloadTask.Result;
        }

        private static async Task<Byte[]> GetProvisioningProfile(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    Byte[] contents = await response.Content.ReadAsByteArrayAsync();
                    return contents;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        
        private static async Task<string> GetProvisioningProfileDownloadUrlAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync("https://y02iaryx0h.execute-api.ap-northeast-2.amazonaws.com/gfios/download");
                    string contents = await response.Content.ReadAsStringAsync();
                    return contents;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string GetProvisioningProfileDownloadUrl()
        {
            Task<string> getTask = Task.Run(() => GetProvisioningProfileDownloadUrlAsync());
            getTask.Wait();

            if (getTask.Result == null)
            {
                throw new HttpRequestException("Download url result is null");
            }
            return getTask.Result.Replace("\"", "");
        }
    }
}
