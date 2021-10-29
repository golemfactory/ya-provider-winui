using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GolemUI.Miners.Claymore;
using Microsoft.Extensions.Logging;

namespace GolemUI.Miners.Phoenix
{
    public class PhoenixMiner : IMinerApp
    {
        private MinerAppName _minerAppName;
        public MinerAppName MinerAppName => _minerAppName;

        private ClaymoreParser _phoenixParserBenchmark;
        private ClaymoreParser _phoenixParserPreBenchmark;
        public IMinerParser MinerParserBenchmark => _phoenixParserBenchmark;
        public IMinerParser MinerParserPreBenchmark => _phoenixParserPreBenchmark;

        private ILogger _logger;
        public PhoenixMiner(ILogger<PhoenixMiner> logger)
        {
            _logger = logger;
            _minerAppName = new MinerAppName(MinerAppName.MinerAppEnum.Phoenix);
            _phoenixParserBenchmark = new ClaymoreParser(false, 5, _logger);
            _phoenixParserPreBenchmark = new ClaymoreParser(true, 5, _logger);
        }

        public string WorkingDir => @"plugins\claymore";

        public string ExePath => @"plugins\claymore\EthDcrMiner64.exe";

        public string PreBenchmarkParams => "-epool test -li 200";

        public string GetBenchmarkParams(string pool, string ethereumAddress, string nodeName)
        {
            return $"-wd 0 -r -1 -epool {pool} -ewal {ethereumAddress} -eworker \"benchmark:0x0/{nodeName}:{ethereumAddress}/0\" -clnew 1 -clKernel 0";
        }
    }
}
