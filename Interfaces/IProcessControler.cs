using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Interfaces
{
    public delegate void LogLineHandler(string logger, string line);

    public interface IProcessControler
    {

        public Task<bool> Init();

        public Task<Command.KeyInfo> Me();

        bool IsRunning { get; }


        LogLineHandler? LineHandler { get; set; }

    }
}
