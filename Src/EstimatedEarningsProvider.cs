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

        double IEstimatedProfitProvider.HashRateToCoinPerDay(double hashRate, IEstimatedProfitProvider.Coin coin)
        {
            RemoteSettings rs;
            _remoteSettingsProvider.LoadRemoteSettings(out rs);

            switch (coin)
            {
                case IEstimatedProfitProvider.Coin.ETC:
                    return rs.DayEtcPerGH * hashRate * rs.RequestorCoeff * 0.001 ?? 0.0;
                case IEstimatedProfitProvider.Coin.ETH:
                    return rs.DayEthPerGH * hashRate * rs.RequestorCoeff * 0.001 ?? 0.0;
                default:
                    return 0;
            }
        }


        //
    }

}
