﻿using GolemUI.Interfaces;
using GolemUI.Model;
using GolemUI.Src.AppNotificationService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace GolemUI.Src
{
    class YaSSEStatusProvider : Interfaces.IStatusProvider, IDisposable
    {
        public DateTime? LastUpdate { get; private set; }

        public ICollection<ActivityState> Activities
        {
            get => _activities;
        }
        private List<ActivityState> _activities = new List<ActivityState>();

        public event PropertyChangedEventHandler? PropertyChanged;

        public YaSSEStatusProvider(Interfaces.IProcessController processController, ILogger<YaSSEStatusProvider> logger, INotificationService notificationService)
        {
            _processController = processController;
            _notificationService = notificationService;
            _logger = logger;
            _tokenSource = new CancellationTokenSource();
            _processController.PropertyChanged += OnProcessControllerChanged;
            _hc = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMinutes(2)
            };
            _hc.Tick += this._checkHealth;
            _hc.Start();

            _client.BaseAddress = new Uri("http://worldtimeapi.org/api/timezone/Europe/London");
            _client.DefaultRequestHeaders.Accept.Clear();

            Microsoft.Win32.SystemEvents.TimeChanged += CheckSynchronization;

            _intervalTimer.Interval = TimeSpan.FromSeconds(1);
            _intervalTimer.Tick += CheckSynchronization;
            _intervalTimer.Start();
        }

        private void OnProcessControllerChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsProviderRunning")
            {
                if (!_processController.IsProviderRunning)
                {
                    _activities = new List<ActivityState>();
                    OnPropertyChanged("");
                }
            }
            if (_processController.IsServerRunning && _loop == null)
            {
                _loop = _refreshLoop();
            }
        }

        private void _checkHealth(object sender, EventArgs e)
        {
            if (_loop != null && _loop.IsFaulted)
            {
                _logger.LogError(_loop.Exception, "Restarting status event receiver, IsServerRunning={0}", _processController.IsServerRunning);

                if (_processController.IsServerRunning && !_processController.IsStarting)
                {
                    _loop = _refreshLoop();
                }
            }
        }

        public void Dispose()
        {
            _hc.Stop();
            _processController.PropertyChanged -= OnProcessControllerChanged;
            _tokenSource.Cancel();
            Microsoft.Win32.SystemEvents.TimeChanged -= CheckSynchronization;
        }

        private async Task _refreshLoop()
        {
            _logger.LogInformation("Starting");
            var token = _tokenSource.Token;

            var options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };

            using WebClient webClient = new WebClient();
            token.Register(webClient.CancelAsync);

            var appKey = await _processController.GetAppKey();
            webClient.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {appKey}");
            webClient.BaseAddress = _processController.ServerUri;

            DateTime newReconnect = DateTime.Now;
            while (!token.IsCancellationRequested)
            {
                var now = DateTime.Now;
                if (newReconnect > now)
                {
                    await Task.Delay(newReconnect - now);
                }
                newReconnect = now + TimeSpan.FromMinutes(2);
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }

                try
                {
                    var stream = await webClient.OpenReadTaskAsync("/activity-api/v1/_monitor");
                    using StreamReader reader = new StreamReader(stream);
                    token.Register(() =>
                    {
                        _logger.LogDebug("stop");
                        reader.Close();

                    });
                    StringBuilder dataBuilder = new StringBuilder();
                    while (true)
                    {
                        if (token.IsCancellationRequested)
                        {
                            token.ThrowIfCancellationRequested();
                        }
                        var line = await reader.ReadLineAsync();
                        if (line.StartsWith("data:"))
                        {
                            dataBuilder.Append(line.Substring(5).TrimStart());
                        }
                        else if (line == "")
                        {
                            var json = dataBuilder.ToString();
                            dataBuilder.Clear();
                            _logger.LogTrace("got json {0}", json);
                            try
                            {
                                var ev = JsonSerializer.Deserialize<TrackingEvent>(json, options);
                                _activities = ev?.Activities ?? new List<ActivityState>();
                                LastUpdate = ev?.Ts ?? null;
                                try
                                {
                                    OnPropertyChanged("Activities");
                                }
                                catch (Exception e)
                                {
                                    _logger.LogError(e, "Failed to send notification");
                                }
                            }
                            catch (JsonException e)
                            {
                                _logger.LogError(e, "Invalid monitoring event: {0}", json);
                                break;
                            }
                        }
                    }
                }
                catch (WebException e)
                {
                    _logger.LogError(e, "failed to get exe-units status");
                }
                catch (IOException e)
                {
                    _logger.LogError(e, "status loop failure");
                }
            }
        }

        private System.Net.Http.HttpClient _client = new System.Net.Http.HttpClient();
        private DispatcherTimer _intervalTimer = new DispatcherTimer();
        private bool _isClockSynchronized = true;
        private const int _synchronizedRangeInMinutes = 5;

        public bool IsSynchronized()
        {
            return _isClockSynchronized;
        }

        // This is the method to run when the timer is raised.
        private void CheckSynchronization(Object myObject,
                                                EventArgs myEventArgs)
        {
            var notification = new SimpleNotificationObject(Tag.Synchronization, "Evaluating system time", "", 1000);
            _notificationService.PushNotification(notification);
            var task = GetTime().ContinueWith((response) =>
            {
                var resp = response.GetAwaiter().GetResult();
                DateTime parsedDate;
                if (DateTime.TryParse(resp, out parsedDate))
                {
                    DateTime serverUTC = parsedDate.ToUniversalTime();
                    DateTime now = DateTime.Now.ToUniversalTime();
                    TimeSpan difference = now.Subtract(serverUTC);

                    var minutesDifference = Math.Abs(difference.TotalMinutes);

                    bool newIsClockSynchronized = minutesDifference < _synchronizedRangeInMinutes;

                    if (newIsClockSynchronized != _isClockSynchronized)
                    {
                        _isClockSynchronized = newIsClockSynchronized;
                        OnPropertyChanged("isSynchronized");
                    }
                }
            });
            _intervalTimer.Stop();
        }

        async Task<String> GetTime()
        {
            System.Net.Http.HttpResponseMessage response = await _client.GetAsync("");
            var responseString = await response.Content.ReadAsStringAsync();

            Dictionary<string, string> json;

            try
            {
                json = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
                return json["utc_datetime"];
            }
            catch (Exception e)
            {
                return "";
            }
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private class TrackingEvent
        {
            public DateTime Ts { get; set; }

            public List<Model.ActivityState> Activities { get; set; } = new List<ActivityState>();
        }


        private readonly IProcessController _processController;
        private readonly INotificationService _notificationService;
        private readonly ILogger<YaSSEStatusProvider> _logger;
        private Task? _loop;
        private readonly CancellationTokenSource _tokenSource;
        private readonly DispatcherTimer _hc;
    }
}
