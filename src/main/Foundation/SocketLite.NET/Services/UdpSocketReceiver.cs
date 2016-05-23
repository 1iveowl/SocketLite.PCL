using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ISocketLite.PCL.Interface;
using SocketLite.Extensions;
using SocketLite.Services.Base;
using CommunicationInterface = SocketLite.Model.CommunicationInterface;
using PlatformSocketException = System.Net.Sockets.SocketException;
using PclSocketException = ISocketLite.PCL.Exceptions.SocketException;

namespace SocketLite.Services
{
    public class UdpSocketReceiver : UdpSocketBase, IUdpSocketReceiver
    {
        private CancellationTokenSource _messageCanceller;

        public async Task StartListeningAsync(int port = 0, ICommunicationInterface communicationInterface = null)
        {
            CheckCommunicationInterface(communicationInterface);

            var ip = communicationInterface != null ? ((CommunicationInterface)communicationInterface).NativeIpAddress : IPAddress.Any;
            var ep = new IPEndPoint(ip, port);

            _messageCanceller = new CancellationTokenSource();

            BackingUdpClient = new UdpClient(ep)
            {
                EnableBroadcast = true
            };

            await Task.Run(() => RunMessageReceiver(_messageCanceller.Token));
        }

        public void StopListening()
        {
            _messageCanceller.Cancel();
            BackingUdpClient.Close();
        }

        public override async Task SendToAsync(byte[] data, int length, string address, int port)
        {
            if (BackingUdpClient == null)
            {
                try
                {
                    using (var backingPort = new UdpClient { EnableBroadcast = true })
                    {
                        await backingPort.SendAsync(data, data.Length, address, port).WrapNativeSocketExceptions();
                    }
                }
                catch (PlatformSocketException ex)
                {
                    throw new PclSocketException(ex);
                }
            }
            else
            {
                await base.SendToAsync(data, length, address, port);
            }
        }
    }
}
