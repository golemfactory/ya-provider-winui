using GolemUI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Utils
{
    public static class PriceProviderExtension
    {
        public static decimal? GLM2USD(this Interfaces.IPriceProvider priceProvider, decimal? glm)
        {
            if (glm.HasValue)
            {
                return priceProvider.CoinValue(glm.Value, Model.Coin.GLM);
            }
            return null;
        }

        const decimal SECONDS_IN_DAY = 3600m * 24m;
        const decimal GH_TO_MH = 0.001m;

        public static double CoinValue(this Interfaces.IPriceProvider priceProvider, double amount, Coin coin, Currency currency = Currency.USD)
        {
            return Convert.ToDouble(priceProvider.CoinValue(Convert.ToDecimal(amount), coin, currency));
        }

        public static decimal? GLMPerGhsToUSDPerDay(this Interfaces.IPriceProvider priceProvider, decimal? glmPerGhs)
        {
            return glmPerGhs switch
            {
                null => null,
                decimal v => priceProvider.CoinValue(v * GH_TO_MH * SECONDS_IN_DAY, Model.Coin.GLM)
            };
        }

        private static double? ApplyInDecimal(Func<decimal?, decimal?> func, double? value) =>
            value switch { double v => func(Convert.ToDecimal(v)), null => null }
                  switch
            { decimal v => Convert.ToDouble(v), null => null };

        public static double? GLMPerGhsToUSDPerDay(this Interfaces.IPriceProvider priceProvider, double? glmPerGhs)
        {
            return ApplyInDecimal(v => priceProvider.GLMPerGhsToUSDPerDay(v), glmPerGhs);
        }
    }
}