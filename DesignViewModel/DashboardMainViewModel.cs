using GolemUI.Src;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.DesignViewModel
{
    public class DashboardMainViewModel
    {
        public string StartButtonExplanation => "Please wait until all subsystems are initialized";
        public string SstartButtonExplanation => "At least one GPU card with mining capability must be enabled by user " +
                           "(Settings). You can rerun benchmark to determine gpu capabilities again.";
        public string GpuStatus => "Ready";
        public string? GpuStatusAnnotation => "20.15 MH/s";
        public string? PaymentStateMessage => "Unable to get account's balance";
        public string PolygonLink => "https://polygonscan.com/token/0x0b220b82f3ea3b7f6d9a1d8ab58930c064a2b5bf?a=0x00000000000000000000000000000123";
        public bool ShouldPaymentMessageTooltipBeAccessible => true;
        public string CpuStatus => "Ready";
        public decimal AmountUSD => 0.00m;
        public decimal Amount => 0;
        public double GpuOpacity => IsAnyGpuEnabled ? 1.0 : 0.2f;
        public double CpuOpacity => IsCpuMiningEnabledByNetwork ? 1.0 : 0.2f;
        public bool IsCpuMiningEnabledByNetwork => false;
        public bool IsAnyGpuEnabled => false;
        public decimal PendingAmountUSD => 0;
        public decimal PendingAmount => 0;
        public decimal UsdPerDay => 41.32m;
        public decimal GlmPerDay => 41.32m / 2;
        public string GpuCardsInfo => "2/2";
        public string CpuCardsInfo => "3/7";
        public bool IsProviderRunning { get; } = true;
        public DashboardStatusEnum Status => DashboardStatusEnum.Ready;
        public string StatusAdditionalInfo => "4 GB mode";
        public bool ShouldGpuAnimationBeVisible => true;
    }
}
