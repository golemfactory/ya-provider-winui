using GolemUI.Interfaces;

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

        public double CoinValue(double amount, IPriceProvider.Coin coin, IPriceProvider.Currency currency = IPriceProvider.Currency.USD)
        {
            switch (coin)
            {
                case IPriceProvider.Coin.ETC:
                    return 47.76 * amount;
                case IPriceProvider.Coin.ETH:
                    return 2250.96 * amount;
                case IPriceProvider.Coin.GLM:
                    return 0.343176 * amount;
            }
            return 0.0;
        }
    }
}
