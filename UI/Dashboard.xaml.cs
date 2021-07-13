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
using GolemUI.Notifications;

namespace GolemUI
{

    public enum DashboardPages
    {
        PageDashboardMain,
        PageDashboardSettings,
        PageDashboardAdvancedSettings,
        PageDashboardBenchmark,
        PageDashboardWallet,
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
        public DashboardAdvancedSettings DashboardAdvancedSettings { get; set; }
        public DashboardWallet DashboardWallet { get; set; }
        public DashboardBenchmark DashboardBenchmark { get; set; }
        public DashboardDetails DashboardDetails { get; set; }
        public WelcomeStart WelcomeStart { get; set; }
        public WelcomeNodeName WelcomeNodeName { get; set; }
        public WelcomeAddress WelcomeAddress { get; set; }
        public WelcomeBenchmark WelcomeBenchmark { get; set; }
        public WelcomeDecide WelcomeDecide { get; set; }
        public DashboardPages _pageSelected = DashboardPages.PageWelcomeStart;


        public Dictionary<DashboardPages, DashboardPageDescriptor> _pages = new Dictionary<DashboardPages, DashboardPageDescriptor>();

        private bool _forceExit = false;
        bool _minimizeOnly = true;


        public Dashboard(DashboardWallet _dashboardWallet, DashboardSettings _dashboardSettings)
        {
            InitializeComponent();
            
            DashboardMain = new DashboardMain();
            DashboardSettings = _dashboardSettings;
            DashboardAdvancedSettings = new DashboardAdvancedSettings();
            DashboardWallet = _dashboardWallet;
            DashboardBenchmark = new DashboardBenchmark();
            DashboardDetails = new DashboardDetails();
            WelcomeStart = new WelcomeStart();
            WelcomeNodeName = new WelcomeNodeName();
            WelcomeAddress = new WelcomeAddress();
            WelcomeBenchmark = new WelcomeBenchmark();
            WelcomeDecide = new WelcomeDecide();

            _pages.Add(DashboardPages.PageDashboardMain, new DashboardPageDescriptor(  DashboardMain));
            _pages.Add(DashboardPages.PageDashboardSettings, new DashboardPageDescriptor(DashboardSettings));
            _pages.Add(DashboardPages.PageDashboardAdvancedSettings, new DashboardPageDescriptor(DashboardAdvancedSettings));
            _pages.Add(DashboardPages.PageDashboardWallet, new DashboardPageDescriptor(DashboardWallet));
            _pages.Add(DashboardPages.PageDashboardBenchmark, new DashboardPageDescriptor(DashboardBenchmark));
            _pages.Add(DashboardPages.PageDashboardDetails, new DashboardPageDescriptor(DashboardDetails));
            _pages.Add(DashboardPages.PageWelcomeStart, new DashboardPageDescriptor(WelcomeStart));
            _pages.Add(DashboardPages.PageWelcomeNodeName, new DashboardPageDescriptor(WelcomeNodeName));
            _pages.Add(DashboardPages.PageWelcomeAddress, new DashboardPageDescriptor(WelcomeAddress));
            _pages.Add(DashboardPages.PageWelcomeBenchmark, new DashboardPageDescriptor(WelcomeBenchmark));
            _pages.Add(DashboardPages.PageWelcomeDecide, new DashboardPageDescriptor(WelcomeDecide));




            if (GlobalSettings.isDemo)
            {
                //cvMain.Children.Add(WelcomeStart);
                _pageSelected = DashboardPages.PageWelcomeStart;
            }
            else
            {
               // cvMain.Children.Add(DashboardMain);
                _pageSelected = DashboardPages.PageDashboardMain;
            }


            GlobalApplicationState.Instance.ApplicationStateChanged += OnGlobalApplicationStateChanged;


            foreach (var pair in _pages)
            {
                UserControl control = pair.Value.View;
                control.Visibility = Visibility.Hidden;
                control.Opacity = 0;
                tcNo1.Children.Add(control);
            }


            _pages[_pageSelected].View.Visibility = Visibility.Visible;
            _pages[_pageSelected].View.Opacity = 1.0f;

            /*Style s = new Style();
            s.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Collapsed));
            tcNo1.ItemContainerStyle = s;*/
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

        private void Page5Click(object sender, RoutedEventArgs e)
        {
            SwitchPage(DashboardPages.PageDashboardAdvancedSettings);
        }
        private void Page6Click(object sender, RoutedEventArgs e)
        {
            SwitchPage(DashboardPages.PageDashboardWallet);
        }


        public UserControl GetUserControlFromPage(DashboardPages page)
        {
            if (!_pages.ContainsKey(page))
            {
                throw new Exception(String.Format("Requested page not added to _pages. Page: {0}", (int) page));
            }
            return _pages[page].View;
        }

        public void SwitchPage(DashboardPages page, bool animate = true)
        {
            if (page != _pageSelected)
            {
                foreach (var pair in _pages)
                {
                    if (pair.Key != _pageSelected && pair.Key != page)
                    {
                        pair.Value.View.Visibility = Visibility.Hidden;
                        pair.Value.View.Opacity = 0.0f;
                    }
                }

                UserControl ucOld = GetUserControlFromPage(_pageSelected);
                ucOld.Opacity = 0.0f;
                ucOld.Visibility = Visibility.Hidden;


                UserControl uc = GetUserControlFromPage(page);
                if (animate)
                {
                    uc.Visibility = Visibility.Visible;
                    ShowSlowly(uc, TimeSpan.FromMilliseconds(250));
                }
                else
                {
                    uc.Visibility = Visibility.Visible;
                    uc.Opacity = 1.0f;
                }

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

        private void ShowSlowly(UserControl uc, TimeSpan duration)
        {
            DoubleAnimation animation = new DoubleAnimation(0.0, 1.0, duration);
            uc.BeginAnimation(UserControl.OpacityProperty, animation);
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

        public void RequestClose()
        {
            DashboardBenchmark.RequestBenchmarkEnd();
            if (!GlobalApplicationState.Instance.ProcessController.IsRunning)
            {
                this._forceExit = true;
                this.Close();
            }
            //there is no way for now of gently stopping provider so kill it
            GlobalApplicationState.Instance.ProcessController.KillProvider();
            Task.Run(async () =>
            {
                //try to gently stop yagna
                await GlobalApplicationState.Instance.ProcessController.StopYagna();
                this.Dispatcher.Invoke(() =>
                {
                    //force exit know after trying to gently stop yagna (which may succeeded or failed)
                    this._forceExit = true;
                    this.Close();
                });
            });

            //wait for yagna until it is stopped asynchronously
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Note: this event is called one or twice during normal shutdown
            if (_forceExit)
            {
                //if somehow yagna failed to close itself gently just kill it
                GlobalApplicationState.Instance.ProcessController.KillProvider();
                GlobalApplicationState.Instance.ProcessController.KillYagna();
                return;
            }
            if (_minimizeOnly)
            {
                tbNotificationIcon.Visibility = Visibility.Visible;
                e.Cancel = true;
                this.WindowState = WindowState.Minimized;
            }
            e.Cancel = true;

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
