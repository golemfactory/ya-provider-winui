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

namespace GolemUI
{
    /// <summary>
    /// Interaction logic for GpuMiningPanelUI.xaml
    /// </summary>
    public partial class GpuMiningPanelUI : UserControl
    {

        public GpuEntryUI AddSingleGpuInfo(string info, int gpuNo)
        {
            //bool canMine = false;
            /*if (info.Memory > 4500000000 && info.Vendor != "Intel")
            {
                canMine = true;
            }*/

            /*
            Brush backgroundBrush = Brushes.LightGreen;
            if (!canMine)
            {
                backgroundBrush = Brushes.Salmon;
            }

            ge.lblName = new Label();
            ge.lblName.Content = info;

            ge.lblName.Background = backgroundBrush;

            grdGpuList.Children.Add(ge.lblName);

            Grid.SetColumn(ge.lblName, 0);
            Grid.SetRow(ge.lblName, gpuNo);

            ge.lblProgress = new Label();
            ge.lblProgress.Content = "Starting...";

            ge.lblProgress.Background = backgroundBrush;
            grdGpuList.Children.Add(ge.lblProgress);

            Grid.SetColumn(ge.lblProgress, 1);
            Grid.SetRow(ge.lblProgress, gpuNo);

            ge.lblPower = new Label();
            ge.lblPower.Content = "N/A";

            ge.lblPower.Background = backgroundBrush;
            grdGpuList.Children.Add(ge.lblPower);

            Grid.SetColumn(ge.lblPower, 2);
            Grid.SetRow(ge.lblPower, gpuNo);
            */

            GpuEntryUI ge = new GpuEntryUI();

            this.spGpuList.Children.Add(ge);
            Grid.SetColumn(ge, 0);
            Grid.SetRow(ge, gpuNo);
            ge.SetInfo(info);
            return ge;
        }

        public GpuMiningPanelUI()
        {
            InitializeComponent();
        }
    }
}
