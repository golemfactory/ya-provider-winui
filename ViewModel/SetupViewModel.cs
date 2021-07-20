using NBitcoin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.ViewModel
{
    public class SetupViewModel : INotifyPropertyChanged
    {
        private readonly Interfaces.IProviderConfig _providerConfig;
        private readonly Src.BenchmarkService _benchmarkService;
        private readonly Interfaces.IEstimatedProfitProvider _profitEstimator;

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

        private int _flow;

        private int _noobStep;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsDesingMode => false;

        public SetupViewModel(Interfaces.IProviderConfig providerConfig, Src.BenchmarkService benchmarkService, Interfaces.IEstimatedProfitProvider profitEstimator)
        {
            _flow = 0;
            _noobStep = 0;
            _providerConfig = providerConfig;
            _benchmarkService = benchmarkService;
            _profitEstimator = profitEstimator;

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

                OnPropertyChanged("GPUs");
                OnPropertyChanged("TotalHashRate");
                OnPropertyChanged("ExpectedProfit");
            }
            if (e.PropertyName == "IsRunning")
            {
                OnPropertyChanged("BenchmarkIsRunning");
                OnPropertyChanged("ExpectedProfit");
                if (_noobStep == (int)NoobSteps.Benchmark && !BenchmarkIsRunning)
                {
                    NoobStep = (int)NoobSteps.Enjoy;
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
                _noobStep = value;
                OnPropertyChanged("NoobStep");
            }
        }

        public Src.BenchmarkService BenchmarkService => _benchmarkService;

        private ObservableCollection<Claymore.ClaymoreGpuStatus> _gpus = new ObservableCollection<Claymore.ClaymoreGpuStatus>();
        public ObservableCollection<Claymore.ClaymoreGpuStatus>? GPUs => _gpus;

        public float? TotalHashRate => _benchmarkService.TotalMhs;

        public double? ExpectedProfit
        {
            get
            {
                var totalHr = TotalHashRate;
                if (totalHr != null)
                {
                    return _profitEstimator.EthHashRateToDailyEarnigns((double)totalHr, Interfaces.MiningType.MiningTypeEth, Interfaces.MiningEarningsPeriod.MiningEarningsPeriodDay);
                }
                return null;
            }
        }

        public bool BenchmarkIsRunning => _benchmarkService.IsRunning;

        public void GoToStart()
        {
            Flow = 0;
        }
        public void GoToNoobFlow()
        {
            Flow = 1;
        }
        public void GoToExpertMode()
        {
            Flow = 2;
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

        public string[]? MnemonicWords => _mnemo?.Words;

        private void OnPropertyChanged(string? propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


    }
}
