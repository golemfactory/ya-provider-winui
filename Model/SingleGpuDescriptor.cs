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
        public string _Name { get; set; }
        public bool _IsActive { get; set; }
        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                NotifyChange("Name");
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
        public SingleGpuDescriptor(string name, bool isActive)
        {
            Name = name;
            IsActive = isActive;
        }
    }
}
