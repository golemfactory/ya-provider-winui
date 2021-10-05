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

        public PrettyChartBinData BinData { get; private set; }

        private TimeSpan _binTimeSize = TimeSpan.FromMinutes(10);
        public TimeSpan BinTimeSpan
        {
            get => _binTimeSize;
        }
        DispatcherTimer _timeSpanUpdate = new DispatcherTimer();

        bool _active;

        public void SetBinTimeSize(TimeSpan binTimeSize, bool update)
        {
            _binTimeSize = binTimeSize;
            if (_active)
            {
                BinData.Clear();
                RawDataToBinData(false);

                _timeSpanUpdate.Interval = binTimeSize;
                _timeSpanUpdate.Start();
                _timeSpanUpdate.Tick += _timeSpanUpdate_Tick;

                OnBinTimeSizeChanged?.Invoke();
            }
        }

        private void _timeSpanUpdate_Tick(object sender, EventArgs e)
        {
            if (_active)
            {
                RawDataToBinData(true);
            }
        }

        private readonly AggregateTypeEnum _aggregateTypeEnum;

        public PrettyChartDataHistogram(AggregateTypeEnum aggregateTypeEnum = AggregateTypeEnum.Aggregate)
        {
            BinData = new PrettyChartBinData();
            _aggregateTypeEnum = aggregateTypeEnum;
        }

        public void Clear()
        {
            BinData.Clear();
            RawData = new PrettyChartRawData();
            _active = false;
        }

        public delegate void BinTimeSizeChangedHandler();
        public delegate void RedrawDataHandler();

        public RedrawDataHandler? OnRedrawData = null;


        public BinTimeSizeChangedHandler? OnBinTimeSizeChanged { get; set; }


        public void SetRawData(PrettyChartRawData rawData)
        {
            rawData.OnRawEntryAdded += OnRawEntryAdded;
            rawData.OnRawEntriesChanged += OnRawEntriesChanged;
            RawData = rawData;
            //RawDataToBinData();
        }
        public void Activate()
        {
            _active = true;
            RawDataToBinData(false);
            OnRedrawData?.Invoke();
        }

        public void OnRawEntryAdded(DateTime dt, double newValue)
        {
            if (_active)
            {
                RawDataToBinData(true);
            }
        }

        public void OnRawEntriesChanged()
        {
            ResetBinData();
        }

        void ResetBinData()
        {
            if (_active)
            {
                BinData.Clear();
                RawDataToBinData(false);
                OnRedrawData?.Invoke();
            }
        }

        void RawDataToBinData(bool notify)
        {
            var timespan = _binTimeSize;

            if (RawData != null && RawData.RawElements.Count > 0)
            {
                DateTime startTime = DateTimeUtils.RoundDown(RawData.RawElements.First().Dt, timespan);
                DateTime endTime = DateTimeUtils.RoundUp(DateTime.Now, timespan);

                PrettyChartRawData.RawEntry? firstInBin = null;
                //PrettyChartRawData.RawEntry? lastInBin = null; //may be used later if algorithm changed

                List<PrettyChartRawData.RawEntry> entries = RawData.RawElements;

                int idx = 0;
                int entryNo = 0;
                PrettyChartRawData.RawEntry val = entries[0];
                for (DateTime binDate = startTime; binDate < endTime; binDate += timespan)
                {
                    DateTime endBinDate = binDate + timespan;

                    double sumValues = 0.0;
                    while (idx < entries.Count)
                    {
                        val = entries[idx];
                        if (val.Dt < endBinDate)
                        {
                            sumValues += val.Value;
                            if (firstInBin == null)
                            {
                                firstInBin = val;
                            }
                            //lastInBin = val;
                        }
                        else
                        {
                            break;
                        }
                        idx += 1;
                    }
                    double earnings = sumValues;

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

                    BinData.AddOrUpdateBinEntry(entryNo, binDate.ToString(dateFormat), earnings * 1000.0, notify);
                    //lastInBin = null;
                    firstInBin = null;
                    entryNo += 1;
                }
            }
            else
            {
                BinData.Clear();
            }
        }
    }
}
