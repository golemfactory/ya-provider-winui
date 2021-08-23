using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.UI.Charts
{

    public class PrettyChartRawData
    {
        public struct RawEntry
        {
            public DateTime Dt;
            public double Value;

            public RawEntry(DateTime dt, double value)
            {
                Dt = dt;
                Value = value;
            }
        };

        public delegate void RawEntryAddedHandler(DateTime dt, double newValue);

        public RawEntryAddedHandler? OnRawEntryAdded;

        public List<RawEntry> RawElements { get; private set; } = new List<RawEntry>();

        public PrettyChartRawData()
        {
        }

        public void AddNewEntry(DateTime dt, double value)
        {
            RawElements.Add(new RawEntry(dt, value));
            if (OnRawEntryAdded != null)
            {
                OnRawEntryAdded(dt, value);
            }
        }
    }
}
