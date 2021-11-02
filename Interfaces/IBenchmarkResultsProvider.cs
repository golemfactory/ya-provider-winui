using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GolemUI.Miners;
using GolemUI.Model;

namespace GolemUI.Interfaces
{
    public interface IBenchmarkResultsProvider
    {
        public BenchmarkResults LoadBenchmarkResults(MinerAppName minerAppName);

        public void SaveBenchmarkResults(BenchmarkResults benchmarkResults, MinerAppName minerAppName);
    }
}
