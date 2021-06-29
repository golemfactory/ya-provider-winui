using GolemUI.Interfaces;
using GolemUI.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GolemUI
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        public DebugWindow()
        {
            InitializeComponent();
#if DEBUG
            GlobalApplicationState.Instance.ProcessController.LineHandler += LogLine;
            GlobalApplicationState.Instance.ApplicationStateChanged += OnGlobalApplicationStateChanged;
            NameGen g = new NameGen();
            for (int i = 0; i < 20; i++)
            {
                txtProvider.Text += g.GenerateElvenName() + "-" + g.GenerateDwarvenName() + "\n";
            }
#endif
        }

        void TrimControlTextSize(TextBox tb)
        {
            int maxLogSize = 100000;
            int trimLogSize = (int)(maxLogSize * 0.1);
            if (tb.Text.Length > maxLogSize)
            {
                tb.Text = tb.Text.Substring(trimLogSize);
            }
        }

        void LogLine(string logger, string line)
        {
            if (logger == "provider")
            {
                this.Dispatcher.Invoke(() =>
                {
                    TrimControlTextSize(txtProvider);
                    txtProvider.Text += $"{line}\n";
                    svProvider.ScrollToBottom();
                });
            }
            if (logger == "yagna")
            {
                this.Dispatcher.Invoke(() =>
                {
                    TrimControlTextSize(txtYagna);
                    txtYagna.Text += $"{line}\n";
                    svYagna.ScrollToBottom();
                });
            }

        }
        public void OnGlobalApplicationStateChanged(object sender, GlobalApplicationStateEventArgs? args)
        {
            if (args != null)
            {
                switch (args.action)
                {
                    case GlobalApplicationStateAction.yagnaAppStarting:
                        txtYagna.Text = "";
                        txtProvider.Text = "";
                        break;
                    case GlobalApplicationStateAction.yagnaAppStopped:
                        break;
                    case GlobalApplicationStateAction.yagnaAppStarted:
                        break;
                    case GlobalApplicationStateAction.benchmarkStarted:
                        break;
                    case GlobalApplicationStateAction.benchmarkStopped:
                        break;
                }
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("CMD.exe");
        }

        private void btnOpenSettingsData_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", SettingsLoader.GetLocalPath());
        }

        private async void btnVersionInfo_Click(object sender, RoutedEventArgs e)
        {
            txtYagna.Text = await GlobalApplicationState.Instance.ProcessController.GetOffers();

        }
    }


}
