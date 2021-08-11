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


        public List<double> HashrateHistory { get; set; } = new List<double>();
        public Dictionary<DateTime, GPUHistoryUsage> MiningHistoryGpu { get; set; } = new Dictionary<DateTime, GPUHistoryUsage>();
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

        IProcessControler _processControler;
        ILogger<HistoryDataProvider> _logger;
        IPriceProvider _priceProvider;

        public HistoryDataProvider(IStatusProvider statusProvider, IProcessControler processControler, ILogger<HistoryDataProvider> logger, IPriceProvider priceProvider)
        {
            _priceProvider = priceProvider;
            _statusProvider = statusProvider;
            statusProvider.PropertyChanged += StatusProvider_PropertyChanged;
            _processControler = processControler;
            _logger = logger;

            _agreementLookup = LookupCache<string, Dictionary<string, double>?>.FromFunc(agreementID => _processControler.GetUsageVectors(agreementID));
        }

        Task? _getUsageVectorsTask = null;
        private void GetUsageVectors(string? agreementID)
        {
            if (_getUsageVectorsTask != null)
            {
                _logger.LogError("_getUsageVectorsTask should be null before calling get usage vectors");
            }
            _getUsageVectorsTask = Task.Run(() => GetUsageVectorAsync(agreementID));
        }

        private async void GetUsageVectorAsync(string? agreementID)
        {
            UsageVectorsAsDict = await _processControler.GetUsageVectors(agreementID);
            if (UsageVectorsAsDict != null && UsageVectorsAsDict.TryGetValue("golem.usage.mining.hash", out double miningHashParameter))
            {
                _currentRequestorPayout[Coin.ETH] = miningHashParameter;
            }
            _getUsageVectorsTask = null;
        }

        private void CheckForMiningActivityStateChange(Model.ActivityState gminerState)
        {
            if (gminerState.State == ActivityState.StateType.New)
            {
                MiningHistoryGpu.Clear();
                HashrateChartData.Clear(); //clear chart data
                EarningsStats = null;

                ActiveAgreementID = gminerState.AgreementId;
                GetUsageVectors(ActiveAgreementID);
            }
            if (gminerState.State == ActivityState.StateType.Terminated)
            {
                ActiveAgreementID = null;
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

        private LookupCache<string, Dictionary<string, double>?> _agreementLookup;
        private async void CheckForActivityEarningChange(Model.ActivityState gminerState)
        {
            Dictionary<string, double>? usageVector = null;
            if (gminerState.AgreementId != null)
            {
                usageVector = await _agreementLookup.Get(gminerState.AgreementId);
            }
            if (usageVector != null && gminerState?.Usage != null)
            {
                double sumMoney = 0.0;
                double shares = 0.0;
                double hashRate = 0.0;
                double sharesTimesDiff = 0.0;
                double duration = 0.0;

                foreach (var usage in gminerState.Usage)
                {
                    switch (usage.Key)
                    {
                        case "golem.usage.mining.share":
                            shares = usage.Value;
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
                MiningHistoryGpu[key] = new GPUHistoryUsage((int)(shares + 0.5), sumMoney, duration, sharesTimesDiff, hashRate);

                _computeEstimatedEarnings();
            }
        }

        private void _computeEstimatedEarnings()
        {
            if (MiningHistoryGpu.Count > 3)
            {
                DateTime timeStart = MiningHistoryGpu.First().Key;
                DateTime timeEnd = MiningHistoryGpu.Last().Key;
                GPUHistoryUsage earningsStart = MiningHistoryGpu.First().Value;
                GPUHistoryUsage earningsEnd = MiningHistoryGpu.Last().Value;

                double diffEarnings = earningsEnd.Earnings - earningsStart.Earnings;
                TimeSpan diffTime = timeEnd - timeStart;
                int shares = earningsEnd.Shares - earningsStart.Shares;

                double currentHashrate = MiningHistoryGpu.Last().Value.HashRate;
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
                        CheckForMiningActivityStateChange(actState);
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
