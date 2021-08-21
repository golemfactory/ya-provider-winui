﻿using BetaMiner.Command;
using BetaMiner.Interfaces;

using BetaMiner.ViewModel;
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

namespace BetaMiner
{
    /// <summary>
    /// Interaction logic for DashboardMain.xaml
    /// </summary>
    public partial class DashboardMain : UserControl
    {
        public DashboardMainViewModel Model => (DataContext as DashboardMainViewModel)!;

        IProcessControler _processControler;

        IBenchmarkResultsProvider _benchmarkResultsProvider;
        public DashboardMain(DashboardMainViewModel viewModel, IBenchmarkResultsProvider benchmarkResultsProvider, IProcessControler processControler)
        {
            _benchmarkResultsProvider = benchmarkResultsProvider;
            DataContext = viewModel;
            InitializeComponent();
            _processControler = processControler;
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
            System.Diagnostics.Process.Start(BetaMiner.Properties.Settings.Default.GolemWebPage);
        }
    }
}
