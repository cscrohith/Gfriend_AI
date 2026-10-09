using HP.GFriend.Keywords;
using HP.GFriend.Keywords.Event;
using HP.GFriend.Utils.Charter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Logger = HP.GFriend.GFLogger.Logger;

namespace GFK.Ares
{
    public class Ares : IGFLibrary
    {
        private System.Timers.Timer _memoryTimer;
        private static MemoryUsageItem _memoryUsage = null;

        private string _checkPointName;

        private DeviceUnderTest _dut;
        private string _outputDir;

        private string _testCaseName= string.Empty;
        private string _csvPath;

        private HttpClient _httpClient;
        private JObject json;
        string memoryStats;

        public void Dispose()
        {
            _httpClient?.Dispose();
            _memoryTimer?.Dispose();
        }

        public bool DutUsed()
        {
            return true;
        }

        public List<string> GetDependencies()
        {
            return null;
        }

        public string GetName()
        {
            return "Ares";
        }

        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
            _dut = dut;
            _outputDir = outputDir;
            _httpClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                {
                    if (message.RequestUri.Host == _dut.DeviceAddress ||
                        message.RequestUri.Host.Contains("deviceaddress"))
                    {
                        return true;
                    }
                    return sslPolicyErrors == System.Net.Security.SslPolicyErrors.None;
                }
            });
        }
        private async Task<string> GetMemoryDataFromUrl(string url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    Logger.Error("Invalid input: 'url' parameter is null or empty.");
                    throw new ArgumentException("URL cannot be null or empty.", nameof(url));
                }

                Logger.Trace($"Fetching memory data from URL: {url}");

                // Set timeout to prevent indefinite hanging
                using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(30)))
                {
                    HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseContentRead, cts.Token);
                    response.EnsureSuccessStatusCode();
                    string jsonData = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrWhiteSpace(jsonData))
                    {
                        Logger.Error($"Empty response received from URL: {url}");
                        throw new InvalidOperationException("Response content is null or empty.");
                    }

                    Logger.Trace($"Successfully retrieved JSON data from {url}");
                    return jsonData;
                }
            }
            catch (System.OperationCanceledException ex)
            {
                Logger.Error($"Request timeout while fetching data from URL: {url}. Timeout: 30 seconds");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error fetching data from URL: {ex.Message}");
                throw;
            }
        }

        private string DetermineCsvPath()
        {
            string csvPath;
            if (CommonExecutionInfo.CurrentRepeatCount > 0)
            {
                string parentDir = Directory.GetParent(_outputDir).FullName;
                csvPath = Path.Combine(parentDir, "Memory_Usage.csv");
                Logger.Trace($"Current Repeat Count is greater than 0. CSV Path is : {csvPath}");
            }
            else
            {
                csvPath = Path.Combine(_outputDir, $"{_testCaseName}_AresMemoryMonitoring_{DateTime.Now:yyMMdd_HHmmssff}.csv");
                Logger.Trace($"CSV Path is : {csvPath}");
            }

            _csvPath = csvPath;
            _memoryUsage = new MemoryUsageItem(csvPath);

            return csvPath;
        }

        private void InitializeMemoryTimer(int intervalSeconds)
        {
            // Dispose previous timer to prevent resource leaks
            if (_memoryTimer != null)
            {
                _memoryTimer.Stop();
                _memoryTimer.Elapsed -= MemoryTimerElapsed;
                _memoryTimer.Dispose();
            }

            // Configure timer in single block
            int intervalMilliseconds = intervalSeconds * 1000;
            _memoryTimer = new System.Timers.Timer(intervalMilliseconds)
            {
                AutoReset = true,
                Enabled = true
            };
            _memoryTimer.Elapsed += MemoryTimerElapsed;
            _memoryTimer.Start();
        }

        [KeywordDescription("Initialize Memory monitoring for Ares Device. Collects memory data every 5 seconds automatically.")]
        [KeywordDisplayName("Start Memory Monitoring")]
        [KeywordParameters("time", "Interval in seconds for memory data collection")]
        [SampleScript("Ares.Start Memory Monitoring(20)")]
        public KeywordResult StartMemoryMonitoring(string time)
        {
            _testCaseName = CommonExecutionInfo.GetVariable("_tcName");

            // Early validation and parsing
            if (string.IsNullOrWhiteSpace(time))
            {
                Logger.Error("Invalid input: 'time' parameter is null or empty. Please provide a valid time value.");
                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = "Invalid input: 'time' parameter is null or empty. Please provide a valid time value."
                };
            }

            if (!int.TryParse(time, out int intervalSeconds) || intervalSeconds <= 0)
            {
                Logger.Error($"Invalid input: 'time' parameter must be a positive integer. Provided value: {time}");
                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = $"Invalid input: 'time' parameter must be a positive integer. Provided value: {time}"
                };
            }

            string csvPath = DetermineCsvPath();
            InitializeMemoryTimer(intervalSeconds);

            return new KeywordResult(KeywordResults.Pass)
            {
                Output = $"Memory Monitoring started (every {intervalSeconds} seconds). Data will be saved to : {csvPath}"
            };
        }



        [KeywordDescription("Dump memory usage and save with given check point name")]
        [KeywordDisplayName("Collect Memory Details")]
        [KeywordParameters("checkPoint", "Check Point name to save (e.g., job type or stage identifier)")]
        [SampleScript("Ares.Collect Memory Details")]
        public KeywordResult CollectMemoryDetails()
        {
            try
            {
                // Execute async operation synchronously - required by keyword framework
                return CollectMemoryDetailsAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = "Fail during dump memory",
                    AdditionalInfo = ex.ToString()
                };
            }
        }

        private async Task<KeywordResult> CollectMemoryDetailsAsync()
        {
            if (_memoryUsage == null)
            {
                Logger.Error("Memory monitoring not started. Call StartMemoryMonitoring first.");
                throw new InvalidOperationException(
                    "Memory monitoring not started. Call StartMemoryMonitoring first.");
            }

            const string endpoint = "/cdm/system/v1/statistics";
            string url = $"https://{_dut.DeviceAddress}{endpoint}";
           
            string response;

            try
            {
                response = await GetMemoryDataFromUrl(url).ConfigureAwait(false);
                
                if (string.IsNullOrWhiteSpace(response))
                {
                    Logger.Error($"Memory statistics response is empty. URL: {url}");
                    return new KeywordResult(KeywordResults.Fail)
                    {
                        Output = "Memory statistics response is null or empty."
                    };
                }
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404"))
            {
                Logger.Error($"Memory statistics endpoint not found: {url}");
                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = "Memory statistics endpoint not found (404). Device may not support this feature.",
                    AdditionalInfo = ex.Message
                };
            }
            catch (System.OperationCanceledException ex)
            {
                Logger.Error($"Request timeout while fetching memory data from {url}");
                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = "Request timeout while fetching memory statistics.",
                    AdditionalInfo = ex.Message
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"Error fetching memory data: {ex.Message}");
                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = "Error fetching memory statistics from device.",
                    AdditionalInfo = ex.Message
                };
            }

            JObject memoryJson;

            try
            {
                memoryJson = JObject.Parse(response);
            }
            catch (Newtonsoft.Json.JsonReaderException ex)
            {
                Logger.Error($"Invalid JSON received from {url}. Error: {ex.Message}");
                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = "Invalid JSON received in memory statistics response.",
                    AdditionalInfo = ex.Message
                };
            }

            long totalMemory = memoryJson.Value<long?>("totalMemory") ?? 0;
            long availableMemory = memoryJson.Value<long?>("availableMemory") ?? 0;
            long usedMemory = Math.Max(0, totalMemory - availableMemory);

            double usagePercentage = totalMemory > 0
                ? (usedMemory * 100.0) / totalMemory
                : 0;

            _memoryUsage.DataAppend(new Dictionary<string, object>
            {
                ["Timestamp"] = DateTime.Now.ToString("HH:mm:ss tt"),
                ["Metric_name"] = "AvailableRamUnits",
                ["AvailableMemory"] = availableMemory,
                ["TotalMemory"] = totalMemory,
                ["UsedMemory_KB"] = usedMemory,
                ["MemoryUsagePercentage"] = $"{usagePercentage:F2}%"
            });

            return new KeywordResult(KeywordResults.Pass)
            {
                Output = $"Memory data collected. Usage: {usagePercentage:F2}% ({usedMemory} KB used / {totalMemory} KB total)"
            };
        }
        private void MemoryTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Check for disposal
            if (_memoryUsage == null || _memoryTimer == null)
            {
                return;
            }

            try
            {
                // Use cached format and null-coalescing for safety
                string timestamp = $"{DateTime.Now:HH:mm:ss tt}";
                CollectMemoryDetails();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error during automatic memory collection: {ex.Message}");
            }
        }

    }
}