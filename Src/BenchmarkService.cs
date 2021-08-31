using GolemUI.Claymore;
using GolemUI.Command;
using GolemUI.Interfaces;
using GolemUI.Model;

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
            var results = benchmarkResultsProvider.LoadBenchmarkResults();
            if (results != null)
            {
                _claymoreLiveStatus = results.liveStatus;
            }
        }

        private readonly Interfaces.IProviderConfig _providerConfig;
        private readonly ILogger<BenchmarkService> _logger;
        private ClaymoreLiveStatus? _claymoreLiveStatus = null;

        public ClaymoreLiveStatus? Status => _claymoreLiveStatus;

        public bool IsRunning { get; private set; }

        public bool _requestStop = false;

        public double? TotalMhs
        {
            get
            {
                var enabledAndCapableGpus = _claymoreLiveStatus?.GPUs.Values.Where(gpu => gpu.IsEnabledByUser && gpu.IsReadyForMining);
                return enabledAndCapableGpus?.Sum(gpu => gpu.BenchmarkSpeed);
            }
        }

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
            var walletAddress = _providerConfig.Config?.Account ?? "0x0000000000000000000000000000000000000001";
            var nodeName = _providerConfig.Config?.NodeName ?? "DefaultBenchmark";
            var poolAddr = GolemUI.Properties.Settings.Default.DefaultProxy;
            var totalClaymoreReportsNeeded = 5;

            IsRunning = true;
            OnPropertyChanged("IsRunning");

            bool preBenchmarkNeeded = !String.IsNullOrEmpty(cards);

            var cc = new ClaymoreBenchmark(totalClaymoreReportsNeeded, logger: _logger);


            _logger.LogInformation("Benchmark started");

            try
            {
                if (preBenchmarkNeeded)
                {
                    _logger.LogInformation("PreBenchmarkNeeded cards: " + cards + " niceness: " + niceness);


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
                            _logger.LogError("PreBenchmark failed with error: " + cc.BenchmarkError);

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
                            _logger.LogError("PreBenchmark failed timeElapsed > CLAYMORE_GPU_INFO_TIMEOUT: " + cc.BenchmarkError);

                            return;
                        }

                        if (_requestStop)
                        {
                            cc.Stop();
                            _claymoreLiveStatus!.ErrorMsg = "Stopped by user";
                            OnPropertyChanged("Status");
                            _logger.LogError("PreBenchmark stopped by user.");

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
                        result = cc.RunBenchmark(cards, niceness, poolAddr, walletAddress, nodeName);
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
                        foreach (var gpu in _claymoreLiveStatus.GPUs)
                        {
                            gpu.Value.BenchmarkDoneForThrottlingLevel = gpu.Value.ClaymorePerformanceThrottling;
                        }

                        _logger.LogInformation("Benchmark succeeded.");
                        break;
                    }
                    if (_claymoreLiveStatus.GPUInfosParsed && _claymoreLiveStatus.GPUs.Count == 0)
                    {
                        _logger.LogError("Benchmark succeeded, but no cards found");
                        break;
                    }
                    await Task.Delay(100);

                    double timeElapsed = (DateTime.Now - benchmarkStartTime).TotalSeconds;

                    if (_requestStop)
                    {
                        cc.Stop();
                        _claymoreLiveStatus.ErrorMsg = "Stopped by user";
                        OnPropertyChanged("Status");
                        _logger.LogError("Benchmark stopped by user.");
                        break;
                    }
                    if (timeElapsed > CLAYMORE_GPU_INFO_TIMEOUT && !_claymoreLiveStatus.GPUInfosParsed)
                    {
                        cc.Stop();
                        _claymoreLiveStatus.ErrorMsg = "Timeout, cannot read gpu info";
                        OnPropertyChanged("Status");
                        _logger.LogError("Timeout, cannot read gpu info");
                        break;
                    }
                    if (timeElapsed > CLAYMORE_TOTAL_BENCHMARK_TIMEOUT)
                    {
                        cc.Stop();
                        _claymoreLiveStatus.ErrorMsg = "Timeout, benchmark taking too long time";
                        OnPropertyChanged("Status");
                        _logger.LogError("Benchmark timeout, total benchmark timeout");
                        break;
                    }

                    if (_claymoreLiveStatus.GPUInfosParsed && _claymoreLiveStatus.GPUs.Values.Count > 0)
                    {
                        bool allCardsEndedWithError = true;
                        foreach (var gpu in _claymoreLiveStatus.GPUs.Values)
                        {
                            if (String.IsNullOrEmpty(gpu.GPUError))
                            {
                                allCardsEndedWithError = false;
                            }
                        }
                        if (allCardsEndedWithError && String.IsNullOrEmpty(_claymoreLiveStatus.ErrorMsg))
                        {
                            _claymoreLiveStatus.ErrorMsg = "Failed to validate cards";
                            break;
                        }
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
                        if (_requestStop)
                        {
                            gpu.GPUError = "Benchmark stopped by user";
                        }
                        else
                        {
                            if (!gpu.IsReadyForMining && !gpu.IsOperationStopped)
                            {
                                gpu.GPUError = "Timeout";
                            }
                        }
                        gpu.SetStepFinished();
                    }
                }
                if (_requestStop)
                {
                    //additional conditions when to revert back to old status when benchmark stopped
                    if (externalLiveStatus != null && String.IsNullOrEmpty(externalLiveStatus.ErrorMsg) && externalLiveStatus.GPUs.Count > 0
                        && externalLiveStatus.GPUs.Values.Where(x => x.BenchmarkSpeed > 0.0f).Count() > 0)
                    {
                        _claymoreLiveStatus = externalLiveStatus;
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
            args.Add("-wd");
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
                BenchmarkResultVersion = GolemUI.Properties.Settings.Default.BenchmarkResultsVersion,
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

        public bool IsMiningPossibleWithCurrentSettings
        {
            get
            {
                int count = _claymoreLiveStatus?.GPUs.Values.Where(x => x.IsEnabledByUser && x.IsReadyForMining).Count() ?? 0;
                return count > 0;
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
