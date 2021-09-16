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
        public Network Network { get; private set; }

        public ProviderConfigService(Provider provider, Network network)
        {
            _provider = provider;
            Config = _provider.Config;
            Network = network;

            string? name = Config?.NodeName;
            int count = ActiveCpuCount;
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

        public int ActiveCpuCount => _provider?.DefaultProfile?.CpuThreads ?? 0;

        public void UpdateActiveCpuThreadsCount(int threadsCount)
        {
            _provider.UpdateDefaultProfile("--cpu-threads", threadsCount.ToString());
        }
        public void UpdateNodeName(string? nodeName)
        {
            var config = Config ?? _provider.Config;
            if (config != null)
            {
                config.NodeName = nodeName;
                _provider.Config = Config;
            }
            OnPropertyChanged("Config");
            OnPropertyChanged("NodeName");

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

        public async Task Prepare(bool isGpuCapable)
        {
            var changedProps = await Task.Run(() =>
            {
                var changedProperties = new List<string>();
                var config = Config ?? _provider.Config;
                if (config!.Subnet == null || config!.Subnet != GolemUI.Properties.Settings.Default.Subnet)
                {
                    config.Subnet = GolemUI.Properties.Settings.Default.Subnet;
                    _provider.Config = Config;
                }

                var presets = _provider.Presets.Select(x => x.Name).ToList();
                string _info, _args;

                if (!presets.Contains("gminer"))
                {

                    _provider.AddPreset(new GolemUI.Command.Preset("gminer", "gminer", new Dictionary<string, decimal>()
                    {
                        { "share", 0.003m },
                        { "duration", 0m },
                        { "raw-share", 0m },
                        { "hash-rate", 0m }
                    }), out _args, out _info);
                    if (isGpuCapable)
                    {
                        _provider.ActivatePreset("gminer");
                        changedProperties.Add("IsMiningActive");
                    }
                }
                else
                {
                    _provider.Preset["gminer"].UpdatePrices(new Dictionary<string, decimal>() {
                            { "share", 0.003m },
                            { "duration", 0m },
                            { "raw-share", 0m },
                            { "hash-rate", 0m }
                        });

                }

                if (!presets.Contains("wasmtime"))
                {
                    _provider.AddPreset(new GolemUI.Command.Preset("wasmtime", "wasmtime", new Dictionary<string, decimal>()
                    {
                        { "cpu", 0.001m },
                        { "duration", 0m }
                    }), out _args, out _info);
                    _provider.ActivatePreset("wasmtime");
                    changedProperties.Add("IsCpuActive");
                }

                if (presets.Contains("default"))
                {
                    _provider.DeactivatePreset("default");
                }

                return changedProperties;
            });
            if (changedProps.Count > 0)
            {
                _activePresets = _provider.ActivePresets;
                foreach (var propName in changedProps)
                {
                    OnPropertyChanged(propName);
                }
            }
        }
    }
}
