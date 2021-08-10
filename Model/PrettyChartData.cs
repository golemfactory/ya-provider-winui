using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Model
{
    static class Extensions
    {
        public static List<T> Clone<T>(this List<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
    }


    public class PrettyChartData : ICloneable
    {


        public class PrettyChartBinEntry : ICloneable
        {
            public string? Label { get; set; } = null;
            public double Value { get; set; } = 0.0;

            public object Clone()
            {
                return new PrettyChartBinEntry { Label = Label, Value = Value };
            }
        }

        public class PrettyChartBinData : ICloneable
        {
            public List<PrettyChartBinEntry> BinEntries { get; set; } = new List<PrettyChartBinEntry>();

            public object Clone()
            {
                return new PrettyChartBinData() { BinEntries = BinEntries.Clone() };

            }

            public double GetMaxValue(double minValue)
            {
                double maxValue = minValue;
                foreach (var entry in BinEntries)
                {
                    if (entry.Value > maxValue)
                    {
                        maxValue = entry.Value;
                    }
                }
                return maxValue;
            }

        }

        public PrettyChartBinData BinData { get; set; } = new PrettyChartBinData();
        public bool NoAnimate { get; set; } = false;


        public void Clear()
        {
            BinData.BinEntries.Clear();
        }

        public PrettyChartData()
        {
            //BinData = new PrettyChartBinData();


        }

        public object Clone()
        {
            return new PrettyChartData() { BinData = (PrettyChartBinData)BinData.Clone(), NoAnimate = NoAnimate };

        }
    }
}
