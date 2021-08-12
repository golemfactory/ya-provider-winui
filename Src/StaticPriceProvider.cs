using GolemUI.Interfaces;
using GolemUI.Model;
using System;

namespace GolemUI.Src
{
    public class StaticPriceProvider : IPriceProvider
    {
        public decimal CoinValue(decimal amount, Coin coin, Currency currency = Currency.USD) => amount * coin switch
        {
            Coin.ETC => 47.76m,
            Coin.ETH => 2250.96m,
            Coin.GLM => 0.343176m,
            var _id => throw new ArgumentException($"unnamed coin: id={_id}")
        };

    }
}
