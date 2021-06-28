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
using GolemUI.Settings;

namespace GolemUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IProcessControler _processController;

#if DEBUG
        public DebugWindow? DebugWindow { get; set; }
#endif
        public MainWindow(IProcessControler processController)
        {
            _processController = processController;

            InitializeComponent();
            this.Title = GlobalSettings.AppTitle;
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            lblRunning.Content = "Starting";
            lblRunning.Background = Brushes.Yellow;
            btnStart.IsEnabled = false;


            var settings = SettingsLoader.LoadSettingsFromFileOrDefault();

            ((ProcessController)_processController).Subnet = settings.Subnet;
          
            await _processController.Init();

            lblRunning.Content = "Started";
            lblRunning.Background = Brushes.Green;
            btnStart.IsEnabled = false;

        }


 



        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
