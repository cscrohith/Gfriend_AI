using System;
using HP.GFriend.GFLogger;
using HP.GFriend.Keywords.Event;
using HP.GFriend.Utils.Charter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using System.Linq;
using System.Drawing.Imaging;
using System.Text;

namespace HP.GFriend.Keywords
{
    public class LogInfo : IGFLibrary
    {
        private static Dictionary<string, MeasureSet> _measureSets;
        private static string _outputDir;
        public void Dispose()
        {
            _measureSets = null;
        }

        public string GetName()
        {
            return "LogInfo";
        }

        public List<string> GetDependencies()
        {
            return null;
        }
        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
            _outputDir = outputDir;
            _measureSets = new Dictionary<string, MeasureSet>();
        }
        public bool DutUsed()
        {
            return false;
        }
        [KeywordDescription("Start new set of time checker")]
        [KeywordDisplayName("Start Time Check")]
        [KeywordParameters("measureSet", "name of sets of check point.")]
        [SampleScript("LogInfo.Start Time Check (LoginTime)")]
        public KeywordResult StartTimeCheck(string measureSet)
        {
            string csvPath = Path.Combine(_outputDir, $"{measureSet}.csv");

            if (_measureSets.ContainsKey(measureSet))
            {
                _measureSets.Remove(measureSet);
            }

            MeasureSet aItem = new MeasureSet(measureSet, csvPath);
            _measureSets[measureSet] = aItem;

            return new KeywordResult(KeywordResults.Pass)
            {
                Output = $"Time check is started and output will be saved to {csvPath}"
            };
        }
        [KeywordDescription("Start time with given check point name in measure set.")]
        [KeywordDisplayName("Start Time")]
        [KeywordParameters("measureSet", "measure set to save check point")]
        [KeywordParameters("checkPoint", "checkpoint name to save time data")]
        [SampleScript("LogInfo.Start Time (LoginTime,${Sample})")]
        public KeywordResult StartTime(string measureSet, string checkPoint)
        {
            if (!_measureSets.ContainsKey(measureSet))
            {
                return new KeywordResult(KeywordResults.Error)
                {
                    Output = $"Cannot find Measure set with name {measureSet}"
                };
            }
            _measureSets[measureSet].ResetStartTime();
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("End time with given check point name in measure set")]
        [KeywordDisplayName("End Time")]
        [KeywordParameters("measureSet", "measure set to save check point")]
        [KeywordParameters("checkPoint", "checkpoint name to save time data")]
        [SampleScript("LogInfo.End Time (LoginTime,${Sample1})")]
        public KeywordResult EndTime(string measureSet, string checkPoint)
        {
            if (!_measureSets.ContainsKey(measureSet))
            {
                return new KeywordResult(KeywordResults.Error)
                {
                    Output = $"Cannot find Measure set with name {measureSet}"
                };
            }

            // Log the timing with the modified checkpoint name
            _measureSets[measureSet].AddTimeCheck_LogInfo(checkPoint.Trim());
            return new KeywordResult(KeywordResults.Pass);
        }
        [KeywordDescription("Draw column chart : loops with elapsed time with results in color . Pie chart : shows the result percentage in the chart")]
        [KeywordDisplayName("Draw Charts")]
        [KeywordParameters("measureSet", "Measure set to draw chart")]
        [SampleScript("LoginInfo.DrawCharts(LoginTime)")]
        public KeywordResult DrawCharts(string measureSet)
        {
            try
            {
                // Load CSV
                string csvPath = _measureSets[measureSet].GetCSVPath();
                var lines = File.ReadAllLines(csvPath);

                // Extract LoopNumber, Elapsed time, and Result columns
                List<string> loopNumbers = new List<string>();
                List<int> elapsedTimes = new List<int>();
                Dictionary<string, int> resultCounts = new Dictionary<string, int>();

                for (int i = 1; i < lines.Length; i++) // Skip header row
                {
                    var values = lines[i].Split(',');
                    loopNumbers.Add(values[0]); // Assuming LoopNumber is the first column
                    string elapsed = values[3]; // Assuming Elapsed time is in 4th column
                    string Result = values[4]; // Assuming Result is in 5th column

                    // Convert "0 h 0 m 3 s" to seconds
                    var timeParts = elapsed.Split(new[] { ' ', 'h', 'm', 's' }, StringSplitOptions.RemoveEmptyEntries);
                    int seconds = (int.Parse(timeParts[0]) * 3600) + (int.Parse(timeParts[1]) * 60) + int.Parse(timeParts[2]);
                    elapsedTimes.Add(seconds);

                    // Count results
                    Result = Result.Trim().ToUpper();
                    if (resultCounts.ContainsKey(Result))
                    {
                        resultCounts[Result]++;
                    }
                    else
                    {
                        resultCounts[Result] = 1;
                    }
                }

                // Create column chart for elapsed time
                Chart columnChart = new Chart();
                columnChart.Series.Clear();
                columnChart.ChartAreas.Clear();
                columnChart.ChartAreas.Add(new ChartArea("ColumnChartArea"));
                Series columnSeries = new Series("ElapsedTime")
                {
                    ChartType = SeriesChartType.Column
                };
                columnChart.Series.Add(columnSeries);
                columnChart.ChartAreas[0].AxisX.MajorGrid.Enabled = false; // Disable grid lines on the X-axis
                columnChart.ChartAreas[0].AxisY.MajorGrid.Enabled = false; // Disable grid lines on the Y-axis
                columnChart.ChartAreas[0].AxisX.Title = "Iterations";
                columnChart.ChartAreas[0].AxisY.Title = "Time taken(seconds)";

                // Add data points to column chart and set colors
                for (int i = 0; i < loopNumbers.Count; i++)
                {
                    string Result = lines[i + 1].Split(',')[4].Trim().ToUpper(); // Get result for this loop
                    columnSeries.Points.AddXY(loopNumbers[i], elapsedTimes[i]);
                    columnSeries.Points[i].Color = GetResultColor(Result);
                }

                // Save column chart as image
                string columnChartImage = Path.Combine(_outputDir, measureSet + "_ColumnChart.png");
                columnChart.SaveImage(columnChartImage, ChartImageFormat.Png);

                // Create pie chart for result percentages
                Chart pieChart = new Chart();
                pieChart.Series.Clear();
                pieChart.ChartAreas.Clear();
                pieChart.ChartAreas.Add(new ChartArea("PieChartArea"));
                pieChart.Titles.Add("Percenatge of Results");
                Series pieSeries = new Series("Results")
                {
                    ChartType = SeriesChartType.Pie
                };
                pieChart.Series.Add(pieSeries);
                Legend legend = new Legend
                {
                    Docking = Docking.Right,
                    Alignment = StringAlignment.Center
                };
                pieChart.Legends.Add(legend);

                // Calculate total results
                int totalResults = resultCounts.Values.Sum();
                foreach (var Result in resultCounts)
                {
                    double percentage = (double)Result.Value / totalResults * 100;
                    string label = $"{Result.Key} ({percentage:F1}%)"; // Prepare label
                    pieSeries.Points.AddXY(label, percentage); // Add the label as the point name
                    // Show percentage inside the slice
                    pieSeries.Points[pieSeries.Points.Count - 1].Label = $"{percentage:F1}%";
                   pieSeries.Points[pieSeries.Points.Count - 1].Font = new Font("Arial",6 , FontStyle.Regular);

                    pieSeries.Points[pieSeries.Points.Count - 1].LegendText = $"{label} {percentage:F1}%";

                    // Set the color for each slice
                    pieSeries.Points[pieSeries.Points.Count - 1].Color = GetResultColor(Result.Key);
                }

                // Save pie chart as image
                string pieChartImage = Path.Combine(_outputDir, measureSet + "_PieChart.png");
                pieChart.SaveImage(pieChartImage, ChartImageFormat.Png);

                // Copy to parent directory if required
                if (CommonExecutionInfo.CurrentRepeatCount > 0)
                {
                    File.Copy(columnChartImage, Path.Combine(Directory.GetParent(_outputDir).FullName, measureSet + "_ColumnChart.png"), true);
                    File.Copy(pieChartImage, Path.Combine(Directory.GetParent(_outputDir).FullName, measureSet + "_PieChart.png"), true);
                }

                // Return result
                KeywordResult result = new KeywordResult(KeywordResults.Pass);
                result.ScreenShot = CombineImages(columnChartImage, pieChartImage);
                return result;
            }
            catch (Exception ex)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Failed to create charts";
                fail.AdditionalInfo = ex.ToString();
                return fail;
            }
        }

        // Helper method to get color based on result
        private Color GetResultColor(string result)
        {
            switch (result)
            {
                case "PASS":
                    return Color.Green;
                case "FAIL":
                    return Color.Red;
                case "ERROR":
                    return Color.Orange;
                default:
                    return Color.Gray; // Default color for unknown results
            }
        }
        public byte[] CombineImages(string columnChartImage, string pieChartImage)
        {
            var img1 = Image.FromFile(columnChartImage);
            var img2 = Image.FromFile(pieChartImage);

            var combinedWidth = img1.Width + img2.Width;
            var combinedHeight = Math.Max(img1.Height, img2.Height);

            var combinedImg = new Bitmap(combinedWidth, combinedHeight);
            var g = Graphics.FromImage(combinedImg);

            g.DrawImage(img1, 0, 0);
            g.DrawImage(img2, img1.Width, 0);

            var ms = new MemoryStream();
            combinedImg.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }
    }
}
