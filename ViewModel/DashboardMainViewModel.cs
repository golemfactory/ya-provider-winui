﻿using System;
using GolemUI.Converters;
using GolemUI.Interfaces;
using GolemUI.Model;
using GolemUI.Src;
using GolemUI.Src.AppNotificationService;
using GolemUI.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using static GolemUI.Interfaces.IHistoryDataProvider;

namespace GolemUI.ViewModel
{

    public class DashboardMainViewModel : INotifyPropertyChanged, ISavableLoadableDashboardPage, IDialogInvoker
    {
        public DashboardMainViewModel(IPriceProvider priceProvider, IPaymentService paymentService, IProviderConfig providerConfig, IProcessController processController, Src.BenchmarkService benchmarkService, IBenchmarkResultsProvider benchmarkResultsProvider,
            IStatusProvider statusProvider, IHistoryDataProvider historyDataProvider, INotificationService notificationService, IUserSettingsProvider userSettingsProvider,
            ITaskProfitEstimator taskProfitEstimator)
        {
            _userSettingsProvider = userSettingsProvider;
            _benchmarkResultsProvider = benchmarkResultsProvider;
            _priceProvider = priceProvider;
            _paymentService = paymentService;
            _processController = processController;
            _providerConfig = providerConfig;
            _benchmarkService = benchmarkService;
            _historyDataProvider = historyDataProvider;

            _benchmarkService.AntivirusStatus += _benchmarkService_AntivirusStatus;
            _statusProvider = statusProvider;
            _notificationService = notificationService;
            _taskProfitEstimator = taskProfitEstimator;

            _historyDataProvider.PropertyChanged += _historyDataProvider_PropertyChanged;
            _paymentService.PropertyChanged += OnPaymentServiceChanged;
            _providerConfig.PropertyChanged += OnProviderConfigChanged;
            _statusProvider.PropertyChanged += OnActivityStatusChanged;
            _processController.PropertyChanged += OnProcessControllerChanged;

            _benchmarkService.PropertyChanged += _benchmarkService_PropertyChanged;
            _taskProfitEstimator.PropertyChanged += _taskProfitEstimator_PropertyChanged;
        }


        public event RequestDarkBackgroundEventHandler? DarkBackgroundRequested;
        public void RequestDarkBackgroundVisibilityChange(bool shouldBackgroundBeVisible)
        {
            DarkBackgroundRequested?.Invoke(shouldBackgroundBeVisible);
        }
        private void _benchmarkService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsRunning")
            {
                OnPropertyChanged(nameof(IsMiningReadyToRun));
                OnPropertyChanged(nameof(StartButtonExplanation));
                OnPropertyChanged(nameof(IsBenchmarkNotRunning));
                OnPropertyChanged(nameof(IsAnyGpuEnabled));
                OnPropertyChanged(nameof(GpuOpacity));

                OnPropertyChanged(nameof(ShouldGpuSwitchBeEnabled));
            }
        }
        bool AntiVirusCheckActive { get; set; } = false;
        private bool MiningWasAlreadySuccessfull = false;
        public bool IsCpuMiningEnabledByNetwork => false;
        public bool IsAnyGpuEnabled => _benchmarkService.IsMiningPossibleWithCurrentSettings;
        public double GpuOpacity => _benchmarkService.IsMiningPossibleWithCurrentSettings ? 1.0 : 0.2f;
        public double CpuOpacity => IsCpuMiningEnabledByNetwork ? 1.0 : 0.2f;

        public bool IsMiningReadyToRun => !Process.IsStarting && !_benchmarkService.IsRunning && IsGpuEnabled && IsAnyGpuEnabled && !AntiVirusCheckActive;
        public bool IsBenchmarkNotRunning => !_benchmarkService.IsRunning;
        public bool ShouldGpuSwitchBeEnabled => IsBenchmarkNotRunning && IsAnyGpuEnabled;
        public string StartButtonExplanation
        {
            get
            {
                if (Process.IsStarting)
                {
                    return "Please wait until all subsystems are initialized.";
                }

                if (_benchmarkService.IsRunning)
                {
                    return "Can't start mining while benchmark is running.";
                }

                if (!_providerConfig.IsMiningActive)
                {
                    return "Can't start mining with GPU support disabled.";
                }

                if (!IsAnyGpuEnabled)
                {
                    return "At least one GPU card with mining capability must be enabled by user " +
                           "(Settings). You can rerun benchmark to determine gpu capabilities again.";
                }

                return "";

            }
        }

        private string _shareInfo = "Start mining to get info";

        public string ShareInfo
        {
            get => _shareInfo;
            set
            {
                _shareInfo = value;
                OnPropertyChanged();
            }
        }

        private void _historyDataProvider_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveAgreementID")
            {
                OnPropertyChanged("ActiveAgreementID");
            }

            if (e.PropertyName == "EarningsStats")
            {
                EarningsStatsType? es = _historyDataProvider.EarningsStats;
                if (es != null)
                {
                    ShareInfo = String.Format("Share info: {0} (stale: {1}, invalid: {2})", es.Shares, es.StaleShares, es.InvalidShares);
                }
                else
                {
                    ShareInfo = "No stats available yet";
                }
            }
        }

        private void _taskProfitEstimator_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "EstimatedEarningsPerSecondUSD")
            {
                if (_taskProfitEstimator.EstimatedEarningsPerSecondUSD != null)
                {
                    UsdPerDay = (decimal)(_taskProfitEstimator.EstimatedEarningsPerSecondUSD * 3600 * 24);
                }
                else
                {
                    UsdPerDay = null;
                }
            }
            if (e.PropertyName == "EstimatedEarningsPerSecondGLM")
            {
                if (_taskProfitEstimator.EstimatedEarningsPerSecondGLM != null)
                {
                    GlmPerDay = (decimal)(_taskProfitEstimator.EstimatedEarningsPerSecondGLM * 3600 * 24);
                }
                else
                {
                    GlmPerDay = null;
                }

            }
            if (e.PropertyName == "EstimatedEarningsMessage")
            {
                EstimationMessage = _taskProfitEstimator.EstimatedEarningsMessage;
            }
        }

        private void OnProcessControllerChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsProviderRunning")
            {
                RefreshStatus();
            }
            if (e.PropertyName == "IsStarting")
            {
                OnPropertyChanged(nameof(IsMiningReadyToRun));
                OnPropertyChanged(nameof(StartButtonExplanation));
            }
        }

        private string _gpuStatus = "Ready";

        public string GpuStatus
        {
            get
            {
                if (!IsAnyGpuEnabled)
                {
                    return "Disabled";
                }

                if (!IsGpuEnabled)
                {
                    return "Off";
                }

                return _gpuStatus;
            }
            set
            {
                if (_gpuStatus != value)
                {
                    _gpuStatus = value;
                    OnPropertyChanged("GpuStatus");
                }
            }
        }

        public string? GpuStatusAnnotation { get; private set; }

        public string CpuStatus
        {
            get
            {
                if (IsCpuMiningEnabledByNetwork)
                {
                    return "Ready";
                }
                else
                {
                    return "Disabled";
                }
            }
        }

        private void OnActivityStatusChanged(object sender, PropertyChangedEventArgs e)
        {
            var act = _statusProvider.Activities;
            if (act == null)
            {
                return;
            }
            Debug.WriteLine(act.ToString());
            Model.ActivityState? gminerState = act.Where(a => (a.ExeUnit == "gminer" || a.ExeUnit == "hminer") && a.State == Model.ActivityState.StateType.Ready).FirstOrDefault();
            var isGpuMining = gminerState != null;
            var isCpuMining = act.Any(a => a.ExeUnit == "wasmtime" || a.ExeUnit == "vm" && a.State == Model.ActivityState.StateType.Ready);


            var gpuStatus = "Ready";
            string? gpuStatusAnnotation = null;
            if (gminerState?.Usage is Dictionary<string, float> usage)
            {
                gpuStatus = "Mining";
                if (usage.TryGetValue("golem.usage.mining.hash-rate", out var hashRate) && hashRate > 0.0)
                {
                    HashRateConverter hashRateConverter = new HashRateConverter();
                    string hashrate = (string)hashRateConverter.Convert(hashRate, typeof(string), null, System.Globalization.CultureInfo.InvariantCulture);
                    gpuStatusAnnotation = $"Speed: {hashrate}";
                    MiningWasAlreadySuccessfull = true;
                }
            }
            GpuStatus = gpuStatus;
            if (gpuStatusAnnotation != GpuStatusAnnotation)
            {
                GpuStatusAnnotation = gpuStatusAnnotation;
                OnPropertyChanged("GpuStatusAnnotation");
            }
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            OnPropertyChanged(nameof(IsMiningReadyToRun));
            OnPropertyChanged(nameof(IsGpuEnabled));
            OnPropertyChanged(nameof(GpuStatus));
            OnPropertyChanged(nameof(StartButtonExplanation));
            var isMining = _statusProvider.Activities?.Any(a => a.State == Model.ActivityState.StateType.Ready) ??
                           false;
            var newStatus = DashboardStatusEnum.Hidden;
            var newMemoryStatus = _providerConfig.IsLowMemoryModeActive ? MiningMemoryMode.Above3GB : MiningMemoryMode.Above6Gb;
            if (isMining)
            {
                newStatus = DashboardStatusEnum.Mining;

            }
            else if (_processController.IsProviderRunning && (IsCpuEnabled || IsGpuEnabled))
            {
                newStatus = DashboardStatusEnum.Ready;
            }

            if (_statusMiningMemory != newMemoryStatus)
            {
                StatusMiningMemory = newMemoryStatus;
            }
            if (_status != newStatus)
            {
                Status = newStatus;
            }
        }


        public void SwitchToSettings()
        {
            PageChangeRequested?.Invoke(DashboardViewModel.DashboardPages.PageDashboardSettings);
        }
        public void SwitchToStatistics()
        {
            PageChangeRequested?.Invoke(DashboardViewModel.DashboardPages.PageDashboardStatistics);
        }

        private void OnProviderConfigChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsMiningActive")
            {
                OnPropertyChanged(nameof(IsGpuEnabled));
            }

            if (e.PropertyName == "IsCpuActive")
            {
                OnPropertyChanged(nameof(IsCpuEnabled));
            }

            if (e.PropertyName == "IsMiningActive" || e.PropertyName == "IsCpuActive")
            {
                RefreshStatus();
            }
        }

        private void OnPaymentServiceChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("Amount");
            OnPropertyChanged("AmountUSD");
            OnPropertyChanged("PendingAmount");
            OnPropertyChanged("PendingAmountUSD");
            OnPropertyChanged("PaymentStateError");
        }

        public void LoadData()
        {

            var benchmark = _benchmarkResultsProvider.LoadBenchmarkResults();

            _enabledGpuCount = benchmark?.liveStatus?.GPUs.ToList().Where(gpu => gpu.Value != null && gpu.Value.IsReadyForMining && gpu.Value.IsEnabledByUser).Count() ?? 0;
            _totalGpuCount = benchmark?.liveStatus?.GPUs.ToList().Count() ?? 0;
            _totalCpuCount = Src.CpuInfo.GetCpuCount(Src.CpuCountMode.Threads);

            var activeCpuCount = _providerConfig?.ActiveCpuCount ?? 0;
            if (activeCpuCount <= _totalCpuCount)
            {
                _enabledCpuCount = activeCpuCount;
            }
            else
            {
                _enabledCpuCount = _totalCpuCount;
            }

            OnPropertyChanged("TotalCpuCount");
            OnPropertyChanged("TotalGpuCount");
            OnPropertyChanged("EnabledCpuCount");
            OnPropertyChanged("EnabledGpuCount");
            OnPropertyChanged("GpuCardsInfo");
            OnPropertyChanged("CpuCardsInfo");
            OnPropertyChanged(nameof(ShouldGpuSwitchBeEnabled));
            OnPropertyChanged(nameof(IsAnyGpuEnabled));
            OnPropertyChanged(nameof(GpuOpacity));
            OnPropertyChanged(nameof(EnabledCpuCount));
            OnPropertyChanged(nameof(EnabledGpuCount));
            OnPropertyChanged(nameof(IsMiningReadyToRun));
            OnPropertyChanged(nameof(IsGpuEnabled));
            OnPropertyChanged(nameof(IsCpuEnabled));
            OnPropertyChanged(nameof(GpuStatus));
            OnPropertyChanged(nameof(StartButtonExplanation));

        }


        public void SaveData()
        {
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event PageChangeRequestedEvent? PageChangeRequested;

        public IProcessController Process => _processController;
        public decimal? Amount => _paymentService.State?.Balance;
        public string? PaymentStateError => _paymentService.LastError;

        private decimal? _usdPerDay = null;
        public decimal? UsdPerDay
        {
            get => _usdPerDay;
            set
            {
                _usdPerDay = value;
                OnPropertyChanged();
            }
        }

        private decimal? _glmPerDay = null;
        public decimal? GlmPerDay
        {
            get => _glmPerDay;
            set
            {
                _glmPerDay = value;
                OnPropertyChanged(nameof(GlmPerDay));
            }
        }

        public string _estimationMessage = "";
        public string EstimationMessage
        {
            get => _estimationMessage;
            set
            {
                _estimationMessage = value;
                OnPropertyChanged();
            }
        }

        public decimal? AmountUSD => _priceProvider.GLM2USD(Amount);

        public decimal? PendingAmount => _paymentService.State?.PendingBalance > 0 ? _paymentService.State?.PendingBalance : 0;

        public decimal? PendingAmountUSD => _priceProvider.GLM2USD(PendingAmount);
        public int _totalCpuCount;
        public int _totalGpuCount;
        public int _enabledGpuCount;
        public int _enabledCpuCount;

        public DashboardStatusEnum _status = DashboardStatusEnum.Hidden;
        public DashboardStatusEnum Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }
        public string StatusAdditionalInfo
        {
            get
            {
                return StatusMiningMemory switch
                {
                    MiningMemoryMode.None => "",
                    MiningMemoryMode.Above3GB => "4 GB mode",
                    MiningMemoryMode.Above6Gb => "",
                    _ => ""
                };
            }
        }
        public MiningMemoryMode _statusMiningMemory = MiningMemoryMode.None;
        public MiningMemoryMode StatusMiningMemory
        {
            get => _statusMiningMemory;
            set
            {
                _statusMiningMemory = value;
                OnPropertyChanged(nameof(StatusMiningMemory));
                OnPropertyChanged(nameof(StatusAdditionalInfo));
            }
        }

        public bool IsGpuEnabled
        {
            get => _providerConfig.IsMiningActive;
            set
            {
                if (_benchmarkService.IsMiningPossibleWithCurrentSettings || value == false)
                {

                    bool isLowMemoryMode = _userSettingsProvider.LoadUserSettings().ForceLowMemoryMode || (_benchmarkService.Status?.LowMemoryMode ?? false);

                    _providerConfig.SetMiningActive(value, isLowMemoryMode);

                    if (value == false)
                    {
                        if (_processController.IsProviderRunning)
                        {
                            _notificationService.PushNotification(new SimpleNotificationObject(Tag.AppStatus, "stopping GPU mining", expirationTimeInMs: 3000, group: false));
                            _processController.Stop();
                        }
                        else
                        {
                            _notificationService.PushNotification(new SimpleNotificationObject(Tag.AppStatus, "GPU mining deactivated", expirationTimeInMs: 3000, group: false));
                        }
                    }

                    OnPropertyChanged(nameof(IsMiningReadyToRun));
                    OnPropertyChanged(nameof(IsGpuEnabled));
                    OnPropertyChanged(nameof(GpuStatus));
                    OnPropertyChanged(nameof(StartButtonExplanation));
                }
                else
                {
                    _notificationService.PushNotification(new SimpleNotificationObject(Tag.AppStatus, "cannot turn on mining support - please enable at least one GPU with mining ability or re-run benchmark to check your hardware again", expirationTimeInMs: 6000, group: false));
                }

            }
        }

        public bool IsCpuEnabled
        {
            get => _providerConfig.IsCpuActive;
            set
            {
                _providerConfig.IsCpuActive = value;
                OnPropertyChanged("IsCpuEnabled");
            }
        }


        public int TotalCpuCount => _totalCpuCount;
        public int TotalGpuCount => _totalGpuCount;
        public int EnabledCpuCount => _enabledCpuCount;
        public int EnabledGpuCount => _enabledGpuCount;
        public string GpuCardsInfo => EnabledGpuCount + "/" + TotalGpuCount;
        public string CpuCardsInfo => EnabledCpuCount + "/" + TotalCpuCount;
        public void Stop()
        {
            _processController.Stop();
            //insta kill provider and gracefully shutdown yagna
        }

        private async void RunMiner()
        {
            var extraClaymoreParams = _benchmarkService.ExtractClaymoreParams();

            bool isLowMemoryMode = _userSettingsProvider.LoadUserSettings().ForceLowMemoryMode || (_benchmarkService.Status?.LowMemoryMode ?? false);

            _providerConfig.SwitchMiningMode(isLowMemoryMode);

            await _processController.Start(_providerConfig.Network, extraClaymoreParams);
        }
        public void Start()
        {
            if (MiningWasAlreadySuccessfull)
            {
                this.RunMiner();
            }
            else
            {
                AntiVirusCheckActive = true;
                OnPropertyChanged(nameof(IsMiningReadyToRun));
                _notificationService.PushNotification(new SimpleNotificationObject(Src.AppNotificationService.Tag.AppStatus, "checking system...", expirationTimeInMs: 5000, group: false));
                _benchmarkService.AssessIfAntivirusIsBlockingClaymore();

            }
        }
        private void _benchmarkService_AntivirusStatus(Command.ProblemWithExeFile problem)
        {
            AntiVirusCheckActive = false;
            OnPropertyChanged(nameof(IsMiningReadyToRun));
            if (problem == Command.ProblemWithExeFile.None)
            {
                _notificationService.PushNotification(new SimpleNotificationObject(Src.AppNotificationService.Tag.AppStatus, "ok", expirationTimeInMs: 3000, group: false));
                this.RunMiner();
            }
            else
            {
                _notificationService.PushNotification(new SimpleNotificationObject(Src.AppNotificationService.Tag.AppStatus, "detected problem: " + problem.ToString(), expirationTimeInMs: 10000, group: false));
                var settings = GolemUI.Properties.Settings.Default;
                var dlg = new UI.Dialogs.DlgGenericInformation(new ViewModel.Dialogs.DlgGenericInformationViewModel(
                    settings.dialog_antivir_image,
                    settings.dialog_antivir_title,
                    settings.dialog_antivir_message,
                    settings.dialog_antivir_button
                ));

                dlg.Owner = Application.Current.MainWindow;
                RequestDarkBackgroundVisibilityChange(true);
                bool? result = dlg?.ShowDialog();
                if (result == true)
                {
                    _notificationService.PushNotification(new SimpleNotificationObject(Src.AppNotificationService.Tag.AppStatus, "please check your antivirus settings...", expirationTimeInMs: 17000, group: false));
                }
                RequestDarkBackgroundVisibilityChange(false);
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        private readonly IPriceProvider _priceProvider;
        private readonly IPaymentService _paymentService;
        private readonly IProviderConfig _providerConfig;
        private readonly BenchmarkService _benchmarkService;
        private readonly IStatusProvider _statusProvider;
        private readonly IProcessController _processController;
        private readonly IBenchmarkResultsProvider _benchmarkResultsProvider;
        private readonly IUserSettingsProvider _userSettingsProvider;
        private readonly INotificationService _notificationService;
        private readonly ITaskProfitEstimator _taskProfitEstimator;
        private readonly IHistoryDataProvider _historyDataProvider;
    }
}
