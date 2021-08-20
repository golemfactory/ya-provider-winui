using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using GolemUI.Interfaces;

namespace GolemUI.Src
{
    class GsbEndpointFactory : Command.GSB.IGsbEndpointFactory
    {
        private readonly IProcessControler _processControler;

        public GsbEndpointFactory(IProcessControler processControler)
        {
            _processControler = processControler;
        }
        public async Task<HttpClient> NewClient()
        {
            var appKey = await _processControler.GetAppKey();

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", appKey);
            client.BaseAddress = new Uri(new Uri(_processControler.ServerUri), "_gsb/");
            return client;
        }
    }
}
