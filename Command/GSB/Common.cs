using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Command.GSB
{
    public class Common
    {
        public class Result<TOk, TError>
        {
            public TOk? Ok { get; set; }

            public TError? Err { get; set; }
        }
    }
}
