using GolemUI.Interfaces;
using GolemUI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GolemUI.Interfaces.IEstimatedProfitProvider;

namespace GolemUI.Src
{

    class EstimatedEarningsProvider : IEstimatedProfitProvider
    {
        IRemoteSettingsProvider _remoteSettingsProvider;

        public EstimatedEarningsProvider(IRemoteSettingsProvider remoteSettingsProvider)
        {
            _remoteSettingsProvider = remoteSettingsProvider;
        }

        double? glmPerGhEtc = null;
        double? glmPerGhEth = null;

        public void UpdateCurrentRequestorPayout(double glmPerGh, IEstimatedProfitProvider.Coin coin = IEstimatedProfitProvider.Coin.ETH)
        {
            switch (coin)
            {
                case IEstimatedProfitProvider.Coin.ETC:
                    glmPerGhEtc = glmPerGh;
                    break;
                case IEstimatedProfitProvider.Coin.ETH:
                    glmPerGhEth = glmPerGh;
                    break;
                default:
                    break;
            }
        }



        double IEstimatedProfitProvider.HashRateToCoinPerDay(double hashRate, IEstimatedProfitProvider.Coin coin)
        {
            RemoteSettings rs;
            _remoteSettingsProvider.LoadRemoteSettings(out rs);

            double? pricePerGH;
            switch (coin)
            {
                case IEstimatedProfitProvider.Coin.ETC:
                    pricePerGH = overrideDayEtcPerGH != null ? overrideDayEtcPerGH : rs.DayEtcPerGH;
                    break;
                case IEstimatedProfitProvider.Coin.ETH:
                    pricePerGH = overrideDayEthPerGH != null ? overrideDayEthPerGH : rs.DayEthPerGH;
                    break;
                default:
                    return 0;
            }

            return pricePerGH * hashRate * rs.RequestorCoeff * 0.001 ?? 0.0;

        }

        double IEstimatedProfitProvider.HashRateToCurrencyPerDay(double hashRate, Currency currency)
        {
            RemoteSettings rs;
            _remoteSettingsProvider.LoadRemoteSettings(out rs);

            double? pricePerGH;
            switch (coin)
            {
                case IEstimatedProfitProvider.Coin.ETC:
                    pricePerGH = overrideDayEtcPerGH != null ? overrideDayEtcPerGH : rs.DayEtcPerGH;
                    break;
                case IEstimatedProfitProvider.Coin.ETH:
                    pricePerGH = overrideDayEthPerGH != null ? overrideDayEthPerGH : rs.DayEthPerGH;
                    break;
                default:
                    return 0;
            }

            return pricePerGH * hashRate * rs.RequestorCoeff * 0.001 ?? 0.0;

        }


        //
    }

}
