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
        static Random r = new Random();

        public StatisticsViewModel ViewModel;
        public DashboardStatistics(StatisticsViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            this.DataContext = this.ViewModel;
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ChartData1.Clear();
        }
        private void btnMoveRight_Click(object sender, RoutedEventArgs e)
        {
            //chartData.MoveRight(1, true);
        }

        private void btnMoveLeft_Click(object sender, RoutedEventArgs e)
        {
            //chartData.MoveLeft(1, true);
        }

        private void btnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            chartData.ZoomIn(1, true);
        }
        private void btnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            chartData.ZoomOut(1, true);
        }

        private async void btnAddEntry_Click(object sender, RoutedEventArgs e)
        {
            //ViewModel.ChartData1.SetBinTimeSize(TimeSpan.FromMinutes(1));

            for (int i = 0; i < 1000000; i++)
            {
                ViewModel.ChartData1.RawData?.AddNewEntry(DateTime.Now, r.NextDouble() * 1.0, true);
                await Task.Delay(1000);
            };


            //this.ViewModel.ChartData4.AddOrUpdateBinEntry(-1, DateTime.Now.ToString("88-88-88"), r.NextDouble() * 100.0);
            //            await Task.Delay(10);

            /* for (int i = 0; i < 1000000; i++)
             {
                 this.ViewModel.ChartData4.AddOrUpdateBinEntry(-1, DateTime.Now.ToString("HH-mm-ss"), r.NextDouble() * 100.0);
                 await Task.Delay(100);
             };*/

        }
    }
}
