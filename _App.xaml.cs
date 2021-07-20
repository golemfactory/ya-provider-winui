using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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


        public App()
        {
            GlobalApplicationState.Initialize();

            GlobalApplicationState.Instance.ApplicationStateChanged += OnGlobalApplicationStateChanged;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(10);
            timer.Tick += timer_Tick;
            timer.Start();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        void timer_Tick(object? sender, EventArgs e)
        {
            GlobalApplicationState.Instance.NotifyApplicationStateChanged(this, GlobalApplicationStateAction.timerEvent);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<Interfaces.IPriceProvider, Src.StaticPriceProvider>();
            services.AddSingleton<Interfaces.IEstimatedProfitProvider, Src.StaticEstimatedEarningsProvider>();

            services.AddSingleton(typeof(Interfaces.IProcessControler), GlobalApplicationState.Instance.ProcessController);
            services.AddSingleton(typeof(Command.YagnaSrv));
            services.AddSingleton(typeof(Command.Provider));

            services.AddSingleton(Command.Network.Rinkeby);
            services.AddSingleton<Interfaces.IPaymentService, Src.PaymentService>();
            services.AddSingleton<Interfaces.IProviderConfig, Src.ProviderConfigService>();

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

        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var args = e.Args;
            if (args.Length > 0 && args[0] == "setup")
            {
                var window = _serviceProvider.GetRequiredService<UI.SetupWindow>();
                window.Show();
                return;
            }


            try
            {
                var dashboardWindow = _serviceProvider.GetRequiredService<Dashboard>();
                GlobalApplicationState.Instance.Dashboard = dashboardWindow;

                dashboardWindow.Show();

#if DEBUG
                GlobalApplicationState.Instance.NotifyApplicationStateChanged(this, GlobalApplicationStateAction.startDebugWindow);
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }





        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            _serviceProvider.Dispose();
            GlobalApplicationState.Finish();
        }

        public void OnGlobalApplicationStateChanged(object sender, GlobalApplicationStateEventArgs? args)
        {
            if (args != null)
            {
                switch (args.action)
                {
                    case GlobalApplicationStateAction.startDebugWindow:
                        if (GlobalApplicationState.Instance.DebugWindow == null)
                        {
                            var debugWindow = new DebugWindow();
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
                        break;
                    case GlobalApplicationStateAction.debugWindowClosed:
                        GlobalApplicationState.Instance.DebugWindow = null;
                        break;
                }
            }
        }
    }
}
