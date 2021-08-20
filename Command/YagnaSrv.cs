using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace GolemUI.Command
{

    public class YagnaStartupOptions
    {
        public string? ForceAppKey { get; set; }

        public string? PrivateKey { get; set; }

        public bool Debug { get; set; }

        public bool OpenConsole { get; set; }
    }


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
        [JsonProperty("alias")]
        public string? Alias { get; set; }

        [JsonProperty("default")]
        public bool IsDefault { get; set; }

        [JsonProperty("locked")]
        public bool IsLocked { get; set; }

        [JsonProperty("nodeId")]
        public string Address { get; set; }

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
            if (!File.Exists(_yaExePath))
            {
                throw new Exception($"File not found: {_yaExePath}");
            }
        }

        private string _escapeArgument(string argument)
        {
            if (argument.Contains(" ") || argument.StartsWith("\""))
            {
                return $"\"{argument.Replace("\"", "\\\"")}\"";
            }
            return argument;
        }
        private Process _createProcess(params string[] arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = this._yaExePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Arguments = String.Join(" ", (from arg in arguments where arg != null select _escapeArgument(arg)))
            };

            var p = new Process
            {
                StartInfo = startInfo
            };
            p.Start();
            return p;
        }

        internal string ExecToText(params string[] arguments)
        {
            var process = _createProcess(arguments);
            string output = process.StandardOutput.ReadToEnd();
            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                throw new Exception("Yagna call failed");
            }
            return output;
        }

        internal async Task<string> ExecToTextAsync(params string[] arguments)
        {
            var process = _createProcess(arguments);
            return await process.StandardOutput.ReadToEndAsync();
        }

        internal T? Exec<T>(params string[] arguments) where T : class
        {
            var text = ExecToText(arguments);
            return JsonConvert.DeserializeObject<T>(text);
        }

        internal async Task<T?> ExecAsync<T>(params string[] arguments) where T : class
        {
            var text = await ExecToTextAsync(arguments);
            return JsonConvert.DeserializeObject<T>(text);
        }


        public IdSrv Ids
        {
            get
            {
                return new IdSrv(this);
            }
        }

        public IdInfo? Id => Exec<Result<IdInfo>>("--json", "id", "show")?.Ok;

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


        public Process Run(YagnaStartupOptions options)
        {
            string debugFlag = "";
            if (options.Debug)
            {
                debugFlag = "--debug";
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = this._yaExePath,
                Arguments = $"service run {debugFlag}",
            };

            if (options.PrivateKey != null)
            {
                startInfo.EnvironmentVariables.Add("YAGNA_AUTOCONF_ID_SECRET", options.PrivateKey);
            }

            if (options.ForceAppKey != null)
            {
                startInfo.EnvironmentVariables.Add("YAGNA_AUTOCONF_APPKEY", options.ForceAppKey);
            }

            var certs = Path.Combine(Path.GetDirectoryName(_yaExePath), "cacert.pem");
            if (File.Exists(certs))
            {
                startInfo.EnvironmentVariables.Add("SSL_CERT_FILE", certs);
            }

            if (options.OpenConsole)
            {
                startInfo.RedirectStandardOutput = false;
                startInfo.RedirectStandardError = false;
                startInfo.UseShellExecute = false;
            }
            else
            {
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
            }


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

        private string[] prepareArgs(params string[] arguments)
        {
            var execArgs = new List<string>(3 + arguments.Length);
            execArgs.Add("--json");
            if (_id != null)
            {
                execArgs.Add("--id");
                execArgs.Add(_id);
            }
            execArgs.Add("app-key");
            execArgs.AddRange(arguments);

            return execArgs.ToArray();
        }

        private T? Exec<T>(params string[] arguments) where T : class
        {
            return _yagnaSrv.Exec<T>(prepareArgs(arguments));
        }

        private async Task<T?> ExecAsync<T>(params string[] arguments) where T : class
        {
            return await _yagnaSrv.ExecAsync<T>(prepareArgs(arguments));
        }

        public string? Create(string name)
        {
            return Exec<string>("create", name);
        }
        public async Task<string?> CreateAsync(string name)
        {
            return await ExecAsync<string>("create", name);
        }

        public void Drop(string name)
        {
            var opt = Exec<string>("drop", name);
        }

        public List<KeyInfo> List()
        {
            var output = new List<KeyInfo>();
            Table? table = null;
            int tries = 0;
            while (table == null)
            {
                try
                {
                    table = Exec<Table>("list");
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.ToString());
                    //do nothing
                }
                Thread.Sleep(1000);
                tries++;
                if (tries == 10)
                {
                    throw new Exception("Failed to obtain key list from yagna service");
                }
            }
            for (var i = 0; i < table?.Values.Count; ++i)
            {
                output.Add(new KeyInfo(table.Headers, table.Values[i]));
            }
            return output;
        }

        public async Task<List<KeyInfo>> ListAsync()
        {
            var output = new List<KeyInfo>();
            var table = await ExecAsync<Table>("list");
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
            var table = _srv.Exec<Table>("--json", "id", "list");
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

        public void Init(Network network, string driver, string account)
        {
            _srv.Exec<PaymentStatus>("payment", "init", "--receiver", "--network", network.Id, "--driver", driver, "--account", account);
        }

        public async Task<PaymentStatus?> Status(Network network, string driver, string account)
        {
            return await _srv.ExecAsync<PaymentStatus>("--json", "payment", "status", "--network", network.Id, "--driver", driver, "--account", account);
        }

        public async Task<string?> ExitTo(Network network, string driver, string account, string? destination)
        {
            if (destination == null)
            {
                return null;
            }
            return await _srv.ExecToTextAsync("--json", "payment", "exit", "--network", network.Id, "--driver", driver, "--account", account, "--to-address", destination);
        }

        public async Task<ActivityStatus?> ActivityStatus()
        {
            return await _srv.ExecAsync<ActivityStatus>("--json", "activity", "status");
        }

    }
}
