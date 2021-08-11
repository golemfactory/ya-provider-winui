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
        public static double HashRateToUSDPerDay(double hashRate, IHistoryDataProvider historyDataProvider, IEstimatedProfitProvider.Coin coin, IPriceProvider priceProvider, IRemoteSettingsProvider? remoteSettingsProvider = null)
        {
            RemoteSettings rs;

            double? dailyUSDPerMh = null;

            switch (coin)
            {
                case IEstimatedProfitProvider.Coin.ETC:
                    double? glmPerGhEtc = historyDataProvider.GetCurrentRequestorPayout(IEstimatedProfitProvider.Coin.ETC);
                    if (glmPerGhEtc != null)
                    {
                        dailyUSDPerMh = ConvertGlmPerGhToDollarsPerDay(glmPerGhEtc.Value, priceProvider);
                    }
                    else if (remoteSettingsProvider != null)
                    {
                        remoteSettingsProvider.LoadRemoteSettings(out rs);
                        dailyUSDPerMh = rs.DayEtcPerGH.HasValue ? ConvertEtcPerGhToDollarsPerDay(rs.DayEtcPerGH.Value, priceProvider) : null;
                        if (rs.RequestorCoeff.HasValue && dailyUSDPerMh.HasValue)
                        {
                            dailyUSDPerMh *= rs.RequestorCoeff;
                        }
                    }
                    break;
                case IEstimatedProfitProvider.Coin.ETH:
                    double? glmPerGhEth = historyDataProvider.GetCurrentRequestorPayout(IEstimatedProfitProvider.Coin.ETH);
                    if (glmPerGhEth != null)
                    {
                        dailyUSDPerMh = ConvertGlmPerGhToDollarsPerDay(glmPerGhEth.Value, priceProvider);
                    }
                    else if (remoteSettingsProvider != null)
                    {
                        remoteSettingsProvider.LoadRemoteSettings(out rs);
                        dailyUSDPerMh = rs.DayEthPerGH.HasValue ? ConvertEthPerGhToDollarsPerDay(rs.DayEthPerGH.Value, priceProvider) : null;
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


        public static double? ConvertGlmPerGhToDollarsPerDay(double? glmPerGh, IPriceProvider priceProvider)
        {
            if (glmPerGh == null)
            {
                return null;
            }
            const double secondsInDay = 3600 * 24;
            const double GhToMh = 0.001;
            double valueInUsd = (double)priceProvider.CoinValue((decimal)(glmPerGh.Value * secondsInDay * GhToMh), IPriceProvider.Coin.GLM);
            return valueInUsd;
        }

        public static double? ConvertEthPerGhToDollarsPerDay(double? dayEthPerGh, IPriceProvider priceProvider)
        {
            if (dayEthPerGh == null)
            {
                return null;
            }
            const double GhToMh = 0.001;
            double ethValueInUsd = (double)priceProvider.CoinValue(1.0m, IPriceProvider.Coin.ETH);
            double valueInUSD = dayEthPerGh.Value * ethValueInUsd * GhToMh;
            return valueInUSD;
        }

        public static double? ConvertEtcPerGhToDollarsPerDay(double? dayEtcPerGh, IPriceProvider priceProvider)
        {
            if (dayEtcPerGh == null)
            {
                return null;
            }
            const double GhToMh = 0.001;
            double ethValueInUsd = (double)priceProvider.CoinValue(1.0m, IPriceProvider.Coin.ETC);
            double valueInUSD = dayEtcPerGh.Value * ethValueInUsd * GhToMh;
            return valueInUSD;
        }



    }
}
