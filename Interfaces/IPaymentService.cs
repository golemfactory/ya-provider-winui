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
        WalletState? InternalWalletState { get; }

        string? LastError { get; }

        DateTime? LastSuccessfullRefresh { get; }
        string? Address { get; }

        string InternalAddress { get; }

        Task<bool> TransferOutTo(string address);

        Task<decimal> ExitFee(decimal? amount = null, string? to = null);

        Task<decimal> TransferFee(decimal? amount, string? to = null);

        Task Refresh();

        Task<string> ExitTo(string driver, decimal amount, string destinationAddress, decimal? txFee);

        Task<string> TransferTo(string driver, decimal amount, string destinationAddress, decimal? txFee);

    }
}
