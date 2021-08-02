
using GolemUI.ViewModel;
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
    public partial class DashboardSettings : UserControl
    {

        public SettingsViewModel ViewModel;
        public DashboardSettings(SettingsViewModel viewModel)
        {

            InitializeComponent();
            ViewModel = viewModel;
            this.DataContext = this.ViewModel;
        }

        private void btnRunBenchmark_Click(object sender, RoutedEventArgs e)
        {
            ViewModel!.StartBenchmark();
        }

        private void btnStopBenchmark_Click(object sender, RoutedEventArgs e)
        {
            ViewModel!.StopBenchmark();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnAdvancedSettings_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SwitchToAdvancedSettings();
        }
    }
}
