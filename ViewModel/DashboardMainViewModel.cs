﻿using GolemUI.Interfaces;

using GolemUI.Src;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.ViewModel
{

    public class DashboardMainViewModel : INotifyPropertyChanged, ISavableLoadableDashboardPage
    {
        public DashboardMainViewModel(IPriceProvider priceProvider, IPaymentService paymentService, IProviderConfig providerConfig, IProcessControler processControler, Src.BenchmarkService benchmarkService, IBenchmarkResultsProvider benchmarkResultsProvider,
            IStatusProvider statusProvider, IHistoryDataProvider historyDataProvider)
        {
            _benchmarkResultsProvider = benchmarkResultsProvider;
            _priceProvider = priceProvider;
            _paymentService = paymentService;
            _processController = processControler;
            _providerConfig = providerConfig;
            _benchmarkService = benchmarkService;
            _statusProvider = statusProvider;
            _historyDataProvider = historyDataProvider;

            _paymentService.PropertyChanged += OnPaymentServiceChanged;
            _providerConfig.PropertyChanged += OnProviderConfigChanged;
            _statusProvider.PropertyChanged += OnActivityStatusChanged;
            _processController.PropertyChanged += OnProcessControllerChanged;

            _historyDataProvider.PropertyChanged += _historyDataProvider_PropertyChanged;
            _benchmarkService.PropertyChanged += _benchmarkService_PropertyChanged;
        }

        private void _benchmarkService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsRunning")
            {
                OnPropertyChanged(nameof(MiningReadyToRun));
                OnPropertyChanged(nameof(StartButtonExplanation));
            }
        }

        public bool MiningReadyToRun => !Process.IsStarting && !_benchmarkService.IsRunning;
        public string StartButtonExplanation
        {
            get
            {
                if (Process.IsStarting)
                    return "Please waint until all subsystems will initialize";
                if (_benchmarkService.IsRunning)
                    return "Can't start mining while benchmark is running";
                return "";
            }
        }



        private void _historyDataProvider_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveAgreementID")
            {
                OnPropertyChanged("ActiveAgreementID");
            }
            if (e.PropertyName == "EstimatedEarningsPerSecond")
            {
                if (_historyDataProvider.EstimatedEarningsPerSecond != null)
                {
                    var glmPerDay = (decimal)(_historyDataProvider.EstimatedEarningsPerSecond * 3600 * 24);
                    UsdPerDay = _priceProvider.CoinValue(glmPerDay, IPriceProvider.Coin.GLM);
                }
                else
                {
                    UsdPerDay = null;
                }
            }
            if (e.PropertyName == "EstimatedEarningsMessage")
            {
                EstimationMessage = _historyDataProvider.EstimatedEarningsMessage;
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
                OnPropertyChanged(nameof(MiningReadyToRun));
                OnPropertyChanged(nameof(StartButtonExplanation));
            }
        }


        public string? ActiveAgreementID
        {
            get
            {
                return _historyDataProvider.ActiveAgreementID;
            }
        }

        public string GpuStatus { get; private set; } = "Idle";

        private void OnActivityStatusChanged(object sender, PropertyChangedEventArgs e)
        {
            var act = _statusProvider.Activities;
            if (act == null)
            {
                return;
            }
            Debug.WriteLine(act.ToString());
            Model.ActivityState? gminerState = act.Where(a => a.ExeUnit == "gminer" && a.State == Model.ActivityState.StateType.Ready).SingleOrDefault();
            var isGpuMining = gminerState != null;
            var isCpuMining = act.Any(a => a.ExeUnit == "wasmtime" || a.ExeUnit == "vm" && a.State == Model.ActivityState.StateType.Ready);


            var gpuStatus = "Idle";
            if (gminerState?.Usage is Dictionary<string, float> usage)
            {
                if (usage.TryGetValue("golem.usage.mining.hash-rate", out var hashRate) && hashRate > 0.0)
                {
                    gpuStatus = $"running {hashRate:#.00} MH/s";
                }
                else
                {
                    gpuStatus = "running";
                }
            }
            if (GpuStatus != gpuStatus)
            {
                GpuStatus = gpuStatus;
                OnPropertyChanged("GpuStatus");
            }

            RefreshStatus();
        }

        private void RefreshStatus()
        {
            var isMining = _statusProvider.Activities.Any(a => a.State == Model.ActivityState.StateType.Ready);
            var newStatus = DashboardStatusEnum.Hidden;
            if (isMining)
            {
                newStatus = DashboardStatusEnum.Mining;
            }
            else if (_processController.IsProviderRunning && (IsCpuEnabled || IsGpuEnabled))
            {
                newStatus = DashboardStatusEnum.Ready;
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

        private void OnProviderConfigChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsMiningActive" || e.PropertyName == "IsCpuActive")
            {
                OnPropertyChanged(e.PropertyName);
                RefreshStatus();
            }
        }

        private void OnPaymentServiceChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("Amount");
            OnPropertyChanged("AmountUSD");
            OnPropertyChanged("PendingAmount");
            OnPropertyChanged("PendingAmountUSD");
        }

        public void LoadData()
        {

            var benchmark = _benchmarkResultsProvider.LoadBenchmarkResults();

            _enabledGpuCount = benchmark?.liveStatus?.GPUs.ToList().Where(gpu => gpu.Value != null && gpu.Value.IsReadyForMining && gpu.Value.IsEnabledByUser).Count() ?? 0;
            _totalGpuCount = benchmark?.liveStatus?.GPUs.ToList().Count() ?? 0;
            _totalCpuCount = Src.CpuInfo.GetCpuCount(Src.CpuCountMode.Threads);

            var activeCpuCount = _providerConfig?.ActiveCpuCount ?? 0;
            if (activeCpuCount <= _totalCpuCount)
                _enabledCpuCount = activeCpuCount;
            else
                _enabledCpuCount = _totalCpuCount;

            OnPropertyChanged("TotalCpuCount");
            OnPropertyChanged("TotalGpuCount");
            OnPropertyChanged("EnabledCpuCount");
            OnPropertyChanged("EnabledGpuCount");
            OnPropertyChanged("GpuCardsInfo");
            OnPropertyChanged("CpuCardsInfo");
        }


        public void SaveData()
        {
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event PageChangeRequestedEvent? PageChangeRequested;

        public IProcessControler Process => _processController;
        public decimal? Amount => _paymentService.State?.Balance;

        public decimal? _usdPerDay = null;
        public decimal? UsdPerDay
        {
            get => _usdPerDay;
            set
            {
                _usdPerDay = value;
                OnPropertyChanged();
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

        public decimal? AmountUSD => _priceProvider.CoinValue(Amount ?? 0, IPriceProvider.Coin.GLM);

        public decimal? PendingAmount => _paymentService.State?.PendingBalance;

        public decimal? PendingAmountUSD => _priceProvider.CoinValue(PendingAmount ?? 0m, IPriceProvider.Coin.GLM);
        public int _totalCpuCount;
        public int _totalGpuCount;
        public int _enabledGpuCount;
        public int _enabledCpuCount;

        public DashboardStatusEnum _status = DashboardStatusEnum.Ready;
        public DashboardStatusEnum Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        public bool IsGpuEnabled
        {
            get => _providerConfig.IsMiningActive;
            set
            {
                _providerConfig.IsMiningActive = value;
                OnPropertyChanged("IsGpuEnabled");
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

        public async void Start()
        {
            var extraClaymoreParams = _benchmarkService.ExtractClaymoreParams();

            await _processController.Start(_providerConfig.Network, extraClaymoreParams);
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
        private readonly IProcessControler _processController;
        private readonly IBenchmarkResultsProvider _benchmarkResultsProvider;
        private readonly IHistoryDataProvider _historyDataProvider;
    }
}
