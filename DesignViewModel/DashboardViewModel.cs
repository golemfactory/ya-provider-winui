using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.DesignViewModel
{
    public class DashboardViewModel
    {
        public int EnabledGpuCount => 2;
        public int TotalGpuCount => 3;
        public int EnabledCpuCount => 4;
        public int TotalCpuCount => 5;
        public bool IsProviderRunning { get; } = true;
        public DashboardStatusEnum Status => DashboardStatusEnum.Ready;
    }
}
