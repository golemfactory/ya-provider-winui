using GolemUI.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.ViewModel.CustomControls
{
    public class NotificationBarViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        INotificationService _notificationService;
        string _lastNotification = "";
        public string LastNotification
        {
            get => _lastNotification;
            set
            {
                _lastNotification = value;
                OnPropertyChanged("LastNotification");
            }
        }
        public NotificationBarViewModel(INotificationService notificationService)
        {
            _notificationService = notificationService;
            _notificationService.NotificationArrived += _notificationService_NotificationArrived;
        }

        private void _notificationService_NotificationArrived(INotificationObject notification)
        {
            LastNotification = notification.Title;
        }
        private void OnPropertyChanged(string? propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
