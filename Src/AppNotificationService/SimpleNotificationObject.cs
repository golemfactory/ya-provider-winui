using GolemUI.Interfaces;

namespace GolemUI.Src.AppNotificationService
{
    public enum Tag { YagnaStatus, Benchmark, YagnaStarting, ProviderStatus, SettingsChanged, MiningStatus, AppStatus, Info, Clipboard };
    public class SimpleNotificationObject : INotificationObject
    {
        public Tag Tag { get; set; }
        public bool Group { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public int ExpirationTimeInMs { get; set; }

        public SimpleNotificationObject(Tag tag, string title, string message = "", int expirationTimeInMs = 0, bool group = true)
        {
            Tag = tag;
            Title = title;
            Message = message;
            ExpirationTimeInMs = expirationTimeInMs;
            Group = group;
        }


        public string GetId() => Tag.ToString();
    }
}
