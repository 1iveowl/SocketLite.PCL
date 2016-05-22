using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ISocketLite.PCL.Interface;
using SocketLite.Extensions;
using SocketLite.Services.Base;
using PlatformSocketException = System.Net.Sockets.SocketException;
using PclSocketException = ISocketLite.PCL.Exceptions.SocketException;


namespace SocketLite.Services
{
    public class UdpSocketClient : UdpSocketBase, IUdpSocketClient
    {
        private CancellationTokenSource _messageCanceller;

        public UdpSocketClient()
        {
            try
            {
                BackingUdpClient = new UdpClient
                {
                    EnableBroadcast = true
                };
            }
            catch (PlatformSocketException ex)
            {
                throw new PclSocketException(ex);
            }
        }

        public Task ConnectAsync(string address, int port)
        {
            _messageCanceller = new CancellationTokenSource();

            return Task
                .Run(() => this.BackingUdpClient.Connect(address, port))
                .WrapNativeSocketExceptions();
        }

        public void Disconnect()
        {
            _messageCanceller.Cancel();
            BackingUdpClient.Close();
        }
    }
}
