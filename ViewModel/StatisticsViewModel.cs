using GolemUI.Interfaces;
using GolemUI.Model;
using GolemUI.UI.Charts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace GolemUI.ViewModel
{
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

            var histogram = new PrettyChartDataHistogram();
            histogram.SetRawData(_historyDataProvider.EarningsChartData);
            ChartData1 = histogram;


            PropertyChanged += StatisticsViewModel_PropertyChanged;
        }

        private void StatisticsViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void HistoryDataProvider_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyChange("ChartData1");
        }

        public PrettyChartDataHistogram ChartData1 { get; set; }


        public event PageChangeRequestedEvent? PageChangeRequested;
        public event PropertyChangedEventHandler PropertyChanged;

        Random _rand = new Random();

        public void LoadData()
        {
            //ChartData2 = RandomData();
            /*            ChartData3 = RandomData();
                        ChartData4 = RandomData();

                        NotifyChange("ChartData1");
                        NotifyChange("ChartData2");
                        NotifyChange("ChartData3");
                        NotifyChange("ChartData4");*/
            //throw new NotImplementedException();
            ChartData1.Activate();
        }

        public void SaveData()
        {
            //throw new NotImplementedException();
        }

        //public PrettyChartData RandomData()
        // {
        /*
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
        */

        // }



        private void NotifyChange([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
