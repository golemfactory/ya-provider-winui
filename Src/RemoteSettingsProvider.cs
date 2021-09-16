using GolemUI.Interfaces;
using GolemUI.Model;
using GolemUI.Src.AppNotificationService;
using GolemUI.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace GolemUI.Src
{
    class RemoteSettingsProvider : IRemoteSettingsProvider
    {
        ILogger<RemoteSettingsProvider> _logger;
        bool _isDownloadingSettings = false;
        private DispatcherTimer? _timer = null;

#if DEBUG
        private TimeSpan RefreshInterval => TimeSpan.FromSeconds(120.0);
#else
        private TimeSpan RefreshInterval => TimeSpan.FromMinutes(60.0);
#endif


        private readonly INotificationService _notificationService;

        public IRemoteSettingsProvider.RemoteSettingsUpdatedEventHandler? OnRemoteSettingsUpdated { get; set; }

        public RemoteSettingsProvider(ILogger<RemoteSettingsProvider> logger, INotificationService notificationService)
        {
            _notificationService = notificationService;
            _logger = logger;

            _timer = new DispatcherTimer();
            _timer.Interval = RefreshInterval;
            _timer.Tick += OnRefreshTick;
            _timer.Start();

            //I have to get tak variable, otherwise there is warning
            var firstTickTaskUnused = RemoteSettingsUpdateAsync();
        }

        private async void OnRefreshTick(object sender, EventArgs? e)
        {
            await RemoteSettingsUpdateAsync();
        }


        public async Task<bool> RemoteSettingsUpdateAsync()
        {
            if (_isDownloadingSettings)
            {
                return false;
            }
            _isDownloadingSettings = true;

            //todo, download settings and update lastRemoteSettingsDownload


            string remoteTmpPath = PathUtil.GetRemoteTmpSettingsPath();
            string remotePath = PathUtil.GetRemoteSettingsPath();

            try
            {
                using (var client = new WebClient())
                {
                    if (File.Exists(remoteTmpPath))
                    {
                        _logger.LogInformation("Removing old download tmp file: " + remoteTmpPath);
                        File.Delete(remoteTmpPath);
                    }
                    long timestamp = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
                    await client.DownloadFileTaskAsync(new Uri($"https://golemfactory.github.io/ya-provider-winui/config.json?timestamp={timestamp}"), remoteTmpPath);
                    if (!File.Exists(remoteTmpPath))
                    {
                        _logger.LogError("Failed to download new config");
                        return false;
                    }
                    RemoteSettings? rs = JsonConvert.DeserializeObject<RemoteSettings>(File.ReadAllText(remoteTmpPath));
                    if (String.IsNullOrEmpty(rs.LatestVersion))
                    {
                        throw new Exception("Version field cannot be empty");
                    }
                    File.Delete(remoteTmpPath);

                    rs.DownloadedDateTime = DateTime.Now;
                    File.WriteAllText(remotePath, JsonConvert.SerializeObject(rs, Formatting.Indented));

                    _notificationService.PushNotification(new SimpleNotificationObject(Tag.AppStatus, "config downloaded: " + rs.LatestVersion, expirationTimeInMs: 5000));
                    if (OnRemoteSettingsUpdated != null)
                    {
                        OnRemoteSettingsUpdated(rs);
                    }
                    return true;
                }
            }
            catch (WebException ex)
            {
                _logger.LogWarning("Web exception when downloading remote config: " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error when downloading remote config: " + ex.Message);
            }
            finally
            {
                _isDownloadingSettings = false;
            }

            return false;
        }

        public bool LoadRemoteSettings(out RemoteSettings remoteSettings)
        {
            try
            {
                string remoteFilePath = PathUtil.GetRemoteSettingsPath();
                if (!File.Exists(remoteFilePath))
                {
                    _logger.LogInformation("Remote file path not exists");
                    remoteSettings = new RemoteSettings();
                    return false;
                }
                string jsonText = File.ReadAllText(remoteFilePath);
                remoteSettings = JsonConvert.DeserializeObject<RemoteSettings>(jsonText);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Problem when reading remote settings " + ex.Message);
                remoteSettings = new RemoteSettings();
                return false;
            }
        }
    }
}
