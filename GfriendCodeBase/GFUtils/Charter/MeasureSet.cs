using HP.GFriend.Keywords;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HP.GFriend.Utils.Charter
{
    public class MeasureSet
    {
        private string _itemName;
        private string _csvPath;
        private Dictionary<string, string> _aData;
        private DateTime _currentStartTime;
        private StreamWriter _csvFile;


        public MeasureSet(string itemName, string csvPath)
        {
            this._currentStartTime = DateTime.Now;
            this._itemName = itemName;
            this._csvPath = csvPath;
            _aData = new Dictionary<string, string>();
            if(!itemName.StartsWith("Global_", StringComparison.CurrentCultureIgnoreCase) && File.Exists(csvPath))
            {
                File.Delete(csvPath);
            }
            if(!File.Exists(csvPath))
            {
                AddData("Point", "Time (ms)");
            }
            
            

        }

        public string GetCSVPath()
        {
            return _csvPath;
        }

        public bool AddData(string pointName, string value)
        {
            if(_itemName.StartsWith("Global_", StringComparison.CurrentCultureIgnoreCase))
            {
                pointName = string.Join("_", CommonExecutionInfo.CurrentRepeatCount.ToString(), pointName);
            }

            if (_aData.ContainsKey(pointName))
            {
                return false;
            }

            _aData[pointName] = value;
            _csvFile = new StreamWriter(_csvPath, true, Encoding.UTF8);
            _csvFile.WriteLine(string.Join(",", pointName, value));
            _csvFile.Flush();
            _csvFile.Close();

            return true;
        }

        public bool AddData(string pointName, List<string> values)
        {
            if (_aData.ContainsKey(pointName))
            {
                return false;
            }

            _aData[pointName] = values[0];
            _csvFile = new StreamWriter(_csvPath, true, Encoding.UTF8);
            _csvFile.WriteLine(string.Join(",", pointName, string.Join(",", values.ToArray())));
            _csvFile.Flush();
            _csvFile.Close();

            return true;
        }

        public void ResetStartTime()
        {
            _currentStartTime = DateTime.Now;
        }

        public bool AddTimeCheck(string pointName)
        {
            TimeSpan thisTime = DateTime.Now.Subtract(_currentStartTime);
            string elapsedTime = thisTime.TotalMilliseconds.ToString();
            return AddData(pointName, elapsedTime);

        }
        //loginfo methods
        public bool AddTimeCheck_LogInfo(string pointName)
        {
            DateTime endTime = DateTime.Now;
            TimeSpan elapsed = endTime - _currentStartTime;

            // Format times for logging
            string formattedStartTime = _currentStartTime.ToString("yyyy-MM-dd HH:mm:ss");
            string formattedEndTime = endTime.ToString("yyyy-MM-dd HH:mm:ss");
            string formattedElapsedTime = $"{(int)elapsed.TotalHours} h {(int)elapsed.TotalMinutes % 60} m {(int)elapsed.TotalSeconds % 60} s";
            string loopResult = CommonExecutionInfo.GetVariable("LoopResult");

            // Create a CSV format line
            string csvLine = $"{pointName},{formattedStartTime},{formattedEndTime},{formattedElapsedTime},{loopResult}";

            // Add data to the CSV
            return AddData(csvLine);
        }
        public bool AddData(string value)
        {
            using (var csvFile = new StreamWriter(_csvPath, true, Encoding.UTF8))
            {
                csvFile.WriteLine(value);
            }
            return true;
        }
        // New overloaded constructor
        public MeasureSet(string itemName, string csvPath, bool initializeCsv): this(itemName, csvPath) // Calls the existing constructor
        {
            if (initializeCsv)
            {
                InitializeCsvFile();
            }
        }

        private void InitializeCsvFile()
        {
            // Create the CSV file with headers if it doesn't exist
            if (!File.Exists(_csvPath))
            {
                using (var csvFile = new StreamWriter(_csvPath, false, Encoding.UTF8))
                {
                    csvFile.WriteLine("LoopNumber, StartTime, EndTime, Elapsed, Result");
                }
            }
        }

    }
}
