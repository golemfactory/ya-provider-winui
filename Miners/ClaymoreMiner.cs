using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GolemUI.Interfaces;
using Microsoft.Extensions.Logging;

namespace GolemUI.Miners
{
    public class ClaymoreMiner : IMinerApp
    {
        private MinerAppName _minerAppName;
        public MinerAppName MinerAppName => _minerAppName;

        private ClaymoreParser _claymoreParserBenchmark;
        private ClaymoreParser _claymoreParserPreBenchmark;
        public IMinerParser MinerParserBenchmark => _claymoreParserBenchmark;
        public IMinerParser MinerParserPreBenchmark => _claymoreParserPreBenchmark;

        private ILogger? _logger;

        public ClaymoreMiner(ILogger<ClaymoreMiner> logger)
        {
            _logger = logger;
            _minerAppName = new MinerAppName(MinerAppName.MinerAppEnum.Claymore);
            _claymoreParserBenchmark = new ClaymoreParser(false, 5, _logger);
            _claymoreParserPreBenchmark = new ClaymoreParser(true, 5, _logger);
        }


        public string WorkingDir => @"plugins\claymore";

        public string ExePath => @"plugins\claymore\EthDcrMiner64.exe";

        public string PreBenchmarkParams => "";

        public string GetBenchmarkParams(string pool, string ethereumAddress, string nodeName)
        {
            return $"-wd 0 -r -1 -epool {pool} -ewal {ethereumAddress} -eworker \"benchmark:0x0/{nodeName}:{ethereumAddress}/0\" -clnew 1 -clKernel 0";
        }


    }
}
