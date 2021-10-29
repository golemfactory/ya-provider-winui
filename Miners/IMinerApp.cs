using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Miners
{
    public interface IMinerApp
    {
        public MinerAppName MinerAppName { get; }

        public IMinerParser MinerParserBenchmark { get; }
        public IMinerParser MinerParserPreBenchmark { get; }

        public string WorkingDir { get; }
        public string ExePath { get; }
        public string PreBenchmarkParams { get; }
        public string GetBenchmarkParams(string pool, string ethereumAddress, string nodeName);
    }
}
