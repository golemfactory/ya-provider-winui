using GolemUI.Command;
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
using GolemUI.Miners;

namespace GolemUI.Miners.Phoenix
{

    public class PhoenixBenchmarkLine
    {
        public long delta_time_ms { get; set; }
        public string line = "";
    }

    public class PhoenixParser : IMinerParser
    {
        private readonly ILogger? _logger;
        const StringComparison STR_COMP_TYPE = StringComparison.InvariantCultureIgnoreCase;

        BenchmarkLiveStatus _liveStatus;

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

        public PhoenixParser(bool isPreBenchmark, int totalPhoenixReportsNeeded, ILogger? logger)
        {
            _logger = logger;
            _isPreBenchmark = isPreBenchmark;
            _liveStatus = new BenchmarkLiveStatus(totalPhoenixReportsNeeded);
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
        /// <returns>copy of BenchmarkLiveStatus structure</returns>
        public BenchmarkLiveStatus GetLiveStatusCopy()
        {
            lock (__lockObj)
            {
                return (BenchmarkLiveStatus)_liveStatus.Clone();
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
                string benchmarkRecordingFolder = Path.Combine(PathUtil.GetLocalPath(), "BenchmarkRecordingsClay");

                if (!Directory.Exists(benchmarkRecordingFolder))
                {
                    Directory.CreateDirectory(benchmarkRecordingFolder);
                }

                string suffix = _isPreBenchmark ? "pre_recording" : "recording";

                string benchmarkRecordingFile = String.Format("BenchmarkClay_{0}.{1}", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"), suffix);
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
        /// Parse output line of phoenix process
        /// </summary>
        public void ParseLine(string line)
        {
            lock (__lockObj)
            {
                // Your code...

                PhoenixBenchmarkLine benchLine = new PhoenixBenchmarkLine();
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
                string gpu_phoenix_index = "";
                BenchmarkGpuStatus? currentStatus = null;

                int indexNo = 0;
                foreach (var key in new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G" })
                {
                    indexNo += 1;
                    if (lineText.StartsWith($"GPU{key}"))
                    {
                        gpuNo = indexNo;
                        gpu_phoenix_index = key;
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
                        _liveStatus.GPUs.Add(gpuNo, new BenchmarkGpuStatus(gpuNo, true, (int)PerformanceThrottlingEnumConverter.Default));
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
                        currentStatus.GPUError = "GPU - not enough RAM for mining";
                        currentStatus.GpuErrorType = GpuErrorType.NotEnoughMemory;
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
                            _liveStatus.NumberOfPhoenixPerfReports += 1;
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
                                _liveStatus.GPUs.Add(parsedGpuNo, new BenchmarkGpuStatus(parsedGpuNo, true, 0));
                            }
                            _liveStatus.GPUs[parsedGpuNo].BenchmarkSpeed = (float)mhs;
                        }


                    }

                    if (_liveStatus.AreAllDagsFinishedOrFailed())
                    {
                        _liveStatus.NumberOfPhoenixPerfReports += 1;
                    }
                }
            }
            _logger.LogDebug("done log {0}", line);
        }

        public async Task<bool> TimerBasedUpdateTick()
        {
            //no timer based update on phoenix
            await Task.Delay(1);
            return false;
        }
    }
}
