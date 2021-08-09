using GolemUI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Interfaces
{
    public interface IRemoteSettingsProvider
    {
        public Task<bool> RequestRemoteSettingsUpdate();

        public bool LoadRemoteSettings(out RemoteSettings remoteSettings);
    }
}
