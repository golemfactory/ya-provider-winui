#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using GolemUI.Interfaces;
using Sentry;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace GolemUI.Src
{

    public class SentryUserFeedbackService : IUserFeedbackService
    {
        /*  private readonly IProcessControler _processControler;
          private readonly Interfaces.IProviderConfig _providerConfig;*/
        private SentryContext Context = new SentryContext();



        public SentryUserFeedbackService(/*Interfaces.IProcessControler processControler, Interfaces.IProviderConfig providerConfig*/)
        {
            /* _processControler = processControler;
             _providerConfig = providerConfig;
             _providerConfig.PropertyChanged += ProviderConfig_PropertyChanged;
             _processControler.PropertyChanged += _processControler_PropertyChanged;
             Context.MemberChanged += Context_MemberChanged;*/

        }
        public void SendUserFeedback(string tag, string name, string email, string comments)
        {
            var eventID = Sentry.SentrySdk.CaptureMessage(tag);
            Sentry.SentrySdk.CaptureUserFeedback(new Sentry.UserFeedback(eventID, name, email, comments));
        }
    }
}
