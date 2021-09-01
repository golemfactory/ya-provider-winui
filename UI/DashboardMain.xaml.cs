using GolemUI.Command;
using GolemUI.Interfaces;

using GolemUI.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for DashboardMain.xaml
    /// </summary>
    public partial class DashboardMain : UserControl
    {
        public DashboardMainViewModel Model => (DataContext as DashboardMainViewModel)!;

        IProcessController _processController;

        IBenchmarkResultsProvider _benchmarkResultsProvider;
        public DashboardMain(DashboardMainViewModel viewModel, IBenchmarkResultsProvider benchmarkResultsProvider, IProcessController processController)
        {
            _benchmarkResultsProvider = benchmarkResultsProvider;
            DataContext = viewModel;
            InitializeComponent();
            _processController = processController;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            Model!.Start();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            Model!.Stop();
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            Model.SwitchToSettings();
        }

        private void BtnGolemLogo_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(GolemUI.Properties.Settings.Default.GolemWebPage);
        }
    }
}
