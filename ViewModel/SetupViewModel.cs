
using GolemUI.Interfaces;
using GolemUI.Validators;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GolemUI.Miners;
using GolemUI.Miners.Phoenix;
using static GolemUI.Command.GSB.Payment;

namespace GolemUI.ViewModel
{
    public delegate void RequestChangeBlackRectVisibilityEventHandler(bool visible);
    public class SetupViewModel : INotifyPropertyChanged
    {
        private readonly Interfaces.IProviderConfig _providerConfig;
        private readonly Src.BenchmarkService _benchmarkService;
        private readonly Interfaces.IEstimatedProfitProvider _profitEstimator;
        private readonly IProcessController _processController;
        private readonly IPriceProvider _priceProvider;
        private readonly IUserSettingsProvider _userSettingsProvider;


        public enum FlowSteps
        {
            Start = 0,
            Noob,
            Expert,
            OwnWallet,
            NoGPU,
            Antivirus
        }

        public enum NoobSteps
        {
            Prepare = 0,
            SeedPhase,
            SeedPhase2,
            Name,
            Benchmark,
            Enjoy

        }

        public enum ExpertSteps
        {
            Wallet = 0,
            Name,
            Benchmark,
            Enjoy

        }

        public enum GpuCardStatus
        {
            None,
            BenchmarkInProgress,
            AtLeastOneGoodGpu,
            NoSufficientGpuDetected
        }

        private int _flow;

        private int _noobStep;

        private ExpertSteps _expertStep;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsDesingMode => false;

        private Nethereum.HdWallet.Wallet? _wallet = null;

        private ILogger<SetupViewModel> _logger;
        private readonly IRemoteSettingsProvider _remoteSettingsProvider;


        private PhoenixMiner _miner;

        public SetupViewModel(Interfaces.IProviderConfig providerConfig,
            Src.BenchmarkService benchmarkService, Interfaces.IEstimatedProfitProvider profitEstimator, Interfaces.IProcessController processController, Interfaces.IPriceProvider priceProvider, IUserSettingsProvider userSettingsProvider, ILogger<SetupViewModel> logger,
            IRemoteSettingsProvider remoteSettingsProvider, PhoenixMiner miner)
        {
            _miner = miner;

            _flow = 0;
            _noobStep = 0;
            _providerConfig = providerConfig;
            _benchmarkService = benchmarkService;
            _profitEstimator = profitEstimator;
            _processController = processController;
            _priceProvider = priceProvider;
            _userSettingsProvider = userSettingsProvider;
            _logger = logger;
            _remoteSettingsProvider = remoteSettingsProvider;
            _providerConfig.PropertyChanged += OnProviderConfigChanged;
            _benchmarkService.PropertyChanged += OnBenchmarkChanged;
            _benchmarkService.ProblemWithExe += BenchmarkService_AntivirusStatus;

        }
        public void RemoveEventListeners()
        {
            _providerConfig.PropertyChanged -= OnProviderConfigChanged;
            _benchmarkService.PropertyChanged -= OnBenchmarkChanged;
            _benchmarkService.ProblemWithExe -= BenchmarkService_AntivirusStatus;
        }

        private int _lastFlowSteps = 0;
        bool AntiVirusDetectedBefore = false;
        public string AntivirusTitle { get; set; } = "Your antivirus is blocking Thorg";
        private void BenchmarkService_AntivirusStatus(ProblemWithExeFile problem)
        {
            if (problem == ProblemWithExeFile.Antivirus || problem == ProblemWithExeFile.FileMissing)
            {
                _lastFlowSteps = Flow;
                Flow = (int)FlowSteps.Antivirus;
                if (AntiVirusDetectedBefore)
                {
                    AntivirusTitle = "Your antivirus is still blocking Thorg";
                    OnPropertyChanged(nameof(AntivirusTitle));
                }
            }
        }

        public void TryAgainBenchmark()
        {
            Flow = _lastFlowSteps;

            int defaultBenchmarkStep = (int)PerformanceThrottlingEnumConverter.Default;
            MinerAppConfiguration minerAppConfiguration = new MinerAppConfiguration();
            minerAppConfiguration.Niceness = defaultBenchmarkStep.ToString();

            this.BenchmarkService.StartBenchmark(_miner, minerAppConfiguration, null);
            AntiVirusDetectedBefore = true;
        }

        private void OnBenchmarkChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Status")
            {
                var _newGpus = _benchmarkService.Status?.GPUs.Values?.ToArray();
                if (_newGpus != null)
                {
                    for (var i = 0; i < _newGpus.Length; ++i)
                    {
                        if (i < _gpus.Count)
                        {
                            _gpus[i] = _newGpus[i];
                        }
                        else
                        {
                            _gpus.Add(_newGpus[i]);
                        }
                    }
                    while (_newGpus.Length < _gpus.Count)
                    {
                        _gpus.RemoveAt(_gpus.Count - 1);
                    }
                }
                if (_benchmarkService.Status != null)
                {
                    BenchmarkError = _benchmarkService.Status.ErrorMsg;
                }

                OnPropertyChanged("GPUs");
                OnPropertyChanged("TotalHashRate");
                OnPropertyChanged("ExpectedProfit");
                OnPropertyChanged("BenchmarkError");
            }
            if (e.PropertyName == "IsRunning")
            {
                OnPropertyChanged("BenchmarkIsRunning");
                OnPropertyChanged("ExpectedProfit");
                if (_flow == (int)FlowSteps.Noob)
                {
                    if (_noobStep == (int)NoobSteps.Benchmark && !BenchmarkIsRunning)
                    {
                        if (AnySufficientGpusFound())
                            NoobStep = (int)NoobSteps.Enjoy;
                        else if (_benchmarkService.Status?.ProblemWithExeFile != ProblemWithExeFile.Antivirus && _benchmarkService.Status?.ProblemWithExeFile != ProblemWithExeFile.FileMissing)
                        {
                            if (_benchmarkService.Status?.LowMemoryMode ?? false)
                            {
                                Flow = (int)FlowSteps.NoGPU;
                            }
                            else
                            {
                                int defaultBenchmarkStep = (int)PerformanceThrottlingEnumConverter.Default;
                                MinerAppConfiguration minerAppConfiguration = new MinerAppConfiguration();
                                minerAppConfiguration.Niceness = defaultBenchmarkStep.ToString();
                                minerAppConfiguration.MiningMode = "ETC";

                                BenchmarkService.StartBenchmark(_miner, minerAppConfiguration, null);
                            }
                        }
                    }
                }
                else if (_flow == (int)FlowSteps.OwnWallet)
                {
                    if (_expertStep == ExpertSteps.Benchmark && !BenchmarkIsRunning)
                    {
                        if (AnySufficientGpusFound())
                            ExpertStep = (int)ExpertSteps.Enjoy;
                        else if (_benchmarkService.Status?.ProblemWithExeFile != ProblemWithExeFile.Antivirus && _benchmarkService.Status?.ProblemWithExeFile != ProblemWithExeFile.FileMissing)
                        {
                            if (_benchmarkService.Status?.LowMemoryMode ?? false)
                            {
                                Flow = (int)FlowSteps.NoGPU;
                            }
                            else
                            {
                                int defaultBenchmarkStep = (int)PerformanceThrottlingEnumConverter.Default;
                                MinerAppConfiguration minerAppConfiguration = new MinerAppConfiguration();
                                minerAppConfiguration.Niceness = defaultBenchmarkStep.ToString();
                                minerAppConfiguration.MiningMode = "ETC";

                                BenchmarkService.StartBenchmark(_miner, minerAppConfiguration, null);

                            }
                        }
                    }
                }
            }
        }
        bool AnySufficientGpusFound()
        {
            if (_benchmarkService.Status != null)
            {
                return _benchmarkService.Status?.GPUs.Values.Where(x => x.IsReadyForMining == true).Count() > 0;
            }
            return false;
        }
        private void OnProviderConfigChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "NodeName")
            {
                OnPropertyChanged("NodeName");
            }
        }

        public int Flow
        {
            get { return _flow; }
            set
            {
                _flow = value;
                OnPropertyChanged("Flow");
            }
        }

        public int NoobStep
        {
            get => _noobStep;
            set
            {
                if (BenchmarkIsRunning) _benchmarkService.StopBenchmark();
                _noobStep = value;
                OnPropertyChanged("NoobStep");
            }
        }

        public int ExpertStep
        {
            get => (int)_expertStep;
            set
            {
                if (BenchmarkIsRunning) _benchmarkService.StopBenchmark();
                _expertStep = (ExpertSteps)value;
                OnPropertyChanged("ExpertStep");
            }
        }

        public string? Address
        {
            get
            {
                if (_providerConfig?.Config?.Account == null)
                {
                    return null;
                }
                var addressUtil = new AddressUtil();
                return addressUtil.ConvertToChecksumAddress(_providerConfig.Config.Account);
            }
            set
            {
                _providerConfig?.UpdateWalletAddress(value);
            }
        }

        public Src.BenchmarkService BenchmarkService => _benchmarkService;

        private ObservableCollection<BenchmarkGpuStatus> _gpus = new ObservableCollection<BenchmarkGpuStatus>();
        public ObservableCollection<BenchmarkGpuStatus>? GPUs => _gpus;
        public string? BenchmarkError { get; set; }

        internal async Task<bool> ActivateHdWallet()
        {
            if (_mnemo == null)
            {
                // TODO: Error message to user here.
                _logger.LogError("_mnemo is null");
                return false;
            }

            var seed = _mnemo.ToString();

            if (_wallet != null)
            {
                NoobStep = (int)NoobSteps.Name;
                return true;
            }

            if (_processController.IsServerRunning)
            {
                _logger.LogError("Wallet is null and server is running and it shouldn't be possible");
                throw new Exception("Wallet is null and server is running and it shouldn't be possible");
            }

            _wallet = new Nethereum.HdWallet.Wallet(seed, "");
            var address = await _processController.PrepareForKey(_wallet.GetPrivateKey(0));
            if (address == _wallet.GetAccount(0).Address.ToLower())
            {
                _providerConfig.UpdateWalletAddress(address);
                NoobStep = (int)NoobSteps.Name;
            }
            return true;
        }



        public double? TotalHashRate => _benchmarkService.TotalMhs;

        public double? ExpectedProfit
        {
            get
            {
                var totalHr = TotalHashRate;
                if (totalHr != null)
                {
                    var coin = (_benchmarkService.Status?.LowMemoryMode ?? false) ? GolemUI.Model.Coin.ETC : GolemUI.Model.Coin.ETH;
                    return _profitEstimator.HashRateToUSDPerDay(totalHr.Value, coin);
                }
                return null;
            }
        }

        public bool BenchmarkIsRunning => _benchmarkService.IsRunning;

        public Visibility BackButtonVisibilty => Flow == 0 ? Visibility.Hidden : Visibility.Visible;

        public void GoToStart()
        {
            Flow = 0;
            OnPropertyChanged("BackButtonVisibilty");
        }
        public void GoToNoobFlow()
        {
            Flow = 1;
            OnPropertyChanged("BackButtonVisibilty");
        }
        public void GoToExpertMode()
        {
            Flow = 2;
            OnPropertyChanged("BackButtonVisibilty");
        }

        public string? NodeName
        {
            get
            {
                string? result = _providerConfig.Config?.NodeName;
                if (string.IsNullOrEmpty(result))
                {
                    NameGen gen = new NameGen();
                    result = gen.GenerateElvenName() + "-" + gen.GenerateElvenName();
                    _providerConfig.UpdateNodeName(result);
                }
                return result;
            }
            set
            {
                _providerConfig.UpdateNodeName(value);
            }
        }

        private Mnemonic? _mnemo;
        public void GenerateSeed()
        {
            if (_mnemo == null)
            {
                _mnemo = new Mnemonic(Wordlist.English, WordCount.Twelve);
                OnPropertyChanged("MnemonicWords");
            }
            NoobStep = 2;
        }

        public bool Save()
        {
            _benchmarkService.Save();
            var ls = _userSettingsProvider.LoadUserSettings();
            ls.SetupFinished = true;
            _userSettingsProvider.SaveUserSettings(ls);

            return true;
        }

        public string[]? MnemonicWords => _mnemo?.Words;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
