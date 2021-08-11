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

        double IEstimatedProfitProvider.HashRateToUSDPerDay(double hashRate, IEstimatedProfitProvider.Coin coin)
        {
            return EarningsCalculator.HashRateToUSDPerDay(hashRate, _historyDataProvider, coin, _priceProvider, _remoteSettingsProvider);
        }
    }

}
