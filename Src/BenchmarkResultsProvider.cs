﻿using System;
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
    class BenchmarkResultsProvider : IBenchmarkResultsProvider
    {
        public BenchmarkResults LoadBenchmarkResults()
        {
            BenchmarkResults? settings = null;
            try
            {
                string fp = PathUtil.GetLocalBenchmarkPath();
                string jsonText = File.ReadAllText(fp);
                settings = JsonConvert.DeserializeObject<BenchmarkResults>(jsonText);
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

        public void SaveBenchmarkResults(BenchmarkResults benchmarkResults)
        {
            if (benchmarkResults == null)
            {
                return;
            }
            benchmarkResults.BenchmarkResultVersion = GolemUI.Properties.Settings.Default.BenchmarkResultsVersion;

            string fp = PathUtil.GetLocalBenchmarkPath();

            string s = JsonConvert.SerializeObject(benchmarkResults, Formatting.Indented);

            File.WriteAllText(fp, s);
        }
    }
}