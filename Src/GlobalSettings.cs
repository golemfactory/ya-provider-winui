using GolemUI.Claymore;
using GolemUI.Utils;
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

#if DEBUG
        public static void ClearProviderPresetsFile()
        {
            string path = Path.Combine(GetLocalGolemFactoryPath(), @"ya-provider\data\presets.json");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
#endif



    }

}
