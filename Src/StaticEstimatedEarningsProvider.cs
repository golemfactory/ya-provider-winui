using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GolemUI.Interfaces;
using GolemUI.Model;

namespace GolemUI.Src
{
    class StaticEstimatedEarningsProvider : IEstimatedProfitProvider
    {
        const double DAY_ETH_FOR_GH = 0.0235501168196185;

        const double DAY_ETC_FOR_GH = 0.925568;

        const double REQUESTOR_COEF = 0.66;

        double IEstimatedProfitProvider.HashRateToUSDPerDay(double hashRate, Coin coin) => hashRate * 0.001 * REQUESTOR_COEF * coin switch
        {
            Coin.ETC => DAY_ETC_FOR_GH,
            Coin.ETH => DAY_ETH_FOR_GH,
            _ => 0
        };



    }
}
