using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
            get => _disableNotificationsWhenMinimized;
            set
            {
                _disableNotificationsWhenMinimized = value;
                NotifyChanged("DisableNotificationsWhenMinimized");
            }
        }

        private bool _notificationsEnabled = true;
        public bool NotificationsEnabled
        {
            get => _notificationsEnabled;
            set
            {
                _notificationsEnabled = value;
                NotifyChanged(nameof(NotificationsEnabled));
            }
        }
        private bool _shouldDisplayNotificationsIfMiningIsActive = true;
        public bool ShouldDisplayNotificationsIfMiningIsActive
        {
            get => _shouldDisplayNotificationsIfMiningIsActive;
            set
            {
                _shouldDisplayNotificationsIfMiningIsActive = value;
                NotifyChanged(nameof(ShouldDisplayNotificationsIfMiningIsActive));
            }
        }
        private bool _shouldAutoRestartMiningAfterBenchmark = true;
        public bool ShouldAutoRestartMiningAfterBenchmark
        {
            get => _shouldAutoRestartMiningAfterBenchmark;
            set
            {
                _shouldAutoRestartMiningAfterBenchmark = value;
                NotifyChanged(nameof(ShouldAutoRestartMiningAfterBenchmark));
            }
        }

        private bool _minimizeToTrayOnMinimize;
        public bool MinimizeToTrayOnMinimize
        {
            get => _minimizeToTrayOnMinimize;
            set
            {
                _minimizeToTrayOnMinimize = value;
                NotifyChanged("MinimizeToTrayOnMinimize");
            }
        }

        private bool _closeOnExit;
        public bool CloseOnExit
        {
            get => _closeOnExit;
            set
            {
                _closeOnExit = value;
                NotifyChanged("CloseOnExit");
            }
        }

        private bool _startWithWindows;
        public bool StartWithWindows
        {
            get => _startWithWindows;
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

        public int _opacity = 80;
        public int Opacity
        {
            get
            {
                return _opacity;
            }
            set
            {
                _opacity = value;
                NotifyChanged("Opacity");
            }
        }
    }
}
