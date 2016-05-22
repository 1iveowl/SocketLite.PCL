using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;

namespace ISocketLite.PCL.Interface
{
    public interface IUdpSocketMulticastClient
    {
        event EventHandler<UdpSocketMessageReceivedEventArgs> MessageReceived;

        Task JoinMulticastGroupAsync(string multicastAddress, int port, ICommunicationEntity communicationEntity);

        void Disconnect();

        Task SendMulticastAsync(byte[] data);

        Task SendMulticastAsync(byte[] data, int length);

        int TTL { get; set; }

        
    }
}
