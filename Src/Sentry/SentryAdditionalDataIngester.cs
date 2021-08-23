#nullable enable
using GolemUI.Interfaces;
using GolemUI.Src;
using Sentry;
using System;
using System.ComponentModel;
using System.Linq;

namespace GolemUI
{
    public class SentryAdditionalDataIngester
    {
        private readonly IProcessControler _processControler;
        private readonly Interfaces.IProviderConfig _providerConfig;
        private readonly IBenchmarkResultsProvider _benchmarkResultsProvider;
        private readonly BenchmarkService _benchmarkService;
        private SentryContext Context = new SentryContext();



        public SentryAdditionalDataIngester(BenchmarkService benchmarkService, Interfaces.IProcessControler processControler, IBenchmarkResultsProvider benchmarkResultsProvider, Interfaces.IProviderConfig providerConfig)
        {
            _benchmarkService = benchmarkService;
            _benchmarkResultsProvider = benchmarkResultsProvider;
            _processControler = processControler;
            _providerConfig = providerConfig;
            _providerConfig.PropertyChanged += ProviderConfig_PropertyChanged;
            _processControler.PropertyChanged += _processControler_PropertyChanged;
            Context.MemberChanged += Context_MemberChanged;
            _benchmarkService.PropertyChanged += _benchmarkService_PropertyChanged;

        }

        private void _benchmarkService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsRunning")
            {
                if (!_benchmarkService.IsRunning && _benchmarkService != null)
                {
                    var benchmarkStatus = _benchmarkService.Status;
                    if (benchmarkStatus != null)
                    {
                        if (benchmarkStatus.GPUInfosParsed)
                        {
                            int gpusCount = benchmarkStatus?.GPUs?.Count ?? 0;
                            Context.AddItem("GpuCount", gpusCount.ToString());
                            if (gpusCount > 0)
                                Context.AddItem("MainGpu", benchmarkStatus?.GPUs?[0]?.GpuName ?? "");
                        }
                    }
                }
            }
        }

        public void InitContextItems()
        {
            var benchmarkSetting = _benchmarkResultsProvider.LoadBenchmarkResults();
            int gpusCount = benchmarkSetting?.liveStatus?.GPUs?.Count ?? 0;
            Context.AddItem("GpuCount", gpusCount.ToString());
            if (gpusCount > 0)
                Context.AddItem("MainGpu", benchmarkSetting?.liveStatus?.GPUs?[0]?.GpuName ?? "");


            UpdateNodeName(_providerConfig.Config?.NodeName ?? "");
            Context.AddItem("UserName", Environment.UserName);
            Context_MemberChanged();
        }
        private void SetScope(Scope scope, String key, String value)
        {
            scope.SetTag(key, value);
            scope.SetExtra(key, value);
        }
        private void Context_MemberChanged()
        {
            SentrySdk.ConfigureScope(scope => Context.Items.ToList().ForEach(x => SetScope(scope, x.Key, x.Value)));

        }

        void UpdateNodeName(string nodeName)
        {
            if (!String.IsNullOrEmpty(nodeName))
                Context.AddItem("NodeName", nodeName);
        }
        private void ProviderConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "NodeName")
                UpdateNodeName(_providerConfig.Config?.NodeName ?? "");
        }

        private async void _processControler_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsServerRunning")
                Context.AddItem("YagnaId", (await _processControler.Me()).Id);
        }

    }
}
