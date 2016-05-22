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
    public class UdpSocketClient : UdpSocketBase, IUdpSocketClient
    {
        public event EventHandler<UdpSocketMessageReceivedEventArgs> MessageReceived;
        public Task ConnectAsync(string address, int port)
        {
            throw new NotImplementedException(BaitNoSwitch);
        }

        public void Disconnect()
        {
            throw new NotImplementedException(BaitNoSwitch);
        }
    }
}
