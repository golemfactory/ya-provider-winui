
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
    public partial class DashboardSettingsAdv : UserControl
    {

        public SettingsAdvViewModel ViewModel;
        public DashboardSettingsAdv(SettingsAdvViewModel viewModel)
        {

            InitializeComponent();
            ViewModel = viewModel;
            this.DataContext = this.ViewModel;
        }

        private void BtnBackToMainScreen_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GoBackToSettings();
        }

        private void BtnGolemLogo_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(GolemUI.Properties.Settings.Default.GolemWebPage);
        }
    }
}
