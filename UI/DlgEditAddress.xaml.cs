using GolemUI.ViewModel;
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

namespace GolemUI.UI
{
    /// <summary>
    /// Interaction logic for DlgEditAddress.xaml
    /// </summary>
    public partial class DlgEditAddress : Window
    {
        public DlgEditAddress(EditAddressViewModel model)
        {
            InitializeComponent();
            DataContext = model;
        }

        public EditAddressViewModel? Model => DataContext as EditAddressViewModel;

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (Model != null)
            {
                Model.ChangeAction = EditAddressViewModel.Action.Change;
            }
            DialogResult = true;
            Close();
        }
            

        private void TransferOUt_Click(object sender, RoutedEventArgs e)
        {
            if (Model != null)
            {
                Model.ChangeAction = EditAddressViewModel.Action.TransferOut;
            }
            DialogResult = true;
            Close();
        }
    }
}
