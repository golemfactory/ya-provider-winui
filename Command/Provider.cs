using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using GolemUI.Settings;
using System.Globalization;

namespace GolemUI.Command
{

    [JsonObject(MemberSerialization.OptIn)]
    public class ExeUnitDesc
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("version")]
        public string? Version { get; set; }

        [JsonProperty("supervisor-path")]
        public string? SupervisiorPath { get; set; }

        [JsonProperty("runtime-path")]
        public string? RuntimePath { get; set; }

        [JsonProperty("extra-args")]
        public List<string>? ExtraArgs { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("properties")]
        public JObject? Properties { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Config
    {
        [JsonProperty("node_name")]
        public string? NodeName { get; set; }

        [JsonProperty("subnet")]
        public string? Subnet { get; set; }

        [JsonProperty("account")]
        public string? Account { get; set; }

    }

    public class Preset
    {
        [JsonConstructor]
        public Preset(string name, string exeunitName, Dictionary<string, decimal> usageCoeffs)
        {
            Name = name;
            ExeunitName = exeunitName;
            PricingModel = "linear";
            UsageCoeffs = usageCoeffs;
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("exeunit-name")]
        public string ExeunitName { get; set; }

        [JsonProperty("pricing-model")]
        public string? PricingModel { get; set; }

        [JsonProperty("usage-coeffs")]
        public Dictionary<string, decimal> UsageCoeffs { get; set; }
    }

    public class Provider
    {
        private string _yaProviderPath;
        private string _pluginsPath;
        private string _exeUnitsPath;

        public Provider()
        {
            var appBaseDir = AppContext.BaseDirectory;
            if (appBaseDir == null)
            {
                appBaseDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            if (appBaseDir == null)
            {
                throw new ArgumentException();
            }
            _yaProviderPath = Path.Combine(appBaseDir, "ya-provider.exe");
            _pluginsPath = Path.Combine(appBaseDir, "plugins");
            _exeUnitsPath = Path.Combine(_pluginsPath, @"ya-runtime-*.json");

            if (!File.Exists(_yaProviderPath))
            {
                throw new Exception($"File not found: {_yaProviderPath}");
            }
            if (!Directory.Exists(_pluginsPath))
            {
                throw new Exception($"Plugins directory not found: {_pluginsPath}");
            }

        }

        private T? Exec<T>(string arguments) where T : class
        {
            var text = this.ExecToText(arguments);
            return JsonConvert.DeserializeObject<T>(text);
        }

        private string ExecToText(string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = this._yaProviderPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            startInfo.Environment.Add("EXE_UNIT_PATH", this._exeUnitsPath);

            var process = new Process
            {
                StartInfo = startInfo
            };
            process.Start();
            return process.StandardOutput.ReadToEnd();
        }

        public List<ExeUnitDesc> ExeUnitList()
        {
            return this.Exec<List<ExeUnitDesc>>("--json exe-unit list") ?? new List<ExeUnitDesc>();
        }

        public Config? Config
        {
            get
            {
                return this.Exec<Config>("config get --json");
            }
            set
            {
                if (value != null)
                {
                    StringBuilder cmd = new StringBuilder("--json config set", 60);
                    if (value.Subnet != null)
                    {
                        cmd.Append(" --subnet \"");
                        cmd.Append(value.Subnet);
                        cmd.Append("\"");
                    }
                    if (value.NodeName != null)
                    {
                        cmd.Append(" --node-name \"");
                        cmd.Append(value.NodeName);
                        cmd.Append("\"");
                    }
                    if (value.Account != null)
                    {
                        cmd.Append(" --account \"").Append(value.Account).Append('"');
                    }
                    var _none = this.ExecToText(cmd.ToString());
                }
            }
        }

        public List<Preset> Presets
        {
            get
            {
                return this.Exec<List<Preset>>("--json preset list") ?? new List<Preset>();
            }
        }

        public void ActivatePreset(string presetName)
        {
            this.ExecToText($"preset activate {presetName}");
        }
        public void DeactivatePreset(string presetName)
        {
            this.ExecToText($"preset deactivate {presetName}");
        }

        public void AddPreset(Preset preset, out string args, out string info)
        {
            StringBuilder cmd = new StringBuilder("preset create --no-interactive", 60);

            cmd.Append(" --preset-name \"").Append(preset.Name).Append('"');
            cmd.Append(" --exe-unit \"").Append(preset.ExeunitName).Append('"');
            if (preset.PricingModel != null)
            {
                foreach (KeyValuePair<string, decimal> kv in preset.UsageCoeffs)
                {
                    cmd.Append(" --price ").Append(kv.Key).Append("=").Append(kv.Value.ToString(CultureInfo.InvariantCulture));
                }
            }
            args = cmd.ToString();
            info = this.ExecToText(cmd.ToString());
        }

        public Process Run(string appkey, Network network, string subnet, LocalSettings ls, bool enableClaymoreMining, BenchmarkResults br)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = this._yaProviderPath,
                Arguments = $"run --payment-network {network.Id} --subnet {subnet}",
            };
            if (ls.StartProviderCommandLine)
            {
                startInfo.RedirectStandardOutput = false;
                startInfo.RedirectStandardError = false;
                startInfo.CreateNoWindow = false;
            }
            else
            {
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.CreateNoWindow = true;
            }

            if (ls.EnableDebugLogs)
            {
                startInfo.EnvironmentVariables["RUST_LOG"] = "debug";
            }

            if (enableClaymoreMining)
            {
                string extraParams = "- ";
                if (!String.IsNullOrEmpty(ls.MinerSelectedGPUIndices))
                {
                    string diSwitch = "-di ";
                    string cards = ls.MinerSelectedGPUIndices?.Replace(",", "")?.Replace(".", "")?.Trim();

                    diSwitch += cards;

                    extraParams += diSwitch;
                }
                startInfo.EnvironmentVariables["EXTRA_CLAYMORE_PARAMS"] = extraParams;
            }
            startInfo.EnvironmentVariables["MIN_AGREEMENT_EXPIRATION"] = "30s";
            startInfo.EnvironmentVariables["EXE_UNIT_PATH"] = _exeUnitsPath;
            //startInfo.EnvironmentVariables["DATA_DIR"] = "data_dir";
            startInfo.EnvironmentVariables["YAGNA_APPKEY"] = appkey;

            var process = new Process
            {
                StartInfo = startInfo
            };

            return process;
        }
    }
}
