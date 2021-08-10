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
            App app = (App)Application.Current;

            app.ActivateFromTray();
        }

        public override bool CanExecute(object? parameter)
        {
            return true;
        }
    }

}