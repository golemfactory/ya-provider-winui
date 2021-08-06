using GolemUI.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.ViewModel.CustomControls
{

    public class NotificationBarViewModel: INotifyPropertyChanged
    {

        public NotificationBarViewModel(INotificationService notificationService)
        {
            _items = new ObservableCollection<NotificationBarNotification>();
            _notificationService = notificationService;
            _notificationService.NotificationArrived += _notificationService_NotificationArrived;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        INotificationService _notificationService;
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


      

        private void _notificationService_NotificationArrived(INotificationObject ntf)
        {
            AddOrUpdate(new NotificationBarNotification(NotificationState.Visible, ntf.Title, ntf.GetId(), ntf.Message, 5000, 0));
        }
        private bool ElementWithSpecifiedIdAlreatExists(string id)
        {
            return Items.ToList().Exists(x => x.Id == id);
        }
        private void TryUpdateElement(NotificationBarNotification ntf)
        {
            var item = Items.ToList().Find(x => x.Id == ntf.Id);
            if (item != default(NotificationBarNotification))
            {
                item.Title = ntf.Title;
            }
        }
        private void AddOrUpdate(NotificationBarNotification ntf)
        {
            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                LastNotification = ntf.Title;
                if (ElementWithSpecifiedIdAlreatExists(ntf.Id))
                    TryUpdateElement(ntf);
                else
                    Items.Add(ntf);
            });
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
