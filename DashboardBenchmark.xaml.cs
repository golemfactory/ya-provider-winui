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
    /// Interaction logic for DashboardBenchmark.xaml
    /// </summary>
    public partial class DashboardBenchmark : UserControl
    {
        public DashboardBenchmark()
        {
            InitializeComponent();

            localSettings = SettingsLoader.LoadSettingsFromFileOrDefault();
            var benchmarkSettings = SettingsLoader.LoadBenchmarkFromFileOrDefault();
            if (benchmarkSettings != null && benchmarkSettings.liveStatus != null)
            {
                UpdateBenchmarkStatus(benchmarkSettings.liveStatus);
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
        }

        Dictionary<int, GpuEntryUI> _entries = new Dictionary<int, GpuEntryUI>();
        bool _requestExit = false;
        LocalSettings? localSettings = null;



        private void ResetGpuList()
        {
           // grdGpuList.Children.Clear();


        }


        private void UpdateBenchmarkStatus(ClaymoreLiveStatus s)
        {
            //s.BenchmarkTotalSpeed;
            if (s.GPUInfosParsed && s.AreAllDagsFinishedOrFailed())
            {
                gpuMiningPanel.lblStatus.Content = $"Measuring performance {s.NumberOfClaymorePerfReports}/{s.TotalClaymoreReportsBenchmark}";
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

                string gdetails = gpuInfo.GPUDetails ?? "";
                if (!String.IsNullOrEmpty(gdetails) && !_entries.ContainsKey(gpuNo))
                {
                    var rowDef = new RowDefinition();
                    rowDef.Height = GridLength.Auto;
                    //grdGpuList.RowDefinitions.Add(rowDef);
                    currentEntry = gpuMiningPanel.AddGpuEntry(gdetails, (gpuNo - 1).ToString());
                    _entries.Add(gpuNo, currentEntry);
                }
                currentEntry?.SetDagProgress(gpuInfo.DagProgress);
                currentEntry?.SetMiningSpeed(gpuInfo.BenchmarkSpeed);


                totalMhs += gpuInfo.BenchmarkSpeed;
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
            gpuMiningPanel.lblPower.Content = totalMhs.ToString();
            gpuMiningPanel.SetBenchmarkProgress(s.GetEstimatedBenchmarkProgress());
        }

        private void BenchmarkFinished()
        {
            this.btnStartBenchmark.IsEnabled = true;
            this.btnStopBenchmark.IsEnabled = false;

        }

        private async void btnStartBenchmark_Click(object sender, RoutedEventArgs e)
        {
            //BenchmarkDialog dB = new BenchmarkDialog();
            //dB.ShowDialog();
            _requestExit = false;
            btnStartBenchmark.IsEnabled = false;
            btnStopBenchmark.IsEnabled = true;

            gpuMiningPanel.lblStatus.Content = $"Preparing...";

            ClaymoreBenchmark cc = new ClaymoreBenchmark(gpuNo: null);
            bool result = cc.RunBenchmark();
            if (!result)
            {
                MessageBox.Show(cc.BenchmarkError);
            }
            while (!cc.BenchmarkFinished)
            {
                await Task.Delay(100);
                if (_requestExit)
                {
                    cc.Stop();
                    gpuMiningPanel.lblStatus.Content = "Stopped";
                    BenchmarkFinished();
                    return;
                }

                //function returns copy, so we can work with safety (even if original value is updated on separate thread)
                var s = cc.ClaymoreParser.GetLiveStatusCopy();

                UpdateBenchmarkStatus(s);

                if (s.NumberOfClaymorePerfReports >= s.TotalClaymoreReportsBenchmark)
                {
                    cc.Stop();
                    gpuMiningPanel.FinishBenchmark(true);
                    BenchmarkFinished();
                    {
                        BenchmarkResults res = new BenchmarkResults();
                        res.liveStatus = s;
                        SettingsLoader.SaveBenchmarkToFile(res);
                    }

                    return;
                }


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
    }
}
