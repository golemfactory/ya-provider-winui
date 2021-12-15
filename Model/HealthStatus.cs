using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Model
{
    public class HealthStatus
    {
        public bool? isNetConnected { get; set; }
        public int? lastConnectedTime { get; set; }
        public int? lastDisconnnectedTime { get; set; }
        public int? lastHealthCheckWorker { get; set; }
        public object? metrics { get; set; }
        //public 
    }

    public class HealthStatusResponse
    {
        public string? error { get; set; }
        public bool? success { get; set; }
        public HealthStatus? value { get; set; }
    }
}
