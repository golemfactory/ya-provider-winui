using GolemUI.Interfaces;
using GolemUI.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Src
{
    public class HistoryDataProvider : IHistoryDataProvider
    {
        List<double> HashrateHistory { get; set; } = new List<double>();
        Dictionary<DateTime, double> MoneyHistory { get; set; } = new Dictionary<DateTime, double>();
        PrettyChartData ChartData { get; set; } = new PrettyChartData();


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
                NotifyChanged("EstimatedEarningsPerSecond");
            }
        }

        public void ComputeEstimatedEarnings()
        {
            if (MoneyHistory.Count > 1)
            {
                DateTime timeStart = MoneyHistory.First().Key;
                DateTime timeEnd = MoneyHistory.Last().Key;
                double moneyStart = MoneyHistory.First().Value;
                double moneyEnd = MoneyHistory.Last().Value;

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
            get
            {
                return _activeAgreementID;
            }
            set
            {
                _activeAgreementID = value;
                NotifyChanged("ActiveAgreementID");
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
                NotifyChanged("SumMoney");
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

        private void StatusProvider_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Activities")
            {
                var act = _statusProvider.Activities;

                foreach (ActivityState actState in act ?? new List<ActivityState>())
                {
                    if (actState.ExeUnit == "gminer")
                    {
                        Model.ActivityState a = act.First();
                        if (a.State == ActivityState.StateType.New)
                        {
                            MoneyHistory.Clear();
                            ChartData = new PrettyChartData(); //clear chart data
                            EstimatedEarningsPerSecond = null;

                            ActiveAgreementID = actState.AgreementId;
                            Task.Run(() => GetUsageVectors());
                        }
                        if (a.State == ActivityState.StateType.Terminated)
                        {
                            ActiveAgreementID = null;
                        }
                    }
                }

                Model.ActivityState? gminerState = act.Where(a => a.ExeUnit == "gminer" && a.State == Model.ActivityState.StateType.Ready).SingleOrDefault();
                var isGpuMining = gminerState != null;
                var isCpuMining = act.Any(a => a.ExeUnit == "wasmtime" || a.ExeUnit == "vm" && a.State == Model.ActivityState.StateType.Ready);

                if (isGpuMining)
                {
                    float hashRate = 0.0f;

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

                        MoneyHistory.Add(DateTime.Now, SumMoney);
                        ComputeEstimatedEarnings();
                    }
                    gminerState?.Usage?.TryGetValue("golem.usage.mining.hash-rate", out hashRate);
                    if (HashrateHistory.Count == 0 || HashrateHistory.Last() != hashRate)
                    {
                        HashrateHistory.Add(hashRate);
                        if (PropertyChanged != null)
                        {
                            PrettyChartData chartData = new PrettyChartData() { BinData = new PrettyChartData.PrettyChartBinData() };

                            int idx_f = 0;
                            for (int i = Math.Max(HashrateHistory.Count - 14, 0); i < HashrateHistory.Count; i++)
                            {
                                chartData.BinData.BinEntries.Add(new PrettyChartData.PrettyChartBinEntry() { Label = i.ToString(), Value = HashrateHistory[i] });
                                idx_f += 1;
                            }

                            ChartData = chartData;

                            PropertyChanged(this, new PropertyChangedEventArgs("ChartData"));
                        }
                    }
                }
            }
        }

        private async void GetUsageVectors()
        {
            Dictionary<string, double> usageDict = new Dictionary<string, double>();
            YagnaAgreement? aggr = await _processControler.GetAgreement(ActiveAgreementID);

            if (aggr == null)
            {
                _logger.LogError("Failed to get GetAgreementInfo.");
                return;
            }
            try
            {
                object linearCoefficients = null;
                object usageVector = null;
                if (aggr.Offer.Properties?.TryGetValue("golem.com.pricing.model.linear.coeffs", out linearCoefficients) ?? false)
                {
                    if (aggr.Offer.Properties?.TryGetValue("golem.com.usage.vector", out usageVector) ?? false)
                    {
                        Newtonsoft.Json.Linq.JArray? lc = (Newtonsoft.Json.Linq.JArray)linearCoefficients;
                        Newtonsoft.Json.Linq.JArray? usV = (Newtonsoft.Json.Linq.JArray)usageVector;

                        if (lc != null && usV != null && lc.Count > 0)
                        {
                            usageDict["start"] = (double)lc[0];
                            for (int i = 0; i < usV.Count; i++)
                            {
                                usageDict[(string)usV[i]] = (double)lc[i + 1];
                            }
                        }
                    }
                }

                UsageVectorsAsDict = usageDict;
                _logger.LogInformation("Parsed usage vector: " + "{" + string.Join(",", usageDict.Select(kv => kv.Key + "=" + ((double)kv.Value).ToString(CultureInfo.InvariantCulture)).ToArray()) + "}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed GetAgreementInfo: " + ex.Message);
                UsageVectorsAsDict = null;
            }
        }

        public PrettyChartData GetMegaHashHistory()
        {
            return ChartData;
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
