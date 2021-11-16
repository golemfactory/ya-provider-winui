
using GolemUI.Command;
using GolemUI.Interfaces;
using GolemUI.Model;
using GolemUI.Miners;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GolemUI.Miners.Phoenix;
using GolemUI.Miners.TRex;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace GolemUI.Src
{
    public class BenchmarkService : INotifyPropertyChanged
    {
        public event OnProblemsWithExeFileEventHander? ProblemWithExe;
        public event OnProblemsWithExeFileEventHander? AntivirusStatus;
        private IUserSettingsProvider _userSettingsProvider;
        private PhoenixMiner _phoenixMiner;
        private TRexMiner _trexMiner;

        public BenchmarkService(IProviderConfig providerConfig, ILogger<BenchmarkService> logger, IBenchmarkResultsProvider benchmarkResultsProvider, IUserSettingsProvider userSettingsProvider, PhoenixMiner phoenixMiner, TRexMiner trexMiner)
        {
            _userSettingsProvider = userSettingsProvider;

            _benchmarkResultsProvider = benchmarkResultsProvider;
            _providerConfig = providerConfig;
            _logger = logger;
            var results = benchmarkResultsProvider.LoadBenchmarkResults(_userSettingsProvider.LoadUserSettings().SelectedMinerName);
            if (results != null)
            {
                _minerLiveStatus = results.liveStatus;
            }

            _phoenixMiner = phoenixMiner;
            _trexMiner = trexMiner;
        }

        private readonly Interfaces.IProviderConfig _providerConfig;
        private readonly ILogger<BenchmarkService> _logger;
        private BenchmarkLiveStatus? _minerLiveStatus = null;
        public IMinerApp? ActiveMinerApp { get; set; } = null;

        public BenchmarkLiveStatus? Status
        {
            get
            {
                return _minerLiveStatus;
            }
        }

        public void ReloadBenchmarkSettingsFromFile()
        {
            if (!IsRunning)
            {
                var minerAppName = _userSettingsProvider.LoadUserSettings().SelectedMinerName;
                switch (minerAppName.NameEnum)
                {
                    case MinerAppName.MinerAppEnum.Phoenix:
                        this.ActiveMinerApp = _phoenixMiner;
                        break;
                    case MinerAppName.MinerAppEnum.TRex:
                        this.ActiveMinerApp = _trexMiner;
                        break;
                }
                var results = _benchmarkResultsProvider.LoadBenchmarkResults(minerAppName);
                if (results != null)
                {
                    _minerLiveStatus = results.liveStatus;
                }
            }
        }

        public bool IsRunning { get; private set; }

        public bool _requestStop = false;

        public double? TotalMhs
        {
            get
            {
                var enabledAndCapableGpus = _minerLiveStatus?.GPUs.Values.Where(gpu => gpu.IsEnabledByUser && gpu.IsReadyForMining);
                return enabledAndCapableGpus?.Sum(gpu => gpu.BenchmarkSpeed);
            }
        }

        private readonly double MINER_GPU_INFO_TIMEOUT = 20.0;
        private readonly double MINER_TOTAL_BENCHMARK_TIMEOUT = 240.0;
        private IBenchmarkResultsProvider _benchmarkResultsProvider;
        public async void AssessIfAntivirusIsBlocking(IMinerApp minerApp, MinerAppConfiguration minerAppConfiguration)
        {
            try
            {
                var totalPhoenixReportsNeeded = 2;
                DateTime benchmarkStartTime = DateTime.Now;
                _logger.LogInformation("AntiVirus status assesment...");
                var cc = new MinerBenchmark(minerApp, totalPhoenixReportsNeeded, logger: _logger);
                bool stopped = false;
                cc.ProblemWithExe += (reason) =>
                {

                    this.AntivirusStatus?.Invoke(reason);

                    cc.Stop();
                    stopped = true;
                };

                //cc.RunBenchmarkRecording(@"antivirus.pre_recording", isPreBenchmark: true);


                bool result = cc.RunPreBenchmark(minerApp, minerAppConfiguration);
                if (!result)
                {
                    _logger.LogError("PreBenchmark failed with error: " + cc.BenchmarkError);

                    return;
                }

                while (!cc.PreBenchmarkFinished)
                {


                    double timeElapsed = (DateTime.Now - benchmarkStartTime).TotalSeconds;

                    if (timeElapsed > MINER_GPU_INFO_TIMEOUT)
                    {


                        _logger.LogError("antivirus check failed timeElapsed > MINER_GPU_INFO_TIMEOUT: " + cc.BenchmarkError);
                        if (!stopped)
                        {
                            this.AntivirusStatus?.Invoke(ProblemWithExeFile.Timeout);
                            cc.Stop();
                        }
                        return;
                    }

                    if (cc.MinerParserPreBenchmark.GetLiveStatusCopy().GPUInfosParsed)
                    {
                        _logger.LogInformation("antivirus check finished succesfully - phoenix is not blocked");
                        if (!stopped)
                        {

                            this.AntivirusStatus?.Invoke(ProblemWithExeFile.None);
                            cc.Stop();
                        }
                        break;
                    }
                    await Task.Delay(30);
                }
            }
            catch (Exception er)
            {
                _logger.LogInformation("exception in antivir check routine : " + er);
            }
        }



        public async void StartBenchmark(IMinerApp minerApp, MinerAppConfiguration minerAppConfiguration, BenchmarkLiveStatus? externalLiveStatus)
        {
            ActiveMinerApp = minerApp;

            if (this._minerLiveStatus != null)
                this._minerLiveStatus.ProblemWithExeFile = ProblemWithExeFile.None;
            if (IsRunning)
            {
                return;
            }
            _requestStop = false;

            BenchmarkLiveStatus? baseLiveStatus = null;


            DateTime benchmarkStartTime = DateTime.Now;
            var walletAddress = _providerConfig.Config?.Account ?? "0x0000000000000000000000000000000000000001";
            var nodeName = _providerConfig.Config?.NodeName ?? "DefaultBenchmark";
            minerAppConfiguration.EthereumAddress = walletAddress;
            if (minerAppConfiguration.MiningMode == "ETH")
            {
                minerAppConfiguration.Pool = GolemUI.Properties.Settings.Default.DefaultProxy;
            }
            else if (minerAppConfiguration.MiningMode == "ETC")
            {
                minerAppConfiguration.Pool = GolemUI.Properties.Settings.Default.DefaultProxyLowMem;
            }
            else
            {
                throw new Exception("unknown mining mode, select ETH or ETC");
            }



            var totalPhoenixReportsNeeded = 5;

            IsRunning = true;
            OnPropertyChanged("IsRunning");

            bool preBenchmarkNeeded = !String.IsNullOrEmpty(minerAppConfiguration.Cards);

            var cc = new MinerBenchmark(minerApp, totalPhoenixReportsNeeded, logger: _logger);
            cc.ProblemWithExe += (reason) =>
            {
                this.ProblemWithExe?.Invoke(reason);
                if (this._minerLiveStatus != null)
                    _minerLiveStatus.ProblemWithExeFile = reason;
            };


            _logger.LogInformation("Benchmark started");



            try
            {
                if (preBenchmarkNeeded)
                {
                    _logger.LogInformation("PreBenchmarkNeeded cards: " + minerAppConfiguration.Cards + " niceness: " + minerAppConfiguration.Niceness);


                    bool result = cc.RunBenchmarkRecording(@"test.pre_recording", isPreBenchmark: true);

                    if (!result)
                    {
                        result = cc.RunPreBenchmark(minerApp, minerAppConfiguration);
                    }

                    if (!result)
                    {
                        if (_minerLiveStatus != null)
                        {
                            _minerLiveStatus.GPUs.Clear();
                            _minerLiveStatus.ErrorMsg = cc.BenchmarkError;
                            OnPropertyChanged("Status");
                            _logger.LogError("PreBenchmark failed with error: " + cc.BenchmarkError);

                        }
                        return;
                    }

                    while (!cc.PreBenchmarkFinished)
                    {
                        await Task.Delay(30);

                        double timeElapsed = (DateTime.Now - benchmarkStartTime).TotalSeconds;

                        if (timeElapsed > MINER_GPU_INFO_TIMEOUT)
                        {
                            cc.Stop();

                            _minerLiveStatus!.GPUs.Clear();
                            _minerLiveStatus!.ErrorMsg = "Failed to obtain card list";
                            OnPropertyChanged("Status");
                            _logger.LogError("PreBenchmark failed timeElapsed > MINER_GPU_INFO_TIMEOUT: " + cc.BenchmarkError);

                            return;
                        }

                        if (_requestStop)
                        {
                            cc.Stop();
                            _minerLiveStatus!.ErrorMsg = "Stopped by user";
                            OnPropertyChanged("Status");
                            _logger.LogError("PreBenchmark stopped by user.");

                            break;
                        }
                        _minerLiveStatus = cc.MinerParserPreBenchmark.GetLiveStatusCopy();
                        _minerLiveStatus.MergeUserSettingsFromExternalLiveStatus(externalLiveStatus);
                        baseLiveStatus = _minerLiveStatus;
                        OnPropertyChanged("Status");
                        if (_minerLiveStatus.GPUInfosParsed)
                        {
                            cc.Stop();
                            break;
                        }
                    }
                }
                await Task.Delay(30);



                if (preBenchmarkNeeded && _minerLiveStatus != null && _minerLiveStatus.GPUs.Count == 0)
                {
                    return;
                }

                benchmarkStartTime = DateTime.Now;

                {
                    bool result = cc.RunBenchmarkRecording(@"test.recording", isPreBenchmark: false);
                    if (!result)
                    {
                        result = cc.RunBenchmark(minerApp, minerAppConfiguration);
                    }
                    if (!result)
                    {
                        if (_minerLiveStatus != null)
                        {
                            _minerLiveStatus.GPUs.Clear();
                            _minerLiveStatus.ErrorMsg = cc.BenchmarkError;
                            OnPropertyChanged("Status");
                        }
                        return;
                    }
                }

                while (!cc.BenchmarkFinished && IsRunning)
                {
                    _minerLiveStatus = cc.MinerParserBenchmark.GetLiveStatusCopy();
                    if (minerAppConfiguration.MiningMode == "ETC")
                    {
                        _minerLiveStatus.LowMemoryMode = true;
                        foreach (var gpu in _minerLiveStatus.GPUs)
                        {
                            gpu.Value.LowMemoryMode = true;
                        }
                    }
                    else
                    {
                        _minerLiveStatus.LowMemoryMode = false;
                        foreach (var gpu in _minerLiveStatus.GPUs)
                        {
                            gpu.Value.LowMemoryMode = false;
                        }
                    }

                    bool allExpectedGPUsFound = false;
                    if (baseLiveStatus != null)
                    {
                        _minerLiveStatus.MergeFromBaseLiveStatus(baseLiveStatus, minerAppConfiguration.Cards, out allExpectedGPUsFound);
                    }
                    _minerLiveStatus.MergeUserSettingsFromExternalLiveStatus(externalLiveStatus);
                    OnPropertyChanged("Status");
                    OnPropertyChanged("TotalMhs");
                    if (_minerLiveStatus.NumberOfPhoenixPerfReports >= _minerLiveStatus.TotalPhoenixReportsBenchmark)
                    {
                        foreach (var gpu in _minerLiveStatus.GPUs)
                        {
                            gpu.Value.BenchmarkDoneForThrottlingLevel = gpu.Value.PhoenixPerformanceThrottling;
                        }

                        _logger.LogInformation("Benchmark succeeded.");
                        break;
                    }
                    if (_minerLiveStatus.GPUInfosParsed && _minerLiveStatus.GPUs.Count == 0)
                    {
                        _logger.LogError("Benchmark succeeded, but no cards found");
                        break;
                    }
                    await Task.Delay(100);

                    double timeElapsed = (DateTime.Now - benchmarkStartTime).TotalSeconds;

                    if (_requestStop)
                    {
                        cc.Stop();
                        _minerLiveStatus.ErrorMsg = "Stopped by user";
                        OnPropertyChanged("Status");
                        _logger.LogError("Benchmark stopped by user.");
                        break;
                    }
                    if (timeElapsed > MINER_GPU_INFO_TIMEOUT && !_minerLiveStatus.GPUInfosParsed)
                    {
                        cc.Stop();
                        _minerLiveStatus.ErrorMsg = "Timeout, cannot read gpu info";
                        OnPropertyChanged("Status");
                        _logger.LogError("Timeout, cannot read gpu info");
                        break;
                    }
                    if (timeElapsed > MINER_TOTAL_BENCHMARK_TIMEOUT)
                    {
                        cc.Stop();
                        _minerLiveStatus.ErrorMsg = "Timeout, benchmark taking too long time";
                        OnPropertyChanged("Status");
                        _logger.LogError("Benchmark timeout, total benchmark timeout");
                        break;
                    }

                    if (_minerLiveStatus.GPUInfosParsed && _minerLiveStatus.GPUs.Values.Count > 0)
                    {
                        bool allCardsEndedWithError = true;
                        foreach (var gpu in _minerLiveStatus.GPUs.Values)
                        {
                            if (String.IsNullOrEmpty(gpu.GPUError))
                            {
                                allCardsEndedWithError = false;
                            }
                        }
                        //sdgcb

                        if (allCardsEndedWithError && String.IsNullOrEmpty(_minerLiveStatus.ErrorMsg))
                        {
                            _minerLiveStatus.ErrorMsg = "Failed to validate cards";
                            break;
                        }

                        bool allCardsFinished = true;
                        foreach (var gpu in _minerLiveStatus.GPUs.Values)
                        {
                            if (!gpu.IsFinished && gpu.IsEnabledByUser)
                            {
                                allCardsFinished = false;
                            }
                        }
                        if (allCardsFinished)
                        {
                            _minerLiveStatus.ErrorMsg = "All cards finished";
                            break;
                        }

                    }
                }
            }
            finally
            {
                //todo: remove later on, right now I've let it stay here to  help test sentry
                //_logger.LogError("--test error");
                IsRunning = false;
                cc.Stop();
                if (_minerLiveStatus != null)
                {
                    _minerLiveStatus.BenchmarkFinished = true;
                    foreach (var gpu in _minerLiveStatus.GPUs.Values)
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
                        _minerLiveStatus = externalLiveStatus;
                    }
                }
                OnPropertyChanged("IsRunning");
                OnPropertyChanged("Status");
            }
        }

        public void Apply(BenchmarkLiveStatus liveStatus)
        {
            _minerLiveStatus = liveStatus;
            OnPropertyChanged("Status");
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
                liveStatus = _minerLiveStatus
            };
            _benchmarkResultsProvider.SaveBenchmarkResults(results, _userSettingsProvider.LoadUserSettings().SelectedMinerName);
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
                int count = _minerLiveStatus?.GPUs.Values.Where(x => x.IsEnabledByUser && x.IsReadyForMining).Count() ?? 0;
                return count > 0;
            }
        }

        public bool IsPhoenixMiningPossible
        {
            get
            {
                var gpus = _minerLiveStatus?.GPUs.Values;
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
