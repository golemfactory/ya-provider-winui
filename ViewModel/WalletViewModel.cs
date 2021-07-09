using GolemUI.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.ViewModel
{
  

    public class WalletViewModel : INotifyPropertyChanged
    {
        private IPriceProvider _priceProvider;
        private string _walletAddress;
        private decimal _amount;
        private decimal _pendingAmount;
        private decimal _glmPerDay;

        public static WalletViewModel Example
        {
            get
            {
                return new WalletViewModel(new Src.StaticPriceProvider());
            }
        }

        public WalletViewModel(IPriceProvider priceProvider)
        {
            _priceProvider = priceProvider;
            this._walletAddress = "0xa1a7c282badfa6bd188a6d42b5bc7fa1e836d1f8";
            this._amount = 0;
            this._pendingAmount = 0;
            this._glmPerDay = 13;

        }

        public string WalletAddress
        {
            get
            {
                return this._walletAddress;
            }
            set
            {
                _walletAddress = value;
                OnPropertyChanged("WalletAddress");
            }
        }

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

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
