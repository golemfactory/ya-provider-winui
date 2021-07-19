using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.DesignViewModel
{
    public class SetupViewModel
    {
        public bool IsDesingMode { get; }

        public int Flow { get; set; }

        public int NoobStep { get; set; }

        public SetupViewModel()
        {
            IsDesingMode = true;
            Flow = 0;
            NoobStep = 0;
        }
    }
}
