using GolemUI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static GolemUI.Model.PrettyChartData;

namespace GolemUI.ViewModel
{
    public class StatisticsViewModel : INotifyPropertyChanged, ISavableLoadableDashboardPage
    {
        public StatisticsViewModel()
        {

        }

        public PrettyChartData ChartData { get; set; }
        

        public event PageChangeRequestedEvent PageChangeRequested;
        public event PropertyChangedEventHandler PropertyChanged;

        public void LoadData()
        {
            //throw new NotImplementedException();
        }

        public void SaveData()
        {
            //throw new NotImplementedException();
        }

        public void RandomData()
        {
            var chartData = new PrettyChartData();
            chartData.NoAnimate = false;

            var rand = new Random();
            var binData = chartData.BinData;
            for (int i = 0; i < 10 + rand.Next(10); i++)
            {
                var bin = new PrettyChartBinEntry();
                bin.Value = rand.NextDouble() * 30;
                bin.Label = $"{i}";

                chartData.BinData.BinEntries.Add(bin);
            }


            ChartData = chartData;


            NotifyChange("ChartData");
        }
        public void MoveDataRight()
        {
            var chartData = (PrettyChartData)ChartData.Clone();
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

            ChartData = new PrettyChartData() { BinData = new PrettyChartBinData() { BinEntries = binEntries } };


            NotifyChange("ChartData");
        }


        private void NotifyChange([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
