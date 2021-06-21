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
using GolemUI.Claymore;

namespace GolemUI
{



    /// <summary>
    /// Interaction logic for GolemUISettingsWindow.xaml
    /// </summary>
    public partial class GolemUISettingsWindow : Window
    {

        Dictionary<int, GpuEntryUI> _entries = new Dictionary<int, GpuEntryUI>();

        bool _requestExit = false;
        LocalSettings? localSettings = null;

        public GolemUISettingsWindow()
        {
   

            InitializeComponent();

            localSettings = SettingsLoader.LoadSettingsFromFileOrDefault();
            var benchmarkSettings = SettingsLoader.LoadBenchmarkFromFileOrDefault();
            if (benchmarkSettings != null && benchmarkSettings.liveStatus != null)
            {
               // UpdateBenchmarkStatus(benchmarkSettings.liveStatus);
            }


            txNodeName.Text = localSettings.NodeName;
            txWalletAddress.Text = localSettings.EthAddress;
            txSubnet.Text = localSettings.Subnet;
           
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


        private void btnStopBenchmark_Click(object sender, RoutedEventArgs e)
        {
            _requestExit = true;
        }




        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _requestExit = true;
        }

        private void btnSaveBenchmark_Click(object sender, RoutedEventArgs e)
        {
            LocalSettings settings = new LocalSettings();
            //settings.EthAddress = t
            settings.NodeName = txNodeName.Text;
            settings.EthAddress = txWalletAddress.Text;
            settings.Subnet = txSubnet.Text;
            SettingsLoader.SaveSettingsToFile(settings);
        }

        private void txSubnet_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
