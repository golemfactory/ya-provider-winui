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
        private readonly Interfaces.IProviderConfig _providerConfig;
        private SentryContext Context = new SentryContext();



        public SentryAdditionalDataIngester(Interfaces.IProcessControler processControler, Interfaces.IProviderConfig providerConfig)
        {
            _processControler = processControler;
            _providerConfig = providerConfig;
            _providerConfig.PropertyChanged += ProviderConfig_PropertyChanged;
            _processControler.PropertyChanged += _processControler_PropertyChanged;
            Context.MemberChanged += Context_MemberChanged;

        }
        public void InitContextItems()
        {
            UpdateNodeName(_providerConfig.Config?.NodeName ?? "");
            Context.AddItem("UserName", Environment.UserName);
            Context_MemberChanged();
        }
        private void SetScope(Scope scope, String key, String value)
        {
            scope.SetTag(key, value);
            scope.SetExtra(key, value);
        }
        private void Context_MemberChanged()
        {
            SentrySdk.ConfigureScope(scope => Context.Items.ToList().ForEach(x => SetScope(scope, x.Key, x.Value)));

        }

        void UpdateNodeName(string nodeName)
        {
            if (!String.IsNullOrEmpty(nodeName))
                Context.AddItem("NodeName", nodeName);
        }
        private void ProviderConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "NodeName")
                UpdateNodeName(_providerConfig.Config?.NodeName ?? "");
        }

        private async void _processControler_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsServerRunning")
                Context.AddItem("YagnaId", (await _processControler.Me()).Id);
        }

    }
}
