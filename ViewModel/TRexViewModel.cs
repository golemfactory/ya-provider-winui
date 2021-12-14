
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
        private TRexDetails.TRexJsonDetailsGpu _detailsGPU = new TRexDetails.TRexJsonDetailsGpu();
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

        public event PropertyChangedEventHandler? PropertyChanged;

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
        private readonly IUserSettingsProvider _userSettingsProvider;
        private readonly IBenchmarkResultsProvider _benchmarkResultsProvider;

        public ObservableCollection<TRexGpuStatus> GpuList { get; set; }

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

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(4);
            _timer.Tick += UpdateTRexData;
            _timer.Start();
        }

        private void OnDarkBackgroundRequested(bool _t)
        {

        }

        private async void UpdateTRexData(object sender, EventArgs? e)
        {
            //exception is handled in function calling TimerBasedUpdateTick
            if (_providerConfig.IsMiningActive && _benchmarkService.ActiveMinerApp?.MinerAppName.NameEnum == MinerAppName.MinerAppEnum.TRex)
            {
                using var client = new HttpClient();
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
            else
            {
                GpuList.Clear();
            }
        }


        public void LoadData()
        {
            GpuList.Clear();

            _benchmarkService.ReloadBenchmarkSettingsFromFile();

            NotifyChange("HashRate");
            NotifyChange("ExpectedProfit");
            NotifyChange("GPUMessage");


            UpdateTRexData(this, null);
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



        private void OnProviderCofigChanged(object sender, PropertyChangedEventArgs e)
        {
            //todo fix info


        }

        private void NotifyChange([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
