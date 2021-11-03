using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GolemUI.Interfaces;
using Microsoft.Extensions.Logging;

namespace GolemUI.Miners.TRex
{
    public class TRexMiner : IMinerApp
    {
        private MinerAppName _minerAppName;
        public MinerAppName MinerAppName => _minerAppName;
        

        private ILogger _logger;
        public TRexMiner(ILogger<TRexMiner> logger)
        {
            _logger = logger;
            _minerAppName = new MinerAppName(MinerAppName.MinerAppEnum.TRex);
        }

        public string WorkingDir => @"plugins\t-rex";

        public string ExePath => @"plugins\t-rex\t-rex.exe";

        public string PreBenchmarkParams => "--algo ethash --benchmark --benchmark-epoch 450";

        public IMinerParser MinerParser => throw new NotImplementedException();

        public string GetBenchmarkParams(MinerAppConfiguration minerAppConfiguration)
        {
            return $"--no-watchdog --algo ethash -o {minerAppConfiguration.Pool} -u {minerAppConfiguration.EthereumAddress} -p x -w \"benchmark:0x0/{minerAppConfiguration.NodeName}:{minerAppConfiguration.EthereumAddress}/0\"";
        }

        public IMinerParser CreateParserForBenchmark()
        {
            return new TRexParser(false, 5, _logger);
        }

        public IMinerParser CreateParserForPreBenchmark()
        {
            return new TRexParser(true, 5, _logger);
        }

        public string GetExtraMiningParams()
        {
            throw new NotImplementedException();
        }
    }
}
