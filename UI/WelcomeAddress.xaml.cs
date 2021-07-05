using GolemUI.Settings;
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
    /// Interaction logic for WelcomeAddress.xaml
    /// </summary>
    public partial class WelcomeAddress : UserControl
    {
        public WelcomeAddress()
        {
            InitializeComponent();

            LocalSettings s = SettingsLoader.LoadSettingsFromFileOrDefault();
            tbWalletAddress.Text = s.EthAddress;
        }

        private bool validateAddress()
        {
            if (String.IsNullOrEmpty(tbWalletAddress.Text))
            {
                return false;
            }
            return true;
        }

        private void btnCheckAddress(object sender, RoutedEventArgs e)
        {
            if (validateAddress())
            {
                System.Diagnostics.Process.Start("explorer.exe", "https://etherscan.io/address/" + tbWalletAddress.Text);
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (validateAddress())
            {
                LocalSettings s = SettingsLoader.LoadSettingsFromFileOrDefault();
                s.EthAddress = tbWalletAddress.Text;
                SettingsLoader.SaveSettingsToFile(s);
                GlobalApplicationState.Instance.NotifyApplicationStateChanged(this, GlobalApplicationStateAction.reloadSettings);
                GlobalApplicationState.Instance.Dashboard?.SwitchPage(DashboardPages.PageWelcomeNodeName);
            }
        }
    }
}
