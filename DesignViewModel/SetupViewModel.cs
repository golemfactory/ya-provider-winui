using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GolemUI.DesignViewModel
{
    public class SetupViewModel
    {
        public bool IsDesingMode { get; }

        public int Flow { get; set; }

        public int NoobStep { get; set; }

        public string[]? MnemonicWords { get; set; } = new string[] { "Bulb", "Flower", "Bump", "Thick", "Leaf", "Yeah", "Neon" };

        public Claymore.ClaymoreGpuStatus[] GPUs { get; set; }

        public float? TotalHashRate { get; set; } = 55.2f;

        public bool BenchmarkIsRunning { get; set; } = true;

        public string BenchmarkError { get; set; } = "Benchmark failed with error\nTest multiline\nerrors";

        public double? ExpectedProfit { get; set; } = 150.33;

        public Visibility BackButtonVisibility => Visibility.Visible;
        public SetupViewModel()
        {
            IsDesingMode = true;
            Flow = 0;
            NoobStep = 0;
            GPUs = new Claymore.ClaymoreGpuStatus[]
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
            };
        }
    }
}
