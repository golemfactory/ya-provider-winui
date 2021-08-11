using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GolemUI.Interfaces;

namespace GolemUI.Src
{
    class StaticEstimatedEarningsProvider : IEstimatedProfitProvider
    {
        const double DAY_ETH_FOR_GH = 0.0235501168196185;

        const double DAY_ETC_FOR_GH = 0.925568;

        const double REQUESTOR_COEF = 0.66;

        public double HashRateToCoinPerDay(double hashRate, IEstimatedProfitProvider.Coin coin = IEstimatedProfitProvider.Coin.ETH)
        {
            throw new NotImplementedException();
        }

        public double HashRateToCurrencyPerDay(double hashRate, IEstimatedProfitProvider.Currency currency = IEstimatedProfitProvider.Currency.USD)
        {
            throw new NotImplementedException();
        }

        public void UpdateCurrentRequestorPayout(double pricePerGhDay, IEstimatedProfitProvider.Coin coin = IEstimatedProfitProvider.Coin.ETH)
        {
            throw new NotImplementedException();
        }

        double IEstimatedProfitProvider.HashRateToUSDPerDay(double hashRate, IEstimatedProfitProvider.Coin coin)
        {
            switch (coin)
            {
                case IEstimatedProfitProvider.Coin.ETC:
                    return DAY_ETC_FOR_GH * hashRate * 0.001 * REQUESTOR_COEF;
                case IEstimatedProfitProvider.Coin.ETH:
                    return DAY_ETH_FOR_GH * hashRate * 0.001 * REQUESTOR_COEF;
                default:
                    return 0;
            }
        }


    }
}
