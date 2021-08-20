using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GolemUI.Command.GSB
{
    public class Payment
    {
        private readonly IGsbEndpointFactory _gsbEndpointFactory;

        public class Model
        {
            public class GetStatus
            {
                public string Address { get; set; }

                public string Driver { get; set; }

                public string? Network { get; set; }

                public string? Token { get; set; }

                public GetStatus(string address, string driver, string? network = null, string? token = null)
                {
                    Address = address ?? throw new ArgumentNullException(nameof(address));
                    Driver = driver ?? throw new ArgumentNullException(nameof(driver));
                    Network = network;
                    Token = token;
                }
            }

            public class StatusResult
            {
                public decimal Amount { get; set; }

                public decimal Reserved { get; set; }

                public StatusNotes Outgoing { get; set; } = default!;
                public StatusNotes Incoming { get; set; } = default!;
                public string Driver { get; set; } = default!;
                public string Network { get; set; } = default!;

                public string Token { get; set; } = default!;
            }

            public class StatusNotes
            {
                public StatValue Requested { get; set; } = default!;
                public StatValue Accepted { get; set; } = default!;
                public StatValue Confirmed { get; set; } = default!;
            }

        }
              

        public Payment(IGsbEndpointFactory gsbEndpointFactory)
        {
            _gsbEndpointFactory = gsbEndpointFactory;
        }

        public async Task<Model.StatusResult> GetStatus(string address, string driver, string? network = null, string? token = null)
        {
            var result = await _doPost<Common.Result<Model.StatusResult, object>, Model.GetStatus>("local/payment/GetStatus", new Model.GetStatus(address, driver, network, token));
            return result.Ok ?? throw new HttpRequestException($"Invalid output: {result.Err}");
        }

        private async Task<TOut> _doPost<TOut, TIn>(string serviceUri, TIn input)
        {
            var options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };

            using HttpClient client = await _gsbEndpointFactory.NewClient();
            var json = JsonSerializer.Serialize(input, options);
            var requestBody = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(serviceUri, requestBody);
            var outputData = await response.Content.ReadAsByteArrayAsync();
            return JsonSerializer.Deserialize<TOut>(outputData, options: options) ?? throw new HttpRequestException("Invalid output");
        }
    }
}
