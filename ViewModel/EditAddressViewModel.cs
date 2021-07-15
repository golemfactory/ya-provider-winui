using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.ViewModel
{
    public class EditAddressViewModel : INotifyPropertyChanged
    {
        public string InternalAddress { get; }

        public bool HaveInternalBalance { get; }

        private string? _address;
        public string? Address { get {
                return _address;
            }
            set {
                _address = value;
                OnPropertyChanged("Address");
                OnPropertyChanged("IsInternal");
                OnPropertyChanged("CanTransferOut");
            } 
        }

        public bool IsInternal => _address?.Equals(InternalAddress, StringComparison.OrdinalIgnoreCase) ?? false;

        public bool CanTransferOut => !IsInternal && HaveInternalBalance;

        public EditAddressViewModel(Interfaces.IPaymentService paymentService)
        {
            _address = paymentService.Address;
            InternalAddress = paymentService.InternalAddress;
            HaveInternalBalance = paymentService.State?.Balance > 0;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public enum Action
        {
            None, TransferOut, Change
        }

        public Action ChangeAction { get; set; }

    }
}
