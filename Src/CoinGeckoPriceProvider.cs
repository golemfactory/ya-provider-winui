using GolemUI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoinGecko.Interfaces;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;

namespace GolemUI.Src
{
    public class CoinGeckoPriceProvider : IPriceProvider, IDisposable
    {
        private readonly ICoinGeckoClient _client;
        private readonly Dictionary<IPriceProvider.Coin, decimal> _prices = new Dictionary<IPriceProvider.Coin, decimal>();
        private readonly DispatcherTimer _timer;
        private readonly ILogger _logger;


        public CoinGeckoPriceProvider(ILogger<CoinGeckoPriceProvider> logger)
        {
            _logger = logger;
            _client = CoinGecko.Clients.CoinGeckoClient.Instance;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromHours(1);
            _timer.Tick += OnRefreshTick;
            _timer.Start();
            Refresh();
        }

        private void OnRefreshTick(object sender, EventArgs e)
        {
            Refresh();
        }

        public decimal CoinValue(decimal amount, IPriceProvider.Coin coin, IPriceProvider.Currency currency = IPriceProvider.Currency.USD)
        {
            decimal value;
            if (_prices.TryGetValue(coin, out value))
            {
                return amount * value;
            }
            return 0;
        }

        public async void Refresh()
        {
            try
            {
                var markets = await _client.CoinsClient.GetCoinMarkets("USD", new string[] { "golem", "ethereum-classic", "ethereum" }, "", perPage: null, page: null, sparkline: false, priceChangePercentage: null, category: null);
                foreach (var market in markets)
                {
                    var price = market.CurrentPrice;
                    if (price == null)
                    {
                        continue;
                    }

                    switch (market.Id)
                    {
                        case "golem":
                            _prices[IPriceProvider.Coin.GLM] = price ?? 0m;
                            break;
                        case "ethereum":
                            _prices[IPriceProvider.Coin.ETH] = price ?? 0m;
                            break;
                        case "ethereum-classic":
                            _prices[IPriceProvider.Coin.ETC] = price ?? 0m;
                            break;
                    }
                }

            }
            catch (System.Net.Http.HttpRequestException)
            {
                _logger.LogError("no internet connection");
            }

        }

        public void Dispose()
        {
            _timer.Stop();
        }


    }
}
