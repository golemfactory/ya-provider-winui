using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GolemUI.Command.GSB
{
    public class Identity
    {
        private readonly IGsbEndpointFactory _gsbEndpointFactory;

        public Identity(IGsbEndpointFactory gsbEndpointFactory)
        {
            _gsbEndpointFactory = gsbEndpointFactory;
        }

        public class IdentityInfo
        {
            public string? Alias { get; set; }
            public string NodeId { get; set; }

            public bool IsLocked { get; set; }

            public bool IsDefault { get; set; }
        }

        public class SignRequest
        {
            public string NodeId { get; set; }

            public int[] Payload { get; set; }
        }

        public async Task<IdentityInfo?> GetDefaultIdentity()
        {
            var result = await _doPost<Common.Result<IdentityInfo, object>, string>("local/identity/Get", "byDefault");
            return result.Ok;
        }

        public async Task<byte[]> SignBy(string nodeId, byte[] message)
        {
            int[] payload = message.Select(x => (int)x).ToArray();

            var result = await _doPost<Common.Result<int[], object>, SignRequest>("local/identity/Sign", new SignRequest()
            {
                NodeId = nodeId,
                Payload = payload
            });
            return result.Ok.Select(x => (byte)x).ToArray();
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
            try
            {
                return JsonSerializer.Deserialize<TOut>(outputData, options: options) ?? throw new HttpRequestException($"Invalid output: {outputData}");
            }
            catch (JsonException e)
            {
                throw new GsbServiceException($"Unable to parse output for {serviceUri}", System.Text.Encoding.UTF8.GetString(outputData), json, e);
            }
        }

    }
}
