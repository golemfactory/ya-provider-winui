using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BetaMiner.Model;

namespace BetaMiner.Interfaces
{
    public interface IBenchmarkResultsProvider
    {
        public BenchmarkResults LoadBenchmarkResults();

        public void SaveBenchmarkResults(BenchmarkResults userSettings);
    }
}
