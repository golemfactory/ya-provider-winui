using GolemUI.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Interfaces
{
    public interface IProviderConfig : INotifyPropertyChanged
    {
        Command.Config? Config { get; }

        Network Network { get; }

        bool IsMiningActive { get; }
        public void SetMiningActive(bool active, bool isLowMemoryMode);
        public void SwitchMiningMode(bool isLowMemoryMode);

        public bool IsLowMemoryModeActive { get; }

        bool IsCpuActive { get; set; }
        public int ActiveCpuCount { get; }
        public void UpdateActiveCpuThreadsCount(int threadsCount);
        void UpdateWalletAddress(string? walletAddress = null);
        void UpdateNodeName(string? value);

        Task Prepare(bool isGpuCapable, bool isLowMemoryMode);
    }
}
