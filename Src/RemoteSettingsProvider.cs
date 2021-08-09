using GolemUI.Interfaces;
using GolemUI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Src
{
    class RemoteSettingsProvider : IRemoteSettingsProvider
    {
        RemoteSettings? _cachedRemoteSettings = null;
        DateTime lastRemoteSettingsDownload = new DateTime();

        public async Task<bool> RequestRemoteSettingsUpdate()
        {
            await Task.Delay(0);

        //todo, download settings and update lastRemoteSettingsDownload

//        "https://golemfactory.github.io/ya-provider-winui/config.json"

            return false;
        }

        public bool LoadRemoteSettings(out RemoteSettings remoteSettings)
        {
            if ((DateTime.Now - lastRemoteSettingsDownload).TotalSeconds < 3600 && _cachedRemoteSettings != null)
            {
                remoteSettings = _cachedRemoteSettings;
                return true;
            }
            remoteSettings = new RemoteSettings();
            return false;
        }

    }
}
