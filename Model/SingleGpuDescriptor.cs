using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GolemUI
{
    /// <summary>
    /// Interaction logic for DashboardSettings.xaml
    /// </summary>
    /// 

    
    public class SingleGpuDescriptor : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyChange([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public string? _Name { get; set; }
        public int _Id { get; set; }
        public int Id
        {
            get { return _Id; }
            set
            {
                _Id = value;
                NotifyChange("Id");
            }
        }
        public float _hashrate { get; set; }
        public float Hashrate
        {
            get { return _hashrate; }
            set
            {
                _hashrate = value;
                NotifyChange("Hashrate");
                NotifyChange("HashrateAsString");
            }
        }
        public string? HashrateAsString => _hashrate.ToString();

        public string? Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                NotifyChange("Name");
            }
        }
        public string DisplayName
        {
            get
            {
                return Id + ". " + Name;
            }
        }
        public bool _IsActive { get; set; }
        public bool IsActive
        {
            get { return _IsActive; }
            set
            {
                _IsActive = value;
                NotifyChange("IsActive");
            }
        }

        public bool _canMine { get; set; }
        public bool CanMine
        {
            get { return _canMine; }
            set
            {
                _canMine = value;
                NotifyChange("CanMine");
                NotifyChange("StatusIcon");
            }
        }

        public bool _inProgress { get; set; }
        public bool InProgress
        {
            get { return _inProgress; }
            set
            {
                _inProgress = value;
                NotifyChange("InProgress");
             
            }
        }

        public int _claymorePerformanceThrottling { get; set; }
        public int ClaymorePerformanceThrottling
        {
            get { return _claymorePerformanceThrottling; }
            set
            {
                _claymorePerformanceThrottling = value;
                NotifyChange("ClaymorePerformanceThrottling");
 
            }
        }
      
        public SingleGpuDescriptor(int id, string name, float hashrate, bool isActive, bool canMine, int claymorePerformanceThrottling, bool inProgress)
        {
            Id = id;
            Name = name;
            Hashrate = hashrate;
            IsActive = isActive;
            CanMine = canMine;
            ClaymorePerformanceThrottling = claymorePerformanceThrottling;
            InProgress = InProgress;
        }
        public SingleGpuDescriptor(Claymore.ClaymoreGpuStatus val)
        {
            Id = val.gpuNo;
            Name = val.gpuName == null ? "video card" : val.gpuName;
            Hashrate = val.BenchmarkSpeed;
            IsActive = val.IsEnabledByUser;
            CanMine = val.IsReadyForMining;
            ClaymorePerformanceThrottling = val.ClaymorePerformanceThrottling;
            InProgress = val.InProgress;
        }
        public SingleGpuDescriptor(Claymore.ClaymoreGpuStatus val, bool isEnabledByUser, int claymorePerformanceThrottling)
        {
            Id = val.gpuNo;
            Name = val.gpuName == null ? "video card" : val.gpuName;
            Hashrate = val.BenchmarkSpeed;
            CanMine = val.IsReadyForMining;
            InProgress = val.InProgress;
            IsActive = isEnabledByUser;
            ClaymorePerformanceThrottling = claymorePerformanceThrottling;
        }
    }
}
