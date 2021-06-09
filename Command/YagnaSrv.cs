using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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

    [JsonObject(MemberSerialization.OptIn, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class IdInfo
    {
        [JsonProperty("alias")]
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
            Address = "";
            for (var i = 0; i < _headers.Length; ++i)
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
                            Address = value.Value.ToString() ?? "";
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

        public IdSrv Ids
        {
            get
            {
                return new IdSrv(this);
            }
        }

        public IdInfo? Id => Exec<IdInfo>("--json id show");

        public PaymentSrv Payment
        {
            get
            {
                return new PaymentSrv(this);
            }
        }

        public AppKeySrv AppKey
        {
            get
            {
                return new AppKeySrv(this, null);
            }
        }


        public Process Run()
        {

            var startInfo = new ProcessStartInfo
            {
                FileName = this._yaExePath,
                Arguments = "service run",
#if DEBUG
                //UseShellExecute = false,
                //RedirectStandardOutput = true,
                //CreateNoWindow = true
#else
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
#endif
            };
            //startInfo.EnvironmentVariables.Add();
            var process = new Process
            {
                StartInfo = startInfo
            };
            process.Start();

            return process;
        }
    }

    public class KeyInfo
    {
        public string Name { get; }

        public string? Key { get; }

        public string Id { get; }

        public string? Role { get; }

        public DateTime? Created { get; }

        [JsonConstructor]
        public KeyInfo(string identity, string name, string? role)
        {
            Id = identity;
            Name = name;
            Role = role;
        }

        internal KeyInfo(string[] _headers, JValue[] _row)
        {
            Name = "";
            Key = "";
            Id = "";
            for (var i = 0; i < _headers.Length; ++i)
            {
                var value = _row[i];
                switch (_headers[i])
                {
                    case "name":
                        Name = value?.Value?.ToString() ?? "";
                        break;
                    case "key":
                        Key = value?.Value?.ToString() ?? "";
                        break;
                    case "id":
                        Id = value?.Value?.ToString() ?? "";
                        break;
                    case "role":
                        Role = value?.Value?.ToString();
                        break;

                    case "created":
                        var dt = value?.Value?.ToString();
                        if (dt != null)
                        {
                            Created = DateTime.Parse(dt, null, DateTimeStyles.AssumeUniversal);
                        }
                        break;
                }
            }
        }
    }

    public class AppKeySrv
    {
        private YagnaSrv _yagnaSrv;
        private string? _id;

        internal AppKeySrv(YagnaSrv yagnaSrv, string? id)
        {
            this._yagnaSrv = yagnaSrv;
            this._id = id;
        }

        private T? Exec<T>(string arguments) where T : class
        {
            string idSpec = _id == null ? "" : $" --id \"{_id}\"";
            var args = $"--json app-key{idSpec} {arguments}";

            return _yagnaSrv.Exec<T>(args);
        }



        public string? Create(string name)
        {
            return Exec<string>($"create \"{name}\"");
        }

        public void Drop(string name)
        {
            var opt = Exec<string>($"drop \"{name}\"");
        }

        public List<KeyInfo> List()
        {
            var output = new List<KeyInfo>();
            var table = Exec<Table>("list");
            for (var i = 0; i < table?.Values.Count; ++i)
            {
                output.Add(new KeyInfo(table.Headers, table.Values[i]));
            }
            return output;
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


    public class PaymentSrv
    {
        YagnaSrv _srv;

        internal PaymentSrv(YagnaSrv srv)
        {
            _srv = srv;
        }

        public PaymentStatus? Status(Network network, string driver)
        {
            return _srv.Exec<PaymentStatus>($"--json payment status --network \"{network.Id}\" --driver \"{driver}\"");
        }

    }
}
