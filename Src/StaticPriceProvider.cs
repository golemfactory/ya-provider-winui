using GolemUI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Src
{
    public class StaticPriceProvider : IPriceProvider
    {
        public decimal CoinValue(decimal amount, IPriceProvider.Coin coin, IPriceProvider.Currency currency = IPriceProvider.Currency.USD)
        {
            switch (coin)
            {
                case IPriceProvider.Coin.ETC:
                    return 47.76m * amount;
                case IPriceProvider.Coin.ETH:
                    return 2250.96m * amount;
                case IPriceProvider.Coin.GLM:
                    return 0.343176m * amount;
            }
            return 0m;
        }

    }
}
