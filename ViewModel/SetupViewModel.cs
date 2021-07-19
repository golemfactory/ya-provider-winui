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
        private readonly Interfaces.IProviderConfig _providerConfig;

        private int _flow;

        private int _noobStep;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsDesingMode => false;

        public SetupViewModel(Interfaces.IProviderConfig providerConfig)
        {
            _flow = 0;
            _noobStep = 0;
            _providerConfig = providerConfig;

            _providerConfig.PropertyChanged += OnProviderConfigChanged;
        }

        private void OnProviderConfigChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "NodeName")
            {
                OnPropertyChanged("NodeName");
            }
        }

        public int Flow
        {
            get { return _flow; }
            set
            {
                _flow = value;
                OnPropertyChanged("Flow");
            }
        }

        public int NoobStep
        {
            get => _noobStep;
            set
            {
                _noobStep = value;
                OnPropertyChanged("NoobStep");
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

        public string? NodeName
        {
            get => _providerConfig.Config?.NodeName;
            set
            {
                _providerConfig.UpdateNodeName(value);
            }
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
