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
        public float  _hashrate { get; set; }
        public float Hashrate
        {
            get { return _hashrate; }
            set
            {
                _hashrate = value;
                NotifyChange("Hashrate");
            }
        }
        public string? HashrateAsString => _hashrate.ToString();
        public bool _IsActive { get; set; }
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
        public bool IsActive
        {
            get { return _IsActive; }
            set
            {
                _IsActive = value;
                NotifyChange("Name");
            }
        }
        public SingleGpuDescriptor(int id, string name, float hashrate, bool isActive)
        {
            Id = id;
            Name = name;
            Hashrate = hashrate;
            IsActive = isActive;
        }
    }
}
