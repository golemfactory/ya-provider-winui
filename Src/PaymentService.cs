using GolemUI.Command;
using GolemUI.Model;
using GolemUI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace GolemUI.Src
{
    public class PaymentService : Interfaces.IPaymentService
    {
        private Network _network;
        private string? _walletAddress;
        private string? _buildInAdress;
        private Command.YagnaSrv _srv;
        private readonly IProviderConfig _providerConfig;
        private IProcessControler _processControler;
        private DispatcherTimer _timer;

        public event PropertyChangedEventHandler? PropertyChanged;

        public PaymentService(Network network, Command.YagnaSrv srv, IProcessControler processControler, IProviderConfig providerConfig)
        {
            _network = network;
            _srv = srv;
            _processControler = processControler;
            _providerConfig = providerConfig;
                        
            _walletAddress = _providerConfig.Config?.Account;
            _providerConfig.PropertyChanged += this.OnProviderConfigChange;
            
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(20);
            _timer.Tick += async (object? s, EventArgs a) => this.UpdateState();
            _timer.Start();
            if (processControler.IsServerRunning)
            {                
                UpdateState();
            }
            else
            {
                _processControler.PropertyChanged += this.OnProcessControllerStateChange;
            }
        }

        public WalletState? State { get; private set; }

        public string Address => _walletAddress ?? _buildInAdress;

        private void OnProcessControllerStateChange(object? sender, PropertyChangedEventArgs ev)
        {
            if (ev.PropertyName == "IsServerRunning" && this._processControler.IsServerRunning)
            {
                UpdateState();
            }
        }

        private void OnProviderConfigChange(object? sender, PropertyChangedEventArgs ev)
        {
            _walletAddress = _providerConfig.Config?.Account ?? _srv.Id?.Address;
            UpdateState();
            OnPropertyChanged("Address");
        }

        private async void UpdateState()
        {
            if (!_processControler.IsServerRunning)
            {
                return;
            }
            if (_buildInAdress == null)
            {
                _buildInAdress = _srv.Id?.Address;
                if (_walletAddress == null)
                {
                    OnPropertyChanged("Address");
                }
            }
            var walletAddress = _walletAddress ?? _buildInAdress;

            var statusOnL2 = await _srv.Payment.PaymentStatus(_network, "zksync", walletAddress);
            var statusOnL1 = await _srv.Payment.PaymentStatus(_network, "erc20", walletAddress);

            var pending = (statusOnL2?.Incoming?.Accepted?.TotalAmount ?? 0m) + (statusOnL2?.Incoming?.Confirmed?.TotalAmount ?? 0m);
            var amountOnL2 = statusOnL2?.Amount ?? 0;
            var amountOnL1 = statusOnL1?.Amount ?? 0;

            var state = new WalletState(statusOnL2?.Token ?? "GLM")
            {
                Balance = amountOnL1 + amountOnL2,
                PendingBalance = pending,
                BalanceOnL2 = amountOnL2
            };

            var oldState = State;
            if (state != oldState)
            {
                State = state;
                OnPropertyChanged("State");
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
                
    }
}
