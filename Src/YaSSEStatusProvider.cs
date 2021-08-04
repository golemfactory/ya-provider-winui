using GolemUI.Interfaces;
using GolemUI.Model;
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

namespace GolemUI.Src
{
    class YaSSEStatusProvider : Interfaces.IStatusProvider, IDisposable
    {
        public DateTime? LastUpdate { get; private set; }

        public ICollection<ActivityState>? Activities { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public YaSSEStatusProvider(Interfaces.IProcessControler processControler, ILogger<YaSSEStatusProvider> logger)
        {
            _processControler = processControler;
            _logger = logger;
            _tokenSource = new CancellationTokenSource();

            _processControler.PropertyChanged += OnProcessControlerChanged;

        }

        public void Dispose()
        {
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
            var appKey = await _processControler.GetAppKey();

            webClient.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {appKey}");
            webClient.BaseAddress = _processControler.ServerUri;
            token.Register(webClient.CancelAsync);
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
                            Activities = ev?.Activities ?? null;
                            LastUpdate = ev?.Ts ?? null;
                            OnPropertyChanged("Activities");
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
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private class TrackingEvent
        {
            public DateTime Ts { get; set; }

            public List<Model.ActivityState>? Activities { get; set; }
        }


        private readonly IProcessControler _processControler;
        private readonly ILogger<YaSSEStatusProvider> _logger;
        private Task? _loop;
        private readonly CancellationTokenSource _tokenSource;
    }
}
