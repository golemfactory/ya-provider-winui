using GolemUI.Interfaces;
using GolemUI.Settings;
using GolemUI.Src;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.ViewModel
{

    public class DashboardMainViewModel : INotifyPropertyChanged, ISavableLoadableDashboardPage
    {

        public DashboardMainViewModel(IPriceProvider priceProvider, IPaymentService paymentService, IProviderConfig providerConfig, IProcessControler processControler, Src.BenchmarkService benchmarkService)
        {
            _priceProvider = priceProvider;
            _paymentService = paymentService;
            _processController = processControler;
            _providerConfig = providerConfig;
            _benchmarkService = benchmarkService;

            _paymentService.PropertyChanged += OnPaymentServiceChanged;
            _providerConfig.PropertyChanged += OnProviderConfigChanged;
        }

        private void OnProviderConfigChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsMiningActive" || e.PropertyName == "IsCpuActive")
            {
                OnPropertyChanged(e.PropertyName);
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

            var benchmark = SettingsLoader.LoadBenchmarkFromFileOrDefault();

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
        }


        public void SaveData()
        {
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public IProcessControler Process => _processController;
        public decimal? Amount => _paymentService.State?.Balance;

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

        public bool IsMiningActive
        {
            get => _providerConfig.IsMiningActive;
            set
            {
                _providerConfig.IsMiningActive = value;
            }
        }

        public bool IsCpuActive
        {
            get => _providerConfig.IsCpuActive;
            set
            {
                _providerConfig.IsCpuActive = value;
            }
        }


        public int TotalCpuCount => _totalCpuCount;
        public int TotalGpuCount => _totalGpuCount;
        public int EnabledCpuCount => _enabledCpuCount;
        public int EnabledGpuCount => _enabledGpuCount;
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

        private void OnPropertyChanged(string? propertyName)
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
        private readonly IProcessControler _processController;
    }
}
