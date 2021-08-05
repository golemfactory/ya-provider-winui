using GolemUI.Command;
using GolemUI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Interfaces
{
    public delegate void LogLineHandler(string logger, string line);

    public interface IProcessControler : INotifyPropertyChanged
    {
        string ServerUri { get; }
        Task<string> GetAppKey();

        public Task<bool> Prepare();


        public Task<YagnaAgreement?> GetAgreement(string agreementID);

        /// <summary>
        /// Starts daemon with given private key.
        /// </summary>
        /// <param name="privateKEy"></param>
        /// <returns>Wallet address</returns>
        public Task<string> PrepareForKey(byte[] privateKey);

        public Task<bool> Start(Network network, string? claymoreExtraParams);

        public Task<Command.KeyInfo> Me();

        bool IsProviderRunning { get; }

        bool IsServerRunning { get; }

        bool IsStarting { get; }

        LogLineHandler? LineHandler { get; set; }

        Task<bool> Stop();

        void StopYagna();
    }
}
