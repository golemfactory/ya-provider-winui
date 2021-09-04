using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GolemUI.Model;
using static GolemUI.Model.PrettyChartData;

namespace GolemUI.DesignViewModel
{
    class DashboardStatisticsDesignViewModel
    {
        public PrettyChartData ChartData => new PrettyChartData()
        {
            NoAnimate = true,
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
    }
}
