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
            var chartData = new PrettyChartData()
            {
                BinData = new PrettyChartBinData()
                {
                    BinEntries = new List<PrettyChartBinEntry>() {
                            new PrettyChartBinEntry(){Label="Piąta", Value=5.5},
                            new PrettyChartBinEntry(){Label="Szósta", Value=2.5},
                            new PrettyChartBinEntry(){Label="Siódma", Value=6.5},
                            new PrettyChartBinEntry(){Label="Piąta", Value=5.5},
                            new PrettyChartBinEntry(){Label="Szósta", Value=2.5},
                            new PrettyChartBinEntry(){Label="Siódma", Value=6.5},
                            new PrettyChartBinEntry(){Label="Piąta", Value=5.5},
                            new PrettyChartBinEntry(){Label="Szósta", Value=2.5},
                            new PrettyChartBinEntry(){Label="Siódma", Value=6.5},
                            new PrettyChartBinEntry(){Label="Piąta", Value=5.5},
                            new PrettyChartBinEntry(){Label="Szósta", Value=2.5},
                            new PrettyChartBinEntry(){Label="Siódma", Value=6.5},
                            new PrettyChartBinEntry(){Label="Piąta", Value=5.5},
                            new PrettyChartBinEntry(){Label="Szósta", Value=2.5},
                            new PrettyChartBinEntry(){Label="Siódma", Value=6.5},
                            new PrettyChartBinEntry(){Label="Siódma", Value=6.5},
                            new PrettyChartBinEntry(){Label="Siódma", Value=2.5},
                            new PrettyChartBinEntry(){Label="Siódma", Value=1.5},
                            new PrettyChartBinEntry(){Label="Siódma", Value=0.5},
                            new PrettyChartBinEntry(){Label="Siódma", Value=6.5},
                            new PrettyChartBinEntry(){Label="Siódma", Value=4.5},
                            new PrettyChartBinEntry(){Label="Siódma", Value=3.5},
                            new PrettyChartBinEntry(){Label="Siódma", Value=6.5},
                            new PrettyChartBinEntry(){Label="Siódma", Value=6.5},
                        }
                }
            };

            var rand = new Random();
            var binData = chartData.BinData;
            foreach (var bin in binData.BinEntries)
            {
                bin.Value = rand.NextDouble() * 30;
            }

            chartData.BinData = binData;

            ChartData = chartData;


            NotifyChange("ChartData");
        }

        private void NotifyChange([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
