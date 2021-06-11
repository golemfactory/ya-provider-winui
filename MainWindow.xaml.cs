using GolemUI.Interfaces;
using Newtonsoft.Json;
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
using System.Diagnostics;

namespace GolemUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IProcessControler _processController;

#if DEBUG
        public DebugWindow DebugWindow { get; set; }
#endif
        public MainWindow(IProcessControler processController)
        {
            _processController = processController;

            InitializeComponent();
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            lblRunning.Content = "Starting";
            lblRunning.Background = Brushes.Yellow;
            btnStart.IsEnabled = false;


            await _processController.Init();

            lblRunning.Content = "Started";
            lblRunning.Background = Brushes.Green;
            btnStart.IsEnabled = false;

        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            _processController.Stop();
        }

        private async void btnId_Click(object sender, RoutedEventArgs e)
        {
            await _processController.Init();


            await _processController.Me();


        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new GolemUISettingsWindow();
            settingsWindow.ShowDialog();
        }


        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            Process[] yagnaProcesses;
            Process[] providerProcesses;

            ProcessMonitor.GetProcessList(out yagnaProcesses, out providerProcesses);
            if (yagnaProcesses.Length > 0 || providerProcesses.Length > 0)
            {
                ExistingProcessesWindow w = new ExistingProcessesWindow();
                w.Owner = this;
                var dialogResult = w.ShowDialog();
                switch (dialogResult)
                {
                    case true:
                        // User accepted dialog box
                        break;
                    case false:
                        // User canceled dialog box
                        return;
                    default:
                        // Indeterminate
                        break;
                }
            }
            lblRunning.Content = "Not running";
            lblRunning.Background = Brushes.Red;
            btnStart.IsEnabled = true;
        }
    }
}
