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
        private readonly IDisposable _sentrySdk;

        private Dashboard? _dashboard;
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            _sentrySdk = (SentrySdk.Init(o =>
              {

                  o.Dsn = GolemUI.Properties.Settings.Default.SentryDsn;
                  o.Debug = true; //todo: change to false for production release
                  o.TracesSampleRate = 1.0; //todo: probably should change in future ?
              }));

            SentrySdk.ConfigureScope(scope =>
            {
                scope.Contexts["user_data"] = new
                {
                    UserName = Environment.UserName
                };
            });



            _childProcessManager = new GolemUI.ChildProcessManager();
            _childProcessManager.AddProcess(Process.GetCurrentProcess());


            //GlobalApplicationState.Initialize();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
            SentrySdk.CaptureMessage("> App constructor", SentryLevel.Info);

        }
        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            SentrySdk.CaptureException(e.Exception);

            //TODO: to discuss if we should allow the app to crash or not
            //e.Handled = true;
        }
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<Interfaces.IBenchmarkResultsProvider, Src.BenchmarkResultsProvider>();
            services.AddSingleton<Interfaces.IUserSettingsProvider, Src.UserSettingsProvider>();
            services.AddSingleton<Interfaces.IPriceProvider, Src.CoinGeckoPriceProvider>();
            services.AddSingleton<Interfaces.IEstimatedProfitProvider, Src.StaticEstimatedEarningsProvider>();

            services.AddSingleton(typeof(Interfaces.IProcessControler), typeof(GolemUI.ProcessController));
            services.AddSingleton(typeof(Command.YagnaSrv));
            services.AddSingleton(typeof(Command.Provider));
            services.AddSingleton(cfg => new Src.SingleInstanceLock());

            services.AddSingleton(GolemUI.Properties.Settings.Default.TestNet ? Network.Rinkeby : Network.Mainnet);
            services.AddSingleton<Interfaces.IPaymentService, Src.PaymentService>();
            services.AddSingleton<Interfaces.IProviderConfig, Src.ProviderConfigService>();
            services.AddSingleton<Src.BenchmarkService>();


            services.AddTransient(typeof(SentryAdditionalDataIngester));
            services.AddTransient(typeof(DashboardWallet));
            services.AddTransient(typeof(ViewModel.WalletViewModel));
            services.AddTransient(typeof(ViewModel.DashboardMainViewModel));
            services.AddTransient(typeof(ViewModel.SetupViewModel));

            services.AddTransient(typeof(DashboardMain));
            services.AddTransient(typeof(DashboardSettings));
            services.AddTransient(typeof(SettingsViewModel));

            // Top-Level Windows
            services.AddTransient(typeof(Dashboard));
            services.AddTransient(typeof(UI.SetupWindow));
            services.AddTransient(typeof(GolemUI.DebugWindow));

            services.AddLogging(logBuilder =>
            {
                logBuilder.AddDebug();
                logBuilder.SetMinimumLevel(LogLevel.Trace);
                logBuilder.AddSentry(GolemUI.Properties.Settings.Default.SentryDsn);
            });

        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            if (!_serviceProvider.GetRequiredService<Src.SingleInstanceLock>().IsMaster)
            {
                this.Shutdown();
                return;
            }
            var userSettingsLoader = _serviceProvider!.GetRequiredService<Interfaces.IUserSettingsProvider>();
            var sentryAdditionalData = _serviceProvider!.GetRequiredService<SentryAdditionalDataIngester>();
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


            try
            {

                var dashboardWindow = _serviceProvider!.GetRequiredService<Dashboard>();

                dashboardWindow.Show();
#if DEBUG
                StartDebugWindow(dashboardWindow);
#endif

                _dashboard = dashboardWindow;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
