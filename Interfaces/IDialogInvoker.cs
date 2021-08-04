using GolemUI.ViewModel;

namespace GolemUI
{
    public delegate void RequestDarkBackgroundEventHandler(bool shouldBackgroundBeVisible);
    public interface IDialogInvoker
    {
        public event RequestDarkBackgroundEventHandler? DarkBackgroundRequested;

    }
}
