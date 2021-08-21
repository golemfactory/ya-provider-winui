﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetaMiner.DesignViewModel
{
    public class DashboardMainViewModel
    {
        public string StartButtonExplanation => "Please wait until all subsystems are initialized";
        public string GpuStatus => "Ready";
        public string? GpuStatusAnnotation => "20.15 MH/s";

        public string CpuStatus => "Ready";
        public decimal AmountUSD => 0.00m;
        public decimal Amount => 0;
        public decimal PendingAmountUSD => 0;
        public decimal PendingAmount => 0;
        public decimal UsdPerDay => 41.32m;
        public string GpuCardsInfo => "2/2";
        public string CpuCardsInfo => "3/7";
        public bool IsProviderRunning { get; } = true;
        public DashboardStatusEnum Status => DashboardStatusEnum.Ready;
    }
}
