using HP.Automation.SES.Log;
using HP.GFriend.Utils.Vision;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HP.GFriend.Keywords
{
    public class OcrCacheHelper
    {
        private readonly string cacheDir = Path.Combine(Directory.GetCurrentDirectory(), "OcrCache");

        public OcrCacheHelper()
        {
            if (!Directory.Exists(cacheDir))
                Directory.CreateDirectory(cacheDir);
        }

        public string GetImageHash(Bitmap image)
        {
            using (var clone = new Bitmap(image))
            using (var ms = new MemoryStream())
            {
                clone.Save(ms, ImageFormat.Png);
                byte[] bytes = ms.ToArray();
                using (var sha = SHA256.Create())
                {
                    return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLower();
                }
            }
        }


        public bool TryLoadCachedResult(string hash, out List<OcrResult> results)
        {
            string file = Path.Combine(cacheDir, $"{hash}.json");

            if (File.Exists(file))
            {
                var json = File.ReadAllText(file);
                results = JsonConvert.DeserializeObject<List<OcrResult>>(json);
                return true;
            }
            results = null;
            return false;
        }

        public void SaveResultToCache(string hash, List<OcrResult> results)
        {
            string file = Path.Combine(cacheDir, $"{hash}.json");
            var json = JsonConvert.SerializeObject(results);
            File.WriteAllText(file, json);
        }
    }
}
