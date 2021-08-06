using System.ComponentModel;

namespace GolemUI.ViewModel.CustomControls
{
    public class NotificationBarNotification : INotifyPropertyChanged
    {



        private NotificationState _state;

        public NotificationState State
        {
            get => _state;
            set
            {
                _state = value;
                OnPropertyChanged(nameof(State));
            }
        }

        private string _title = "";
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        private string _id = "";
        public string Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        private string _message = "";
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        private int _expirationTime = 1000;
        public int ExpirationTime
        {
            get => _expirationTime;
            set
            {
                _expirationTime = value;
                OnPropertyChanged(nameof(ExpirationTime));
            }
        }

        private int _lifeTime = 0;

        public NotificationBarNotification(NotificationState state, string title, string id, string message, int expirationTime, int lifeTime)
        {
            State = state;
            Title = title;
            Id = id;
            Message = message;
            ExpirationTime = expirationTime;
            LifeTime = lifeTime;
        }

        public int LifeTime
        {
            get => _lifeTime;
            set
            {
                _lifeTime = value;
                OnPropertyChanged(nameof(LifeTime));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string? propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
