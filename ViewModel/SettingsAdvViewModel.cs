using GolemUI.Interfaces;
using GolemUI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.ViewModel
{
    public class SettingsAdvViewModel
    {
        private readonly IUserSettingsProvider _userSettingsProvider;

        public UserSettings _userSettings;

        public SettingsAdvViewModel(IUserSettingsProvider userSettingsProvider)
        {
            _userSettingsProvider = userSettingsProvider;

            _userSettings = _userSettingsProvider.LoadUserSettings();
        }

        public void SaveData()
        {
            _userSettingsProvider.SaveUserSettings(_userSettings);
        }




    }
}
