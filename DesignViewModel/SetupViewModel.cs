using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.DesignViewModel
{
    public class SetupViewModel
    {
        public bool IsDesingMode { get; }

        public int Flow { get; set; }

        public int NoobStep { get; set; }

        public string[]? MnemonicWords { get; set; } = new string[] { "ala", "ma", "kota" };

        public Claymore.ClaymoreGpuStatus[] GPUs { get; set; }

        public float? TotalHashRate { get; set; } = 55.2f;

        public bool BenchmarkIsRunning { get; set; } = true;

        public double? ExpectedProfit { get; set; } = 150.33;

        public SetupViewModel()
        {
            IsDesingMode = true;
            Flow = 0;
            NoobStep = 0;
            GPUs = new Claymore.ClaymoreGpuStatus[]
            {
                new Claymore.ClaymoreGpuStatus(1)
                {
                    gpuName = "AMD Radeon R9 200 Series (pcie 1), OpenCL 2.0, 8 GB VRAM, 44 CUs",
                    BenchmarkSpeed = 15.6f,
                    DagProgress = 1.0f

                },
                new Claymore.ClaymoreGpuStatus(2)
                {
                    gpuName = "Radeon RX 5500 XT (pcie 8), OpenCL 2.0, 8 GB VRAM, 22 CUs"
                }
            };
        }
    }
}
