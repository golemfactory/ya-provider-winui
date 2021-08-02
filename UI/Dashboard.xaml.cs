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


        public Dashboard(DashboardSettingsAdv _dashboardSettingsAdv, Interfaces.IProcessControler processControler, Src.SingleInstanceLock singleInstanceLock, Interfaces.IProviderConfig providerConfig, Src.BenchmarkService benchmarkService, Interfaces.IUserSettingsProvider userSettingsProvider, ViewModel.DashboardViewModel dashboardViewModel)
        {
            _processControler = processControler;
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
        }

        private void PageChangeRequested(DashboardViewModel.DashboardPages page)
        {
            ViewModel.SwitchPage(page);
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

        private bool _forceExit = false;
        public void RequestClose()
        {
            Task.Run(async () =>
            {
                //Disable logging in debugwindow to prevent problems with invoke 
                DebugWindow.EnableLoggingToDebugWindow = false;

                //Stop provider
                await _processControler.Stop();

                this.Dispatcher.Invoke(() =>
                {
                    //force exit know after trying to gently stop yagna (which may succeeded or failed)
                    this._forceExit = true;
                    this.Close();
                });
            });
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
                this.WindowState = WindowState.Minimized;
                this.ShowInTaskbar = false;
            }
            e.Cancel = true;
        }

        internal void EnableBlur()
        {
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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EnableBlur();
            await Task.WhenAll(
                _providerConfig.Prepare(_benchmarkService.IsClaymoreMiningPossible),
                _processControler.Prepare()
            );
        }

        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            UserSettings ls = _userSettingsProvider.LoadUserSettings();

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

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private readonly Interfaces.IProcessControler _processControler;
        private readonly IProviderConfig _providerConfig;
        private readonly BenchmarkService _benchmarkService;
        private readonly IUserSettingsProvider _userSettingsProvider;

        private void btnAppInformation_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new UI.Dialogs.DlgAppInfo(new ViewModel.Dialogs.DlgAppInfoViewModel(_providerConfig));
            dlg.Owner = Window.GetWindow(this);
            RectBlack.Visibility = Visibility.Visible;
            dlg?.ShowDialog();
            RectBlack.Visibility = Visibility.Hidden;
        }
    }
}
