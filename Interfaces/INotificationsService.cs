using GolemUI.ViewModel;

namespace GolemUI
{

    public delegate void NewNotificationEventHandler(INotificationObject notification);
    public interface INotificationsService
    {
        public event NewNotificationEventHandler? NotificationArrived;
        public event NewNotificationEventHandler? NotificationUpdated;
        public event NewNotificationEventHandler? NotificationDeleted;
        public void PushNotification(INotificationObject notification);
        public void UpdateNotification(INotificationObject notification);
        public void DeleteNotification(INotificationObject notification);

    }

    public interface INotificationObject
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
    }
}
