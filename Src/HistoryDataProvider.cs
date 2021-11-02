using GolemUI.Interfaces;
using GolemUI.Model;
using GolemUI.UI.Charts;
using GolemUI.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace GolemUI.Src
{
    public class HistoryDataProvider : IHistoryDataProvider, IDisposable
    {
        DispatcherTimer _dispatcherTimer = new DispatcherTimer();

        public struct GPUHistoryUsage
        {
            public DateTime? Dt;
            public int? Shares;
            public int? StaleShares;
            public int? InvalidShares;
            public double? Earnings;
            public double? Duration;
            public double? SharesTimesDifficulty;
            public double? HashRate;
            public string? ExeUnit;

            public GPUHistoryUsage(DateTime dt, int shares, int staleShares, int invalidShares, double earnings, double duration, double sharesTimesDifficulty, double hashRate, string exeUnit)
            {
                Dt = dt;
                Shares = shares;
                StaleShares = staleShares;
                InvalidShares = invalidShares;
                Earnings = earnings;
                Duration = duration;
                SharesTimesDifficulty = sharesTimesDifficulty;
                HashRate = hashRate;
                ExeUnit = exeUnit;
            }
        };
        public Dictionary<string, ActivityState> Activities { get; set; } = new Dictionary<string, ActivityState>();


        //public List<double> HashrateHistory { get; set; } = new List<double>();


        public List<GPUHistoryUsage> MiningHistoryGpuTotal { get; set; } = new List<GPUHistoryUsage>();

        public List<GPUHistoryUsage> MiningHistoryGpuSinceStart { get; set; } = new List<GPUHistoryUsage>();


        public PrettyChartRawData EarningsChartData { get; set; } = new PrettyChartRawData();

        public PrettyChartRawData HashrateChartData { get; set; } = new PrettyChartRawData();


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

        public void SetCurrentRequestorPayout(Coin coin, double glmPerHourPerGh)
        {
            _currentRequestorPayout[coin] = glmPerHourPerGh;
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

            _dispatcherTimer.Tick += _dispatcherTimer_Tick;
            _dispatcherTimer.Interval = TimeSpan.FromMinutes(20);
            _dispatcherTimer.Start();

            LoadAllHistory();
        }

        public void MergeIntoTotalHistory(List<GPUHistoryUsage> historyPart)
        {
            foreach (var entry in historyPart)
            {
                GPUHistoryUsage val = entry;

                MiningHistoryGpuTotal.Add(val);

            }
        }

        public DateTime? ExtractDateFromHistoryFileName(string fileName)
        {
            var splitted = fileName.Split('_');
            if (splitted.Length > 1)
            {
                string datePart = splitted[1].Split('.')[0];
                if (DateTime.TryParseExact(datePart, "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime res))
                {
                    return res;
                }
            }

            return null;
        }

        public void LoadAllHistory()
        {
            DateTime currentDate = DateTime.Now;

            string datePart = DateTime.Now.ToString("yyyy-MM-dd");
            var historyPath = PathUtil.GetRemoteHistoryPath();


            if (Directory.Exists(historyPath))
            {
                string[] files = Directory.GetFiles(historyPath, "history_*.json", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    DateTime? dateOfFile = ExtractDateFromHistoryFileName(file);
                    int daysDiff = (currentDate - dateOfFile.GetValueOrDefault()).Days;
                    //do not read history older than a week 
                    if (dateOfFile != null && daysDiff < 7 && daysDiff >= 0)
                    {
                        string historyFilePath = Path.Combine(historyPath, file);
                        //string historyFilePath = Path.Combine(historyPath, $"history_{datePart}.json");

                        if (File.Exists(historyFilePath))
                        {
                            var historyData = File.ReadAllText(historyFilePath);

                            List<GPUHistoryUsage>? oldHistory = JsonConvert.DeserializeObject<List<GPUHistoryUsage>>(historyData);
                            if (oldHistory == null)
                            {
                                _logger.LogWarning("Failed to download old history");
                                return;
                            }

                            MergeIntoTotalHistory(oldHistory);
                        }
                    }
                }
            }



            /*
            
            if (File.Exists(historyFilePath))
            {
                var historyData = File.ReadAllText(historyFilePath);

                List<GPUHistoryUsage>? oldHistory = JsonConvert.DeserializeObject<List<GPUHistoryUsage>>(historyData);
                if (oldHistory == null)
                {
                    _logger.LogWarning("Failed to download old history");
                    return;
                }

                MergeIntoTotalHistory(oldHistory);
            }*/



            UpdateEarningsChartData();

        }

        private void TrimMiningHistoryGpuTotalHistory(DateTime removeBeforeDate)
        {
            if (MiningHistoryGpuTotal.Count > 0)
            {
                try
                {
                    int idx = MiningHistoryGpuTotal.FindIndex(x => x.Dt >= removeBeforeDate);
                    if (idx > 0)
                    {
                        MiningHistoryGpuTotal = MiningHistoryGpuTotal.GetRange(idx, MiningHistoryGpuTotal.Count - idx);
                    }
                    else if (idx == 0)
                    {
                        //nothing changes
                    }
                    else
                    {
                        MiningHistoryGpuTotal.Clear();
                    }
                }
                catch (ArgumentNullException e)
                {
                    _logger.LogError("ArgumentNullException when trimming history: " + e.Message);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    _logger.LogError("ArgumentOutOfRangeException when trimming history: " + e.Message);
                }
                catch (ArgumentException e)
                {
                    _logger.LogError("ArgumentException when trimming history: " + e.Message);
                }
            }
        }

        private void TrimHistory(DateTime trimDataUpTo)
        {
            //TrimMiningHistoryGpuTotalHistory(trimDataUpTo);
            //EarningsChartData.RawElements.Clear();
            //UpdateEarningsChartData();
            //EarningsChartData.TrimData(trimDataUpTo, _logger, true);
            //HashrateChartData.TrimData(trimDataUpTo, _logger, true);
        }


        private void DumpHistory()
        {
            DateTime currentDay = DateTimeUtils.RoundDown(DateTime.Now, TimeSpan.FromDays(1));

            DateTime trimDataUpTo = currentDay.AddDays(-7.0);
            TrimHistory(trimDataUpTo);


            int idx = 0;
            while (idx < MiningHistoryGpuTotal.Count)
            {
                DateTime dt = MiningHistoryGpuTotal[idx].Dt.GetValueOrDefault();
                DateTime day_low = DateTimeUtils.RoundDown(dt, TimeSpan.FromDays(1));
                DateTime day_max = DateTimeUtils.RoundUp(dt, TimeSpan.FromDays(1));

                if (day_low > currentDay)
                {
                    _logger.LogError("Entry date cannot be greater than current date");
                    return;
                }
                if ((currentDay - day_low).Days > 1)
                {
                    idx++;
                    continue;
                }

                List<GPUHistoryUsage> dailyList = new List<GPUHistoryUsage>();
                while (idx < MiningHistoryGpuTotal.Count)
                {
                    dt = MiningHistoryGpuTotal[idx].Dt.GetValueOrDefault();
                    if (dt >= day_max)
                    {
                        break;
                    }
                    else
                    {
                        dailyList.Add(MiningHistoryGpuTotal[idx]);
                    }
                    idx++;
                }
                var history = JsonConvert.SerializeObject(dailyList, Formatting.Indented);
                var historyPath = PathUtil.GetRemoteHistoryPath();
                if (!Directory.Exists(historyPath))
                {
                    Directory.CreateDirectory(historyPath);
                }
                string datePart = day_low.ToString("yyyy-MM-dd");
                string historyFilePath = Path.Combine(historyPath, $"history_{datePart}.json");

                File.WriteAllText(historyFilePath, history);
            }
        }

        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            DumpHistory();
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
            //DateTime trimDataUpTo = DateTime.Now.AddSeconds(-50.0);
            //TrimHistory(trimDataUpTo);

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
                int invalidShares = 0;
                int staleShares = 0;
                double hashRate = 0.0;
                double sharesTimesDiff = 0.0;
                double duration = 0.0;
                string exeUnit = newActivity.ExeUnit ?? "";
                foreach (var usage in usageVectorDiff)
                {
                    switch (usage.Key)
                    {
                        case "golem.usage.mining.share":
                            shares = MathUtils.RoundToInt(usage.Value);
                            break;
                        case "golem.usage.mining.stale-share":
                            staleShares = MathUtils.RoundToInt(usage.Value);
                            break;
                        case "golem.usage.mining.invalid-share":
                            invalidShares = MathUtils.RoundToInt(usage.Value);
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

                bool updateChartsNeeded = false;
                foreach (List<GPUHistoryUsage> MiningHistoryGpu in new List<GPUHistoryUsage>[] { MiningHistoryGpuSinceStart, MiningHistoryGpuTotal })
                {
                    if (MiningHistoryGpu.Count == 0)
                    {
                        MiningHistoryGpu.Add(new GPUHistoryUsage(key, shares, staleShares, invalidShares, sumMoney, duration, sharesTimesDiff, hashRate, exeUnit));
                    }
                    else
                    {
                        if (shares > 0)
                        {
                            var lastEntry = MiningHistoryGpu.Last();
                            MiningHistoryGpu.Add(new GPUHistoryUsage(key, shares, staleShares, invalidShares, sumMoney, duration, sharesTimesDiff, hashRate, exeUnit));
                            updateChartsNeeded = true;
                        }
                    }
                }
                if (updateChartsNeeded)
                {
                    var entry = MiningHistoryGpuTotal.Last();
                    EarningsChartData.AddNewEntry(entry.Dt.GetValueOrDefault(), entry.Earnings ?? 0, true);
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

            if (usageVector != null)
            {
                const string key = "golem.usage.mining.hash";
                if (usageVector.ContainsKey(key))
                {
                    double glmPerGhPerSecond = usageVector[key];
                    if (glmPerGhPerSecond >= 0.0)
                    {
                        if (gminerState.ExeUnit == "gminer")
                        {
                            SetCurrentRequestorPayout(Coin.ETH, glmPerGhPerSecond);
                        }
                        else if (gminerState.ExeUnit == "hminer")
                        {
                            SetCurrentRequestorPayout(Coin.ETC, glmPerGhPerSecond);
                        }
                        else
                        {
                            _logger.LogError("Unknown exe unit: " + gminerState.ExeUnit ?? "null");
                        }
                    }
                }
            }
            if (usageVector != null && gminerState?.Usage != null)
            {
                AddGMinerActivityEntry(gminerState, usageVector, _logger);
            }
        }

        private void UpdateChartData()
        {
            foreach (var entry in MiningHistoryGpuSinceStart)
            {
                HashrateChartData.AddNewEntry(entry.Dt.GetValueOrDefault(), entry.HashRate ?? 0);
            }

            NotifyChanged("HashrateChartData");
        }



        private void UpdateEarningsChartData()
        {
            if (MiningHistoryGpuTotal.Count > 0)
            {
                foreach (var entry in MiningHistoryGpuTotal)
                {
                    EarningsChartData.AddNewEntry(entry.Dt.GetValueOrDefault(), entry.Earnings ?? 0);
                }
            }

            NotifyChanged("EarningsChartData");
        }


        private void CheckForActivityHashrateChange(Model.ActivityState gminerState)
        {
            float hashRate = 0.0f;

            gminerState.Usage?.TryGetValue("golem.usage.mining.hash-rate", out hashRate);
            /*
            TODO - implement proper hashrate history for statistics 
            if (HashrateHistory.Count == 0 || HashrateHistory.Last() != hashRate)
            {
                HashrateHistory.Add(hashRate);
                UpdateChartData();
            }*/
        }

        private void _computeEstimatedEarnings()
        {
            const int MINIMUM_SHARES_FOR_ESTIMATION = 6;
            const int MINIMUM_SHARES_FOR_REMOVE_HISTORY = 11;
            const double MINIMUM_MINUTES_FOR_REMOVE_HISTORY = 125;


            if (MiningHistoryGpuSinceStart.Count > 1)
            {
                DateTime timeEnd = MiningHistoryGpuSinceStart.Last().Dt.GetValueOrDefault();
                double earnings = 0;
                int shares = 0;
                TimeSpan diffTime = new TimeSpan(0);
                for (int idx = MiningHistoryGpuSinceStart.Count - 1; idx >= 0; idx--)
                {
                    DateTime timeStart = MiningHistoryGpuSinceStart[idx].Dt.GetValueOrDefault();
                    shares += MiningHistoryGpuSinceStart[idx].Shares ?? 0;
                    earnings += MiningHistoryGpuSinceStart[idx].Earnings ?? 0;
                    diffTime = timeEnd - timeStart;
                    if (diffTime.TotalMinutes > MINIMUM_MINUTES_FOR_REMOVE_HISTORY && shares > MINIMUM_SHARES_FOR_REMOVE_HISTORY)
                    {
                        break;
                    }
                }

                if (shares >= MINIMUM_SHARES_FOR_ESTIMATION && earnings > 0 && diffTime.TotalSeconds > 0)
                {
                    var glmValue = earnings / diffTime.TotalSeconds;
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
                    if (actState.ExeUnit == "gminer" || actState.ExeUnit == "hminer")
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

        public void Dispose()
        {
            DumpHistory();
        }
    }
}
