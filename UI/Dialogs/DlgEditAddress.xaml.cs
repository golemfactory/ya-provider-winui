
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
    public partial class DlgEditAddress : Window
    {

        public DlgEditAddress(DlgEditAddressViewModel model)
        {
            InitializeComponent();
            DataContext = model;
        }

        public DlgEditAddressViewModel? Model => DataContext as DlgEditAddressViewModel;


        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Model != null)
                Model.ChangeAction = DlgEditAddressViewModel.Action.None;
            this.DialogResult = false;
            this.Close();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (Model != null)
            {
                Model.ChangeAction = Model.ShouldTransferFunds ? DlgEditAddressViewModel.Action.TransferOut : DlgEditAddressViewModel.Action.Change;
            }
            this.DialogResult = true;
            this.Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            if (Model != null)
                Model.ChangeAction = DlgEditAddressViewModel.Action.None;
            this.DialogResult = false;
            this.Close();
        }

    }
}
