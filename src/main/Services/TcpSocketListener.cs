using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;
using ISocketLite.PCL.Interface;
using SocketLite.Services.Base;
using static SocketLite.Helper.Helper;

namespace SocketLite.Services
{
    public class TcpSocketListener : TcpSocketBase, ITcpSocketListener
    {
        public event EventHandler<TcpSocketListenerConnectEventArgs> ConnectionReceived;

        public int LocalPort => 0;

        public Task StartListeningAsync(int port, ICommunicationEntity communicationEntity)
        {
            throw new NotImplementedException(BaitNoSwitch);
        }

        public void StopListening()
        {
            throw new NotImplementedException(BaitNoSwitch);
        }
    }
}
