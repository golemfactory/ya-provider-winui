using GolemUI.Command;
using GolemUI.Interfaces;
using GolemUI.Settings;
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
        public DashboardMain()
        {
            InitializeComponent();

            //_processController = processController;

            //this.Title = GlobalSettings.AppTitle;
            btnStop.Visibility = Visibility.Collapsed;

            GlobalApplicationState.Instance.ApplicationStateChanged += OnGlobalApplicationStateChanged;

            RefreshStatus();
            RefreshPaymentStatus();
        }

        public void RefreshStatus()
        {
            var br = SettingsLoader.LoadBenchmarkFromFileOrDefault();

            string reason;
            if (!br.IsClaymoreMiningPossible(out reason))
            {
                this.txtGpuStatus.Text = reason;
            }
            else
            {
                this.txtGpuStatus.Text = "Ready";
            }
        }
        public async void RefreshPaymentStatus()
        {
            if (GlobalApplicationState.Instance.ProcessController.IsRunning)
            {
                LocalSettings ls = SettingsLoader.LoadSettingsFromFileOrDefault();
                PaymentStatus? st = await GlobalApplicationState.Instance.ProcessController.GetPaymentStatus(ls.EthAddress);
                decimal? totalGLM = st?.Amount;
                string sTotalGLM = "?";
                string sTotalUSD = "?";
                if (totalGLM != null)
                {
                    sTotalGLM = String.Format("{0:0.000}GLM", (double)totalGLM);
                    sTotalUSD = String.Format("{0:0.00}$", (double)totalGLM * GlobalSettings.GLMUSD);
                }
                this.lblTotalGLM.Content = sTotalGLM;
                this.lblTotalUSD.Content = sTotalUSD;

                decimal? pendingGLM = st?.Incoming?.Accepted?.TotalAmount;
                string sPendingGLM = "?";
                string sPendingUSD = "?";
                if (pendingGLM != null)
                {
                    sPendingGLM = String.Format("{0:0.000}GLM", (double)pendingGLM);
                    sPendingUSD = String.Format("{0:0.00}$", (double)pendingGLM * GlobalSettings.GLMUSD);
                }
                this.lblPendingGLM.Content = sPendingGLM;
                this.lblPendingUSD.Content = sPendingUSD;
                this.lblEstimatedProfit.Content = "N/A";
            }
            else
            {
                this.lblTotalGLM.Content = "N/A";
                this.lblTotalUSD.Content = "N/A";
                this.lblPendingGLM.Content = "N/A";
                this.lblPendingUSD.Content = "N/A";
                this.lblEstimatedProfit.Content = "N/A";
            }
        }

        public async void RefreshActivityStatus()
        {
            if (GlobalApplicationState.Instance.ProcessController.IsRunning)
            {
                ActivityStatus? st = await GlobalApplicationState.Instance.ProcessController.GetActivityStatus();
                if (st?.last1h?.Ready >= 1)
                {
                    txtGpuStatus.Text = "Mining";
                }
            }
        }



        public void OnGlobalApplicationStateChanged(object sender, GlobalApplicationStateEventArgs? args)
        {
            if (args != null)
            {
                switch (args.action)
                {
                    case GlobalApplicationStateAction.timerEvent:
                        RefreshStatus();
                        RefreshPaymentStatus();
                        RefreshActivityStatus();
                        break;
                    case GlobalApplicationStateAction.yagnaAppStarted:
                        RefreshStatus();
                        RefreshPaymentStatus();
                        break;
                    case GlobalApplicationStateAction.yagnaAppStopped:
                        RefreshStatus();
                        RefreshPaymentStatus();
                        break;

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

        private void MiningBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtGpuStatus.Text = "Clicked";
        }
    }
}
