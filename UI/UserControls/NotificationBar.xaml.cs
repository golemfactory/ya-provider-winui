using GolemUI.Model;
using GolemUI.ViewModel.CustomControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace GolemUI.UI.CustomControls
{
    /// <summary>
    /// Interaction logic for NavBar.xaml
    /// </summary>
    public partial class NotificationBar : UserControl
    {


        private NotificationBarViewModel? _model = null;
        public NotificationBar()
        {
            InitializeComponent();
        }
        public void SetViewModel(NotificationBarViewModel model)
        {
            _model = model;
            this.DataContext = model;

        }
        private static void OnStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }



        private void OnReady(object sender, RoutedEventArgs e)
        {

        }

        private void NavButtons_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
