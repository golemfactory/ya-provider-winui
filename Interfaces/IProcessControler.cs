using GolemUI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Interfaces
{
    public delegate void LogLineHandler(string logger, string line);

    public interface IProcessControler : INotifyPropertyChanged
    {

        public Task<bool> Init();

        public Task<Command.KeyInfo> Me();

        bool IsProviderRunning { get; }

        bool IsServerRunning { get; }


        LogLineHandler? LineHandler { get; set; }

    }
}
