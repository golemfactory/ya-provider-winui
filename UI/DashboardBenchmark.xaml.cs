using GolemUI.Claymore;
using GolemUI.Command;
using GolemUI.Settings;
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

    public enum NavigationType
    {
        Type1,
        Type2,
        Type3,

    }

    /// <summary>
    /// Interaction logic for DashboardBenchmark.xaml
    /// </summary>
    public partial class DashboardBenchmark : UserControl
    {
        public DashboardBenchmark()
        {
            InitializeComponent();


            SetActiveNavigationType(NavigationType.Type1);
        }



        private void ResetGpuList()
        {
            // grdGpuList.Children.Clear();
        }


        public void SetActiveNavigationType(NavigationType navigationType)
        {
            if (navigationType == NavigationType.Type1)
            {
                brdNavigationType1.Visibility = Visibility.Visible;
                brdNavigationType2.Visibility = Visibility.Collapsed;
            }
            if (navigationType == NavigationType.Type2)
            {
                brdNavigationType1.Visibility = Visibility.Collapsed;
                brdNavigationType2.Visibility = Visibility.Visible;
            }
        }

        private void ChangeGpuEnabled()
        {
            /*
            var benchmarkSettings = SettingsLoader.LoadBenchmarkFromFileOrDefault();

            if (benchmarkSettings != null && benchmarkSettings.liveStatus != null)
            {
                bool changed = false;
                foreach (var gpu in benchmarkSettings.liveStatus.GPUs)
                {
                    int gpuNo = gpu.Key;
                    GpuEntryUI? currentEntry = null;
                    if (_entries.ContainsKey(gpuNo))
                    {
                        currentEntry = _entries[gpuNo];
                        if (currentEntry != null)
                        {
                           // gpu.Value.IsEnabledByUser = currentEntry.cbEnableMining.IsChecked ?? false;
                            changed = true;
                        }
                    }
                }
                if (changed)
                {
                    SettingsLoader.SaveBenchmarkToFile(benchmarkSettings);
                    UpdateBenchmarkStatus(benchmarkSettings.liveStatus, );
                }
            }*/

        }

        private void btnReady_Click(object sender, RoutedEventArgs e)
        {
            GlobalApplicationState.Instance?.Dashboard?.SwitchPage(DashboardPages.PageDashboardMain);
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            GlobalApplicationState.Instance?.Dashboard?.SwitchPageBack();
        }
    }
}
