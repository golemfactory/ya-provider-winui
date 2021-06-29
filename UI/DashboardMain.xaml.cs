using GolemUI.Command;
using GolemUI.Interfaces;
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
    /// Interaction logic for DashboardMain.xaml
    /// </summary>
    public partial class DashboardMain : UserControl
    {
        public DashboardMain()
        {
            InitializeComponent();

            //_processController = processController;

            //this.Title = GlobalSettings.AppTitle;
            btnStop.IsEnabled = false;

            GlobalApplicationState.Instance.ApplicationStateChanged += OnGlobalApplicationStateChanged;
        }

        public async void OnGlobalApplicationStateChanged(object sender, GlobalApplicationStateEventArgs? args)
        {
            if (args != null)
            {
                switch (args.action)
                {
                    case GlobalApplicationStateAction.timerEvent:
                        if (GlobalApplicationState.Instance.ProcessController.IsRunning)
                        {
                            PaymentStatus? st = GlobalApplicationState.Instance.ProcessController.GetStatus();
                            decimal? totalGLM = st?.Incoming?.Confirmed?.TotalAmount;
                            string sTotalGLM = "?";
                            string sTotalUSD = "?";
                            if (totalGLM != null)
                            {
                                sTotalGLM = totalGLM.ToString() + "GLM";
                                sTotalUSD = String.Format("{0:0.00}$", (double)totalGLM * GlobalSettings.GLMUSD);
                            }
                            this.lblTotalGLM.Content = sTotalGLM;
                            this.lblTotalUSD.Content = sTotalUSD;

                            decimal? pendingGLM = st?.Incoming?.Requested?.TotalAmount;
                            string sPendingGLM = "?";
                            string sPendingUSD = "?";
                            if (pendingGLM != null)
                            {
                                sPendingGLM = pendingGLM.ToString() + "GLM";
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

            lblStatus.Content = "Started";
            //lblStatus.Background = Brushes.Green;
            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
            
            GlobalApplicationState.Instance.NotifyApplicationStateChanged(this, GlobalApplicationStateAction.yagnaAppStarted);
        }

        private async void btnStop_Click(object sender, RoutedEventArgs e)
        {
            bool killProviderInsteadOfStopping = true;
            if (killProviderInsteadOfStopping)
            {
                GlobalApplicationState.Instance.ProcessController.KillProvider();
                GlobalApplicationState.Instance.ProcessController.KillYagna();
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
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
        }
    }
}
