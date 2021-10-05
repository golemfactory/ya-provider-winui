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

    public class PrettyChartBinData
    {
        public delegate void BinEntryUpdatedHandler(object sender, int binIdx, double oldValue, double newValue);
        public delegate void BinEntryAddedHandler(object sender, double newValue);


        public BinEntryUpdatedHandler? OnBinEntryUpdated = null;
        public BinEntryAddedHandler? OnBinEntryAdded = null;

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

        public List<PrettyChartBinEntry> BinEntries { get; set; } = new List<PrettyChartBinEntry>();


        public void AddOrUpdateBinEntry(int binEntryIndex, string label, double newValue, bool notify)
        {
            if (!UpdateBinEntry(binEntryIndex, newValue, notify))
            {
                AddBinEntry(label, newValue, notify);
            }
        }


        private bool UpdateBinEntry(int binEntryIndex, double newValue, bool notify)
        {
            if (binEntryIndex >= 0 && binEntryIndex < BinEntries.Count)
            {
                double oldValue = BinEntries[binEntryIndex].Value;
                BinEntries[binEntryIndex].Value = newValue;
                if (notify)
                {
                    OnBinEntryUpdated?.Invoke(this, binEntryIndex, oldValue, newValue);
                }
                return true;
            }
            return false;
        }

        private void AddBinEntry(string label, double newValue, bool notify)
        {
            BinEntries.Add(new PrettyChartBinEntry() { Label = label, Value = newValue });
            if (notify)
            {
                OnBinEntryAdded?.Invoke(this, newValue);
            }
        }

        public void Clear()
        {
            BinEntries.Clear();
        }
    }
}
