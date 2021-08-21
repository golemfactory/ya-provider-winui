﻿using BetaMiner.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetaMiner.Interfaces
{
    public interface IPaymentService : INotifyPropertyChanged
    {
        WalletState? State { get; }

        string? Address { get; }

        string InternalAddress { get; }

        Task<bool> TransferOutTo(string address);

    }
}
