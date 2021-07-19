using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Interfaces
{
    public interface IProviderConfig : INotifyPropertyChanged
    {
        Command.Config? Config { get; }

        void UpdateWalletAddress(string? walletAddress = null);
        void UpdateNodeName(string? value);
    }
}
