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
        public const string DefaultProxy = "staging-backend.chessongolem.app:3334";
        public const int CurrentSettingsVersion = 348;
        public const int CurrentBenchmarkResultVersion = 140;

        public const double GLMUSD = 0.27;

        public static bool enableLoggingToDebugWindow = true;
    }



    public class BenchmarkResults
    {
        public int BenchmarkResultVersion { get; set; }

        public ClaymoreLiveStatus? liveStatus = null;

        public bool IsClaymoreMiningPossible(out string reason)
        {
            reason = "";
            if (this.liveStatus == null)
            {
                reason = "No benchmark run";
                return false;
            }
            if (!this.liveStatus.BenchmarkFinished)
            {
                reason = "Benchmark not finished";
                return false;
            }
            if (this.liveStatus.GPUs.Count <= 0)
            {
                reason = "No GPUs detected";
                return false;
            }
            foreach (var gpu in this.liveStatus.GPUs)
            {
                if (!String.IsNullOrEmpty(gpu.Value.GPUError))
                {
                    reason = "Benchmark failed: " + gpu.Value.GPUError;
                    return false;
                }
            }

            return true;
        }
    }

    public class LocalSettings
    {
        public int SettingsVersion { get; set; }
        public string? MinerSelectedGPUIndices { get; set; }
        public string? MinerSelectedGPUsNiceness { get; set; }
        public string? BenchmarkLength { get; set; }
        public string? CustomPool { get; set; }
        public string? OptionalEmail { get; set; }

        public bool EnableDetailedBenchmarkInfo { get; set; }
        public bool EnableDebugLogs { get; set; }
        public bool StartYagnaCommandLine { get; set; }
        public bool StartProviderCommandLine { get; set; }
        public bool DisableNotificationsWhenMinimized { get; set; }
        public bool MinimizeToTrayOnMinimize { get; set; }
        public bool CloseOnExit { get; set; }

        public bool StartWithWindows { get; set; }
        public bool EnableWASMUnit { get; set; }

        public LocalSettings()
        {

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

        public static bool IsFirstRun()
        {
            string settingsFilePath = GetLocalSettingsPath();
            if (!File.Exists(settingsFilePath))
            {
                return true;
            }
            return false;
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

        public static void SaveBenchmarkToFile(BenchmarkResults? benchmarkSettings)
        {
            if (benchmarkSettings == null)
            {
                return;
            }
            benchmarkSettings.BenchmarkResultVersion = GlobalSettings.CurrentBenchmarkResultVersion;

            string fp = GetLocalBenchmarkPath();

            string s = JsonConvert.SerializeObject(benchmarkSettings, Formatting.Indented);

            File.WriteAllText(fp, s);
        }
    }

}
