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
        private ClaymoreLiveStatus _claymoreLiveStatus = new ClaymoreLiveStatus(true, 5);

        public ClaymoreLiveStatus? Status => _claymoreLiveStatus;

        public bool IsRunning { get; private set; }

        public bool _requestStop = false;

        public float? TotalMhs => _claymoreLiveStatus == null ? null : (from gpus in _claymoreLiveStatus?.GPUs.Values select gpus.BenchmarkSpeed).Sum();

        public async void StartBenchmark(string cards, string niceness, string pool, string ethereumAddress, ClaymoreLiveStatus? externalLiveStatus)
        {
            if (IsRunning)
            {
                return;
            }
            _requestStop = false;

            ClaymoreLiveStatus? baseLiveStatus = null;


            DateTime benchmarkStartTime = DateTime.Now;
            var walletAddress = _providerConfig.Config?.Account ?? "0xD593411F3E6e79995E787b5f81D10e12fA6eCF04";
            var poolAddr = GlobalSettings.DefaultProxy;
            var totalClaymoreReportsNeeded = 5;

            IsRunning = true;
            OnPropertyChanged("IsRunning");

            bool preBenchmarkNeeded = !String.IsNullOrEmpty(cards);

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
                    if (preBenchmarkNeeded)
                    {
                        bool result = cc.RunPreBenchmark();

                        if (!result)
                        {
                            _claymoreLiveStatus.GPUs.Clear();
                            _claymoreLiveStatus.ErrorMsg = cc.BenchmarkError;
                            OnPropertyChanged("Status");
                            return;
                        }
                        int retry = 0;
                        while (!cc.PreBenchmarkFinished)
                        {
                            await Task.Delay(30);

                            if (retry >= 200)
                            {
                                cc.Stop();

                                _claymoreLiveStatus.GPUs.Clear();
                                _claymoreLiveStatus.ErrorMsg = "Failed to obtain card list";
                                OnPropertyChanged("Status");

                                return;
                            }

                            if (_requestStop)
                            {
                                cc.Stop();
                                _claymoreLiveStatus.ErrorMsg = "Stopped by user";
                                OnPropertyChanged("Status");
                                break;
                            }
                            _claymoreLiveStatus = cc.ClaymoreParserPreBenchmark.GetLiveStatusCopy();
                            _claymoreLiveStatus.MergeUserSettingsFromExternalLiveStatus(externalLiveStatus);
                            baseLiveStatus = _claymoreLiveStatus;
                            OnPropertyChanged("Status");
                            if (_claymoreLiveStatus.GPUInfosParsed)
                            {
                                cc.Stop();
                                break;
                            }
                            retry += 1;
                        }
                    }
                }
                await Task.Delay(30);


                if (preBenchmarkNeeded && _claymoreLiveStatus != null && _claymoreLiveStatus.GPUs.Count == 0)
                {
                    return;
                }
                if (!benchmarkRecordingActive)
                {
                    bool result = cc.RunBenchmark(cards, "", poolAddr, walletAddress);
                    if (!result)
                    {
                        _claymoreLiveStatus.GPUs.Clear();
                        _claymoreLiveStatus.ErrorMsg = cc.BenchmarkError;
                        OnPropertyChanged("Status");
                        return;
                    }
                }

                while (!cc.BenchmarkFinished && IsRunning)
                {
                    _claymoreLiveStatus = cc.ClaymoreParserBenchmark.GetLiveStatusCopy();
                    
                    bool allExpectedGPUsFound = false;
                    if (baseLiveStatus != null)
                    {
                        _claymoreLiveStatus.MergeFromBaseLiveStatus(baseLiveStatus, cards, out allExpectedGPUsFound);
                    }
                    _claymoreLiveStatus.MergeUserSettingsFromExternalLiveStatus(externalLiveStatus);
                    OnPropertyChanged("Status");
                    OnPropertyChanged("TotalMhs");
                    if (_claymoreLiveStatus.NumberOfClaymorePerfReports >= _claymoreLiveStatus.TotalClaymoreReportsBenchmark)
                    {
                        break;
                    }
                    if (_claymoreLiveStatus.GPUInfosParsed && _claymoreLiveStatus.GPUs.Count == 0)
                    {
                        break;
                    }
                    await Task.Delay(100);
                    if (_requestStop)
                    {
                        cc.Stop();
                        _claymoreLiveStatus.ErrorMsg = "Stopped by user";
                        OnPropertyChanged("Status");
                        break;
                    }


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
                        gpu.SetStepFinished();
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

        public void StopBenchmark()
        {
            _requestStop = true;
        }

        public void Save()
        {
            var results = new BenchmarkResults()
            {
                BenchmarkResultVersion = GlobalSettings.CurrentBenchmarkResultVersion,
                liveStatus = _claymoreLiveStatus
            };
            SettingsLoader.SaveBenchmarkToFile(results);
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
