using System;

namespace GolemUI.Src.EIP712
{
    public static class GolemContractAddress
    {
        public static string PolygonMainnet = "0x0b220b82f3ea3b7f6d9a1d8ab58930c064a2b5bf";
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
