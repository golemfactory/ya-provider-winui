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
        private readonly ITaskProfitEstimator _taskProfitEstimator;
        private PropertyChangedEventHandler _handler;

        public event RequestDarkBackgroundEventHandler? DarkBackgroundRequested;


        private string? _walletAddress;
        private decimal _amount;
        private decimal _pendingAmount;

        public WalletViewModel(IPriceProvider priceProvider, IPaymentService paymentService, Command.Provider provider, IProviderConfig providerConfig,
            ITaskProfitEstimator taskProfitEstimator)
        {
            _priceProvider = priceProvider;
            _paymentService = paymentService;
            _provider = provider;
            _providerConfig = providerConfig;
            _taskProfitEstimator = taskProfitEstimator;

            var wallet = _providerConfig?.Config?.Account;

            this._walletAddress = wallet;
            this._amount = 0;
            this._pendingAmount = 0;
            this.Tickler = "GLM";
            var state = _paymentService.State;
            if (state != null)
            {
                this._amount = state.Balance ?? 0m;
                this._pendingAmount = state.PendingBalance ?? 0m;
            }
            _handler = this.OnPaymentStateChanged;
            paymentService.PropertyChanged += _handler;

            _taskProfitEstimator.PropertyChanged += OnProfitEstimatorChanged;
        }

        public string _estimationMessage = "";
        public string EstimationMessage
        {
            get => _estimationMessage;
            set
            {
                _estimationMessage = value;
                OnPropertyChanged(nameof(EstimationMessage));
            }
        }

        private void OnProfitEstimatorChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "EstimatedEarningsPerSecond")
            {
                if (_taskProfitEstimator.EstimatedEarningsPerSecond != null)
                {
                    var glmPerDay = (decimal)(_taskProfitEstimator.EstimatedEarningsPerSecond * 3600 * 24);
                    UsdPerDay = _priceProvider.CoinValue(glmPerDay, Coin.GLM);
                }
                else
                {
                    UsdPerDay = null;
                }
            }
            if (e.PropertyName == "EstimatedEarningsMessage")
            {
                EstimationMessage = _taskProfitEstimator.EstimatedEarningsMessage;
            }
            OnPropertyChanged("GlmPerDay");
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

        public decimal AmountUSD => _priceProvider.CoinValue(_amount, Coin.GLM);


        public decimal PendingAmount => _pendingAmount;

        public decimal PendingAmountUSD => _priceProvider.CoinValue(_pendingAmount, Coin.GLM);

        public decimal GlmPerDay => (decimal)((_taskProfitEstimator.EstimatedEarningsPerSecond ?? 0) * 3600 * 24);

        // public decimal UsdPerDay => _priceProvider.CoinValue(GlmPerDay, Coin.GLM);
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
        public DlgWithdrawViewModel WithDrawModel => new DlgWithdrawViewModel(_paymentService, _priceProvider);

        public void Dispose()
        {
            _paymentService.PropertyChanged -= _handler;
        }
    }
}
