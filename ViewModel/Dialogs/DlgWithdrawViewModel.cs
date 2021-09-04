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

    public class OutputNetwork
    {
        public string Driver { get; }
        public string Name { get; }

        public string Description { get; }

        public OutputNetwork(string driver, string name, string description)
        {
            Driver = driver;
            Name = name;
            Description = description;
        }
    }

    public class DlgWithdrawViewModel : INotifyPropertyChanged
    {

        public DlgWithdrawViewModel(Interfaces.IPaymentService paymentService, Interfaces.IPriceProvider priceProvider)
        {
            _withdrawAddress = "";
            _amount = paymentService.State?.BalanceOnL2;

            _paymentService = paymentService;
            _priceProvider = priceProvider;
        }

        public DlgWithdrawStatus TransactionStatus => DlgWithdrawStatus.None;
        string? _withdrawAddress;
        public string WithdrawAddress
        {
            get => _withdrawAddress ?? "";
            set
            {
                _withdrawAddress = value;
                OnPropertyChanged("WithdrawAddress");
                OnPropertyChanged("IsValid");
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
                OnPropertyChanged("IsValid");
                OnPropertyChanged("AmountUSD");
            }
        }

        public decimal AmountUSD => _priceProvider.CoinValue(Amount ?? 0m, Model.Coin.GLM);

        public decimal? AvailableGLM => _paymentService.State?.BalanceOnL2;
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

        int _processing = 0;
        public bool IsProcessing => _processing > 0;

        private void _lock()
        {
            _processing += 1;
            if (_processing == 1)
            {
                OnPropertyChanged("IsProcessing");
            }
        }

        private void _unlock()
        {
            _processing -= 1;
            if (_processing == 0)
            {
                OnPropertyChanged("IsProcessing");
            }
        }

        public async Task UpdateTxFee()
        {
            _lock();
            try
            {
                if (_withdrawAddress != null)
                {
                    TxFee = await _paymentService.ExitFee(Amount, _withdrawAddress) * 1.1m;
                }
            }
            finally
            {
                _unlock();
            }
        }

        public void OpenZkSyncExplorer()
        {
            System.Diagnostics.Process.Start(ZksyncUrl);
        }

        public async Task<bool> SendTx()
        {
            if (_amount is decimal amount && _withdrawAddress is string withdrawAddress && TransferTo is OutputNetwork transferTo)
            {
                _lock();
                try
                {
                    if (transferTo.Driver == "erc20")
                    {
                        ZksyncUrl = await _paymentService.ExitTo("zksync", amount, withdrawAddress, TxFee);
                        OnPropertyChanged("ZksyncUrl");
                    }
                    else
                    {
                        await _paymentService.TransferTo("zksync", amount, withdrawAddress, TxFee);
                    }
                    return true;

                }
                finally
                {
                    _unlock();
                }
            }
            else
            {
                return false;
            }
        }

        public string? ZksyncUrl { get; private set; } = null;


        public string WithdrawTextStatus => "Withdraw success";

        public decimal MinAmount => 0;
        public decimal MaxAmount => AvailableGLM ?? 0m;

        public decimal _txFee = 0m;
        public decimal TxFee
        {
            get => _txFee;

            set
            {
                _txFee = value;
                OnPropertyChanged("TxFee");
                OnPropertyChanged("TxFeeUSD");
            }
        }
        public decimal TxFeeUSD => _priceProvider.CoinValue(TxFee, Model.Coin.GLM);

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string? propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private OutputNetwork[] _networks = new OutputNetwork[]
        {
        new("erc20", "L1 - Eth", "Make sure that you have this selected when withrdawing to the Crypto Exchange wallet address!"),
        new("zksync", "L2 - Zksync", "Transfer of funds to the provided zksync address")
        };

        public OutputNetwork[] Networks => _networks;

        private OutputNetwork? _transferTo = null;
        public OutputNetwork? TransferTo
        {
            get => _transferTo; set
            {
                _transferTo = value;
                OnPropertyChanged("TransferTo");
                OnPropertyChanged("IsValid");
            }
        }

        public bool IsValid => _transferTo != null && Amount != null && (Amount > 0m) && Amount <= MaxAmount && _withdrawAddress != "";

        private readonly IPriceProvider _priceProvider;
        private readonly IPaymentService _paymentService;

    }
}
