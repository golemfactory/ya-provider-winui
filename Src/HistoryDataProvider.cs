using GolemUI.Interfaces;
using GolemUI.Model;
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
        public List<double> HashrateHistory { get; set; } = new List<double>();
        public Dictionary<DateTime, double> EarningsHistoryGpu { get; set; } = new Dictionary<DateTime, double>();
        public PrettyChartData HashrateChartData { get; set; } = new PrettyChartData();


        IStatusProvider _statusProvider;

        public event PropertyChangedEventHandler? PropertyChanged;

        private double? _estimatedEarningsPerSecond;
        public double? EstimatedEarningsPerSecond
        {
            get
            {
                return _estimatedEarningsPerSecond;
            }
            set
            {
                _estimatedEarningsPerSecond = value;
                NotifyChanged();
            }
        }

        public void ComputeEstimatedEarnings()
        {
            /*
             * TODO: this should be more
             */
            if (EarningsHistoryGpu.Count > 1)
            {
                DateTime timeStart = EarningsHistoryGpu.First().Key;
                DateTime timeEnd = EarningsHistoryGpu.Last().Key;
                double moneyStart = EarningsHistoryGpu.First().Value;
                double moneyEnd = EarningsHistoryGpu.Last().Value;

                if (moneyEnd - moneyStart > 0 && (timeEnd - timeStart).TotalSeconds > 0)
                {
                    var glmValue = (moneyEnd - moneyStart) / (timeEnd - timeStart).TotalSeconds;
                    EstimatedEarningsPerSecond = glmValue;
                }
            }
        }

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

        double _sumMoney = 0.0;
        double SumMoney
        {
            get
            {
                return _sumMoney;
            }
            set
            {
                _sumMoney = value;
                NotifyChanged();
            }
        }
        IProcessControler _processControler;
        ILogger<HistoryDataProvider> _logger;
        IPriceProvider _priveProvider;

        public HistoryDataProvider(IStatusProvider statusProvider, IProcessControler processControler, ILogger<HistoryDataProvider> logger, IPriceProvider priveProvider)
        {
            _priveProvider = priveProvider;
            _statusProvider = statusProvider;
            statusProvider.PropertyChanged += StatusProvider_PropertyChanged;
            _processControler = processControler;
            _logger = logger;

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
            _getUsageVectorsTask = null;
        }

        private void CheckForMiningActivityStateChange(Model.ActivityState gminerState)
        {
            if (gminerState.State == ActivityState.StateType.New)
            {
                EarningsHistoryGpu.Clear();
                HashrateChartData.Clear(); //clear chart data
                EstimatedEarningsPerSecond = null;

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

        private void CheckForActivityEarningChange(Model.ActivityState gminerState)
        {
            if (UsageVectorsAsDict != null && gminerState?.Usage != null)
            {
                double sumMoney = 0.0;
                foreach (var usage in gminerState.Usage)
                {
                    if (UsageVectorsAsDict.ContainsKey(usage.Key))
                    {
                        sumMoney += UsageVectorsAsDict[usage.Key] * usage.Value;
                    }
                }
                SumMoney = sumMoney;

                EarningsHistoryGpu.Add(DateTime.Now, SumMoney);
                ComputeEstimatedEarnings();
            }
        }

        private void StatusProvider_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Activities")
            {
                var act = _statusProvider.Activities;

                Model.ActivityState? gminerState = act.Where(a => a.ExeUnit == "gminer" && a.State == Model.ActivityState.StateType.Ready).SingleOrDefault();
                var isCpuMining = act.Any(a => a.ExeUnit == "wasmtime" || a.ExeUnit == "vm" && a.State == Model.ActivityState.StateType.Ready);

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

        private void NotifyChanged([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
