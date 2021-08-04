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
        public string YagnaId = "";


        public SentryAdditionalDataIngester(Interfaces.IProcessControler processControler, Src.BenchmarkService benchmarkService)
        {
            _processControler = processControler;
            _benchmarkService = benchmarkService;
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
