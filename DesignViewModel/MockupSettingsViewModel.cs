using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GolemUI.Miners;

namespace GolemUI.DesignViewModel
{
    public class MockupSettingsViewModel
    {
        public double? ExpectedProfit => 41.32f;

        public float? Hashrate => 10.19f;
        public double CpuOpacity => IsCpuMiningEnabledByNetwork ? 1.0 : 0.2f;
        public bool IsCpuMiningEnabledByNetwork => false;

        public ObservableCollection<BenchmarkGpuStatus> GpuList { get; } = new ObservableCollection<BenchmarkGpuStatus>(new BenchmarkGpuStatus[]
        {
            new BenchmarkGpuStatus(1,true,0)
                {
                    GpuName = "AMD Radeon R9 200 Series (pcie 1), OpenCL 2.0, 8 GB VRAM, 44 CUs",
                    BenchmarkSpeed = 15.6f,
                    DagProgress = 1.0f
                },
            new BenchmarkGpuStatus(2,true,0)
                {
                    GpuName = "Radeon RX 5500 XT (pcie 8), OpenCL 2.0, 8 GB VRAM, 22 CUs"
                },
            new BenchmarkGpuStatus(3,true,0)
                {
                    GpuName = "Radeon RX 5500 XT (pcie 8), OpenCL 2.0, 8 GB VRAM, 22 CUs",
                    DagProgress = 0.5f,
                    GPUError = "Fail"
                }
        });

        public bool IsMiningActive { get; set; } = true;

        public bool IsCpuActive { get; set; } = false;


        public string? NodeName { get; set; } = "SuperNode1";

        public string ActiveCpusCountAsString => 3.ToString();
        public int ActiveCpus => 3;

        public string TotalCpusCountAsString => 7.ToString();

        public bool BenchmarkIsRunning => true;

        public string BenchmarkError { get; set; } = "Bechmark failed with error";

    }
}
