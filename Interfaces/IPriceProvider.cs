using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Interfaces
{
    public interface IPriceProvider
    {
        public enum Coin
        {
            GLM,
            ETH,
            ETC
        }

        public enum Currency
        {
            USD
        }

        decimal CoinValue(decimal amount, Coin coin, Currency currency = Currency.USD);
    }
}
