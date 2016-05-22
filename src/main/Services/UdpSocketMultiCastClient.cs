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
    public class UdpSocketMulticastClient : UdpSocketBase, IUdpSocketMulticastClient
    {
        public int TTL { get; set; }

        public event EventHandler<UdpSocketMessageReceivedEventArgs> MessageReceived;
        public Task JoinMulticastGroupAsync(string multicastAddress, int port, ICommunicationEntity communicationEntity)
        {
            throw new NotImplementedException(BaitNoSwitch);
        }

        public void Disconnect()
        {
            throw new NotImplementedException(BaitNoSwitch);
        }

        public Task SendMulticastAsync(byte[] data)
        {
            throw new NotImplementedException(BaitNoSwitch);
        }

        public Task SendMulticastAsync(byte[] data, int length)
        {
            throw new NotImplementedException(BaitNoSwitch);
        }
    }
}
