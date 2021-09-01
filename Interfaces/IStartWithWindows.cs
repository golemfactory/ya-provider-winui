using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Interfaces
{
    public interface IStartWithWindows
    {
        public bool IsStartWithSystemEnabled();
        public void SetStartWithSystemEnabled(bool enable);
    }
}
