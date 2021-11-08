using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GolemUI.Model;
using GolemUI.Interfaces;
using GolemUI.Utils;
using System.IO;
using GolemUI.Miners;
using Newtonsoft.Json;

namespace GolemUI.Src
{
    class BenchmarkResultsProvider : IBenchmarkResultsProvider
    {

        private string GetMinerSettingsPath(MinerAppName minerAppName)
        {
            switch (minerAppName.NameEnum)
            {
                case MinerAppName.MinerAppEnum.Phoenix:
                    return PathUtil.GetLocalBenchmarkPath();
                case MinerAppName.MinerAppEnum.TRex:
                    return PathUtil.GetLocalBenchmarkPathTRex();
                default:
                    throw new Exception("unknown miner name");
            }
        }

        public BenchmarkResults LoadBenchmarkResults(MinerAppName minerAppName)
        {
            BenchmarkResults? settings = null;
            try
            {
                string fp = GetMinerSettingsPath(minerAppName);

                string jsonText = File.ReadAllText(fp);
                settings = JsonConvert.DeserializeObject<BenchmarkResults>(jsonText);

                //normalization - if user has only one gpu card it is enabled by default
                int gpusCount = settings?.liveStatus?.GPUs?.Count ?? 0;
                if (gpusCount == 1 && settings != null && settings.liveStatus != null) settings.liveStatus.GPUs.ToArray()[0].Value.IsEnabledByUser = true;
            }
            catch (Exception)
            {
                settings = null;
            }

            if (settings == null || settings.BenchmarkResultVersion != GolemUI.Properties.Settings.Default.BenchmarkResultsVersion)
            {
                settings = null;
            }

            if (settings == null)
            {
                settings = new BenchmarkResults();
            }

            return settings;
        }

        public void SaveBenchmarkResults(BenchmarkResults benchmarkResults, MinerAppName minerAppName)
        {
            if (benchmarkResults == null)
            {
                return;
            }
            benchmarkResults.BenchmarkResultVersion = GolemUI.Properties.Settings.Default.BenchmarkResultsVersion;

            string fp = GetMinerSettingsPath(minerAppName);

            string s = JsonConvert.SerializeObject(benchmarkResults, Formatting.Indented);

            File.WriteAllText(fp, s);
        }
    }
}
