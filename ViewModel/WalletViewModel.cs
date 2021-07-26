using GolemUI.Command;
using GolemUI.Interfaces;
using GolemUI.Model;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.ViewModel
{


    public class WalletViewModel : INotifyPropertyChanged, IDisposable
    {
        private IPriceProvider _priceProvider;
        private IPaymentService _paymentService;
        private Command.Provider _provider;
        private readonly IProviderConfig? _providerConfig;
        private PropertyChangedEventHandler _handler;


        private string? _walletAddress;
        private decimal _amount;
        private decimal _pendingAmount;
        private decimal _glmPerDay;

        public WalletViewModel(IPriceProvider priceProvider, IPaymentService paymentService, Command.Provider provider, IProviderConfig providerConfig)
        {
            _priceProvider = priceProvider;
            _paymentService = paymentService;
            _provider = provider;
            _providerConfig = providerConfig;

            var wallet = _providerConfig?.Config?.Account;

            this._walletAddress = wallet;
            this._amount = 0;
            this._pendingAmount = 0;
            this._glmPerDay = 0;
            this.Tickler = "GLM";
            var state = _paymentService.State;
            if (state != null)
            {
                this._amount = state.Balance ?? 0m;
                this._pendingAmount = state.PendingBalance ?? 0m;
            }
            _handler = this.OnPaymentStateChanged;
            paymentService.PropertyChanged += _handler;
        }

        public async void UpdateAddress(EditAddressViewModel.Action changeAction, string? address)
        {
            if (address == null)
            {
                return;
            }
            if (changeAction == EditAddressViewModel.Action.TransferOut)
            {
                var trnsferOut = _paymentService.TransferOutTo(address);
                _providerConfig?.UpdateWalletAddress(address);
                await trnsferOut;
            }
            else
            {
                _providerConfig?.UpdateWalletAddress(address);
            }
        }

        private void OnPaymentStateChanged(object? sender, PropertyChangedEventArgs e)
        {
            var state = _paymentService.State;

            if (state != null)
            {
                this._amount = state.Balance ?? 0m;
                this._pendingAmount = state.PendingBalance ?? 0m;
                this.Tickler = state.Tickler;
                OnPropertyChanged("Amount");
                OnPropertyChanged("AmountUSD");
                OnPropertyChanged("PendingAmount");
                OnPropertyChanged("PendingAmountUSD");
                OnPropertyChanged("Tickler");
            }
            if (e.PropertyName == "Address" || e.PropertyName == "InternalAddress")
            {
                OnPropertyChanged("WalletAddress");
                OnPropertyChanged("IsInternal");
            }
        }

        public string? WalletAddress => new AddressUtil().ConvertToChecksumAddress(_paymentService.Address);

        public decimal Amount
        {
            get { return _amount; }
        }

        public decimal AmountUSD
        {
            get { return _priceProvider.glmToUsd(_amount); }
        }

        public decimal PendingAmount
        {
            get { return _pendingAmount; }
        }

        public decimal PendingAmountUSD
        {
            get { return _priceProvider.glmToUsd(_pendingAmount); }
        }

        public decimal GlmPerDay
        {
            get
            {
                return _glmPerDay;
            }
        }

        public decimal UsdPerDay
        {
            get
            {
                return _priceProvider.glmToUsd(_glmPerDay);
            }
        }

        public string Tickler { get; private set; }

        public bool IsInternal => _paymentService.Address == _paymentService.InternalAddress;

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public EditAddressViewModel EditModel => new EditAddressViewModel(_paymentService);

        public void Dispose()
        {
            _paymentService.PropertyChanged -= _handler;
        }
    }
}
