using BetaMiner.ViewModel;

namespace BetaMiner
{
    public delegate void RequestDarkBackgroundEventHandler(bool shouldBackgroundBeVisible);
    public interface IDialogInvoker
    {
        public event RequestDarkBackgroundEventHandler? DarkBackgroundRequested;

    }
}
