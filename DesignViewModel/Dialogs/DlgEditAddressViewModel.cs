using GolemUI.Interfaces;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GolemUI.DesignViewModel.Dialogs
{
    public class DlgEditAddressViewModel
    {
        string? _newAddress;
        public string NewAddress
        {
            get => "0x605Af04a3cF9a9162bcBaED33f3AfBf671064eE4";
            set
            {
                _newAddress = value;
            }
        }


        bool _shouldTransferFunds = true;
        public bool ShouldTransferFunds
        {
            get => _shouldTransferFunds;
            set
            {
                _shouldTransferFunds = value;
            }
        }
    }
}
