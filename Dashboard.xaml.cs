using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Windows.Media.Animation;
using GolemUI.Settings;

namespace GolemUI
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : Window
    {
        public DashboardMain DashboardMain { get; set; }
        public DashboardSettings DashboardSettings { get; set; }
        public DashboardBenchmark DashboardBenchmark { get; set; }
        public DashboardDetails DashboardDetails { get; set; }

        public int _pageSelected = 0;

        public Dashboard()
        {
            InitializeComponent();

            DashboardMain = new DashboardMain();
            DashboardSettings = new DashboardSettings();
            DashboardBenchmark = new DashboardBenchmark();
            DashboardDetails = new DashboardDetails();
            cvMain.Children.Add(DashboardMain);


            GlobalApplicationState.Instance.ApplicationStateChanged += OnGlobalApplicationStateChanged;

            //this.WindowStyle = WindowStyle.None;
            //this.ResizeMode = ResizeMode.NoResize;
        }

        private void Page1Click(object sender, RoutedEventArgs e)
        {
            /*var position = 0;
            var amount = 500;
            TimeSpan duration = new TimeSpan(1000);
            if (double.IsNaN(position)) position = 0;
            var animation =
                new DoubleAnimation
                {
                    // fine-tune animation here
                    From = position,
                    To = position + amount,
                    Duration = new Duration(duration),
                };
            Storyboard.SetTarget(animation, svName);
            svName.set*/
            //AnimateScroll(cvMain, 500, TimeSpan.FromMilliseconds(500));
            //cvMain.Visibility = Visibility.Visible;
            if (_pageSelected != 0)
            {
                cvMain.Children.Clear();
                cvMain.Children.Add(DashboardMain);
                _pageSelected = 0;
            }
        }

        private void Page2Click(object sender, RoutedEventArgs e)
        {
            //AnimateScroll(cvMain, -500, TimeSpan.FromMilliseconds(500));
            if (_pageSelected != 1)
            {
                cvMain.Children.Clear();
                cvMain.Children.Add(DashboardSettings);
                _pageSelected = 1;
            }
            //DashboardSettings.Opacity = 0.5f;
        }

        private void Page3Click(object sender, RoutedEventArgs e)
        {
            if (_pageSelected != 2)
            {
                cvMain.Children.Clear();
                cvMain.Children.Add(DashboardBenchmark);
                _pageSelected = 2;
            }
        }

        private void Page4Click(object sender, RoutedEventArgs e)
        {
            if (_pageSelected != 3)
            {
                cvMain.Children.Clear();
                cvMain.Children.Add(DashboardDetails);
                _pageSelected = 3;
            }
        }

        public void BlockNavigation()
        {
            btnPage1.IsEnabled = false;
            btnPage2.IsEnabled = false;
            btnPage3.IsEnabled = false;
            btnPage4.IsEnabled = false;
        }
        public void ResumeNavigation()
        {
            btnPage1.IsEnabled = true;
            btnPage2.IsEnabled = true;
            btnPage3.IsEnabled = true;
            btnPage4.IsEnabled = true;
        }

        public void OnGlobalApplicationStateChanged(object sender, GlobalApplicationStateChangedArgs? args)
        {
            if (args != null)
            {
                switch (args.action)
                {
                    case GlobalApplicationStateAction.yagnaAppStarting:
                        BlockNavigation();
                        break;
                    case GlobalApplicationStateAction.yagnaAppStopped:
                        ResumeNavigation();
                        break;
                    case GlobalApplicationStateAction.yagnaAppStarted:
                        ResumeNavigation();
                        break;
                }
            }
        }

        static void AnimateScroll(UIElement element, double amount, TimeSpan duration)
        {
            var sb = new Storyboard();
            var position = Canvas.GetTop(element);
            if (double.IsNaN(position)) position = 0;
            var animation =
                new DoubleAnimation
                {
                    // fine-tune animation here
                    From = position,
                    To = position + amount,
                    Duration = new Duration(duration),
                };
            Storyboard.SetTarget(animation, element);
            Storyboard.SetTargetProperty(animation, new PropertyPath(Canvas.TopProperty));
            sb.Children.Add(animation);
            sb.Begin();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DashboardBenchmark.RequestBenchmarkEnd();
        }


    }
}
