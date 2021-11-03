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

        private IBenchmarkResultsProvider _benchmarkResultsProvider;

        private ILogger _logger;
        public TRexMiner(ILogger<TRexMiner> logger, IBenchmarkResultsProvider benchmarkResultsProvider)
        {
            _logger = logger;
            _minerAppName = new MinerAppName(MinerAppName.MinerAppEnum.TRex);
            _benchmarkResultsProvider = benchmarkResultsProvider;
        }

        public string WorkingDir => @"plugins\t-rex";

        public string ExePath => @"plugins\t-rex\t-rex.exe";

        public string PreBenchmarkParams => "--algo ethash --benchmark --benchmark-epoch 450";

        public IMinerParser MinerParser => throw new NotImplementedException();

        public string GetBenchmarkParams(MinerAppConfiguration minerAppConfiguration)
        {
            string extraParams = "";
            if (!string.IsNullOrEmpty(minerAppConfiguration.Cards))
            {
                extraParams += " --devices " + minerAppConfiguration.Cards;
            }
            if (!string.IsNullOrEmpty(minerAppConfiguration.Niceness))
            {
                extraParams += " --intensity " + minerAppConfiguration.Niceness;
            }

            return $"--no-watchdog --algo ethash -o {minerAppConfiguration.Pool} -u {minerAppConfiguration.EthereumAddress} -p x -w \"benchmark:0x0/{minerAppConfiguration.NodeName}:{minerAppConfiguration.EthereumAddress}/0\"" + extraParams;
        }

        public IMinerParser CreateParserForBenchmark()
        {
            return new TRexParser(false, 5, _logger);
        }

        public IMinerParser CreateParserForPreBenchmark()
        {
            return new TRexParser(true, 5, _logger);
        }

        public string? GetExtraMiningParams()
        {
            // --intensity

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
                args.Add("--devices");
                args.Add(String.Join(",", gpus.Where(gpu => gpu.IsEnabledByUser).Select(gpu => gpu.GpuNo - 1)));
            }
            args.Add("--intensity");
            args.Add(String.Join(",", gpus.Where(gpu => gpu.IsEnabledByUser).Select(gpu => gpu.ClaymorePerformanceThrottling)));

            return String.Join(" ", args);
        }
    }
}
