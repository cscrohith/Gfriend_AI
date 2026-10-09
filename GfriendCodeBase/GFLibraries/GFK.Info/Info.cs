using HP.GFriend.GFLogger;
using HP.GFriend.Keywords.Event;
using HP.GFriend.Utils.Charter;
using HP.Automation.SES;
using System;
using System.Collections.Generic;
using System.IO;


namespace HP.GFriend.Keywords
{
    public class Info : IGFLibrary
    {
        private static Dictionary<string, MeasureSet> _measureSets;
        private static DeviceUnderTest _dut;
        private static string _outputDir;
        private static MemoryUsageItem _memoryUsage = null;
        private static bool _isAndroidUsed = false;
        private SESLib _android;
        public string _testCaseName = "";

        public void Dispose()
        {
            _measureSets = null;
            if(_isAndroidUsed)
            {
                _android.Dispose();
                _android = null;
                _isAndroidUsed = false;
            }
        }

        public string GetName()
        {
            return "Info";
        }

        public List<string> GetDependencies()
        {
            return null;
        }

        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
            _dut = dut;
            _outputDir = outputDir;
            _measureSets = new Dictionary<string, MeasureSet>();
        }

        public bool DutUsed()
        {
            return false;
        }

        private void GetAndroidController()
        {
            _android = (SESLib)CommonExecutionInfo.GetSharedObject("android:" + _dut.DeviceId);
            
        }

        [KeywordDescription("Start new set of time checker")]
        [KeywordDisplayName("Start Time Check")]
        [KeywordParameters("measureSet", "name of sets of check point. \r\n\tIf this name starts with 'Global_' data will be saved in same file while repeatation in Remote Execution")]
        [SampleScript("Info.Start Time Check (LoginTime)")]
        public KeywordResult StartTimeCheck(string measureSet)
        {
            string csvPath = "";
            try
            {
                if(CommonExecutionInfo.CurrentRepeatCount > 0 && measureSet.StartsWith("Global_",StringComparison.CurrentCultureIgnoreCase))
                {
                    csvPath = Path.Combine(Directory.GetParent(_outputDir).FullName, $"{measureSet}.csv");
                }
                else
                {
                    csvPath = Path.Combine(_outputDir, $"{measureSet}.csv");
                }
                
            }
            catch(Exception ex)
            {
                KeywordResult err = new KeywordResult(KeywordResults.Error);
                err.Output = "Can not create csv file";
                err.AdditionalInfo = ex.ToString();
                return err;
            }
            
            if (_measureSets.ContainsKey(measureSet))
            {
                _measureSets.Remove(measureSet);
            }

            MeasureSet aItem = new MeasureSet(measureSet, csvPath);
            _measureSets[measureSet] = aItem;
            KeywordResult r = new KeywordResult(KeywordResults.Pass);
            r.Output = $"Time check is started and output will be saved to {csvPath}";
            return r;
        }

        [KeywordDescription("Mark start time with given check point name in measure set.\r\n" +
            "Raises KeywordEvent with EventDetail forms of Tuple<string, string>(measureSet, checkPoint)")]
        [KeywordDisplayName("Mark Start")]
        [KeywordParameters("measureSet", "measure set to save check point")]
        [KeywordParameters("checkPoint", "checkpoint name to save time data")]
        [SampleScript("Info.Mark Start (LoginTime,${Sample})")]
        public KeywordResult MarkStart(string measureSet, string checkPoint)
        {
            if(!_measureSets.ContainsKey(measureSet))
            {
                KeywordResult err = new KeywordResult(KeywordResults.Error);
                err.Output = $"Can not find Measure set with name {measureSet}";
                return err;
            }
            _measureSets[measureSet].ResetStartTime();
            KeywordEvent.Trigger("MarkStart", new Tuple<string, string>(measureSet, checkPoint));
            KeywordResult r = new KeywordResult(KeywordResults.Pass);
            return r;
        }

        [KeywordDescription("Mark end time with given check point name in measure set\r\n" +
            "Raises KeywordEvent with EventDetail forms of Tuple<string, string>(measureSet, checkPoint)")]
        [KeywordDisplayName("Mark End")]
        [KeywordParameters("measureSet", "measure set to save check point")]
        [KeywordParameters("checkPoint", "checkpoint name to save time data")]
        [SampleScript("Info.Mark End (LoginTime,${Sample1})")]
        public KeywordResult MarkEnd(string measureSet, string checkPoint)
        {
            if(!_measureSets.ContainsKey(measureSet))
            {
                KeywordResult err = new KeywordResult(KeywordResults.Error);
                err.Output = $"Can not find Measure set with name {measureSet}";
                return err;
            }
            if (CommonExecutionInfo.CurrentRepeatCount > 0 && measureSet.StartsWith("Global_", StringComparison.CurrentCultureIgnoreCase))
            {
                checkPoint = string.Join("_", CommonExecutionInfo.CurrentRepeatCount.ToString(), checkPoint.Trim());
            }
            _measureSets[measureSet].AddTimeCheck(checkPoint.Trim());
            KeywordEvent.Trigger("MarkEnd", new Tuple<string, string>(measureSet, checkPoint));
            KeywordResult r = new KeywordResult(KeywordResults.Pass);
            return r;
        }

        [KeywordDescription("Draw a bar graph for the selected measure set. " +
        "Note: This keyword should be used at the end of your script, after all repetitions or iterations are completed. " +
        "It generates a graph showing all the data points collected during the iterations, making it easy to visualize and analyze " +
        "the overall performance.")]
        [KeywordDisplayName("Draw Time Graph")]
        [KeywordParameters("measureSet", "Measure set to draw graph")]
        [SampleScript(
        @"SamplePerformanceCheck_TC <br>
        {
        <br>    Info.Start Time Check (LoginTime)
    
        <br>    @Repeat:100
        <br>
            {
        <br>        Info.Mark Start (LoginTime,${R})
        <br>        Random Sleep (1,3)
        <br>        Info.Mark End (LoginTime,${R})
        <br>    }
        <br>
         <b> Info.Draw Time Graph (LoginTime) </b>
        <br>}<br>"
        )]
        public KeywordResult DrawTimeGraph(string measureSet)
        {
            try
            {
                
                ChartCreator chart = new ChartCreator(_measureSets[measureSet].GetCSVPath());
                string chartImage = Path.Combine(_outputDir, measureSet + ".png");
                chart.SaveImage(chartImage, System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column);
                if (CommonExecutionInfo.CurrentRepeatCount > 0)
                {
                    File.Copy(chartImage, Path.Combine(Directory.GetParent(_outputDir).FullName, measureSet+".png"), true);
                }
                KeywordResult r = new KeywordResult(KeywordResults.Pass);
                r.ScreenShot = File.ReadAllBytes(chartImage);
                return r;
            }
            catch (Exception ex)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Fail during create chart";
                fail.AdditionalInfo = ex.ToString();
                return fail;
            }
        }

        [KeywordDescription("Initialize Memory monitoring for Android")]
        [KeywordDisplayName("Start Memory Monitoring")]
        [Deprecated("Use Android.Start Memory Monitoring instead of this.")]
        [SampleScript("Android.Start Memory Monitoring")]
        public KeywordResult StartMemoryMonitoring()
        {

            _isAndroidUsed = true;
            GetAndroidController();

            string csvPath;
            _testCaseName = CommonExecutionInfo.GetVariable("_tcName");

            if (CommonExecutionInfo.CurrentRepeatCount > 0)
            {
                csvPath = Path.Combine(Directory.GetParent(_outputDir).FullName, $"{_testCaseName}_Android_Memory_Usage.csv");
                Logger.Trace($"Current Repeat Count is greater than 0. CSV Path is : {csvPath}");
            }
            else
            {
                csvPath = Path.Combine(_outputDir, $"{_testCaseName}_AndroidMemoryMonitoring_{DateTime.Now.ToString("yyMMdd_HHmmssff")}.csv");
                Logger.Trace($"CSV Path is : {csvPath}");
            }

            
            
            _memoryUsage = new MemoryUsageItem(csvPath);
            
            
            KeywordResult r = new KeywordResult(KeywordResults.Pass);
            r.Output = $"Memory Monitoring data will be saved to : {csvPath}";
            return r;
        }

        [KeywordDescription("Dump memory usage and save with given check point name")]
        [KeywordDisplayName("Collect Memory Info")]
        [KeywordParameters("checkPoint", "Check Point name to save")]
        [Deprecated("Use Android.Collect Memory Info instead of this.")]
        [SampleScript("Android.Collect Memory Info (${PackageName_ODB})")]
        public KeywordResult CollectMemoryInfo(string checkPoint)
        {
            GetAndroidController();
            try
            {
                Dictionary<string, long> memInfo = _android.GetMemoryUsage();

                if(CommonExecutionInfo.CurrentRepeatCount > 0)
                {
                    checkPoint = string.Join("_", CommonExecutionInfo.CurrentRepeatCount.ToString(), checkPoint.Trim());
                }
                
                _memoryUsage.AddData(checkPoint, memInfo);
                KeywordResult r = new KeywordResult(KeywordResults.Pass);
                return r;
            }
            catch(Exception ex)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Fail during dump memory";
                fail.AdditionalInfo = ex.ToString();
                GFLogger.Logger.Error(ex.ToString());
                return fail;
            }
            
        }

        [KeywordDescription("Draw Android Memory Usage Graph")]
        [KeywordDisplayName("Draw Memory Usage Graph")]
        [Deprecated("Use Android.Draw Memory Usage Graph instead of this.")]
        [SampleScript("Android.Draw Memory Usage Graph ()")]
        public KeywordResult DrawMemoryUsageGraph()
        {
            try
            {
                ChartCreator chart = new ChartCreator(_memoryUsage.GetCSVPath());
                string chartImage = Path.Combine(_outputDir, $"{_testCaseName}_AndroidMemory.png");
                chart.SaveImage(chartImage, System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line, true, "Total");

                if(CommonExecutionInfo.CurrentRepeatCount > 0)
                {
                    File.Copy(chartImage, Path.Combine(Directory.GetParent(_outputDir).FullName, $"{_testCaseName}_Android_Memory_Usage.png"), true);
                }

                KeywordResult r = new KeywordResult(KeywordResults.Pass);
                r.ScreenShot = File.ReadAllBytes(chartImage);
                return r;
            }
            catch (Exception ex)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Fail during create chart";
                fail.AdditionalInfo = ex.ToString();
                return fail;
            }
        }
    }
    
}
