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

        double HashRateToCoinPerDay(double hashRate, Coin coin = Coin.ETH);
        public void UpdateCurrentRequestorPayout(double pricePerGhDay, IEstimatedProfitProvider.Coin coin = IEstimatedProfitProvider.Coin.ETH);

    }
}
