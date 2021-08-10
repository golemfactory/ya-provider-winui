using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GolemUI.Model;

namespace GolemUI.Interfaces
{
    public interface IBenchmarkResultsProvider
    {
        public BenchmarkResults LoadBenchmarkResults();

        public void SaveBenchmarkResults(BenchmarkResults userSettings);
    }
}
