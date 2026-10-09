using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;


namespace HP.GFriend.Utils.Charter
{
    internal class SeriesCreator
    {
        private CsvReader _reader;
        private List<List<PointSD>> _pointLists;
        public Legend Legend { get; set; }

        public SeriesCreator(CsvReader reader)
        {
            this._reader = reader;
            Legend = new Legend();
        }

        private string StripDoubleQuotes(string str)
        {
            int startIndex = 0;
            int lastIndex = str.Length - 1;

            if (str[0] == '"')
            {
                startIndex++;
            }
            if (str[lastIndex] == '"')
            {
                lastIndex--;
            }
            string returnString = str.Substring(startIndex, (lastIndex - startIndex) + 1);
            return returnString;
        }

        private IEnumerable<Series> AddPoints(List<Series> serieses)
        {
            foreach (List<string> tokens in _reader)
            {
                for (int i = 1; i < tokens.Count; i++)
                {
                    if (string.IsNullOrEmpty(tokens[i]))
                    {
                        tokens[i] = "0";
                    }
                    
                    PointSD p = new PointSD(tokens[0], double.Parse(StripDoubleQuotes(tokens[i])));
                    _pointLists[i - 1].Add(p);
                    serieses[i - 1].Points.AddXY(p.X, p.Y);
                }
            }

            return serieses;
        }

        public IEnumerable<Series> ToSerieses(SeriesChartType chartType)
        {
            List<Series> serieses = new List<Series>();
            _pointLists = new List<List<PointSD>>();

            for (int i = 1; i < _reader.Header.Count; i++)
            {
                _pointLists.Add(new List<PointSD>());
                Series s = new Series()
                {
                    Name = StripDoubleQuotes(_reader.Header[i]),
                    BorderWidth = 3,
                    IsVisibleInLegend = true,
                    IsXValueIndexed = true,
                    ChartType = chartType
                };
                serieses.Add(s);
            }

            return AddPoints(serieses);
        }

        public double GetMinimumY()
        {
            double maxY = _pointLists.SelectMany(l => l.Select(p => p.Y)).Max();
            double minY = _pointLists.SelectMany(l => l.Select(p => p.Y)).Min();
            double range = Math.Abs(maxY - minY);

            if (range <= 1)
            {
                return minY - 0.001;
            }

            return Math.Floor(minY - (range) * 0.001);
        }

        public double GetMaximumY()
        {
            double maxY = _pointLists.SelectMany(l => l.Select(p => p.Y)).Max();
            double minY = _pointLists.SelectMany(l => l.Select(p => p.Y)).Min();
            double range = Math.Abs(maxY - minY);

            if (range <= 1)
            {
                return maxY + 0.001;
            }

            return Math.Floor(maxY - (range) * 0.001);
        }
    }
}