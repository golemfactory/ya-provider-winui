
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
using GolemUI.Interfaces;
using GolemUI.Model;

namespace GolemUI
{
    /// <summary>
    /// Interaction logic for DashboardSettings.xaml
    /// </summary>
    public partial class DashboardAdvancedSettings : UserControl
    {
        bool _initialized = false;
        bool _readOnly = false;

        private readonly IUserSettingsProvider _userSettingsProvider;

        public DashboardAdvancedSettings(IUserSettingsProvider userSettingsProvider)
        {
            _userSettingsProvider = userSettingsProvider;

            InitializeComponent();
            ResetChanges();

            _initialized = true;
        }

        public void ResetChanges()
        {
            UserSettings settings = _userSettingsProvider.LoadUserSettings();

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
            UserSettings settings = _userSettingsProvider.LoadUserSettings();

            settings.EnableDebugLogs = cbDebugOutput.IsChecked ?? false;
            // settings.EnableWASMUnit = cbEnableWASM.IsChecked ?? false;
            settings.StartWithWindows = cbStartWithWindows.IsChecked ?? false;
            settings.StartYagnaCommandLine = cbShowYagnaConsole.IsChecked ?? false;
            settings.StartProviderCommandLine = cbShowProviderConsole.IsChecked ?? false;
            settings.DisableNotificationsWhenMinimized = cbDisableNotificationsWhenMinimized.IsChecked ?? false;
            settings.CloseOnExit = cbCloseOnExit.IsChecked ?? false;
            settings.MinimizeToTrayOnMinimize = cbMinimizeToTrayOnMinimize.IsChecked ?? false;

            _userSettingsProvider.SaveUserSettings(settings);
            ResetChanges();
        }


        private void settingsChanged()
        {
            if (!_initialized || _readOnly)
            {
                return;
            }
            UserSettings settings = _userSettingsProvider.LoadUserSettings();

            bool different = false;
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
