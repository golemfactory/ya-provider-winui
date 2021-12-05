using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;
using Nethereum.Util;
using System;
using System.Numerics;

namespace GolemUI.Src.EIP712
{
    public static class EIP712MetaTransactionPayload
    {
        public static byte[] GenerateForTrasfer(string networkName, string contractAddress, string fromAddress, BigInteger nonce, byte[] functionAbi)
        {
            //var PrivateKey = new EthECKey(privateKey); // remove after tests


            byte[] salt = Eip712TransactionSignerSaltValue.Get(networkName);

            var typedData = EIP712TransactionSignerTypedData.Get(contractAddress, fromAddress, nonce, functionAbi, salt);

            var payload = Eip712TypedDataSigner.Current.EncodeTypedData(typedData);
            var keccakedPayload = Sha3Keccack.Current.CalculateHash(payload);

            /* Console.WriteLine("function abi: " + "0x" + functionAbi.ToHex())
             Console.WriteLine("==" + payload.ToHex());
             Console.WriteLine("==" + keccakedPayload.ToHex());
             var hashedData = Sha3Keccack.Current.CalculateHash(Eip712TypedDataSigner.Current.EncodeTypedData(typedData));
             var correctSignature = PrivateKey.SignAndCalculateV(hashedData);
             string ret = PrivateKey.Sign(hashedData).To64ByteArray().ToHex();
             Console.WriteLine("eth sign: " + EthECDSASignature.CreateStringSignature(correctSignature));
             var sig = Eip712TypedDataSigner.Current.SignTypedData(typedData, new EthECKey(privateKey));
             Console.WriteLine("eth sign 2: " + sig);
             Console.WriteLine("R: " + correctSignature.R.ToHex());
             Console.WriteLine("S: " + correctSignature.S.ToHex());
             Console.WriteLine("V: " + correctSignature.V.ToHex());*/


            return keccakedPayload;
        }
    }
}
