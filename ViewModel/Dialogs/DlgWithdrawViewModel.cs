using GolemUI.Interfaces;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.ViewModel.Dialogs
{
    public enum DlgWithdrawStatus { Pending, Ok, Error, None };
    public class DlgWithdrawViewModel : INotifyPropertyChanged
    {

        public DlgWithdrawViewModel(Interfaces.IPaymentService paymentService, Interfaces.IPriceProvider priceProvider)
        {
            _withdrawAddress = paymentService.Address;
            _amount = paymentService.State.BalanceOnL2;

            _paymentService = paymentService;
            _priceProvider = priceProvider;
        }

        public DlgWithdrawStatus TransactionStatus => DlgWithdrawStatus.None;
        string? _withdrawAddress ;
        public string WithdrawAddress
        {
            get => _withdrawAddress;
            set
            {
                _withdrawAddress = value;
            }
        }

        decimal? _amount;
        

        public decimal? Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged(nameof(Amount));
            }
        }

        public decimal? AvailableGLM => _paymentService.State.BalanceOnL2;
        public decimal AvailableUSD => _priceProvider.CoinValue(AvailableGLM ?? 0m, Model.Coin.GLM);



        bool _shouldTransferAllTokensToL1 = true;


        public bool ShouldTransferAllTokensToL1
        {
            get => _shouldTransferAllTokensToL1;
            set
            {
                _shouldTransferAllTokensToL1 = value;
            }
        }
        public string WithdrawTextStatus => "Withdraw success";

        public decimal MinAmount => 0;
        public decimal MaxAmount => AvailableGLM ?? 0m;

        public decimal TxFeeGLM => 1.2345m;
        public string TxFeeUSD => "$" + 2.46f.ToString("f2");
        public string AmountUSDasString => "$" + 99f.ToString("f4");
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string? propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        private readonly IPriceProvider _priceProvider;
        private readonly IPaymentService _paymentService;

    }
}
