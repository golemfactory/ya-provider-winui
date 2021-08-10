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
        public double? DayEthPerGH { get; set; } = 0.02;
        public double? DayEtcPerGH { get; set; } = 0.9;
        public double? RequestorCoeff { get; set; } = 0.66;
    }
}
