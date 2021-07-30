using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace GolemUI.Src
{
    public class NotyficationService
    {
        public void Show()
        {
            string tag = DateTime.Now.GetHashCode().ToString();

            int downloadDuration = 15;

            var data = new Dictionary<string, string>
            {
                { "progressValue", "0" },
                { "progressValueStringOverride", $"{downloadDuration} seconds" }
            };

            new ToastContentBuilder()
                .AddText("Test it")
                .AddButton(new ToastButton("Show", "show"))
                .Show(t =>
                {
                    t.Tag = tag;
                    t.Data = new NotificationData(data, 1);
                });
            var _nt = Microsoft.Toolkit.Uwp.Notifications.ToastNotificationManagerCompat.CreateToastNotifier();

            ToastContent toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = "Mining..."
                            },
                            new AdaptiveProgressBar()
                            {
                                Title = "InteractiveToastSample.zip",
                                Value = new BindableProgressBarValue("progressValue"),                                
                                ValueStringOverride = new BindableString("progressValueStringOverride"),
                                Status = "Downloading..."
                            }
                        }
                    }
                }
            };
            
            ToastNotification notification = new ToastNotification(toastContent.GetXml())
            {
                Tag = tag,
                Data = new NotificationData(data)
            };
            
            _nt.Show(notification);
            StartDownload(_nt, tag, downloadDuration);
        }

        public async void StartDownload(ToastNotifierCompat NOTIFIER, string toastTag, int durationInSeconds)
        {           

            for (int i=0; i<durationInSeconds; ++i)
            {
                UpdateToast(NOTIFIER, i, durationInSeconds - i, toastTag);
                await Task.Delay(1000);
            }

        
        }

        private void UpdateToast(ToastNotifierCompat NOTIFIER, int value, double secondsRemaining, string toastTag)
        {
            var data = new Dictionary<string, string>
                {
                    { "progressValue", (value*1.0/15.0).ToString() },
                    { "progressValueStringOverride", $"{(int)secondsRemaining} seconds" }
                };

            try
            {
                NOTIFIER.Update(new NotificationData(data, (uint)value+1), toastTag);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }
}
