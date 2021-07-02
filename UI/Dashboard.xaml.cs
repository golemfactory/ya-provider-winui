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
using System.Diagnostics;

using System.Windows.Media.Animation;
using GolemUI.Settings;

namespace GolemUI
{

    public enum DashboardPages
    {
        PageDashboardMain,
        PageDashboardSettings,
        PageDashboardBenchmark,
        PageDashboardDetails,
        PageWelcomeStart,
        PageWelcomeDecide,
        PageWelcomeNodeName,
        PageWelcomeAddress,
        PageWelcomeBenchmark,
    }

    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : Window
    {
        public DashboardMain DashboardMain { get; set; }
        public DashboardSettings DashboardSettings { get; set; }
        public DashboardBenchmark DashboardBenchmark { get; set; }
        public DashboardDetails DashboardDetails { get; set; }
        public WelcomeStart WelcomeStart { get; set; }
        public WelcomeNodeName WelcomeNodeName { get; set; }
        public WelcomeAddress WelcomeAddress { get; set; }
        public WelcomeBenchmark WelcomeBenchmark { get; set; }
        public WelcomeDecide WelcomeDecide { get; set; }
        public DashboardPages _pageSelected = DashboardPages.PageWelcomeStart;

        public Dashboard()
        {
            InitializeComponent();
            
            DashboardMain = new DashboardMain();
            DashboardSettings = new DashboardSettings();
            DashboardBenchmark = new DashboardBenchmark();
            DashboardDetails = new DashboardDetails();
            WelcomeStart = new WelcomeStart();
            WelcomeNodeName = new WelcomeNodeName();
            WelcomeAddress = new WelcomeAddress();
            WelcomeBenchmark = new WelcomeBenchmark();
            WelcomeDecide = new WelcomeDecide();
            if (GlobalSettings.isDemo)
            {
                cvMain.Children.Add(WelcomeStart);
                _pageSelected = DashboardPages.PageWelcomeStart;
            }
            else
            {
                cvMain.Children.Add(DashboardMain);
                _pageSelected = DashboardPages.PageDashboardMain;
            }


            GlobalApplicationState.Instance.ApplicationStateChanged += OnGlobalApplicationStateChanged;

            //this.WindowStyle = WindowStyle.None;
            //this.ResizeMode = ResizeMode.NoResize;
        }


        private void Page1Click(object sender, RoutedEventArgs e)
        {
            SwitchPage(DashboardPages.PageDashboardMain);
        }

        private void Page2Click(object sender, RoutedEventArgs e)
        {
            SwitchPage(DashboardPages.PageDashboardSettings);
        }

        private void Page3Click(object sender, RoutedEventArgs e)
        {
            SwitchPage(DashboardPages.PageDashboardBenchmark);
        }

        private void Page4Click(object sender, RoutedEventArgs e)
        {
            SwitchPage(DashboardPages.PageDashboardDetails);
        }

        public UserControl GetUserControlFromPage(DashboardPages page)
        {
            switch (page)
            {
                case DashboardPages.PageDashboardMain:
                    return DashboardMain;
                case DashboardPages.PageDashboardSettings:
                    return DashboardSettings;
                case DashboardPages.PageDashboardBenchmark:
                    return DashboardBenchmark;
                case DashboardPages.PageDashboardDetails:
                    return DashboardDetails;
                case DashboardPages.PageWelcomeStart:
                    return WelcomeStart;
                case DashboardPages.PageWelcomeDecide:
                    return WelcomeDecide;
                case DashboardPages.PageWelcomeNodeName:
                    return WelcomeNodeName;
                case DashboardPages.PageWelcomeAddress:
                    return WelcomeAddress;
                case DashboardPages.PageWelcomeBenchmark:
                    return WelcomeBenchmark;
                default:
                    throw new Exception("GetUserControlFromPage: Page not found");
            }
        }

        public void SwitchPage(DashboardPages page)
        {
            if (_pageSelected != page)
            {
                cvMain.Children.Clear();
                cvMain.Children.Add(GetUserControlFromPage(page));

                _pageSelected = page;
            }
        }

        public void BlockNavigation()
        {
            btnPage1.IsEnabled = false;
            btnPage2.IsEnabled = false;
            btnPage3.IsEnabled = false;
            //btnPage4.IsEnabled = false;
        }
        public void ResumeNavigation()
        {
            btnPage1.IsEnabled = true;
            btnPage2.IsEnabled = true;
            btnPage3.IsEnabled = true;
            //btnPage4.IsEnabled = true;
        }

        public void OnGlobalApplicationStateChanged(object sender, GlobalApplicationStateEventArgs? args)
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
                    case GlobalApplicationStateAction.benchmarkStarted:
                        BlockNavigation();
                        break;
                    case GlobalApplicationStateAction.benchmarkStopped:
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
            GlobalApplicationState.Instance.ProcessController.KillProvider();
            GlobalApplicationState.Instance.ProcessController.KillYagna();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Process[] yagnaProcesses;
            Process[] providerProcesses;
            Process[] claymoreProcesses;

            ProcessMonitor.GetProcessList(out yagnaProcesses, out providerProcesses, out claymoreProcesses);
            if (yagnaProcesses.Length > 0 || providerProcesses.Length > 0 || claymoreProcesses.Length > 0)
            {
                ExistingProcessesWindow w = new ExistingProcessesWindow();
                w.Owner = this;
                var dialogResult = w.ShowDialog();
                switch (dialogResult)
                {
                    case true:
                        // User accepted dialog box
                        break;
                    case false:
                        // User canceled dialog box
                        return;
                    default:
                        // Indeterminate
                        break;
                }
            }
        }

        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TitleBar_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
