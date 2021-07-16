using GolemUI.Command;
using GolemUI.Interfaces;
using GolemUI.Settings;
using GolemUI.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for DashboardMain.xaml
    /// </summary>
    public partial class DashboardMain : UserControl
    {
        public DashboardMain(DashboardMainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        public void RefreshStatus()
        {
            var br = SettingsLoader.LoadBenchmarkFromFileOrDefault();

            string reason;
            if (!br.IsClaymoreMiningPossible(out reason))
            {
                //this.txtGpuStatus.Text = reason;
            }
            else
            {
                //this.txtGpuStatus.Text = "Ready";
            }
        }
        
        public async void RefreshActivityStatus()
        {
            if (GlobalApplicationState.Instance.ProcessController.IsRunning)
            {
                ActivityStatus? st = await GlobalApplicationState.Instance.ProcessController.GetActivityStatus();
                if (st?.last1h?.Ready >= 1)
                {
                    // txtGpuStatus.Text = "Mining";
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            GlobalApplicationState.Instance.NotifyApplicationStateChanged(this, GlobalApplicationStateAction.yagnaAppStarting);

            lblStatus.Content = "Starting";
            //lblStatus.Background = Brushes.Yellow;
            btnStart.IsEnabled = false;

            var settings = SettingsLoader.LoadSettingsFromFileOrDefault();

            GlobalApplicationState.Instance.ProcessController.Subnet = settings.Subnet;

            await GlobalApplicationState.Instance.ProcessController.Init();
            //File.WriteAllText("debug.txt", GlobalApplicationState.Instance.ProcessController.ConfigurationInfoOutput);

            lblStatus.Content = "Started";
            //lblStatus.Background = Brushes.Green;
            btnStop.Visibility = Visibility.Visible;
            btnStart.Visibility = Visibility.Collapsed;


            GlobalApplicationState.Instance.NotifyApplicationStateChanged(this, GlobalApplicationStateAction.yagnaAppStarted);
        }

        private async void btnStop_Click(object sender, RoutedEventArgs e)
        {
            bool killProviderInsteadOfStopping = true;
            if (killProviderInsteadOfStopping)
            {
                GlobalApplicationState.Instance.ProcessController.KillProvider();
				//do not kill yagna here
                //GlobalApplicationState.Instance.ProcessController.KillYagna();
            }
            else
            {
                //insta kill provider and gracefully shutdown yagna
                GlobalApplicationState.Instance.ProcessController.KillProvider();
                
                bool providerEndedSuccessfully = await GlobalApplicationState.Instance.ProcessController.StopProvider();
                if (!providerEndedSuccessfully)
                {
                    MessageBox.Show("Provider process failed to shutdown gracefully, killing...");
                    GlobalApplicationState.Instance.ProcessController.KillProvider();
                }
                bool yagnaEndedSuccessfully = await GlobalApplicationState.Instance.ProcessController.StopYagna();
                if (!yagnaEndedSuccessfully)
                {
                    MessageBox.Show("Yagna process failed to shutdown gracefully, killing...");
                    GlobalApplicationState.Instance.ProcessController.KillYagna();
                }
            }


            lblStatus.Content = "Stopped";
            //lblStatus.Background = Brushes.Gray;
            GlobalApplicationState.Instance.NotifyApplicationStateChanged(this, GlobalApplicationStateAction.yagnaAppStopped);
            btnStop.Visibility = Visibility.Collapsed;
            btnStart.Visibility = Visibility.Visible;
            btnStart.IsEnabled = true;

        }

    }
}
