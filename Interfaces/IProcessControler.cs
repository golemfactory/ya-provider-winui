using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Interfaces
{
    public interface IProcessControler
    {

        public Task<bool> Init();

        public Task<Command.KeyInfo> Me();

    }
}
