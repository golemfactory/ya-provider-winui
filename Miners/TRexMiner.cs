using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GolemUI.Interfaces;
using Microsoft.Extensions.Logging;

namespace GolemUI.Miners
{
    public class TRexMiner : IMinerApp
    {
        private MinerAppName _minerAppName;
        public MinerAppName MinerAppName => _minerAppName;
        private TRexParser _trexParser;
        private ILogger _logger;
        public TRexMiner(ILogger logger)
        {
            _logger = logger;
            _minerAppName = new MinerAppName(MinerAppName.MinerAppEnum.TRex);

            _trexParser = new TRexParser(true, 5, _logger);
        }

        public string WorkingDir => @"plugins\t-rex";

        public string ExePath => @"plugins\t-rex\t-rex.exe";

        public string PreBenchmarkParams => "--algo ethash --benchmark --benchmark-epoch 450";

        public IMinerParser MinerParser => throw new NotImplementedException();

        public string GetBenchmarkParams(string pool, string ethereumAddress, string nodeName)
        {
            return $"--no-watchdog --algo ethash -o {pool} -u {ethereumAddress} -p x -w \"benchmark:0x0/{nodeName}:{ethereumAddress}/0\"";
        }
    }
}
