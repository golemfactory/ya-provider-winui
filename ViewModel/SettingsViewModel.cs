using GolemUI.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GolemUI
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
      
        public ObservableCollection<SingleGpuDescriptor> GpuList { get; set; }
       
        private IPriceProvider _priceProvider;
        public int _activeCpusCount { get; set; }
        public string _estimatedProfit { get; set; }
        private decimal _glmPerDay; 
        public string _hashrate { get; set; }
        public int _totalCpusCount { get; set; }
        
        public String ActiveCpusCountAsString { get { return this.ActiveCpusCount.ToString(); } }
        public String TotalCpusCountAsString { get { return this.TotalCpusCount.ToString(); } }


        public static SettingsViewModel Example
        {
            get
            {
                return new SettingsViewModel(new Src.StaticPriceProvider());
            }
        }

        public SettingsViewModel(IPriceProvider priceProvider)
        {
            _priceProvider = priceProvider;

            GpuList = new ObservableCollection<SingleGpuDescriptor>();
            GpuList.Add(new SingleGpuDescriptor("1st GPU", false));
            GpuList.Add(new SingleGpuDescriptor("second GPU", true));
            GpuList.Add(new SingleGpuDescriptor("3rd GPU", false));

            ActiveCpusCount = 3;
            TotalCpusCount = 7;
            Hashrate = "101.9 TH/s";
            EstimatedProfit = "$41,32 / day";
          
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
