using GolemUI.ViewModel;
using GolemUI.ViewModel.Dialogs;
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
using System.Windows.Shapes;

namespace GolemUI.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for DlgEditAddress.xaml
    /// </summary>
    public partial class DlgShouldStopMiningBeforeBenchmark : Window
    {
        DlgShouldStopMiningBeforeBenchmarkViewModel? _model = null;
        public DlgShouldStopMiningBeforeBenchmarkViewModel? Model => _model;
        public DlgShouldStopMiningBeforeBenchmark(DlgShouldStopMiningBeforeBenchmarkViewModel model)
        {
            InitializeComponent();
            _model = model;
            this.DataContext = model;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
