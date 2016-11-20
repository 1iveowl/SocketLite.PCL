using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;
using ISocketLite.PCL.Interface;

namespace SocketLite.Model
{
    public class TcpSocket : ITcpSocket
    {
        public ITcpSocketClient SocketClient { get; internal set; }
    }
}
