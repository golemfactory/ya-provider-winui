﻿using GolemUI.Interfaces;

using GolemUI.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        private void BtnStatistics_Click(object sender, RoutedEventArgs e)
        {
            Model.SwitchToStatistics();
        }
        private void BtnTRexInfo_Click(object sender, RoutedEventArgs e)
        {
            Model.SwitchToTRexInfo();
        }

        private void lblPaymentStateMessage_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Model.ShouldPaymentMessageTooltipBeAccessible)
                tooltip.IsOpen = true;
        }

        private void lblPaymentStateMessage_MouseLeave(object sender, MouseEventArgs e)
        {
            tooltip.IsOpen = false;
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Model.PolygonLink);
        }
    }
}
