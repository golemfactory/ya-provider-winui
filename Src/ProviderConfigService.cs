using GolemUI.Command;
using GolemUI.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Src
{
    public class ProviderConfigService : IProviderConfig
    {
        private readonly Command.Provider _provider;

        public ProviderConfigService(Provider provider)
        {
            _provider = provider;
            Config = _provider.Config;
        }

        public Config? Config { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void UpdateNodeName(string? nodeName)
        {
            var config = Config ?? _provider.Config;
            if (config != null)
            {
                config.NodeName = nodeName;
                _provider.Config = Config;
            }
            OnPropertyChanged("Config");
        }

        public void UpdateWalletAddress(string? walletAddress = null)
        {
            var config = Config ?? _provider.Config;
            if (config != null)
            {
                config.Account = walletAddress;
                _provider.Config = Config;
            }
            OnPropertyChanged("Config");
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
