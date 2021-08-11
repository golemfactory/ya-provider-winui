using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Interfaces
{
    public interface IEstimatedProfitProvider
    {
        public enum Coin
        {
            ETH,
            ETC
        }

        public enum Currency
        {
            USD
        }

        public double HashRateToUSDPerDay(double hashRate, IEstimatedProfitProvider.Coin coin = IEstimatedProfitProvider.Coin.ETH);

        public void UpdateCurrentRequestorPayout(double glmPerGh, IEstimatedProfitProvider.Coin coin = IEstimatedProfitProvider.Coin.ETH);
    }
}

