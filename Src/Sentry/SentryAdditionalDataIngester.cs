#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using GolemUI.Interfaces;
using Sentry;

namespace GolemUI
{
    public class SentryAdditionalDataIngester
    {
        private readonly IProcessControler _processControler;
        private readonly Src.BenchmarkService _benchmarkService;
        private readonly Interfaces.IProviderConfig _providerConfig;
        public string YagnaId = "";


        public SentryAdditionalDataIngester(Interfaces.IProcessControler processControler, Src.BenchmarkService benchmarkService, Interfaces.IProviderConfig providerConfig)
        {
            _processControler = processControler;
            _benchmarkService = benchmarkService;
            _providerConfig = providerConfig;
            providerConfig.PropertyChanged += ProviderConfig_PropertyChanged;
            _processControler.PropertyChanged += _processControler_PropertyChanged;

        }

        void UpdateNodeName(string nodeName)
        {
            if (!String.IsNullOrEmpty(nodeName))
            {
                SentrySdk.ConfigureScope( scope =>
                {
                    scope.Contexts["user_info_provider"] = new
                    {
                        NodeName = nodeName
                    };
                });
            }
        }
        private void ProviderConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "NodeName")
            {
                UpdateNodeName(_providerConfig.Config?.NodeName ?? "");

            }
        }

        private async void _processControler_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsServerRunning")
            {
                YagnaId = (await _processControler.Me()).Id;
                SentrySdk.ConfigureScope(async scope =>
                {
                    scope.Contexts["user_info_yagna"] = new
                    {
                        YagnaId = (await _processControler.Me()).Id
                    };
                });

            }
        }

    }
}
