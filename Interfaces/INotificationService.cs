using GolemUI.ViewModel;

namespace GolemUI.Interfaces
{

    public delegate void NewNotificationEventHandler(INotificationObject notification);
    public interface INotificationService
    {
        public event NewNotificationEventHandler? NotificationArrived;
        public event NewNotificationEventHandler? NotificationDeleted;
        public void PushNotification(INotificationObject notification);
        public void DeleteNotification(INotificationObject notification);
    }

    public interface INotificationObject
    {
        public int ExpirationTimeInMs { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool Group { get; set; }
        public string GetId();
    }
}
