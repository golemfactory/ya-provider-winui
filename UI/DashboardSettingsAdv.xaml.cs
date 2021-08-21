﻿
using BetaMiner.ViewModel;
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

namespace BetaMiner
{
    public partial class DashboardSettingsAdv : UserControl
    {

        public SettingsAdvViewModel ViewModel;
        public DashboardSettingsAdv(SettingsAdvViewModel viewModel)
        {

            InitializeComponent();
            ViewModel = viewModel;
            this.DataContext = this.ViewModel;
        }
    }
}
