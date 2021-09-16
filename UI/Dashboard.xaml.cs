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

using GolemUI.Notifications;
using GolemUI.Interfaces;
using GolemUI.Src;
using GolemUI.UI;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using GolemUI.Model;
using GolemUI.ViewModel;
using GolemUI.ViewModel.CustomControls;
using GolemUI.Src.AppNotificationService;

namespace GolemUI
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : Window
    {
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        public ViewModel.DashboardViewModel ViewModel { get; set; }


        public Dashboard(DashboardSettingsAdv _dashboardSettingsAdv, INotificationService notificationService, IUserFeedbackService userFeedback, Interfaces.IProcessController processController, Src.SingleInstanceLock singleInstanceLock, Interfaces.IProviderConfig providerConfig, Src.BenchmarkService benchmarkService, Interfaces.IUserSettingsProvider userSettingsProvider, ViewModel.DashboardViewModel dashboardViewModel, NotificationBarViewModel notificationViewModel)
        {
            _notificationService = notificationService;
            _userFeedback = userFeedback;
            _processController = processController;
            _providerConfig = providerConfig;
            _benchmarkService = benchmarkService;
            _userSettingsProvider = userSettingsProvider;

            InitializeComponent();
            ViewModel = dashboardViewModel;
            this.DataContext = this.ViewModel;

            foreach (var pair in ViewModel._pages)
            {
                UserControl control = pair.Value.View;
                grdPagesContainer.Children.Add(control);
            }

            singleInstanceLock.ActivateEvent += OnAppReactivate;

            ViewModel.SwitchPage(DashboardViewModel.DashboardPages.PageDashboardMain);
            this.NB.SetViewModel(notificationViewModel);
        }

        private void PageChangeRequested(DashboardViewModel.DashboardPages page)
        {
            ViewModel.SwitchPage(page);
        }

        public void UpdateAppearance()
        {
            var us = _userSettingsProvider.LoadUserSettings();
            if (us.Opacity < 100)
            {
                EnableBlur();
            }

            SolidColorBrush b = (SolidColorBrush)this.Resources["SetupWindow.Background"];
            {
                int op = (int)us.Opacity;
                if (op >= 100)
                {
                    b.Opacity = 1.0;
                }
                else if (op <= 1)
                {
                    b.Opacity = 0.01;
                }
                else
                {
                    b.Opacity = op / 100.0;
                }
            }
            this.Resources["SetupWindow.Background"] = b;
        }



        public void OnAppReactivate(object sender)
        {
            Dispatcher.Invoke(() =>
            {
                WindowState = WindowState.Normal;
                ShowInTaskbar = true;
                Activate();
            });
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

        private async Task<bool> RequestCloseAsync()
        {
            //Disable logging in debugwindow to prevent problems with invoke 
            DebugWindow.EnableLoggingToDebugWindow = false;

            //Stop provider
            await _processController.Stop();

            //force exit know after trying to gently stop yagna (which may succeeded or failed)
            this._forceExit = true;
            this.Close();
            return false;
        }

        private bool _forceExit = false;
        public void RequestClose()
        {
            Task.Run(() => this.Dispatcher.Invoke(async () => await RequestCloseAsync()));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_forceExit)
            {
                return;
            }

            bool closeOnExit = _userSettingsProvider.LoadUserSettings().CloseOnExit;
            if (closeOnExit)
            {
                RequestClose();
            }
            else
            {
                tbNotificationIcon.Visibility = Visibility.Visible;
                if (_userSettingsProvider.LoadUserSettings().NotificationsEnabled)
                    tbNotificationIcon.ShowBalloonTip("Beta Miner is still running in tray", "To close application use Beta Miner's tray's context menu.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
                this.WindowState = WindowState.Minimized;
                this.ShowInTaskbar = false;
            }
            e.Cancel = true;
        }

        internal void EnableBlur()
        {
            if (!_blurEffectApplied)
            {
                _blurEffectApplied = true;

                var windowHelper = new WindowInteropHelper(this);

                var accent = new AccentPolicy();
                accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

                var accentStructSize = Marshal.SizeOf(accent);

                var accentPtr = Marshal.AllocHGlobal(accentStructSize);
                Marshal.StructureToPtr(accent, accentPtr, false);

                var data = new WindowCompositionAttributeData();
                data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
                data.SizeOfData = accentStructSize;
                data.Data = accentPtr;

                SetWindowCompositionAttribute(windowHelper.Handle, ref data);

                Marshal.FreeHGlobal(accentPtr);
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateAppearance();

            await Task.WhenAll(
                _providerConfig.Prepare(_benchmarkService.IsClaymoreMiningPossible),
                _processController.Prepare()
            );

            if (_providerConfig.IsMiningActive && _userSettingsProvider.LoadUserSettings().StartWithWindows)
            {
                var extraClaymoreParams = _benchmarkService.ExtractClaymoreParams();
                await _processController.Start(_providerConfig.Network, extraClaymoreParams);
            }
        }

        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            UserSettings ls = _userSettingsProvider.LoadUserSettings();

            if (ls.MinimizeToTrayOnMinimize)
            {
                tbNotificationIcon.Visibility = Visibility.Visible;
                if (_userSettingsProvider.LoadUserSettings().NotificationsEnabled)
                    tbNotificationIcon.ShowBalloonTip("Beta Miner is still running in tray", "To close application use Beta Miner's tray's context menu.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
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

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        //apply blur only once
        private bool _blurEffectApplied = false;

        private readonly Interfaces.IProcessController _processController;
        private readonly IProviderConfig _providerConfig;
        private readonly BenchmarkService _benchmarkService;
        private readonly IUserSettingsProvider _userSettingsProvider;
        private readonly IUserFeedbackService _userFeedback;
        private readonly INotificationService _notificationService;
        private void btnAppInformation_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new UI.Dialogs.DlgAppInfo(new ViewModel.Dialogs.DlgAppInfoViewModel(_providerConfig, _userFeedback));
            dlg.Owner = Window.GetWindow(this);
            ViewModel.DarkBackgroundVisible = true;
            bool? result = dlg?.ShowDialog();
            if (result == true)
            {
                _notificationService.PushNotification(new SimpleNotificationObject(Src.AppNotificationService.Tag.AppStatus, "thank you for your feedback.", expirationTimeInMs: 7000, group: false));
            }
            ViewModel.DarkBackgroundVisible = false;
        }


        public void ShowUpdateDialog()
        {
            ViewModel.DarkBackgroundVisible = true;
            ViewModel.ShowVersionDialog(Window.GetWindow(this));
            ViewModel.DarkBackgroundVisible = false;
        }

        private void TxtVersion_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.DarkBackgroundVisible = true;
            ViewModel.ShowVersionDialog(Window.GetWindow(this));
            ViewModel.DarkBackgroundVisible = false;
        }

    }
}
