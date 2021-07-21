using GolemUI.Interfaces;
using GolemUI.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;


namespace GolemUI
{
    public class SettingsViewModel : INotifyPropertyChanged, ISavableLoadableDashboardPage
    {
        private Command.Provider _provider;
        private BenchmarkResults? _benchmarkSettings;
        private IProviderConfig? _providerConfig; 
        private Src.BenchmarkService _benchmarkService;
        public Src.BenchmarkService BenchmarkService => _benchmarkService;
        public ObservableCollection<SingleGpuDescriptor>? GpuList { get; set; }

        private IPriceProvider? _priceProvider;
        public int _activeCpusCount { get; set; }
        public string? _estimatedProfit { get; set; }
        private decimal _glmPerDay = 0.0m;
        public string? _hashrate { get; set; }

        public void StartBenchmark()
        {
            BenchmarkService.StartBenchmark();
        }

        public int _totalCpusCount { get; set; }
        public string? _nodeName { get; set; }
        public String ActiveCpusCountAsString { get { return this.ActiveCpusCount.ToString(); } }
        public String TotalCpusCountAsString { get { return this.TotalCpusCount.ToString(); } }


        public bool IsMiningActive
        {
            get => _providerConfig.IsMiningActive;
            set
            {
                _providerConfig.IsMiningActive = value;
            }
        }

        public bool IsCpuActive
        {
            get => _providerConfig.IsCpuActive;
            set
            {
                _providerConfig.IsCpuActive = value;
            }
        }

        public void LoadData()
        {
            GpuList?.Clear();
            _benchmarkSettings = SettingsLoader.LoadBenchmarkFromFileOrDefault();
            if (IsBenchmarkSettingsCorrupted()) return;
            _benchmarkSettings?.liveStatus?.GPUs.ToList().Where(gpu => gpu.Value != null && gpu.Value.IsReadyForMining).ToList().ForEach(gpu =>
               {
                   var val = gpu.Value;
                   GpuList?.Add(new SingleGpuDescriptor(val));
               });
            NodeName = _providerConfig?.Config?.NodeName;
            TotalCpusCount = GetCpuCount();
        }

        private bool IsBenchmarkSettingsCorrupted()
        {
            return (_benchmarkSettings == null || _benchmarkSettings.liveStatus == null || _benchmarkSettings.liveStatus.GPUs == null);
        }
        public void SaveData()
        {
            GpuList?.ToList().ForEach(gpu =>
            {
                if (IsBenchmarkSettingsCorrupted()) return;
                var res = _benchmarkSettings?.liveStatus?.GPUs.ToList().Find(x => x.Value.gpuNo == gpu.Id);
                if (res != null && res.HasValue && !res.Equals(default(KeyValuePair<int, Claymore.ClaymoreGpuStatus>)))
                {
                    KeyValuePair<int, Claymore.ClaymoreGpuStatus> keyVal = res.Value;
                    keyVal.Value.IsEnabledByUser = gpu.IsActive;
                }
            });

            SettingsLoader.SaveBenchmarkToFile(_benchmarkSettings);
        }
        private void Init(IPriceProvider? priceProvider, Src.BenchmarkService benchmarkService, Command.Provider? provider, IProviderConfig? providerConfig)
        {
            _priceProvider = priceProvider;
            _provider = provider;
            _providerConfig = providerConfig;
            _benchmarkService = benchmarkService;
            _providerConfig.PropertyChanged += OnProviderCofigChanged;
            _benchmarkService.PropertyChanged += OnBenchmarkChanged;



            GpuList = new ObservableCollection<SingleGpuDescriptor>();
            GpuList.Add(new SingleGpuDescriptor(1, "1st GPU", 20.12f, false, true,0));
            GpuList.Add(new SingleGpuDescriptor(2, "second GPU", 12.10f, true, false,5));
            GpuList.Add(new SingleGpuDescriptor(3, "3rd GPU", 9.00f, false, true,10));

            ActiveCpusCount = 3;
            TotalCpusCount = 7;
            Hashrate = "101.9 TH/s";
            EstimatedProfit = "$41,32 / day";
        }

        private void OnBenchmarkChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Status")
            {
                var _newGpus = _benchmarkService.Status?.GPUs.Values?.ToArray();
                if (_newGpus != null)
                {
                    for (var i = 0; i < _newGpus.Length; ++i)
                    {
                        if (i < GpuList.Count)
                        {
                            GpuList[i] =  new SingleGpuDescriptor(_newGpus[i],GpuList[i].IsActive,GpuList[i].ClaymorePerformanceThrottling);
                        }
                        else
                        {
                            GpuList.Add(new SingleGpuDescriptor(_newGpus[i]));
                        }
                    }
                    while (_newGpus.Length < GpuList.Count)
                    {
                        GpuList.RemoveAt(GpuList.Count - 1);
                    }
                }

                NotifyChange("GpuList"); // ok 
                /*NotifyChange("TotalHashRate");
                NotifyChange("ExpectedProfit");*/
            }
            if (e.PropertyName == "IsRunning")
            {
                /*OnPropertyChanged("BenchmarkIsRunning");
                OnPropertyChanged("ExpectedProfit");
                if (_flow == (int)FlowSteps.Noob)
                {
                    if (_noobStep == (int)NoobSteps.Benchmark && !BenchmarkIsRunning)
                    {
                        NoobStep = (int)NoobSteps.Enjoy;
                    }
                }
                else if (_flow == (int)FlowSteps.OwnWallet)
                {
                    if (_expertStep == ExpertSteps.Benchmark && !BenchmarkIsRunning)
                    {
                        ExpertStep = (int)ExpertSteps.Enjoy;
                    }
                }*/
            }
        }

        private void OnProviderCofigChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Config")
            {
                NotifyChange("NodeName");
            }
            if (e.PropertyName == "IsMiningActive" || e.PropertyName == "IsCpuActive")
            {
                NotifyChange(e.PropertyName);
            }
        }

       /* public SettingsViewModel()
        {
            Init(new Src.StaticPriceProvider(), null, null,null);

        }*/
        public SettingsViewModel(IPriceProvider priceProvider, Src.BenchmarkService benchmarkService, Command.Provider provider, IProviderConfig providerConfig)
        {
            Init(priceProvider,benchmarkService, provider, providerConfig);


        }
        public int ActiveCpusCount
        {
            get { return _activeCpusCount; }
            set
            {
                _activeCpusCount = value;
                NotifyChange("ActiveCpusCount");
                NotifyChange("ActiveCpusCountAsString");
            }
        }
        public int TotalCpusCount
        {
            get { return _totalCpusCount; }
            set
            {
                _totalCpusCount = value;
                NotifyChange("TotalCpusCount");
                NotifyChange("TotalCpusCountAsString");
            }
        }
        public string? Hashrate
        {
            get { return _hashrate; }
            set
            {
                _hashrate = value;
                NotifyChange("Hashrate");
            }
        }

        public string? NodeName
        {
            get => _providerConfig?.Config?.NodeName;
            set
            {
                _providerConfig?.UpdateNodeName(value);
            }
        }

        public string? EstimatedProfit
        {
            get { return _estimatedProfit; }
            set
            {
                _estimatedProfit = value;
                NotifyChange("EstimatedProfit");
            }
        }
        public decimal GlmPerDay
        {
            get
            {
                return _glmPerDay;
            }
        }

        public decimal UsdPerDay
        {
            get
            {
                if (_priceProvider == null)
                {
                    return new Decimal(0.0);
                }
                return _priceProvider.glmToUsd(_glmPerDay);
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyChange([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private int GetCpuCount()
        {
            int coreCount = 0;
            foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }

            return coreCount;
        }
    }
}
