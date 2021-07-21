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

namespace GolemUI
{



    public class ProcessController : IDisposable, IProcessControler, IAppKeyProvider
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
        private string? _appkey;
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

        public bool IsRunning
        {
            get
            {
                return !((_providerDaemon?.HasExited ?? true) || (_yagnaDaemon?.HasExited ?? true));
            }
        }

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

        public async Task<string> GetAppKey()
        {
            if (_appkey == null)
            {
                await Prepare();
            }
            return _appkey ?? throw new InvalidOperationException();
        }

        public KeyInfo? GetFirstAppKey()
        {
            var key = _yagna.AppKey.List().Where(key => key.Name == PROVIDER_APP_NAME).FirstOrDefault();
            return key;
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

                    var keyInfo = GetFirstAppKey();
                    if (keyInfo != null)
                    {
                        _appkey = keyInfo.Key;
                    }
                    else
                    {
                        _appkey = _yagna.AppKey.Create(PROVIDER_APP_NAME);
                        keyInfo = GetFirstAppKey();
                    }
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _appkey);                    

                //yagna is starting and /me won't work until all services are running
                int tries = 0;
                    while (true)
                    {
                        if (tries >= 30)
                        {
                            throw new Exception("Cannot connect to yagna server");
                        }
                        try
                        {
                            var txt = _client.GetStringAsync($"{_baseUrl}/me").Result;
                            KeyInfo? keyMe = JsonConvert.DeserializeObject<Command.KeyInfo>(txt) ?? null;
                        //sanity check
                        if (keyMe != null && keyInfo != null && keyMe.Id == keyInfo.Id)
                            {
                                break;
                            }
                            throw new Exception("Failed to get key");
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(1000);
                        }
                        tries += 1;
                    }

                    Thread.Sleep(1000);                  

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
        private void StartupYagna()
        {
            LocalSettings ls = SettingsLoader.LoadSettingsFromFileOrDefault();

            _yagnaDaemon = _yagna.Run();
            if (!ls.StartYagnaCommandLine)
            {
                _yagnaDaemon.ErrorDataReceived += OnYagnaErrorDataRecv;
                _yagnaDaemon.OutputDataReceived += OnYagnaOutputDataRecv;
                _yagnaDaemon.BeginErrorReadLine();
                _yagnaDaemon.BeginOutputReadLine();
            }
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
            if (_appkey == null)
            {
                throw new Exception("Appkey cannot be null");
            }

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
                preset.UsageCoeffs.Add("share", new decimal(0.1));
                preset.UsageCoeffs.Add("duration", new decimal(0.0001));
                string info, args;
                _provider.AddPreset(preset, out args, out info);
                ConfigurationInfoDebug += "Add preset claymore mining: \nargs:\n" + args + "\nresponse:\n" + info;
                _provider.ActivatePreset(preset.Name);
            }

            {
                var usageCoef = new Dictionary<string, decimal>();
                var preset = new Preset("wasmtime", "wasmtime", usageCoef);
                preset.UsageCoeffs.Add("cpu", 1);
                preset.UsageCoeffs.Add("duration", 1);
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
            _providerDaemon = _provider.Run(_appkey, network, ls, enableClaymoreMining, br);
            _providerDaemon.Exited += OnProviderExit;
            _providerDaemon.ErrorDataReceived += OnProviderErrorDataRecv;
            _providerDaemon.OutputDataReceived += OnProviderOutputDataRecv;
            _providerDaemon.Start();

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
        }

        public async Task<bool> Prepare()
        {
            if (_yagnaDaemon == null) 
            {
                _lock();
                try
                {
                    await Task.Run(() => StartupYagna());
                    await Task.Delay(2000);
                    OnPropertyChanged("IsServerRunning");
                }
                finally
                {
                    _unlock();
                }
            }
            return true;
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