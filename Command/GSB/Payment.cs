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

                public DateTime? Since { get; set; }

                public GetStatus(string address, string driver, string? network = null, string? token = null, DateTime? since = null)
                {
                    Address = address ?? throw new ArgumentNullException(nameof(address));
                    Driver = driver ?? throw new ArgumentNullException(nameof(driver));
                    Network = network;
                    Token = token;
                    Since = since;
                }
            }

            public class StatusResult
            {
                public decimal Amount { get; set; }

                public decimal Reserved { get; set; }

                public StatusNotes Outgoing { get; set; }
                public StatusNotes Incoming { get; set; }
                public string Driver { get; set; }
                public string Network { get; set; }

                public string Token { get; set; }

                public StatusResult(decimal amount, decimal reserved, StatusNotes outgoing, StatusNotes incoming, string driver, string network, string token)
                {
                    Amount = amount;
                    Reserved = reserved;
                    Outgoing = outgoing;
                    Incoming = incoming;
                    Driver = driver;
                    Network = network;
                    Token = token;
                }
            }

            public class StatusNotes
            {
                public StatValue Requested { get; set; }
                public StatValue Accepted { get; set; }
                public StatValue Confirmed { get; set; }

                public StatValue? Overdue { get; set; }

                public StatusNotes(StatValue requested, StatValue accepted, StatValue confirmed, StatValue? overdue = null)
                {
                    Requested = requested;
                    Accepted = accepted;
                    Confirmed = confirmed;
                    Overdue = overdue;
                }
            }


            // `/local/driver/zksync/ExitFee
            public class ExitFee
            {
                public string Sender { get; set; }

                public decimal? Amount { get; set; }

                public string Network { get; set; }

                public ExitFee(string sender, string network, decimal? amount = null)
                {
                    Sender = sender;
                    Network = network;
                    Amount = amount;
                }
            }

            public class ExitFeeResult
            {
                public decimal Amount { get; set; }

                public string Token { get; set; }

                public ExitFeeResult(decimal amount, string token)
                {
                    Amount = amount;
                    Token = token;
                }
            }


            public class Exit
            {

                public string Sender { get; set; }

                public string? To { get; set; }

                public decimal? Amount { get; set; }

                public string Network { get; set; }

                public decimal? FeeLimit { get; set; }

                public Exit(string sender, string? to, decimal? amount, string network, decimal? feeLimit)
                {
                    Sender = sender ?? throw new ArgumentNullException(nameof(sender));
                    To = to;
                    Amount = amount;
                    Network = network ?? throw new ArgumentNullException(nameof(network));
                    FeeLimit = feeLimit;
                }
            }

            public class Transfer
            {
                public string Sender { get; set; }

                public string? To { get; set; }

                [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
                public decimal? Amount { get; set; }

                public string Network { get; set; }

                [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
                public decimal? FeeLimit { get; set; }

                public Transfer(string sender, string? to, decimal? amount, string network, decimal? feeLimit)
                {
                    Sender = sender ?? throw new ArgumentNullException(nameof(sender));
                    To = to;
                    Amount = amount;
                    Network = network ?? throw new ArgumentNullException(nameof(network));
                    FeeLimit = feeLimit;
                }

            }
        }




        public Payment(IGsbEndpointFactory gsbEndpointFactory)
        {
            _gsbEndpointFactory = gsbEndpointFactory;
        }

        public async Task<Model.StatusResult> GetStatus(string address, string driver, string? network = null, string? token = null, DateTime? since = null)
        {
            var result = await _doPost<Common.Result<Model.StatusResult, object>, Model.GetStatus>("local/payment/GetStatus", new Model.GetStatus(address, driver, network, token, since));
            return result.Ok ?? throw new HttpRequestException($"Invalid output: {result.Err}");
        }

        public async Task<Model.ExitFeeResult> ExitFee(string address, string driver, string network, decimal? amount = null)
        {
            var result = await _doPost<Common.Result<Model.ExitFeeResult, object>, Model.ExitFee>($"local/driver/{driver}/ExitFee", new Model.ExitFee(address, network, amount));
            return result.Ok ?? throw new HttpRequestException($"Invalid output: {result.Err}");
        }

        public async Task<string> Exit(string driver, string from, string network, string? to, decimal? amount = null, decimal? feeLimit = null)
        {
            var result = await _doPost<Common.Result<string, object>, Model.Exit>($"local/driver/{driver}/Exit", new Model.Exit(from, to, amount, network, feeLimit));
            var cap = System.Text.RegularExpressions.Regex.Match(result.Ok ?? "", @"https:[^\s]*$").Captures;
            if (cap.Count == 1)
            {
                return cap[0].Value;
            }
            return result.Ok ?? throw new HttpRequestException($"Invalid output: {result.Err}");
        }

        public async Task<string> TransferTo(string driver, string from, string network, string? to, decimal? amount = null, decimal? feeLimit = null)
        {
            var result = await _doPost<Common.Result<string, object>, Model.Transfer>($"local/driver/{driver}/Transfer", new Model.Transfer(from, to, amount, network, feeLimit: null));
            var cap = System.Text.RegularExpressions.Regex.Match(result.Ok ?? "", @"https:[^\s]*$").Captures;
            if (cap.Count == 1)
            {
                return cap[0].Value;
            }
            try
            {
                return result.Ok ?? throw new GsbServiceException("Invalid output", output: result.Err?.ToString() ?? "Empty result error");
            }
            catch (GsbServiceException e)
            {
                if (e.Output.Contains("Not enough ETH balance for gas"))
                {
                    throw new GsbServiceException("Not enough balance for gas", e.Output, e.Input, null);
                }
                else
                {
                    throw e;
                }
            }
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
