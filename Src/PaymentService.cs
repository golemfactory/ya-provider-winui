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
using GolemUI.Command.GSB;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Org.BouncyCastle.Crypto.Digests;
using GolemUI.Src.EIP712;
using Nethereum.Web3;
using System.Windows.Controls;
using Nethereum.Hex.HexConvertors.Extensions;

namespace GolemUI.Src
{
    public class PaymentService : Interfaces.IPaymentService
    {
        private Network _network;
        private string? _walletAddress;
        private string? _buildInAdress;
        private Command.YagnaSrv _srv;
        private readonly IProviderConfig _providerConfig;
        private readonly Payment _gsbPayment;
        private IProcessController _processController;
        private DispatcherTimer _timer;
        private ILogger<PaymentService> _logger;
        private readonly Command.GSB.Identity _gsbId;
        private readonly GasslessForwarderService _gasslessForwarder;
        public DateTime? LastSuccessfullRefresh { get; private set; } = null;

        public event PropertyChangedEventHandler? PropertyChanged;
        private bool _shouldCheckForInternalWallet = true;

        public PaymentService(Network network,
            Command.YagnaSrv srv, IProcessController processController, IProviderConfig providerConfig,
            Command.GSB.Payment gsbPayment,
            Command.GSB.Identity gsbId,
            GasslessForwarderService golemContract,
            ILogger<PaymentService> logger)
        {
            _logger = logger;
            _network = network;
            _srv = srv;
            _processController = processController;
            _providerConfig = providerConfig;
            _gsbPayment = gsbPayment;
            _gasslessForwarder = golemContract;
            _gsbId = gsbId;
            Task.Run(async () =>
            {
                await Task.Delay(5000);
                var id = await gsbId.GetDefaultIdentity();
                _logger.LogDebug("default id {0}", id);
                var digest = new KeccakDigest(256);
                var output = new byte[digest.GetDigestSize()];
                var b = UTF8Encoding.UTF8.GetBytes("ala ma kota");
                digest.BlockUpdate(b, 0, b.Length);
                digest.DoFinal(output, 0);
                var msg = await gsbId.SignBy(id.NodeId, output);
                {
                    var v = msg[0];
                    var r = msg.AsSpan(1, 32).ToArray();
                    var s = msg.AsSpan(33, 32).ToArray();
                    logger.LogWarning("v={},r={}, s={} ", v, r, s);
                }

            });


            _walletAddress = _providerConfig.Config?.Account;
            _providerConfig.PropertyChanged += this.OnProviderConfigChange;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(20);
            _timer.Tick += (object? s, EventArgs a) => this.UpdateState();
            _timer.Start();
            if (processController.IsServerRunning)
            {
                UpdateState();
            }
            else
            {
                _processController.PropertyChanged += this.OnProcessControllerStateChange;
            }
        }

        public WalletState? State { get; private set; }

        private WalletState? _internalWalletState;
        public WalletState? InternalWalletState
        {
            get
            {
                if (Address == InternalAddress)
                    return State;

                return _internalWalletState;
            }
            private set
            {
                _internalWalletState = value;
            }
        }

        public string? LastError { get; private set; }

        public string? Address => _walletAddress ?? _buildInAdress;

        public string InternalAddress => _buildInAdress ?? "";

        private void OnProcessControllerStateChange(object? sender, PropertyChangedEventArgs ev)
        {
            if (ev.PropertyName == "IsServerRunning" && this._processController.IsServerRunning)
            {
                UpdateState();
            }
        }

        private void OnProviderConfigChange(object? sender, PropertyChangedEventArgs ev)
        {
            _walletAddress = _providerConfig.Config?.Account ?? _buildInAdress;
            UpdateState();
            OnPropertyChanged("Address");
        }

        private async Task<WalletState> GetWalletState(String walletAddress)
        {
            var since = DateTime.UtcNow - TimeSpan.FromDays(2);
            var output = await Task.WhenAll(
                   _gsbPayment.GetStatus(walletAddress, PaymentDriver.ERC20.Id, since: since, network: _network.Id)
               );
            //var statusOnL1 = output[1];
            //var amountOnL1 = statusOnL1?.Amount ?? 0;

            var statusOnL2 = output[0];
            var pending = (statusOnL2?.Incoming?.Accepted?.TotalAmount ?? 0m) - (statusOnL2?.Incoming?.Confirmed?.TotalAmount ?? 0m);
            var amountOnL2 = statusOnL2?.Amount ?? 0;
            var state = new WalletState(statusOnL2?.Token ?? "GLM")
            {
                Balance = /*amountOnL1 + */amountOnL2,
                PendingBalance = pending,
                BalanceOnL2 = amountOnL2
            };
            return state;
        }

        public async Task Refresh()
        {
            try
            {
                if (!_processController.IsServerRunning)
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
                    OnPropertyChanged("InternalAddress");
                }
                var walletAddress = _walletAddress ?? _buildInAdress;

                if (walletAddress == null)
                {
                    throw new Exception("Wallet address is null");
                }

                var state = await GetWalletState(walletAddress);


                if (walletAddress != _buildInAdress)
                {
                    if (_shouldCheckForInternalWallet && _buildInAdress != null)
                    {
                        var internalWalletstate = await GetWalletState(_buildInAdress);
                        if (internalWalletstate == null || internalWalletstate?.Balance == 0) _shouldCheckForInternalWallet = false;

                        if (internalWalletstate != InternalWalletState)
                        {
                            InternalWalletState = internalWalletstate;
                            OnPropertyChanged("InternalWalletState");
                        }
                    }
                }
                var oldState = State;
                LastError = null;
                if (state != oldState)
                {
                    State = state;
                    OnPropertyChanged("State");
                }
                LastSuccessfullRefresh = DateTime.Now;
            }
            catch (HttpRequestException ex)
            {
                string errorMsg = $"HttpRequestException when updating payment status: {ex.Message}";
                _logger.LogError(errorMsg);
                LastError = "No connection to payment service";
                State = null;
                OnPropertyChanged("State");
            }
            catch (Exception ex)
            {
                string errorMsg = $"Exception when updating payment status: {ex.Message}";
                _logger.LogError(errorMsg);
                LastError = "Unknown problem with payment service";
                State = null;
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

        public async Task<bool> TransferOutTo(string address)
        {
            if (_buildInAdress == null)
            {
                return false;
            }
            var balance = await _gsbPayment.GetStatus(_buildInAdress, PaymentDriver.ERC20.Id, _network.Id);
            var result = await _gsbPayment.TransferTo(PaymentDriver.ERC20.Id, _buildInAdress, _network.Id, address, amount: balance.Amount);
            return true;
        }

        private async void UpdateState()
        {
            await Refresh();
        }

        public async Task<decimal> ExitFee(decimal? amount, string? to)
        {
            if (_buildInAdress == null)
            {
                throw new InvalidOperationException("intenal wallet not configured");
            }

            var output = await _gsbPayment.ExitFee(_buildInAdress, "zksync", _network.Id, amount);
            return output.Amount;
        }

        public Task<decimal> TransferFee(decimal? amount, string? to)
        {
            if (_buildInAdress == null)
            {
                throw new InvalidOperationException("intenal wallet not configured");
            }

            return Task.FromResult(0.00004022m);
        }


        public async Task<string> ExitTo(string driver, decimal amount, string destinationAddress, decimal? txFee)
        {
            if (_buildInAdress == null)
            {
                throw new InvalidOperationException("intenal wallet not configured");
            }

            string txUrl = await _gsbPayment.Exit(driver, _buildInAdress, destinationAddress, _network.Id, amount, txFee);
            return txUrl;
        }

        public async Task<string> RequestGaslessTransferTo(string driver, decimal amount, string destinationAddress)
        {
            if (driver != PaymentDriver.ERC20.Id)
                throw new ArgumentException($"PaymentDriver {driver} is not supported");

            var amountInWei = Web3.Convert.ToWei(amount);
            var request = await _gasslessForwarder.GetEip712EncodedTransferRequest(_network.Id, _buildInAdress, destinationAddress, amountInWei);

            var id = await _gsbId.GetDefaultIdentity();

            var msg = await _gsbId.SignBy(id.NodeId, request.Message);
            {
                var v = msg[0];
                if (v == 0) v = (byte)27;
                if (v == 1) v = (byte)28;
                var r = msg.AsSpan(1, 32).ToArray();
                var s = msg.AsSpan(33, 32).ToArray();

                request.R = "0x" + r.ToHex();
                request.S = "0x" + s.ToHex();
                request.V = "0x" + new byte[] { v }.ToHex();
                //Console.WriteLine($"R = {request.R}");
                //Console.WriteLine($"S = {request.S}");
                //Console.WriteLine($"V = {request.V}");
            }
            //Console.WriteLine("after signing = " + msg.ToHex());
            request.SignedMessage = msg;
            request.SenderAddress = _buildInAdress;



            string txHash = await _gasslessForwarder.SendRequest(request);

            //Console.WriteLine("signed msg = " + msg.ToHex() + " , "/* + success.ToString()*/);

            return txHash;
        }

        public async Task<string> TransferTo(string driver, decimal amount, string destinationAddress, decimal? txFee)
        {
            if (_buildInAdress == null)
            {
                throw new InvalidOperationException("internal wallet not configured");
            }
            string txUrl = await _gsbPayment.TransferTo(driver, _buildInAdress, _network.Id, destinationAddress, amount, txFee);
            return txUrl;
        }


    }
}
