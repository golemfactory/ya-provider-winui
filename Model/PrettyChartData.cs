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
        public delegate void BinEntryUpdatedHandler(object sender, int binIdx, double oldValue, double newValue);
        public delegate void BinEntryAddedHandler(object sender, double newValue);

        public BinEntryUpdatedHandler? OnBinEntryUpdated = null;
        public BinEntryAddedHandler? OnBinEntryAdded = null;

        public string Suffix { get; set; } = "MH/s";

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


        public void AddOrUpdateBinEntry(int binEntryIndex, string label, double newValue)
        {
            if (!UpdateBinEntry(binEntryIndex, newValue))
            {
                AddBinEntry(label, newValue);
            }
        }

        private bool UpdateBinEntry(int binEntryIndex, double newValue)
        {
            if (binEntryIndex >= 0 && binEntryIndex < BinData.BinEntries.Count)
            {
                double oldValue = BinData.BinEntries[binEntryIndex].Value;
                BinData.BinEntries[binEntryIndex].Value = newValue;
                OnBinEntryUpdated?.Invoke(this, binEntryIndex, oldValue, newValue);
                return true;
            }
            return false;
        }
        private void AddBinEntry(string label, double newValue)
        {
            BinData.BinEntries.Add(new PrettyChartBinEntry() { Label = label, Value = newValue });
            OnBinEntryAdded?.Invoke(this, newValue);
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
