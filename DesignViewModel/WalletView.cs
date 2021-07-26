using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.DesignViewModel
{
    public class WalletView
    {
        public decimal Amount { get; }

        public decimal AmountUSD { get; }

        public decimal PendingAmount { get; }

        public decimal PendingAmountUSD { get; }

        public string WalletAddress { get; }

        public bool IsInternal { get; }

        public decimal GlmPerDay { get; }

        public decimal UsdPerDay { get; }

        public string Tickler { get; }

        public bool IsMiningActive { get; set; } = true;

        public bool IsCpuActive { get; set; } = false;



        public WalletView()
        {
            Amount = 13.166m;
            PendingAmount = 100;
            GlmPerDay = 5000m;

            var glmp = 0.23m;

            AmountUSD = Amount * glmp;
            UsdPerDay = 50m * glmp;
            PendingAmountUSD = PendingAmount * glmp;

            WalletAddress = "0xa1a7c282badfa6bd188a6d42b5bc7fa1e836d1f8";
            IsInternal = true;
            Tickler = "tGLM";
        }
    }
}
