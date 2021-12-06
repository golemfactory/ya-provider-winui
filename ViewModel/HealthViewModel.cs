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
    public class HealthViewModel : INotifyPropertyChanged, ISavableLoadableDashboardPage
    {
        private readonly DispatcherTimer _timer;

        public HealthViewModel()
        {

        }

        private void HealthViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
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
