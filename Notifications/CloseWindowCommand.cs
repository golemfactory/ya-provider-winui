using System.Windows;
using System.Windows.Input;

namespace GolemUI.Notifications
{
    /// <summary>
    /// Closes the current window.
    /// </summary>
    public class CloseWindowCommand : CommandBase<CloseWindowCommand>
    {
        public override void Execute(object? parameter)
        {
            App app = (App)Application.Current;

            app.RequestClose();
        }


        public override bool CanExecute(object? parameter)
        {
            return true;
        }
    }
}