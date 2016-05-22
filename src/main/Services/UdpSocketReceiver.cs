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
    public class UdpSocketReceiver : UdpSocketBase, IUdpSocketReceiver
    {
        public event EventHandler<UdpSocketMessageReceivedEventArgs> MessageReceived;
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
