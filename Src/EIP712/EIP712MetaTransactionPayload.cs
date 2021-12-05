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
        public static string GenerateForTrasfer(string networkName, string contractAddress, string fromAddress, BigInteger nonce, string functionAbi, string privateKey)
        {
            var PrivateKey = new EthECKey(privateKey); // remove after tests


            byte[] salt = networkName switch
            {
                "polygon-mumbai" => Eip712TransactionSignerSaltValue.PolygonMumbai,
                "polygon" => Eip712TransactionSignerSaltValue.PolygonMainnet,
                _ => Eip712TransactionSignerSaltValue.PolygonMainnet,
            };

            var typedData = EIP712TransactionSignerTypedData.Get(contractAddress, fromAddress, nonce, functionAbi, salt);


            Console.WriteLine("==" + Eip712TypedDataSigner.Current.EncodeTypedData(typedData).ToHex());
            var hashedData = Sha3Keccack.Current.CalculateHash(Eip712TypedDataSigner.Current.EncodeTypedData(typedData));
            var correctSignature = PrivateKey.SignAndCalculateV(hashedData);
            string ret = PrivateKey.Sign(hashedData).To64ByteArray().ToHex();
            Console.WriteLine("eth sign: " + EthECDSASignature.CreateStringSignature(correctSignature));
            var sig = Eip712TypedDataSigner.Current.SignTypedData(typedData, new EthECKey(privateKey));
            Console.WriteLine("eth sign 2: " + sig);
            Console.WriteLine("R: " + correctSignature.R.ToHex());
            Console.WriteLine("S: " + correctSignature.S.ToHex());
            Console.WriteLine("V: " + correctSignature.V.ToHex());

            return ret;
        }
    }
}
