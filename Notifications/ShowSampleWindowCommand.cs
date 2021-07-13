using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace GolemUI.Notifications
{
    /// <summary>
    /// Shows the main window.
    /// </summary>
    public class ShowWindowCommand : CommandBase<ShowWindowCommand>
    {
        public override void Execute(object? parameter)
        {
            if (GlobalApplicationState.Instance.Dashboard != null)
            {
                GlobalApplicationState.Instance.Dashboard.WindowState = WindowState.Normal;
                GlobalApplicationState.Instance.Dashboard.ShowInTaskbar = true;
                GlobalApplicationState.Instance.Dashboard.tbNotificationIcon.Visibility = Visibility.Hidden;
            }
        }

        public override bool CanExecute(object? parameter)
        {
            return true;
        }
    }

}