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
    /// Interaction logic for SetupWindow.xaml
    /// </summary>
    public partial class SetupWindow : Window
    {
        protected ViewModel.SetupViewModel? Model => DataContext as ViewModel.SetupViewModel;

        public SetupWindow(ViewModel.SetupViewModel model)
        {
            InitializeComponent();
            DataContext = model;
        }

        private void CloseApp(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeApp(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void WantToLearn_Click(object sender, RoutedEventArgs e)
        {
            Model!.GoToNoobFlow();
        }

        private void ExpertMode_Click(object sender, RoutedEventArgs e)
        {
            Model!.GoToExpertMode();
        }


        private void OnWTLStep1(object sender, RoutedEventArgs e)
        {
            Model!.NoobStep = 1;
        }

        private void OnWTLStep2(object sender, RoutedEventArgs e)
        {
            Model!.NoobStep = 2;
        }

        private void OnWTLStep3Print(object sender, RoutedEventArgs e)
        {
            
        }

        private void OnWTLStep3Next(object sender, RoutedEventArgs e)
        {
            Model!.NoobStep = 3;
        }

        private void OnWTLStep4Next(object sender, RoutedEventArgs e)
        {
            Model!.NoobStep = 4;
        }
    }
}
