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

namespace GolemUI.Services
{
    public class ProcessController : IDisposable, IProcessControler
    {
        const string PROVIDER_APP_NAME = "provider";

        private HttpClient _client = new HttpClient();
        private string _baseUrl = "http://127.0.0.1:7465";
        private Command.YagnaSrv _yagna = new Command.YagnaSrv();
        private Command.Provider _provider = new Command.Provider();
        private string? _appkey;
        private Process? _main;

        public void Dispose()
        {
            _client.Dispose();
            if (_main != null)
            {
                _main.Kill();
                _main.Dispose();
            }
        }

        public async Task<bool> Init()
        {
            bool runYagna = false;
            try
            {
                var _test = await _client.GetAsync(_baseUrl);
                Console.WriteLine($"result={_test.StatusCode}");
            }
            catch (HttpRequestException e)
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

                //var key = _yagna.AppKey.List().Where(key => key.Name == PROVIDER_APP_NAME).FirstOrDefault();
                if (_key != null)
                {
                    _appkey = _key.Key;
                }
                else
                {
                    _appkey = _yagna.AppKey.Create(PROVIDER_APP_NAME);
                }
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _appkey);

                _yagna.Payment.PaymentInit(Command.Network.Rinkeby, "zksynch");

                _provider.Run();




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
    }


}