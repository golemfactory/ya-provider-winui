using GolemUI.Interfaces;
using GolemUI.Model;
using GolemUI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Src
{

    class EstimatedEarningsProvider : IEstimatedProfitProvider
    {
        IRemoteSettingsProvider _remoteSettingsProvider;
        IPriceProvider _priceProvider;
        IHistoryDataProvider _historyDataProvider;

        public EstimatedEarningsProvider(IRemoteSettingsProvider remoteSettingsProvider, IPriceProvider priceProvider, IHistoryDataProvider historyDataProvider)
        {
            _remoteSettingsProvider = remoteSettingsProvider;
            _priceProvider = priceProvider;
            _historyDataProvider = historyDataProvider;
        }


        public double? ConvertGlmPerGhToDollarsPerDay(double? glmPerGh)
        {
            if (glmPerGh == null)
            {
                return null;
            }
            const double secondsInDay = 3600 * 24;
            const double GhToMh = 0.001;
            double valueInUsd = (double)_priceProvider.CoinValue((decimal)(glmPerGh.Value * secondsInDay * GhToMh), IPriceProvider.Coin.GLM);
            return valueInUsd;
        }

        public double? ConvertEthPerGhToDollarsPerDay(double? dayEthPerGh)
        {
            if (dayEthPerGh == null)
            {
                return null;
            }
            double ethValueInUsd = (double)_priceProvider.CoinValue(1.0m, IPriceProvider.Coin.ETH);
            double valueInUSD = dayEthPerGh.Value * ethValueInUsd;
            return valueInUSD;
        }

        public double? ConvertEtcPerGhToDollarsPerDay(double? dayEtcPerGh)
        {
            if (dayEtcPerGh == null)
            {
                return null;
            }
            double ethValueInUsd = (double)_priceProvider.CoinValue(1.0m, IPriceProvider.Coin.ETC);
            double valueInUSD = dayEtcPerGh.Value * ethValueInUsd;
            return valueInUSD;
        }


        double IEstimatedProfitProvider.HashRateToUSDPerDay(double hashRate, IEstimatedProfitProvider.Coin coin)
        {
            return EarningsCalculator.HashRateToUSDPerDay(hashRate, _historyDataProvider, coin, _priceProvider, _remoteSettingsProvider);
        }


        //
    }

}
