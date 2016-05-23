using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISocketLite.PCL.Interface;

namespace ISocketLite.PCL.EventArgs
{
    public interface ITcpSocket
    {
        ITcpSocketClient SocketClient { get; }

        //public TcpSocket(ITcpSocketClient socketClient)
        //{
        //    SocketClient = socketClient;
        //}
    }
}
