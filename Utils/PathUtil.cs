using GolemUI.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Utils
{
    class PathUtil
    {
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
            string result = Path.Combine(GetLocalPath(), "UserSettings.json");
            return result;
        }

        public static string GetLocalBenchmarkPath()
        {
            string result = Path.Combine(GetLocalPath(), "BenchmarkSnapshot.json");
            return result;
        }
    }
}
