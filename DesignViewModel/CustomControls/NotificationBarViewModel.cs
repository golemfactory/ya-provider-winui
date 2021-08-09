using GolemUI.ViewModel.CustomControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.DesignViewModel.CustomControls
{

    public class NotificationBarViewModel : INotifyPropertyChanged
    {
        private string __lastNotification = "hello";
        public string LastNotification
        {
            get => __lastNotification;
            set
            {
                __lastNotification = value;
                OnPropertyChanged(nameof(LastNotification));
            }
        }

        public ObservableCollection<NotificationBarNotification> _items;
        public ObservableCollection<NotificationBarNotification> Items => _items;
        public NotificationBarViewModel()
        {
            _items = new ObservableCollection<NotificationBarNotification>();
            LastNotification = "hello2";
            Enumerable.Range(1, 5).ToList().ForEach(x => Items.Add(new NotificationBarNotification(true, NotificationState.Visible, $"title {x}", $"id {x}", $"message {x}", 5000, x * 1000, false)));

        }
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string? propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
