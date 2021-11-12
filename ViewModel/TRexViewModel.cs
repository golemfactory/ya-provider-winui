
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
using System.Windows;
using System.Windows.Threading;
using Sentry;
using GolemUI.Miners;
using GolemUI.Miners.Phoenix;
using GolemUI.Miners.TRex;
using Newtonsoft.Json;
using MinerAppName = GolemUI.Miners.MinerAppName;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Windows.Controls;

namespace GolemUI.ViewModel
{
    public class TRexGpuStatus : INotifyPropertyChanged
    {
        private TRexDetails.TRexJsonDetailsGpu _detailsGPU;
        public TRexDetails.TRexJsonDetailsGpu DetailsGPU
        {
            get => _detailsGPU;
            set
            {
                _detailsGPU = value;
                NotifyChange();
                NotifyChange("ReportedHashrateMh");
                NotifyChange("GPUTitle");
                NotifyChange("Power");
                NotifyChange("SharesInfo");
            }
        }

        public int GPUNo { get; set; }
        public string GPUTitle
        {
            get
            {
                return $"GPU {GPUNo}: {_detailsGPU.name}";
            }
        }

        public double ReportedHashrateMh
        {
            get
            {
                if (_detailsGPU.hashrate_minute == null)
                {
                    return 0.0;
                }
                return _detailsGPU.hashrate_minute.Value / 1000000.0;
            }
        }

        public string Temperature
        {
            get
            {
                if (_detailsGPU.temperature == null)
                {
                    return "N/A";
                }

                return $"{_detailsGPU.temperature} °C";
            }
        }

        public string SharesInfo
        {
            get
            {
                if (_detailsGPU.shares?.accepted_count == null)
                {
                    return "N/A";
                }
                if (_detailsGPU.shares?.rejected_count == null)
                {
                    return "N/A";
                }
                if (_detailsGPU.shares?.invalid_count == null)
                {
                    return "N/A";
                }
                return $"(accepted: {_detailsGPU.shares?.accepted_count}, rejected/stale: {_detailsGPU.shares?.rejected_count}, invalid: {_detailsGPU.shares?.invalid_count})";
            }
        }

        public string Power
        {
            get
            {
                if (_detailsGPU.power == null)
                {
                    return "N/A";
                }

                return $"{_detailsGPU.power} W";
            }
        }

        private string _name = "";

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyChange([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class TRexViewModel : INotifyPropertyChanged, ISavableLoadableDashboardPage, IDialogInvoker
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

        public ObservableCollection<TRexGpuStatus> GpuList { get; set; }
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

        public double CpuOpacity => IsCpuMiningEnabledByNetwork ? 1.0 : 0.2f;
        public bool IsCpuMiningEnabledByNetwork => false;



        private DispatcherTimer _timer;

        private TRexDetails _lastRexDetails = new TRexDetails();
        public TRexDetails LastRexDetails
        {
            get
            {
                return _lastRexDetails;
            }
            set
            {
                _lastRexDetails = value;
                NotifyChange();
            }
        }

        private readonly Interfaces.INotificationService _notificationService;
        public event RequestDarkBackgroundEventHandler? DarkBackgroundRequested;

        private string _trexServerAddress = "http://127.0.0.1:4067/summary";
        private TRexMiner _trexMiner;
        private PhoenixMiner _phoenixMiner;

        public TRexViewModel(IUserSettingsProvider userSettingsProvider, IPriceProvider priceProvider, IProcessController processController, IStatusProvider statusProvider, Src.BenchmarkService benchmarkService, Command.Provider provider, IProviderConfig providerConfig, Interfaces.IEstimatedProfitProvider profitEstimator, IBenchmarkResultsProvider benchmarkResultsProvider, Interfaces.INotificationService notificationService,
            TRexMiner trexMiner, PhoenixMiner phoenixMiner
        )
        {
            DarkBackgroundRequested += OnDarkBackgroundRequested;
            _trexMiner = trexMiner;
            _phoenixMiner = phoenixMiner;
            _userSettingsProvider = userSettingsProvider;
            _statusProvider = statusProvider;
            _processController = processController;
            GpuList = new ObservableCollection<TRexGpuStatus>();
            _notificationService = notificationService;
            _benchmarkResultsProvider = benchmarkResultsProvider;
            _priceProvider = priceProvider;
            _provider = provider;
            _providerConfig = providerConfig;
            _benchmarkService = benchmarkService;
            _providerConfig.PropertyChanged += OnProviderCofigChanged;
            _profitEstimator = profitEstimator;
            _totalCpusCount = Src.CpuInfo.GetCpuCount(Src.CpuCountMode.Threads);
            BenchmarkError = "";
            ActiveCpusCount = 3;
            _benchmarkSettings = _benchmarkResultsProvider.LoadBenchmarkResults(_userSettingsProvider.LoadUserSettings().SelectedMinerName);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(4);
            _timer.Tick += UpdateTRexData;
            _timer.Start();
        }

        private void OnDarkBackgroundRequested(bool _t)
        {

        }

        private async void UpdateTRexData(object sender, EventArgs e)
        {
            //exception is handled in function calling TimerBasedUpdateTick
            using (var client = new HttpClient())
            {
                try
                {
                    //httpAddress should look here like  (which is default t-rex address)
                    var result = await client.GetStringAsync(_trexServerAddress);
                    TRexDetails details = JsonConvert.DeserializeObject<TRexDetails>(result);

                    if (details.gpus != null)
                    {
                        int gpuNo = 0;
                        foreach (var gpu in details.gpus)
                        {
                            if (GpuList.Count < gpuNo + 1)
                            {
                                var trexGpuStatus = new TRexGpuStatus();
                                trexGpuStatus.DetailsGPU = gpu;
                                GpuList.Add(trexGpuStatus);
                            }
                            else
                            {
                                GpuList[gpuNo].DetailsGPU = gpu;
                            }

                            GpuList[gpuNo].GPUNo = gpuNo + 1;
                            gpuNo += 1;
                        }

                        while (GpuList.Count > gpuNo)
                        {
                            GpuList.RemoveAt(GpuList.Count - 1);
                        }
                    }

                    LastRexDetails = details;
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }

            }
            await Task.Delay(100);
        }


        public void LoadData()
        {
            AdvancedSettingsButtonEnabled = true;
            GpuList.Clear();

            _benchmarkService.ReloadBenchmarkSettingsFromFile();
            _benchmarkSettings = _benchmarkResultsProvider.LoadBenchmarkResults(_userSettingsProvider.LoadUserSettings().SelectedMinerName);

            if (_benchmarkSettings == null || _benchmarkSettings.liveStatus == null || _benchmarkSettings.liveStatus.GPUs == null)
            {
                NotifyChange("GPUMessage");
                return;
            }



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
            NotifyChange("GPUMessage");
        }



        public void SaveData()
        {

        }

        public void GetRidOfUnusedWarningsPlease()
        {
            DarkBackgroundRequested?.Invoke(false);
            PageChangeRequested?.Invoke(DashboardViewModel.DashboardPages.PageDashboardMain);
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
                    _providerConfig.SetMiningActive(value, _benchmarkService.Status?.LowMemoryMode ?? false);
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
                //var enabledAndCapableGpus = GpuList.Where(gpu => gpu.IsEnabledByUser && gpu.IsReadyForMining);
                //return enabledAndCapableGpus?.Sum(gpu => gpu.BenchmarkSpeed);
                return 0;
            }
        }

        public double? ExpectedProfit
        {
            get
            {
                var totalHr = HashRate;
                if (totalHr != null)
                {
                    var coin = (_benchmarkService.Status?.LowMemoryMode ?? false) ? Coin.ETC : Coin.ETH;
                    return (double)_profitEstimator.HashRateToUSDPerDay(totalHr.Value, coin);
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
