using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISocketLite.PCL.Interface;

namespace ISocketLite.PCL.EventArgs
{
    public class TcpSocketListenerConnectEventArgs : System.EventArgs
    {
        public ITcpSocketClient SocketClient { get; }

        public TcpSocketListenerConnectEventArgs(ITcpSocketClient socketClient)
        {
            SocketClient = socketClient;
        }
    }
}
