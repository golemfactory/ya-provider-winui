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

        public EstimatedEarningsProvider(IRemoteSettingsProvider remoteSettingsProvider, IPriceProvider priceProvider)
        {
            _remoteSettingsProvider = remoteSettingsProvider;
            _priceProvider = priceProvider;
        }

        double? glmPerGhEtc = null;
        double? glmPerGhEth = null;


        public double? ConvertGlmPerGhToDollarsPerDay(double? glmPerGh)
        {
            if (glmPerGh == null)
            {
                return null;
            }
            const double secondsInDay = 3600 * 24;
            const double GhToMh = 0.001;
            double valueInUsd = _priceProvider.CoinValue((glmPerGh.Value * secondsInDay * GhToMh), IPriceProvider.Coin.GLM);
            return valueInUsd;
        }

        public double? ConvertEthPerGhToDollarsPerDay(double? dayEthPerGh)
        {
            if (dayEthPerGh == null)
            {
                return null;
            }
            double ethValueInUsd = _priceProvider.CoinValue(1.0, IPriceProvider.Coin.ETH);
            double valueInUSD = dayEthPerGh.Value * ethValueInUsd;
            return valueInUSD;
        }

        public double? ConvertEtcPerGhToDollarsPerDay(double? dayEtcPerGh)
        {
            if (dayEtcPerGh == null)
            {
                return null;
            }
            double ethValueInUsd = _priceProvider.CoinValue(1.0, IPriceProvider.Coin.ETC);
            double valueInUSD = dayEtcPerGh.Value * ethValueInUsd;
            return valueInUSD;
        }


        public void UpdateCurrentRequestorPayout(double golemPerGh, IEstimatedProfitProvider.Coin coin = IEstimatedProfitProvider.Coin.ETH)
        {
            switch (coin)
            {
                case IEstimatedProfitProvider.Coin.ETC:
                    glmPerGhEtc = golemPerGh;
                    break;
                case IEstimatedProfitProvider.Coin.ETH:
                    glmPerGhEth = golemPerGh;
                    break;
                default:
                    break;
            }
        }

        double IEstimatedProfitProvider.HashRateToUSDPerDay(double hashRate, IEstimatedProfitProvider.Coin coin)
        {
            RemoteSettings rs;

            double? dailyUSDPerMh;
            switch (coin)
            {
                case IEstimatedProfitProvider.Coin.ETC:
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
