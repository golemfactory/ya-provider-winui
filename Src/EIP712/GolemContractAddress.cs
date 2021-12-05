using System;

namespace GolemUI.Src.EIP712
{
    public static class GolemContractAddress
    {
        public static string PolygonMainnet = "0xe141e7b83A0AE8da88F89C3b8C2ac7A06439E0Da";
        public static string PolygonMumbai = "0x2036807B0B3aaf5b1858EE822D0e111fDdac7018";
        public static string Get(string networkName)
        {
            switch (networkName)
            {
                case "polygon":
                    return PolygonMainnet;
                case "mumbai":
                    return PolygonMumbai;
                default:
                    throw new ArgumentException("unknown network name: " + networkName);
            }
        }
    }
}
