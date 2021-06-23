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
    /// Interaction logic for GpuEntryUI.xaml
    /// </summary>
    public partial class GpuEntryUI : UserControl
    {
        public GpuEntryUI()
        {
            InitializeComponent();
        }

        public void SetInfo(string info)
        {
            this.lblInfo.Content = info;
        }

        public void SetDagProgress(float progr)
        {
            this.lblProgress.Content = progr.ToString();
            this.pbProgress.Value = progr * 100;
        }
        public void SetMiningSpeed(float miningSpeed)
        {
            this.lblPower.Content = miningSpeed.ToString();
        }

    }
}
