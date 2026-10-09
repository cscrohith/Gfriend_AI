using HP.Automation.SES;
using HP.DeviceAutomation.Dune;
using HP.DeviceAutomation.Jedi;
using HP.GFriend.Utils.Vision;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Extension;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Mac;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static HP.GFriend.Keywords.MouseOperations;
using static HP.GFriend.Support.Utils;
using Logger = HP.GFriend.GFLogger.Logger;

namespace HP.GFriend.Keywords
{
    [LibraryDescription("Based on the target device the shared object will be accessed from other device libraries.\r\n Order of the vision keywords : Vision.Set Target (windows), Vision.Set Text Grouping (word), Vision.Is Text Exist (On)")]
    public class Vision : IGFLibrary
    {
        private Func<Byte[]> _GetScreenShot;
        private Func<MousePoint, bool> _ClickPosition;
        // Azure OCR specific fields
        private string _azureEndpoint;
        private string _azureSubscriptionKey;
        private const string OcrUri = "/vision/v3.2/ocr";
        private const string AnalyzeUri = "/vision/v3.2/read/analyze";

        private bool _isImageCropped = false;
        private Dictionary<string, int> croppedValue = new Dictionary<string, int>();

        private DeviceUnderTest _dut;
        private string _outputDir;
        private JediOmniDevice _JediOmni;
        private DuneDevice _duneDevice;
        private Windows _Windows;
        private SESLib _SESLibrary;
        private IOSDriver _IOS;
        private MacDriver _MAC;
        private byte[] _ScreenShot;
        private MousePoint _point = new MousePoint(0, 0);
        private Graphics _graphics;
        private double _resizeFactor;
        private int _imgStartX, _imgStartY;
        private int _pixelRatio = 1;
        private Size _windowSize;
        private AppiumElement _macBaseElement;
        private GFVision _gfVision = new GFVision();
        private string _encryptedXmlFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Replace("libs", ""), "EncryptedData.xml");

        private StreamWriter _azureLogWriter;
        private string _azureLogFilePath = "";
        private int _azureOcrApiCalls = 0;
        private int _azureOcrCacheHits = 0;
        private int _azureAnalyzeApiCalls = 0;
        private int _azureAnalyzeCacheHits = 0;

        private string _aiSolutionName;
        private bool _aiInitiated = false;
        private string _aiBaseUrl;


        public int _thresholdValueDefault { get; set; } = 180;
        public int _thresholdValue_IOSMAC { get; set; } = 150;
        public float _pixelScale_IOSMAC { get; private set; } = 5f;

        public Image _ScreenImage;

        public Vision()
        {

        }

        public void Dispose()
        {
            // Disposing
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
            return "Vision";
        }

        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
            _dut = dut;
            _outputDir = outputDir;
            _azureLogFilePath = Path.Combine(_outputDir, "AzureLogs.txt");
        }
        // OCR Method Selection
        private enum OcrMethod
        {
            Testract,
            Azure,
            AI
        }
        private OcrMethod _currentOcrMethod = OcrMethod.Testract;

        // New keyword to set OCR method
        [KeywordDescription("Set OCR method to use for text recognition")]
        [KeywordDisplayName("Set OCR Method")]
        [KeywordParameters("azureEndpoint", "Azure endpoint URL encrypted variable or encrypted value (based on Method Type)")]
        [KeywordParameters("subscriptionKey", "Azure subscription key encrypted variable or encrypted value (based on Method Type)")]
        [KeywordParameters("methodType", "Method Type - 'UI' for Encrypted Variable Name or directly 'Encrypted value of an endpoint URL or subscription Key. The possible values are UI or ENCRYPT")]
        [SampleScript("Vision.Set OCR Method (https://your-endpoint.cognitiveservices.azure.com/, your-subscription-key, UI or ENCRYPT)")]
        public KeywordResult SetOCRMethod(string azureEndpoint, string subscriptionKey, string methodType)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                if (methodType.ToUpper() == "UI")
                {
                    // Load encrypted data from XML
                    List<EncryptedData> dataList = HP.GFriend.Support.Utils.LoadDataFromXml(_encryptedXmlFilePath);

                    // Find the matching entries
                    var azureEntry = dataList.FirstOrDefault(d => d.Key == azureEndpoint);
                    var subscriptionEntry = dataList.FirstOrDefault(d => d.Key == subscriptionKey);

                    if (azureEntry == null)
                    {
                        return new KeywordResult(KeywordResults.Fail, $"Key '{azureEndpoint}' for Azure Endpoint not found.");
                    }

                    if (subscriptionEntry == null)
                    {
                        return new KeywordResult(KeywordResults.Fail, $"Key '{subscriptionKey}' for Subscription Key not found.");
                    }

                    // Decrypt values using respective secret keys
                    string decryptedAzureEndpoint = Decryptxml(azureEntry.EncryptedValue, azureEndpoint);
                    string decryptedSubscriptionKey = Decryptxml(subscriptionEntry.EncryptedValue, subscriptionKey);

                    // Validate decrypted values
                    if (string.IsNullOrEmpty(decryptedAzureEndpoint))
                    {
                        kr.Result = KeywordResults.Fail;
                        kr.Output = "Decryption failed: Azure Endpoint is empty.";
                        return kr;
                    }

                    if (string.IsNullOrEmpty(decryptedSubscriptionKey))
                    {
                        kr.Result = KeywordResults.Fail;
                        kr.Output = "Decryption failed: Subscription Key is empty.";
                        return kr;
                    }

                    // Validate Azure endpoint and subscription key
                    _currentOcrMethod = OcrMethod.Azure;
                    _azureEndpoint = decryptedAzureEndpoint;
                    _azureSubscriptionKey = decryptedSubscriptionKey;

                    kr.Output = "Using Azure OCR method with UI input.";
                }
                else if (methodType.ToUpper() == "ENCRYPT")
                {
                    // Handle encrypted input
                    if (string.IsNullOrEmpty(azureEndpoint))
                    {
                        return new KeywordResult(KeywordResults.Fail, "Encrypted Azure Endpoint is required for Encrypted method.");
                    }

                    if (string.IsNullOrEmpty(subscriptionKey))
                    {
                        return new KeywordResult(KeywordResults.Fail, "Encrypted Subscription Key is required for Encrypted method.");
                    }

                    // Decrypt values using respective secret keys
                    string decryptedAzureEndpoint = Decryptxml(azureEndpoint);
                    string decryptedSubscriptionKey = Decryptxml(subscriptionKey);

                    // Validate decrypted values
                    if (string.IsNullOrEmpty(decryptedAzureEndpoint))
                    {
                        kr.Result = KeywordResults.Fail;
                        kr.Output = "Decryption failed: Azure Endpoint is empty.";
                        return kr;
                    }

                    if (string.IsNullOrEmpty(decryptedSubscriptionKey))
                    {
                        kr.Result = KeywordResults.Fail;
                        kr.Output = "Decryption failed: Subscription Key is empty.";
                        return kr;
                    }

                    // Set the OCR method to Azure
                    _currentOcrMethod = OcrMethod.Azure;

                    // Assign decrypted values
                    _azureSubscriptionKey = decryptedSubscriptionKey;
                    _azureEndpoint = decryptedAzureEndpoint;

                    kr.Output = "Using Azure OCR method with Encrypted input.";
                }
                else
                {
                    return new KeywordResult(KeywordResults.Fail, "Invalid Method Type. Use 'UI' or 'Encrypted'.");
                }
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = $"Error setting OCR method: {ex.Message}";
                kr.AdditionalInfo = ex.ToString();
            }

            return kr;
        }

        [KeywordDescription("Set target platform which is working with Vision eg. Android, MAC, IOS, JediOmni. This keyword should be called first BEFORE other Vision keywords and it is mandatory .")]
        [KeywordDisplayName("Set Target")]
        [KeywordParameters("target", "target platform (Android, Mac, iOS, Windows, JediOmni")]
        [SampleScript("Vision.Set Target (Windows)")]
        public KeywordResult SetTarget(string target)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            switch (target.ToUpper())
            {
                case "DUNE":
                    _gfVision.ChangeThresholdValue(_thresholdValueDefault);
                    _duneDevice = CommonExecutionInfo.GetSharedObject("dune:" + _dut.DeviceId) as DuneDevice;
                    _GetScreenShot = DuneDevice_CaptureScreen;
                    _ClickPosition = DuneDevice_Click;
                    kr.ScreenShot = _GetScreenShot.Invoke();
                    break;
                case "JEDIOMNI":
                    _gfVision.ChangeThresholdValue(_thresholdValueDefault);
                    _JediOmni = CommonExecutionInfo.GetSharedObject("jediOmni:" + _dut.DeviceId) as JediOmniDevice;
                    _GetScreenShot = JediOmni_CaptureScreen;
                    _ClickPosition = JediOmniClick;
                    kr.ScreenShot = _GetScreenShot.Invoke();
                    break;
                case "ANDROID":
                    _gfVision.ChangeThresholdValue(_thresholdValueDefault);
                    _SESLibrary = CommonExecutionInfo.GetSharedObject("android:" + _dut.DeviceId) as SESLib;
                    _GetScreenShot = _SESLibrary.GetScreenCapture;
                    _ClickPosition = AndroidClick;
                    kr.ScreenShot = _GetScreenShot.Invoke();
                    break;
                case "WINDOWS":
                    _gfVision.ChangeThresholdValue(_thresholdValueDefault);
                    _Windows = CommonExecutionInfo.GetSharedObject("windows") as Windows;
                    _GetScreenShot = _Windows.GetScreenShot;
                    kr.ScreenShot = _GetScreenShot.Invoke();
                    _ClickPosition = WindowsClick;
                    kr.ScreenShot = _GetScreenShot.Invoke();
                    break;
                case "MAC":
                    _gfVision.ChangeThresholdValue(_thresholdValue_IOSMAC);
                    _gfVision.SetPixelScale(_pixelScale_IOSMAC);
                    _MAC = CommonExecutionInfo.GetSharedObject("Mac:" + _dut.DeviceId) as MacDriver;
                    _GetScreenShot = GetMacScreenCapture;
                    _ClickPosition = MacClick;
                    kr.ScreenShot = _GetScreenShot.Invoke();
                    break;
                case "IOS":
                    _gfVision.ChangeThresholdValue(_thresholdValue_IOSMAC);
                    _gfVision.SetPixelScale(_pixelScale_IOSMAC);
                    _IOS = CommonExecutionInfo.GetSharedObject("Ios:" + _dut.DeviceId) as IOSDriver;
                    _GetScreenShot = GetIOSScreenCapture;
                    _ClickPosition = IOSClick;
                    kr.ScreenShot = _GetScreenShot.Invoke();
                    break;
            }

            return kr;

        }

        private bool DuneDevice_Click(MousePoint point)
        {
            _duneDevice.ControlPanel.PressScreen(new DeviceAutomation.Coordinate(point.X, point.Y));
            return true;
        }

        private byte[] DuneDevice_CaptureScreen()
        {
            var Image = _duneDevice.ControlPanel.ScreenCapture();
            return Image.ToByteArray();
        }

        // Modified IsTextExist method to support multiple OCR methods
        [KeywordDescription("Check if given text exists in the current screen")]
        [KeywordDisplayName("Is Text Exist")]
        [KeywordParameters("text", "text to find")]
        [SampleScript("Vision.Is Text Exist (Onn)")]
        public KeywordResult IsTextExist(string text)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            if (string.IsNullOrEmpty(text))
            {
                kr.Result = KeywordResults.Fail;
                kr.Output = "Please provide a text to search";
                return kr;
            }

            if (!_isImageCropped)
            {
                _ScreenShot = _GetScreenShot();
            }

            Bitmap screenshotBitmap = ConvertBytesToBitmap(_ScreenShot);
            var _ocrCacheHelper = new OcrCacheHelper();
            List<OcrResult> ocrResults;

            try
            {
                if (_currentOcrMethod == OcrMethod.Azure)
                {
                    string hash = _ocrCacheHelper.GetImageHash(screenshotBitmap);
                    string analyzeHash = hash + "_analyze";

                    bool hasRunCache = _ocrCacheHelper.TryLoadCachedResult(hash, out var runCache);
                    bool hasAnalyzeCache = _ocrCacheHelper.TryLoadCachedResult(analyzeHash, out var analyzeCache);

                    if (hasAnalyzeCache)
                    {
                        _azureAnalyzeCacheHits++;
                        LogAzureAnalyzeStats();
                        ocrResults = analyzeCache;
                    }
                    else if (hasRunCache)
                    {
                        _azureOcrCacheHits++;
                        LogAzureOcrStats();
                        ocrResults = runCache;
                    }
                    else
                    {
                        ocrResults = RunAzureOCRAsync(_ScreenShot).Result;
                        _azureOcrApiCalls++;
                        LogAzureOcrStats();
                        _ocrCacheHelper.SaveResultToCache(hash, ocrResults);
                    }
                }
                else if (_currentOcrMethod == OcrMethod.AI)
                {
                    ocrResults = RunAiOCRAsync(_ScreenShot).Result;
                }
                else
                {
                    ocrResults = _gfVision.RunOCR(_ScreenShot);
                }

                bool found = ocrResults.Any(e => e.Text.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0);

                if (!found && _currentOcrMethod == OcrMethod.Azure)
                {
                    string analyzeHash = _ocrCacheHelper.GetImageHash(screenshotBitmap) + "_analyze";

                    if (!_ocrCacheHelper.TryLoadCachedResult(analyzeHash, out ocrResults))
                    {
                        ocrResults = RunAnalyzeAzureAsync(_ScreenShot).Result;
                        _azureAnalyzeApiCalls++;
                        LogAzureAnalyzeStats();
                        _ocrCacheHelper.SaveResultToCache(analyzeHash, ocrResults);
                    }

                    found = ocrResults.Any(e => e.Text.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                if (found)
                {
                    kr.Result = KeywordResults.Pass;
                }
                else
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Cannot find '{text}' in the screen";
                    CaptureOCRResultsImage(kr, ocrResults);
                }
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "OCR error";
                kr.ScreenShot = _ScreenShot;
                kr.AdditionalInfo = ex.ToString();
                Logger.Error("OCR error in IsTextExist", ex);
            }

            return kr;
        }
        

        [KeywordDescription("Wait until given text disappears from the screen within specified time")]
        [KeywordDisplayName("Wait For Text Gone")]
        [KeywordParameters("text", "Text to wait to disappear")]
        [KeywordParameters("timeoutInSeconds", "Maximum time to wait in seconds")]
        [SampleScript("Vision.Wait For Text Gone (Delete, 10)")]
        public KeywordResult WaitForTextGone(string text, string timeoutInSeconds)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            if (string.IsNullOrEmpty(text))
            {
                return new KeywordResult(KeywordResults.Fail, "Text cannot be empty");
            }
            if (!int.TryParse(timeoutInSeconds, out int timeout))
            {
                return new KeywordResult(KeywordResults.Fail, "Timeout must be a number (seconds)");
            }
            DateTime endTime = DateTime.Now.AddSeconds(timeout);
            while (DateTime.Now <= endTime)
            {
                var result = IsTextExist(text);
                if (result.ScreenShot != null)
                {
                    _ScreenShot = result.ScreenShot;
                }
                if (result.Result == KeywordResults.Fail)
                {
                    kr.Output = $"Text '{text}' disappeared within {timeout} seconds";
                    kr.ScreenShot = _ScreenShot;
                    return kr;
                }                
            }
            kr.Result = KeywordResults.Fail;
            kr.Output = $"Text '{text}' still present after {timeout} seconds";
            kr.ScreenShot = _ScreenShot;
            return kr;
        }

        // Azure OCR method
        private async Task<List<OcrResult>> RunAzureOCRAsync(byte[] imageBytes)
        {
            if (string.IsNullOrEmpty(_azureEndpoint) || string.IsNullOrEmpty(_azureSubscriptionKey))
            {
                throw new InvalidOperationException("Azure OCR credentials not set. Use Set OCR Method keyword first.");
            }
            try
            {
                _azureLogWriter = new StreamWriter(_azureLogFilePath, true);
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_azureEndpoint);
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _azureSubscriptionKey);
                    client.Timeout = TimeSpan.FromSeconds(120); // Set the timeout for the request


                    // Prepare the content for the request
                    ByteArrayContent content = new ByteArrayContent(imageBytes);
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                    // Send the OCR request with Chinese language setting (Simplified Chinese)
                    HttpResponseMessage response = await client.PostAsync(OcrUri, content);

                    // Check response
                    if (!response.IsSuccessStatusCode)
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        _azureLogWriter.WriteLine($" [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] RunAzureOCRAsync API call failed. Status: {response.StatusCode}, Error: {errorContent}");
                        throw new Exception($"RunAzureOCRAsync API call failed: {errorContent}");
                    }

                    // Get the OCR result
                    string resultJson = await response.Content.ReadAsStringAsync();

                    // Log the successful response
                    _azureLogWriter.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Received successful response from RunAzureOCRAsync API: {response.StatusCode} ");
                    // Parse the result and return it
                    return ParseAzureOCRResponse(resultJson);
                }
            }
            catch (Exception ex)
            {
                // Log exception details in case of an error
                if (_azureLogWriter != null)
                {
                    _azureLogWriter.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] RunAzureOCRAsync API Call Exception: {ex.Message}");

                    // If there's any additional error response, log it
                    if (ex is HttpRequestException httpRequestException)
                    {
                        _azureLogWriter.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] HTTP Request Error: {httpRequestException.Message}");
                    }
                }
                Logger.Error($"RunAzureOCRAsync API Call Exception: {ex.Message}", ex);
                throw;
            }

            finally
            {
                // Ensure StreamWriter is always closed after the operation is complete
                if (_azureLogWriter != null)
                {
                    _azureLogWriter.Close();
                }
            }
        }
        //using read/analyze endpoint
        private async Task<List<OcrResult>> RunAnalyzeAzureAsync(byte[] imageBytes)
        {
            if (string.IsNullOrEmpty(_azureEndpoint) || string.IsNullOrEmpty(_azureSubscriptionKey))
            {
                throw new InvalidOperationException("Azure OCR credentials not set. Use Set OCR Method keyword first.");
            }
            try
            {
                _azureLogWriter = new StreamWriter(_azureLogFilePath, true);
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_azureEndpoint);
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _azureSubscriptionKey);
                    client.Timeout = TimeSpan.FromSeconds(120);


                    // Initial request to submit the image
                    ByteArrayContent content = new ByteArrayContent(imageBytes);
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                    // Send the OCR request
                    HttpResponseMessage response = await client.PostAsync(AnalyzeUri, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        _azureLogWriter.WriteLine($" [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] RunAnalyzeAzureAsync API call failed. Status: {response.StatusCode}, Error: {errorContent}");
                        throw new Exception($"RunAnalyzeAzureAsync API call failed: {errorContent}");
                    }

                    // Get the operation location URL for polling
                    string operationLocation = response.Headers.GetValues("Operation-Location").FirstOrDefault();
                    if (string.IsNullOrEmpty(operationLocation))
                    {
                        throw new Exception("Operation-Location header not found in response");
                    }

                    // Poll for results
                    int maxRetries = 10;
                    int currentTry = 0;
                    while (currentTry < maxRetries)
                    {
                        await Task.Delay(2000); // Wait 2 seconds between polls

                        var resultResponse = await client.GetAsync(operationLocation);
                        if (!resultResponse.IsSuccessStatusCode)
                        {
                            throw new Exception($"Failed to get analysis results. Status: {resultResponse.StatusCode}");
                        }

                        string resultContent = await resultResponse.Content.ReadAsStringAsync();
                        dynamic resultJson = JsonConvert.DeserializeObject(resultContent);

                        // Log the successful response
                        _azureLogWriter.WriteLine($" [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Received successful response from RunAnalyzeAzureAsync API:{resultResponse.StatusCode}");

                        string status = resultJson.status.ToString().ToLower();
                        if (status == "succeeded")
                        {
                            return ParseAzureAnalyzeResponse(resultContent);
                        }
                        else if (status == "failed")
                        {
                            throw new Exception("Analysis failed");
                        }
                        else if (status == "running")
                        {
                            currentTry++;
                            continue;
                        }
                    }

                    throw new Exception("OCR operation timed out");
                }
            }
            catch (Exception ex)
            {
                // Log exception details in case of an error
                if (_azureLogWriter != null)
                {
                    _azureLogWriter.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] RunAnalyzeAzureAsync API Call Exception: {ex.Message}");

                    // If there's any additional error response, log it
                    if (ex is HttpRequestException httpRequestException)
                    {
                        _azureLogWriter.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] HTTP Request Error: {httpRequestException.Message}");
                    }
                }
                Logger.Error($"RunAnalyzeAzureAsync API Call Exception: {ex.Message}", ex);
                throw;
            }
            finally
            {
                // Ensure StreamWriter is always closed after the operation is complete
                if (_azureLogWriter != null)
                {
                    _azureLogWriter.Close();
                }
            }
        }

        // Parse Azure OCR response to OcrResult format (remains unchanged)
        private List<OcrResult> ParseAzureOCRResponse(string ocrResultJson)
        {
            var results = new List<OcrResult>();

            try
            {
                dynamic ocrJson = JsonConvert.DeserializeObject(ocrResultJson);

                foreach (var region in ocrJson.regions)
                {
                    foreach (var line in region.lines)
                    {
                        string lineText = "";
                        Rectangle lineBounds = Rectangle.Empty;

                        // Collect words in the line and join them without adding extra spaces
                        foreach (var word in line.words)
                        {
                            string wordText = word.text.ToString();
                            string[] boundsParts = word.boundingBox.ToString().Split(',');

                            if (boundsParts.Length >= 4)
                            {
                                int x = Convert.ToInt32(boundsParts[0]);
                                int y = Convert.ToInt32(boundsParts[1]);
                                int width = Convert.ToInt32(boundsParts[2]);
                                int height = Convert.ToInt32(boundsParts[3]);

                                var wordBounds = new Rectangle(x, y, width, height);

                                // Combine line text by appending the word
                                lineText += wordText;  // No space between words in the line

                                // Update line bounds to encompass all words
                                if (lineBounds == Rectangle.Empty)
                                {
                                    lineBounds = wordBounds;
                                }
                                else
                                {
                                    lineBounds = Rectangle.Union(lineBounds, wordBounds);
                                }
                            }
                        }

                        // Add line as an OCR result
                        if (!string.IsNullOrWhiteSpace(lineText))
                        {
                            results.Add(new OcrResult(lineText.Trim(), lineBounds));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error parsing Azure OCR response", ex);
            }

            return results;
        }


        private List<OcrResult> ParseAzureAnalyzeResponse(string ocrResultJson)
        {
            var results = new List<OcrResult>();

            try
            {
                dynamic response = JsonConvert.DeserializeObject(ocrResultJson);
                var readResults = response.analyzeResult.readResults;

                foreach (var page in readResults)
                {
                    foreach (var line in page.lines)
                    {
                        string text = line.text.ToString();

                        // Parse bounding box
                        var boundingBox = line.boundingBox.ToObject<double[]>();
                        if (boundingBox?.Length >= 8) // x,y coordinates for 4 corners
                        {
                            // Convert quad coordinates to rectangle
                            int x = (int)Math.Min(
                                Math.Min(boundingBox[0], boundingBox[2]),
                                Math.Min(boundingBox[4], boundingBox[6])
                            );
                            int y = (int)Math.Min(
                                Math.Min(boundingBox[1], boundingBox[3]),
                                Math.Min(boundingBox[5], boundingBox[7])
                            );
                            int right = (int)Math.Max(
                                Math.Max(boundingBox[0], boundingBox[2]),
                                Math.Max(boundingBox[4], boundingBox[6])
                            );
                            int bottom = (int)Math.Max(
                                Math.Max(boundingBox[1], boundingBox[3]),
                                Math.Max(boundingBox[5], boundingBox[7])
                            );

                            var bounds = new Rectangle(x, y, right - x, bottom - y);
                            results.Add(new OcrResult(text.Trim(), bounds));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error parsing Azure OCR response", ex);
                throw;
            }

            return results;
        }

        [KeywordDescription("Click object with given text")]
        [KeywordDisplayName("Click Text")]
        [KeywordParameters("text", "Text to touch")]
        [SampleScript("Vision.Click Text (New meeting)")]
        public KeywordResult ClickText(string text)
        {
            text = text.Replace("(", @"\(").Replace(")", @"\)");

            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            if (string.IsNullOrEmpty(text))
            {
                kr.Result = KeywordResults.Fail;
                kr.Output = "Please provide a text to search";
                return kr;
            }

            if (!_isImageCropped)
            {
                _ScreenShot = _GetScreenShot();
            }

            var _ocrCacheHelper = new OcrCacheHelper();
            Bitmap screenshotBitmap = ConvertBytesToBitmap(_ScreenShot);

            List<OcrResult> ocrResults;

            if (_currentOcrMethod == OcrMethod.Azure)
            {
                string hash = _ocrCacheHelper.GetImageHash(screenshotBitmap);
                string analyzeHash = hash + "_analyze";

                bool hasRunCache = _ocrCacheHelper.TryLoadCachedResult(hash, out var runCache);
                bool hasAnalyzeCache = _ocrCacheHelper.TryLoadCachedResult(analyzeHash, out var analyzeCache);

                if (hasAnalyzeCache)
                {
                    _azureAnalyzeCacheHits++;
                    LogAzureAnalyzeStats();
                    ocrResults = analyzeCache;
                }
                else if (hasRunCache)
                {
                    _azureOcrCacheHits++;
                    LogAzureOcrStats();
                    ocrResults = runCache;
                }
                else
                {
                    // Call Azure OCR
                    ocrResults = RunAzureOCRAsync(_ScreenShot).Result;
                    _azureOcrApiCalls++;
                    LogAzureOcrStats();
                    _ocrCacheHelper.SaveResultToCache(hash, ocrResults);
                }
            }
            else if (_currentOcrMethod == OcrMethod.AI)
            {
                // AI OCR path
                ocrResults = RunAiOCRAsync(_ScreenShot).Result;
            }
            else
            {
                ocrResults = _gfVision.RunOCR(_ScreenShot);
            }

            int index = ocrResults.FindIndex(e => e.Text.ToLower().Contains(text.ToLower()));

            if (index >= 0)
            {
                Rectangle currentPosition = ocrResults[index].Bound;
                if (_isImageCropped)
                    currentPosition = AdjustBoundingBox(currentPosition);

                kr.AdditionalInfo = currentPosition.ToString();
                _point = GetCenterPoint(currentPosition);
                _ClickPosition.Invoke(_point);
            }
            else if (_currentOcrMethod == OcrMethod.Azure)
            {
                // Only try Analyze call if we haven't used it already
                string analyzeHash = _ocrCacheHelper.GetImageHash(screenshotBitmap) + "_analyze";
                if (!_ocrCacheHelper.TryLoadCachedResult(analyzeHash, out ocrResults))
                {
                    ocrResults = RunAnalyzeAzureAsync(_ScreenShot).Result;
                    _azureAnalyzeApiCalls++;
                    LogAzureAnalyzeStats();
                    _ocrCacheHelper.SaveResultToCache(analyzeHash, ocrResults);
                }

                index = ocrResults.FindIndex(e => e.Text.ToLower().Contains(text.ToLower()));
                if (index >= 0)
                {
                    Rectangle currentPosition = ocrResults[index].Bound;
                    if (_isImageCropped)
                        currentPosition = AdjustBoundingBox(currentPosition);

                    kr.AdditionalInfo = currentPosition.ToString();
                    _point = GetCenterPoint(currentPosition);
                    _ClickPosition.Invoke(_point);
                    return kr;
                }

                // Not found in Analyze either
                CaptureOCRResultsImage(kr, ocrResults);
                kr.Result = KeywordResults.Fail;
                kr.Output = "Could not find the given text";
            }

            return kr;
        }

        [KeywordDescription("Stops using the cropped screenshot and resets cropping")]
        [KeywordDisplayName("Stop Crop")]
        [SampleScript("Vision.Stop Crop ()")]
        public KeywordResult StopCrop()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                _isImageCropped = false;
                croppedValue.Clear();
                _ScreenShot = null;
                kr.Output = "Cropping disabled. Future screenshots will use the full screen.";
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Error while stopping crop.";
                kr.AdditionalInfo = ex.ToString();
                Logger.Error("StopCrop error", ex);
            }

            return kr;
        }



        private Rectangle AdjustBoundingBox(Rectangle originalBound)
        {
            Rectangle newBounds;
            switch (croppedValue.Keys.FirstOrDefault().ToLower())
            {
                case "top":
                    newBounds = new Rectangle(
                 originalBound.X,
                 originalBound.Y + croppedValue["top"],
                 originalBound.Width,
                 originalBound.Height);
                    break;
                case "bottom":
                    newBounds = new Rectangle(
                 originalBound.X,
                 originalBound.Y,
                 originalBound.Width,
                 originalBound.Height);
                    break;
                case "left":
                    newBounds = new Rectangle(
                 originalBound.X + croppedValue["left"],
                 originalBound.Y,
                 originalBound.Width,
                 originalBound.Height);
                    break;
                case "right":
                    newBounds = new Rectangle(
                 originalBound.X,
                 originalBound.Y,
                 originalBound.Width,
                 originalBound.Height);
                    break;

                default:
                    newBounds = new Rectangle(
                 originalBound.X,
                 originalBound.Y,
                 originalBound.Width,
                 originalBound.Height);
                    break;
            }

            return newBounds;
        }

        [KeywordDescription("Crops one side of the screenshot by given percentage")]
        [KeywordDisplayName("Crop Screenshot")]
        [KeywordParameters("side, percentage", "Side to crop (top, bottom, left, right) and percentage to remove from that side")]
        [SampleScript("Vision.Crop Screenshot (top, 20)")]
        public KeywordResult CropScreenshot(string side, string percentage)
        {
            _isImageCropped = true;
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                // Convert percentage string to double
                if (!double.TryParse(percentage, out double pct))
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = "Percentage must be a numeric value.";
                    return kr;
                }

                if (pct < 0 || pct > 100)
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = "Percentage must be between 0 and 100.";
                    return kr;
                }

                // Convert byte[] screenshot to Bitmap
                using (MemoryStream ms = new MemoryStream(_GetScreenShot.Invoke()))
                using (Bitmap fullScreenshot = new Bitmap(ms))
                {
                    int width = fullScreenshot.Width;
                    int height = fullScreenshot.Height;

                    Rectangle cropArea = new Rectangle(0, 0, width, height);

                    switch (side.ToLower())
                    {
                        case "top":
                            cropArea.Y = (int)(height * (pct / 100.0));
                            cropArea.Height = height - cropArea.Y;
                            croppedValue["top"] = cropArea.Y;
                            break;
                        case "bottom":
                            int bottomCrop = (int)(height * (pct / 100.0));
                            cropArea.Height = height - bottomCrop;
                            croppedValue["bottom"] = bottomCrop;
                            break;
                        case "left":
                            cropArea.X = (int)(width * (pct / 100.0));
                            cropArea.Width = width - cropArea.X;
                            croppedValue["left"] = cropArea.X;
                            break;
                        case "right":
                            int rightCrop = (int)(width * (pct / 100.0));
                            cropArea.Width = width - rightCrop;
                            croppedValue["right"] = rightCrop;
                            break;

                        default:
                            kr.Result = KeywordResults.Fail;
                            kr.Output = "Invalid side specified. Use: top, bottom, left, or right.";
                            return kr;
                    }

                    // Crop the image
                    using (Bitmap cropped = fullScreenshot.Clone(cropArea, fullScreenshot.PixelFormat))
                    using (MemoryStream croppedStream = new MemoryStream())
                    {
                        cropped.Save(croppedStream, ImageFormat.Png);
                        _ScreenShot = croppedStream.ToArray(); // Convert back to byte[]
                        kr.ScreenShot = _ScreenShot;
                    }

                    kr.Output = $"Screenshot cropped from {side} by {pct}% successfully.";
                }
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Error during cropping.";
                kr.AdditionalInfo = ex.ToString();
                Logger.Error("CropScreenshotSide error", ex);
            }

            return kr;
        }




        [KeywordDescription("Click text with given index")]
        [KeywordDisplayName("Click Text with index")]
        [KeywordParameters("text", "Text to touch with index")]
        [KeywordParameters("index", "Index of text.")]
        [SampleScript("Vision.Click Text with index (Help,1)")]
        public KeywordResult ClickTextWithIndex(string text, string index)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            text = text.Replace("(", @"\(").Replace(")", @"\)");

            if (string.IsNullOrEmpty(text))
            {
                kr.Result = KeywordResults.Fail;
                kr.Output = "Please provide a text to search";
                return kr;
            }

            if (!int.TryParse(index, out int indexNumber) || indexNumber < 1)
            {
                kr.Result = KeywordResults.Fail;
                kr.Output = $"Invalid index: {index}";
                return kr;
            }

            if (!_isImageCropped)
            {
                _ScreenShot = _GetScreenShot();
            }

            Bitmap screenshotBitmap = ConvertBytesToBitmap(_ScreenShot);
            var _ocrCacheHelper = new OcrCacheHelper();
            List<OcrResult> ocrResults;

            try
            {
                if (_currentOcrMethod == OcrMethod.Azure)
                {
                    string hash = _ocrCacheHelper.GetImageHash(screenshotBitmap);
                    string analyzeHash = hash + "_analyze";

                    bool hasRunCache = _ocrCacheHelper.TryLoadCachedResult(hash, out var runCache);
                    bool hasAnalyzeCache = _ocrCacheHelper.TryLoadCachedResult(analyzeHash, out var analyzeCache);

                    if (hasAnalyzeCache)
                    {
                        _azureAnalyzeCacheHits++;
                        LogAzureAnalyzeStats();
                        ocrResults = analyzeCache;
                    }
                    else if (hasRunCache)
                    {
                        _azureOcrCacheHits++;
                        LogAzureOcrStats();
                        ocrResults = runCache;
                    }
                    else
                    {
                        ocrResults = RunAzureOCRAsync(_ScreenShot).Result;
                        _azureOcrApiCalls++;
                        LogAzureOcrStats();
                        _ocrCacheHelper.SaveResultToCache(hash, ocrResults);
                    }
                }
                else if (_currentOcrMethod == OcrMethod.AI)
                {
                    // AI OCR path
                    ocrResults = RunAiOCRAsync(_ScreenShot).Result;
                }
                else
                {
                    ocrResults = _gfVision.RunOCR(_ScreenShot);
                }

                var matchedBounds = ocrResults
                    .Where(e => e.Text.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                    .Select(e => e.Bound)
                    .ToList();

                if (matchedBounds.Count == 0 && _currentOcrMethod == OcrMethod.Azure)
                {
                    string analyzeHash = _ocrCacheHelper.GetImageHash(screenshotBitmap) + "_analyze";

                    if (!_ocrCacheHelper.TryLoadCachedResult(analyzeHash, out ocrResults))
                    {
                        ocrResults = RunAnalyzeAzureAsync(_ScreenShot).Result;
                        _azureAnalyzeApiCalls++;
                        LogAzureAnalyzeStats();
                        _ocrCacheHelper.SaveResultToCache(analyzeHash, ocrResults);
                    }

                    matchedBounds = ocrResults
                        .Where(e => e.Text.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                        .Select(e => e.Bound)
                        .ToList();
                }

                if (matchedBounds.Count == 0)
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Could not find any match for text: '{text}'";
                    CaptureOCRResultsImage(kr, ocrResults);
                    return kr;
                }

                if (indexNumber > matchedBounds.Count)
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Index out of range for text '{text}': {indexNumber} (only {matchedBounds.Count} found)";
                    CaptureOCRResultsImage(kr, ocrResults);
                    return kr;
                }

                Rectangle selectedBound = matchedBounds[indexNumber - 1];

                if (_isImageCropped)
                {
                    selectedBound = AdjustBoundingBox(selectedBound);
                }

                kr.AdditionalInfo = selectedBound.ToString();

                _point = GetCenterPoint(selectedBound);
                _ClickPosition.Invoke(_point);

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Exception occurred";
                kr.ScreenShot = _ScreenShot;
                Logger.Error(ex);
                return kr;
            }
        }



        [KeywordDescription("Click object which is next to given text x and Y Location")]
        [KeywordDisplayName("Click Next To Text")]
        [KeywordParameters("text", "Text after which for a given x and y need to be clicked")]
        [KeywordParameters("xLocation", "how much width next to text. X value")]
        [KeywordParameters("yLocation", "how much width next to text. Y value")]
        [SampleScript("Vision.Click Next To Text (Password,50,10)")]
        public KeywordResult ClickNextToText(string text, string xLocation, string yLocation = "0")
        {
            text = text.Replace("(", @"\(").Replace(")", @"\)");
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            if (string.IsNullOrEmpty(text))
            {
                kr.Result = KeywordResults.Fail;
                kr.Output = "Please provide a text to search";
                return kr;
            }

            if (!_isImageCropped)
            {
                _ScreenShot = _GetScreenShot();
            }

            Bitmap screenshotBitmap = ConvertBytesToBitmap(_ScreenShot);
            var _ocrCacheHelper = new OcrCacheHelper();
            List<OcrResult> ocrResults;

            try
            {
                if (_currentOcrMethod == OcrMethod.Azure)
                {
                    string hash = _ocrCacheHelper.GetImageHash(screenshotBitmap);
                    string analyzeHash = hash + "_analyze";

                    bool hasRunCache = _ocrCacheHelper.TryLoadCachedResult(hash, out var runCache);
                    bool hasAnalyzeCache = _ocrCacheHelper.TryLoadCachedResult(analyzeHash, out var analyzeCache);

                    if (hasAnalyzeCache)
                    {
                        _azureAnalyzeCacheHits++;
                        LogAzureAnalyzeStats();
                        ocrResults = analyzeCache;
                    }
                    else if (hasRunCache)
                    {
                        _azureOcrCacheHits++;
                        LogAzureOcrStats();
                        ocrResults = runCache;
                    }
                    else
                    {
                        ocrResults = RunAzureOCRAsync(_ScreenShot).Result;
                        _azureOcrApiCalls++;
                        LogAzureOcrStats();
                        _ocrCacheHelper.SaveResultToCache(hash, ocrResults);
                    }
                }
                else if (_currentOcrMethod == OcrMethod.AI)
                {
                    // AI OCR path
                    ocrResults = RunAiOCRAsync(_ScreenShot).Result;
                }
                else
                {
                    ocrResults = _gfVision.RunOCR(_ScreenShot);
                }

                int index = ocrResults.FindIndex(e => e.Text.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0);

                if (index < 0 && _currentOcrMethod == OcrMethod.Azure)
                {
                    string analyzeHash = _ocrCacheHelper.GetImageHash(screenshotBitmap) + "_analyze";

                    if (!_ocrCacheHelper.TryLoadCachedResult(analyzeHash, out ocrResults))
                    {
                        ocrResults = RunAnalyzeAzureAsync(_ScreenShot).Result;
                        _azureAnalyzeApiCalls++;
                        LogAzureAnalyzeStats();
                        _ocrCacheHelper.SaveResultToCache(analyzeHash, ocrResults);
                    }

                    index = ocrResults.FindIndex(e => e.Text.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                if (index < 0)
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Could not find the given text: '{text}'";
                    CaptureOCRResultsImage(kr, ocrResults);
                    return kr;
                }

                Rectangle bound = ocrResults[index].Bound;
                if (_isImageCropped)
                {
                    bound = AdjustBoundingBox(bound);
                }

                kr.AdditionalInfo = bound.ToString();
                _point = GetCenterPointNextToControl(bound, Convert.ToInt32(xLocation), Convert.ToInt32(yLocation));
                _ClickPosition.Invoke(_point);

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Error occurred while clicking next to text.";
                kr.AdditionalInfo = ex.ToString();
                Logger.Error("ClickNextToText failed", ex);
                return kr;
            }
        }


        [KeywordDescription("Click next to the text at a given index with offset x and y")]
        [KeywordDisplayName("Click Next To Text with index")]
        [KeywordParameters("text", "Text to search for")]
        [KeywordParameters("index", "Index of the text match to use (1-based)")]
        [KeywordParameters("xLocation", "X offset from the matched text")]
        [KeywordParameters("yLocation", "Y offset from the matched text")]
        [SampleScript("Vision.Click Next To Text with index (Username,2,50,10)")]
        public KeywordResult ClickNextToTextWithIndex(string text, string index, string xLocation, string yLocation = "0")
        {
            text = text.Replace("(", @"\(").Replace(")", @"\)");
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            if (string.IsNullOrEmpty(text))
            {
                kr.Result = KeywordResults.Fail;
                kr.Output = "Please provide a text to search";
                return kr;
            }

            if (!int.TryParse(index, out int indexNumber) || indexNumber < 1)
            {
                kr.Result = KeywordResults.Fail;
                kr.Output = $"Invalid index: {index}";
                return kr;
            }

            if (!_isImageCropped)
            {
                _ScreenShot = _GetScreenShot();
            }

            Bitmap screenshotBitmap = ConvertBytesToBitmap(_ScreenShot);
            var _ocrCacheHelper = new OcrCacheHelper();
            List<OcrResult> ocrResults;

            try
            {
                if (_currentOcrMethod == OcrMethod.Azure)
                {
                    string hash = _ocrCacheHelper.GetImageHash(screenshotBitmap);
                    string analyzeHash = hash + "_analyze";

                    bool hasRunCache = _ocrCacheHelper.TryLoadCachedResult(hash, out var runCache);
                    bool hasAnalyzeCache = _ocrCacheHelper.TryLoadCachedResult(analyzeHash, out var analyzeCache);

                    if (hasAnalyzeCache)
                    {
                        _azureAnalyzeCacheHits++;
                        LogAzureAnalyzeStats();
                        ocrResults = analyzeCache;
                    }
                    else if (hasRunCache)
                    {
                        _azureOcrCacheHits++;
                        LogAzureOcrStats();
                        ocrResults = runCache;
                    }
                    else
                    {
                        ocrResults = RunAzureOCRAsync(_ScreenShot).Result;
                        _azureOcrApiCalls++;
                        LogAzureOcrStats();
                        _ocrCacheHelper.SaveResultToCache(hash, ocrResults);
                    }
                }
                else if (_currentOcrMethod == OcrMethod.AI)
                {
                    // AI OCR path
                    ocrResults = RunAiOCRAsync(_ScreenShot).Result;
                }
                else
                {
                    ocrResults = _gfVision.RunOCR(_ScreenShot);
                }

                var matchingBounds = ocrResults
                    .Where(e => e.Text.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                    .Select(e => e.Bound)
                    .ToList();

                if (matchingBounds.Count == 0 && _currentOcrMethod == OcrMethod.Azure)
                {
                    string analyzeHash = _ocrCacheHelper.GetImageHash(screenshotBitmap) + "_analyze";

                    if (!_ocrCacheHelper.TryLoadCachedResult(analyzeHash, out ocrResults))
                    {
                        ocrResults = RunAnalyzeAzureAsync(_ScreenShot).Result;
                        _azureAnalyzeApiCalls++;
                        LogAzureAnalyzeStats();
                        _ocrCacheHelper.SaveResultToCache(analyzeHash, ocrResults);
                    }

                    matchingBounds = ocrResults
                        .Where(e => e.Text.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                        .Select(e => e.Bound)
                        .ToList();
                }

                if (matchingBounds.Count == 0)
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Could not find the text: {text}";
                    CaptureOCRResultsImage(kr, ocrResults);
                    return kr;
                }

                if (indexNumber > matchingBounds.Count)
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Index out of range: {text}, {indexNumber}";
                    CaptureOCRResultsImage(kr, ocrResults);
                    return kr;
                }

                Rectangle selectedBound = matchingBounds[indexNumber - 1];
                if (_isImageCropped)
                {
                    selectedBound = AdjustBoundingBox(selectedBound);
                }

                kr.AdditionalInfo = selectedBound.ToString();
                _point = GetCenterPointNextToControl(selectedBound, Convert.ToInt32(xLocation), Convert.ToInt32(yLocation));
                _ClickPosition.Invoke(_point);

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Error occurred while clicking next to text with index.";
                kr.AdditionalInfo = ex.ToString();
                Logger.Error("ClickNextToTextWithIndex failed", ex);
                return kr;
            }
        }
        [KeywordDescription("Click middle of the object with x and Y Location")]
        [KeywordDisplayName("Click XY Location")]
        [KeywordParameters("xLocation", "x location of the text. X value")]
        [KeywordParameters("yLocation", "y location of the text. Y value")]
        [KeywordParameters("width", "width of the text. width value")]
        [KeywordParameters("height", "height of the text. height value")]
        [SampleScript("Vision.Click XY Location (72,299,19,19)")]
        public KeywordResult ClickXYLocation(string xLocation, string yLocation, string width, string height)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            _ScreenShot = _GetScreenShot();

            try
            {
                // Ensure width and height are non-zero
                if (Convert.ToInt32(width) == 0 || Convert.ToInt32(height) == 0)
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = "Width and height cannot be zero. Please provide valid values.";
                    return kr;
                }

                // Calculate the center point
                int x = Convert.ToInt32(xLocation) + (Convert.ToInt32(width) / 2);
                int y = Convert.ToInt32(yLocation) + (Convert.ToInt32(height) / 2);

                _point = new MousePoint(x, y);
                _ClickPosition.Invoke(_point);
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Launch error";
                kr.AdditionalInfo = ex.ToString();
                Logger.Error("Launch error", ex);
            }

            return kr;
        }

        private MousePoint GetCenterPointNextToControl(Rectangle bound, int xLocation, int yLocation = 0)
        {
            int x = bound.X + bound.Width / 2;
            int y = bound.Y + bound.Height / 2;
            return new MousePoint(x + xLocation, y + yLocation);
        }
        /*
         * Remove Get Count Keyword because count might be incorrect and this keyword is not much applicable in test scenario.
         * 
         
        [KeywordDescription("Gets the number of count that text contains in the screen")]
        [KeywordDisplayName("Get Count")]
        [KeywordParameters("text", "text to find")]
        [KeywordParameters("number", "Number of occurences")]
        public KeywordResult GetCount(string text, string number)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            _ScreenShot = _GetScreenShot();
            List<OcrResult> results = _gfVision.RunOCR(_ScreenShot);
            var filteredList = results.Count(e => e.Text.ToLower().Contains(text.ToLower()));
            try
            {
                if (filteredList == Convert.ToInt32(number))
                {
                    kr.Result = KeywordResults.Pass;
                }
                else
                {
                    // If text not found then capture the screen with OCR ocrResults
                    CaptureOCRResultsImage(kr, results);
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Count is not matching";
                }
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Get count error";
                kr.AdditionalInfo = ex.ToString();
                Logger.Error("Get count error", ex);
            }

            return kr;
        }
        
         */


        [KeywordDescription("Setting the text based on word or Block. If groupingType is not set to word ,by default it sets to block . It should be called after Set Target.")]
        [KeywordDisplayName("Set Text Grouping")]
        [KeywordParameters("groupingType", "OCR engine to find text by word or block")]
        [SampleScript("Vision.Set Text Grouping (word)")]
        public KeywordResult SetTextGrouping(string groupingType)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                _gfVision.SetPageIterationLevel(groupingType);

            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Set Text Grouping error";
                kr.AdditionalInfo = ex.ToString();
                Logger.Error("Set Text Grouping error", ex);
            }

            return kr;
        }

        [KeywordDescription("Wait until given text appears on the screen within specified time")]
        [KeywordDisplayName("Wait For Text")]
        [KeywordParameters("text", "Text to wait for")]
        [KeywordParameters("timeoutInSeconds", "Maximum time to wait in seconds")]
        [SampleScript("Vision.Wait For Text (Delete, 10)")]
        public KeywordResult WaitForText(string text, string timeoutInSeconds)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            if (string.IsNullOrEmpty(text))
            {
                return new KeywordResult(KeywordResults.Fail, "Text cannot be empty");
            }
            if (!int.TryParse(timeoutInSeconds, out int timeout))
            {
                return new KeywordResult(KeywordResults.Fail, "Timeout must be a number (seconds)");
            }
            DateTime endTime = DateTime.Now.AddSeconds(timeout);
            while (DateTime.Now <= endTime)
            {
                var result = IsTextExist(text);
                _ScreenShot = result.ScreenShot;
                if (result.Result == KeywordResults.Pass)
                {
                    kr.Output = $"Text '{text}' found within {timeout} seconds";
                    return kr;
                }
            }
            kr.Result = KeywordResults.Fail;
            kr.ScreenShot = _ScreenShot;
            kr.Output = $"Text '{text}' not found within {timeout} seconds";
            return kr;
        }

        #region "Various Device Methods"

        public bool MouseLeftClick(MousePoint pt)
        {
            MouseOperations.SetCursorPosition(pt);
            MouseOperations.MouseEvent(MouseEventFlags.LeftDown);
            Thread.Sleep(100);
            MouseOperations.MouseEvent(MouseEventFlags.LeftUp);
            return true;
        }

        private void CalculateResizeFactors()
        {
            System.Drawing.Image i = _ScreenImage;
            if (i == null) return;
            int oriWidth = (int)(i.PhysicalDimension.Width);
            int oriHeight = (int)(i.PhysicalDimension.Height);
            int dipWidth, dipHeight;


            var wfactor = (double)i.Width / _ScreenImage.Width;
            var hfactor = (double)i.Height / _ScreenImage.Height;

            _resizeFactor = Math.Max(wfactor, hfactor);
            dipWidth = (int)(i.Width / _resizeFactor);
            dipHeight = (int)(i.Height / _resizeFactor);
            _imgStartX = (_ScreenImage.Width - dipWidth) / 2;
            _imgStartY = (_ScreenImage.Height - dipHeight) / 2;

        }

        private void DrawBound(Rectangle rect, string caption = null)
        {
            int x, y, w, h;
            int x2, y2;

            x = (int)(rect.X / _resizeFactor) + _imgStartX;
            y = (int)(rect.Y / _resizeFactor) + _imgStartY;
            x2 = (int)((rect.X + rect.Width) / _resizeFactor) + _imgStartX;
            y2 = (int)((rect.Y + rect.Height) / _resizeFactor) + _imgStartY;


            w = x2 - x;
            h = y2 - y;

            Rectangle boundBox = new Rectangle(0, 0, 0, 0);

            boundBox.X = x;
            boundBox.Y = y;
            boundBox.Width = w;
            boundBox.Height = h;

            if (boundBox.X + boundBox.Y + boundBox.Width + boundBox.Height > 0)
            {
                using (Pen pen = new Pen(Color.Red, 3))
                {
                    _graphics.DrawRectangle(pen, boundBox);
                    Font f = new Font(FontFamily.GenericSansSerif, 7, FontStyle.Bold);
                    _graphics.DrawString(caption, f, Brushes.Magenta, x, Math.Max(0, y - 14));
                }
            }
        }

        private bool WindowsClick(MousePoint point)
        {
            return MouseLeftClick(new MousePoint(point.X, point.Y));
        }

        private bool AndroidClick(MousePoint point)
        {
            return _SESLibrary.PressScreen(point.X, point.Y);
        }

        private bool JediOmniClick(MousePoint point)
        {
            _JediOmni.ControlPanel.PressScreen(point.X, point.Y);
            return true;
        }

        private bool MacClick(MousePoint point)
        {
            try
            {
                // Get screen/window size
                _windowSize = _MAC.Manage().Window.Size;

                // Convert the screenshot to get scaling factor
                Image image = Image.FromStream(new MemoryStream(_ScreenShot));
                double xFactor = (double)image.Width / _windowSize.Width;
                double yFactor = (double)image.Height / _windowSize.Height;

                // Get pixel ratio if available
                _pixelRatio = _MAC.GetPixelRatio();
                if (_pixelRatio == 0)
                    _pixelRatio = 1;

                // Convert point based on screen scale and pixel ratio
                int screenX = (int)(point.X / xFactor / _pixelRatio);
                int screenY = (int)(point.Y / yFactor / _pixelRatio);

                // Setup pointer device (mouse)
                var mouse = new OpenQA.Selenium.Interactions.PointerInputDevice(PointerKind.Mouse);
                var sequence = new OpenQA.Selenium.Interactions.ActionSequence(mouse, 0);

                // Move to screen coordinate
                sequence.AddAction(mouse.CreatePointerMove(CoordinateOrigin.Viewport, screenX, screenY, TimeSpan.FromMilliseconds(100)));

                // Click
                sequence.AddAction(mouse.CreatePointerDown(MouseButton.Left));
                sequence.AddAction(mouse.CreatePause(TimeSpan.FromMilliseconds(100)));
                sequence.AddAction(mouse.CreatePointerUp(MouseButton.Left));

                // Perform the action
                _MAC.PerformActions(new List<ActionSequence> { sequence });

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Error performing mouse click", ex);
                return false;
            }
        }

        private bool IOSClick(MousePoint point)
        {
            foreach (KeyValuePair<string, object> attr in _IOS.SessionDetails)
            {
                double plat_v;
                if (attr.Key is "platformVersion")
                {
                    if (double.TryParse(attr.Value.ToString(), out plat_v))
                    {
                        if (plat_v <= 17)
                        {
                            _pixelRatio = 2;

                        }
                        else
                        {
                            _pixelRatio = 3;

                        }
                    }

                }
            }
            int oriX = (int)(point.X / _pixelRatio);

            int oriY = (int)(point.Y / _pixelRatio);

            Console.WriteLine($"Input: ({point.X}, {point.Y}), PixelRatio: {_pixelRatio}, Adjusted: ({oriX}, {oriY})");

            try
            {
                var finger = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch, "finger");

                var tapSequence = new ActionSequence(finger, 0);

                tapSequence.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, oriX, oriY, TimeSpan.Zero));
                tapSequence.AddAction(finger.CreatePointerDown(PointerButton.TouchContact));
                tapSequence.AddAction(finger.CreatePointerUp(PointerButton.TouchContact));
                _IOS.PerformActions(new List<ActionSequence> { tapSequence });
                return true;
            }

            catch (Exception ex)
            {

                Console.WriteLine($"Click (tap) failed at ({point.X},{point.Y}) - {ex.Message}");
                return false;

            }

        }
        

        private byte[] JediOmni_CaptureScreen()
        {
            var Image = _JediOmni.ControlPanel.ScreenCapture();
            return Image.ToByteArray();
        }

        private byte[] GetIOSScreenCapture()
        {
            try
            {
                return _IOS.GetScreenshot().AsByteArray;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private byte[] GetMacScreenCapture()
        {
            try
            {
                return _MAC.GetScreenshot().AsByteArray;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// This method captures original image and draw ocr results Draw bound texts found 
        /// in the screen captured image.
        /// </summary>
        /// <param name="kr"></param>
        /// <param name="ocrResults"></param>
        private void CaptureOCRResultsImage(KeywordResult kr, List<OcrResult> ocrResults)
        {
            _ScreenImage = byteArrayToImage(_ScreenShot);
            CalculateResizeFactors();
            _graphics = Graphics.FromImage(_ScreenImage);
            foreach (var result in ocrResults)
            {
                DrawBound(result.Bound, result.Text);
            }

            kr.ScreenShot = _ScreenImage.ToByteArray();
        }

        private MousePoint GetCenterPoint(Rectangle bound)
        {
            int x = bound.X + bound.Width / 2;
            int y = bound.Y + bound.Height / 2;
            return new MousePoint(x, y);
        }

        #endregion

        #region UtilityMethods
        public Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }

        public Bitmap byteArrayToBmp(byte[] imageData)
        {
            Bitmap bmp;
            using (var ms = new MemoryStream(imageData))
            {
                bmp = new Bitmap(ms);
            }
            return bmp;
        }

        #endregion

        #region "Azure Vision Cache implementation"

        private List<OcrResult> RunAzureOCRWithCache(Bitmap screenshot, OcrCacheHelper cacheHelper)
        {
            string hash = cacheHelper.GetImageHash(screenshot);

            if (cacheHelper.TryLoadCachedResult(hash, out var cachedResults))
            {
                _azureOcrCacheHits++;
                LogAzureOcrStats();
                return cachedResults;
            }

            var result = RunAzureOCRAsync(_ScreenShot).Result;
            _azureOcrApiCalls++;
            LogAzureOcrStats();

            cacheHelper.SaveResultToCache(hash, result);
            return result;
        }

        private List<OcrResult> RunAnalyzeAzureWithCache(Bitmap screenshot, OcrCacheHelper cacheHelper)
        {
            string hash = cacheHelper.GetImageHash(screenshot) + "_analyze";

            if (cacheHelper.TryLoadCachedResult(hash, out var cachedResults))
            {
                _azureAnalyzeCacheHits++;
                LogAzureAnalyzeStats();
                return cachedResults;
            }

            var result = RunAnalyzeAzureAsync(_ScreenShot).Result;
            _azureAnalyzeApiCalls++;
            LogAzureAnalyzeStats();

            cacheHelper.SaveResultToCache(hash, result);
            return result;
        }

        public Bitmap ConvertBytesToBitmap(byte[] imageBytes)
        {
            using (var ms = new MemoryStream(imageBytes))
            {
                return new Bitmap(ms);
            }
        }

        private void LogAzureOcrStats()
        {
            Logger.Trace($"Azure OCR API Calls: {_azureOcrApiCalls}, Cache Hits: {_azureOcrCacheHits}");
        }

        private void LogAzureAnalyzeStats()
        {
            Logger.Trace($"Azure Analyze API Calls: {_azureAnalyzeApiCalls}, Cache Hits: {_azureAnalyzeCacheHits}");
        }
        #endregion

        #region AIOCR
        // AI OCR implementation
        //1.Makes sure AI OCR is already initiated.2.Uploads the screenshot to your AI OCR server(/whatisthis).3.Gets back JSON results.4.Converts the JSON into a list of OcrResult.5.Returns that list to the caller.
        private async Task<List<OcrResult>> RunAiOCRAsync(byte[] imageBytes)
        {
            if (!_aiInitiated)
            {
                throw new Exception("AI OCR has not been initiated. Please call SetAIOCRMethod first.");
            }

            try
            {
                using (HttpClient client = new HttpClient())
                using (var form = new MultipartFormDataContent())
                {
                    var fileContent = new ByteArrayContent(imageBytes);
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                    form.Add(fileContent, "file", "screenshot.png");

                    var response = await client.PostAsync($"{_aiBaseUrl}/whatisthis", form);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"AI OCR whatisthis failed: {response.StatusCode}");
                    }
                    //result from AI server
                    string resultJson = await response.Content.ReadAsStringAsync();
                    return ParseAiOCRResponse(resultJson);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("RunAiOCRAsync failed", ex);
                throw;
            }
        }
        private List<OcrResult> ParseAiOCRResponse(string json)
        {
            var results = new List<OcrResult>();
            if (string.IsNullOrWhiteSpace(json))
            {
                Logger.Error("AI OCR response is empty or null.");
                return results;
            }
            try
            {
                dynamic aiResponse = JsonConvert.DeserializeObject(json);
                if (aiResponse?.objects == null)
                {
                    Logger.Error("AI OCR response does not contain 'objects'.");
                    return results;
                }
                foreach (var obj in aiResponse.objects)
                {
                    string text = obj[0]?.ToString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        int x;
                        int y;
                        int w = 0;
                        int h = 0;

                        // Parse x
                        if (!int.TryParse(obj[1]?.ToString(), out x))
                        {
                            x = 0;
                        }

                        // Parse y
                        if (!int.TryParse(obj[2]?.ToString(), out y))
                        {
                            y = 0;
                        }

                        // If AI later returns width/height (obj[3], obj[4]) use them
                        if (obj.Count >= 5)
                        {
                            int.TryParse(obj[3]?.ToString(), out w);
                            int.TryParse(obj[4]?.ToString(), out h);
                        }

                        results.Add(new OcrResult(text, new Rectangle(x, y, w, h)));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error parsing AI OCR response", ex);
            }

            return results;
        }

        //calls an AI OCR server to initialize it → then sets the OCR method to AI → and returns whether it worked or failed.
        [KeywordDescription("Set AI OCR method with solution name for text recognition")]
        [KeywordDisplayName("Set AI OCR Method")]
        [KeywordParameters("baseUrl", "Base URL of the AI OCR server")]
        [KeywordParameters("nameOfSolution", "Solution name to be used for initiating AI OCR method")]
        [SampleScript("Vision.Set AI OCR Method (http://server-url:port, SolutionName)")]
        public KeywordResult SetAIOCRMethod(string baseUrl,string nameOfSolution)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                if (string.IsNullOrEmpty(nameOfSolution))
                {
                    return new KeywordResult(KeywordResults.Fail, "Parameter 'nameOfSolution' cannot be empty.");
                }

                _currentOcrMethod = OcrMethod.AI;
                _aiSolutionName = nameOfSolution;
                _aiBaseUrl = baseUrl;   

                using (HttpClient client = new HttpClient())
                {
                    var initResponse = client.PostAsync($"{_aiBaseUrl}/initiate?name_of_solution={_aiSolutionName}", null).Result;
                    if (!initResponse.IsSuccessStatusCode)
                    {
                        return new KeywordResult(KeywordResults.Fail, $"AI OCR Initiate failed: {initResponse.StatusCode}");
                    }
                }

                _aiInitiated = true;
                kr.Output = $"AI OCR initiated successfully with solution '{nameOfSolution}'.";
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = $"Error setting AI OCR method: {ex.Message}";
                kr.AdditionalInfo = ex.ToString();
            }

            return kr;
        }

        #endregion
    }
}



enum MyEnum
{
    __gfVision
}




// TODO: Keywords implementation
// Change OCR Page Seg Mode(page segmode)
// Click(text), Is Exist(text), Is Checkbox Selected(text), Is Radio Button selected(text)
// Click Image(image path), Is Image Exist(image path)
