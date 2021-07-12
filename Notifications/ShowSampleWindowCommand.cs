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

            }
        }

        public override bool CanExecute(object? parameter)
        {
            return true;
        }
    }

    public class ShowWindowCommand2 : CommandBase<ShowWindowCommand2>
    {
        public override void Execute(object? parameter)
        {
            if (GlobalApplicationState.Instance.Dashboard != null)
            {
                GlobalApplicationState.Instance.Dashboard.WindowState = WindowState.Normal;
            }
        }

        public override bool CanExecute(object? parameter)
        {
            return true;
        }
    }
}