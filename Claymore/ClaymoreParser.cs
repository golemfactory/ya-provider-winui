using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Claymore
{

    class ClaymoreGpuStatus
    {
        public int gpuNo { get; set; }
        public bool OutOfMemory { get; set; }
        public bool GPUNotFound { get; set; }
        public float BenchmarkSpeed { get; set; }
        public bool IsDagCreating { get; set; }
        public float DagProgress { get; set; }
    }

    class ClaymoreLiveStatus
    {
        public string BenchmarkError = "";

        Dictionary<int, ClaymoreGpuStatus> _gpus = new Dictionary<int, ClaymoreGpuStatus>();

        
        public bool GPUNotFound;
        public bool OutOfMemory;
        public bool BenchmarkFinished;
        public float BenchmarkProgress;
        public float BenchmarkSpeed;
    }



}
