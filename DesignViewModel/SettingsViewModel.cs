using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.DesignViewModel
{
    public class SettingsViewModel
    {
        public double? EstimatedProfit => 300;

        public float? Hashrate => 78.0f;

        public ObservableCollection<SingleGpuDescriptor> GpuList { get; } = new ObservableCollection<SingleGpuDescriptor>(new SingleGpuDescriptor[]
        {

            new SingleGpuDescriptor(1, "1st GPU", 20.12f, false, true,0,false),
            new SingleGpuDescriptor(2, "second GPU", 12.10f, true, false,5,true),
            new SingleGpuDescriptor(3, "3rd GPU", 9.00f, false, true,10,true)
        });

        public bool IsMiningActive { get; set; } = true;

        public bool IsCpuActive { get; set; } = false;


        public string? NodeName { get; set; } = "SuperNode1";

        public int ActiveCpusCount { get; set; } = 3;

        public int TotalCpusCount => 7;

        public bool BenchmarkIsRunning => false;

    }
}
