using System;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Threading;

using System.Diagnostics;
using GolemUI.Interfaces;
using GolemUI.Command;
using GolemUI.Settings;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace GolemUI
{
    public class ProcessController : IDisposable, IProcessControler
    {


        const int CTRL_C_EVENT = 0;
        [DllImport("kernel32.dll")]
        static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AttachConsole(uint dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate? HandlerRoutine, bool Add);
        // Delegate type to be used as the Handler Routine for SCCH
        delegate Boolean ConsoleCtrlDelegate(uint CtrlType);

        private Src.LazyInit<string> _generatedAppKey = new Src.LazyInit<string>(() =>
        {
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                byte[] data = new byte[20];
                crypto.GetBytes(data);
                var str = Convert.ToBase64String(data);
                return str;
            }
        });

        public async Task<bool> SendCtrlCToProcess(Process p, int timeoutMillis)
        {
            bool succesfullyExited = false;
            if (AttachConsole((uint)p.Id))
            {
                SetConsoleCtrlHandler(null, true);

                try
                {
                    if (!GenerateConsoleCtrlEvent(CTRL_C_EVENT, 0))
                    {
                        return false;
                    }

                    const int CHECK_EVERY_MILLIS = 350;
                    int tries = timeoutMillis / CHECK_EVERY_MILLIS;
                    for (int i = 0; i < tries; i++)
                    {
                        succesfullyExited = p.HasExited;
                        if (succesfullyExited)
                            break;
                        await Task.Delay(CHECK_EVERY_MILLIS);
                    }
                }
                finally
                {
                    SetConsoleCtrlHandler(null, false);
                }
            }
            return succesfullyExited;
        }


        const string PROVIDER_APP_NAME = "provider";

        private HttpClient _client = new HttpClient();
        private string _baseUrl = "http://127.0.0.1:7465";
        private Command.YagnaSrv _yagna = new Command.YagnaSrv();
        private Command.Provider _provider = new Command.Provider();
        public string? Subnet { get; set; }

        private Process? _yagnaDaemon;
        private Process? _providerDaemon;

        public LogLineHandler? LineHandler { get; set; }

        public string ConfigurationInfoDebug = "";

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Dispose()
        {
            //StopProvider();
            //StopYagna();
            _client.Dispose();
        }
        public void KillYagna()
        {
            if (_yagnaDaemon != null)
            {
                _yagnaDaemon.Kill(/*entireProcessTree: true*/);
                _yagnaDaemon.Dispose();
                _yagnaDaemon = null;
            }

        }

        public void KillProvider()
        {
            if (_providerDaemon != null)
            {
                _providerDaemon.Kill(/*entireProcessTree: true*/);
                _providerDaemon.Dispose();
                _providerDaemon = null;
            }
        }

        public async Task<string> GetMeInfo()
        {
            var txt = await _client.GetStringAsync($"{_baseUrl}/me");
            return txt;
        }

        public async Task<string> GetOffers()
        {
            var txt = await _client.GetStringAsync($"{_baseUrl}/market-api/v1/offers");
            return txt;
        }

        public async Task<PaymentStatus?> GetPaymentStatus(string account)
        {
            if (_yagna.Payment != null)
            {
                PaymentStatus? st = await _yagna.Payment.Status(Network.Rinkeby, "zksync", account);
                return st;
            }
            return null;
        }
        public async Task<ActivityStatus?> GetActivityStatus()
        {
            if (_yagna.Payment != null)
            {
                ActivityStatus? st = await _yagna.Payment.ActivityStatus();
                return st;
            }
            return null;
        }

        private async Task<bool> StopProvider()
        {
            const int PROVIDER_STOPPING_TIMEOUT = 2500;
            if (_providerDaemon != null)
            {
                bool succesfullyExited = await SendCtrlCToProcess(_providerDaemon, PROVIDER_STOPPING_TIMEOUT);
                if (succesfullyExited)
                {
                    _providerDaemon = null;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> StopYagna()
        {
            const int YAGNA_STOPPING_TIMOUT = 2500;
            if (_yagnaDaemon != null)
            {
                bool succesfullyExited = await SendCtrlCToProcess(_yagnaDaemon, YAGNA_STOPPING_TIMOUT);
                if (succesfullyExited)
                {
                    _yagnaDaemon = null;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsRunning => IsServerRunning && IsProviderRunning;

        public bool IsProviderRunning => !(_providerDaemon?.HasExited ?? true);

        public bool IsServerRunning => !(_yagnaDaemon?.HasExited ?? true);


        private int _startCnt = 0;
        public bool IsStarting => _startCnt > 0;

        private void _lock()
        {
            if (++_startCnt == 1)
            {
                OnPropertyChanged("IsStarting");
            }
        }

        private void _unlock()
        {
            if (--_startCnt == 0)
            {
                OnPropertyChanged("IsStarting");
            }
        }

        public async Task<bool> Start()
        {
            _lock();
            try
            {
                await Task.Run(() =>
                {
                    if (_yagnaDaemon == null)
                    {

                        StartupYagna();
                    }

                    StartupProvider(Network.Rinkeby, Subnet);
                });
                OnPropertyChanged("IsServerRunning");

            }
            finally
            {
                _unlock();
            }
            return true;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public async Task<Command.KeyInfo> Me()
        {
            var txt = await _client.GetStringAsync($"{_baseUrl}/me");
            return JsonConvert.DeserializeObject<Command.KeyInfo>(txt) ?? throw new HttpRequestException("null response on /me");
        }
        private KeyInfo StartupYagna(string? privateKey = null)
        {
            _yagnaDaemon = _yagna.Run(new YagnaStartupOptions() { 
                ForceAppKey = _generatedAppKey.Value,
                OpenConsole = Properties.Settings.Default.StartYagnaCommandLine,
                PrivateKey = privateKey
            });
            if (!Properties.Settings.Default.StartYagnaCommandLine)
            {
                _yagnaDaemon.ErrorDataReceived += OnYagnaErrorDataRecv;
                _yagnaDaemon.OutputDataReceived += OnYagnaOutputDataRecv;
                _yagnaDaemon.BeginErrorReadLine();
                _yagnaDaemon.BeginOutputReadLine();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _generatedAppKey.Value);

            //yagna is starting and /me won't work until all services are running
            for (int tries = 0; tries < 300; ++tries) { 
                if (_yagnaDaemon.HasExited)
                {
                    break;
                }

                try
                {
                    var response = _client.GetAsync($"{_baseUrl}/me").Result;  
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        break;
                    }
                    var txt = response.Content.ReadAsStringAsync().Result;
                    KeyInfo? keyMe = JsonConvert.DeserializeObject<Command.KeyInfo>(txt) ?? null;
                    //sanity check
                    if (keyMe != null)
                    {
                        return keyMe;
                    }
                    throw new Exception("Failed to get key");
                }
                catch (Exception)
                {
                    Thread.Sleep(300);
                }
                tries += 1;
            }
            throw new Exception("Failed to get key");
        }

        private void StartupProvider(Network network, string? subnet)
        {
            BenchmarkResults br = SettingsLoader.LoadBenchmarkFromFileOrDefault();
            LocalSettings ls = SettingsLoader.LoadSettingsFromFileOrDefault();

            ConfigurationInfoDebug = "";
            if (_providerDaemon != null)
            {
                return;
            }
            var config = _provider.Config;

            var paymentAccount = config?.Account ?? _yagna?.Id?.Address;
            if (paymentAccount == null)
            {
                throw new Exception("Failed to retrieve payment Account");
            }

            _yagna?.Payment.Init(network, "erc20", paymentAccount);
            _yagna?.Payment.Init(network, "zksync", paymentAccount);

            var providerPresets = _provider.Presets;

            //make sure we are starting from the same preset all the time by clearing configuration before starting provider
            //otherwise there will be lot of mess to handle (if user changes something or old configuration exist)
            SettingsLoader.ClearProviderPresetsFile();


            string reason;
            bool enableClaymoreMining = br.IsClaymoreMiningPossible(out reason);

            if (enableClaymoreMining)
            {
                var usageCoef = new Dictionary<string, decimal>();
                var preset = new Preset("gminer", "gminer", usageCoef);
                preset.UsageCoeffs.Add("share", 0.1m);
                preset.UsageCoeffs.Add("duration", 0);
                string info, args;
                _provider.AddPreset(preset, out args, out info);
                ConfigurationInfoDebug += "Add preset claymore mining: \nargs:\n" + args + "\nresponse:\n" + info;
                _provider.ActivatePreset(preset.Name);
            }

            {
                var usageCoef = new Dictionary<string, decimal>();
                var preset = new Preset("wasmtime", "wasmtime", usageCoef);
                preset.UsageCoeffs.Add("cpu", 0.1m);
                preset.UsageCoeffs.Add("duration", 0m);
                string info, args;
                _provider.AddPreset(preset, out args, out info);
                ConfigurationInfoDebug += "Add preset claymore WASM: \nargs:\n" + args + "\nresponse:\n" + info;
                _provider.ActivatePreset(preset.Name);
            }

            _provider.DeactivatePreset("default");
            if (enableClaymoreMining)
            {
                _provider.ActivatePreset("gminer");
            }
            _provider.ActivatePreset("wasmtime");
            _providerDaemon = _provider.Run(_generatedAppKey.Value, network, ls, enableClaymoreMining, br);
            _providerDaemon.Exited += OnProviderExit;
            _providerDaemon.ErrorDataReceived += OnProviderErrorDataRecv;
            _providerDaemon.OutputDataReceived += OnProviderOutputDataRecv;
            _providerDaemon.Start();
            _providerDaemon.EnableRaisingEvents = true;

            if (!ls.StartProviderCommandLine)
            {
                _providerDaemon.BeginErrorReadLine();
                _providerDaemon.BeginOutputReadLine();
            }
            OnPropertyChanged("IsProviderRunning");
        }

        public delegate void LogLine(string logger, string line);

        void OnProviderErrorDataRecv(object sender, DataReceivedEventArgs e)
        {
            if (LineHandler != null && e.Data != null)
            {
                LineHandler("provider", e.Data);
            }
        }
        void OnProviderOutputDataRecv(object sender, DataReceivedEventArgs e)
        {
            if (LineHandler != null && e.Data != null)
            {
                LineHandler("provider", e.Data);
            }
        }

        void OnYagnaErrorDataRecv(object sender, DataReceivedEventArgs e)
        {
            if (LineHandler != null && e.Data != null)
            {
                LineHandler("yagna", e.Data);
            }
        }
        void OnYagnaOutputDataRecv(object sender, DataReceivedEventArgs e)
        {
            if (LineHandler != null && e.Data != null)
            {
                LineHandler("yagna", e.Data);
            }
        }

        void OnProviderExit(object? sender, EventArgs e)
        {
            if (LineHandler != null)
            {
                LineHandler("provider", "provider exit");
            }
            if (_providerDaemon != null && _providerDaemon.HasExited)
            {
                _providerDaemon.Dispose();
                _providerDaemon = null;
            }
            OnPropertyChanged("IsProviderRunning");
        }

        public async Task<bool> Prepare()
        {
            if (_yagnaDaemon == null)
            {
                _lock();
                try
                {
                    await Task.Run(() => StartupYagna());
                    OnPropertyChanged("IsServerRunning");
                }
                finally
                {
                    _unlock();
                }
            }
            return true;
        }
        private static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public async Task<string> PrepareForKey(byte[] privateKey)
        {

            if (_yagnaDaemon != null && _yagnaDaemon.HasExited)
            {
                _yagnaDaemon = null;
            }

            if (_yagnaDaemon == null)
            {
                _lock();
                try
                {
                    var key = await Task.Run(() => StartupYagna(ByteArrayToString(privateKey)));
                    OnPropertyChanged("IsServerRunning");
                    return key.Id;
                }
                finally
                {
                    _unlock();
                }
            }
            else
            {
                throw new InvalidOperationException("golem node already running");
            }
        }

        public async Task<bool> Stop()
        {
            try
            {
                _lock();
                bool providerEndedSuccessfully = await StopProvider();
                if (!providerEndedSuccessfully)
                {
                    KillProvider();
                }
            }
            finally
            {
                OnPropertyChanged("IsProviderRunning");
                _unlock();
            }

            return true;
        }

      
    }



}