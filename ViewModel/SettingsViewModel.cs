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
        private BenchmarkResults _benchmarkSettings;
        private IProviderConfig _providerConfig;
        public ObservableCollection<SingleGpuDescriptor> GpuList { get; set; }
       
        private IPriceProvider _priceProvider;
        public int _activeCpusCount { get; set; }
        public string _estimatedProfit { get; set; }
        private decimal _glmPerDay; 
        public string _hashrate { get; set; }
        public int _totalCpusCount { get; set; }
        
        public String ActiveCpusCountAsString { get { return this.ActiveCpusCount.ToString(); } }
        public String TotalCpusCountAsString { get { return this.TotalCpusCount.ToString(); } }

        public void LoadData()
        {
            GpuList.Clear();
            _benchmarkSettings = SettingsLoader.LoadBenchmarkFromFileOrDefault();
            _benchmarkSettings.liveStatus.GPUs.ToList().Where(gpu => gpu.Value.IsReadyForMining()).ToList().ForEach(gpu =>
             {
                 GpuList.Add(new SingleGpuDescriptor(gpu.Value.gpuNo, gpu.Value.gpuName, gpu.Value.IsEnabledByUser));
             });
        }
        public void SaveData()
        {
            GpuList.ToList().ForEach(gpu =>
            {
                var res = _benchmarkSettings.liveStatus.GPUs.ToList().Find(x => x.Value.gpuNo == gpu.Id);
                if (!res.Equals(default(KeyValuePair<int, Claymore.ClaymoreGpuStatus>)))
                {
                    res.Value.IsEnabledByUser = gpu.IsActive;
                }
            });
            
            SettingsLoader.SaveBenchmarkToFile(_benchmarkSettings);
        }
        private void Init(IPriceProvider priceProvider, Command.Provider provider, IProviderConfig providerConfig)
        {
            _priceProvider = priceProvider;
            _provider = provider;
            _providerConfig = providerConfig;
            GpuList = new ObservableCollection<SingleGpuDescriptor>();
            GpuList.Add(new SingleGpuDescriptor(1,"1st GPU", false));
            GpuList.Add(new SingleGpuDescriptor(2,"second GPU", true));
            GpuList.Add(new SingleGpuDescriptor(3,"3rd GPU", false));

            ActiveCpusCount = 3;
            TotalCpusCount = 7;
            Hashrate = "101.9 TH/s";
            EstimatedProfit = "$41,32 / day";
        }
      
        public SettingsViewModel()
        {
            Init(new Src.StaticPriceProvider(),null,null);

        }
        public SettingsViewModel(IPriceProvider priceProvider, Command.Provider provider, IProviderConfig providerConfig)
        {
            Init(priceProvider, provider,providerConfig);


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
        public string Hashrate
        {
            get { return _hashrate; }
            set
            {
                _hashrate = value;
                NotifyChange("Hashrate");
            }
        }

        public string EstimatedProfit
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
                return _priceProvider.glmToUsd(_glmPerDay);
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyChange([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
