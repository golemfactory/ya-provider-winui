namespace GolemUI.Src.EIP712
{
    public static class Eip712TransactionSignerSaltValue
    {
        public static byte[] PolygonMumbai = StringConverters.ConvertHexStringToByteArray("0x0000000000000000000000000000000000000000000000000000000000013881".Substring(2));
        public static byte[] PolygonMainnet = StringConverters.ConvertHexStringToByteArray("0x0000000000000000000000000000000000000000000000000000000000000089".Substring(2));


    }
}
