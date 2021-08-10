using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GolemUI.Model;
using GolemUI.Interfaces;
using GolemUI.Utils;
using System.IO;
using Newtonsoft.Json;

namespace GolemUI.Src
{
    class UserSettingsProvider : IUserSettingsProvider
    {
        public UserSettings LoadUserSettings()
        {
            UserSettings? settings = null;
            try
            {
                string settingsFilePath = PathUtil.GetLocalSettingsPath();
                string jsonText = File.ReadAllText(settingsFilePath);
                settings = JsonConvert.DeserializeObject<UserSettings>(jsonText);
            }
            catch (Exception)
            {
                settings = null;
            }

            if (settings == null || settings.SettingsVersion != GolemUI.Properties.Settings.Default.UserSettingsVersion)
            {
                settings = null;
            }

            if (settings == null)
            {
                settings = new UserSettings();
            }

            return settings;
        }

        public void SaveUserSettings(UserSettings userSettings)
        {
            userSettings.SettingsVersion = GolemUI.Properties.Settings.Default.UserSettingsVersion;

            string settingsFilePath = PathUtil.GetLocalSettingsPath();

            string s = JsonConvert.SerializeObject(userSettings, Formatting.Indented);

            File.WriteAllText(settingsFilePath, s);
        }
    }
}
