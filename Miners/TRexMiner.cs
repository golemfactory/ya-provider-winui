using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GolemUI.Interfaces;

namespace GolemUI.Miners
{
    class TRexMiner : IMinerApp
    {
        private MinerAppName _minerAppName;
        public MinerAppName MinerAppName => _minerAppName;
        public TRexMiner()
        {
            _minerAppName = new MinerAppName(MinerAppName.MinerAppEnum.TRex);
        }

        public string WorkingDir => @"plugins\t-rex";

        public string ExePath => @"plugins\t-rex\t-rex.exe";

        public string PreBenchmarkParams => "";

        public string BenchmarkParams => "";
    }
}
