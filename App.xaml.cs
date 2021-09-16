#nullable enable
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using GolemUI.Command;
using GolemUI.UI;
using GolemUI.UI.CustomControls;
using GolemUI.Utils;
using GolemUI.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sentry;

namespace GolemUI
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly GolemUI.ChildProcessManager _childProcessManager;

        private Dashboard? _dashboard = null;
        public App()
        {
            IsShuttingDown = false;
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            _childProcessManager = new GolemUI.ChildProcessManager();
            _childProcessManager.AddProcess(Process.GetCurrentProcess());

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }
        public bool IsShuttingDown { get; private set; }

        public new void Shutdown(int exitCode = 0)
        {
            this.IsShuttingDown = true;
            base.Shutdown(exitCode);
        }
        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            SentrySdk.CaptureException(e.Exception);

            var logger = _serviceProvider.GetRequiredService<ILogger<App>>();
            logger.LogError(e.Exception, "App_DispatcherUnhandledException");

            //MessageBox.Show(e.Exception.Message, "unexpected error");

            //TODO: to discuss if we should allow the app to crash or not
            //e.Handled = true;
        }
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<Interfaces.IStartWithWindows, Src.StartWithWindows>();
            services.AddSingleton<Interfaces.IBenchmarkResultsProvider, Src.BenchmarkResultsProvider>();
            services.AddSingleton<Interfaces.IUserSettingsProvider, Src.UserSettingsProvider>();
            services.AddSingleton<Interfaces.IRemoteSettingsProvider, Src.RemoteSettingsProvider>();
            services.AddSingleton<Interfaces.IPriceProvider, Src.CoinGeckoPriceProvider>();
            services.AddSingleton<Interfaces.IEstimatedProfitProvider, Src.EstimatedEarningsProvider>();
            services.AddSingleton<Interfaces.ITaskProfitEstimator, Src.TaskProfitEstimator>();
            services.AddSingleton<Interfaces.IUserFeedbackService, Src.SentryUserFeedbackService>();

            services.AddSingleton(typeof(Interfaces.IProcessController), typeof(GolemUI.ProcessController));
            services.AddSingleton(typeof(Command.YagnaSrv));
            services.AddSingleton(typeof(Command.Provider));
            services.AddSingleton(cfg => new Src.SingleInstanceLock());

            services.AddSingleton(GolemUI.Properties.Settings.Default.TestNet ? Network.Mumbai : Network.Polygon);
            services.AddSingleton<Interfaces.IPaymentService, Src.PaymentService>();
            services.AddSingleton<Interfaces.IProviderConfig, Src.ProviderConfigService>();
            services.AddSingleton<Interfaces.IStatusProvider, Src.YaSSEStatusProvider>();
            services.AddSingleton<Interfaces.IHistoryDataProvider, Src.HistoryDataProvider>();
            services.AddSingleton<Interfaces.INotificationService, Src.AppNotificationService.AppNotificationService>();
            services.AddSingleton<Src.BenchmarkService>();

            services.AddTransient(typeof(SentryAdditionalDataIngester));
            services.AddTransient(typeof(Src.AppNotificationService.NotificationsMonitor));
            services.AddTransient(typeof(DashboardWallet));
            services.AddTransient(typeof(ViewModel.WalletViewModel));
            services.AddTransient(typeof(ViewModel.DashboardMainViewModel));
            services.AddTransient(typeof(ViewModel.SetupViewModel));
            services.AddTransient(typeof(ViewModel.CustomControls.NotificationBarViewModel));
#if STATISTICS_ENABLED
            services.AddTransient(typeof(ViewModel.StatisticsViewModel));
#endif

            services.AddTransient(typeof(DashboardMain));
            services.AddTransient(typeof(NotificationBar));
            services.AddTransient(typeof(DashboardSettings));
            services.AddTransient(typeof(DashboardSettingsAdv));
#if STATISTICS_ENABLED
            services.AddTransient(typeof(DashboardStatistics));
#endif
            services.AddTransient(typeof(SettingsViewModel));
            services.AddTransient(typeof(SettingsAdvViewModel));

            services.AddTransient(typeof(ViewModel.DashboardViewModel));

            // Top-Level Windows
            services.AddTransient(typeof(Dashboard));
            services.AddTransient(typeof(UI.SetupWindow));
            services.AddTransient(typeof(GolemUI.DebugWindow));

            services.AddSingleton<Command.GSB.IGsbEndpointFactory, Src.GsbEndpointFactory>();
            services.AddTransient(typeof(Command.GSB.Payment));

            services.AddLogging(logBuilder =>
            {

                logBuilder.SetMinimumLevel(LogLevel.Trace);
                logBuilder.AddFile(PathUtil.GetLocalLogPath(), opts =>
                {
                    opts.Append = true;
                    opts.MinLevel = LogLevel.Debug;
                    opts.MaxRollingFiles = 3;
                    opts.FileSizeLimitBytes = 1_000_000;
                });
                logBuilder.AddDebug();

                logBuilder.AddSentry(o =>
                {
                    o.Dsn = GolemUI.Properties.Settings.Default.SentryDsn;
                    o.Debug = true;
                    o.AttachStacktrace = true;
                    o.AutoSessionTracking = true;
                    o.IsGlobalModeEnabled = true;
                    o.TracesSampleRate = 1.0;
                });

            });

        }

        public Dashboard? GetOrCreateDashboardWindow()
        {
            if (_dashboard == null)
                _dashboard = _serviceProvider!.GetRequiredService<Dashboard>();

            if (_dashboard == null)
            {
                throw new Exception("FATAL ERROR, Dashboard object creation failed.");
            }
            return _dashboard;
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            if (!_serviceProvider.GetRequiredService<Src.SingleInstanceLock>().IsMaster)
            {
                this.Shutdown();
                return;
            }


            var remoteSettingsLoader = _serviceProvider!.GetRequiredService<Interfaces.IRemoteSettingsProvider>();

            var userSettingsLoader = _serviceProvider!.GetRequiredService<Interfaces.IUserSettingsProvider>();

            var sentryAdditionalData = _serviceProvider!.GetRequiredService<SentryAdditionalDataIngester>();
            var notificationMonitor = _serviceProvider!.GetRequiredService<Src.AppNotificationService.NotificationsMonitor>();


            sentryAdditionalData.InitContextItems();
            SentrySdk.CaptureMessage("> OnStartup", SentryLevel.Info);
            //Task.Delay(10000).ContinueWith(x => sentryAdditionalData.InitContextItems());


            var args = e.Args;
            if (args.Length > 0 && args[0] == "skip_setup")
            {
                //skip setup
            }
            else if ((args.Length > 0 && args[0] == "setup") || !userSettingsLoader.LoadUserSettings().SetupFinished)
            {
                var window = _serviceProvider!.GetRequiredService<UI.SetupWindow>();
                window.Show();
                return;
            }




            var dashboardWindow = _serviceProvider!.GetRequiredService<Dashboard>();

            dashboardWindow.Show();
#if DEBUG
            StartDebugWindow(dashboardWindow);
#endif

            _dashboard = dashboardWindow;

        }
        public void RequestClose()
        {
            if (_dashboard != null)
            {
                _dashboard.RequestClose();
            }
        }

        public void ActivateFromTray()
        {
            if (_dashboard != null)
            {
                _dashboard.OnAppReactivate(this);
            }
        }

        public void UpdateAppearance()
        {
            if (_dashboard != null)
            {
                _dashboard.UpdateAppearance();
            }
        }

        public void ShowUpdateDialog()
        {
            if (_dashboard != null)
            {
                _dashboard.ShowUpdateDialog();
            }
        }


        private void StopApp()
        {
            _serviceProvider.Dispose();

        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            StopApp();
        }

        public void StartDebugWindow(Dashboard dashboardWindow)
        {
            var debugWindow = _serviceProvider.GetRequiredService<DebugWindow>();

            if (dashboardWindow != null)
            {
                debugWindow.Owner = dashboardWindow;
                debugWindow.Left = dashboardWindow.Left + dashboardWindow.Width;
                debugWindow.Top = dashboardWindow.Top;
                debugWindow.Show();
            }
        }

    }
}
