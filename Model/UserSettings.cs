using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Model
{
    public class UserSettings : INotifyPropertyChanged
    {
        public int SettingsVersion { get; set; }

        public bool SetupFinished { get; set; }
        public string? BenchmarkLength { get; set; }
        public string? CustomPool { get; set; }
        public string? OptionalEmail { get; set; }

        public bool EnableDebugLogs { get; set; }
        public bool StartYagnaCommandLine { get; set; }
        public bool StartProviderCommandLine { get; set; }

        private bool _disableNotificationsWhenMinimized;
        public bool DisableNotificationsWhenMinimized
        {
            get
            {
                return _disableNotificationsWhenMinimized;
            }
            set
            {
                _disableNotificationsWhenMinimized = value;
                NotifyChanged("DisableNotificationsWhenMinimized");
            }
        }

        private bool _minimizeToTrayOnMinimize;
        public bool MinimizeToTrayOnMinimize
        {
            get
            {
                return _minimizeToTrayOnMinimize;
            }
            set
            {
                _minimizeToTrayOnMinimize = value;
                NotifyChanged("MinimizeToTrayOnMinimize");
            }
        }

        private bool _closeOnExit;
        public bool CloseOnExit
        {
            get
            {
                return _closeOnExit;
            }
            set
            {
                _closeOnExit = value;
                NotifyChanged("CloseOnExit");
            }
        }

        private bool _startWithWindows;
        public bool StartWithWindows
        {
            get
            {
                return _startWithWindows;
            }
            set
            {
                _startWithWindows = value;
                NotifyChanged("StartWithWindows");
            }
        }

        public bool EnableWASMUnit { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyChanged([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
