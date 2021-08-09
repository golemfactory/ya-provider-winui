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

        private bool _lastIsServerRunning = false;
        private bool _lastIsProviderRunning = false;


        public const int NOTIFICATION_TIMEOUT = 5000;

        public NotificationsMonitor(Interfaces.INotificationService notificationService, Interfaces.IProcessControler processControler, Src.BenchmarkService benchmarkService)
        {
            _notificationService = notificationService;
            _processControler = processControler;
            _benchmarkService = benchmarkService;
            _processControler.PropertyChanged += _processControler_PropertyChanged;
            _benchmarkService.PropertyChanged += _benchmarkService_PropertyChanged;
        }

        private void _benchmarkService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsRunning")
            {
                if (_benchmarkService.IsRunning)
                {
                    _notificationService.PushNotification(new SimpleNotificationObject(Tag.Benchmark, "Benchmark is running", expirationTimeInMs: 0));
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
                        _notificationService.PushNotification(new SimpleNotificationObject(Tag.Benchmark, $"benchmark is running ({_benchmarkService.TotalMhs} MH/s)", expirationTimeInMs: 0));
                    }
                    else
                    {
                        _notificationService.PushNotification(new SimpleNotificationObject(Tag.Benchmark, $"benchmark is running (building DAG)", expirationTimeInMs: 0));
                    }
                }
            }
        }

        private void _processControler_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsServerRunning")
            {
                string? notificationText = null;
                if (!_lastIsServerRunning && _processControler.IsServerRunning)
                {
                    notificationText = "Backend service is ready";
                    _lastIsServerRunning = true;
                }
                else if (_lastIsServerRunning && !_processControler.IsServerRunning)
                {
                    notificationText = "Backend service stopped";
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
                if (!_lastIsProviderRunning && _processControler.IsProviderRunning)
                {
                    notificationText = "Provider service started";
                    _lastIsProviderRunning = true;
                }
                else if (_lastIsProviderRunning && !_processControler.IsProviderRunning)
                {
                    notificationText = "Provider service stopped";
                    _lastIsProviderRunning = false;
                }
                if (notificationText != null)
                {
                    _notificationService.PushNotification(new SimpleNotificationObject(Tag.ProviderStatus, notificationText, expirationTimeInMs: NOTIFICATION_TIMEOUT, group: false));
                }
            }
            /*if (e.PropertyName == "IsStarting")
            {
                //   _notificationService.PushNotification(new SimpleNotificationObject(Tag.YagnaStarting, (_processControler.IsStarting ? "starting" : "stopping") + " subsystems...",4000));
            }*/
        }
    }
}
