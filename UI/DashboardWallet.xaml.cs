using GolemUI.Interfaces;
using GolemUI.Src.AppNotificationService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GolemUI
{
    /// <summary>
    /// Interaction logic for DashboardSettings.xaml
    /// </summary>
    public partial class DashboardWallet : UserControl
    {
        private readonly IPaymentService _paymentService;
        private readonly INotificationService _notificationService;
        public ViewModel.WalletViewModel Model => (DataContext as ViewModel.WalletViewModel)!;
        public DashboardWallet(ViewModel.WalletViewModel model, IPaymentService paymentService, INotificationService notificationService)
        {
            _notificationService = notificationService;
            _paymentService = paymentService;
            InitializeComponent();
            DataContext = model;
        }

        private async void BtnWithdraw_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new UI.Dialogs.DlgWithdraw(Model.WithDrawModel);
            dlg.Owner = Window.GetWindow(this);
            Model.RequestDarkBackgroundVisibilityChange(true);
            await _paymentService.Refresh();
            if (dlg != null && dlg.Model != null && (dlg.ShowDialog() ?? false))
            {
                //Model.UpdateAddress(dlg.Model.ChangeAction, dlg.Model.NewAddress);
            }
            Model.RequestDarkBackgroundVisibilityChange(false);
        }

        private void BtnEditWalletAddress_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new UI.Dialogs.DlgEditAddress(Model.EditModel);
            dlg.Owner = Window.GetWindow(this);
            Model.RequestDarkBackgroundVisibilityChange(true);
            if (dlg != null && dlg.Model != null && (dlg.ShowDialog() ?? false))
            {
                Model.UpdateAddress(dlg.Model.ChangeAction, dlg.Model.NewAddress);
            }
            Model.RequestDarkBackgroundVisibilityChange(false);
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (Model.WalletAddress != null)
            {
                _notificationService.PushNotification(new SimpleNotificationObject(Src.AppNotificationService.Tag.Clipboard, "eth address has been copied to clipboard", expirationTimeInMs: 5000));
                Clipboard.SetText(Model.WalletAddress);
            }
        }

        private void BtnGolemLogo_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(GolemUI.Properties.Settings.Default.GolemWebPage);
        }

        private void BtnOpenL2_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://www.thorg.io/usage#Layer-2");
        }


    }
}
