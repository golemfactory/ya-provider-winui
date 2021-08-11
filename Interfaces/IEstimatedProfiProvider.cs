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

        double HashRateToCoinPerDay(double hashRate, Coin coin = Coin.ETH);
        double HashRateToCurrencyPerDay(double hashRate, Currency currency = Currency.USD);

        public void UpdateCurrentRequestorPayout(double glmPerGh, IEstimatedProfitProvider.Coin coin = IEstimatedProfitProvider.Coin.ETH);
    }
}
