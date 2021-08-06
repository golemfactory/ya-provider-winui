using GolemUI.Interfaces;

namespace GolemUI.Src.AppNotificationService
{
    public enum Tag { YagnaStatus, SettingsChanged, MiningStatus, AppStatus, Info };
    public class SimpleNotificationObject: INotificationObject
    {

        
        public Tag Tag { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public int ExpiryTimeInMs { get; set; }

        public SimpleNotificationObject(Tag tag, string title)
        {
            Tag = tag;
            Title = title;
        }

        public SimpleNotificationObject(Tag tag, string title, string message)
        {
            Tag = tag;
            Title = title;
            Message = message;
        }

        public SimpleNotificationObject(Tag tag, string title, string message, int expiryTimeInMs) : this(tag, title, message)
        {
            ExpiryTimeInMs = expiryTimeInMs;
        }

        public SimpleNotificationObject(Tag tag, string title, int expiryTimeInMs) : this(tag, title)
        {
            ExpiryTimeInMs = expiryTimeInMs;
        }
    }
}
