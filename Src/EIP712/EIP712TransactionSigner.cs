using Nethereum.Web3;
using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Src.EIP712
{
    public class EthereumInteractions
    {
        public string ContractAddress = "0x2036807b0b3aaf5b1858ee822d0e111fddac7018";
        public string fromAdderss = "?";
        public string RecpientAddress = "?";
        public string NetworkRpc = "https://rpc-mumbai.maticvigil.com/";
        public string PrivateKey = "?";
        public GolemContract GolemContract;
        public EthereumInteractions()
        {
            GolemContract = new GolemContract(NetworkRpc, ContractAddress);
        }
        public async void SignTransaction()
        {
            BigInteger nonce = await GolemContract.GetNonce(fromAdderss);
            string sign = EIP712MetaTransactionPayload.GenerateForTrasfer("polygon", ContractAddress, fromAdderss, nonce, GolemContract.GetTransferFunctionAbi(RecpientAddress, 1), PrivateKey);
            Console.WriteLine("sign: " + sign);
        }
    }
}
