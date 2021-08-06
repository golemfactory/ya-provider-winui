using GolemUI.Interfaces;

namespace GolemUI.Src.AppNotificationService
{
    public enum Tag { YagnaStatus, Benchmark, YagnaStarting, ProviderStatus, SettingsChanged, MiningStatus, AppStatus, Info };
    public class SimpleNotificationObject : INotificationObject
    {


        public Tag Tag { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public int ExpirationTimeInMs { get; set; }

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

        public SimpleNotificationObject(Tag tag, string title, string message, int expirationTimeInMs) : this(tag, title, message)
        {
            ExpirationTimeInMs = expirationTimeInMs;
        }

        public SimpleNotificationObject(Tag tag, string title, int expirationTimeInMs) : this(tag, title)
        {
            ExpirationTimeInMs = expirationTimeInMs;
        }
        public string GetId() => Tag.ToString();
    }
}
