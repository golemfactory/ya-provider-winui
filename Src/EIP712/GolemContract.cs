using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.ABI;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Nethereum.Hex.HexConvertors.Extensions;
using System.Reflection.Metadata.Ecma335;

namespace GolemUI.Src.EIP712
{



    public class GolemContract
    {
        [Function("getNonce", "uint256")]
        public class GetNonceFunction : FunctionMessage
        {
            [Parameter("address", "user", 1)] public string? User { get; set; }
        }

        [Function("balanceOf", "uint256")]
        public class BalanceOfFunction : FunctionMessage
        {
            [Parameter("address", "_owner", 1)] public string? Owner { get; set; }
        }


        [Function("transfer", "bool")]
        public class TransferFunction : FunctionMessage
        {
            [Parameter("address", "_to", 1)]
            public string? To { get; set; }

            [Parameter("uint256", "_value", 2)]
            public BigInteger? TokenAmount { get; set; }
        }


        readonly Web3? Web3 = null;
        readonly string? ContractAddress;
        readonly string? Key;
        readonly Nethereum.Web3.Accounts.Account? Account;
        public GolemContract(string rpc, string contractAddress, string privateKey)
        {
            if (privateKey == null)
                Web3 = new Nethereum.Web3.Web3(rpc);
            else
            {
                Key = privateKey;
                Account = new Nethereum.Web3.Accounts.Account(privateKey, 80001);
                Web3 = new Web3(Account, rpc);
            }
            ContractAddress = contractAddress;
        }
        public GolemContract(string rpc, string contractAddress)
        {
            Web3 = new Nethereum.Web3.Web3(rpc);
            ContractAddress = contractAddress;
        }


        public async Task<BigInteger> GetNonce(string address)
        {


            var getNonceMessage = new GetNonceFunction() { User = address };

            //Creating a new query handler
            var queryHandler = Web3.Eth.GetContractQueryHandler<GetNonceFunction>();

            var currentNonce = await queryHandler
                .QueryAsync<BigInteger>(this.ContractAddress, getNonceMessage)
                .ConfigureAwait(false);
            //Console.WriteLine("current nonce: " + currentNonce.ToString());

            /* if (this.Web3 == null) throw new NullReferenceException("GolemContract.Web3 hasn't been properly initialized");
             var currentNonce2 = await this.Web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(address, BlockParameter.CreatePending());
            */
            return currentNonce;
        }
        public byte[] GetTransferFunctionAbi(string receiverAddress, BigInteger amount)
        {
            var abiEncode = new ABIEncode();
            var transfer = new TransferFunction()
            {
                To = receiverAddress,
                TokenAmount = amount
            };

            string functionHeader = Sha3Keccack.Current.CalculateHash("transfer(address,uint256)").Substring(0, 4 * 2); // explanation how hash is calculated: https://piyopiyo.medium.com/how-to-get-ethereum-encoded-function-signatures-1449e171c840
            var functionParams = abiEncode.GetABIParamsEncoded(transfer).ToHex();
            return StringConverters.ConvertHexStringToByteArray(functionHeader + functionParams);
        }

        /* public async Task<string> TransferTo(string receiverAddress, BigInteger amount)
         {

             var nonce = this.GetNonce(Account.Address);
             var futureNonce = await Account.NonceService.GetNextNonceAsync();


             BigInteger txCount = nonce.Result;


             var transferHandler = this.Web3Elevated.Eth.GetContractTransactionHandler<TransferFunction>();
             var transfer = new TransferFunction()
             {
                 To = receiverAddress,
                 TokenAmount = amount
             };



             transfer.Nonce = txCount;

             transfer.Gas = 800 * 1000;

             transfer.GasPrice = Nethereum.Web3.Web3.Convert.ToWei(10, UnitConversion.EthUnit.Gwei);
             var signedTransaction2 = await transferHandler.SignTransactionAsync(this.ContractAddress, transfer);

             Console.WriteLine(
          "Signed transaction Fully offline (no need for node, providing manually the nonce, gas and gas price) is: " +
          signedTransaction2);

             var transactionIdPredicted = Sha3Keccack.Current.CalculateHashFromHex(signedTransaction2);
             var transactionIdActual =
                 await this.Web3Elevated.Eth.Transactions.SendRawTransaction.SendRequestAsync("0x" + signedTransaction2);

             Console.WriteLine($"Predicted transaction hash: {transactionIdPredicted}");
             Console.WriteLine($"Actual transaction hash: {transactionIdActual}");

             return transactionIdActual;
             //return await transferHandler.SendRequestAndWaitForReceiptAsync(this.ContractAddress, transfer);
         }*/

        /*

        public async Task<BigInteger> GetBalance(string address)
        {
            var balanceOfMessage = new BalanceOfFunction() { Owner = address };

            //Creating a new query handler
            var queryHandler = Web3.Eth.GetContractQueryHandler<BalanceOfFunction>();

            var balance = await queryHandler
                .QueryAsync<BigInteger>(this.ContractAddress, balanceOfMessage)
                .ConfigureAwait(false);
            Console.WriteLine(balance.ToString());

            return balance;
        }*/
    }
}
