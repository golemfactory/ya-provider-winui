using GolemUI.Claymore;
using GolemUI.Interfaces;
using GolemUI.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;


namespace GolemUI
{
    public class SettingsViewModel : INotifyPropertyChanged, ISavableLoadableDashboardPage
    {

        private readonly Command.Provider _provider;
        private readonly IProviderConfig _providerConfig;
        private readonly IPriceProvider? _priceProvider;
        private readonly IEstimatedProfitProvider _profitEstimator;
        private readonly Src.BenchmarkService _benchmarkService;
        private BenchmarkResults? _benchmarkSettings;
        public Src.BenchmarkService BenchmarkService => _benchmarkService;
        public ObservableCollection<SingleGpuDescriptor> GpuList { get; set; }

        private int _activeCpusCount = 0;
        private readonly int _totalCpusCount = 0;
        public SettingsViewModel(IPriceProvider priceProvider, Src.BenchmarkService benchmarkService, Command.Provider provider, IProviderConfig providerConfig, Interfaces.IEstimatedProfitProvider profitEstimator)
        {
            GpuList = new ObservableCollection<SingleGpuDescriptor>();
            _priceProvider = priceProvider;
            _provider = provider;
            _providerConfig = providerConfig;
            _benchmarkService = benchmarkService;
            _providerConfig.PropertyChanged += OnProviderCofigChanged;
            _benchmarkService.PropertyChanged += OnBenchmarkChanged;
            _profitEstimator = profitEstimator;
            _totalCpusCount = Src.CpuInfo.GetCpuCount(Src.CpuCountMode.Threads);

            ActiveCpusCount = 3;
        }
        public void StartBenchmark()
        {
            SaveData();
            bool allEnabled = true;
            string cards = "";

            string niceness = "";

            var gpus = _benchmarkSettings?.liveStatus?.GPUs.Values;
            if (gpus != null)
            {
                foreach (var gpu in gpus)
                {
                    if (!gpu.IsEnabledByUser)
                    {
                        allEnabled = false;
                    }
                    else
                    {
                        if (cards != "")
                        {
                            cards += ",";
                            niceness += ",";
                        }
                        cards += gpu.GpuNo.ToString();
                        niceness += gpu.ClaymorePerformanceThrottling.ToString();
                    }
                }

            }
            if (allEnabled)
            {
                //If all cards are enabled benchmark prerun is not needed and no need to select cards
                cards = "";
            }

            ClaymoreLiveStatus? externalStatusCopy = (ClaymoreLiveStatus?)_benchmarkSettings?.liveStatus?.Clone();
            BenchmarkService.StartBenchmark(cards, niceness, "", "", externalStatusCopy);
        }
        public void StopBenchmark()
        {
            BenchmarkService.StopBenchmark();
        }
        public void LoadData()
        {
            GpuList?.Clear();
            _benchmarkSettings = SettingsLoader.LoadBenchmarkFromFileOrDefault();
            if (IsBenchmarkSettingsCorrupted()) return;
            _benchmarkSettings?.liveStatus?.GPUs.ToList().Where(gpu => gpu.Value != null).ToList().ForEach(gpu =>
            {
                var val = gpu.Value;
                GpuList?.Add(new SingleGpuDescriptor(val));
            });
            NodeName = _providerConfig?.Config?.NodeName;
            var activeCpuCount = _providerConfig?.ActiveCpuCount ?? 0;
            if (activeCpuCount <= TotalCpusCount)
                ActiveCpusCount = activeCpuCount;
            else
                ActiveCpusCount = TotalCpusCount;


        }

        private bool IsBenchmarkSettingsCorrupted()
        {
            return (_benchmarkSettings == null || _benchmarkSettings.liveStatus == null || _benchmarkSettings.liveStatus.GPUs == null);
        }
        public void SaveData()
        {

            GpuList?.ToList().ForEach(gpu =>
            {
                if (IsBenchmarkSettingsCorrupted()) return;
                var res = _benchmarkSettings?.liveStatus?.GPUs.ToList().Find(x => x.Value.GpuNo == gpu.Id);
                if (res != null && res.HasValue && !res.Equals(default(KeyValuePair<int, Claymore.ClaymoreGpuStatus>)))
                {
                    KeyValuePair<int, Claymore.ClaymoreGpuStatus> keyVal = res.Value;
                    keyVal.Value.IsEnabledByUser = gpu.IsActive;
                    keyVal.Value.ClaymorePerformanceThrottling = gpu.ClaymorePerformanceThrottling;
                }
            });


            _providerConfig?.UpdateActiveCpuThreadsCount(ActiveCpusCount);
            SettingsLoader.SaveBenchmarkToFile(_benchmarkSettings);
        }

        private void OnBenchmarkChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Status")
            {
                var _newGpus = _benchmarkService.Status?.GPUs.Values?.ToArray();
                //  if (_newGpus.Length == 0) return;
                if (_benchmarkService.Status?.GPUInfosParsed == true)
                    if (_newGpus != null)
                    {
                        for (var i = 0; i < _newGpus.Length; ++i)
                        {
                            if (i < GpuList.Count)
                            {
                                GpuList[i] = new SingleGpuDescriptor(_newGpus[i], GpuList[i].IsActive, GpuList[i].ClaymorePerformanceThrottling);
                            }
                            else
                            {
                                GpuList.Add(new SingleGpuDescriptor(_newGpus[i]));
                            }
                        }
                        while (_newGpus.Length < GpuList.Count)
                        {
                            GpuList.RemoveAt(GpuList.Count - 1);
                        }
                    }

                NotifyChange("GpuList"); // ok 
                NotifyChange("HashRate");
                NotifyChange("ExpectedProfit");
                NotifyChange("BenchmarkIsRunning");
                NotifyChange("BenchmarkReadyToRun");
            }
            if (e.PropertyName == "IsRunning")
            {
                NotifyChange("BenchmarkReadyToRun");
                NotifyChange("BenchmarkIsRunning");
                NotifyChange("ExpectedProfit");

                if (!BenchmarkIsRunning)
                {
                    _benchmarkService.Save();
                    _benchmarkSettings = SettingsLoader.LoadBenchmarkFromFileOrDefault();
                }
            }
        }
        public bool BenchmarkIsRunning => _benchmarkService.IsRunning;
        public bool BenchmarkReadyToRun => !(_benchmarkService.IsRunning);

        public bool IsMiningActive
        {
            get => _providerConfig?.IsMiningActive ?? false;
            set
            {
                _providerConfig.IsMiningActive = value;
            }
        }
        public bool IsCpuActive
        {
            get => _providerConfig?.IsCpuActive ?? false;
            set
            {
                _providerConfig.IsCpuActive = value;
            }
        }
        public int ActiveCpusCount
        {
            get => _activeCpusCount;
            set
            {
                _activeCpusCount = value;
                NotifyChange("ActiveCpusCount");
            }
        }
        public int TotalCpusCount => _totalCpusCount;
        public float? Hashrate => _benchmarkService.TotalMhs;
        public string? NodeName
        {
            get => _providerConfig?.Config?.NodeName;
            set
            {
                _providerConfig?.UpdateNodeName(value);
                NotifyChange("NodeName");
            }
        }
        public double? ExpectedProfit
        {
            get
            {
                var totalHr = Hashrate;
                if (totalHr != null)
                {
                    return (double)_priceProvider?.CoinValue((decimal)_profitEstimator.HashRateToCoinPerDay((double)totalHr), IPriceProvider.Coin.ETH);
                }
                return null;
            }
        }
        private void OnProviderCofigChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Config")
            {
                NotifyChange("NodeName");
            }
            if (e.PropertyName == "IsMiningActive" || e.PropertyName == "IsCpuActive")
            {
                NotifyChange(e.PropertyName);
            }
        }
        private void NotifyChange([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
