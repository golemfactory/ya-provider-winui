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
        private readonly IProcessController _processController;
        private readonly Src.BenchmarkService _benchmarkService;
        private readonly Interfaces.INotificationService _notificationService;

        private bool _lastIsServerRunning = false;
        private bool _lastIsProviderRunning = false;


        public const int NOTIFICATION_TIMEOUT = 5000;

        public NotificationsMonitor(Interfaces.INotificationService notificationService, Interfaces.IProcessController processController, Src.BenchmarkService benchmarkService)
        {
            _notificationService = notificationService;
            _processController = processController;
            _benchmarkService = benchmarkService;
            _processController.PropertyChanged += _processController_PropertyChanged;
            _benchmarkService.PropertyChanged += _benchmarkService_PropertyChanged;
        }

        private void _benchmarkService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsRunning")
            {
                if (_benchmarkService.IsRunning)
                {
                    _notificationService.PushNotification(new SimpleNotificationObject(Tag.Benchmark, "benchmark is running", expirationTimeInMs: 0));
                }

                if (!_benchmarkService.IsRunning)
                {
                    _notificationService.PushNotification(new SimpleNotificationObject(Tag.Benchmark, "benchmark finished", expirationTimeInMs: NOTIFICATION_TIMEOUT));
                }

            }
            if (e.PropertyName == "Status")
            {
                if (_benchmarkService.IsRunning)
                {
                    if (_benchmarkService.TotalMhs != 0)
                    {
                        _notificationService.PushNotification(new SimpleNotificationObject(Tag.Benchmark, $"benchmark is running ({String.Format("{0:0.0#}", _benchmarkService.TotalMhs)} MH/s)", expirationTimeInMs: 0));
                    }
                    else
                    {
                        _notificationService.PushNotification(new SimpleNotificationObject(Tag.Benchmark, $"benchmark is running (building DAG)", expirationTimeInMs: 0));
                    }
                }
            }
        }

        private void _processController_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsServerRunning")
            {
                string? notificationText = null;
                if (!_lastIsServerRunning && _processController.IsServerRunning)
                {
                    notificationText = "backend service is ready";
                    _lastIsServerRunning = true;
                }
                else if (_lastIsServerRunning && !_processController.IsServerRunning)
                {
                    notificationText = "backend service stopped";
                    _lastIsServerRunning = false;
                }
                if (notificationText != null)
                {
                    _notificationService.PushNotification(new SimpleNotificationObject(Tag.YagnaStatus, notificationText, expirationTimeInMs: NOTIFICATION_TIMEOUT));
                }
            }
            if (e.PropertyName == "IsProviderRunning")
            {
                string? notificationText = null;
                if (!_lastIsProviderRunning && _processController.IsProviderRunning)
                {
                    notificationText = "provider service started";
                    _lastIsProviderRunning = true;
                }
                else if (_lastIsProviderRunning && !_processController.IsProviderRunning)
                {
                    notificationText = "provider service stopped";
                    _lastIsProviderRunning = false;
                }
                if (notificationText != null)
                {
                    _notificationService.PushNotification(new SimpleNotificationObject(Tag.ProviderStatus, notificationText, expirationTimeInMs: NOTIFICATION_TIMEOUT, group: false));
                }
            }
            /*if (e.PropertyName == "IsStarting")
            {
                //   _notificationService.PushNotification(new SimpleNotificationObject(Tag.YagnaStarting, (_processController.IsStarting ? "starting" : "stopping") + " subsystems...",4000));
            }*/
        }
    }
}
