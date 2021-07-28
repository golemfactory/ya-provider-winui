#nullable enable
using System;
using System.ComponentModel;
using GolemUI.Interfaces;
using Sentry;

namespace GolemUI
{
    public static class Sentry
    {
        public static class Setup
        {
            public static void Log(String msg)
            {
                SentrySdk.CaptureMessage(msg);
                SentrySdk.AddBreadcrumb(message: msg, category: "setup window", level: BreadcrumbLevel.Info);
            }
        }

    }
}
