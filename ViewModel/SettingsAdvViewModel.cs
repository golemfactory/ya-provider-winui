using GolemUI.Interfaces;
using GolemUI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.ViewModel
{
    public class SettingsAdvViewModel : INotifyPropertyChanged, ISavableLoadableDashboardPage
    {
        private readonly IUserSettingsProvider _userSettingsProvider;

        public UserSettings UserSettings {get;set;}

        public SettingsAdvViewModel(IUserSettingsProvider userSettingsProvider)
        {
            _userSettingsProvider = userSettingsProvider;
            UserSettings = _userSettingsProvider.LoadUserSettings();

            PropertyChanged += OnPropertyChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void LoadData()
        {
            UserSettings = _userSettingsProvider.LoadUserSettings();
        }

        public void SaveData()
        {
            _userSettingsProvider.SaveUserSettings(UserSettings);
        }

        private void NotifyChanged([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //

        }


    }
}
