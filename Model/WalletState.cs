using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Model
{
    public class WalletState
    {
        public WalletState(string tickler)
        {
            Tickler = tickler;
        }
        public decimal? Balance { get; set; }

        public decimal? PendingBalance { get; set; }

        public decimal? BalanceOnL2 { get; set; }

        public string Tickler { get; set; }
    }
}
