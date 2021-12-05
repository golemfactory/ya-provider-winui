using Nethereum.Signer.EIP712;
using System.Collections.Generic;
using System.Numerics;

namespace GolemUI.Src.EIP712
{
    public static class EIP712TransactionSignerTypedData
    {


        public static TypedData Get(string contractAddress, string fromAddress, BigInteger nonce, byte[] functionSignature, byte[] salt)
        {

            return new TypedData
            {
                Domain = new Domain
                {
                    Name = "Golem Network Token (PoS)",
                    Version = "1",
                    //ChainId = 80001,

                    VerifyingContract = contractAddress,
                    Salt = salt,
                },
                Types = new Dictionary<string, MemberDescription[]>
                {
                    ["EIP712Domain"] = new[]
{
                        new MemberDescription {Name = "name", Type = "string"},
                        new MemberDescription {Name = "version", Type = "string"},
                        new MemberDescription {Name = "verifyingContract", Type = "address"},
                        new MemberDescription {Name = "salt", Type = "bytes32"}, // network id

                    },
                    ["MetaTransaction"] = new[]
{
                        new MemberDescription {Name = "nonce", Type = "uint256"},
                        new MemberDescription {Name = "from", Type = "address"},
                          new MemberDescription {Name = "functionSignature", Type = "bytes"},
                    },

                },
                PrimaryType = "MetaTransaction",
                Message = new[]
               {
                    new MemberValue {TypeName = "uint256", Value = nonce},
                    new MemberValue {TypeName = "address", Value = fromAddress},
                    new MemberValue {TypeName = "bytes", Value = functionSignature},
                }
            };
        }
    }
}
