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
    public class UdpSocketClient : UdpSendBase, IUdpSocketClient
    {

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

        public async Task ConnectAsync(
            string address, 
            int port, 
            bool allowMultipleBindToSamePort = false)
        {
            if (allowMultipleBindToSamePort)
            {
                BackingUdpClient.ExclusiveAddressUse = false;
                BackingUdpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            }
            else
            {
                BackingUdpClient.ExclusiveAddressUse = true;
                BackingUdpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
            }

            MessageConcellationTokenSource = new CancellationTokenSource();

            await Task.Run(() => BackingUdpClient.Connect(address, port), MessageConcellationTokenSource.Token)
                .WrapNativeSocketExceptions()
                .ConfigureAwait(false);
        }

        public void Disconnect()
        {
            MessageConcellationTokenSource?.Cancel();
            BackingUdpClient?.Close();
        }

        public void Dispose()
        {
            BackingUdpClient?.Close();
        }
    }
}
