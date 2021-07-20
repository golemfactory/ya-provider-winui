using GolemUI.Claymore;
using GolemUI.Command;
using GolemUI.Interfaces;
using GolemUI.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GolemUI.Src
{
    public class BenchmarkService : INotifyPropertyChanged
    {
        private readonly Interfaces.IProviderConfig _providerConfig;
        private ClaymoreLiveStatus? _claymoreLiveStatus = null;

        public ClaymoreLiveStatus? Status => _claymoreLiveStatus;

        public bool IsRunning { get; private set; }

        public float? TotalMhs => _claymoreLiveStatus == null ? null : (from gpus in _claymoreLiveStatus?.GPUs.Values select gpus.BenchmarkSpeed).Sum();

        public async void StartBenchmark()
        {
            if (IsRunning)
            {
                return;
            }

            DateTime benchmarkStartTime = DateTime.Now;
            var walletAddress = _providerConfig.Config?.Account ?? "0xD593411F3E6e79995E787b5f81D10e12fA6eCF04";
            var poolAddr = GlobalSettings.DefaultProxy;
            var totalClaymoreReportsNeeded = 5;

            IsRunning = true;
            OnPropertyChanged("IsRunning");

            var cc = new ClaymoreBenchmark(totalClaymoreReportsNeeded);
            try
            {
                bool benchmarkRecordingActive = cc.RunBenchmarkRecording(@"test.recording");
                if (benchmarkRecordingActive)
                {
                    // TODO: Info
                }
                else
                {
                    cc.RunPreBenchmark();
                    var retry = 30;
                    while (!cc.PreBenchmarkFinished && --retry > 0)
                    {
                        await Task.Delay(30);
                        _claymoreLiveStatus = cc.ClaymoreParserPreBenchmark.GetLiveStatusCopy();
                        OnPropertyChanged("Status");
                        if (_claymoreLiveStatus.GPUInfosParsed)
                        {
                            cc.Stop();
                            break;
                        }
                    }
                }
                await Task.Delay(30);

                if (_claymoreLiveStatus != null && _claymoreLiveStatus.GPUs.Count == 0)
                {
                    return;
                }
                if (!benchmarkRecordingActive)
                {
                    bool result = cc.RunBenchmark("", "", poolAddr, walletAddress);
                    if (!result)
                    {
                        return;
                    }
                }

                while (!cc.BenchmarkFinished && IsRunning)
                {
                    _claymoreLiveStatus = cc.ClaymoreParserBenchmark.GetLiveStatusCopy();
                    OnPropertyChanged("Status");
                    OnPropertyChanged("TotalMhs");
                    if (_claymoreLiveStatus.NumberOfClaymorePerfReports >= _claymoreLiveStatus.TotalClaymoreReportsBenchmark)
                    {
                        break;
                    }
                    await Task.Delay(100);
                }
            }
            finally
            {
                IsRunning = false;
                cc.Stop();
                if (_claymoreLiveStatus != null)
                {
                    foreach (var gpu in _claymoreLiveStatus.GPUs.Values)
                    {
                        if (!gpu.IsReadyForMining && !gpu.IsOperationStopped)
                        {
                            gpu.GPUError = "Timeout";
                        }
                    }
                }
                OnPropertyChanged("IsRunning");
                OnPropertyChanged("Status");
            }
        }

        public BenchmarkService(IProviderConfig providerConfig)
        {
            _providerConfig = providerConfig;
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
