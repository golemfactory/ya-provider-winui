using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GolemUI.Interfaces;

namespace GolemUI
{
    public class PluginsDownloader
    {
        private readonly IRemoteSettingsProvider _remoteSettingsProvider;
        public PluginsDownloader(IRemoteSettingsProvider remoteSettingsProvider)
        {
            _remoteSettingsProvider = remoteSettingsProvider;

        }
        public async void DownloadPlugins()
        {

        }
    }
}
