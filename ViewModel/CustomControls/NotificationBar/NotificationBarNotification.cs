using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GolemUI.ViewModel.CustomControls
{
    public class NotificationBarNotification : INotifyPropertyChanged
    {
        public NotificationBarNotification(bool shouldAutoHide, NotificationState state, string title, string id, string message, int expirationTime, int lifeTime, bool group)
        {
            ShouldAutoHide = shouldAutoHide;
            State = state;
            Title = title;
            Id = id;
            Message = message;
            ExpirationTime = expirationTime;
            LifeTime = lifeTime;
            Group = group;
            CreationTime = DateTime.Now;
        }

        public DateTime CreationTime;

        public bool ShouldAutoHide = false;

        private NotificationState _state;

        public NotificationState State
        {
            get => _state;
            set
            {
                _state = value;
                OnPropertyChanged();
            }
        }

        private string _title = "";
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        private string _id = "";
        public string Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        private string _message = "";
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public bool ShouldDisappear
        {
            get
            {
                return ShouldAutoHide && LifeTime > ExpirationTime;

            }
        }
        public bool ShouldBeRemoved
        {
            get
            {
                return ShouldAutoHide && LifeTime > ExpirationTime + 500;

            }
        }
        private int _expirationTime = 1000;
        public int ExpirationTime
        {
            get => _expirationTime;
            set
            {
                _expirationTime = value;
                OnPropertyChanged();
            }
        }

        private int _lifeTime = 0;


        public int LifeTime
        {
            get => _lifeTime;
            set
            {
                _lifeTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Percentage));
                OnPropertyChanged(nameof(ShouldDisappear));
                OnPropertyChanged(nameof(PercentageAsString));
            }
        }

        private bool _group = false;

        public bool Group
        {
            get => _group;
            set
            {
                _group = value;
                OnPropertyChanged();
            }
        }


        private const int MaxHeight = 20;
        public int Percentage
        {
            get
            {
                if (!ShouldAutoHide && ExpirationTime == 0) return MaxHeight;
                int percentage = MaxHeight - (int)((double)MaxHeight * (double)LifeTime / (double)ExpirationTime);
                if (percentage > MaxHeight) return MaxHeight;
                if (percentage < 0) return 0;
                return percentage;

            }
        }
        public string PercentageAsString
        {
            get
            {
                return Percentage.ToString();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
