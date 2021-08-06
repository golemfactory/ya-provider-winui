#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using GolemUI.Interfaces;
using Sentry;

namespace GolemUI.Src.AppNotificationService
{
    public class NotificationsMonitor
    {
        private readonly IProcessControler _processControler;
        private readonly Src.BenchmarkService _benchmarkService;
        private readonly Interfaces.INotificationService _notificationService;


        public NotificationsMonitor(Interfaces.INotificationService notificationService, Interfaces.IProcessControler processControler, Src.BenchmarkService benchmarkService)
        {
            _notificationService = notificationService;
            _processControler = processControler;
            _benchmarkService = benchmarkService;
            _processControler.PropertyChanged += _processControler_PropertyChanged;

        }
        private  void _processControler_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsServerRunning")
            {
                _notificationService.PushNotification(new SimpleNotificationObject(Tag.YagnaStatus, "yagna " + (_processControler.IsServerRunning ? "started" : "stopped",5000)));
            }
            if (e.PropertyName == "IsProviderRunning")
            {
                _notificationService.PushNotification(new SimpleNotificationObject(Tag.YagnaStatus, "provider " + (_processControler.IsProviderRunning ? "started" : "stopped",5000)));
            }
            if (e.PropertyName == "IsStarting")
            {
                _notificationService.PushNotification(new SimpleNotificationObject(Tag.YagnaStatus, (_processControler.IsStarting ? "starting" : "stopping") + " subsystems...",4000));
            }
        }

    }
}
