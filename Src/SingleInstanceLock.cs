using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GolemUI.Src
{
    public class SingleInstanceLock : IDisposable
    {
        private const string APP_GUID = "{f024e04e-6ae6-4563-95bc-376bf92646aa}";

        private readonly Mutex _instanceMutex;

        private readonly bool _isLocked;

        public delegate void ActivateEventHandler(object sender);

        public event ActivateEventHandler? ActivateEvent;

        private interface IActivator
        {
            bool Activate();
        }
        private class Activator : MarshalByRefObject, IActivator
        {
            public static SingleInstanceLock? _parent;

            public bool Activate()
            {
                _parent?.ActivateEvent?.Invoke(_parent);
                return true;
            }
        }

        public SingleInstanceLock()
        {
            _instanceMutex = new Mutex(false, $"Global\\{APP_GUID}");
            Activator._parent = this;
            _isLocked = _instanceMutex.WaitOne(0, false);
            if (_isLocked)
            {
                IpcServerChannel serverChannel = new IpcServerChannel($"remote-{APP_GUID}");
                ChannelServices.RegisterChannel(serverChannel, true);
                RemotingConfiguration.RegisterWellKnownServiceType(typeof(Activator), "Activator", WellKnownObjectMode.SingleCall);
            }
            else
            {
                var channel = new IpcClientChannel();
                ChannelServices.RegisterChannel(channel, true);
                var remote = (IActivator)System.Activator.GetObject(typeof(IActivator), $"ipc://remote-{APP_GUID}/Activator");
                remote.Activate();
            }
        }

        public bool IsMaster => _isLocked;
        public void Dispose()
        {
            _instanceMutex.Dispose();
        }
    }
}
