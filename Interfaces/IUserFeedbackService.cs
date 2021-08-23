using GolemUI.ViewModel;

namespace GolemUI.Interfaces
{




    public interface IUserFeedbackService
    {
        public void SendUserFeedback(string tag, string name, string email, string comments, bool shouldAttachLogs);
    }
}
