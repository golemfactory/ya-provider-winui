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
            var deviceList = gic.GetGpuInfo();
            int gpuNo = 0;
            foreach (var device in deviceList)
            {
                var rowDef = new RowDefinition();
                rowDef.Height = GridLength.Auto;
                grdMining.RowDefinitions.Add(rowDef);
                AddSignleGpuInfo(device, gpuNo);
                gpuNo += 1;
            }

            //int count = gpuInfo.Count;
        }

        private void ResetGpuList()
        {
            grdMining.Children.Clear();


        }



        private void AddSignleGpuInfo(ComputeDevice info, int gpuNo)
        {
            Label lblName = new Label();
            lblName.Content = info.Name;

            grdMining.Children.Add(lblName);

            Grid.SetColumn(lblName, 0);
            Grid.SetRow(lblName, gpuNo);

            Label lblVendor = new Label();
            lblVendor.Content = info.Vendor;

            grdMining.Children.Add(lblVendor);

            Grid.SetColumn(lblVendor, 1);
            Grid.SetRow(lblVendor, gpuNo);

            Label lblMemory = new Label();
            lblMemory.Content = info.Memory.ToString();

            grdMining.Children.Add(lblMemory);

            Grid.SetColumn(lblMemory, 2);
            Grid.SetRow(lblMemory, gpuNo);


        }
    }
}
