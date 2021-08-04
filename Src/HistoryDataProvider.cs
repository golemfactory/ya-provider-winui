using GolemUI.Interfaces;
using GolemUI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Src
{
    public class HistoryDataProvider : IHistoryDataProvider
    {
        List<double> HashrateHistory { get; set; } = new List<double>();
        PrettyChartData ChartData { get; set; } = new PrettyChartData();

        IStatusProvider _statusProvider;

        public event PropertyChangedEventHandler? PropertyChanged;

        public HistoryDataProvider(IStatusProvider statusProvider)
        {
            _statusProvider = statusProvider;
            statusProvider.PropertyChanged += StatusProvider_PropertyChanged;
        }

        private void StatusProvider_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Activities")
            {
                var act = _statusProvider.Activities;
                Model.ActivityState? gminerState = act.Where(a => a.ExeUnit == "gminer" && a.State == Model.ActivityState.StateType.Ready).SingleOrDefault();
                var isGpuMining = gminerState != null;
                var isCpuMining = act.Any(a => a.ExeUnit == "wasmtime" || a.ExeUnit == "vm" && a.State == Model.ActivityState.StateType.Ready);
                var gpuStatus = "Idle";
                if (isGpuMining)
                {
                    float hashRate = 0.0f;
                    gminerState?.Usage?.TryGetValue("golem.usage.mining.hash-rate", out hashRate);
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

        public PrettyChartData GetMegaHashHistory()
        {
            return ChartData;
        }
    }
}
