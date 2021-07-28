#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using GolemUI.Interfaces;
using Sentry;

namespace GolemUI
{
    public class SentryAdditionalDataIngester
    {
        private readonly IProcessControler _processControler;
        private readonly Src.BenchmarkService _benchmarkService;
        public string YagnaId = "";
        private string _LastBenchmarkError = "";
        Stopwatch _stopwatch = new Stopwatch();
        public SentryAdditionalDataIngester(Interfaces.IProcessControler processControler, Src.BenchmarkService benchmarkService)
        {
            _processControler = processControler;
            _benchmarkService = benchmarkService;
            _processControler.PropertyChanged += _processControler_PropertyChanged;
            _benchmarkService.PropertyChanged += BenchmarkService_PropertyChanged;
        }
        private async void _processControler_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsServerRunning")
            {
                YagnaId = (await _processControler.Me()).Id;
                SentrySdk.ConfigureScope(async scope =>
                {
                    scope.Contexts["user_data"] = new
                    {
                        UserName = Environment.UserName,
                        YagnaId = (await _processControler.Me()).Id
                    };
                });

            }
        }
        private void BenchmarkService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsRunning")
            {
                if (_benchmarkService.IsRunning)
                {
                    _stopwatch.Reset();
                    _stopwatch.Start();
                    _LastBenchmarkError = "";
                    SentrySdk.AddBreadcrumb(message: "Benchmark started ", category: "event", level: BreadcrumbLevel.Info);
                }
                else
                {
                    var benchmarkData = new Dictionary<string, string>() { { "Total hashrate", _benchmarkService.TotalMhs.ToString() } };
                    var gpus = _benchmarkService.Status?.GPUs.Values?.ToArray();
                    foreach (var gpu in gpus)
                    {
                        benchmarkData.Add("GPU #" + gpu.GpuNo + " PCIE", gpu.PciExpressLane.ToString());
                        benchmarkData.Add("GPU #" + gpu.GpuNo + " name", gpu.GpuName);
                        benchmarkData.Add("GPU #" + gpu.GpuNo + " details", gpu.GPUDetails);
                        benchmarkData.Add("GPU #" + gpu.GpuNo + " speed", gpu.BenchmarkSpeed.ToString());
                        benchmarkData.Add("GPU #" + gpu.GpuNo + " gpu throttling", gpu.ClaymorePerformanceThrottling.ToString());

                    }
                    benchmarkData.Add("ELAPSED TIME [MS]", _stopwatch.ElapsedMilliseconds.ToString());
                    _stopwatch.Stop();

                    SentrySdk.AddBreadcrumb(message: "Benchmark stopped ", data: benchmarkData, category: "event", level: BreadcrumbLevel.Info);
                    Sentry.Setup.Log("> SetupWindow > Benchmark Finished");
                }
            }
            if (e.PropertyName == "Status")
            {
                if (_benchmarkService.Status != null)
                {
                    var benchmarkError = _benchmarkService.Status.ErrorMsg;
                    if (_LastBenchmarkError != benchmarkError && benchmarkError != null)
                        SentrySdk.AddBreadcrumb(message: "Benchmark error ", data: new Dictionary<string, string>() { { "Error", benchmarkError } }, category: "event", level: BreadcrumbLevel.Info);
                }
            }
        }
    }
}
