using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Interfaces
{
    public interface IPriceProvider
    {
        decimal glmToUsd(decimal glm);
    }
}
