using GolemUI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace GolemUI.UI.Charts
{
    public class PrettyChartDataHistogram
    {
        public PrettyChartRawData? RawData { get; private set; }

        public PrettyChartData HistData { get; private set; }

        private TimeSpan _binTimeSize = TimeSpan.FromSeconds(10);
        DispatcherTimer _timeSpanUpdate = new DispatcherTimer();

        public void SetBinTimeSize(TimeSpan binTimeSize)
        {
            _binTimeSize = binTimeSize;
            HistData.Clear();
            RawDataToBinData();

            _timeSpanUpdate.Interval = binTimeSize;
            _timeSpanUpdate.Start();
            _timeSpanUpdate.Tick += _timeSpanUpdate_Tick;

            OnBinTimeSizeChanged?.Invoke();
        }

        private void _timeSpanUpdate_Tick(object sender, EventArgs e)
        {
            RawDataToBinData();
        }

        AggregateTypeEnum AggregateTypeEnum;

        public PrettyChartDataHistogram(AggregateTypeEnum aggregateTypeEnum = AggregateTypeEnum.Aggregate)
        {
            HistData = new PrettyChartData();
            AggregateTypeEnum = aggregateTypeEnum;

        }

        public delegate void BinTimeSizeChangedHandler();


        public BinTimeSizeChangedHandler? OnBinTimeSizeChanged { get; set; }


        public void SetRawData(PrettyChartRawData rawData)
        {
            rawData.OnRawEntryAdded += OnRawEntryAdded;
            RawData = rawData;
            RawDataToBinData();
        }

        public void OnRawEntryAdded(DateTime dt, double newValue)
        {
            RawDataToBinData();
        }

        void ResetBinData()
        {
            HistData.Clear();
            RawDataToBinData();
        }

        void RawDataToBinData()
        {
            var timespan = _binTimeSize;

            if (RawData != null && RawData.RawElements.Count > 0)
            {
                DateTime startTime = DateTimeUtils.RoundDown(RawData.RawElements.First().Dt, timespan);
                DateTime endTime = DateTimeUtils.RoundUp(DateTime.Now, timespan);

                PrettyChartRawData.RawEntry? firstInBin = null;
                PrettyChartRawData.RawEntry? lastInBin = null;

                PrettyChartRawData.RawEntry? previousLast = null;

                List<PrettyChartRawData.RawEntry> entries = RawData.RawElements;

                int idx = 0;
                int entry_no = 0;
                PrettyChartRawData.RawEntry val = entries[0];
                for (DateTime binDate = startTime; binDate < endTime; binDate += timespan)
                {
                    DateTime endBinDate = binDate + timespan;

                    while (idx < entries.Count)
                    {
                        val = entries[idx];
                        if (val.Dt < endBinDate)
                        {
                            if (firstInBin == null)
                            {
                                firstInBin = val;
                            }
                            lastInBin = val;
                        }
                        else
                        {
                            break;
                        }
                        idx += 1;
                    }
                    double earnings = 0.0;
                    if (lastInBin != null)
                    {
                        if (previousLast != null)
                        {
                            earnings = lastInBin.Value.Value - previousLast.Value.Value;
                        }
                        else
                        {
                            earnings = lastInBin.Value.Value;
                        }
                        previousLast = lastInBin;
                    }
                    string dateFormat = "yyyy-MM-dd";
                    if (timespan < TimeSpan.FromDays(1))
                    {
                        dateFormat = "dd HH:mm";
                    }
                    if (timespan < TimeSpan.FromHours(1))
                    {
                        dateFormat = "HH:mm";
                    }
                    if (timespan < TimeSpan.FromMinutes(1))
                    {
                        dateFormat = "HH:mm:ss";
                    }


                    HistData.AddOrUpdateBinEntry(entry_no, binDate.ToString(dateFormat), earnings * 1000.0);
                    lastInBin = null;
                    firstInBin = null;
                    entry_no += 1;
                }
            }
        }
    }
}
