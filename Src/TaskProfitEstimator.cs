using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GolemUI.Interfaces;
using GolemUI.Model;
using GolemUI.Utils;
using Microsoft.Extensions.Logging;

namespace GolemUI.Src
{
    class TaskProfitEstimator : ITaskProfitEstimator
    {
        private double? _estimatedEarningsPerSecondUSD;
        public double? EstimatedEarningsPerSecondUSD
        {
            get => _estimatedEarningsPerSecondUSD;
            set
            {
                if (_estimatedEarningsPerSecondUSD != value)
                {
                    _estimatedEarningsPerSecondUSD = value;
                    NotifyChanged();
                }
            }
        }

        //Maybe in future we want to show estimated profits in GLM also.
        private double? _estimatedEarningsPerSecondGLM;
        public double? EstimatedEarningsPerSecondGLM
        {
            get => _estimatedEarningsPerSecondGLM;
            set
            {
                if (_estimatedEarningsPerSecondGLM != value)
                {
                    _estimatedEarningsPerSecondGLM = value;
                    NotifyChanged();
                }
            }
        }


        private string _estimatedEarningsMessage = "";
        private readonly ILogger<TaskProfitEstimator> _logger;
        private readonly IHistoryDataProvider _historyDataProvider;
        private readonly IPriceProvider _priceProvider;
        private readonly IEstimatedProfitProvider _estimatedProfitProvider;
        private readonly IStatusProvider _statusProvider;
        private readonly IProcessController _processController;

        public TaskProfitEstimator(ILogger<TaskProfitEstimator> logger, IHistoryDataProvider historyDataProvider, IPriceProvider priceProvider,
            IEstimatedProfitProvider estimatedProfitProvider, IStatusProvider statusProvider, IProcessController processController)
        {
            _logger = logger;
            _historyDataProvider = historyDataProvider;
            _priceProvider = priceProvider;
            _estimatedProfitProvider = estimatedProfitProvider;
            _statusProvider = statusProvider;
            _historyDataProvider.PropertyChanged += OnHistoryDataProviderChanged;
            _statusProvider.PropertyChanged += OnHistoryDataProviderChanged;
            _processController = processController;

            _processController.PropertyChanged += OnProcessControllerChanged;
        }

        private void OnProcessControllerChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsProviderRunning")
            {
                Refresh();
            }
        }


        private void OnHistoryDataProviderChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((sender == _historyDataProvider && e.PropertyName == "EarningsStats") || sender == _statusProvider)
            {
                Refresh();
            }
        }

        public string EstimatedEarningsMessage
        {
            get => _estimatedEarningsMessage;
            set
            {
                if (_estimatedEarningsMessage != value)
                {
                    _estimatedEarningsMessage = value;
                    NotifyChanged();
                }
            }
        }

        private void Refresh()
        {
            if (!_processController.IsProviderRunning)
            {
                EstimatedEarningsPerSecondUSD = null;
                EstimatedEarningsPerSecondGLM = null;

                EstimatedEarningsMessage = $"Start mining to get estimates";
                return;
            }
            if (_historyDataProvider.EarningsStats is IHistoryDataProvider.EarningsStatsType stats)
            {
                EstimatedEarningsPerSecondUSD = _priceProvider.CoinValue(stats.AvgGlmPerSecond, Model.Coin.GLM);
                EstimatedEarningsPerSecondGLM = stats.AvgGlmPerSecond;

                int totalSecs = Convert.ToInt32(stats.Time.TotalSeconds + 0.5);
                int hours = totalSecs / 3600;
                int minutes = (totalSecs - hours * 3600) / 60;
                if (hours == 0)
                {
                    EstimatedEarningsMessage = $"Estimation based on {stats.Shares} shares mined during last {minutes} minutes.";
                }
                else
                {
                    EstimatedEarningsMessage = $"Estimation based on {stats.Shares} shares mined during last {hours} hours and {minutes} minutes.";
                }
            }
            else
            {
                const string HASH_RATE = "golem.usage.mining.hash-rate";
                var hashRateEth = _statusProvider.Activities
                    .Where(a => (a.ExeUnit == "gminer") && (a.Usage?.ContainsKey(HASH_RATE) ?? false))
                    .FirstOrDefault()?.Usage?[HASH_RATE];
                var hashRateEtc = _statusProvider.Activities
                    .Where(a => (a.ExeUnit == "hminer") && (a.Usage?.ContainsKey(HASH_RATE) ?? false))
                    .FirstOrDefault()?.Usage?[HASH_RATE];

                Debug.WriteLine(_statusProvider.Activities);
                var glm_usd_price = _priceProvider.CoinValue(1.0, Model.Coin.GLM);
                if (hashRateEth is float hr && hr > 0.0 && glm_usd_price > 0.0)
                {
                    EstimatedEarningsPerSecondUSD = _estimatedProfitProvider.HashRateToUSDPerDay(Convert.ToDouble(hr), Coin.ETH) / 3600.0 / 24.0;
                    EstimatedEarningsPerSecondGLM = EstimatedEarningsPerSecondUSD / glm_usd_price;
                    EstimatedEarningsMessage = $"Estimation based on current hashrate and requestor payout.";
                }
                else if (hashRateEtc is float hrc && hrc > 0.0 && glm_usd_price > 0.0)
                {
                    EstimatedEarningsPerSecondUSD = _estimatedProfitProvider.HashRateToUSDPerDay(Convert.ToDouble(hrc), Coin.ETC) / 3600.0 / 24.0;
                    EstimatedEarningsPerSecondGLM = EstimatedEarningsPerSecondUSD / glm_usd_price;
                    EstimatedEarningsMessage = $"Estimation based on current hashrate and requestor payout (low memory mode).";
                }
                else
                {
                    EstimatedEarningsPerSecondUSD = null;
                    EstimatedEarningsPerSecondGLM = null;

                    EstimatedEarningsMessage = $"Estimation in progress...";
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyChanged([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
