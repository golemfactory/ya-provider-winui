using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GolemUI.Interfaces;

namespace GolemUI.Miners
{
    class ClaymoreMiner : IMinerApp
    {
        private MinerAppName _minerAppName;
        public MinerAppName MinerAppName => _minerAppName;
        public ClaymoreMiner()
        {
            _minerAppName = new MinerAppName(MinerAppName.MinerAppEnum.Claymore);
        }


        public string WorkingDir => @"plugins\claymore";

        public string ExePath => @"plugins\claymore\EthDcrMiner64.exe";

        public string PreBenchmarkParams => "";

        public string BenchmarkParams => "";
    }
}
