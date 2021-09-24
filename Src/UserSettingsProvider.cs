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
using Microsoft.Win32;

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
            RegistryKey? rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\GolemFactory\\ThorgMiner", false);
            if (rk?.GetValue("SendReports") is string v)
            {
                settings.SendDebugInformation = "yes" == v;
            }
            else
            {
                settings.SendDebugInformation = false;
            }

            return settings;
        }

        public void SaveUserSettings(UserSettings userSettings)
        {
            userSettings.SettingsVersion = GolemUI.Properties.Settings.Default.UserSettingsVersion;

            string settingsFilePath = PathUtil.GetLocalSettingsPath();

            string s = JsonConvert.SerializeObject(userSettings, Formatting.Indented);

            File.WriteAllText(settingsFilePath, s);

            RegistryKey? rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\GolemFactory\\ThorgMiner", true);
            if (rk != null)
            {
                rk.SetValue("SendReports", userSettings.SendDebugInformation ? "yes" : "no");
            }
        }
    }
}
