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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GolemUI
{
    /// <summary>
    /// Interaction logic for WelcomeStart.xaml
    /// </summary>
    public partial class WelcomeStart : UserControl
    {
        public WelcomeStart()
        {
            InitializeComponent();
        }


        private void btnForwardAdvanced_Click(object sender, RoutedEventArgs e)
        {
            
            var sb = new Storyboard();
            /*
            {
                var marginStart = lblTitle.Margin;
                var marginEnd = marginStart;
                marginEnd.Left = marginStart.Left - lblTitle.ActualWidth;

                var duration = TimeSpan.FromMilliseconds(500);
                var animation =
                    new ThicknessAnimation
                    {
                    // fine-tune animation here
                    From = marginStart,
                        To = marginEnd,
                        Duration = new Duration(duration),
                    };
                Storyboard.SetTarget(animation, lblTitle);
                Storyboard.SetTargetProperty(animation, new PropertyPath(Label.MarginProperty));
                sb.Children.Add(animation);
            }
            
            {
                var opacityStart = 1.0;
                var opacityEnd = 0.0f;
                
                var duration = TimeSpan.FromMilliseconds(500);
                var animation =
                    new DoubleAnimation
                    {
                        // fine-tune animation here
                        From = opacityStart,
                        To = opacityEnd,
                        Duration = new Duration(duration),
                    };
                Storyboard.SetTarget(animation, brdLeft);
                Storyboard.SetTargetProperty(animation, new PropertyPath(Border.OpacityProperty));
                sb.Children.Add(animation);
            }
            {
                var opacityStart = 1.0;
                var opacityEnd = 0.0f;

                var duration = TimeSpan.FromMilliseconds(300);
                var animation =
                    new DoubleAnimation
                    {
                        // fine-tune animation here
                        From = opacityStart,
                        To = opacityEnd,
                        Duration = new Duration(duration),
                    };
                Storyboard.SetTarget(animation, lblChoose);
                Storyboard.SetTargetProperty(animation, new PropertyPath(Label.OpacityProperty));
                sb.Children.Add(animation);
            }
            
            {
                var marginStart = brdRight.Margin;
                var marginEnd = marginStart;
                marginEnd.Left = marginStart.Left - 10;
                marginEnd.Right = marginStart.Right - 10;
                marginEnd.Top = marginStart.Top - 10;
                marginEnd.Bottom = marginStart.Bottom - 10;

                var duration = TimeSpan.FromMilliseconds(300);
                var animation =
                    new ThicknessAnimation
                    {
                        // fine-tune animation here
                        From = marginStart,
                        To = marginEnd,
                        BeginTime = TimeSpan.FromMilliseconds(0),
                        Duration = new Duration(duration),
                    };
                Storyboard.SetTarget(animation, brdRight);
                Storyboard.SetTargetProperty(animation, new PropertyPath(Border.MarginProperty));
                sb.Children.Add(animation);
            }
            
            {
                var opacityStart = 1.0;
                var opacityEnd = 0.0f;

                var duration = TimeSpan.FromMilliseconds(300);
                var animation =
                    new DoubleAnimation
                    {
                        // fine-tune animation here
                        From = opacityStart,
                        To = opacityEnd,
                        BeginTime = duration,
                        Duration = new Duration(duration),
                    };
                Storyboard.SetTarget(animation, brdRight);
                Storyboard.SetTargetProperty(animation, new PropertyPath(Border.OpacityProperty));
                sb.Children.Add(animation);
            }*/
            sb.Completed += AnimationCompleted;
            sb.Begin();
            

        }

        void AnimationCompleted(object? sender, EventArgs e)
        {
            GlobalApplicationState.Instance.Dashboard?.SwitchPage(DashboardPages.PageWelcomeDecide);
        }
    }
}
