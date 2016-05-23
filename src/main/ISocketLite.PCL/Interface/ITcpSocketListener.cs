using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;

namespace ISocketLite.PCL.Interface
{
    public interface ITcpSocketListener : IDisposable
    {
        //event EventHandler<TcpSocketListenerConnectEventArgs> ConnectionReceived;
        ISubject<ITcpSocketClient> ObservableTcpSocket { get; }

        Task StartListeningAsync(int port, ICommunicationInterface communicationEntity = null);

        void StopListening();

        int LocalPort { get; }
        
    }
}
