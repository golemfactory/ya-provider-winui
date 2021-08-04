using GolemUI.Command;
using GolemUI.Interfaces;
using GolemUI.Model;
using GolemUI.ViewModel.Dialogs;
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


    public class WalletViewModel : INotifyPropertyChanged, IDisposable, IDialogInvoker
    {
        private IPriceProvider _priceProvider;
        private IPaymentService _paymentService;
        private Command.Provider _provider;
        private readonly IProviderConfig? _providerConfig;
        private PropertyChangedEventHandler _handler;

        public event RequestDarkBackgroundEventHandler? DarkBackgroundRequested;


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

        public void RequestDarkBackgroundVisibilityChange(bool shouldBackgroundBeVisible)
        {
            DarkBackgroundRequested?.Invoke(shouldBackgroundBeVisible);
        }

        public async void UpdateAddress(DlgEditAddressViewModel.Action changeAction, string? address)
        {
            if (address == null)
            {
                return;
            }
            if (changeAction == DlgEditAddressViewModel.Action.TransferOut)
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

        private string? _asCheckSum(string? addr) => addr == null ? null : AddressUtil.Current.ConvertToChecksumAddress(addr);
        public string? WalletAddress => _asCheckSum(_paymentService.Address);

        public decimal Amount
        {
            get { return _amount; }
        }

        public decimal AmountUSD
        {
            get { return _priceProvider.CoinValue(_amount, IPriceProvider.Coin.GLM); }
        }

        public decimal PendingAmount
        {
            get { return _pendingAmount; }
        }

        public decimal PendingAmountUSD => _priceProvider.CoinValue(_pendingAmount, IPriceProvider.Coin.GLM);

        public decimal GlmPerDay
        {
            get
            {
                return _glmPerDay;
            }
        }

        public decimal UsdPerDay => _priceProvider.CoinValue(_glmPerDay, IPriceProvider.Coin.GLM);

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

        public DlgEditAddressViewModel EditModel => new DlgEditAddressViewModel(_paymentService);

        public void Dispose()
        {
            _paymentService.PropertyChanged -= _handler;
        }
    }
}
