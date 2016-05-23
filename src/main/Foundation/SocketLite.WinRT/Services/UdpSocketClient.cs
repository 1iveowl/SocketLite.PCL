using System;
using System.Threading.Tasks;
using Windows.Networking;
using ISocketLite.PCL.Interface;
using SocketLite.Services.Base;

namespace SocketLite.Services
{
    public class UdpSocketClient : UdpSocketBase, IUdpSocketClient
    {
        public async Task ConnectAsync(string address, int port)
        {
            var hostName = new HostName(address);
            var serviceName = port.ToString();

            await DatagramSocket.ConnectAsync(hostName, serviceName);
        }

        public void Disconnect()
        {
            CloseSocket();
        }
    }
}
