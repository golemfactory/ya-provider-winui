using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GolemUI.Claymore;

namespace GolemUI.DesignViewModel
{
    public class MockupSettingsViewModel
    {
        public double? ExpectedProfit => 300;

        public float? Hashrate => 78.0f;

        public ObservableCollection<ClaymoreGpuStatus> GpuList { get; } = new ObservableCollection<ClaymoreGpuStatus>(new ClaymoreGpuStatus[]
        {
            new Claymore.ClaymoreGpuStatus(1,true,0)
                {
                    GpuName = "AMD Radeon R9 200 Series (pcie 1), OpenCL 2.0, 8 GB VRAM, 44 CUs",
                    BenchmarkSpeed = 15.6f,
                    DagProgress = 1.0f
                },
            new Claymore.ClaymoreGpuStatus(2,true,0)
                {
                    GpuName = "Radeon RX 5500 XT (pcie 8), OpenCL 2.0, 8 GB VRAM, 22 CUs"
                },
            new Claymore.ClaymoreGpuStatus(3,true,0)
                {
                    GpuName = "Radeon RX 5500 XT (pcie 8), OpenCL 2.0, 8 GB VRAM, 22 CUs",
                    DagProgress = 0.5f,
                    GPUError = "Fail"
                }
        });

        public bool IsMiningActive { get; set; } = true;

        public bool IsCpuActive { get; set; } = false;


        public string? NodeName { get; set; } = "SuperNode1";

        public int ActiveCpusCount { get; set; } = 3;

        public int TotalCpusCount => 7;

        public bool BenchmarkIsRunning => false;

        public string BenchmarkError { get; set; } = "Bechmark failed with error";

    }
}
