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
using GolemUI.Controllers;

namespace GolemUI
{
    public enum DashboardPages
    {
        PageDashboardMain,
        PageDashboardSettings,
        PageDashboardAdvancedSettings,
        PageDashboardBenchmark,
        PageDashboardWallet,
        PageDashboardDetails
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

        public DashboardPages _pageSelected = DashboardPages.PageDashboardMain;

        public DashboardPages LastPage { get; set; }


        public Dictionary<DashboardPages, DashboardPage> _pages = new Dictionary<DashboardPages, DashboardPage>();

        public Dashboard(DashboardWallet _dashboardWallet, DashboardSettings _dashboardSettings, DashboardMain dashboardMain,
            Interfaces.IProcessControler processControler, Src.SingleInstanceLock singleInstanceLock)
        {
            _processControler = processControler;

            InitializeComponent();

            DashboardMain = dashboardMain;
            DashboardSettings = _dashboardSettings;
            DashboardAdvancedSettings = new DashboardAdvancedSettings();
            DashboardWallet = _dashboardWallet;
            DashboardBenchmark = new DashboardBenchmark();
            DashboardDetails = new DashboardDetails();


            _pages.Add(DashboardPages.PageDashboardMain, new DashboardPage(DashboardMain, DashboardMain.Model));
            _pages.Add(DashboardPages.PageDashboardSettings, new DashboardPage(DashboardSettings, DashboardSettings.ViewModel));
            _pages.Add(DashboardPages.PageDashboardAdvancedSettings, new DashboardPage(DashboardAdvancedSettings));
            _pages.Add(DashboardPages.PageDashboardWallet, new DashboardPage(DashboardWallet));
            _pages.Add(DashboardPages.PageDashboardBenchmark, new DashboardPage(DashboardBenchmark));
            _pages.Add(DashboardPages.PageDashboardDetails, new DashboardPage(DashboardDetails));
            _pageSelected = DashboardPages.PageDashboardMain;

            dashboardMain.Model.LoadData();

            GlobalApplicationState.Instance.Dashboard = this;

            foreach (var pair in _pages)
            {
                UserControl control = pair.Value.View;
                control.Visibility = Visibility.Hidden;
                control.Opacity = 0;
                tcNo1.Children.Add(control);
            }


            _pages[_pageSelected].View.Visibility = Visibility.Visible;
            _pages[_pageSelected].View.Opacity = 1.0f;

            singleInstanceLock.ActivateEvent += OnAppReactivate;
        }

        private void OnAppReactivate(object sender)
        {
            Dispatcher.Invoke(() =>
            {
                WindowState = WindowState.Normal;
                ShowInTaskbar = true;
                tbNotificationIcon.Visibility = Visibility.Hidden;
                Activate();
            });
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



        public DashboardPage GetPageDescriptorFromPage(DashboardPages page)
        {
            if (!_pages.ContainsKey(page))
            {
                throw new Exception(String.Format("Requested page not added to _pages. Page: {0}", (int)page));
            }
            return _pages[page];
        }

        public void SwitchPageBack()
        {
            SwitchPage(LastPage);
        }

        public void SwitchPage(DashboardPages page)
        {
            if (page == _pageSelected) return;

            _pages.ToList().Where(x => x.Key != _pageSelected && x.Key != page).ToList().ForEach(x => x.Value.Clear());

            var lastPage = GetPageDescriptorFromPage(_pageSelected);
            lastPage.Unmount();
            lastPage.Hide();

            var currentPage = GetPageDescriptorFromPage(page);
            currentPage.Mount();
            currentPage.Show();


            if (page == DashboardPages.PageDashboardBenchmark)
            {
                brdNavigation.Visibility = Visibility.Collapsed;
                grdMain.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Pixel);
            }
            else
            {
                brdNavigation.Visibility = Visibility.Visible;
                grdMain.ColumnDefinitions[0].Width = new GridLength(120, GridUnitType.Pixel);
            }

            LastPage = _pageSelected;

            _pageSelected = page;
        }

        public void BlockNavigation()
        {
            btnPage1.IsEnabled = false;
            btnPage2.IsEnabled = false;
            //btnPage4.IsEnabled = false;
        }
        public void ResumeNavigation()
        {
            btnPage1.IsEnabled = true;
            btnPage2.IsEnabled = true;
            //btnPage4.IsEnabled = true;
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

        private bool _forceExit = false;
        public void RequestClose(bool isAlreadyClosing = false)
        {
            if (_processControler.IsProviderRunning)
            {
                _processControler.Stop();
            }
            if (!isAlreadyClosing)
            {
                _forceExit = true;
                Close();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_forceExit)
            {
                return;
            }

            LocalSettings ls = SettingsLoader.LoadSettingsFromFileOrDefault();
            if (ls.CloseOnExit)
            {
                RequestClose(true);
            }
            else
            {
                tbNotificationIcon.Visibility = Visibility.Visible;
                this.WindowState = WindowState.Minimized;
                this.ShowInTaskbar = false;
            }
            e.Cancel = true;

        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await _processControler.Prepare();
        }

        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            LocalSettings ls = SettingsLoader.LoadSettingsFromFileOrDefault();

            if (ls.MinimizeToTrayOnMinimize)
            {
                tbNotificationIcon.Visibility = Visibility.Visible;
                this.WindowState = WindowState.Minimized;
                this.ShowInTaskbar = false;
            }
            else
            {
                this.WindowState = WindowState.Minimized;
            }
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

        private readonly Interfaces.IProcessControler _processControler;
    }
}
