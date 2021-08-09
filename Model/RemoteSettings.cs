using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Model
{
    public class RemoteSettings
    {
        public DateTime? DownloadedDateTime { get; set; }
        public string? Version { get; set; }
        public double? EthPricePerMhs { get; set; }
        public double? EtcPricePerMhs { get; set; }
    }
}
