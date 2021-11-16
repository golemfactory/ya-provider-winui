
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

            string localFolder = Path.Combine(settingPath, GolemUI.Properties.Settings.Default.GolemFactoryPath, GolemUI.Properties.Settings.Default.SettingsSubfolder);

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
            string result = Path.Combine(GetLocalPath(), "BenchmarkResults.json");
            return result;
        }
        public static string GetLocalBenchmarkPathTRex()
        {
            string result = Path.Combine(GetLocalPath(), "BenchmarkResultsT.json");
            return result;
        }
        public static string GetLocalBenchmarkPathPhoenix()
        {
            string result = Path.Combine(GetLocalPath(), "BenchmarkResultsP.json");
            return result;
        }

        public static string GetLocalLogPath()
        {
            string result = Path.Combine(GetLocalPath(), "ThorgMiner.log");
            return result;
        }

        public static List<string> GetLocalLogPaths()
        {
            return System.IO.Directory.GetFiles(GetLocalPath(), "ThorgMiner?.log").ToList();
        }


        public static string GetRemoteSettingsPath()
        {
            string result = Path.Combine(GetLocalPath(), "RemoteSettings.json");
            return result;
        }

        public static string GetRemoteTmpSettingsPath()
        {
            string result = Path.Combine(GetLocalPath(), "RemoteSettings-tmp.json");
            return result;
        }

        public static string GetRemoteHistoryPath()
        {
            string result = Path.Combine(GetLocalPath(), "History");
            return result;
        }



    }
}
