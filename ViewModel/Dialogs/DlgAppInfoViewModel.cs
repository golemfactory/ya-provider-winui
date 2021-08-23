using GolemUI.Interfaces;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GolemUI.ViewModel.Dialogs
{
    public class DlgAppInfoViewModel : INotifyPropertyChanged
    {
        string _userName = "";
        string _userEmail = "";
        string _userComment = "";
        bool _shouldAttachLogs = true;
        public bool ShouldAttachLogs
        {
            get => _shouldAttachLogs;
            set
            {
                _shouldAttachLogs = value;
                OnPropertyChanged(nameof(ShouldAttachLogs));
            }
        }

        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }
        public string UserEmail
        {
            get => _userEmail;
            set
            {
                _userEmail = value;
                OnPropertyChanged(nameof(UserEmail));
            }
        }
        public string UserComment
        {
            get => _userComment;
            set
            {
                _userComment = value;
                OnPropertyChanged(nameof(UserComment));
            }
        }
        public void SendFeedback()
        {
            _userFeedback.SendUserFeedback("User Feedback > Manual entry", UserName, UserEmail, UserComment, ShouldAttachLogs);
        }

        public string AppVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public string? NodeName => _provider?.Config?.NodeName;
        private readonly IProviderConfig _provider;
        private readonly IUserFeedbackService _userFeedback;
        public DlgAppInfoViewModel(IProviderConfig provider, IUserFeedbackService userFeedback)
        {
            _userFeedback = userFeedback;
            _provider = provider;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


    }
}
