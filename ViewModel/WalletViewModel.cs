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
        private string _walletAddress;
        private decimal _amount;
        private decimal _pendingAmount;

        public WalletViewModel()
        {
            this._walletAddress = "0xa1a7c282badfa6bd188a6d42b5bc7fa1e836d1f8";
            this._amount = 0;
            this._pendingAmount = 0;
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

        public decimal PendingAmount
        {
            get { return _pendingAmount; }
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
