using GolemUI.Command;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.JsonRpc.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Src.EIP712
{
    public class GasslessForwarderConfig
    {
        public String RpcUrl;
        public Network NetworkName;
        public String ForwarderUrl;

        public GasslessForwarderConfig(string rpcUrl, Network networkName, String forwarderUrl)
        {
            RpcUrl = rpcUrl;
            NetworkName = networkName;
            ForwarderUrl = forwarderUrl;
        }
    }

    public class Eip712Request
    {
        public byte[]? Message;
        public byte[]? FunctionCallEncodedInAbi;
        public byte[]? SignedMessage;

        public string? R;
        public string? S;
        public string? V;

        public string? SenderAddress;
    }

    public class GasslessForwarderService
    {
        GasslessForwarderConfig _config;
        public GasslessForwarderService(GasslessForwarderConfig config)
        {
            _config = config;
        }
        public async Task<Eip712Request> GetEip712EncodedTransferRequest(string networkName, string fromAddress, string recipentAddress, BigInteger amount)
        {
            var contractAddress = GolemContractAddress.Get(networkName);
            GolemContract contract = new GolemContract(_config.RpcUrl, contractAddress);
            BigInteger nonce = await contract.GetNonce(fromAddress);
            var functionEncodedInAbi = contract.GetTransferFunctionAbi(recipentAddress, amount);
            var payload = EIP712MetaTransactionPayload.GenerateForTrasfer(networkName, contractAddress, fromAddress, nonce, functionEncodedInAbi);

            Eip712Request request = new Eip712Request();
            request.FunctionCallEncodedInAbi = functionEncodedInAbi;
            request.Message = payload;

            return request;

        }

        public async Task<string> SendRequest(Eip712Request request)
        {
            HttpClient httpClient = new HttpClient();
            var payload = new { r = request.R, v = request.V, s = request.S, sender = request.SenderAddress, signedRequest = "0x" + request.SignedMessage.ToHex(), abiFunctionCall = "0x" + request.FunctionCallEncodedInAbi.ToHex() };
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            try
            {
                var result = await httpClient.PostAsync(_config.ForwarderUrl, content);
                var resultBody = await result.Content.ReadAsStringAsync();
                Console.WriteLine("SERVER : " + result.StatusCode + " : " + resultBody);
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    try
                    {
                        dynamic data = JsonConvert.DeserializeObject<dynamic>(resultBody);

                        if (data["txhash"] != null)
                        {
                            return data.txhash.ToString();
                        }
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            catch
            {
                return null;
            }
            return null;
        }


    }
}
