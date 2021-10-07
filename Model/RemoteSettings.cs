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
        public string? LatestVersion { get; set; }
        public string? LastSupportedVersion { get; set; }
        public string? LatestVersionCodename { get; set; }
        public string? LatestVersionDownloadLink { get; set; }
        public string? ChangeLog { get; set; }
        public double? DayEthPerGH { get; set; } = 0.02;
        public double? DayEtcPerGH { get; set; } = 0.9;
        public double? RequestorCoeff { get; set; } = 0.66;
        public double? GLMFallbackValue { get; set; } = 0.50;
    }
}
