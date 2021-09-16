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
using System.Threading.Tasks;
using GolemUI.Utils;

namespace GolemUI.ViewModel
{
    public class SettingsViewModel : INotifyPropertyChanged, ISavableLoadableDashboardPage, IDialogInvoker
    {
        public bool ShouldRestartMiningAfterBenchmark = false;
        public event PageChangeRequestedEvent? PageChangeRequested;
        private readonly Command.Provider _provider;
        private readonly IProviderConfig _providerConfig;
        private readonly IPriceProvider _priceProvider;
        private readonly IProcessController _processController;
        private readonly IEstimatedProfitProvider _profitEstimator;
        private readonly IStatusProvider _statusProvider;
        private readonly BenchmarkService _benchmarkService;
        private BenchmarkResults _benchmarkSettings;
        private readonly IUserSettingsProvider _userSettingsProvider;
        private readonly IBenchmarkResultsProvider _benchmarkResultsProvider;
        public BenchmarkService BenchmarkService => _benchmarkService;
        public ObservableCollection<ClaymoreGpuStatus> GpuList { get; set; }
        public string BenchmarkError { get; set; }

        //It's needed to prevent blinking button when transition to advanced window
        public bool _advancedSettingsButtonEnabled;
        public bool AdvancedSettingsButtonEnabled
        {
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
        public event RequestDarkBackgroundEventHandler? DarkBackgroundRequested;
        public SettingsViewModel(IUserSettingsProvider userSettingsProvider, IPriceProvider priceProvider, IProcessController processController, IStatusProvider statusProvider, Src.BenchmarkService benchmarkService, Command.Provider provider, IProviderConfig providerConfig, Interfaces.IEstimatedProfitProvider profitEstimator, IBenchmarkResultsProvider benchmarkResultsProvider, Interfaces.INotificationService notificationService)
        {
            _userSettingsProvider = userSettingsProvider;
            _statusProvider = statusProvider;
            _processController = processController;
            _processController.PropertyChanged += _processController_PropertyChanged;
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

        private void _processController_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsProviderRunning")
            {
                NotifyChange(nameof(IsMiningProcessRunning));
            }
        }

        internal UserSettings UserSettings => _userSettingsProvider.LoadUserSettings();
        internal void UpdateBenchmarkDialogSettings(bool shouldAutoRestartMining, bool rememberMyPreference)
        {
            var settings = _userSettingsProvider.LoadUserSettings();
            settings.ShouldAutoRestartMiningAfterBenchmark = shouldAutoRestartMining;
            settings.ShouldDisplayNotificationsIfMiningIsActive = rememberMyPreference;
            _userSettingsProvider.SaveUserSettings(settings);
        }

        public void PushNotification(INotificationObject notification)
        {
            _notificationService.PushNotification(notification);
        }

        public void RequestDarkBackgroundVisibilityChange(bool shouldBackgroundBeVisible)
        {
            DarkBackgroundRequested?.Invoke(shouldBackgroundBeVisible);
        }
        public void SwitchToAdvancedSettings()
        {
            AdvancedSettingsButtonEnabled = false;
            PageChangeRequested?.Invoke(DashboardViewModel.DashboardPages.PageDashboardSettingsAdv);
        }

        public void StopMiningProcess()
        {
            _processController.Stop();
            _notificationService.PushNotification(new SimpleNotificationObject(Tag.AppStatus, "stopping mining...", expirationTimeInMs: 3000, group: false));
        }
        public void RestartMiningProcess()
        {

            var extraClaymoreParams = _benchmarkService.ExtractClaymoreParams();

            _processController.Start(_providerConfig.Network, extraClaymoreParams);
            _notificationService.PushNotification(new SimpleNotificationObject(Tag.AppStatus, "starting mining...", expirationTimeInMs: 3000, group: false));
        }

        public bool IsMiningProcessRunning
        {
            get
            {
                bool mining = false;
                mining = _processController.IsProviderRunning;
                return mining;
            }
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
            else
            {
                niceness = ((int)PerformanceThrottlingEnumConverter.Default).ToString();
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

        private bool _nodeNameHasChanged = false;
        public bool NodeNameHasChanged
        {
            get => _nodeNameHasChanged;
            set
            {
                _nodeNameHasChanged = value;
                NotifyChange(nameof(NodeNameHasChanged));
            }
        }

        public void LoadData()
        {
            NodeNameHasChanged = false;
            AdvancedSettingsButtonEnabled = true;
            GpuList.Clear();
            _benchmarkSettings = _benchmarkResultsProvider.LoadBenchmarkResults();

            if (_benchmarkSettings == null || _benchmarkSettings.liveStatus == null || _benchmarkSettings.liveStatus.GPUs == null)
            {
                return;
            }

            _benchmarkSettings.liveStatus?.GPUs.ToList().Where(gpu => gpu.Value != null).ToList().ForEach(gpu =>
            {
                var val = gpu.Value;
                val.PropertyChanged += Val_PropertyChanged;
                GpuList.Add(val);
            });

            var activeCpuCount = _providerConfig?.ActiveCpuCount ?? 0;
            if (activeCpuCount <= TotalCpusCount)
                ActiveCpusCount = activeCpuCount;
            else
                ActiveCpusCount = TotalCpusCount;

            NotifyChange("TotalCpusCountAsString");

            NotifyChange(nameof(IsCpuEnabled));
            NotifyChange(nameof(IsGpuEnabled));
            NotifyChange(nameof(BenchmarkReadyToRun));
            NotifyChange(nameof(ShouldGpuCheckBoxesBeEnabled));
            NotifyChange("HashRate");
            NotifyChange("ExpectedProfit");
        }

        void ChangeSettingsWithMiningRestart(string msg)
        {
            if (IsMiningProcessRunning)
            {
                _notificationService.PushNotification(new SimpleNotificationObject(Tag.SettingsChanged, msg, expirationTimeInMs: 2000));
                SaveData();
                StopMiningProcess();
                if (IsGpuEnabled)
                    Task.Delay(3000).ContinueWith(_ => RestartMiningProcess());

            }
        }
        private void Val_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ClaymoreGpuStatus status)
            {
                if (e.PropertyName == "SelectedMiningMode")
                {
                    ChangeSettingsWithMiningRestart("applying settings (gpu intensity changed to: " + PerformanceThrottlingEnumConverter.ConvertToString(status.SelectedMiningMode) + ")");
                }
                if (e.PropertyName == "IsEnabledByUser")
                {
                    bool isAnyCardEnabled = false;
                    GpuList.ToList().ForEach(x =>
                    {
                        isAnyCardEnabled = isAnyCardEnabled || (x.IsEnabledByUser /*&& x.IsReadyForMining*/);
                    });
                    if (!isAnyCardEnabled && IsGpuEnabled)
                    {
                        _notificationService.PushNotification(new SimpleNotificationObject(Tag.SettingsChanged, "GPU disabled since user disabled all cards, to enable mining again please activate at least one card and enable GPU support in Task type", expirationTimeInMs: 10000, group: false));
                        IsGpuEnabled = false;
                    }

                    ChangeSettingsWithMiningRestart("applying settings (card enabled: " + status.IsEnabledByUser.ToString() + ")");

                }
            }
            NotifyChange("HashRate");
            NotifyChange("ExpectedProfit");
        }

        public void SaveData()
        {
            GpuList?.ToList().ForEach(gpu =>
            {
                var res = _benchmarkSettings.liveStatus?.GPUs.ToList().Find(x => x.Value.GpuNo == gpu.GpuNo);
                if (res != null && res.HasValue && !res.Equals(default(KeyValuePair<int, Claymore.ClaymoreGpuStatus>)))
                {
                    KeyValuePair<int, Claymore.ClaymoreGpuStatus> keyVal = res.Value;
                    keyVal.Value.IsEnabledByUser = gpu.IsEnabledByUser;
                    keyVal.Value.BenchmarkSpeed = gpu.BenchmarkSpeed;
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
                if (_benchmarkService.Status?.GPUInfosParsed == true)
                {
                    var newGpus = _benchmarkService.Status?.GPUs.Values;
                    GpuList.CopyFromStandardCollection(newGpus);
                    foreach (var gpu in GpuList)
                    {
                        gpu.PropertyChanged += Val_PropertyChanged;
                    }
                }

                BenchmarkError = _benchmarkService?.Status?.ErrorMsg ?? "";

                NotifyChange("GpuList");
                NotifyChange("HashRate");
                NotifyChange("ExpectedProfit");
                NotifyChange("BenchmarkIsRunning");
                NotifyChange("BenchmarkReadyToRun");
                NotifyChange("BenchmarkError");
                NotifyChange(nameof(IsBenchmarkNotRunning));
            }
            if (e.PropertyName == "IsRunning")
            {
                NotifyChange("BenchmarkReadyToRun");
                NotifyChange("BenchmarkIsRunning");
                NotifyChange("ExpectedProfit");
                NotifyChange(nameof(IsBenchmarkNotRunning));
                if (!BenchmarkIsRunning && _benchmarkService != null)
                {
                    // finished ?

                    if (ShouldRestartMiningAfterBenchmark)
                        Task.Delay(3000).ContinueWith(_ => RestartMiningProcess());

                    _benchmarkService.Save();
                    _benchmarkSettings = _benchmarkResultsProvider.LoadBenchmarkResults();

                    var benchmarkStatus = _benchmarkService.Status;
                    if (benchmarkStatus != null)
                    {
                        var _newGpus = benchmarkStatus.GPUs.Values?.ToArray();

                        if (benchmarkStatus.GPUInfosParsed)
                        {
                            var newGpus = benchmarkStatus.GPUs.Values;
                            GpuList.CopyFromStandardCollection(newGpus);
                        }

                        BenchmarkError = benchmarkStatus.ErrorMsg ?? "";
                    }

                    NotifyChange("BenchmarkReadyToRun");
                    NotifyChange("BenchmarkIsRunning");
                    NotifyChange("ExpectedProfit");
                    NotifyChange("GpuList");
                    NotifyChange("HashRate");
                    NotifyChange("ExpectedProfit");
                    NotifyChange("BenchmarkIsRunning");
                    NotifyChange("BenchmarkReadyToRun");
                    NotifyChange("BenchmarkError");
                    NotifyChange(nameof(IsBenchmarkNotRunning));
                    NotifyChange(nameof(ShouldGpuCheckBoxesBeEnabled));
                    SaveData();
                }
            }
        }
        public bool BenchmarkIsRunning => _benchmarkService.IsRunning;
        public bool BenchmarkReadyToRun => !(_benchmarkService.IsRunning);
        public bool IsBenchmarkNotRunning => !(_benchmarkService.IsRunning);
        public bool ShouldGpuCheckBoxesBeEnabled => IsBenchmarkNotRunning && ((this._benchmarkService.Status?.GPUs?.Count ?? 0) > 1);
        public bool IsGpuEnabled
        {
            get => _providerConfig?.IsMiningActive ?? false;
            set
            {
                if (_benchmarkService.IsMiningPossibleWithCurrentSettings || value == false)
                {
                    _providerConfig.IsMiningActive = value;
                    if (value == false)
                    {
                        if (_processController.IsProviderRunning)
                        {
                            _processController.Stop();
                            _notificationService.PushNotification(new SimpleNotificationObject(Tag.AppStatus, "stopping GPU mining", expirationTimeInMs: 3000, group: false));
                        }
                        else
                        {
                            _notificationService.PushNotification(new SimpleNotificationObject(Tag.AppStatus, "GPU mining deactivated", expirationTimeInMs: 3000, group: false));
                        }
                    }
                    NotifyChange(nameof(IsGpuEnabled));
                    NotifyChange(nameof(IsBenchmarkNotRunning));
                    NotifyChange(nameof(BenchmarkReadyToRun));
                }
                else
                {
                    _notificationService.PushNotification(new SimpleNotificationObject(Tag.AppStatus, "cannot turn on mining support - please enable at least one GPU with mining ability or re-run benchmark to check your hardware again", expirationTimeInMs: 6000, group: false));
                }


            }
        }
        public bool IsCpuEnabled
        {
            get => _providerConfig?.IsCpuActive ?? false;
            set
            {
                _providerConfig.IsCpuActive = value;
                NotifyChange(nameof(IsCpuEnabled));
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
        public double? HashRate
        {
            get
            {
                var enabledAndCapableGpus = GpuList.Where(gpu => gpu.IsEnabledByUser && gpu.IsReadyForMining);
                return enabledAndCapableGpus?.Sum(gpu => gpu.BenchmarkSpeed);
            }
        }
        public string? NodeName
        {
            get => _providerConfig?.Config?.NodeName;
            set
            {
                NodeNameHasChanged = true;
                _providerConfig?.UpdateNodeName(value);
                NotifyChange("NodeName");
            }
        }
        public double? ExpectedProfit
        {
            get
            {
                var totalHr = HashRate;
                if (totalHr != null)
                {
                    return (double)_profitEstimator.HashRateToUSDPerDay(totalHr.Value, Coin.ETH);
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
            if (e.PropertyName == "IsMiningActive")
                NotifyChange(nameof(IsGpuEnabled));
            if (e.PropertyName == "IsCpuActive")
                NotifyChange(nameof(IsCpuEnabled));

        }
        private void NotifyChange([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
