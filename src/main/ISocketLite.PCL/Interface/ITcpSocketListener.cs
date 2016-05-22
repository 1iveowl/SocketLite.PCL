using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;

namespace ISocketLite.PCL.Interface
{
    public interface ITcpSocketListener
    {
        Task StartListeningAsync(int port, ICommunicationEntity communicationEntity);

        void StopListening();

        int LocalPort { get; }

        event EventHandler<TcpSocketListenerConnectEventArgs> ConnectionReceived;
    }
}
