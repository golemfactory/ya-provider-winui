using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Command.GSB
{
    public interface IGsbEndpointFactory
    {
        Task<HttpClient> NewClient();
    }
}
