using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GolemUI
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyChange([CallerMemberName] string? propertyName = null )
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<SingleGpuDescriptor> GpuList { get; set; }
        public int _ActiveCpusCount { get; set; }
        public int _TotalCpusCount { get; set; }

        public int TotalCpusCount
        {
            get { return _TotalCpusCount; }
            set
            {
                _TotalCpusCount = value;
                NotifyChange("TotalCpusCount");
                NotifyChange("TotalCpusCountAsString");
            }
        }
        public int ActiveCpusCount
        {
            get { return _ActiveCpusCount; }
            set
            {
                _ActiveCpusCount = value;
                NotifyChange("ActiveCpusCount");
                NotifyChange("ActiveCpusCountAsString");
            }
        }

        public String ActiveCpusCountAsString { get { return this.ActiveCpusCount.ToString(); } }
        public String TotalCpusCountAsString { get { return this.TotalCpusCount.ToString(); } }

        public SettingsViewModel()
        {
            GpuList = new ObservableCollection<SingleGpuDescriptor>();
            GpuList.Add(new SingleGpuDescriptor("1st GPU", false));
            GpuList.Add(new SingleGpuDescriptor("second GPU", true));
            GpuList.Add(new SingleGpuDescriptor("3rd GPU", false));

            ActiveCpusCount = 3;
            TotalCpusCount = 7;
        }
    }
}
