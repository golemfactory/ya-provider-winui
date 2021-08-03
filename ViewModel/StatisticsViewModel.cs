using GolemUI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GolemUI.Model.PrettyChartData;

namespace GolemUI.ViewModel
{
    public class StatisticsViewModel : INotifyPropertyChanged, ISavableLoadableDashboardPage
    {

        public PrettyChartData ChartData => new PrettyChartData()
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
                    new PrettyChartBinEntry(){Label="Siódma", Value=6.5},
                    new PrettyChartBinEntry(){Label="Siódma", Value=6.5},
                    new PrettyChartBinEntry(){Label="Siódma", Value=6.5},
                    new PrettyChartBinEntry(){Label="Siódma", Value=6.5},
                    new PrettyChartBinEntry(){Label="Siódma", Value=6.5},
                    new PrettyChartBinEntry(){Label="Siódma", Value=6.5},
                    new PrettyChartBinEntry(){Label="Siódma", Value=6.5},
                    new PrettyChartBinEntry(){Label="Siódma", Value=6.5},
                }
            }
        };

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
    }
}
