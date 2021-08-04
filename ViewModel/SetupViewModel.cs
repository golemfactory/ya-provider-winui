using GolemUI.Claymore;
using GolemUI.Interfaces;
using GolemUI.Validators;
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

namespace GolemUI.ViewModel
{
    public class SetupViewModel : INotifyPropertyChanged
    {
        private readonly Interfaces.IProviderConfig _providerConfig;
        private readonly Src.BenchmarkService _benchmarkService;
        private readonly Interfaces.IEstimatedProfitProvider _profitEstimator;
        private readonly IProcessControler _processControler;
        private readonly IPriceProvider _priceProvider;
        private readonly IUserSettingsProvider _userSettingsProvider;


        public enum FlowSteps
        {
            Start = 0,
            Noob,
            Expert,
            OwnWallet
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

        private int _flow;

        private int _noobStep;

        private ExpertSteps _expertStep;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsDesingMode => false;

        private Nethereum.HdWallet.Wallet? _wallet = null;

        public SetupViewModel(Interfaces.IProviderConfig providerConfig,
            Src.BenchmarkService benchmarkService, Interfaces.IEstimatedProfitProvider profitEstimator, Interfaces.IProcessControler processControler, Interfaces.IPriceProvider priceProvider, IUserSettingsProvider userSettingsProvider)
        {
            _flow = 0;
            _noobStep = 0;
            _providerConfig = providerConfig;
            _benchmarkService = benchmarkService;
            _profitEstimator = profitEstimator;
            _processControler = processControler;
            _priceProvider = priceProvider;
            _userSettingsProvider = userSettingsProvider;



            _providerConfig.PropertyChanged += OnProviderConfigChanged;

            _benchmarkService.PropertyChanged += OnBenchmarkChanged;


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
                        NoobStep = (int)NoobSteps.Enjoy;
                    }
                }
                else if (_flow == (int)FlowSteps.OwnWallet)
                {
                    if (_expertStep == ExpertSteps.Benchmark && !BenchmarkIsRunning)
                    {
                        ExpertStep = (int)ExpertSteps.Enjoy;
                    }
                }
            }
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

        private ObservableCollection<Claymore.ClaymoreGpuStatus> _gpus = new ObservableCollection<Claymore.ClaymoreGpuStatus>();
        public ObservableCollection<Claymore.ClaymoreGpuStatus>? GPUs => _gpus;
        public string? BenchmarkError { get; set; }

        internal async void ActivateHdWallet()
        {



            if (_mnemo == null)
            {
                // TODO: Error message to user here.
                return;
            }

            var seed = _mnemo.ToString();


            if (_wallet != null)
            {
                NoobStep = (int)NoobSteps.Name;
            }
            else if (_processControler.IsServerRunning)
            {
                throw new Exception("should not happen");
            }
            else
            {
                _wallet = new Nethereum.HdWallet.Wallet(seed, "");
                var address = await _processControler.PrepareForKey(_wallet.GetPrivateKey(0));
                if (address == _wallet.GetAccount(0).Address.ToLower())
                {
                    _providerConfig.UpdateWalletAddress(address);
                    NoobStep = (int)NoobSteps.Name;
                }
            }
        }

        public float? TotalHashRate => _benchmarkService.TotalMhs;

        public double? ExpectedProfit
        {
            get
            {
                var totalHr = TotalHashRate;
                if (totalHr != null)
                {
                    return (double)_priceProvider.CoinValue((decimal)_profitEstimator.HashRateToCoinPerDay((double)totalHr), IPriceProvider.Coin.ETH);
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
            get => _providerConfig.Config?.NodeName;
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
