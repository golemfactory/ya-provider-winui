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
    /// Interaction logic for DashboardSettings.xaml
    /// </summary>
    public partial class DashboardSettings : UserControl
    {
        public DashboardSettings()
        {
            InitializeComponent();

            LocalSettings settings = SettingsLoader.LoadSettingsFromFileOrDefault();

            txNodeName.Text = settings.NodeName;
            txSubnet.Text = settings.Subnet;
            txWalletAddress.Text = settings.EthAddress;
        }

        private void btnApplySettings(object sender, RoutedEventArgs e)
        {
            LocalSettings settings = new LocalSettings();
            settings.NodeName = txNodeName.Text;
            settings.Subnet = txSubnet.Text;
            settings.EthAddress = txWalletAddress.Text;

            SettingsLoader.SaveSettingsToFile(settings);
        }
    }
}
