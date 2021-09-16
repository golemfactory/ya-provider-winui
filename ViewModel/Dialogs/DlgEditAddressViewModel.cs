using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.ViewModel.Dialogs
{
    public class DlgEditAddressViewModel : INotifyPropertyChanged
    {
        bool _shouldTransferFunds = false;
        public bool ShouldTransferFunds
        {
            get => _shouldTransferFunds;
            set
            {
                _shouldTransferFunds = value;
                OnPropertyChanged("ShouldTransferFunds");
            }
        }
        private bool _nodeNameHasChanged = false;
        public bool NodeNameHasChanged
        {
            get => _nodeNameHasChanged;
            set
            {
                _nodeNameHasChanged = value;
                OnPropertyChanged(nameof(NodeNameHasChanged));
            }
        }
        public string InternalAddress { get; }

        public bool HaveInternalBalance { get; }

        private string? _newAddress;
        public string? NewAddress
        {
            get
            {
                return _newAddress;
            }
            set
            {
                _newAddress = value;
                NodeNameHasChanged = true;
                OnPropertyChanged("NewAddress");
                OnPropertyChanged("IsInternal");
                OnPropertyChanged("CanTransferOut");
            }
        }

        public bool IsInternal => _newAddress?.Equals(InternalAddress, StringComparison.OrdinalIgnoreCase) ?? false;

        public bool CanTransferOut => !IsInternal && HaveInternalBalance;

        public DlgEditAddressViewModel(Interfaces.IPaymentService paymentService)
        {
            _newAddress = new AddressUtil().ConvertToChecksumAddress(paymentService.Address); // one time conversion while loading model
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

        public Action ChangeAction { get; set; } = Action.Change;

    }
}
