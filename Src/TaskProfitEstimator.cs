using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GolemUI.Interfaces;
using GolemUI.Utils;
using Microsoft.Extensions.Logging;

namespace GolemUI.Src
{
    class TaskProfitEstimator : ITaskProfitEstimator
    {
        private double? _estimatedEarningsPerSecond;
        public double? EstimatedEarningsPerSecond
        {
            get => _estimatedEarningsPerSecond;
            set
            {
                if (_estimatedEarningsPerSecond != value)
                {
                    _estimatedEarningsPerSecond = value;
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
                EstimatedEarningsPerSecond = null;
                EstimatedEarningsMessage = $"Start mining to get estimates";
                return;
            }
            if (_historyDataProvider.EarningsStats is IHistoryDataProvider.EarningsStatsType stats)
            {
                EstimatedEarningsPerSecond = _priceProvider.CoinValue(stats.AvgGlmPerSecond, Model.Coin.GLM);
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
                var hashRate = _statusProvider.Activities
                    .Where(a => a.ExeUnit == "gminer" && (a.Usage?.ContainsKey(HASH_RATE) ?? false))
                    .FirstOrDefault()?.Usage?[HASH_RATE];

                Debug.WriteLine(_statusProvider.Activities);
                if (hashRate is float hr && hr > 0.0)
                {
                    EstimatedEarningsPerSecond = _estimatedProfitProvider.HashRateToUSDPerDay(Convert.ToDouble(hr)) / 3600.0 / 24.0;
                    EstimatedEarningsMessage = $"Estimation based on current hashrate and requestor payout.";
                }
                else
                {
                    EstimatedEarningsPerSecond = null;
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
