
using GolemUI.ViewModel;
using GolemUI.ViewModel.Dialogs;
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

namespace GolemUI.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for DlgEditAddress.xaml
    /// </summary>
    public partial class DlgWithdraw : Window
    {

        public DlgWithdraw(DlgWithdrawViewModel model)
        {
            InitializeComponent();
            DataContext = model;
        }

        public DlgWithdrawViewModel Model => (DataContext as DlgWithdrawViewModel)!;


        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            /*   if (Model != null)
                   Model.ChangeAction = DlgWithdrawViewModel.Action.None;*/
            this.DialogResult = false;
            this.Close();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (Model != null)
            {
                /*  Model.ChangeAction = Model.ShouldTransferFunds ? DlgEditAddressViewModel.Action.TransferOut : DlgEditAddressViewModel.Action.Change;*/
            }
            this.DialogResult = true;
            this.Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            /*  if (Model != null)
                  Model.ChangeAction = DlgEditAddressViewModel.Action.None;*/
            this.DialogResult = false;
            this.Close();
        }


        private void BtnCancelStep1_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BtnSetMax_Click(object sender, RoutedEventArgs e)
        {
            Model.Amount = Model.MaxAmount;
        }

        private async void BtnConfirmStep1_Click(object sender, RoutedEventArgs e)
        {
            await Model.UpdateTxFee();
            tabControl.SelectedIndex = 1;
        }

        private async void BtnConfirmStep2_Click(object sender, RoutedEventArgs e)
        {
            await Model.SendTx();
            tabControl.SelectedIndex = 2;
        }

        private void BtnCancelStep2_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 0;
        }

        private void BtnConfirmStep3_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void BtnCheckOnEtherScan_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://etherscan.io/");
        }

        private void BtnCheckOnZKsyncExplorer_Click(object sender, RoutedEventArgs e)
        {
            Model.OpenZkSyncExplorer();
        }
    }
}
