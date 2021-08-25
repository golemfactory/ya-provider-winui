#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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


        void AddAttachment(Scope scope, String path)
        {
            if (System.IO.File.Exists(path))
                scope.AddAttachment(PathUtil.GetLocalBenchmarkPath());
            else
                scope.SetExtra(Path.GetFileName(path), "does not exist");
        }

        public void SendUserFeedback(string tag, string name, string email, string comments, bool shouldAttachLogs)
        {
            if (shouldAttachLogs)
                SentrySdk.ConfigureScope(scope =>
                {
                    this.AddAttachment(scope, PathUtil.GetLocalBenchmarkPath());
                    this.AddAttachment(scope, PathUtil.GetRemoteSettingsPath());
                    this.AddAttachment(scope, PathUtil.GetLocalSettingsPath());
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
