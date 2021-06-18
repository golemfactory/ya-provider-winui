using GolemUI.Command;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading.Tasks;
using GolemUI.Settings;

namespace GolemUI
{



    /// <summary>
    /// Interaction logic for GolemUISettingsWindow.xaml
    /// </summary>
    public partial class GolemUISettingsWindow : Window
    {

        Dictionary<int, GpuEntryUI> _entries = new Dictionary<int, GpuEntryUI>();

        bool _requestExit = false;
        NameGen _gen;
        public GolemUISettingsWindow()
        {
            _gen = new NameGen();

            InitializeComponent();
            txNodeName.Text = _gen.GenerateElvenName() + "-" + _gen.GenerateElvenName();
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
                    grdGpuList.RowDefinitions.Add(rowDef);
                    AddSingleGpuInfo(device, gpuNo);
                    gpuNo += 1;
                }
            }

            //int count = gpuInfo.Count;
        }

        private void ResetGpuList()
        {
            grdGpuList.Children.Clear();

        
        }




        private void AddSingleGpuInfo(ComputeDevice info, int gpuNo)
        {
            bool canMine = false;
            if (info.Memory > 4500000000 && info.Vendor != "Intel")
            {
                canMine = true;
            }


            Brush backgroundBrush = Brushes.LightGreen;
            if (!canMine)
            {
                backgroundBrush = Brushes.Salmon;
            }

            Label lblName = new Label();
            lblName.Content = info.Name;

            lblName.Background = backgroundBrush;

            grdGpuList.Children.Add(lblName);

            Grid.SetColumn(lblName, 0);
            Grid.SetRow(lblName, gpuNo);

            Label lblVendor = new Label();
            lblVendor.Content = info.Vendor;
            lblVendor.Background = backgroundBrush;

            if (!canMine)
            {
                lblName.Background = Brushes.Salmon;
            }

            grdGpuList.Children.Add(lblVendor);

            Grid.SetColumn(lblVendor, 1);
            Grid.SetRow(lblVendor, gpuNo);

            Label lblMemory = new Label();
            string strVal = String.Format(CultureInfo.InvariantCulture, "{0:0.02} GB", (double)info.Memory / 1024.0 / 1024.0 / 1024.0);
            lblMemory.Content = strVal;
            lblMemory.Background = backgroundBrush;

            grdGpuList.Children.Add(lblMemory);

            Grid.SetColumn(lblMemory, 2);
            Grid.SetRow(lblMemory, gpuNo);


        }

        private void BenchmarkFinished()
        {
            this.btnStartBenchmark.IsEnabled = true;
            this.btnStopBenchmark.IsEnabled = false;
            
        }

        private void btnStopBenchmark_Click(object sender, RoutedEventArgs e)
        {
            _requestExit = true;
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

                //s.BenchmarkTotalSpeed;
                if (s.GPUInfosParsed && s.AreAllDagsFinishedOrFailed())
                {
                    gpuMiningPanel.lblStatus.Content = $"Measuring performance {s.NumberOfClaymorePerfReports}/{s.TotalClaymoreReportsBenchmark}";
                }
                if (s.NumberOfClaymorePerfReports >= s.TotalClaymoreReportsBenchmark)
                {
                    cc.Stop();
                    gpuMiningPanel.FinishBenchmark(true);
                    BenchmarkFinished();
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

                    string gdetails = gpuInfo.GPUDetails ?? "";
                    if (!String.IsNullOrEmpty(gdetails) && !_entries.ContainsKey(gpuNo))
                    {
                        var rowDef = new RowDefinition();
                        rowDef.Height = GridLength.Auto;
                        grdGpuList.RowDefinitions.Add(rowDef);
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
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _requestExit = true;
        }

        private void btnSaveBenchmark_Click(object sender, RoutedEventArgs e)
        {
            LocalSettings settings = new LocalSettings();
            //settings.EthAddress = t
            SettingsLoader.SaveSettingsToFile(settings);
        }
    }
}
