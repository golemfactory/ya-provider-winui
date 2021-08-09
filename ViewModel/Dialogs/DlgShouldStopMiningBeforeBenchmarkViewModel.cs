﻿using GolemUI.Interfaces;
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
    public class DlgShouldStopMiningBeforeBenchmarkViewModel : INotifyPropertyChanged
    {

        public bool _shouldAutoRestartMining;
        public bool ShouldAutoRestartMining
        {
            get => _shouldAutoRestartMining;
            set
            {
                _shouldAutoRestartMining = value;
                OnPropertyChanged(nameof(ShouldAutoRestartMining));
            }
        }

        public bool _rememberMyPreference;
        public bool RememberMyPreference
        {
            get => _rememberMyPreference;
            set
            {
                _rememberMyPreference = value;
                OnPropertyChanged(nameof(RememberMyPreference));
            }
        }
      

        public DlgShouldStopMiningBeforeBenchmarkViewModel( )
        {
          
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
