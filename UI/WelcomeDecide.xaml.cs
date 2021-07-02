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

namespace GolemUI
{
    /// <summary>
    /// Interaction logic for WelcomeDecide.xaml
    /// </summary>
    public partial class WelcomeDecide : UserControl
    {
        public WelcomeDecide()
        {
            InitializeComponent();
        }

        private void btnUseWallet_Click(object sender, RoutedEventArgs e)
        {
            GlobalApplicationState.Instance.Dashboard.SwitchPage(DashboardPages.PageWelcomeAddress);
        }
    }
}
