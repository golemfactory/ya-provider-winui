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

        private IList<string>? _activePresets;

        private bool _isPresetActive(string presetName)
        {
            if (_activePresets == null)
            {
                _activePresets = _provider.ActivePresets;
            }
            return _activePresets.Contains(presetName);
        }
        private void _setPreset(string presetName, bool value)
        {
            if (value)
            {
                _provider.ActivatePreset(presetName);
            }
            else
            {
                _provider.DeactivatePreset(presetName);
            }
            _activePresets = _provider.ActivePresets;
            OnPropertyChanged("IsMiningActive");
            OnPropertyChanged("IsCpuActive");
        }

        public bool IsMiningActive
        {
            get => _isPresetActive("gminer");
            set
            {
                _setPreset("gminer", value);
            }
        }


        public bool IsCpuActive
        {
            get => _isPresetActive("wasmtime");
            set { _setPreset("wasmtime", value); }
        }

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
