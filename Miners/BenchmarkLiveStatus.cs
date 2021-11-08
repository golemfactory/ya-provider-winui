using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using GolemUI.Command;

namespace GolemUI.Miners
{

    public class BenchmarkLiveStatus : ICloneable
    {
        Dictionary<int, BenchmarkGpuStatus> _gpus = new Dictionary<int, BenchmarkGpuStatus>();
        public Dictionary<int, BenchmarkGpuStatus> GPUs { get { return _gpus; } }

        public bool BenchmarkFinished { get; set; }

        [JsonIgnore]
        public ProblemWithExeFile ProblemWithExeFile { get; set; }
        public float BenchmarkTotalSpeed { get; set; }
        public string? ErrorMsg { get; set; }
        public bool GPUInfosParsed { get; set; }
        public int NumberOfPhoenixPerfReports { get; set; }
        public int TotalPhoenixReportsBenchmark { get; set; }

        public bool LowMemoryMode { get; set; }

        public List<int> GetEnabledGpus()
        {
            List<int> result = new List<int>();
            foreach (KeyValuePair<int, BenchmarkGpuStatus> entry in this._gpus)
            {
                BenchmarkGpuStatus st = entry.Value;
                if (st.BenchmarkSpeed > 0.1 && st.IsDagFinished() && st.IsEnabledByUser)
                {
                    result.Add(st.GpuNo);
                }
            }
            return result;
        }

        public BenchmarkLiveStatus(int totalPhoenixReportsNeeded)
        {
            TotalPhoenixReportsBenchmark = totalPhoenixReportsNeeded;
        }

        public object Clone()
        {
            BenchmarkLiveStatus s = new BenchmarkLiveStatus(this.TotalPhoenixReportsBenchmark);
            s.BenchmarkFinished = this.BenchmarkFinished;
            s.BenchmarkTotalSpeed = this.BenchmarkTotalSpeed;
            //s.BenchmarkProgress = this.BenchmarkProgress;
            s.ErrorMsg = this.ErrorMsg;
            s.GPUInfosParsed = this.GPUInfosParsed;
            s.NumberOfPhoenixPerfReports = this.NumberOfPhoenixPerfReports;
            s.TotalPhoenixReportsBenchmark = this.TotalPhoenixReportsBenchmark;


            s._gpus = new Dictionary<int, BenchmarkGpuStatus>();
            foreach (KeyValuePair<int, BenchmarkGpuStatus> entry in this._gpus)
            {
                s._gpus.Add(entry.Key, (BenchmarkGpuStatus)entry.Value.Clone());
            }

            return s;
        }

        public bool AreAllDagsFinishedOrFailed()
        {
            foreach (var gpu in GPUs.Values)
            {
                bool gpuDagFinished = gpu.IsDagFinished() || gpu.IsOperationStopped;
                if (!gpuDagFinished)
                {
                    return false;
                }
            }
            return true;
        }

        public float GetEstimatedBenchmarkProgress()
        {
            const float INFO_PARSED = 0.1f;
            const float DAG_CREATION_STOP = 0.5f;

            if (!GPUInfosParsed)
            {
                return 0.0f;
            }
            if (!AreAllDagsFinishedOrFailed())
            {
                float minDagProgress = 1.0f;
                foreach (var gpu in GPUs.Values)
                {
                    if (gpu.DagProgress < minDagProgress)
                    {
                        minDagProgress = gpu.DagProgress;
                    }
                }
                return INFO_PARSED + minDagProgress * (DAG_CREATION_STOP - INFO_PARSED);
            }
            return DAG_CREATION_STOP + (float)NumberOfPhoenixPerfReports / TotalPhoenixReportsBenchmark * (1.0f - DAG_CREATION_STOP);



        }

        public void MergeFromBaseLiveStatus(BenchmarkLiveStatus baseLiveStatus, string cards, out bool allExpectedGpusFound)
        {
            int numberOfBaseGpus = baseLiveStatus.GPUs.Count;
            if (numberOfBaseGpus == 0)
            {
                allExpectedGpusFound = false;
                return;
            }

            int numberOfUsedGpus = this.GPUs.Count;
            if (numberOfBaseGpus == numberOfUsedGpus)
            {
                //no merging needed, because all gpus where used in benchmark
                allExpectedGpusFound = true;
                return;
            }

            var cardSplit = cards.Split(',');
            List<int> selectedIndices = new List<int>();
            foreach (var str in cardSplit)
            {
                int parsed;
                if (int.TryParse(str, out parsed))
                {
                    selectedIndices.Add(parsed);
                }
            }

            int numberOfSelectedCards = selectedIndices.Count;

            if (numberOfSelectedCards < numberOfUsedGpus)
            {
                //something went wrong
                Debug.Assert(false, "numberOfUsedGpus cannot be greater than numberOfSelectedCards");
                allExpectedGpusFound = false;
                return;
            }


            allExpectedGpusFound = true;
            Dictionary<int, int> indexMap = new Dictionary<int, int>();
            {
                int targetIdx = 1;
                for (int baseIdx = 1; baseIdx <= numberOfBaseGpus; baseIdx++)
                {
                    if (selectedIndices.Contains(baseIdx))
                    {
                        if (this.GPUs.ContainsKey(targetIdx))
                        {
                            indexMap.Add(baseIdx, targetIdx);
                            targetIdx += 1;
                        }
                        else
                        {
                            indexMap.Add(baseIdx, -1);
                            allExpectedGpusFound = false;
                        }
                    }
                    else
                    {
                        indexMap.Add(baseIdx, -1);
                    }
                }
            }

            Dictionary<int, BenchmarkGpuStatus> newDictionary = new Dictionary<int, BenchmarkGpuStatus>();
            for (int baseIdx = 1; baseIdx <= numberOfBaseGpus; baseIdx++)
            {
                BenchmarkGpuStatus gpuInfo;
                if (indexMap[baseIdx] == -1)
                {
                    gpuInfo = baseLiveStatus.GPUs[baseIdx];
                    gpuInfo.IsEnabledByUser = false;
                }
                else
                {
                    gpuInfo = this.GPUs[indexMap[baseIdx]];
                    gpuInfo.IsEnabledByUser = true;
                }
                gpuInfo.GpuNo = baseIdx;
                newDictionary.Add(baseIdx, gpuInfo);
            }

            _gpus = newDictionary;
        }
        public void MergeUserSettingsFromExternalLiveStatus(BenchmarkLiveStatus? externalLiveStatus)
        {
            if (externalLiveStatus == null)
            {
                return;
            }
            if (this.GPUs.Count == externalLiveStatus.GPUs.Count)
            {
                //copy to allow modifications inside
                var keys = this.GPUs.Keys.ToArray();
                foreach (var key in keys)
                {
                    if (externalLiveStatus.GPUs.ContainsKey(key))
                    {
                        this.GPUs[key].IsEnabledByUser = externalLiveStatus.GPUs[key].IsEnabledByUser;
                        this.GPUs[key].PhoenixPerformanceThrottling = externalLiveStatus.GPUs[key].PhoenixPerformanceThrottling;
                        if (!this.GPUs[key].IsEnabledByUser)
                        {
                            this.GPUs[key] = (BenchmarkGpuStatus)externalLiveStatus.GPUs[key].Clone();
                        }
                    }
                }
            }
        }
    }


}
