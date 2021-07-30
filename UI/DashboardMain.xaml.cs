﻿using GolemUI.Command;
using GolemUI.Interfaces;
using GolemUI.Settings;
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

        IBenchmarkResultsProvider _benchmarkResultsProvider;
        public DashboardMain(DashboardMainViewModel viewModel, IBenchmarkResultsProvider benchmarkResultsProvider)
        {
            _benchmarkResultsProvider = benchmarkResultsProvider;
            DataContext = viewModel;
            InitializeComponent();
        }

        public void RefreshStatus()
        {
            var br = _benchmarkResultsProvider.LoadBenchmarkResults();

            string reason;
            if (!br.IsClaymoreMiningPossible(out reason))
            {
                //this.txtGpuStatus.Text = reason;
            }
            else
            {
                //this.txtGpuStatus.Text = "Ready";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            Model!.Start();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            Model!.Stop();
        }

    }
}
