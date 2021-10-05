using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.UI.Charts
{
    public enum AggregateTypeEnum
    {
        Aggregate,
        Average
    };

    public class PrettyChartData
    {
        public delegate void BinEntryUpdatedHandler(object sender, int binIdx, double oldValue, double newValue);
        public delegate void BinEntryAddedHandler(object sender, double newValue);
        public delegate void RedrawDataHandler(object sender);


        public BinEntryUpdatedHandler? OnBinEntryUpdated = null;
        public BinEntryAddedHandler? OnBinEntryAdded = null;
        public RedrawDataHandler? OnRedrawData = null;

        public string Suffix { get; set; } = "mGLM";

        public class PrettyChartBinEntry : ICloneable
        {
            public string? Label { get; set; } = null;
            public double Value { get; set; } = 0.0;

            public object Clone()
            {
                return new PrettyChartBinEntry { Label = Label, Value = Value };
            }
        }

        public class PrettyChartBinData
        {
            public List<PrettyChartBinEntry> BinEntries { get; set; } = new List<PrettyChartBinEntry>();

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

        public void RedrawData()
        {
            OnRedrawData?.Invoke(this);
        }

        public void AddOrUpdateBinEntry(int binEntryIndex, string label, double newValue, bool update)
        {
            if (!UpdateBinEntry(binEntryIndex, newValue, update))
            {
                AddBinEntry(label, newValue, update);
            }
        }


        private bool UpdateBinEntry(int binEntryIndex, double newValue, bool update)
        {
            if (binEntryIndex >= 0 && binEntryIndex < BinData.BinEntries.Count)
            {
                double oldValue = BinData.BinEntries[binEntryIndex].Value;
                BinData.BinEntries[binEntryIndex].Value = newValue;
                if (update)
                {
                    OnBinEntryUpdated?.Invoke(this, binEntryIndex, oldValue, newValue);
                }
                return true;
            }
            return false;
        }

        private void AddBinEntry(string label, double newValue, bool update)
        {
            BinData.BinEntries.Add(new PrettyChartBinEntry() { Label = label, Value = newValue });
            if (update)
            {
                OnBinEntryAdded?.Invoke(this, newValue);
            }
        }

        public PrettyChartBinData BinData { get; set; } = new PrettyChartBinData();
        public bool NoAnimate { get; set; } = false;

        public void Clear()
        {
            BinData.BinEntries.Clear();
        }
    }
}
