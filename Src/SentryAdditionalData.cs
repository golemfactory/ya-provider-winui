#nullable enable
using System;
using System.ComponentModel;
using GolemUI.Interfaces;
using Sentry;

namespace GolemUI
{
    public class SentryAdditionalData
    {
        private readonly IProcessControler _processControler;
        public string YagnaId = "";
        public SentryAdditionalData(Interfaces.IProcessControler processControler)
        {
            _processControler = processControler;
            _processControler.PropertyChanged += _processControler_PropertyChanged;
        }
        private async void _processControler_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsServerRunning")
            {
                YagnaId = (await _processControler.Me()).Id;
                SentrySdk.ConfigureScope(async scope =>
                {
                    scope.Contexts["user_data"] = new
                    {
                        UserName = Environment.UserName,
                        YagnaId = (await _processControler.Me()).Id
                    };
                });

            }
        }

    }
}
