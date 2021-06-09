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

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            var p = new GolemUI.Command.Provider();
            var lst = p.ExeUnitList();
            DebugWindow.txtR.Text = JsonConvert.SerializeObject(lst, Formatting.Indented);
            var cfg = p.Config ?? new Command.Config();
            cfg.Subnet = "reqc2";
            p.Config = cfg;

            DebugWindow.txtR.Text += JsonConvert.SerializeObject(p.Config, Formatting.Indented);

            foreach (var profile in p.Presets)
            {
                DebugWindow.txtR.Text += "\n\n--\n";
                DebugWindow.txtR.Text += JsonConvert.SerializeObject(profile, Formatting.Indented);
            }
#endif
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
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

        }
    }
}
