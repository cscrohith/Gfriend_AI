using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;

namespace HP.GFriend.Utils.Charter
{
    public class ChartCreator
    {
        private string _filePath;


        public ChartCreator(string filePath)
        {
            _filePath = filePath;
        }

        public void SaveImage(string saveFileName, SeriesChartType chartType, bool autoYXis = false, string drawOnly = null)
        {
            SeriesCreator creator = new SeriesCreator(CsvReader.Builder.CreateInstance(_filePath));
            GenerateChart(creator, _filePath, saveFileName, chartType, autoYXis, drawOnly);
        }

        private void GenerateChart(SeriesCreator creator, string filePath, string saveFileName, SeriesChartType chartType, bool autoYXis = false, string drawOnly = null)
        {
            IEnumerable<Series> serieses = creator.ToSerieses(chartType);

            using (var ch = new Chart())
            {
                ch.Size = new Size(1300, 800);
                ch.AntiAliasing = AntiAliasingStyles.All;
                ch.TextAntiAliasingQuality = TextAntiAliasingQuality.High;
                ch.Palette = ChartColorPalette.BrightPastel;


                ChartArea area = new ChartArea();
                area.AxisX.MajorGrid.Enabled = false;
                area.AxisY.MajorGrid.Enabled = false;


                if (autoYXis)
                {
                    area.AxisY.Minimum = creator.GetMinimumY();
                }
                else
                {
                    area.AxisY.Minimum = 0;
                }


                area.AxisX.Interval = 1;
                area.AxisX.LabelStyle.Angle = -45;

                ch.ChartAreas.Add(area);

                Legend legend = new Legend();
                legend.Font = new Font("Microsoft Sans Serif", 12, FontStyle.Regular);
                ch.Legends.Add(legend);

                foreach (var s in serieses)
                {
                    // Check if any point in the series has a value greater than zero
                    bool hasPositiveValue = s.Points.Any(p => p.YValues[0] > 0);
                    if (hasPositiveValue)
                    {
                        if (drawOnly == null)
                        {
                            ch.Series.Add(s);
                            if (area.AxisX.Maximum < s.Points.Count)
                            {
                                area.AxisX.Maximum = s.Points.Count;
                            }
                        }
                        else
                        {
                            if (drawOnly.Equals(s.Name))
                            {
                                ch.Series.Add(s);
                                if (autoYXis)
                                {
                                    area.AxisY.Minimum = s.Points.FindMinByValue().YValues[0];
                                }

                                if (area.AxisX.Maximum < s.Points.Count)
                                {
                                    area.AxisX.Maximum = s.Points.Count;
                                }
                            }
                        }

                    }
                }
                ch.SaveImage(saveFileName, ChartImageFormat.Png);
            }
        }
    }
}