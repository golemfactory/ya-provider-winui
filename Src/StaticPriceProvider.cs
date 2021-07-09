using GolemUI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Src
{
    public class StaticPriceProvider : IPriceProvider
    {
        public decimal glmToUsd(decimal glm)
        {
            return glm * 0.23m;
        }
    }
}
