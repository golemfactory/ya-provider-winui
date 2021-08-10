using GolemUI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static GolemUI.Model.PrettyChartData;

namespace GolemUI.UI.Charts
{
    /// <summary>
    /// Interaction logic for PrettyChartDesign.xaml
    /// </summary>
    public partial class PrettyChartDesign : UserControl
    {
        public PrettyChartDesign()
        {
            InitializeComponent();
        }
    }

    class PrettyChartDesignViewModel
    {
        public int NumberOfBins => ChartData.BinData.BinEntries.Count;

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
