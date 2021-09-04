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
        private readonly IProcessController _processController;

        public GsbEndpointFactory(IProcessController processController)
        {
            _processController = processController;
        }
        public async Task<HttpClient> NewClient()
        {
            var appKey = await _processController.GetAppKey();

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", appKey);
            client.BaseAddress = new Uri(new Uri(_processController.ServerUri), "_gsb/");
            return client;
        }
    }
}
