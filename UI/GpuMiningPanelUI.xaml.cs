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
    /// <summary>
    /// Interaction logic for GpuMiningPanelUI.xaml
    /// </summary>
    public partial class GpuMiningPanelUI : UserControl
    {

        public GpuEntryUI AddGpuEntry(string info)
        {
            //bool canMine = false;
            /*if (info.Memory > 4500000000 && info.Vendor != "Intel")
            {
                canMine = true;
            }*/

            /*
            Brush backgroundBrush = Brushes.LightGreen;
            if (!canMine)
            {
                backgroundBrush = Brushes.Salmon;
            }

            ge.lblName = new Label();
            ge.lblName.Content = info;

            ge.lblName.Background = backgroundBrush;

            grdGpuList.Children.Add(ge.lblName);

            Grid.SetColumn(ge.lblName, 0);
            Grid.SetRow(ge.lblName, gpuNo);

            ge.lblProgress = new Label();
            ge.lblProgress.Content = "Starting...";

            ge.lblProgress.Background = backgroundBrush;
            grdGpuList.Children.Add(ge.lblProgress);

            Grid.SetColumn(ge.lblProgress, 1);
            Grid.SetRow(ge.lblProgress, gpuNo);

            ge.lblPower = new Label();
            ge.lblPower.Content = "N/A";

            ge.lblPower.Background = backgroundBrush;
            grdGpuList.Children.Add(ge.lblPower);

            Grid.SetColumn(ge.lblPower, 2);
            Grid.SetRow(ge.lblPower, gpuNo);
            */

            GpuEntryUI ge = new GpuEntryUI();

            this.spGpuList.Children.Add(ge);
            ge.SetInfo(info);
            return ge;

        }

        Dictionary<int, GpuEntryUI> _entries = new Dictionary<int, GpuEntryUI>();
        bool _requestExit = false;


        public void ClearGpusEntries()
        {
            this.spGpuList.Children.Clear();
        }


        public void SetBenchmarkProgress(float benchmarkProgress)
        {
            if (benchmarkProgress <= 0.0f)
            {
                //pbBenchmark.Value = 0.0f;
            }
            else if (benchmarkProgress >= 1.0f)
            {
                //pbBenchmark.Value = 100.0f;
            }
            else
            {
                //pbBenchmark.Value = benchmarkProgress * 100.0f;
            }
        }
        public void FinishBenchmark(bool success)
        {
            if (success)
            {
                SetBenchmarkProgress(1.0f);
                lblTimeElapsed.Content = "Finished";
                lblTimeElapsed.Foreground = Brushes.White;
            }
            else
            {
                lblTimeElapsed.Content = "Finished - Error";
            }
        }
        public void StopBenchmark()
        {

        }

        public GpuMiningPanelUI()
        {
            InitializeComponent();

            this.brdAdvanced.Height = 33;
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
                this.lblBenchmarkTime.Content = $"Measuring performance {s.NumberOfClaymorePerfReports}/{s.TotalClaymoreReportsBenchmark}";
            }
            if (s.BenchmarkFinished)
            {
                this.lblTimeElapsed.Content = $"Finished";
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
                    currentEntry = this.AddGpuEntry(gdetails);
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
            this.lblPower.Content = totalMhs.ToString() + "MH/s";
            this.lblEstimated.Content = String.Format("Estimated profit: {0:0.00} USD/day", totalMhs * 0.05);
            //this.SetBenchmarkProgress(s.GetEstimatedBenchmarkProgress());
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
            this.ClearGpusEntries();
            _requestExit = false;


            this.btnStartBenchmark.Visibility = Visibility.Collapsed;
            this.btnStopBenchmark.Visibility = Visibility.Visible;
            this.btnNext.IsEnabled = false;

            this.lblGatherGpuInfo.Content = $"Scanning GPUs...";
            this.lblGatherGpuInfo.Foreground = Brushes.White;

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
                this.lblGatherGpuInfo.Foreground = Brushes.White;
                this.lblGatherGpuInfo.Content = String.Format("GPU Info acquired {0:0.00}s", (DateTime.Now - benchmarkStartTime).TotalSeconds);
                this.lblTimeElapsed.Content = String.Format("Time elapsed: {0:0.00}s", (DateTime.Now - benchmarkStartTime).TotalSeconds);

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
                this.lblTimeElapsed.Content = String.Format("Time elapsed: {0:0.0}s", (DateTime.Now - benchmarkStartTime).TotalSeconds);
                await Task.Delay(100);
                if (_requestExit)
                {
                    cc.Stop();
                    this.lblTimeElapsed.Content = "Stopped";
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
                    this.lblInitializeTime.Content = String.Format("Prepare mining time: {0:0.0}s", initializeTime);
                    this.lblBenchmarkTime.Foreground = Brushes.White;
                }

                if (!s.AreAllDagsFinishedOrFailed())
                {
                    this.lblInitializeTime.Content = String.Format("Initializing: {0:0.0}s", (DateTime.Now - benchmarkStartTime).TotalSeconds);
                    this.lblInitializeTime.Foreground = Brushes.White;
                    this.lblBenchmarkTime.Content = "Measuring performance";
                }

                if (s.NumberOfClaymorePerfReports >= s.TotalClaymoreReportsBenchmark)
                {
                    cc.Stop();
                    this.FinishBenchmark(true);
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


        private void btnStopBenchmark_Click(object sender, RoutedEventArgs e)
        {
            _requestExit = true;
        }

        private void btnReady_Click(object sender, RoutedEventArgs e)
        {

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
    }
}
