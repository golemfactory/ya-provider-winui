using GolemUI.Interfaces;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GolemUI.Src
{

    public class SimpleNotificationObject: INotificationObject
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
    }

    public class AppNotificationService : INotificationService
    {
        public AppNotificationService()
        {
        }

        public event NewNotificationEventHandler? NotificationArrived;
        public event NewNotificationEventHandler? NotificationUpdated;
        public event NewNotificationEventHandler? NotificationDeleted;
        public void PushNotification(INotificationObject notification)
        {

        }
        public void UpdateNotification(INotificationObject notification)
        {

        }
        public void DeleteNotification(INotificationObject notification)
        {

        }
    }
}
