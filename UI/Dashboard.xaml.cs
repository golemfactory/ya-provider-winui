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
using GolemUI.Miners;
using GolemUI.Model;
using GolemUI.ViewModel;
using GolemUI.ViewModel.CustomControls;
using GolemUI.Src.AppNotificationService;
using GolemUI.DesignViewModel;
using static GolemUI.ViewModel.DashboardViewModel;
using GolemUI.Miners.Phoenix;
using GolemUI.Miners.TRex;

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
        private PhoenixMiner _phoenixMiner;
        private TRexMiner _trexMiner;

        public Dashboard(DashboardSettingsAdv _dashboardSettingsAdv, INotificationService notificationService, IUserFeedbackService userFeedback, Interfaces.IProcessController processController, Src.SingleInstanceLock singleInstanceLock, Interfaces.IProviderConfig providerConfig, Src.BenchmarkService benchmarkService, Interfaces.IUserSettingsProvider userSettingsProvider, ViewModel.DashboardViewModel dashboardViewModel, NotificationBarViewModel notificationViewModel,
            PhoenixMiner phoenixMiner, TRexMiner trexMiner)
        {
            _phoenixMiner = phoenixMiner;
            _trexMiner = trexMiner;

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

            ViewModel.SwitchPage(DashboardPages.PageDashboardMain);
            this.NB.SetViewModel(notificationViewModel);
        }

        private void PageChangeRequested(DashboardPages page)
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
                    tbNotificationIcon.ShowBalloonTip("Thorg Miner is still running in tray", "To close application use Thorg Miner's tray's context menu.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
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

            bool isLowMemoryMode = _userSettingsProvider.LoadUserSettings().ForceLowMemoryMode || (_benchmarkService.Status?.LowMemoryMode ?? false);

            await Task.WhenAll(
                _providerConfig.Prepare(_benchmarkService.IsPhoenixMiningPossible, isLowMemoryMode),
                _processController.Prepare()
            );


            if (_providerConfig.IsMiningActive && _userSettingsProvider.LoadUserSettings().StartWithWindows)
            {
                if (_benchmarkService.ActiveMinerApp != null)
                {
                    MinerAppConfiguration minerAppConfiguration = new MinerAppConfiguration();
                    minerAppConfiguration.MiningMode = isLowMemoryMode ? "ETC" : "ETH";
                    await _processController.Start(_providerConfig.Network, _benchmarkService.ActiveMinerApp, minerAppConfiguration);
                }
            }


        }

        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            UserSettings ls = _userSettingsProvider.LoadUserSettings();

            if (ls.MinimizeToTrayOnMinimize)
            {
                tbNotificationIcon.Visibility = Visibility.Visible;
                if (_userSettingsProvider.LoadUserSettings().NotificationsEnabled)
                    tbNotificationIcon.ShowBalloonTip("Thorg Miner is still running in tray", "To close application use Thorg Miner's tray's context menu.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
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
            var dlg = new UI.Dialogs.DlgAppInfo(new ViewModel.Dialogs.DlgAppInfoViewModel(_providerConfig, _userFeedback, _userSettingsProvider));
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

        private void DashboardWindow_Closed(object sender, EventArgs e)
        {
            ViewModel.ChangeWindowState(MainWindowState.Closed);
        }

        private void DashboardWindow_StateChanged(object sender, EventArgs e)
        {
            MainWindowState state = WindowState switch
            {
                WindowState.Minimized => MainWindowState.Minimized,
                WindowState.Normal => MainWindowState.Normal,
                WindowState.Maximized => MainWindowState.Maximized,
                _ => MainWindowState.Normal
            };
            ViewModel.ChangeWindowState(state);


            if (WindowState == WindowState.Maximized) // i guess it is more self explanatory then xaml equivalent 
                MaximizeButton.Style = Resources["DeMaximizeWindowButton"] as Style;
            else
                MaximizeButton.Style = Resources["MaximizeWindowButton"] as Style;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }
    }
}
