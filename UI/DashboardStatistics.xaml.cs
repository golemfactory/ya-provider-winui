using GolemUI.ViewModel;
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

namespace GolemUI.UI
{
    /// <summary>
    /// Interaction logic for DasboardStatistics.xaml
    /// </summary>
    public partial class DashboardStatistics : UserControl
    {
#if STATISTICS_ENABLED
        public StatisticsViewModel ViewModel;
        public DashboardStatistics(StatisticsViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            this.DataContext = this.ViewModel;
        }
#endif

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
#if STATISTICS_ENABLED
            ViewModel.LoadData();
#endif
        }
        private void btnMoveRight_Click(object sender, RoutedEventArgs e)
        {
#if STATISTICS_ENABLED
            ViewModel.MoveDataRight();
#endif
        }

    }
}
