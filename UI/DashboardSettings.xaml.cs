
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
    public partial class DashboardSettings : UserControl
    {
        public SettingsViewModel ViewModel;
        public DashboardSettings(SettingsViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            this.DataContext = this.ViewModel;
        }

        private void btnRunBenchmark_Click(object sender, RoutedEventArgs e)
        {
            bool shouldStartBenchmark = true;
            if (ViewModel.IsMiningProcessRunning())
            {
                ViewModel.StopBenchmark();

                var userSettings = ViewModel.UserSettings;
                if (userSettings.ShouldDisplayNotificationsIfMiningIsActive)
                {

                    ViewModel.ShouldRestartMiningAfterBenchmark = true;
                    var dlg = new UI.Dialogs.DlgShouldStopMiningBeforeBenchmark(new GolemUI.ViewModel.Dialogs.DlgShouldStopMiningBeforeBenchmarkViewModel(shouldAutoRestartMining: userSettings.ShouldAutoRestartMiningAfterBenchmark, rememberMyPreference: !userSettings.ShouldDisplayNotificationsIfMiningIsActive));
                    dlg.Owner = Window.GetWindow(this);
                    ViewModel.RequestDarkBackgroundVisibilityChange(true);
                    if (dlg != null && dlg.Model != null && (dlg.ShowDialog() ?? false))
                    {
                        ViewModel.UpdateBenchmarkDialogSettings(dlg.Model.ShouldAutoRestartMining, dlg.Model.RememberMyPreference);
                    }
                    else
                    {
                        shouldStartBenchmark = false;
                    }

                    ViewModel.RequestDarkBackgroundVisibilityChange(false);
                }
                else
                {
                    ViewModel.ShouldRestartMiningAfterBenchmark = userSettings.ShouldAutoRestartMiningAfterBenchmark;
                }

            }
            else
            {
                ViewModel.ShouldRestartMiningAfterBenchmark = false;
            }

            if (shouldStartBenchmark)
                ViewModel!.StartBenchmark();
        }

        private void btnStopBenchmark_Click(object sender, RoutedEventArgs e)
        {
            ViewModel!.StopBenchmark();
        }

        private void BtnAdvancedSettings_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SwitchToAdvancedSettings();
        }
    }
}
