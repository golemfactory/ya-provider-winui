using GolemUI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Interfaces
{
    public interface IHistoryDataProvider : INotifyPropertyChanged
    {
        public class EarningsStatsType
        {
            public int Shares { get; set; }
            public TimeSpan Time { get; set; }

            public double AvgGlmPerSecond { get; set; }

        }

        PrettyChartData HashrateChartData { get; set; }

        public string? ActiveAgreementID { get; set; }

        public EarningsStatsType? EarningsStats { get; set; }

        public double? GetCurrentRequestorPayout(Coin coin = Coin.ETH);
    }
}

