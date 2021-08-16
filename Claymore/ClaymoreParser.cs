using GolemUI.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Claymore
{

    public class ClaymoreGpuStatus : ICloneable, INotifyPropertyChanged
    {
        public int GpuNo { get; set; }
        public string? GpuName { get; set; }
        public int? PciExpressLane { get; set; }
        public bool OutOfMemory { get; set; }
        public bool GPUNotFound { get; set; }
        public float BenchmarkSpeed { get; set; }

        [JsonIgnore]
        public bool BenchmarkSpeedForDifferentThrottling
        {
            get => (BenchmarkSpeed > 0 && BenchmarkDoneForThrottlingLevel != ClaymorePerformanceThrottling);
        }

        public int BenchmarkDoneForThrottlingLevel { get; set; }

        public bool IsDagCreating { get; set; }

        private bool _isEnabledByUser;
        public bool IsEnabledByUser
        {
            get => _isEnabledByUser;
            set
            {

                if (IsEnabledByUser != value)
                {
                    _isEnabledByUser = value;
                    NotifyChange(nameof(IsEnabledByUser));
                }
            }
        }

        public float DagProgress { get; set; }
        public string? GPUVendor { get; set; }
        public string? GPUDetails { get; set; }
        public string? GPUError { get; set; }

        //steps for view presentation (only one state is possible at the time)
        [JsonIgnore]
        public bool IsPreInitialization { get; set; }
        [JsonIgnore]
        public bool IsInitialization { get; set; }
        [JsonIgnore]
        public bool IsEstimation { get; set; }

        public bool IsFinished { get; set; }

        private void NotifyChange([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        //temporary parameters to be removed
        [JsonIgnore]
        public string DisplayName
        {
            get
            {
                return GpuNo + ". " + GpuName;
            }
        }

        [JsonIgnore]
        public string ClaymorePerformanceThrottlingDebug => "(debug: " + ClaymorePerformanceThrottling + ") ";

        [JsonIgnore]
        public int _claymorePerformanceThrottling { get; set; } = (int)PerformanceThrottlingEnumConverter.Default;
        public int ClaymorePerformanceThrottling
        {
            get { return _claymorePerformanceThrottling; }
            set
            {
                _claymorePerformanceThrottling = value;
                NotifyChange("ClaymorePerformanceThrottling");
                NotifyChange("ClaymorePerformanceThrottlingDebug");
            }
        }

        [JsonIgnore]
        public PerformanceThrottlingEnum SelectedMiningMode
        {
            get
            {
                return PerformanceThrottlingEnumConverter.FromInt(ClaymorePerformanceThrottling);
            }
            set
            {
                if (ClaymorePerformanceThrottling != (int)value)
                {
                    ClaymorePerformanceThrottling = (int)value;
                    NotifyChange(nameof(SelectedMiningMode));
                    NotifyChange(nameof(BenchmarkSpeed));
                    NotifyChange(nameof(BenchmarkSpeedForDifferentThrottling));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void SetStepInitialization()
        {
            IsPreInitialization = false;
            IsInitialization = true;
            IsEstimation = false;
            IsFinished = false;
            NotifyChange("");
        }

        public void SetStepEstimation()
        {
            IsPreInitialization = false;
            IsInitialization = false;
            IsEstimation = true;
            IsFinished = false;
            NotifyChange("");
        }

        public void SetStepFinished()
        {
            IsPreInitialization = false;
            IsInitialization = false;
            IsEstimation = false;
            IsFinished = true;
            NotifyChange("");
        }

        public ClaymoreGpuStatus()
        {

        }

        public ClaymoreGpuStatus(int gpuNo, bool isEnabledByUser, int claymorePerformanceThrottling)
        {
            this.IsEnabledByUser = isEnabledByUser;
            this.ClaymorePerformanceThrottling = claymorePerformanceThrottling;
            this.GpuNo = gpuNo;
            this.IsPreInitialization = true;
        }

        public object Clone()
        {
            ClaymoreGpuStatus s = new ClaymoreGpuStatus(this.GpuNo, this.IsEnabledByUser, this.ClaymorePerformanceThrottling);
            s.GpuName = this.GpuName;
            s.OutOfMemory = this.OutOfMemory;
            s.BenchmarkDoneForThrottlingLevel = this.BenchmarkDoneForThrottlingLevel;
            s.GPUNotFound = this.GPUNotFound;
            s.BenchmarkSpeed = this.BenchmarkSpeed;
            s.IsDagCreating = this.IsDagCreating;
            s.DagProgress = this.DagProgress;
            s.GPUVendor = this.GPUVendor;
            s.GPUDetails = this.GPUDetails;
            s.PciExpressLane = this.PciExpressLane;
            s.IsEnabledByUser = this.IsEnabledByUser;
            s.ClaymorePerformanceThrottling = this.ClaymorePerformanceThrottling;
            s.GPUError = this.GPUError;
            s.IsPreInitialization = this.IsPreInitialization;
            s.IsInitialization = this.IsInitialization;
            s.IsEstimation = this.IsEstimation;
            s.IsFinished = this.IsFinished;
            return s;
        }

        [JsonIgnore]
        public bool IsReadyForMining => (IsDagFinished() && BenchmarkSpeed > 0.5 && String.IsNullOrEmpty(GPUError));

        [JsonIgnore]
        public bool IsOperationStopped => (OutOfMemory || GPUNotFound || !String.IsNullOrEmpty(GPUError) || !IsEnabledByUser);

        public bool IsDagFinished()
        {
            if (DagProgress < 1.0f)
            {
                return false;
            }
            if (IsDagCreating)
            {
                return false;
            }
            return true;
        }


    }

    public class ClaymoreLiveStatus : ICloneable
    {
        Dictionary<int, ClaymoreGpuStatus> _gpus = new Dictionary<int, ClaymoreGpuStatus>();
        public Dictionary<int, ClaymoreGpuStatus> GPUs { get { return _gpus; } }

        public bool BenchmarkFinished { get; set; }

        public float BenchmarkTotalSpeed { get; set; }
        public string? ErrorMsg { get; set; }
        public bool GPUInfosParsed { get; set; }
        public int NumberOfClaymorePerfReports { get; set; }
        public int TotalClaymoreReportsBenchmark { get; set; }

        public bool IsBenchmark;

        public List<int> GetEnabledGpus()
        {
            List<int> result = new List<int>();
            foreach (KeyValuePair<int, ClaymoreGpuStatus> entry in this._gpus)
            {
                ClaymoreGpuStatus st = entry.Value;
                if (st.BenchmarkSpeed > 0.1 && st.IsDagFinished() && st.IsEnabledByUser)
                {
                    result.Add(st.GpuNo);
                }
            }
            return result;
        }

        public ClaymoreLiveStatus(bool isBenchmark, int totalClaymoreReportsNeeded)
        {
            IsBenchmark = isBenchmark;
            TotalClaymoreReportsBenchmark = totalClaymoreReportsNeeded;
        }

        public object Clone()
        {
            ClaymoreLiveStatus s = new ClaymoreLiveStatus(this.IsBenchmark, this.TotalClaymoreReportsBenchmark);
            s.BenchmarkFinished = this.BenchmarkFinished;
            s.BenchmarkTotalSpeed = this.BenchmarkTotalSpeed;
            //s.BenchmarkProgress = this.BenchmarkProgress;
            s.ErrorMsg = this.ErrorMsg;
            s.GPUInfosParsed = this.GPUInfosParsed;
            s.NumberOfClaymorePerfReports = this.NumberOfClaymorePerfReports;
            s.TotalClaymoreReportsBenchmark = this.TotalClaymoreReportsBenchmark;


            s._gpus = new Dictionary<int, ClaymoreGpuStatus>();
            foreach (KeyValuePair<int, ClaymoreGpuStatus> entry in this._gpus)
            {
                s._gpus.Add(entry.Key, (ClaymoreGpuStatus)entry.Value.Clone());
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
            return DAG_CREATION_STOP + (float)NumberOfClaymorePerfReports / TotalClaymoreReportsBenchmark * (1.0f - DAG_CREATION_STOP);



        }

        public void MergeFromBaseLiveStatus(ClaymoreLiveStatus baseLiveStatus, string cards, out bool allExpectedGpusFound)
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

            Dictionary<int, ClaymoreGpuStatus> newDictionary = new Dictionary<int, ClaymoreGpuStatus>();
            for (int baseIdx = 1; baseIdx <= numberOfBaseGpus; baseIdx++)
            {
                ClaymoreGpuStatus gpuInfo;
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
        public void MergeUserSettingsFromExternalLiveStatus(ClaymoreLiveStatus? externalLiveStatus)
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
                        this.GPUs[key].ClaymorePerformanceThrottling = externalLiveStatus.GPUs[key].ClaymorePerformanceThrottling;
                        if (!this.GPUs[key].IsEnabledByUser)
                        {
                            this.GPUs[key] = (ClaymoreGpuStatus)externalLiveStatus.GPUs[key].Clone();
                        }
                    }
                }
            }
        }
    }



    public class ClaymoreBenchmarkLine
    {
        public long delta_time_ms { get; set; }
        public string line = "";
    }

    public class ClaymoreParser
    {
        private readonly ILogger? _logger;
        const StringComparison STR_COMP_TYPE = StringComparison.InvariantCultureIgnoreCase;

        ClaymoreLiveStatus _liveStatus;

        private readonly object __lockObj = new object();

        private bool _readyForGpusEthInfo = false;
        private bool _isPreBenchmark;

        private DateTime _start;
        private string? _benchmarkRecordingPath = null;

        private bool _gpusInfosParsed = false;
        public bool AreAllCardInfosParsed()
        {
            return _gpusInfosParsed;
        }

        public ClaymoreParser(bool isBenchmark, bool isPreBenchmark, int totalClaymoreReportsNeeded, ILogger logger)
        {
            _logger = logger;
            _isPreBenchmark = isPreBenchmark;
            _liveStatus = new ClaymoreLiveStatus(isBenchmark, totalClaymoreReportsNeeded);
        }


        static int FindInStrEx(string baseStr, string searchedString)
        {
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(baseStr, searchedString, CompareOptions.IgnoreCase);
        }
        static bool ContainsInStrEx(string baseStr, string searchedString)
        {
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(baseStr, searchedString, CompareOptions.IgnoreCase) >= 0;
        }

        /// <summary>
        /// Thread safe 
        /// </summary>
        /// <returns>copy of ClaymoreLiveStatus structure</returns>
        public ClaymoreLiveStatus GetLiveStatusCopy()
        {
            lock (__lockObj)
            {
                return (ClaymoreLiveStatus)_liveStatus.Clone();
                // Your code...
            }
        }

        /// <summary>
        /// call before parsing to get good recording
        /// </summary>
        public void BeforeParsing(bool enableRecording)
        {
            _start = DateTime.Now;
            if (enableRecording)
            {
                string benchmarkRecordingFolder = Path.Combine(PathUtil.GetLocalPath(), "BenchmarkRecordings");

                if (!Directory.Exists(benchmarkRecordingFolder))
                {
                    Directory.CreateDirectory(benchmarkRecordingFolder);
                }

                string suffix = _isPreBenchmark ? "pre_recording" : "recording";

                string benchmarkRecordingFile = String.Format("Benchmark_{0}.{1}", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"), suffix);
                _benchmarkRecordingPath = Path.Combine(benchmarkRecordingFolder, benchmarkRecordingFile);
                StreamWriter? sw = null;
                try
                {
                    sw = new StreamWriter(_benchmarkRecordingPath, false);
                    sw.WriteLine("0: Start recording");
                }
                finally
                {
                    if (sw != null)
                    {
                        sw.Close();
                    }
                }
            }
        }

        public void SetFinished()
        {
            foreach (var gpu in _liveStatus.GPUs)
            {
                gpu.Value.SetStepFinished();
            }
        }

        /// <summary>
        /// Parse output line of claymore process
        /// </summary>
        public void ParseLine(string line)
        {
            lock (__lockObj)
            {
                // Your code...

                ClaymoreBenchmarkLine benchLine = new ClaymoreBenchmarkLine();
                benchLine.line = line;
                benchLine.delta_time_ms = (long)(DateTime.Now - _start).TotalMilliseconds;
#if DEBUG
                Debug.WriteLine(String.Format("{0}: {1}", benchLine.delta_time_ms, line));
#endif

                StreamWriter? sw = null;
                if (_benchmarkRecordingPath != null)
                {
                    try
                    {
                        sw = new StreamWriter(_benchmarkRecordingPath, true);
                        sw.WriteLine(String.Format("{0}: {1}", benchLine.delta_time_ms, line));
                    }
                    finally
                    {
                        if (sw != null)
                        {
                            sw.Close();
                        }
                    }
                }

                string lineText = line;

                if (lineText == null)
                    return;

                //output contains spelling error avaiable instead of available:
                if (ContainsInStrEx(lineText, "No avaiable GPUs for mining") || ContainsInStrEx(lineText, "No available GPUs for mining"))
                {
                    _liveStatus.GPUInfosParsed = true;
                    _liveStatus.BenchmarkFinished = true;
                    return;
                }
                int gpuNo = -1;
                string gpu_claymore_index = "";
                ClaymoreGpuStatus? currentStatus = null;

                int indexNo = 0;
                foreach (var key in new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G" })
                {
                    indexNo += 1;
                    if (lineText.StartsWith($"GPU{key}"))
                    {
                        gpuNo = indexNo;
                        gpu_claymore_index = key;
                        break;
                    }
                }


                if (lineText == "Fatal error detected")
                {
                    _liveStatus.ErrorMsg = lineText;
                }
                if (_isPreBenchmark)
                {
                    if (lineText.Contains("Missing host or wallet"))
                    {
                        _liveStatus.GPUInfosParsed = true;
                    }
                }


                if (gpuNo != -1)
                {
                    if (!_liveStatus.GPUs.ContainsKey(gpuNo))
                    {
                        _liveStatus.GPUs.Add(gpuNo, new ClaymoreGpuStatus(gpuNo, true, (int)PerformanceThrottlingEnumConverter.Default));
                    }
                    currentStatus = _liveStatus.GPUs[gpuNo];


                    if (currentStatus.GPUVendor == null)
                    {
                        bool nVidiaGpuFound = false;
                        bool amdGpuFound = false;

                        if (ContainsInStrEx(lineText, "NVIDIA") ||
                            ContainsInStrEx(lineText, "GeForce") ||
                            ContainsInStrEx(lineText, "Quadro") ||
                            ContainsInStrEx(lineText, "CUDA")
                            )
                        {
                            currentStatus.GPUVendor = "nVidia";
                            nVidiaGpuFound = true;
                        }

                        if (ContainsInStrEx(lineText, "RADEON"))
                        {
                            currentStatus.GPUVendor = "AMD";
                            amdGpuFound = true;
                        }

                        if ((nVidiaGpuFound || amdGpuFound) && lineText.Contains(":"))
                        {
                            //todo - what happens when details contains :
                            currentStatus.GPUDetails = lineText.Split(':')[1].Trim();

                            if (currentStatus.GPUDetails.Contains(','))
                            {
                                currentStatus.GpuName = currentStatus.GPUDetails.Split(',')[0];
                                if (currentStatus.GpuName.Contains("(pcie"))
                                {
                                    var split2 = currentStatus.GpuName.Replace("(pcie", "^").Split('^');
                                    currentStatus.GpuName = split2[0].Trim();
                                    int pciExpressLane;
                                    if (split2.Length >= 2)
                                    {
                                        bool success = int.TryParse(split2[1].Trim(new char[] { ' ', ')' }), out pciExpressLane);
                                        if (success)
                                        {
                                            currentStatus.PciExpressLane = pciExpressLane;
                                        }
                                    }
                                }
                            }
                        }
                    }


                    if (ContainsInStrEx(lineText, ": clSetKernelArg"))
                    {
                        currentStatus.GPUError = lineText;
                    }

                    if (ContainsInStrEx(lineText, ": Starting up"))
                    {
                        _liveStatus.GPUInfosParsed = true;
                    }

                    if (ContainsInStrEx(lineText, ": Allocating DAG"))
                    {
                        _liveStatus.GPUInfosParsed = true;
                        currentStatus.IsDagCreating = true;
                        currentStatus.DagProgress = 0.0f;
                        currentStatus.SetStepInitialization();
                    }
                    if (ContainsInStrEx(lineText, ": Generating DAG"))
                    {
                        _liveStatus.GPUInfosParsed = true;
                        currentStatus.IsDagCreating = true;
                        currentStatus.DagProgress = 0.05f;
                        currentStatus.SetStepInitialization();
                    }

                    if (ContainsInStrEx(lineText, ": DAG"))
                    {
                        var splits = lineText.Split(' ');

                        foreach (var split in splits)
                        {
                            if (split.Contains("%"))
                            {
                                string percentDag = split.Trim(new char[] { ' ', '%' });
                                double res = 0;
                                if (double.TryParse(percentDag, NumberStyles.Float, CultureInfo.InvariantCulture, out res))
                                {
                                    currentStatus.DagProgress = (float)(res / 100.0);
                                }
                                else
                                {
                                    //handle error somehow??
                                }
                            }
                        }
                    }

                    if (ContainsInStrEx(lineText, ": DAG generated"))
                    {
                        currentStatus.IsDagCreating = false;
                        currentStatus.DagProgress = 1.0f;
                        currentStatus.SetStepEstimation();
                    }
                    if (ContainsInStrEx(lineText, "out of memory"))
                    {
                        currentStatus.GPUError = lineText;
                    }

                }
                if (lineText.StartsWith("Eth speed", STR_COMP_TYPE))
                {
                    _readyForGpusEthInfo = true;

                    var splits = lineText.Split(' ');
                    double val;
                    if (splits.Length > 2 && double.TryParse(splits[2], NumberStyles.Float, CultureInfo.InvariantCulture, out val))
                    {
                        double s = val;

                        _liveStatus.BenchmarkTotalSpeed = (float)val;

                        if (_liveStatus.BenchmarkTotalSpeed > 0.1 && _liveStatus.GPUs.Count == 1 && _liveStatus.AreAllDagsFinishedOrFailed())
                        {
                            _liveStatus.GPUs.First().Value.BenchmarkSpeed = _liveStatus.BenchmarkTotalSpeed;
                            _liveStatus.NumberOfClaymorePerfReports += 1;
                        }
                    }
                }

                if (_readyForGpusEthInfo && lineText.StartsWith("GPUs:", STR_COMP_TYPE))
                {
                    //sample:
                    //"GPUs: 1: 0.000 MH/s (0) 2: 0.000 MH/s (0)"

                    var splits = lineText.Replace("MH/s", "^").Split('^');


                    for (int i = 0; i < splits.Length - 1; i++)
                    {
                        //double val;
                        //if (split.Length > 2 && double.TryParse(split[2], out val))
                        var s = splits[i].TrimEnd().Split(' ');

                        var p = s.Last();
                        double mhs;

                        bool parsedOK = double.TryParse(p, NumberStyles.Float, CultureInfo.InvariantCulture, out mhs);

                        if (parsedOK)
                        {
                            int parsedGpuNo = i + 1;

                            if (!_liveStatus.GPUs.ContainsKey(parsedGpuNo))
                            {
                                _liveStatus.GPUs.Add(parsedGpuNo, new ClaymoreGpuStatus(parsedGpuNo, true, 0));
                            }
                            _liveStatus.GPUs[parsedGpuNo].BenchmarkSpeed = (float)mhs;
                        }


                    }

                    if (_liveStatus.AreAllDagsFinishedOrFailed())
                    {
                        _liveStatus.NumberOfClaymorePerfReports += 1;
                    }
                }
            }
            _logger.LogDebug("done log {0}", line);
        }
    }
}
