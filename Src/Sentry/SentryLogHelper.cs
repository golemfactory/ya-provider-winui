#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using GolemUI.Interfaces;
using Sentry;

namespace GolemUI
{
    public static class Sentry
    {
        public static class Breadcrumb
        {
            public static void PropertySet(String message, String parameterName, String parameterValue)
            {
                SentrySdk.AddBreadcrumb(message: message + " - " + parameterName + " set", data: new Dictionary<string, string>() { { parameterName, parameterValue } }, category: "property set", level: BreadcrumbLevel.Info);
            }
            public static void PropertySet(String parameterName, String parameterValue)
            {
                PropertySet("", parameterName, parameterValue);
            }

        }
        public static class Setup
        {
            public static void Log(String msg)
            {
                msg = "> Setup Window > " + msg;
                SentrySdk.CaptureMessage(msg);
                SentrySdk.AddBreadcrumb(message: msg, category: "setup window", level: BreadcrumbLevel.Info);
            }
        }
        public static class Dashboard
        {
            public static void Log(String msg)
            {
                msg = "> Dashboard > " + msg;
                SentrySdk.CaptureMessage(msg);
                SentrySdk.AddBreadcrumb(message: msg, category: "dashboard window", level: BreadcrumbLevel.Info);
            }
            public static void Log(String msg, string parameterName, string parameterValue)
            {
                msg = "> Dashboard > " + msg;
                SentrySdk.CaptureMessage(msg);
                SentrySdk.AddBreadcrumb(message: msg, data: new Dictionary<string, string>() { { parameterName, parameterValue } }, category: "dashboard window", level: BreadcrumbLevel.Info);
            }
        }

    }
}
