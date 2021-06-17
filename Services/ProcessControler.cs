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

namespace GolemUI.Services
{
    public class ProcessController : IDisposable, IProcessControler, IAppKeyProvider
    {
        const string PROVIDER_APP_NAME = "provider";

        private HttpClient _client = new HttpClient();
        private string _baseUrl = "http://127.0.0.1:7465";
        private Command.YagnaSrv _yagna = new Command.YagnaSrv();
        private Command.Provider _provider = new Command.Provider();
        private string? _appkey;
        private Process? _main;
        private Process? _providerDaemon;

        public LogLineHandler? LineHandler { get; set; }


        public void Dispose()
        {
            Stop();
            _client.Dispose();
        }

        public void Stop()
        {
            if (_providerDaemon != null)
            {
                _providerDaemon.Kill();
                _providerDaemon.Dispose();
                _providerDaemon = null;
            }

            if (_main != null)
            {
                _main.Kill();
                _main.Dispose();
                _main = null;
            }
        }

        public bool IsRunning
        {
            get
            {
                return !((_providerDaemon?.HasExited ?? true) || (_main?.HasExited ?? true));
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
            bool runYagna = false;
            try
            {
                var _test = await _client.GetAsync(_baseUrl);
                Console.WriteLine($"result={_test.StatusCode}");
            }
            catch (HttpRequestException)
            {
                runYagna = true;
            }

            var t = new Thread(() =>
            {
                if (runYagna)
                {
                    _main = _yagna.Run();
                }
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

                StartupProvider(Network.Rinkeby);
            });

            t.Start();

            while (t.IsAlive)
            {
                await Task.Delay(500);
            }


            return true;
        }

        public async Task<Command.KeyInfo> Me()
        {
            var txt = await _client.GetStringAsync($"{_baseUrl}/me");
            return JsonConvert.DeserializeObject<Command.KeyInfo>(txt) ?? throw new HttpRequestException("null response on /me");
        }


        private void StartupProvider(Network network)
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
            _yagna?.Payment.Init(network, "erc20", paymentAccount);
            _yagna?.Payment.Init(network, "zksync", paymentAccount);
            if (_appkey == null)
            {
                throw new Exception("Appkey cannot be null");
            }
            _providerDaemon = _provider.Run(_appkey, network);
            _providerDaemon.Exited += OnProviderExit;
            _providerDaemon.ErrorDataReceived += OnProviderErrorDataRecv;
            _providerDaemon.Start();
            _providerDaemon.BeginErrorReadLine();
        }

        public delegate void LogLine(string logger, string line);

        void OnProviderErrorDataRecv(object sender, DataReceivedEventArgs e)
        {
            if (LineHandler != null && e.Data != null)
            {
                LineHandler("provider", e.Data);
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