using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace GolemUI.Command
{
    /* 
    {
    "name": "wasmtime",
    "version": "0.2.1",
    "supervisor-path": "C:\\Users\\golem-dev\\Documents\\yagna\\plugins\\exe-unit.exe",
    "extra-args": [],
    "runtime-path": "C:\\Users\\golem-dev\\Documents\\yagna\\plugins\\ya-runtime-wasi.exe",
    "properties": {},
    "description": "wasmtime wasi runtime"
    }
    */
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

    public class Provider
    {
        private string _yaProviderPath;
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
            _yaProviderPath = Path.Combine(appBaseDir, "ya-provider");
            _exeUnitsPath = Path.Combine(appBaseDir, @"plugins\ya-runtime-*.json");
        }

        public List<ExeUnitDesc> ExeUnitList()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = this._yaProviderPath,
                Arguments = "--json exe-unit list",
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

            
            var resultTxt = process.StandardOutput.ReadToEnd();
            return JsonConvert.DeserializeObject<List<ExeUnitDesc>>(resultTxt) ?? new List<ExeUnitDesc>();
        }

    }
}
