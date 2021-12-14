using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Miners
{
    public class MinerAppName
    {
        public enum MinerAppEnum
        {
            Phoenix,
            TRex
        }

        private MinerAppEnum _minerAppEnum;

        public MinerAppName(MinerAppEnum minerAppEnum)
        {
            _minerAppEnum = minerAppEnum;
        }

        public MinerAppEnum NameEnum => _minerAppEnum;
        public string NameString => MinerAppEnumToString(_minerAppEnum);

        public static string MinerAppEnumToString(MinerAppEnum minerAppEnum)
        {
            switch (minerAppEnum)
            {
                case MinerAppEnum.Phoenix:
                    return "Phoenix";
                case MinerAppEnum.TRex:
                    return "TRex";
                default:
                    throw new Exception("Unknown miner");
            }
        }

        public static MinerAppEnum MinerAppStringToEnum(string minerAppName)
        {
            switch (minerAppName)
            {
                case "Phoenix":
                    return MinerAppEnum.Phoenix;
                case "TRex":
                    return MinerAppEnum.TRex;
                default:
                    throw new Exception("Uknown miner type: " + minerAppName);
            }
        }

    }




}
