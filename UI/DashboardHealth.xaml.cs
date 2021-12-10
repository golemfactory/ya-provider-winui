using GolemUI.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using GolemUI.Utils;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace GolemUI.UI
{
    /// <summary>
    /// Interaction logic for DasboardStatistics.xaml
    /// </summary>
    public partial class DashboardHealth : UserControl
    {
        static Random r = new Random();

        public HealthViewModel ViewModel;
        public DashboardHealth(HealthViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            this.DataContext = this.ViewModel;
        }


        private async void btnPerformYagnaHealthCheck_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.PerformYagnaHealthCheck();
        }
        private async void btnPerformProviderHealthCheck_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.PerformProviderHealthCheck(this.SelectedProviderProperties);
        }

        private void btnOpenLogs_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", PathUtil.GetLocalPath());
        }

        private void btnScanYagnaLogs_Click(object sender, RoutedEventArgs e)
        {
            var path = Path.Combine(PathUtil.GetYagnaPath(), "data", "yagna_rCURRENT.log");

            if (!File.Exists(path))
            {
                MessageBox.Show(String.Format("Cannot find yagna log file {}. This suggest that yagna didn't start at all, or there some problems with directory settings.", path));
                return;
            }

            System.Diagnostics.Process.Start("explorer.exe", path);
        }

        private void btnOpenYagnaFolder_Click(object sender, RoutedEventArgs e)
        {

            System.Diagnostics.Process.Start("explorer.exe", PathUtil.GetYagnaPath());
        }

        private void btnOpenProvider_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", PathUtil.GetProviderPath());
        }


    }
}
