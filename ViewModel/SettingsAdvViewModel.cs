using GolemUI.Interfaces;
using GolemUI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GolemUI.ViewModel
{
    public class SettingsAdvViewModel : INotifyPropertyChanged, ISavableLoadableDashboardPage
    {
        private readonly IUserSettingsProvider _userSettingsProvider;
        public event PageChangeRequestedEvent? PageChangeRequested;
        IStartWithWindows _startWithWindowsProvider;

        private UserSettings _userSettings;
        public UserSettings UserSettings
        {
            get
            {
                return _userSettings;
            }
            set
            {
                _userSettings = value;
                NotifyChanged("UserSettings");
            }
        }

        public SettingsAdvViewModel(IUserSettingsProvider userSettingsProvider, IStartWithWindows startWithWindowsProvider)
        {
            _startWithWindowsProvider = startWithWindowsProvider;
            _userSettingsProvider = userSettingsProvider;
            _userSettings = _userSettingsProvider.LoadUserSettings();
            _userSettings.PropertyChanged += OnUserSettingsPropertyChanged;

            PropertyChanged += OnPropertyChanged;

            PageChangeRequested = null;
        }

        public void GoBackToSettings()
        {
            PageChangeRequested?.Invoke(DashboardViewModel.DashboardPages.PageDashboardSettings);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void LoadData()
        {
            UserSettings = _userSettingsProvider.LoadUserSettings();
            UserSettings.PropertyChanged += OnUserSettingsPropertyChanged;
        }

        public void SaveData()
        {
            _startWithWindowsProvider.SetStartWithWindows(UserSettings.StartWithWindows);
            _userSettingsProvider.SaveUserSettings(UserSettings);
        }

        private void NotifyChanged([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnUserSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SaveData();
            if (e.PropertyName == "Opacity")
            {
                App app = (App)Application.Current;
                app.UpdateAppearance();
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PageChangeRequested != null)
            {

            }
        }


    }
}
