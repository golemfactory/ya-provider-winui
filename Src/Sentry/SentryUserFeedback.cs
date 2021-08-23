#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using GolemUI.Interfaces;
using GolemUI.Utils;
using Sentry;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace GolemUI.Src
{

    public class SentryUserFeedbackService : IUserFeedbackService
    {

        private SentryContext Context = new SentryContext();

        public SentryUserFeedbackService()
        {
        }

        public void SendUserFeedback(string tag, string name, string email, string comments)
        {
            SentrySdk.ConfigureScope(scope =>
            {
                scope.AddAttachment(PathUtil.GetLocalBenchmarkPath());
                scope.AddAttachment(PathUtil.GetRemoteSettingsPath());
                scope.AddAttachment(PathUtil.GetLocalSettingsPath());
            });
            var eventID = Sentry.SentrySdk.CaptureMessage(tag);
            Sentry.SentrySdk.CaptureUserFeedback(new Sentry.UserFeedback(eventID, name, email, comments));
            SentrySdk.ConfigureScope(scope =>
            {
                scope.ClearAttachments();
            });
        }

    }
}
