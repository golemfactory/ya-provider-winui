using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Threading;
using GolemUI.Utils;

namespace GolemUI
{
    /// <summary>
    /// Interaction logic for ExistingProcessesWindow.xaml
    /// </summary>
    public partial class ExistingProcessesWindow : Window
    {
        private System.Timers.Timer _timer;
        public ExistingProcessesWindow()
        {
            InitializeComponent();
            Reload();
            _timer = new System.Timers.Timer();
        }
        public delegate void ReloadDelegate();


        private void Reload()
        {
            this.lbProcesses.Items.Clear();

            Process[] yagnaProcesses;
            Process[] providerProcesses;
            Process[] phoenixProcesses;
            ProcessMonitor.GetProcessList(out yagnaProcesses, out providerProcesses, out phoenixProcesses);
            foreach (var yagnaProcess in yagnaProcesses)
            {
                this.lbProcesses.Items.Add(String.Format("Found running yagna process: {0}\n", yagnaProcess.Id));
            }

            foreach (var providerProcess in providerProcesses)
            {
                this.lbProcesses.Items.Add(String.Format("Found running ya-provider process: {0}\n", providerProcess.Id));
            }

            foreach (var phoenixProcess in phoenixProcesses)
            {
                this.lbProcesses.Items.Add(String.Format("Found running phoenix process: {0}\n", phoenixProcess.Id));
            }

            if (yagnaProcesses.Length == 0 && providerProcesses.Length == 0 && phoenixProcesses.Length == 0)
            {
                this._timer.Stop();
                this.DialogResult = true;
                this.Close();
            }
        }


        private void StartTimer()
        {
            _timer = new System.Timers.Timer();
            _timer.Interval = 2000;

            // Hook up the Elapsed event for the timer. 
            _timer.Elapsed += OnTimedEvent;

            // Have the timer fire repeated events (true is the default)
            _timer.AutoReset = true;

            // Start the timer
            _timer.Enabled = true;
        }

        private void btnKillAll_Click(object sender, RoutedEventArgs e)
        {
            Process[] yagnaProcesses;
            Process[] providerProcesses;
            Process[] phoenixProcesses;
            ProcessMonitor.GetProcessList(out yagnaProcesses, out providerProcesses, out phoenixProcesses);

            foreach (var yagnaProcess in yagnaProcesses)
            {
                yagnaProcess.Kill(entireProcessTree: true);
            }
            foreach (var providerProcess in providerProcesses)
            {
                providerProcess.Kill(entireProcessTree: true);
            }
            foreach (var phoenixProcess in phoenixProcesses)
            {
                phoenixProcess.Kill(entireProcessTree: true);
            }
        }

        private void btnIgnore_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            //   Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);
            this.Dispatcher.Invoke(new ReloadDelegate(this.Reload));
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            Reload();

        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            StartTimer();
        }
    }
}
