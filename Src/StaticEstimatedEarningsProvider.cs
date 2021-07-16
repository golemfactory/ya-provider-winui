using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GolemUI.Interfaces;

namespace GolemUI.Src
{
    class StaticEstimatedEarningsProvider : IEstimatedProfitProvider
    {
        public double EthHashRateToDailyEarnigns(double hashrate, MiningType miningType, MiningEarningsPeriod miningEarningsPeriod)
        {
            double earnings = 0;
            if (miningType == MiningType.MiningTypeEth)
            {
                earnings = 0.05 * hashrate;
                if (miningEarningsPeriod == MiningEarningsPeriod.MiningEarningsPeriodWeek)
                {
                    return earnings * 7;
                }
                if (miningEarningsPeriod == MiningEarningsPeriod.MiningEarningsPeriodMonth)
                {
                    return earnings * (365.0 / 12.0);
                }
            }

            throw new NotImplementedException();
        }
    }
}
