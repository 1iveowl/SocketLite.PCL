using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;

namespace ISocketLite.PCL.Interface
{
    public interface IUdpSocketReceiver
    {
        event EventHandler<UdpSocketMessageReceivedEventArgs> MessageReceived;
        Task StartListeningAsync(int port, ICommunicationEntity communicationEntity);

        void StopListening();

        Task SendToAsync(byte[] data, string address, int port);

        Task SendToAsync(byte[] data, int length, string address, int port);

        
    }
}
