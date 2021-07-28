﻿#nullable enable
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
using GolemUI.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace GolemUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private readonly ServiceProvider _serviceProvider;
        private readonly GolemUI.ChildProcessManager _childProcessManager;

        public App()
        {
            _childProcessManager = new GolemUI.ChildProcessManager();
            _childProcessManager.AddProcess(Process.GetCurrentProcess());


            GlobalApplicationState.Initialize();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
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

            services.AddLogging(logBuilder => {
                logBuilder.AddConsole();
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

            var args = e.Args;
            if ((args.Length > 0 && args[0] == "setup") || !GolemUI.Properties.Settings.Default.Configured)
            {
                var window = _serviceProvider!.GetRequiredService<UI.SetupWindow>();
                window.Show();
                return;
            }


            try
            {
                var dashboardWindow = _serviceProvider!.GetRequiredService<Dashboard>();
                if (GlobalApplicationState.Instance != null)
                {
                    GlobalApplicationState.Instance.Dashboard = dashboardWindow;

                    dashboardWindow.Show();
#if DEBUG
                    StartDebugWindow();
#endif
                }
                else
                {
                    throw new NullReferenceException("GlobalApplicationState.Instance should not be null! ");

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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

        public void StartDebugWindow()
        {

            if (GlobalApplicationState.Instance?.DebugWindow == null)
            {
                var debugWindow = _serviceProvider.GetRequiredService<DebugWindow>();
                if (GlobalApplicationState.Instance != null)
                {
                    GlobalApplicationState.Instance.DebugWindow = debugWindow;

                    var dashboard = GlobalApplicationState.Instance.Dashboard;
                    if (dashboard != null)
                    {
                        debugWindow.Owner = dashboard;
                        debugWindow.Left = dashboard.Left + dashboard.Width;
                        debugWindow.Top = dashboard.Top;
                        debugWindow.Show();
                    }

                }
            }
        }

    }
}
