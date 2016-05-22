using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using ISocketLite.PCL.Interface;
using SocketLite.Model;
using SocketLite.Services.Base;

namespace SocketLite.Services
{
    public class UdpSocketMulticastClient : UdpSocketBase, IUdpSocketMulticastClient
    {
        private string _multicastAddress;
        private int _multicastPort;

        public int TTL { get; set; } = 1;

        public async Task JoinMulticastGroupAsync(string multicastAddress, int port, ICommunicationEntity communicationEntity = null)
        {
            //Throws and exception if the communication interface is not ready og valid.
            CheckCommunicationInterface(communicationEntity);

            var hostName = new HostName(multicastAddress);
            var serviceName = port.ToString();

#if WINDOWS_UWP
            BackingDatagramSocket.Control.MulticastOnly = true;
#endif
            await BindeUdpServiceNameAsync(communicationEntity, serviceName);

            BackingDatagramSocket.Control.OutboundUnicastHopLimit = (byte)TTL;
            BackingDatagramSocket.JoinMulticastGroup(hostName);

            _multicastAddress = multicastAddress;
            _multicastPort = port;
        }

        public async Task SendMulticastAsync(byte[] data)
        {
            await SendMulticastAsync(data, data.Length);
        }

        public async Task SendMulticastAsync(byte[] data, int length)
        {
            if (_multicastAddress == null)
                throw new InvalidOperationException("Must join a multicast group before sending.");

            await base.SendToAsync(data, _multicastAddress, _multicastPort);
        }

        public void Disconnect()
        {
            CloseSocket();
        }
    }
}
