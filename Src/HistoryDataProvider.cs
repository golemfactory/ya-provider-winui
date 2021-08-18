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

        private readonly IProcessControler _processControler;
        private readonly ILogger<HistoryDataProvider> _logger;
        private readonly IPriceProvider _priceProvider;

        private LookupCache<string, Dictionary<string, double>?> _agreementLookup;

        public HistoryDataProvider(IStatusProvider statusProvider, IProcessControler processControler, ILogger<HistoryDataProvider> logger, IPriceProvider priceProvider)
        {
            _priceProvider = priceProvider;
            _statusProvider = statusProvider;
            statusProvider.PropertyChanged += StatusProvider_PropertyChanged;
            _processControler = processControler;
            _logger = logger;

            _agreementLookup = LookupCache<string, Dictionary<string, double>?>.FromFunc(agreementID => _processControler.GetUsageVectors(agreementID));
            _processControler.PropertyChanged += OnProcessControllerChanged;
        }


        private void OnProcessControllerChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsProviderRunning")
            {
                if (!_processControler.IsProviderRunning)
                {
                    MiningHistoryGpuSinceStart.Clear();
                    _computeEstimatedEarnings();
                }
            }
        }



        public void AddGMinerActivityEntry(ActivityState newActivity, Dictionary<string, double> usageVector, ILogger? logger = null)
        {
            ActivityState? previousActivity = null;
            if (newActivity.Id == null || newActivity.Usage == null)
            {
                return;
            }
            Activities.TryGetValue(newActivity.Id, out previousActivity);

            Dictionary<string, float> usageVectorDiff = new Dictionary<string, float>();
            if (previousActivity != null && previousActivity.Usage != null)
            {
                foreach (var entry in previousActivity.Usage)
                {
                    if (entry.Key == "golem.usage.mining.hash-rate")
                    {
                        //skip hash-rate - it is not summable
                        continue;
                    }
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
                if (entry.Value < 0)
                {
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
            Dictionary<string, double>? usageVector = null;
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
            int entry_no = 0;
            foreach (var entry in MiningHistoryGpuSinceStart)
            {
                HashrateChartData.AddOrUpdateBinEntry(entry_no, entry.Key.ToString("HH-mm-ss"), entry.Value.HashRate);
                entry_no += 1;
            }

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
            if (!_processControler.IsProviderRunning)
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

                //Model.ActivityState? gminerState = act.Where(a => a.ExeUnit == "gminer" && a.State == Model.ActivityState.StateType.Ready).SingleOrDefault();
                //var isCpuMining = act.Any(a => a.ExeUnit == "wasmtime" || a.ExeUnit == "vm" && a.State == Model.ActivityState.StateType.Ready);

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
