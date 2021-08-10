using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GolemUI.Model;

namespace GolemUI.Interfaces
{
    public interface IUserSettingsProvider
    {
        public UserSettings LoadUserSettings();

        public void SaveUserSettings(UserSettings userSettings);
    }
}
