using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
        public delegate void RawEntriesChangedHandler();

        public RawEntryAddedHandler? OnRawEntryAdded;
        public RawEntriesChangedHandler? OnRawEntriesChanged;

        public List<RawEntry> RawElements { get; private set; } = new List<RawEntry>();

        public void TrimData(DateTime removeBeforeDate, ILogger? logger, bool notify = false)
        {
            if (RawElements.Count > 0)
            {
                try
                {
                    int idx = RawElements.FindIndex(x => x.Dt >= removeBeforeDate);
                    if (idx > 0)
                    {
                        RawElements = RawElements.GetRange(idx, RawElements.Count - idx);
                        if (notify && OnRawEntriesChanged != null)
                        {
                            OnRawEntriesChanged();
                        }
                    }
                    else if (idx == 0)
                    {
                        //nothing changes
                    }
                    else
                    {
                        RawElements.Clear();
                        if (notify && OnRawEntriesChanged != null)
                        {
                            OnRawEntriesChanged();
                        }
                    }
                }
                catch (ArgumentNullException e)
                {
                    logger?.LogError("ArgumentNullException when trimming history: " + e.Message);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    logger?.LogError("ArgumentOutOfRangeException when trimming history: " + e.Message);
                }
                catch (ArgumentException e)
                {
                    logger?.LogError("ArgumentException when trimming history: " + e.Message);
                }
            }
        }

        public PrettyChartRawData()
        {
        }

        public void AddNewEntry(DateTime dt, double value, bool notify = false)
        {
            RawElements.Add(new RawEntry(dt, value));
            if (notify && OnRawEntryAdded != null)
            {
                OnRawEntryAdded(dt, value);
            }
        }
    }
}
