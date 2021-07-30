using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Model
{
    public class UserSettings
    {
        public int SettingsVersion { get; set; }
        public string? BenchmarkLength { get; set; }
        public string? CustomPool { get; set; }
        public string? OptionalEmail { get; set; }

        public bool EnableDetailedBenchmarkInfo { get; set; }
        public bool EnableDebugLogs { get; set; }
        public bool StartYagnaCommandLine { get; set; }
        public bool StartProviderCommandLine { get; set; }
        public bool DisableNotificationsWhenMinimized { get; set; }
        public bool MinimizeToTrayOnMinimize { get; set; }
        public bool CloseOnExit { get; set; }

        public bool StartWithWindows { get; set; }
        public bool EnableWASMUnit { get; set; }
    }
}
