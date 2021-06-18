using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GolemUI.Settings
{


    public class GlobalSettings
    {
        public const string AppName = "LazyMiner";
        public const string AppTitle = "Lazy Miner";
        public const string SettingsFolder = "LazyMiner";
        public const int CurrentSettingsVersion = 347;

    }

    public class LocalSettings
    {
        public int SettingsVersion { get; set; }
        public string? NodeName { get; set; }
        public string? EthAddress { get; set; }

        public LocalSettings()
        {
#if DEBUG
            EthAddress = "D593411F3E6e79995E787b5f81D10e12fA6eCF04";
#endif
        }
    }


    public class SettingsLoader
    {


        public SettingsLoader()
        {
            //_localSettings = new LocalSettings();
        }

        public static string GetLocalSettingsPath()
        {
            string settingPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            string localFolder = Path.Combine(settingPath, GlobalSettings.SettingsFolder);

            if (!Directory.Exists(localFolder))
            {
                Directory.CreateDirectory(localFolder);
            }

            string result = Path.Combine(localFolder, "settings.json");
            return result;
        }

        public static LocalSettings LoadSettingsFromFileOrDefault()
        {
            LocalSettings? settings = null;
            try
            {
                string settingsFilePath = GetLocalSettingsPath();
                string jsonText = File.ReadAllText(settingsFilePath);
                settings = JsonConvert.DeserializeObject<LocalSettings>(jsonText);
            }
            catch (Exception)
            {
                settings = null;
            }

            if (settings == null || settings.SettingsVersion != GlobalSettings.CurrentSettingsVersion)
            {
                settings = null;
            }

            if (settings == null)
            {
                settings = new LocalSettings();
            }

            return settings;
        }

        public static void SaveSettingsToFile(LocalSettings localSettings)
        {
            localSettings.SettingsVersion = GlobalSettings.CurrentSettingsVersion;

            string settingsFilePath = GetLocalSettingsPath();

            string s = JsonConvert.SerializeObject(localSettings, Formatting.Indented);

            File.WriteAllText(settingsFilePath, s);
        }

    }

}
