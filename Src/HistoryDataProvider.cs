using GolemUI.Interfaces;
using GolemUI.Model;
using GolemUI.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GolemUI.Src
{
    public class HistoryDataProvider : IHistoryDataProvider
    {
        public struct GPUHistoryUsage
        {
            public int Shares;
            public double Earnings;
            public double Duration;
            public double SharesTimesDifficulty;
            public double HashRate;

            public GPUHistoryUsage(int shares, double earnings, double duration, double sharesTimesDifficulty, double hashRate)
            {
                Shares = shares;
                Earnings = earnings;
                Duration = duration;
                SharesTimesDifficulty = sharesTimesDifficulty;
                HashRate = hashRate;
            }
        };
        public Dictionary<string, ActivityState> Activities { get; set; } = new Dictionary<string, ActivityState>();


        public List<double> HashrateHistory { get; set; } = new List<double>();


        public SortedDictionary<DateTime, GPUHistoryUsage> MiningHistoryGpuTotal { get; set; } = new SortedDictionary<DateTime, GPUHistoryUsage>();

        public SortedDictionary<DateTime, GPUHistoryUsage> MiningHistoryGpuSinceStart { get; set; } = new SortedDictionary<DateTime, GPUHistoryUsage>();

        public PrettyChartData HashrateChartData { get; set; } = new PrettyChartData();


        IStatusProvider _statusProvider;

        public event PropertyChangedEventHandler? PropertyChanged;

        private Dictionary<Coin, double> _currentRequestorPayout = new Dictionary<Coin, double>();

        public string? _activeAgreementID = null;
        public string? ActiveAgreementID
        {
            get => _activeAgreementID;
            set
            {
                _activeAgreementID = value;
                NotifyChanged();
            }
        }

        Dictionary<string, double>? UsageVectorsAsDict { get; set; } = null;

        private IHistoryDataProvider.EarningsStatsType? _stats = null;
        public IHistoryDataProvider.EarningsStatsType? EarningsStats
        {
            get => _stats;
            set
            {
                _stats = value;
                NotifyChanged();
            }
        }

        private readonly IProcessController _processController;
        private readonly ILogger<HistoryDataProvider> _logger;
        private readonly IPriceProvider _priceProvider;

        private LookupCache<string, SortedDictionary<string, double>?> _agreementLookup;

        public HistoryDataProvider(IStatusProvider statusProvider, IProcessController processController, ILogger<HistoryDataProvider> logger, IPriceProvider priceProvider)
        {
            _priceProvider = priceProvider;
            _statusProvider = statusProvider;
            statusProvider.PropertyChanged += StatusProvider_PropertyChanged;
            _processController = processController;
            _logger = logger;

            _agreementLookup = LookupCache<string, SortedDictionary<string, double>?>.FromFunc(agreementID => _processController.GetUsageVectors(agreementID));
            _processController.PropertyChanged += OnProcessControllerChanged;
        }


        private void OnProcessControllerChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsProviderRunning")
            {
                if (!_processController.IsProviderRunning)
                {
                    MiningHistoryGpuSinceStart.Clear();
                    _computeEstimatedEarnings();
                }
            }
        }



        Random r = new Random();
        public void AddGMinerActivityEntry(ActivityState newActivity, SortedDictionary<string, double> usageVector, ILogger? logger = null)
        {
            ActivityState? previousActivity = null;
            if (newActivity.Id == null || newActivity.Usage == null)
            {
                return;
            }
            Activities.TryGetValue(newActivity.Id, out previousActivity);

            SortedDictionary<string, float> usageVectorDiff = new SortedDictionary<string, float>();
            if (previousActivity != null && previousActivity.Usage != null)
            {
                foreach (var entry in previousActivity.Usage)
                {
                    usageVectorDiff[entry.Key] = -entry.Value;
                }
            }

            foreach (var entry in newActivity.Usage)
            {
                if (usageVectorDiff.ContainsKey(entry.Key))
                {
                    usageVectorDiff[entry.Key] += entry.Value;
                }
                else
                {
                    usageVectorDiff[entry.Key] = entry.Value;
                }
            }



            foreach (var entry in usageVectorDiff)
            {
                if (entry.Key != "golem.usage.mining.hash-rate" && entry.Value < 0)
                {
                    if (previousActivity?.Usage != null)
                    {
                        _logger.LogInformation("Old usage vector: {" + string.Join(",", previousActivity.Usage.Select(kv => kv.Key + "=" + ((double)kv.Value).ToString(CultureInfo.InvariantCulture)).ToArray()) + "}");
                    }
                    else
                    {
                        _logger.LogInformation("Old usage vector is null");
                    }
                    _logger.LogInformation("New usage vector: {" + string.Join(",", newActivity.Usage.Select(kv => kv.Key + "=" + ((double)kv.Value).ToString(CultureInfo.InvariantCulture)).ToArray()) + "}");
                    if (previousActivity != null)
                    {
                        _logger.LogInformation(String.Format("Previous activity state: {0}", previousActivity.State.ToString()));
                    }
                    else
                    {
                        _logger.LogInformation(String.Format("Previous activity null"));
                    }
                    _logger.LogInformation(String.Format("New activity state: {0}", newActivity.State.ToString()));

                    logger.LogError("usageVectorDiff entry cannot be lower than 0: " + entry.Key);
                }
            }

            {
                double sumMoney = 0.0;
                int shares = 0;
                double hashRate = 0.0;
                double sharesTimesDiff = 0.0;
                double duration = 0.0;

                foreach (var usage in usageVectorDiff)
                {
                    switch (usage.Key)
                    {
                        case "golem.usage.mining.share":
                            shares = MathUtils.RoundToInt(usage.Value);
                            break;
                        case "golem.usage.duration_sec":
                            duration = usage.Value;
                            break;
                        case "golem.usage.mining.hash":
                            sharesTimesDiff = usage.Value;
                            break;
                        case "golem.usage.mining.hash-rate":
                            hashRate = usage.Value;
                            break;
                    }

                    if (usageVector.ContainsKey(usage.Key))
                    {
                        sumMoney += usageVector[usage.Key] * usage.Value;
                    }
                }
                DateTime key = DateTime.Now;


                foreach (SortedDictionary<DateTime, GPUHistoryUsage> MiningHistoryGpu in new SortedDictionary<DateTime, GPUHistoryUsage>[] { MiningHistoryGpuSinceStart, MiningHistoryGpuTotal })
                {
                    if (MiningHistoryGpu.Count == 0)
                    {
                        MiningHistoryGpu[key] = new GPUHistoryUsage(shares, sumMoney, duration, sharesTimesDiff, hashRate);
                    }
                    else
                    {
                        if (shares > 0)
                        {
                            var lastEntry = MiningHistoryGpu.Last().Value;
                            MiningHistoryGpu[key] = new GPUHistoryUsage(lastEntry.Shares + shares, lastEntry.Earnings + sumMoney, lastEntry.Duration + duration, sharesTimesDiff, hashRate);
                        }
                    }
                }
            }

            Activities[newActivity.Id] = newActivity;


            _computeEstimatedEarnings();
        }

        private async void CheckForActivityEarningChange(Model.ActivityState gminerState)
        {
            SortedDictionary<string, double>? usageVector = null;
            if (gminerState.AgreementId != null)
            {
                usageVector = await _agreementLookup.Get(gminerState.AgreementId);
            }
            if (usageVector != null && gminerState?.Usage != null)
            {
                AddGMinerActivityEntry(gminerState, usageVector, _logger);
            }
        }

        private void UpdateChartData()
        {
            PrettyChartData chartData = new PrettyChartData() { BinData = new PrettyChartData.PrettyChartBinData() };

            int idx_f = 0;
            for (int i = Math.Max(HashrateHistory.Count - 14, 0); i < HashrateHistory.Count; i++)
            {
                chartData.BinData.BinEntries.Add(new PrettyChartData.PrettyChartBinEntry() { Label = i.ToString(), Value = HashrateHistory[i] });
                idx_f += 1;
            }

            HashrateChartData = chartData;

            NotifyChanged("HashrateChartData");
        }

        private void CheckForActivityHashrateChange(Model.ActivityState gminerState)
        {
            float hashRate = 0.0f;

            gminerState.Usage?.TryGetValue("golem.usage.mining.hash-rate", out hashRate);
            if (HashrateHistory.Count == 0 || HashrateHistory.Last() != hashRate)
            {
                HashrateHistory.Add(hashRate);
                UpdateChartData();
            }
        }



        private void _computeEstimatedEarnings()
        {
            if (MiningHistoryGpuSinceStart.Count > 1)
            {
                DateTime timeStart = MiningHistoryGpuSinceStart.First().Key;
                DateTime timeEnd = MiningHistoryGpuSinceStart.Last().Key;
                GPUHistoryUsage earningsStart = MiningHistoryGpuSinceStart.First().Value;
                GPUHistoryUsage earningsEnd = MiningHistoryGpuSinceStart.Last().Value;

                double diffEarnings = earningsEnd.Earnings - earningsStart.Earnings;
                TimeSpan diffTime = timeEnd - timeStart;
                int shares = earningsEnd.Shares - earningsStart.Shares;

                double currentHashrate = MiningHistoryGpuSinceStart.Last().Value.HashRate;
                if (shares > 0 && diffEarnings > 0 && diffTime.TotalSeconds > 0)
                {
                    var glmValue = diffEarnings / diffTime.TotalSeconds;
                    EarningsStats = new IHistoryDataProvider.EarningsStatsType()
                    {
                        Time = diffTime,
                        Shares = shares,
                        AvgGlmPerSecond = glmValue
                    };
                }
            }
            else
            {
                EarningsStats = null;
            }
            if (!_processController.IsProviderRunning)
            {
                EarningsStats = null;
            }
        }

        private void StatusProvider_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Activities")
            {
                ICollection<ActivityState>? act = _statusProvider.Activities;

                if (act == null)
                {
                    throw new Exception("Activities cannot be null!");
                }

                foreach (ActivityState actState in _statusProvider.Activities ?? new List<ActivityState>())
                {
                    if (actState.ExeUnit == "gminer")
                    {
                        CheckForActivityEarningChange(actState);
                        CheckForActivityHashrateChange(actState);
                    }
                }
            }
        }

        public double? GetCurrentRequestorPayout(Coin coin = Coin.ETH)
        {
            if (_currentRequestorPayout.TryGetValue(coin, out double v))
            {
                return v;
            }
            return null;
        }

        private void NotifyChanged([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
