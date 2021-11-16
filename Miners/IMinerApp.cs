using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Miners
{
    public class MinerAppConfiguration
    {
        public string Pool = "";
        public string EthereumAddress = "";
        public string NodeName = "";
        public string Cards = "";
        public string Niceness = "";
        public string MiningMode = "ETH"; //eth or etc
    }

    public interface IMinerApp
    {
        public MinerAppName MinerAppName { get; }

        public IMinerParser CreateParserForBenchmark();
        public IMinerParser CreateParserForPreBenchmark();

        public string WorkingDir { get; }
        public string ExePath { get; }
        public string GetPreBenchmarkParams(MinerAppConfiguration minerAppConfiguration);
        public string GetBenchmarkParams(MinerAppConfiguration minerAppConfiguration);

        public string? GetExtraMiningParams(MinerAppConfiguration minerAppConfiguration);
    }


}
