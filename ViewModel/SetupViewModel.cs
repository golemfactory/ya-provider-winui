using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.ViewModel
{
    public class SetupViewModel : INotifyPropertyChanged
    {
        private int _flow;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsDesingMode => false;

        public SetupViewModel()
        {
            _flow = 0;
        }

        public int Flow { 
            get { return _flow; } 
            set
            {
                _flow = value;
                OnPropertyChanged("Flow");
            }
        }

        public void GoToStart()
        {
            Flow = 0;
        }
        public void GoToNoobFlow()
        {
            Flow = 1;
        }
        public void GoToExpertMode()
        {
            Flow = 2;
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
