using GolemUI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Interfaces
{
    public interface IPaymentService : INotifyPropertyChanged
    {
        WalletState? State { get; }

        string Address { get; }

        string InternalAddress { get; }

        bool TransferOutTo(string address);

        bool SetAddress(string newAddress);
    }
}
