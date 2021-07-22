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
    public partial class DashboardAdvancedSettings : UserControl
    {
        bool _initialized = false;
        bool _readOnly = false;

        public DashboardAdvancedSettings()
        {
            InitializeComponent();
            ResetChanges();

            _initialized = true;
        }

        public void ResetChanges()
        {
            LocalSettings settings = SettingsLoader.LoadSettingsFromFileOrDefault();

            txNodeName.Text = settings.NodeName;
            txSubnet.Text = settings.Subnet;
            txWalletAddress.Text = settings.EthAddress;
            cbDebugOutput.IsChecked = settings.EnableDebugLogs;
            //cbEnableWASM.IsChecked = settings.EnableWASMUnit;
            cbStartWithWindows.IsChecked = settings.StartWithWindows;
            cbShowYagnaConsole.IsChecked = settings.StartYagnaCommandLine;
            cbShowProviderConsole.IsChecked = settings.StartProviderCommandLine;
            cbMinimizeToTrayOnMinimize.IsChecked = settings.MinimizeToTrayOnMinimize;
            cbDisableNotificationsWhenMinimized.IsChecked = settings.DisableNotificationsWhenMinimized;
            cbCloseOnExit.IsChecked = settings.CloseOnExit;


            btnApplySettings.IsEnabled = false;
            btnCancelChanges.IsEnabled = false;
        }


        private void SetSettingsEnabled(bool enabled)
        {
            ResetChanges();
            txNodeName.IsEnabled = enabled;
            txSubnet.IsEnabled = enabled;
            txWalletAddress.IsEnabled = enabled;
            cbDebugOutput.IsEnabled = enabled;
            //cbEnableWASM.IsEnabled = enabled;
            cbStartWithWindows.IsEnabled = enabled;
            cbShowYagnaConsole.IsEnabled = enabled;
            cbShowProviderConsole.IsEnabled = enabled;

            _readOnly = !enabled;

        }
        public void SwitchSettingsToReadOnly()
        {
            SetSettingsEnabled(enabled: false);
        }
        public void SwitchSettingsToEdit()
        {
            SetSettingsEnabled(enabled: true);
        }

        private void btnApplySettings_Click(object sender, RoutedEventArgs e)
        {
            LocalSettings settings = SettingsLoader.LoadSettingsFromFileOrDefault();

            settings.NodeName = txNodeName.Text;
            settings.Subnet = txSubnet.Text;
            settings.EthAddress = txWalletAddress.Text;
            settings.EnableDebugLogs = cbDebugOutput.IsChecked ?? false;
            // settings.EnableWASMUnit = cbEnableWASM.IsChecked ?? false;
            settings.StartWithWindows = cbStartWithWindows.IsChecked ?? false;
            settings.StartYagnaCommandLine = cbShowYagnaConsole.IsChecked ?? false;
            settings.StartProviderCommandLine = cbShowProviderConsole.IsChecked ?? false;
            settings.DisableNotificationsWhenMinimized = cbDisableNotificationsWhenMinimized.IsChecked ?? false;
            settings.CloseOnExit = cbCloseOnExit.IsChecked ?? false;
            settings.MinimizeToTrayOnMinimize = cbMinimizeToTrayOnMinimize.IsChecked ?? false;

            SettingsLoader.SaveSettingsToFile(settings);
            ResetChanges();
        }


        private void settingsChanged()
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
            if (settings.EnableDebugLogs != cbDebugOutput.IsChecked) different = true;
            // if (settings.EnableWASMUnit != cbEnableWASM.IsChecked) different = true;
            if (settings.StartWithWindows != cbStartWithWindows.IsChecked) different = true;
            if (settings.StartYagnaCommandLine != cbShowYagnaConsole.IsChecked) different = true;
            if (settings.StartProviderCommandLine != cbShowProviderConsole.IsChecked) different = true;
            if (settings.DisableNotificationsWhenMinimized != cbDisableNotificationsWhenMinimized.IsChecked) different = true;
            if (settings.MinimizeToTrayOnMinimize != cbMinimizeToTrayOnMinimize.IsChecked) different = true;
            if (settings.CloseOnExit != cbCloseOnExit.IsChecked) different = true;

            if (different)
            {
                btnApplySettings.IsEnabled = true;
                btnCancelChanges.IsEnabled = true;
            }
            else
            {
                btnApplySettings.IsEnabled = false;
                btnCancelChanges.IsEnabled = false;
            }
        }

        private void settingsTextChanged(object sender, TextChangedEventArgs e)
        {
            settingsChanged();
        }

        private void settingsCheckboxChecked(object sender, RoutedEventArgs e)
        {
            settingsChanged();
        }

        private void btnCancelSettings_Click(object sender, RoutedEventArgs e)
        {
            ResetChanges();
        }
    }
}
