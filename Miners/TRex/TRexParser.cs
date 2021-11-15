using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using System.IO;
using System.Globalization;
using System.Net.Http;
using GolemUI.Utils;
using GolemUI.Command;
using Newtonsoft.Json.Linq;

namespace GolemUI.Miners.TRex
{
    public class TRexBenchmarkLine
    {
        public long delta_time_ms { get; set; }
        public string line = "";
    }

    public class TRexParser : IMinerParser
    {
        private readonly ILogger? _logger;
        const StringComparison STR_COMP_TYPE = StringComparison.InvariantCultureIgnoreCase;

        BenchmarkLiveStatus _liveStatus;

        private readonly object __lockObj = new object();

        private bool _readyForGpusEthInfo = false;
        private bool _isPreBenchmark;

        private DateTime _start;
        private string? _benchmarkRecordingPath = null;

        private string _trexServerAddress = "";

        private bool _gpusInfosParsed = false;

        public bool AreAllCardInfosParsed()
        {
            return _gpusInfosParsed;
        }

        public TRexParser(bool isPreBenchmark, int totalTRexReportsNeeded, ILogger logger)
        {
            _logger = logger;
            _isPreBenchmark = isPreBenchmark;
            _liveStatus = new BenchmarkLiveStatus(totalTRexReportsNeeded);
        }


        static int FindInStrEx(string baseStr, string searchedString)
        {
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(baseStr, searchedString, CompareOptions.IgnoreCase);
        }

        static bool ContainsInStrEx(string baseStr, string searchedString)
        {
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(baseStr, searchedString,
                CompareOptions.IgnoreCase) >= 0;
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
            _trexServerAddress = "";
            _start = DateTime.Now;
            if (enableRecording)
            {
                string benchmarkRecordingFolder = Path.Combine(PathUtil.GetLocalPath(), "BenchmarkRecordingsTRex");

                if (!Directory.Exists(benchmarkRecordingFolder))
                {
                    Directory.CreateDirectory(benchmarkRecordingFolder);
                }

                string suffix = _isPreBenchmark ? "pre_recording" : "recording";

                string benchmarkRecordingFile = String.Format("BenchmarkTRex_{0}.{1}",
                    DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"), suffix);
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
            _trexServerAddress = "";
            foreach (var gpu in _liveStatus.GPUs)
            {
                gpu.Value.SetStepFinished();
            }
        }

        private static bool _reportOnlyOnce = true;

        public async Task<bool> TimerBasedUpdateTick()
        {
            if (String.IsNullOrEmpty(_trexServerAddress))
            {
                return false;
            }

            try
            {

                //exception is handled in function calling TimerBasedUpdateTick
                using (var client = new HttpClient())
                {
                    string httpAddress = _trexServerAddress;
                    if (!httpAddress.StartsWith("http"))
                    {
                        httpAddress = "http://" + httpAddress;
                    }

                    if (!httpAddress.EndsWith("summary"))
                    {
                        httpAddress = httpAddress + "/summary";
                    }

                    //httpAddress should look here like http://127.0.0.1:4067/summary (which is default t-rex address)
                    var result = await client.GetStringAsync(httpAddress);

                    TRexDetails? details = JsonConvert.DeserializeObject<TRexDetails>(result);
                }

                return true;
            }
            catch (Exception e)
            {
                string errorMsg = "Error when getting data from T-Rex: " + e.Message;
                if (_reportOnlyOnce)
                {
                    _reportOnlyOnce = false;
                    _logger.LogError(errorMsg);
                }
                Console.WriteLine(errorMsg);
            }

            return false;
        }

        /// <summary>
        /// Parse output line of trex process
        /// </summary>
        public void ParseLine(string line)
        {
            lock (__lockObj)
            {
                // Your code...

                TRexBenchmarkLine benchLine = new TRexBenchmarkLine();
                benchLine.line = line;
                benchLine.delta_time_ms = (long)(DateTime.Now - _start).TotalMilliseconds;
#if DEBUG
                Debug.WriteLine(String.Format("{0}: {1}", benchLine.delta_time_ms, line));
#endif


                if (benchLine.line.Contains("HTTP server started on "))
                {
                    var newLine = benchLine.line.Replace("HTTP server started on ", "$");
                    var split = newLine.Split('$');
                    if (split.Length == 2)
                    {
                        _trexServerAddress = split[1].Trim();
                        Console.WriteLine($"Trex server running on address: {_trexServerAddress}");
                    }
                }

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
                string gpu_trex_index = "";
                BenchmarkGpuStatus? currentStatus = null;

                int indexNo = 0;
                foreach (var key in new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G" })
                {
                    indexNo += 1;
                    if (lineText.Contains($"GPU #{key}:"))
                    {
                        gpuNo = indexNo;
                        gpu_trex_index = key;
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
                            var splits = lineText.Split(':');

                            if (splits.Length > 0)
                            {
                                string gpuName = splits[splits.Length - 1].Trim();

                                var spplit = gpuName.Split(']');

                                if (spplit.Length >= 2)
                                {
                                    currentStatus.GPUDetails = spplit[1].Trim();
                                    currentStatus.GpuName = currentStatus.GPUDetails;
                                }
                            }
                        }
                    }


                    if (ContainsInStrEx(lineText, "generating DAG"))
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

                    if (ContainsInStrEx(lineText, " MH/s"))
                    {
                        var splits = lineText.Split('-');
                        if (splits.Length == 2)
                        {
                            var splits2 = splits[1].Replace("MH/s", "^").Split('^');
                            if (splits2.Length == 2)
                            {
                                var mhs_str = splits2[0].Trim();
                                if (Double.TryParse(mhs_str, out double mhs))
                                {

                                    int parsedGpuNo = gpuNo;

                                    if (!_liveStatus.GPUs.ContainsKey(parsedGpuNo))
                                    {
                                        _liveStatus.GPUs.Add(parsedGpuNo, new BenchmarkGpuStatus(parsedGpuNo, true, 0));
                                    }
                                    _liveStatus.GPUs[parsedGpuNo].BenchmarkSpeed = (float)mhs;

                                    if (_liveStatus.AreAllDagsFinishedOrFailed())
                                    {
                                        _liveStatus.NumberOfPhoenixPerfReports += 5;
                                    }
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
    }
}

