using BetaMiner.Interfaces;
using BetaMiner.Model;
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

namespace BetaMiner.Src
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

        public YaSSEStatusProvider(Interfaces.IProcessControler processControler, ILogger<YaSSEStatusProvider> logger)
        {
            _processControler = processControler;
            _logger = logger;
            _tokenSource = new CancellationTokenSource();
            _processControler.PropertyChanged += OnProcessControlerChanged;
            _hc = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMinutes(2)
            };
            _hc.Tick += this._checkHealth;
            _hc.Start();

            _processControler.PropertyChanged += OnProcessControllerChanged;
        }

        private void OnProcessControllerChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsProviderRunning")
            {
                if (!_processControler.IsProviderRunning)
                {
                    _activities = new List<ActivityState>();
                    OnPropertyChanged("");
                }
            }
        }

        private void _checkHealth(object sender, EventArgs e)
        {
            if (_loop != null && _loop.IsFaulted)
            {
                _logger.LogError(_loop.Exception, "Restarting status event receiver, IsServerRunning={0}", _processControler.IsServerRunning);

                if (_processControler.IsServerRunning && !_processControler.IsStarting)
                {
                    _loop = _refreshLoop();
                }
            }
        }

        public void Dispose()
        {
            _hc.Stop();
            _processControler.PropertyChanged -= OnProcessControlerChanged;
            _tokenSource.Cancel();
        }

        private void OnProcessControlerChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_processControler.IsServerRunning && _loop == null)
            {
                _loop = _refreshLoop();
            }
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

            var appKey = await _processControler.GetAppKey();
            webClient.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {appKey}");
            webClient.BaseAddress = _processControler.ServerUri;

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

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private class TrackingEvent
        {
            public DateTime Ts { get; set; }

            public List<Model.ActivityState> Activities { get; set; } = new List<ActivityState>();
        }


        private readonly IProcessControler _processControler;
        private readonly ILogger<YaSSEStatusProvider> _logger;
        private Task? _loop;
        private readonly CancellationTokenSource _tokenSource;
        private readonly DispatcherTimer _hc;
    }
}
