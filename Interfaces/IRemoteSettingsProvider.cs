using BetaMiner.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetaMiner.Interfaces
{
    public interface IRemoteSettingsProvider
    {
        public delegate void RemoteSettingsUpdatedEventHandler(RemoteSettings remoteSettings);

        public RemoteSettingsUpdatedEventHandler? OnRemoteSettingsUpdated { get; set; }

        public bool LoadRemoteSettings(out RemoteSettings remoteSettings);
    }
}
