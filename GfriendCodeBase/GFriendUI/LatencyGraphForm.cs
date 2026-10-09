using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Linq;
using System;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;

namespace HP.GFriend.UI
{
    public partial class LatencyGraphForm : Form
    {
        private System.Windows.Forms.DataVisualization.Charting.Chart latencyChart;
        private Timer refreshTimer;
        private string csvFilePath;
        private bool isTestCaseRunning;

        public LatencyGraphForm(string logFilePath, bool isTCRunning)
        {
            InitializeComponent();
            csvFilePath = logFilePath;
            isTestCaseRunning = isTCRunning;
            this.Text = "Live Latency Graph";
            this.Size = new Size(800, 400);

            if (isTCRunning)
            {
                InitializeChart();
                InitializeTimer();
            }
            else
            {
                InitializeChart();
            }
        }

        private void InitializeChart()
        {
            latencyChart = new System.Windows.Forms.DataVisualization.Charting.Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            var chartArea = new System.Windows.Forms.DataVisualization.Charting.ChartArea("MainArea");
            chartArea.AxisX.LabelStyle.Format = "HH:mm:ss";
            chartArea.AxisX.Title = "Timestamp";
            chartArea.AxisY.Title = "Latency (ms)";
            chartArea.AxisY.Minimum = -10; // ensure -1 values are visible

            // Add threshold lines
            var warnLine100 = new StripLine
            {
                IntervalOffset = 100,
                BorderColor = Color.Orange,
                BorderDashStyle = ChartDashStyle.Dash,
                BorderWidth = 2,
                Text = "Warning (100ms)",
                TextAlignment = StringAlignment.Near,
                TextLineAlignment = StringAlignment.Far,
                Font = new Font("Arial", 8, FontStyle.Italic),
                ForeColor = Color.Orange
            };
            chartArea.AxisY.StripLines.Add(warnLine100);

            var criticalLine200 = new StripLine
            {
                IntervalOffset = 200,
                BorderColor = Color.Red,
                BorderDashStyle = ChartDashStyle.Dash,
                BorderWidth = 2,
                Text = "Critical (200ms)",
                TextAlignment = StringAlignment.Near,
                TextLineAlignment = StringAlignment.Far,
                Font = new Font("Arial", 8, FontStyle.Italic),
                ForeColor = Color.Red
            };
            chartArea.AxisY.StripLines.Add(criticalLine200);

            latencyChart.ChartAreas.Add(chartArea);

            var series = new System.Windows.Forms.DataVisualization.Charting.Series("Latency")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.Green,
                BorderWidth = 2,
                XValueType = ChartValueType.DateTime
            };

            latencyChart.Series.Add(series);
            this.Controls.Add(latencyChart);
        }

        private void InitializeTimer()
        {
            refreshTimer = new Timer
            {
                Interval = 1000 // every 1 second
            };
            refreshTimer.Tick += (s, e) => RefreshChart();
            refreshTimer.Start();
        }

        private void RefreshChart()
        {
            if (!File.Exists(csvFilePath)) return;
            if (latencyChart?.Series["Latency"] == null) return;

            var lines = File.ReadAllLines(csvFilePath)
                            .Skip(1)
                            .Where(line => !string.IsNullOrWhiteSpace(line))
                            .Select(line => line.Split(','))
                            .Where(parts => parts.Length >= 4);

            var points = lines
                .Select(parts =>
                {
                    if (DateTime.TryParse(parts[0], out DateTime timestamp) &&
                        long.TryParse(parts[3], out long latency))
                    {
                        return new { Timestamp = timestamp, Latency = latency };
                    }
                    return null;
                })
                .Where(x => x != null)
                .OrderBy(x => x.Timestamp)
                .ToList();

            var series = latencyChart.Series["Latency"];
            series.Points.Clear();

            foreach (var point in points)
            {
                int index = series.Points.AddXY(point.Timestamp, point.Latency);
                var chartPoint = series.Points[index];

                if (point.Latency == -1)
                {
                    chartPoint.Color = Color.Red;
                    chartPoint.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Cross;
                    chartPoint.MarkerSize = 8;
                    chartPoint.MarkerColor = Color.Red;
                    chartPoint.ToolTip = "Unreachable (-1)";
                }
                else
                {
                    chartPoint.ToolTip = $"{point.Latency} ms";

                    if (point.Latency >= 200)
                        chartPoint.Color = Color.Red;
                    else if (point.Latency >= 100)
                        chartPoint.Color = Color.Orange;
                    else
                        chartPoint.Color = Color.Green;
                }
            }

            latencyChart.ChartAreas[0].RecalculateAxesScale();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            refreshTimer?.Stop();
            refreshTimer?.Dispose();
            refreshTimer = null;
            try
            {
                if (isTestCaseRunning)
                {
                    string imagePath = Path.Combine(
                        Path.GetDirectoryName(csvFilePath),
                        $"LatencyGraph_{DateTime.Now:yyyyMMdd_HHmmss}.png");

                    latencyChart.SaveImage(imagePath, ChartImageFormat.Png);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save chart: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            base.OnFormClosing(e);
        }
    }
}