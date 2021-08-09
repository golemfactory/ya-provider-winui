using GolemUI.Claymore;
using GolemUI.Interfaces;
using GolemUI.Model;

using GolemUI.Src;
using GolemUI.Src.AppNotificationService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;


namespace GolemUI.ViewModel
{
    public class SettingsViewModel : INotifyPropertyChanged, ISavableLoadableDashboardPage
    {
        public event PageChangeRequestedEvent? PageChangeRequested;
        private readonly Command.Provider _provider;
        private readonly IProviderConfig _providerConfig;
        private readonly IPriceProvider _priceProvider;
        private readonly IProcessControler _processControler;
        private readonly IEstimatedProfitProvider _profitEstimator;
        private readonly IStatusProvider _statusProvider;
        private readonly BenchmarkService _benchmarkService;
        private BenchmarkResults _benchmarkSettings;
        private readonly IBenchmarkResultsProvider _benchmarkResultsProvider;
        public BenchmarkService BenchmarkService => _benchmarkService;
        public ObservableCollection<ClaymoreGpuStatus>? GpuList { get; set; }
        public string BenchmarkError { get; set; }

        //It's needed to prevent blinking button when transition to advanced window
        public bool _advancedSettingsButtonEnabled;
        public bool AdvancedSettingsButtonEnabled { 
            get => _advancedSettingsButtonEnabled;
            set
            {
                _advancedSettingsButtonEnabled = value;
                NotifyChange();
            }
        }

        private int _activeCpusCount = 0;
        private readonly int _totalCpusCount = 0;
        private readonly Interfaces.INotificationService _notificationService;
        public SettingsViewModel(IPriceProvider priceProvider, IProcessControler processControler, IStatusProvider statusProvider, Src.BenchmarkService benchmarkService, Command.Provider provider, IProviderConfig providerConfig, Interfaces.IEstimatedProfitProvider profitEstimator, IBenchmarkResultsProvider benchmarkResultsProvider, Interfaces.INotificationService notificationService)
        {
            _statusProvider = statusProvider;
            _processControler = processControler;
            GpuList = new ObservableCollection<ClaymoreGpuStatus>();
            _notificationService = notificationService;
            _benchmarkResultsProvider = benchmarkResultsProvider;
            _priceProvider = priceProvider;
            _provider = provider;
            _providerConfig = providerConfig;
            _benchmarkService = benchmarkService;
            _providerConfig.PropertyChanged += OnProviderCofigChanged;
            _benchmarkService.PropertyChanged += OnBenchmarkChanged;
            _profitEstimator = profitEstimator;
            _totalCpusCount = Src.CpuInfo.GetCpuCount(Src.CpuCountMode.Threads);
            BenchmarkError = "";

            ActiveCpusCount = 3;
            _benchmarkSettings = _benchmarkResultsProvider.LoadBenchmarkResults();
        }
        public void SwitchToAdvancedSettings()
        {
            AdvancedSettingsButtonEnabled = false;
            PageChangeRequested?.Invoke(DashboardViewModel.DashboardPages.PageDashboardSettingsAdv);
        }
        public void StartBenchmark()
        {
            SaveData();
            bool allEnabled = true;
            string cards = "";

            string niceness = "";

            var gpus = _benchmarkSettings.liveStatus?.GPUs.Values;
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

            ClaymoreLiveStatus? externalStatusCopy = (ClaymoreLiveStatus?)_benchmarkSettings.liveStatus?.Clone();
            BenchmarkService.StartBenchmark(cards, niceness, "", "", externalStatusCopy);
        }
        public void StopBenchmark()
        {
            BenchmarkService.StopBenchmark();
        }
        public void LoadData()
        {
            AdvancedSettingsButtonEnabled = true;
            GpuList?.Clear();
            _benchmarkSettings = _benchmarkResultsProvider.LoadBenchmarkResults();
            if (IsBenchmarkSettingsCorrupted()) return;
            _benchmarkSettings.liveStatus?.GPUs.ToList().Where(gpu => gpu.Value != null).ToList().ForEach(gpu =>
            {
                var val = gpu.Value;
                val.PropertyChanged += Val_PropertyChanged;
                GpuList?.Add(val);
            });
            NodeName = _providerConfig?.Config?.NodeName;
            var activeCpuCount = _providerConfig?.ActiveCpuCount ?? 0;
            if (activeCpuCount <= TotalCpusCount)
                ActiveCpusCount = activeCpuCount;
            else
                ActiveCpusCount = TotalCpusCount;

            NotifyChange("TotalCpusCountAsString");

        }

        private void Val_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedMiningMode")
            {
                if (sender is ClaymoreGpuStatus status)
                {
                    bool mining = false;
                    //var act = _statusProvider.Activities;
                    //if (act != null)
                    //{
                    //    Model.ActivityState? gminerState = act.Where(a => a.ExeUnit == "gminer"/* && a.State == Model.ActivityState.StateType.Ready*/).SingleOrDefault();
                    //    mining = gminerState != null;
                    //}
                    mining = _processControler.IsProviderRunning;
                    if (mining)
                        _notificationService.PushNotification(new SimpleNotificationObject(Tag.SettingsChanged, "applying settings (performance throttling changed to: " + PerformanceThrottlingEnumConverter.ConvertToString(status.SelectedMiningMode) + ")", 5000));
                }
            }
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
                var res = _benchmarkSettings.liveStatus?.GPUs.ToList().Find(x => x.Value.GpuNo == gpu.GpuNo);
                if (res != null && res.HasValue && !res.Equals(default(KeyValuePair<int, Claymore.ClaymoreGpuStatus>)))
                {
                    KeyValuePair<int, Claymore.ClaymoreGpuStatus> keyVal = res.Value;
                    keyVal.Value.IsEnabledByUser = gpu.IsEnabledByUser;
                    keyVal.Value.ClaymorePerformanceThrottling = gpu.ClaymorePerformanceThrottling;
                }
            });


            _providerConfig?.UpdateActiveCpuThreadsCount(ActiveCpusCount);
            var _ls = _benchmarkSettings.liveStatus;
            if (_ls != null)
            {
                _benchmarkService.Apply(_ls);
            }
            _benchmarkResultsProvider.SaveBenchmarkResults(_benchmarkSettings);
        }

        private void OnBenchmarkChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Status")
            {
                var _newGpus = _benchmarkService.Status?.GPUs.Values?.ToArray();
                //  if (_newGpus.Length == 0) return;
                if (_benchmarkService.Status?.GPUInfosParsed == true)
                    if (_newGpus != null && GpuList != null)
                    {
                        for (var i = 0; i < _newGpus.Length; ++i)
                        {
                            if (i < GpuList.Count)
                            {
                                _newGpus[i].IsEnabledByUser = GpuList[i].IsEnabledByUser;
                                _newGpus[i].ClaymorePerformanceThrottling = GpuList[i].ClaymorePerformanceThrottling;
                                GpuList[i] = _newGpus[i];
                            }
                            else
                            {
                                GpuList.Add(_newGpus[i]);
                            }
                        }
                        while (_newGpus.Length < GpuList.Count)
                        {
                            GpuList.RemoveAt(GpuList.Count - 1);
                        }
                    }

                BenchmarkError = _benchmarkService?.Status?.ErrorMsg ?? "";

                NotifyChange("GpuList"); // ok 
                NotifyChange("HashRate");
                NotifyChange("ExpectedProfit");
                NotifyChange("BenchmarkIsRunning");
                NotifyChange("BenchmarkReadyToRun");
                NotifyChange("BenchmarkError");
            }
            if (e.PropertyName == "IsRunning")
            {
                NotifyChange("BenchmarkReadyToRun");
                NotifyChange("BenchmarkIsRunning");
                NotifyChange("ExpectedProfit");

                if (!BenchmarkIsRunning && _benchmarkService != null)
                {
                    _benchmarkService.Save();
                    _benchmarkSettings = _benchmarkResultsProvider.LoadBenchmarkResults();

                    var _newGpus = _benchmarkService.Status?.GPUs.Values?.ToArray();
                    GpuList!.Clear();
                    if (_benchmarkService.Status?.GPUInfosParsed == true)
                    {
                        if (_newGpus != null)
                        {
                            for (var i = 0; i < _newGpus.Length; ++i)
                            {
                                if (i < GpuList!.Count)
                                {
                                    _newGpus[i].IsEnabledByUser = GpuList[i].IsEnabledByUser;
                                    _newGpus[i].ClaymorePerformanceThrottling = GpuList[i].ClaymorePerformanceThrottling;
                                    GpuList[i] = _newGpus[i];
                                }
                                else
                                {
                                    GpuList!.Add(_newGpus[i]);
                                }
                            }
                            while (_newGpus.Length < GpuList.Count)
                            {
                                GpuList!.RemoveAt(GpuList.Count - 1);
                            }
                        }
                    }
                    if (_benchmarkService.Status != null)
                    {
                        BenchmarkError = _benchmarkService?.Status?.ErrorMsg ?? "";
                    }
                    NotifyChange("GpuList"); // ok 
                    NotifyChange("HashRate");
                    NotifyChange("ExpectedProfit");
                    NotifyChange("BenchmarkIsRunning");
                    NotifyChange("BenchmarkReadyToRun");
                    NotifyChange("BenchmarkError");

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
        public string TotalCpusCountAsString => TotalCpusCount.ToString();
        public string ActiveCpusCountAsString => ActiveCpusCount.ToString();
        public int ActiveCpusCount
        {
            get => _activeCpusCount;
            set
            {
                _activeCpusCount = value;
                NotifyChange("ActiveCpusCount");
                NotifyChange("ActiveCpusCountAsString");

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
                    return (double)_priceProvider.CoinValue((decimal)_profitEstimator.HashRateToCoinPerDay((double)totalHr), IPriceProvider.Coin.ETH);
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
