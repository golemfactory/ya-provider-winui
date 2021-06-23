using GolemUI.Claymore;
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
        public const string GolemFactoryPath = "GolemFactory";
        public const string SettingsSubFolder = "LazyMiner";

        public const int CurrentSettingsVersion = 348;
        public const int CurrentBenchmarkResultVersion = 138;

    }

    public class BenchmarkResults
    { 
        public int BenchmarkResultVersion { get; set; }

        public ClaymoreLiveStatus? liveStatus = null;
    }

    public class LocalSettings
    {
        public int SettingsVersion { get; set; }
        public string? NodeName { get; set; }
        public string? EthAddress { get; set; }
        public string? Subnet { get; set; }

        public LocalSettings()
        {
#if DEBUG
            EthAddress = "D593411F3E6e79995E787b5f81D10e12fA6eCF04";
            Subnet = "LazySubnet";
#endif

            var _gen = new NameGen();
            NodeName = _gen.GenerateElvenName() + "-" + _gen.GenerateElvenName();
        }
    }


    public class SettingsLoader
    {

        //static LocalSettings _LocalSettings;

        public SettingsLoader()
        {
            //_localSettings = new LocalSettings();
        }

        public static string GetLocalGolemFactoryPath()
        {
            string settingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            string localFolder = Path.Combine(settingPath, GlobalSettings.GolemFactoryPath);

            if (!Directory.Exists(localFolder))
            {
                Directory.CreateDirectory(localFolder);
            }

            return localFolder;
        }

        public static string GetLocalPath()
        {
            string settingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            string localFolder = Path.Combine(settingPath, GlobalSettings.GolemFactoryPath, GlobalSettings.SettingsSubFolder);

            if (!Directory.Exists(localFolder))
            {
                Directory.CreateDirectory(localFolder);
            }

            return localFolder;
        }

        public static string GetLocalSettingsPath()
        {
            string result = Path.Combine(GetLocalPath(), "settings.json");
            return result;
        }
        public static string GetLocalBenchmarkPath()
        {
            string result = Path.Combine(GetLocalPath(), "benchmark.json");
            return result;
        }
        public static void ClearProviderPresetsFile()
        {
            string path = Path.Combine(GetLocalGolemFactoryPath(), @"ya-provider\data\presets.json");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
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


        public static BenchmarkResults LoadBenchmarkFromFileOrDefault()
        {
            BenchmarkResults? settings = null;
            try
            {
                string fp = GetLocalBenchmarkPath();
                string jsonText = File.ReadAllText(fp);
                settings = JsonConvert.DeserializeObject<BenchmarkResults>(jsonText);
            }
            catch (Exception)
            {
                settings = null;
            }

            if (settings == null || settings.BenchmarkResultVersion != GlobalSettings.CurrentBenchmarkResultVersion)
            {
                settings = null;
            }

            if (settings == null)
            {
                settings = new BenchmarkResults();
            }

            return settings;
        }

        public static void SaveBenchmarkToFile(BenchmarkResults localSettings)
        {
            localSettings.BenchmarkResultVersion = GlobalSettings.CurrentBenchmarkResultVersion;

            string fp = GetLocalBenchmarkPath();

            string s = JsonConvert.SerializeObject(localSettings, Formatting.Indented);

            File.WriteAllText(fp, s);
        }
    }

}
