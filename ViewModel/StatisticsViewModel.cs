using GolemUI.Interfaces;
using GolemUI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using static GolemUI.Model.PrettyChartData;

namespace GolemUI.ViewModel
{
#if STATISTICS_ENABLED
    public class StatisticsViewModel : INotifyPropertyChanged, ISavableLoadableDashboardPage
    {
        private readonly DispatcherTimer _timer;

        IHistoryDataProvider _historyDataProvider;
        public StatisticsViewModel(IHistoryDataProvider historyDataProvider)
        {
            _historyDataProvider = historyDataProvider;
            historyDataProvider.PropertyChanged += HistoryDataProvider_PropertyChanged;

            //get rid of null
            if (PageChangeRequested != null)
            {

            }
            _timer = new DispatcherTimer();
        }

        private void HistoryDataProvider_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ChartData1 = _historyDataProvider.GetMegaHashHistory();
            NotifyChange("ChartData1");
        }

        public PrettyChartData ChartData1 { get; set; }
        public PrettyChartData ChartData2 { get; set; }
        public PrettyChartData ChartData3 { get; set; }
        public PrettyChartData ChartData4 { get; set; }


        public event PageChangeRequestedEvent? PageChangeRequested;
        public event PropertyChangedEventHandler PropertyChanged;

        Random _rand = new Random();

        public void LoadData()
        {
            ChartData2 = RandomData();
            ChartData3 = RandomData();
            ChartData4 = RandomData();

            NotifyChange("ChartData1");
            NotifyChange("ChartData2");
            NotifyChange("ChartData3");
            NotifyChange("ChartData4");
            //throw new NotImplementedException();
        }

        public void SaveData()
        {
            //throw new NotImplementedException();
        }

        public PrettyChartData RandomData()
        {
            var chartData = new PrettyChartData();
            chartData.NoAnimate = false;

            var binData = chartData.BinData;
            for (int i = 0; i < 10 + _rand.Next(10); i++)
            {
                var bin = new PrettyChartBinEntry();
                bin.Value = _rand.NextDouble() * 30;
                bin.Label = $"{i}";

                chartData.BinData.BinEntries.Add(bin);
            }


            return chartData;


        }
        public PrettyChartData MoveDataRight(PrettyChartData cd)
        {
            var chartData = (PrettyChartData)cd.Clone();
            var binEntries = chartData.BinData.BinEntries;

            var firstElem = binEntries[0];
            for (int i = 0; i < binEntries.Count; i++)
            {
                if (i == binEntries.Count - 1)
                {
                    binEntries[i] = firstElem;
                }
                else
                {
                    binEntries[i] = binEntries[i + 1];
                }
            }

            return new PrettyChartData() { BinData = new PrettyChartBinData() { BinEntries = binEntries } };


        }

        public void MoveDataRight()
        {
            ChartData1 = MoveDataRight(ChartData1);
            ChartData2 = MoveDataRight(ChartData2);
            ChartData3 = MoveDataRight(ChartData3);
            ChartData4 = MoveDataRight(ChartData4);

            NotifyChange("ChartData1");
            NotifyChange("ChartData2");
            NotifyChange("ChartData3");
            NotifyChange("ChartData4");
        }

        private void NotifyChange([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
#endif
}
