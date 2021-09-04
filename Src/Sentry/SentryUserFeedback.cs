#nullable enable
using GolemUI.Interfaces;
using GolemUI.Utils;
using Sentry;
using System;
using System.IO;

namespace GolemUI.Src
{

    public class SentryUserFeedbackService : IUserFeedbackService
    {

        private readonly SentryContext Context = new SentryContext();

        public SentryUserFeedbackService()
        {
        }


        void AddAttachment(Scope scope, String path, bool readAsStream = false)
        {
            if (System.IO.File.Exists(path))
            {
                if (readAsStream)
                {
                    scope.AddAttachment(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Path.GetFileName(path));
                }
                else
                {
                    scope.AddAttachment(path);
                }
            }
            else
            {
                scope.SetExtra(Path.GetFileName(path), "does not exist");
            }
        }

        public void SendUserFeedback(string tag, string name, string email, string comments, bool shouldAttachLogs)
        {

            if (shouldAttachLogs)
            {
                SentrySdk.ConfigureScope(scope =>
                {
                    scope.ClearAttachments();
                    this.AddAttachment(scope, PathUtil.GetLocalBenchmarkPath());
                    this.AddAttachment(scope, PathUtil.GetRemoteSettingsPath());
                    this.AddAttachment(scope, PathUtil.GetLocalSettingsPath());
                    this.AddAttachment(scope, PathUtil.GetLocalLogPath(), readAsStream: true);
                });
            }

            var eventID = Sentry.SentrySdk.CaptureMessage(tag);
            Sentry.SentrySdk.CaptureUserFeedback(new Sentry.UserFeedback(eventID, name, email, comments));
            SentrySdk.ConfigureScope(scope =>
            {
                scope.ClearAttachments();
            });
        }

    }
}
