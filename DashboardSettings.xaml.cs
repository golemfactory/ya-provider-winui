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
        bool _initialized = false;
        bool _readOnly = false;

        public DashboardSettings()
        {
            InitializeComponent();

            GlobalApplicationState.Instance.ApplicationStateChanged += OnGlobalApplicationStateChanged;

            ResetChanges();

            _initialized = true;
        }

        public void ResetChanges()
        {
            LocalSettings settings = SettingsLoader.LoadSettingsFromFileOrDefault();

            txNodeName.Text = settings.NodeName;
            txSubnet.Text = settings.Subnet;
            txWalletAddress.Text = settings.EthAddress;

            btnApplySettings.IsEnabled = false;
        }

        public void SwitchSettingsToReadOnly()
        {
            ResetChanges();
            txNodeName.IsEnabled = false;
            txSubnet.IsEnabled = false;
            txWalletAddress.IsEnabled = false;
            _readOnly = true;
        }
        public void SwitchSettingsToEdit()
        {
            ResetChanges();
            txNodeName.IsEnabled = true;
            txSubnet.IsEnabled = true;
            txWalletAddress.IsEnabled = true;
            _readOnly = false;
        }

        public void OnGlobalApplicationStateChanged(object sender, GlobalApplicationStateChangedArgs? args)
        {
            if (args != null)
            {
                switch(args.action)
                {
                    case GlobalApplicationStateAction.yagnaAppStarting:
                        SwitchSettingsToReadOnly();
                        break;
                    case GlobalApplicationStateAction.yagnaAppStopped:
                        SwitchSettingsToEdit();
                        break;
                }
            }
        }

        private void btnApplySettings_Click(object sender, RoutedEventArgs e)
        {
            LocalSettings settings = new LocalSettings();
            settings.NodeName = txNodeName.Text;
            settings.Subnet = txSubnet.Text;
            settings.EthAddress = txWalletAddress.Text;

            SettingsLoader.SaveSettingsToFile(settings);
            ResetChanges();
        }

        private void settingsTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_initialized || _readOnly)
            {
                return;
            }
            LocalSettings settings = SettingsLoader.LoadSettingsFromFileOrDefault();

            bool different = false;
            if (settings.NodeName != txNodeName.Text) different = true;
            if (settings.EthAddress != txWalletAddress.Text) different = true;
            if (settings.Subnet != txSubnet.Text) different = true;

            if (different)
            {
                btnApplySettings.IsEnabled = true;
            }
            else
            {
                btnApplySettings.IsEnabled = false;
            }
        }
    }
}
