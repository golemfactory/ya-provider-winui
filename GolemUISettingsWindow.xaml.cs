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

namespace GolemUI
{


    /// <summary>
    /// Interaction logic for GolemUISettingsWindow.xaml
    /// </summary>
    public partial class GolemUISettingsWindow : Window
    {



        NameGen _gen;
        public GolemUISettingsWindow()
        {
            _gen = new NameGen();
            InitializeComponent();
            txNodeName.Text = _gen.GenerateElvenName() + "-" + _gen.GenerateElvenName();
            //lbGpus.Items.Add("GPU 1");
            //lbGpus.Items.Add("GPU 2");


            GpuInfoCommand gic = new GpuInfoCommand();
            
            var deviceList = gic.GetGpuInfo(hideNvidiaOpenCLDevices: true);
            int gpuNo = 0;
            foreach (var device in deviceList)
            {
                var rowDef = new RowDefinition();
                rowDef.Height = GridLength.Auto;
                grdMining.RowDefinitions.Add(rowDef);
                AddSingleGpuInfo(device, gpuNo);
                gpuNo += 1;
            }

            //int count = gpuInfo.Count;
        }

        private void ResetGpuList()
        {
            grdMining.Children.Clear();


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

            grdMining.Children.Add(lblName);

            Grid.SetColumn(lblName, 0);
            Grid.SetRow(lblName, gpuNo);

            Label lblVendor = new Label();
            lblVendor.Content = info.Vendor;
            lblVendor.Background = backgroundBrush;

            if (!canMine)
            {
                lblName.Background = Brushes.Salmon;
            }

            grdMining.Children.Add(lblVendor);

            Grid.SetColumn(lblVendor, 1);
            Grid.SetRow(lblVendor, gpuNo);

            Label lblMemory = new Label();
            string strVal = String.Format(CultureInfo.InvariantCulture, "{0:0.02} GB", (double)info.Memory / 1024.0 / 1024.0 / 1024.0);
            lblMemory.Content = strVal;
            lblMemory.Background = backgroundBrush;

            grdMining.Children.Add(lblMemory);

            Grid.SetColumn(lblMemory, 2);
            Grid.SetRow(lblMemory, gpuNo);


        }
    }
}
