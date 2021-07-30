using GolemUI.Claymore;
using GolemUI.Command;
using GolemUI.Interfaces;
using GolemUI.Model;
using GolemUI.Settings;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GolemUI.Src
{
    public class BenchmarkService : INotifyPropertyChanged
    {
        public BenchmarkService(IProviderConfig providerConfig, ILogger<BenchmarkService> logger, IBenchmarkResultsProvider benchmarkResultsProvider)
        {
            _benchmarkResultsProvider = benchmarkResultsProvider;
            _providerConfig = providerConfig;
            _logger = logger;
        }

        private readonly Interfaces.IProviderConfig _providerConfig;
        private readonly ILogger<BenchmarkService> _logger;
        private ClaymoreLiveStatus? _claymoreLiveStatus = null;

        public ClaymoreLiveStatus? Status => _claymoreLiveStatus;

        public bool IsRunning { get; private set; }

        public bool _requestStop = false;

        public float? TotalMhs => _claymoreLiveStatus == null ? null : (from gpus in _claymoreLiveStatus?.GPUs.Values select gpus.BenchmarkSpeed).Sum();

        private readonly double CLAYMORE_GPU_INFO_TIMEOUT = 10.0;
        private readonly double CLAYMORE_TOTAL_BENCHMARK_TIMEOUT = 200.0;
        private IBenchmarkResultsProvider _benchmarkResultsProvider;

        public async void StartBenchmark(string cards, string niceness, string pool, string ethereumAddress, ClaymoreLiveStatus? externalLiveStatus)
        {
            if (IsRunning)
            {
                return;
            }
            _requestStop = false;

            ClaymoreLiveStatus? baseLiveStatus = null;


            DateTime benchmarkStartTime = DateTime.Now;
            var walletAddress = _providerConfig.Config?.Account ?? "0xD593411F3E6e79995E787b5f81D10e12fA6eCF04";
            var poolAddr = GlobalSettings.DefaultProxy;
            var totalClaymoreReportsNeeded = 5;

            IsRunning = true;
            OnPropertyChanged("IsRunning");

            bool preBenchmarkNeeded = !String.IsNullOrEmpty(cards);

            var cc = new ClaymoreBenchmark(totalClaymoreReportsNeeded, logger: _logger);


            _logger.LogInformation("running benchmark");   //todo: add more informative logs about benchmark further on

            try
            {
                if (preBenchmarkNeeded)
                {
                    bool result = cc.RunBenchmarkRecording(@"test.pre_recording", isPreBenchmark: true);

                    if (!result)
                    {
                        result = cc.RunPreBenchmark();
                    }

                    if (!result)
                    {
                        if (_claymoreLiveStatus != null)
                        {
                            _claymoreLiveStatus.GPUs.Clear();
                            _claymoreLiveStatus.ErrorMsg = cc.BenchmarkError;
                            OnPropertyChanged("Status");
                        }
                        return;
                    }

                    while (!cc.PreBenchmarkFinished)
                    {
                        await Task.Delay(30);

                        double timeElapsed = (DateTime.Now - benchmarkStartTime).TotalSeconds;

                        if (timeElapsed > CLAYMORE_GPU_INFO_TIMEOUT)
                        {
                            cc.Stop();

                            _claymoreLiveStatus!.GPUs.Clear();
                            _claymoreLiveStatus!.ErrorMsg = "Failed to obtain card list";
                            OnPropertyChanged("Status");

                            return;
                        }

                        if (_requestStop)
                        {
                            cc.Stop();
                            _claymoreLiveStatus!.ErrorMsg = "Stopped by user";
                            OnPropertyChanged("Status");
                            break;
                        }
                        _claymoreLiveStatus = cc.ClaymoreParserPreBenchmark.GetLiveStatusCopy();
                        _claymoreLiveStatus.MergeUserSettingsFromExternalLiveStatus(externalLiveStatus);
                        baseLiveStatus = _claymoreLiveStatus;
                        OnPropertyChanged("Status");
                        if (_claymoreLiveStatus.GPUInfosParsed)
                        {
                            cc.Stop();
                            break;
                        }
                    }
                }
                await Task.Delay(30);



                if (preBenchmarkNeeded && _claymoreLiveStatus != null && _claymoreLiveStatus.GPUs.Count == 0)
                {
                    return;
                }

                benchmarkStartTime = DateTime.Now;

                {
                    bool result = cc.RunBenchmarkRecording(@"test.recording", isPreBenchmark: false);
                    if (!result)
                    {
                        result = cc.RunBenchmark(cards, niceness, poolAddr, walletAddress);
                    }
                    if (!result)
                    {
                        if (_claymoreLiveStatus != null)
                        {
                            _claymoreLiveStatus.GPUs.Clear();
                            _claymoreLiveStatus.ErrorMsg = cc.BenchmarkError;
                            OnPropertyChanged("Status");
                        }
                        return;
                    }
                }

                while (!cc.BenchmarkFinished && IsRunning)
                {
                    _claymoreLiveStatus = cc.ClaymoreParserBenchmark.GetLiveStatusCopy();

                    bool allExpectedGPUsFound = false;
                    if (baseLiveStatus != null)
                    {
                        _claymoreLiveStatus.MergeFromBaseLiveStatus(baseLiveStatus, cards, out allExpectedGPUsFound);
                    }
                    _claymoreLiveStatus.MergeUserSettingsFromExternalLiveStatus(externalLiveStatus);
                    OnPropertyChanged("Status");
                    OnPropertyChanged("TotalMhs");
                    if (_claymoreLiveStatus.NumberOfClaymorePerfReports >= _claymoreLiveStatus.TotalClaymoreReportsBenchmark)
                    {
                        break;
                    }
                    if (_claymoreLiveStatus.GPUInfosParsed && _claymoreLiveStatus.GPUs.Count == 0)
                    {
                        break;
                    }
                    await Task.Delay(100);

                    double timeElapsed = (DateTime.Now - benchmarkStartTime).TotalSeconds;

                    if (_requestStop)
                    {
                        cc.Stop();
                        _claymoreLiveStatus.ErrorMsg = "Stopped by user";
                        OnPropertyChanged("Status");
                        break;
                    }
                    if (timeElapsed > CLAYMORE_GPU_INFO_TIMEOUT && !_claymoreLiveStatus.GPUInfosParsed)
                    {
                        cc.Stop();
                        _claymoreLiveStatus.ErrorMsg = "Timeout, cannot read gpu info";
                        OnPropertyChanged("Status");
                        break;
                    }
                    if (timeElapsed > CLAYMORE_TOTAL_BENCHMARK_TIMEOUT)
                    {
                        cc.Stop();
                        _claymoreLiveStatus.ErrorMsg = "Timeout, benchmark taking too long time";
                        OnPropertyChanged("Status");
                        break;
                    }


                }
            }
            finally
            {
                //todo: remove later on, right now I've let it stay here to  help test sentry
                _logger.LogError("--test error");
                IsRunning = false;
                cc.Stop();
                if (_claymoreLiveStatus != null)
                {
                    _claymoreLiveStatus.BenchmarkFinished = true;
                    foreach (var gpu in _claymoreLiveStatus.GPUs.Values)
                    {
                        gpu.SetStepFinished();
                        if (!gpu.IsReadyForMining && !gpu.IsOperationStopped)
                        {
                            gpu.GPUError = "Timeout";
                        }
                    }
                }
                OnPropertyChanged("IsRunning");
                OnPropertyChanged("Status");
            }
        }

        public void Apply(ClaymoreLiveStatus liveStatus)
        {
            _claymoreLiveStatus = liveStatus;
            OnPropertyChanged("Status");
        }

        internal string? ExtractClaymoreParams()
        {
            var status = _claymoreLiveStatus ?? _benchmarkResultsProvider.LoadBenchmarkResults().liveStatus;
            if (status == null)
            {
                return null;
            }

            var gpus = status.GPUs.Values;

            if (gpus.Count == 0)
            {
                return null;
            }

            var args = new List<string>();
            if (gpus.Any(gpu => !gpu.IsEnabledByUser))
            {
                args.Add("-gpus");
                args.Add(String.Join(",", gpus.Where(gpu => gpu.IsEnabledByUser).Select(gpu => gpu.GpuNo)));
            }
            args.Add("-li");
            args.Add(String.Join(",", gpus.Where(gpu => gpu.IsEnabledByUser).Select(gpu => gpu.ClaymorePerformanceThrottling)));
            args.Add("-clnew");
            args.Add("1");
            args.Add("-clKernel");
            args.Add("0");
            return String.Join(" ", args);
        }

        public void StopBenchmark()
        {
            _requestStop = true;
        }

        public void Save()
        {
            var results = new BenchmarkResults()
            {
                BenchmarkResultVersion = GlobalSettings.CurrentBenchmarkResultVersion,
                liveStatus = _claymoreLiveStatus
            };
            _benchmarkResultsProvider.SaveBenchmarkResults(results);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public bool IsClaymoreMiningPossible
        {
            get
            {
                var gpus = _claymoreLiveStatus?.GPUs.Values;
                if (gpus != null)
                {
                    return gpus.Any(gpu => (gpu.GPUError ?? "") == "");
                }
                else
                {
                    return false;
                }
            }
        }

    }
}
