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
        ClaymoreLiveStatus? _currentLiveStatus;

        bool _advancedSettingsVisible = false;

        public GpuMiningPanelUI()
        {
            InitializeComponent();



            LocalSettings settings = SettingsLoader.LoadSettingsFromFileOrDefault();
            var benchmarkSettings = SettingsLoader.LoadBenchmarkFromFileOrDefault();
            if (benchmarkSettings != null && benchmarkSettings.liveStatus != null)
            {
                _currentLiveStatus = benchmarkSettings.liveStatus;
                UpdateBenchmarkStatus(_currentLiveStatus, true);
            }

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
            txBenchmarkLength.Text = settings.BenchmarkLength;
            txNiceness.Text = settings.MinerSelectedGPUsNiceness;
            txPool.Text = settings.CustomPool;
            txEmail.Text = settings.OptionalEmail;
            cbDetailedView.IsChecked = settings.EnableDetailedBenchmarkInfo;

            SetAdvancedSettingsVisibility(_advancedSettingsVisible);
            btnStopBenchmark.Visibility = Visibility.Collapsed;
            grdMainBenchmark.ColumnDefinitions[3].Width = new GridLength(100);
        }

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

        

        public void OnGlobalApplicationStateChanged(object sender, GlobalApplicationStateEventArgs? args)
        {
            if (args != null)
            {
                switch (args.action)
                {
                    case GlobalApplicationStateAction.benchmarkSettingsChanged:
                        //ChangeGpuEnabled();
                        break;
                }
            }
        }

        private void UpdateBenchmarkStatus(ClaymoreLiveStatus? s, bool allExpectedGPUsFound)
        {
            if (s == null)
            {
                return;
            }

            bool isDetailedInfo = this.cbDetailedView.IsChecked ?? false;

            if (!isDetailedInfo)
            {
                this.lblTimeElapsed.Visibility = Visibility.Collapsed;
                this.lblGatherGpuInfo.Visibility = Visibility.Collapsed;
                this.lblInitializeTime.Visibility = Visibility.Collapsed;
                this.lblBenchmarkTime.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.lblTimeElapsed.Visibility = Visibility.Visible;
                this.lblGatherGpuInfo.Visibility = Visibility.Visible;
                this.lblInitializeTime.Visibility = Visibility.Visible;
                this.lblBenchmarkTime.Visibility = Visibility.Visible;
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
                if (currentEntry != null)
                {
                    currentEntry.ExtendedView = isDetailedInfo;
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
            this.lblPower.Content = String.Format("{0:0.00}MH/s", totalMhs);
            this.lblEstimated.Content = String.Format("Estimated profit: {0:0.00} USD/day", totalMhs * 0.05);
            //this.SetBenchmarkProgress(s.GetEstimatedBenchmarkProgress());


        }


        private void SaveSelectedCardsSetting()
        {
            LocalSettings ls = SettingsLoader.LoadSettingsFromFileOrDefault();
            if (txRunOnSelectedCards.Text != ls.MinerSelectedGPUIndices
                || txNiceness.Text != ls.MinerSelectedGPUsNiceness
                || txBenchmarkLength.Text != ls.BenchmarkLength
                || ls.EnableDetailedBenchmarkInfo != (cbDetailedView.IsChecked ?? false)
                || ls.CustomPool != txPool.Text
                || ls.OptionalEmail != txEmail.Text)
            {
                ls.MinerSelectedGPUIndices = txRunOnSelectedCards.Text;
                ls.MinerSelectedGPUsNiceness = txNiceness.Text;
                ls.BenchmarkLength = txBenchmarkLength.Text;
                ls.EnableDetailedBenchmarkInfo = cbDetailedView.IsChecked ?? false;
                ls.OptionalEmail = txEmail.Text;
                ls.CustomPool = txPool.Text;

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

            string ethAddress = ls.EthAddress ?? "";
            if (string.IsNullOrEmpty(ethAddress))
            {
                MessageBox.Show("WARNING, No ethereum address provided, default address will be used");
                ethAddress = "0xD593411F3E6e79995E787b5f81D10e12fA6eCF04";
            }
            ClaymoreLiveStatus? baseLiveStatus = null;

            string poolAddr = GlobalSettings.DefaultProxy;
            if (!string.IsNullOrEmpty(txPool.Text))
            {
                poolAddr = txPool.Text;
                ethAddress += "/" + ls.NodeName;
                
                if (!string.IsNullOrEmpty(txEmail.Text))
                {
                    ethAddress += "/" + txEmail.Text;
                }
            }

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

            int totalClaymoreReportsNeeded = 5;

            int outVal;
            if (int.TryParse(this.txBenchmarkLength.Text, out outVal))
            {
                if (outVal >= 5)
                {
                    totalClaymoreReportsNeeded = outVal;
                }
            }
            ClaymoreBenchmark cc = new ClaymoreBenchmark(totalClaymoreReportsNeeded);
            

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
                    _currentLiveStatus = cc.ClaymoreParserPreBenchmark.GetLiveStatusCopy();
                    baseLiveStatus = _currentLiveStatus;
                    if (_currentLiveStatus.GPUInfosParsed)
                    {
                        cc.Stop();
                        break;
                    }
                }
                this.lblGatherGpuInfo.Foreground = Brushes.White;
                this.lblGatherGpuInfo.Content = String.Format("GPU Info acquired {0:0.00}s", (DateTime.Now - benchmarkStartTime).TotalSeconds);
                this.lblTimeElapsed.Content = String.Format("Time elapsed: {0:0.00}s", (DateTime.Now - benchmarkStartTime).TotalSeconds);

                UpdateBenchmarkStatus(_currentLiveStatus, false);

                await Task.Delay(30);


                result = cc.RunBenchmark(selectedIndices, niceness, poolAddr, ethAddress);
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
                var _currentLiveStatus = cc.ClaymoreParserBenchmark.GetLiveStatusCopy();
                bool allExpectedGPUsFound = false;
                if (baseLiveStatus != null)
                {
                    _currentLiveStatus.MergeFromBaseLiveStatus(baseLiveStatus, selectedIndices, out allExpectedGPUsFound);
                }
                if (initializeTime == null && _currentLiveStatus.AreAllDagsFinishedOrFailed() && allExpectedGPUsFound)
                {
                    initializeTime = (DateTime.Now - benchmarkStartTime).TotalSeconds;
                    this.lblInitializeTime.Content = String.Format("Prepare mining time: {0:0.0}s", initializeTime);
                    this.lblBenchmarkTime.Foreground = Brushes.White;
                }

                if (!_currentLiveStatus.AreAllDagsFinishedOrFailed())
                {
                    this.lblInitializeTime.Content = String.Format("Initializing: {0:0.0}s", (DateTime.Now - benchmarkStartTime).TotalSeconds);
                    this.lblInitializeTime.Foreground = Brushes.White;
                    this.lblBenchmarkTime.Content = "Measuring performance";
                }

                if (_currentLiveStatus.NumberOfClaymorePerfReports >= _currentLiveStatus.TotalClaymoreReportsBenchmark)
                {
                    cc.Stop();
                    this.FinishBenchmark(true);
                    {
                        BenchmarkResults res = new BenchmarkResults();
                        res.liveStatus = _currentLiveStatus;
                        _currentLiveStatus.BenchmarkFinished = true;
                        SettingsLoader.SaveBenchmarkToFile(res);
                    }
                    UpdateBenchmarkStatus(_currentLiveStatus, allExpectedGPUsFound);
                    BenchmarkFinished();

                    return;
                }
                UpdateBenchmarkStatus(_currentLiveStatus, allExpectedGPUsFound);


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

        private void SetAdvancedSettingsVisibility(bool visible)
        {
            if (visible)
            {
                brdAdvanced.Height = double.NaN;
                grdMainBenchmark.ColumnDefinitions[0].Width = new GridLength(200, GridUnitType.Star);
                grdMainBenchmark.ColumnDefinitions[0].MinWidth = 100;
                grdMainBenchmark.ColumnDefinitions[1].Width = new GridLength(250, GridUnitType.Star);
                grdMainBenchmark.ColumnDefinitions[1].MinWidth = 200;
                grdMainBenchmark.ColumnDefinitions[1].MaxWidth = 300;
                grdMainBenchmark.ColumnDefinitions[2].Width = new GridLength(150);
                grdMainBenchmark.ColumnDefinitions[2].MinWidth = 170;
                grdMainBenchmark.ColumnDefinitions[3].Width = new GridLength(220, GridUnitType.Star);
                grdMainBenchmark.ColumnDefinitions[3].MinWidth = 220;
            }
            else
            {
                brdAdvanced.Height = 33;
                grdMainBenchmark.ColumnDefinitions[0].Width = new GridLength(200, GridUnitType.Star);
                grdMainBenchmark.ColumnDefinitions[0].MinWidth = 200;
                grdMainBenchmark.ColumnDefinitions[0].MaxWidth = 300;
                grdMainBenchmark.ColumnDefinitions[1].Width = new GridLength(250, GridUnitType.Star);
                grdMainBenchmark.ColumnDefinitions[1].MinWidth = 200;
                grdMainBenchmark.ColumnDefinitions[1].MaxWidth = 300;
                grdMainBenchmark.ColumnDefinitions[2].Width = new GridLength(150);
                grdMainBenchmark.ColumnDefinitions[2].MinWidth = 170;
                grdMainBenchmark.ColumnDefinitions[3].Width = new GridLength(80, GridUnitType.Star);
                grdMainBenchmark.ColumnDefinitions[3].MinWidth = 100;
            }
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _advancedSettingsVisible = !_advancedSettingsVisible;
            SetAdvancedSettingsVisibility(_advancedSettingsVisible);
        }

        private void DetailViewChanged()
        {
            UpdateBenchmarkStatus(_currentLiveStatus, true);
        }

        private void cbDetailedView_Checked(object sender, RoutedEventArgs e)
        {
            DetailViewChanged();
        }

        private void cbDetailedView_Unchecked(object sender, RoutedEventArgs e)
        {
            DetailViewChanged();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void ShowCardSelectInfo(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Select which cards you want to run benchmark and mining on). For example 1,3,4 means run on first, third and fourth card. Run benchmark to confirm that your selection is valid. Leave the field empty to run on all cards");
        }

        private void ShowNicenessInfo(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Enter niceness parameters for selected cards 0 means full power, 1 means almost full power. Separate with coma for specify cards.");
        }

        private void ShowDetailedInfo(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Additional progress information during benchmark");
        }

        private void cbShowAdvanced_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void cbShowAdvanced_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void ShowBenchLengthInfo(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Benchmark length (one unit is around 10 seconds). Use this option to check if your mining setup is stable. You can also check if it's not too taxing on your computer, you can modify niceness parameter if that's the case.");
        }

        private void ShowCustomPoolInfo(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("If you don't like running benchmark with default pool, you can set pool of your choise");
        }

        private void ShowEmailInfo(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Optionally add your email");
        }
    }
}
