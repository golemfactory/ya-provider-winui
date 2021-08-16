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

        private double _estimateFromSettings(double hashRate, Coin coin)
        {
            if (_remoteSettingsProvider.LoadRemoteSettings(out var settings))
            {
                var dayIncomePerGH = coin switch { Coin.ETC => settings.DayEtcPerGH, Coin.ETH => settings.DayEthPerGH, _ => null };
                if (dayIncomePerGH is double v)
                {

                    return hashRate * Convert.ToDouble(_priceProvider.CoinValue(Convert.ToDecimal(v * 0.001 * (settings.RequestorCoeff ?? 1.0)), coin));
                }
            }
            return 0;
        }

        public double HashRateToUSDPerDay(double hashRate, Coin coin)
        {
            if (_historyDataProvider.GetCurrentRequestorPayout(coin) is double payout)
            {
                var dailyUSDPerMh = _priceProvider.GLMPerGhsToUSDPerDay(payout);
                return dailyUSDPerMh * hashRate ?? 0.0;
            }
            return _estimateFromSettings(hashRate, coin);
        }
    }

}
