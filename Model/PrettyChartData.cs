using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Model
{

    public class PrettyChartData
    {
        public class PrettyChartBinEntry
        {
            public string? Label { get; set; } = null;
            public double Value { get; set; } = 0.0;
        }

        public class PrettyChartBinData
        {
            public List<PrettyChartBinEntry> BinEntries { get; set; } = new List<PrettyChartBinEntry>();

        }

        public PrettyChartBinData BinData { get; set; } = new PrettyChartBinData();

        public PrettyChartData()
        {
            BinData = new PrettyChartBinData();


        }

    }
}
