using GolemUI.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.ViewModel
{

    public class DashboardMainViewModel : INotifyPropertyChanged
    {
        public DashboardMainViewModel(IPriceProvider priceProvider, IPaymentService paymentService, IProviderConfig providerConfig, IProcessControler processControler)
        {
            _priceProvider = priceProvider;
            _paymentService = paymentService;
            _processController = processControler;
            _providerConfig = providerConfig;

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

        public event PropertyChangedEventHandler? PropertyChanged;

        public IProcessControler Process => _processController;
        public decimal? Amount => _paymentService.State?.Balance;

        public decimal? AmountUSD => _priceProvider.glmToUsd(Amount ?? 0);

        public decimal? PendingAmount => _paymentService.State?.PendingBalance;

        public decimal? PendingAmountUSD => _priceProvider.glmToUsd(PendingAmount ?? 0m);

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

        public void Stop()
        {
            _processController.Stop();
            //insta kill provider and gracefully shutdown yagna
            

        }

        public async void Start()
        {
            await _processController.Start();
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
        private readonly IProcessControler _processController;
    }
}
