using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Utils
{
    class MathUtils
    {
        public static int RoundToInt(double d)
        {
            if (d < 0)
            {
                return (int)(d - 0.5);
            }
            return (int)(d + 0.5);
        }
        public static int? RoundToInt(double? d)
        {
            if (d == null) return null;
            return RoundToInt(d.Value);
        }

    }
}
