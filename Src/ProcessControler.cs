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
                    FreeConsole();
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

        public void Dispose()
        {
            StopProvider();
            StopYagna();
            _client.Dispose();
        }
        public void KillYagna()
        {
            if (_yagnaDaemon != null)
            {
                _yagnaDaemon.Kill();
                _yagnaDaemon.Dispose();
                _yagnaDaemon = null;
            }

        }

        public void KillProvider()
        {
            if (_providerDaemon != null)
            {
                _providerDaemon.Kill();
                _providerDaemon.Dispose();
                _providerDaemon = null;
            }
        }

        public async Task<bool> StopProvider()
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

        public async Task<string> GetAppKey()
        {
            if (_appkey == null)
            {
                await Init();
            }
            return _appkey ?? throw new InvalidOperationException();
        }

        public async Task<bool> Init()
        {
            /*
            bool runYagna = false;
            try
            {
                var _test = await _client.GetAsync(_baseUrl);
                Console.WriteLine($"result={_test.StatusCode}");
            }
            catch (HttpRequestException)
            {
                runYagna = true;
            }*/

            var t = new Thread(() =>
            {
                StartupYagna();

                
                bool runYagna = false;


                var _key = _yagna.AppKey.List().Where(key => key.Name == PROVIDER_APP_NAME).FirstOrDefault();

                if (_key != null)
                {
                    _appkey = _key.Key;
                }
                else
                {
                    _appkey = _yagna.AppKey.Create(PROVIDER_APP_NAME);
                }
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _appkey);
                if (Subnet == null)
                {
                    throw new Exception("Subnet cannot be null"); 
                }

                //yagna is starting and /me won't work until all services are running
                int tries = 0;
                while(true)
                {
                    if (tries >= 10)
                    {
                        throw new Exception("Cannot connect to yagna server");
                    }
                    try
                    {
                        var txt = _client.GetStringAsync($"{_baseUrl}/me").Result;
                        KeyInfo? key = JsonConvert.DeserializeObject<Command.KeyInfo>(txt) ?? null;
                        if (key != null && _key != null && key.Id == _key.Id)
                        {
                            StartupProvider(Network.Rinkeby, Subnet);
                        }
                        else
                        {
                            throw new Exception("Failed to get key");
                        }
                        break;
                    }
                    catch (Exception ex)
                    {
                        Thread.Sleep(1000);
                    }
                    tries += 1;
                }
            });

            t.Start();

            while (t.IsAlive)
            {
                await Task.Delay(200);
            }


            return true;
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

        private void StartupProvider(Network network, string subnet)
        {
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
            /*var subnet = config?.Subnet;
            if (subnet == null)
            {
                throw new Exception("Failed to retrieve Subnet account");
            }*/

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

            BenchmarkResults br = SettingsLoader.LoadBenchmarkFromFileOrDefault();
            LocalSettings ls = SettingsLoader.LoadSettingsFromFileOrDefault();

            bool enableClaymoreMining = br.liveStatus != null && /*br.liveStatus.BenchmarkFinished && */br.liveStatus.GetEnabledGpus().Count > 0;

            if (enableClaymoreMining)
            {
                var usageCoef = new Dictionary<string, decimal>();
                var preset = new Preset("gminer", "gminer", usageCoef);
                preset.UsageCoeffs.Add("share", 0);
                preset.UsageCoeffs.Add("duration", 0);
                _provider.AddPreset(preset);
                _provider.ActivatePreset(preset.Name);
            }

            {
                var usageCoef = new Dictionary<string, decimal>();
                var preset = new Preset("wasmtime", "wasmtime", usageCoef);
                preset.UsageCoeffs.Add("cpu", 0);
                preset.UsageCoeffs.Add("duration", 0);
                _provider.AddPreset(preset);
                _provider.ActivatePreset(preset.Name);
            }

            _provider.DeactivatePreset("default");
            _provider.ActivatePreset("gminer");
            _provider.ActivatePreset("wasmtime");

            _providerDaemon = _provider.Run(_appkey, network, subnet, ls, enableClaymoreMining, br);
            _providerDaemon.Exited += OnProviderExit;
            _providerDaemon.ErrorDataReceived += OnProviderErrorDataRecv;
            _providerDaemon.OutputDataReceived += OnProviderOutputDataRecv;
            _providerDaemon.Start();

            if (!ls.StartProviderCommandLine)
            {
                _providerDaemon.BeginErrorReadLine();
                _providerDaemon.BeginOutputReadLine();
            }
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
    }



}