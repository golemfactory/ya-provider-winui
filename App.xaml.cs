using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<Interfaces.IProcessControler, Services.ProcessController>();
            services.AddSingleton<MainWindow>();
#if DEBUG
            services.AddSingleton<DebugWindow>();
#endif
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {

            
            try
            {
                var dashboardWindows = new Dashboard();
                dashboardWindows.Show();

                var mainWindow = _serviceProvider.GetService<MainWindow>();
                if (mainWindow == null)
                {
                    throw new Exception("Main window not found");
                }
                mainWindow.Left = 50;
                mainWindow.Top = 50;

#if DEBUG
                var debugWindow = _serviceProvider.GetService<DebugWindow>();
                if (debugWindow == null)
                {
                    throw new Exception("Debug window");
                }
                mainWindow.DebugWindow = debugWindow;
#endif

                mainWindow.Show();
#if DEBUG
                debugWindow.Owner = mainWindow;
                debugWindow.Left = mainWindow.Left + mainWindow.Width;
                debugWindow.Top = mainWindow.Top;
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
            GlobalApplicationState.Finalize();
        }



    }
}
