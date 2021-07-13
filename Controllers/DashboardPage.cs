using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GolemUI.Controllers
{
    public class DashboardPage
    {
        public UserControl View;
        ISavableLoadableDashboardPage ViewModel=null;
        public bool ShouldAutoLoad=false;
        public bool ShouldAutoSave=false;
        bool ShouldAnimate = true;
        public DashboardPage(UserControl view)
        {
            View = view;
        }
        public DashboardPage(UserControl view, ISavableLoadableDashboardPage viewModel)
        {
            View = view;
            ViewModel = viewModel;
            ShouldAutoLoad = true;
            ShouldAutoLoad = true;
        }
        public void Unmount()
        {
            if (ShouldAutoSave && ViewModel != null)
            {
                ViewModel.SaveData();
            }
        }

        public void Mount()
        {
            if (ShouldAutoLoad && ViewModel != null)
            {
                ViewModel.LoadData();
            }
        }
        public void Show()
        {
            if (ShouldAnimate)
            {
                View.Visibility = Visibility.Visible;
                ShowSlowly(TimeSpan.FromMilliseconds(250));
            }
            else
            {
                View.Visibility = Visibility.Visible;
                View.Opacity = 1.0f;
            }
        }
       
        public void Hide()
        {
            if (ShouldAnimate)
            {
                HideSlowly(TimeSpan.FromMilliseconds(300));
            }
            else
            {
                View.Opacity = 0.0f;
                View.Visibility = Visibility.Hidden;
            }
        }
        public void Clear()
        {
            View.Visibility = Visibility.Hidden;
            View.Opacity = 0.0f;
        }
        private void ShowSlowly(TimeSpan duration)
        {

            DoubleAnimation animation = new DoubleAnimation(0.0, 1.0, duration);
            DoubleAnimation animation2 = new DoubleAnimation(0.92, 1.0, duration);

            View.RenderTransform = new ScaleTransform(0.9, 0.9, 0.5, 0.5);
            View.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation2);
            View.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation2);
            View.BeginAnimation(UserControl.OpacityProperty, animation);

        }
        private void HideSlowly(TimeSpan duration)
        {
            EventHandler handler = null;
            DoubleAnimation anim1 = new DoubleAnimation(1, 0, duration);
            anim1.Completed += handler = new EventHandler(delegate (object s, EventArgs ev)
            {
                View.Visibility = Visibility.Hidden;
            });
            View.BeginAnimation(UserControl.OpacityProperty, anim1);

            /*DoubleAnimation animation = new DoubleAnimation(1.0, 0.0, duration);
            uc.BeginAnimation(UserControl.OpacityProperty, animation);*/
        }

    }
}