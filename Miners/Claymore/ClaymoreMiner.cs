using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GolemUI.Interfaces;
using Microsoft.Extensions.Logging;

namespace GolemUI.Miners.Claymore
{
    public class ClaymoreMiner : IMinerApp
    {
        private MinerAppName _minerAppName;
        public MinerAppName MinerAppName => _minerAppName;

        private ILogger? _logger;
        private IBenchmarkResultsProvider _benchmarkResultsProvider;

        public ClaymoreMiner(ILogger<ClaymoreMiner> logger, IBenchmarkResultsProvider benchmarkResultsProvider)
        {
            _logger = logger;
            _minerAppName = new MinerAppName(MinerAppName.MinerAppEnum.Claymore);
            _benchmarkResultsProvider = benchmarkResultsProvider;
        }


        public string WorkingDir => @"plugins\claymore";

        public string ExePath => @"plugins\claymore\EthDcrMiner64.exe";

        public string PreBenchmarkParams => "-epool test -li 200";

        public IMinerParser CreateParserForBenchmark()
        {
            return new ClaymoreParser(false, 5, _logger);
        }

        public IMinerParser CreateParserForPreBenchmark()
        {
            return new ClaymoreParser(true, 5, _logger);
        }

        public string GetBenchmarkParams(MinerAppConfiguration minerAppConfiguration)
        {
            string extraParams = "";
            if (!string.IsNullOrEmpty(minerAppConfiguration.Cards))
            {
                extraParams += " -gpus " + minerAppConfiguration.Cards;
            }
            if (!string.IsNullOrEmpty(minerAppConfiguration.Niceness))
            {
                extraParams += " -li " + minerAppConfiguration.Niceness;
            }

            return $"-wd 0 -r -1 -epool {minerAppConfiguration.Pool} -ewal {minerAppConfiguration.EthereumAddress} -eworker \"benchmark:0x0/{minerAppConfiguration.NodeName}:{minerAppConfiguration.EthereumAddress}/0\" -clnew 1 -clKernel 0" + extraParams;
        }


        public string? GetExtraMiningParams()
        {
            var status = _benchmarkResultsProvider.LoadBenchmarkResults(_minerAppName).liveStatus;
            if (status == null)
            {
                return null;
            }

            var gpus = status.GPUs.Values;

            if (gpus.Count == 0)
            {
                return null;
            }

            var args = new List<string>();
            if (gpus.Any(gpu => !gpu.IsEnabledByUser))
            {
                args.Add("-gpus");
                args.Add(String.Join(",", gpus.Where(gpu => gpu.IsEnabledByUser).Select(gpu => gpu.GpuNo)));
            }
            args.Add("-li");
            args.Add(String.Join(",", gpus.Where(gpu => gpu.IsEnabledByUser).Select(gpu => gpu.ClaymorePerformanceThrottling)));
            args.Add("-clnew");
            args.Add("1");
            args.Add("-clKernel");
            args.Add("0");
            args.Add("-wd");
            args.Add("0");
            return String.Join(" ", args);
        }
    }
}
