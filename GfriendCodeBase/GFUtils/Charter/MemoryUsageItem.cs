using HP.GFriend.GFLogger;
using HP.GFriend.Keywords;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HP.GFriend.Utils.Charter
{
    public class MemoryUsageItem
    {
        public class Item
        {
            private Dictionary<string, string> _data;
            private string _itemName;

            public Item()
            {
                _data = new Dictionary<string, string>();
            }

            public void AddData(string procName, string MemUsage)
            {
                _data.Add(procName, MemUsage);
            }

            public string GetData(string procName)
            {
                if (_data.ContainsKey(procName))
                {
                    return _data[procName];
                }
                else
                {
                    return string.Empty;
                }
            }

            public void SetItemName(string itemName)
            {
                _itemName = itemName;
            }

            public string GetItemName()
            {
                return _itemName;
            }
        }

        private readonly object _csvLock = new object();
        private List<string> _procNames;
        private List<Item> _datas;
        private string _csvPath;
        private string _outputDir;
        private StreamWriter _csvFile;
        private bool _headerWritten;

        public MemoryUsageItem(string csvPath)
        {
            _csvPath = csvPath;
            _outputDir = Path.GetDirectoryName(_csvPath);
            if (CommonExecutionInfo.CurrentRepeatCount <= 0 && File.Exists(csvPath))
            {
                Logger.Trace("Delete CSV file");
                //File.Delete(csvPath);
            }
            if (CommonExecutionInfo.CurrentRepeatCount < 2)
            {
                _procNames = new List<string>();
                _datas = new List<Item>();
            }

            _headerWritten = File.Exists(csvPath) && new FileInfo(csvPath).Length > 0;
        }


        public string GetCSVPath()
        {
            return _csvPath;
        }

        public void AddData(string pointName, Dictionary<string, long> memUsages)
        {
            lock (_csvLock)
            {
                Item aItem = new Item();
                aItem.SetItemName(pointName);
                foreach (KeyValuePair<string, long> memUsage in memUsages)
                {
                    if (!_procNames.Contains(memUsage.Key))
                    {
                        _procNames.Add(memUsage.Key);
                    }
                    aItem.AddData(memUsage.Key, memUsage.Value.ToString());
                }

                _datas.Add(aItem);

                WriteToCSV();
            }
        }

        public void DataAppend(Dictionary<string, object> memUsages)
        {
            lock (_csvLock)
            {
                Item aItem = new Item();
                bool headerChanged = false;

                foreach (var memUsage in memUsages)
                {
                    if (!_procNames.Contains(memUsage.Key))
                    {
                        _procNames.Add(memUsage.Key);
                        headerChanged = true;
                    }

                    aItem.AddData(memUsage.Key, memUsage.Value?.ToString());
                }

                _datas.Add(aItem);

                // If header changed, rewrite entire file; otherwise append
                if (headerChanged)
                {
                    WriteToCSV();
                }
                else
                {
                    AppendRowToCSV(aItem);
                }
            }
        }

        private void AppendRowToCSV(Item data)
        {
            Logger.Trace("Append data row to csv file");

            try
            {
                using (StreamWriter writer = new StreamWriter(_csvPath, true))
                {
                    // Write header if this is the first write
                    if (!_headerWritten)
                    {
                        string header = string.Join(",", _procNames);
                        writer.WriteLine(header);
                        _headerWritten = true;
                    }

                    // Write data row
                    List<string> row = new List<string>();
                    foreach (string procName in _procNames)
                    {
                        row.Add(data.GetData(procName));
                    }
                    writer.WriteLine(string.Join(",", row));
                }
            }
            catch (IOException ex)
            {
                Logger.Error($"Failed to append to CSV file: {ex.Message}");
            }
        }

        public void WriteToCSV()
        {
            Logger.Trace("Write data to csv file");

            using (_csvFile = new StreamWriter(_csvPath))
            {
                // Header
                string output = string.Join(",", _procNames);
                _csvFile.WriteLine(output);

                // Data
                foreach (Item data in _datas)
                {
                    List<string> row = new List<string>();

                    foreach (string procName in _procNames)
                    {
                        row.Add(data.GetData(procName));
                    }

                    _csvFile.WriteLine(string.Join(",", row));
                }
            }

            _headerWritten = true;
        }

    }
}
