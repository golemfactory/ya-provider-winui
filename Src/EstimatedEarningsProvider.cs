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

        public EstimatedEarningsProvider(IRemoteSettingsProvider remoteSettingsProvider)
        {
            _remoteSettingsProvider = remoteSettingsProvider;
        }

        double? overrideDayEtcPerGH = null;
        double? overrideDayEthPerGH = null;

        public void UpdateCurrentRequestorPayout(double pricePerGhDay, IEstimatedProfitProvider.Coin coin = IEstimatedProfitProvider.Coin.ETH)
        {
            switch (coin)
            {
                case IEstimatedProfitProvider.Coin.ETC:
                    overrideDayEtcPerGH = pricePerGhDay;
                    break;
                case IEstimatedProfitProvider.Coin.ETH:
                    overrideDayEthPerGH = pricePerGhDay;
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

            return rs.DayEtcPerGH * hashRate * rs.RequestorCoeff * 0.001 ?? 0.0;

        }


        //
    }

}
