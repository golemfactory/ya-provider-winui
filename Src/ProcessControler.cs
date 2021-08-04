﻿using System;
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

using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using GolemUI.Utils;
using System.Windows;

namespace GolemUI
{
    public class ProcessController : IDisposable, IProcessControler
    {

        private readonly Lazy<string> _generatedAppKey = new Lazy<string>(() =>
        {
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                byte[] data = new byte[20];
                crypto.GetBytes(data);
                var str = Convert.ToBase64String(data);
                return str;
            }
        });

        const string PROVIDER_APP_NAME = "provider";

        private HttpClient _client = new HttpClient();
        private string _baseUrl = "http://127.0.0.1:7465";
        private Command.YagnaSrv _yagna = new Command.YagnaSrv();
        private Command.Provider _provider = new Command.Provider();

        private Process? _yagnaDaemon;
        private Process? _providerDaemon;

        public LogLineHandler? LineHandler { get; set; }

        public string ConfigurationInfoDebug = "";

        public event PropertyChangedEventHandler? PropertyChanged;

        public void KillYagna()
        {
            if (_yagnaDaemon != null)
            {
                _yagnaDaemon.Kill(entireProcessTree: true);
                _yagnaDaemon.Dispose();
                _yagnaDaemon = null;
            }

        }

        public void KillProvider()
        {
            if (_providerDaemon != null)
            {
                _providerDaemon.Kill(entireProcessTree: true);
                _providerDaemon.Dispose();
                _providerDaemon = null;
            }
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
                bool succesfullyExited = await _providerDaemon.StopWithCtrlCAsync(PROVIDER_STOPPING_TIMEOUT);
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

        public void StopYagna()
        {
            const int YAGNA_STOPPING_TIMOUT = 2500;
            if (_yagnaDaemon != null && !_yagnaDaemon.HasExited)
            {
                if (!_yagnaDaemon.StopWithCtrlC(YAGNA_STOPPING_TIMOUT))
                {
                    _yagnaDaemon.Kill();
                }
            }
        }

        public bool IsRunning => IsServerRunning && IsProviderRunning;

        public bool IsProviderRunning => !(_providerDaemon?.HasExited ?? true);

        public bool IsServerRunning => !(_yagnaDaemon?.HasExited ?? true);


        private int _startCnt = 0;
        public bool IsStarting => _startCnt > 0;

        public string ServerUri => _baseUrl;

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

        public async Task<bool> Start(Network network, string? claymoreExtraParams)
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

                    StartupProvider(network, claymoreExtraParams);
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
            _yagnaDaemon = _yagna.Run(new YagnaStartupOptions()
            {
                ForceAppKey = _generatedAppKey.Value,
                OpenConsole = false,
                PrivateKey = privateKey
            });
            _yagnaDaemon.ErrorDataReceived += OnYagnaErrorDataRecv;
            _yagnaDaemon.OutputDataReceived += OnYagnaOutputDataRecv;
            _yagnaDaemon.BeginErrorReadLine();
            _yagnaDaemon.BeginOutputReadLine();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _generatedAppKey.Value);

            //yagna is starting and /me won't work until all services are running
            for (int tries = 0; tries < 300; ++tries)
            {
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

        private void StartupProvider(Network network, string? claymoreExtraParams)
        {
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

            _providerDaemon = _provider.Run(_generatedAppKey.Value, network, claymoreExtraParams: claymoreExtraParams);
            _providerDaemon.Exited += OnProviderExit;
            _providerDaemon.ErrorDataReceived += OnProviderErrorDataRecv;
            _providerDaemon.OutputDataReceived += OnProviderOutputDataRecv;
            _providerDaemon.Start();
            _providerDaemon.EnableRaisingEvents = true;

            _providerDaemon.BeginErrorReadLine();
            _providerDaemon.BeginOutputReadLine();

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

        public void Dispose()
        {
            KillProvider();
            StopYagna();
            _client.Dispose();
        }

        public Task<string> GetAppKey()
        {
            return Task.FromResult(this._generatedAppKey.Value);
        }
    }



}