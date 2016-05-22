using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISocketLite.PCL.Interface;
using SocketLite.Model;
using SocketLite.Services.Base;

namespace SocketLite.Services
{
    public class UdpSocketReceiver : UdpSocketBase, IUdpSocketReceiver
    {
        public async Task StartListeningAsync(int port = 0, ICommunicationEntity communicationEntity = null)
        {
            CheckCommunicationInterface(communicationEntity);

            var serviceName = port == 0 ? "" : port.ToString();

            await BindeUdpServiceNameAsync(communicationEntity, serviceName);
        }

        public void StopListening()
        {
            CloseSocket();
        }
    }
}
