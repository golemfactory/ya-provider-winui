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
    public class DlgWithdrawViewModel
    {
        string? _withdrawAddress = "0x605Af04a3cF9a9162bcBaED33f3AfBf671064eE4";
        public string WithdrawAddress
        {
            get => _withdrawAddress;
            set
            {
                _withdrawAddress = value;
            }
        }

        double _amount = 66;
        public double Amount
        {
            get => _amount;
            set
            {
                _amount = value;
            }
        }

        double _availableGLM = 73;
        public string AvailableGLM => _availableGLM.ToString("f4");


        double _availableUSD = 16;
        public string AvailableUSD => "$" + _availableUSD.ToString("f2");

        public double MinAmount => 0;
        public double MaxAmount => _availableGLM;

        public string TxFeeGLM => 1.2345f.ToString("f4");
        public string TxFeeUSD => "$" + 2.46f.ToString("f2");
        public string AmountGLMasString => Amount.ToString("f4");
        public string AmountUSDasString => "$" + 99f.ToString("f4");

        bool _shouldTransferAllTokensToL1 = true;
        public bool ShouldTransferAllTokensToL1
        {
            get => _shouldTransferAllTokensToL1;
            set
            {
                _shouldTransferAllTokensToL1 = value;
            }
        }
    }
}
