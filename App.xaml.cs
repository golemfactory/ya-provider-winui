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

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(10);
            timer.Tick += timer_Tick;
            timer.Start();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            GlobalApplicationState.Instance.NotifyApplicationStateChanged(this, GlobalApplicationStateAction.timerEvent);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            //services.AddSingleton<Interfaces.IProcessControler, ProcessController>();
#if DEBUG
            services.AddSingleton<DebugWindow>();
#endif
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {

            
            try
            {
                var dashboardWindow = new Dashboard();
                dashboardWindow.Show();


#if DEBUG
                var debugWindow = new DebugWindow();
                if (debugWindow == null)
                {
                    throw new Exception("Debug window");
                }
#endif

#if DEBUG
                debugWindow.Owner = dashboardWindow;
                debugWindow.Left = dashboardWindow.Left + dashboardWindow.Width;
                debugWindow.Top = dashboardWindow.Top;
                debugWindow.Show();
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



    }
}
