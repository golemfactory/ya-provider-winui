using GolemUI.Interfaces;
using GolemUI.Model;
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
            RemoteSettings rs;

            double? dailyUSDPerMh;

            switch (coin)
            {
                case IEstimatedProfitProvider.Coin.ETC:
                    double? glmPerGhEtc = _historyDataProvider.GetCurrentRequestorPayout(IEstimatedProfitProvider.Coin.ETC);
                    if (glmPerGhEtc != null)
                    {
                        dailyUSDPerMh = ConvertGlmPerGhToDollarsPerDay(glmPerGhEtc.Value);
                    }
                    else
                    {
                        _remoteSettingsProvider.LoadRemoteSettings(out rs);
                        dailyUSDPerMh = rs.DayEtcPerGH.HasValue ? ConvertEtcPerGhToDollarsPerDay(rs.DayEtcPerGH.Value) : null;
                        if (rs.RequestorCoeff.HasValue && dailyUSDPerMh.HasValue)
                        {
                            dailyUSDPerMh *= rs.RequestorCoeff;
                        }
                    }
                    break;
                case IEstimatedProfitProvider.Coin.ETH:
                    double? glmPerGhEth = _historyDataProvider.GetCurrentRequestorPayout(IEstimatedProfitProvider.Coin.ETH);
                    if (glmPerGhEth != null)
                    {
                        dailyUSDPerMh = ConvertGlmPerGhToDollarsPerDay(glmPerGhEth.Value);
                    }
                    else
                    {
                        _remoteSettingsProvider.LoadRemoteSettings(out rs);
                        dailyUSDPerMh = rs.DayEthPerGH.HasValue ? ConvertEthPerGhToDollarsPerDay(rs.DayEthPerGH.Value) : null;
                        if (rs.RequestorCoeff.HasValue && dailyUSDPerMh.HasValue)
                        {
                            dailyUSDPerMh *= rs.RequestorCoeff;
                        }
                    }
                    break;
                default:
                    return 0;
            }

            return dailyUSDPerMh * hashRate ?? 0.0;

        }


        //
    }

}
