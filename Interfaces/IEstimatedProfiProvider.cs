using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Interfaces
{
    public enum MiningType
    {
        MiningTypeEth,
        MiningTypeEtc
    }

    public enum MiningEarningsPeriod
    {
        MiningEarningsPeriodDay,
        MiningEarningsPeriodWeek,
        MiningEarningsPeriodMonth
    }


    public interface IEstimatedProfitProvider
    {
        double EthHashRateToDailyEarnigns(double hashrate, MiningType miningType, MiningEarningsPeriod miningEarningsPeriod);
    }
}
