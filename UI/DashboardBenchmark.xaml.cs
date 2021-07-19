using GolemUI.Claymore;
using GolemUI.Command;
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

    public enum NavigationType
    {
        NavigationType1,
        NavigationType2,
        NavigationType3,

    }

    /// <summary>
    /// Interaction logic for DashboardBenchmark.xaml
    /// </summary>
    public partial class DashboardBenchmark : UserControl
    {
        public DashboardBenchmark()
        {
            InitializeComponent();

            LocalSettings settings = SettingsLoader.LoadSettingsFromFileOrDefault();
            var benchmarkSettings = SettingsLoader.LoadBenchmarkFromFileOrDefault();
            if (benchmarkSettings != null && benchmarkSettings.liveStatus != null)
            {
                UpdateBenchmarkStatus(benchmarkSettings.liveStatus, true);
            }

            //txNodeName.Text = localSettings.NodeName;
            //txWalletAddress.Text = localSettings.EthAddress;
            //txSubnet.Text = localSettings.Subnet;

            //lbGpus.Items.Add("GPU 1");
            //lbGpus.Items.Add("GPU 2");

            bool bUseInfoCommand = false;
            if (bUseInfoCommand)
            {
                GpuInfoCommand gic = new GpuInfoCommand();

                var deviceList = gic.GetGpuInfo(hideNvidiaOpenCLDevices: true);
                int gpuNo = 0;
                foreach (var device in deviceList)
                {
                    var rowDef = new RowDefinition();
                    rowDef.Height = GridLength.Auto;
                    //grdGpuList.RowDefinitions.Add(rowDef);
                    //AddSingleGpuInfo(device, gpuNo);
                    gpuNo += 1;
                }
            }

            GlobalApplicationState.Instance.ApplicationStateChanged += OnGlobalApplicationStateChanged;

            txRunOnSelectedCards.Text = settings.MinerSelectedGPUIndices;

            brdAdvanced.Height = 33;
            btnStopBenchmark.Visibility = Visibility.Collapsed;

            SetActiveNavigationType(NavigationType.NavigationType2);
        }

        Dictionary<int, GpuEntryUI> _entries = new Dictionary<int, GpuEntryUI>();
        bool _requestExit = false;


        private void ResetGpuList()
        {
            // grdGpuList.Children.Clear();
        }


        public void SetActiveNavigationType(NavigationType navigationType)
        {
            if (navigationType == NavigationType.NavigationType1)
            {
                brdNavigationType1.Visibility = Visibility.Visible;
                brdNavigationType2.Visibility = Visibility.Collapsed;
            }
            if (navigationType == NavigationType.NavigationType2)
            {
                brdNavigationType1.Visibility = Visibility.Collapsed;
                brdNavigationType2.Visibility = Visibility.Visible;
            }
        }

        private void ChangeGpuEnabled()
        {
            /*
            var benchmarkSettings = SettingsLoader.LoadBenchmarkFromFileOrDefault();

            if (benchmarkSettings != null && benchmarkSettings.liveStatus != null)
            {
                bool changed = false;
                foreach (var gpu in benchmarkSettings.liveStatus.GPUs)
                {
                    int gpuNo = gpu.Key;
                    GpuEntryUI? currentEntry = null;
                    if (_entries.ContainsKey(gpuNo))
                    {
                        currentEntry = _entries[gpuNo];
                        if (currentEntry != null)
                        {
                           // gpu.Value.IsEnabledByUser = currentEntry.cbEnableMining.IsChecked ?? false;
                            changed = true;
                        }
                    }
                }
                if (changed)
                {
                    SettingsLoader.SaveBenchmarkToFile(benchmarkSettings);
                    UpdateBenchmarkStatus(benchmarkSettings.liveStatus, );
                }
            }*/

        }

        private void UpdateBenchmarkStatus(ClaymoreLiveStatus? s, bool allExpectedGPUsFound)
        {
            if (s == null)
            {
                return;
            }
            //s.BenchmarkTotalSpeed;
            if (s.GPUInfosParsed && s.AreAllDagsFinishedOrFailed())
            {
                gpuMiningPanel.lblBenchmarkTime.Content = $"Measuring performance {s.NumberOfClaymorePerfReports}/{s.TotalClaymoreReportsBenchmark}";
            }
            if (s.BenchmarkFinished)
            {
                gpuMiningPanel.lblTimeElapsed.Content = $"Finished";
                svgBenchmarkWorking.Visibility = Visibility.Hidden;
                svgBenchmarkEnjoy.Visibility = Visibility.Visible;
            }

            if (!allExpectedGPUsFound)
            {
                return;
            }

            float totalMhs = 0;
            foreach (var gpu in s.GPUs)
            {
                int gpuNo = gpu.Key;
                var gpuInfo = gpu.Value;

                GpuEntryUI? currentEntry = null;
                if (_entries.ContainsKey(gpuNo))
                {
                    currentEntry = _entries[gpuNo];
                }

                string gdetails = gpuInfo.gpuName ?? "";
                if (!String.IsNullOrEmpty(gdetails) && !_entries.ContainsKey(gpuNo))
                {
                    var rowDef = new RowDefinition();
                    rowDef.Height = GridLength.Auto;
                    //grdGpuList.RowDefinitions.Add(rowDef);
                    currentEntry = gpuMiningPanel.AddGpuEntry(gdetails);
                    _entries.Add(gpuNo, currentEntry);
                }
                currentEntry?.SetEnableByUser(gpuInfo.IsEnabledByUser);
                currentEntry?.SetMiningSpeed(gpuInfo.BenchmarkSpeed);
                if (!gpuInfo.IsEnabledByUser)
                {
                    //do nothing
                    //currentEntry?.SetEnableByUser(false);
                }
                else if (s.BenchmarkFinished)
                {
                    currentEntry?.SetFinished(gpuInfo.GPUError);
                    totalMhs += gpuInfo.BenchmarkSpeed;
                }
                else
                {
                    if (!allExpectedGPUsFound)
                    {
                        break;
                    }

                    if (s.GPUInfosParsed && s.AreAllDagsFinishedOrFailed())
                    {
                        currentEntry?.SetMiningProgress((float)s.NumberOfClaymorePerfReports / (float)s.TotalClaymoreReportsBenchmark);
                    }
                    else
                    {
                        currentEntry?.SetDagProgress(gpuInfo.DagProgress);
                    }

                    if (gpuInfo.GPUError != null)
                    {
                        currentEntry?.SetFinished(gpuInfo.GPUError);
                    }

                    totalMhs += gpuInfo.BenchmarkSpeed;
                }


                /*
                if (currentEntry != null)
                {
                    if (currentEntry.lblProgress != null)
                    {
                        currentEntry.lblProgress.Content = gpuInfo.DagProgress.ToString();
                    }
                    if (currentEntry.lblPower != null)
                    {
                        currentEntry.lblPower.Content = gpuInfo.BenchmarkSpeed.ToString();
                    }
                }*/
            }
            gpuMiningPanel.lblPower.Content = totalMhs.ToString() + "MH/s";
            gpuMiningPanel.lblEstimated.Content = String.Format("Estimated profit: {0:0.00} USD/day", totalMhs * 0.05);
            //gpuMiningPanel.SetBenchmarkProgress(s.GetEstimatedBenchmarkProgress());
        }



        private void BenchmarkFinished()
        {
            BenchmarkResults benchmark = SettingsLoader.LoadBenchmarkFromFileOrDefault();

            string claymoreString = "";
            if (benchmark.liveStatus != null && benchmark.liveStatus.GPUs != null)
            {
                foreach (var gpu in benchmark.liveStatus.GPUs)
                {
                    if (gpu.Value.IsReadyForMining())
                    {
                        if (gpu.Value.gpuNo > 0 && gpu.Value.gpuNo < 10)
                        {
                            if (claymoreString != "")
                            {
                                claymoreString += ",";
                            }
                            claymoreString += gpu.Value.gpuNo.ToString();
                        }
                    }
                }
            }

            if (String.IsNullOrEmpty(txRunOnSelectedCards.Text))
            {
                txRunOnSelectedCards.Text = claymoreString;
                SaveSelectedCardsSetting();
            }


            this.btnStartBenchmark.Visibility = Visibility.Visible;
            this.btnStopBenchmark.Visibility = Visibility.Collapsed;

            this.btnNext.IsEnabled = true;

            UpdateBenchmarkStatus(benchmark.liveStatus, true);
            GlobalApplicationState.Instance.NotifyApplicationStateChanged(this, GlobalApplicationStateAction.benchmarkStopped);
        }

        private void SaveSelectedCardsSetting()
        {
            LocalSettings ls = SettingsLoader.LoadSettingsFromFileOrDefault();
            if (txRunOnSelectedCards.Text != ls.MinerSelectedGPUIndices)
            {
                ls.MinerSelectedGPUIndices = txRunOnSelectedCards.Text;
                SettingsLoader.SaveSettingsToFile(ls);
            }
        }


        private async void btnStartBenchmark_Click(object sender, RoutedEventArgs e)
        {
            svgBenchmarkWorking.Visibility = Visibility.Visible;
            svgBenchmarkEnjoy.Visibility = Visibility.Hidden;



            DateTime benchmarkStartTime = DateTime.Now;
            double? initializeTime = null;
            LocalSettings ls = SettingsLoader.LoadSettingsFromFileOrDefault();

            string? ethAddress = ls.EthAddress;
            if (string.IsNullOrEmpty(ethAddress))
            {
                MessageBox.Show("WARNING, No ethereum address provided, default address will be used");
                ethAddress = "0xD593411F3E6e79995E787b5f81D10e12fA6eCF04";
            }
            ClaymoreLiveStatus? baseLiveStatus = null;

            string selectedIndices = txRunOnSelectedCards.Text;
            string niceness = txNiceness.Text;
            SaveSelectedCardsSetting();

            //BenchmarkDialog dB = new BenchmarkDialog();
            //dB.ShowDialog();
            GlobalApplicationState.Instance.NotifyApplicationStateChanged(this, GlobalApplicationStateAction.benchmarkStarted);


            _entries.Clear();
            gpuMiningPanel.ClearGpusEntries();
            _requestExit = false;


            this.btnStartBenchmark.Visibility = Visibility.Collapsed;
            this.btnStopBenchmark.Visibility = Visibility.Visible;
            this.btnNext.IsEnabled = false;

            gpuMiningPanel.lblGatherGpuInfo.Content = $"Scanning GPUs...";
            gpuMiningPanel.lblGatherGpuInfo.Foreground = Brushes.White;

            ClaymoreBenchmark cc = new ClaymoreBenchmark();

            bool result = cc.RunBenchmarkRecording(@"test.recording");
            if (result)
            {
                MessageBox.Show(GlobalApplicationState.Instance.Dashboard, "WARNING: Running test recording. Remove test.recording to run real benchmark.");
            }
            if (!result)
            {
                cc.RunPreBenchmark();
                while (!cc.PreBenchmarkFinished)
                {
                    await Task.Delay(30);
                    baseLiveStatus = cc.ClaymoreParserPreBenchmark.GetLiveStatusCopy();
                    if (baseLiveStatus.GPUInfosParsed)
                    {
                        cc.Stop();
                        break;
                    }
                }
                gpuMiningPanel.lblGatherGpuInfo.Foreground = Brushes.White;
                gpuMiningPanel.lblGatherGpuInfo.Content = String.Format("GPU Info acquired {0:0.00}s", (DateTime.Now - benchmarkStartTime).TotalSeconds);
                gpuMiningPanel.lblTimeElapsed.Content = String.Format("Time elapsed: {0:0.00}s", (DateTime.Now - benchmarkStartTime).TotalSeconds);

                UpdateBenchmarkStatus(baseLiveStatus, false);

                await Task.Delay(30);


                result = cc.RunBenchmark(selectedIndices, niceness, GlobalSettings.DefaultProxy, ethAddress);
                if (!result)
                {
                    MessageBox.Show(cc.BenchmarkError);
                }
            }
            while (!cc.BenchmarkFinished)
            {
                gpuMiningPanel.lblTimeElapsed.Content = String.Format("Time elapsed: {0:0.0}s", (DateTime.Now - benchmarkStartTime).TotalSeconds);
                await Task.Delay(100);
                if (_requestExit)
                {
                    cc.Stop();
                    gpuMiningPanel.lblTimeElapsed.Content = "Stopped";
                    BenchmarkFinished();
                    return;
                }

                //function returns copy, so we can work with safety (even if original value is updated on separate thread)
                var s = cc.ClaymoreParserBenchmark.GetLiveStatusCopy();
                bool allExpectedGPUsFound = false;
                if (baseLiveStatus != null)
                {
                    s.MergeFromBaseLiveStatus(baseLiveStatus, selectedIndices, out allExpectedGPUsFound);
                }
                if (initializeTime == null && s.AreAllDagsFinishedOrFailed() && allExpectedGPUsFound)
                {
                    initializeTime = (DateTime.Now - benchmarkStartTime).TotalSeconds;
                    gpuMiningPanel.lblInitializeTime.Content = String.Format("Prepare mining time: {0:0.0}s", initializeTime);
                    gpuMiningPanel.lblBenchmarkTime.Foreground = Brushes.White;
                }

                if (!s.AreAllDagsFinishedOrFailed())
                {
                    gpuMiningPanel.lblInitializeTime.Content = String.Format("Initializing: {0:0.0}s", (DateTime.Now - benchmarkStartTime).TotalSeconds);
                    gpuMiningPanel.lblInitializeTime.Foreground = Brushes.White;
                    gpuMiningPanel.lblBenchmarkTime.Content = "Measuring performance";
                }

                if (s.NumberOfClaymorePerfReports >= s.TotalClaymoreReportsBenchmark)
                {
                    cc.Stop();
                    gpuMiningPanel.FinishBenchmark(true);
                    {
                        BenchmarkResults res = new BenchmarkResults();
                        res.liveStatus = s;
                        s.BenchmarkFinished = true;
                        SettingsLoader.SaveBenchmarkToFile(res);
                    }
                    UpdateBenchmarkStatus(s, allExpectedGPUsFound);
                    BenchmarkFinished();

                    return;
                }
                UpdateBenchmarkStatus(s, allExpectedGPUsFound);


            }

        }

        public void RequestBenchmarkEnd()
        {
            //to do proper block until killed (probably with timeout)
            _requestExit = true;
        }

        private void btnStopBenchmark_Click(object sender, RoutedEventArgs e)
        {
            _requestExit = true;
        }

        public void OnGlobalApplicationStateChanged(object sender, GlobalApplicationStateEventArgs? args)
        {
            if (args != null)
            {
                switch (args.action)
                {
                    case GlobalApplicationStateAction.benchmarkSettingsChanged:
                        ChangeGpuEnabled();
                        break;
                }
            }
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (double.IsNaN(brdAdvanced.Height))
            {
                brdAdvanced.Height = 33;
            }
            else
            {
                brdAdvanced.Height = double.NaN;
            }
        }

        private void btnReady_Click(object sender, RoutedEventArgs e)
        {
            GlobalApplicationState.Instance.Dashboard?.SwitchPage(DashboardPages.PageDashboardMain);
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            GlobalApplicationState.Instance.Dashboard?.SwitchPageBack();
        }
    }
}
