using GolemUI.Interfaces;
using GolemUI.ViewModel.Dialogs;
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
        public bool IsValid => true;
        public DlgWithdrawStatus TransactionStatus => DlgWithdrawStatus.Ok;
        public string WithdrawTextStatus => "Withdraw success";
        string? _withdrawAddress;
        public string WithdrawAddress
        {
            get => _withdrawAddress ?? "";
            set
            {
                _withdrawAddress = value;
            }
        }

        decimal _amount = 66;
        public decimal Amount
        {
            get => _amount;
            set
            {
                _amount = value;
            }
        }

        public decimal AmountUSD => 15.15m;

        decimal _availableGLM = 73;
        public decimal AvailableGLM => _availableGLM;
        public decimal AvailableUSD => 16m;

        public decimal MinAmount => 0;
        public decimal MaxAmount => _availableGLM;

        public decimal TxFee => 0.0345m;
        public decimal TxFeeUSD => TxFee * 0.55m;


        public string? ZksyncUrl => null;
    }
}
