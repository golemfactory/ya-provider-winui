using GolemUI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Interfaces
{
    // Estimates profit from hash rate.
    public interface IEstimatedProfitProvider
    {
        public double HashRateToUSDPerDay(double hashRate, Coin coin);
    }
}

