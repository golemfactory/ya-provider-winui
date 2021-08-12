using GolemUI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoinGecko.Interfaces;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using GolemUI.Model;

namespace GolemUI.Src
{
    public class CoinGeckoPriceProvider : IPriceProvider, IDisposable
    {
        private static readonly Dictionary<string, Coin> _coinGeckoNames = new Dictionary<string, Coin>()
        {
            { "golem", Coin.GLM},
            { "ethereum", Coin.ETH },
            { "ethereum-classic", Coin.ETC }
        };

        private readonly ICoinGeckoClient _client;
        private readonly Dictionary<Coin, decimal> _prices = new Dictionary<Coin, decimal>();
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

        public decimal CoinValue(decimal amount, Coin coin, Currency currency = Currency.USD)
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
                var markets = await _client.CoinsClient.GetCoinMarkets("USD", _coinGeckoNames.Keys.ToArray<string>(), "", perPage: null, page: null, sparkline: false, priceChangePercentage: null, category: null);
                foreach (var market in markets)
                {
                    if (_coinGeckoNames.TryGetValue(market.Id, out Coin coin) && market.CurrentPrice is decimal price)
                    {
                        _prices[coin] = price;
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
