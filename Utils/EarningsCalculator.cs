using GolemUI.Interfaces;
using GolemUI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Utils
{
    public class EarningsCalculator
    {
        const decimal SECONDS_IN_DAY = 3600m * 24m;
        const decimal GH_TO_MH = 0.001m;

        public static double HashRateToUSDPerDay(double hashRate, IHistoryDataProvider historyDataProvider, Coin coin, IPriceProvider priceProvider)
        {
            double? dailyUSDPerMh = null;
            if (historyDataProvider.GetCurrentRequestorPayout(coin) is double payout)
            {
                dailyUSDPerMh = priceProvider.GLMPerGhsToUSDPerDay(payout);
            }
            return dailyUSDPerMh * hashRate ?? 0.0;
        }


    }
}
