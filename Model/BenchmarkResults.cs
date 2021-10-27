using GolemUI.Claymore;
using GolemUI.TRex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Model
{
    public class BenchmarkResults
    {
        public int BenchmarkResultVersion { get; set; }

        public ClaymoreLiveStatus? liveStatus = null;
        public TRexLiveStatus? liveStatusTrex = null;

        public bool IsClaymoreMiningPossible(out string reason)
        {
            reason = "";
            if (this.liveStatus == null)
            {
                reason = "No benchmark run";
                return false;
            }
            if (!this.liveStatus.BenchmarkFinished)
            {
                reason = "Benchmark not finished";
                return false;
            }
            if (this.liveStatus.GPUs.Count <= 0)
            {
                reason = "No GPUs detected";
                return false;
            }
            foreach (var gpu in this.liveStatus.GPUs)
            {
                if (!String.IsNullOrEmpty(gpu.Value.GPUError))
                {
                    reason = "Benchmark failed: " + gpu.Value.GPUError;
                    return false;
                }
            }

            return true;
        }
    }
}
