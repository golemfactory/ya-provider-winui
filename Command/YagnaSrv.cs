using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GolemUI.Command
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class Table
    {
        [JsonProperty("headers")]
        public string[] Headers { get; set; }

        [JsonProperty("values")]
        public List<JValue[]> Values { get; set; }

        [JsonConstructor]
        public Table(string[] headers, List<JValue[]> values)
        {
            this.Headers = headers;
            this.Values = values;
        }

    }

    [JsonObject(MemberSerialization.OptIn)]
    public class IdInfo
    {
        [JsonProperty]
        string? Alias { get; set; }

        [JsonProperty]
        bool IsDefault { get; set; }

        [JsonProperty]
        bool IsLocked { get; set; }

        [JsonProperty]
        string Address { get; set; }

        public IdInfo(bool _isDefault, bool _isLocked, string? _alias, string _address)
        {
            IsDefault = _isDefault;
            IsLocked = _isLocked;
            Alias = _alias;
            Address = _address;
        }

        internal IdInfo(string[] _headers, JValue[] _row)
        {
            for (var i=0; i<_headers.Length; ++i)
            {
                var value = _row[i];
                switch (_headers[i])
                {
                    case "default":
                        IsDefault = value.Value != null && value.Value.ToString() == "X";
                        break;
                    case "locked":
                        IsLocked = value.Value != null && value.Value.ToString() == "X";
                        break;
                    case "alias":
                        Alias = value.Value as string;
                        break;
                    case "address":
                        if (value.Value != null)
                        {
                            Address = value.Value.ToString();
                        }
                        break;
                }
            }
        }
    }

    public class YagnaSrv
    {
        private string _yaExePath;

        public YagnaSrv()
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
            _yaExePath = Path.Combine(appBaseDir, "yagna.exe");
        }

        internal string ExecToText(string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = this._yaExePath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };            

            var process = new Process
            {
                StartInfo = startInfo
            };
            process.Start();
            return process.StandardOutput.ReadToEnd();
        }

        internal T? Exec<T>(string arguments) where T : class
        {
            var text = ExecToText(arguments);
            return JsonConvert.DeserializeObject<T>(text);
        }

        public IdSrv Id
        {
            get
            {
                return new IdSrv(this);
            }
        }
    }

    public class IdSrv
    {
        YagnaSrv _srv;

        internal IdSrv(YagnaSrv srv)
        {
            _srv = srv;
        }

        public List<IdInfo> List()
        {
            var table = _srv.Exec<Table>("--json id list");
            var ret = new List<IdInfo>();
            if (table == null)
            {
                return ret;
            }

            foreach (var row in table.Values)
            {
                ret.Add(new IdInfo(table.Headers, row));
            }
            return ret;
        }

    }
}
