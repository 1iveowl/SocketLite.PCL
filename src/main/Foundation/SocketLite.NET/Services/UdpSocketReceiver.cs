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

        public async Task StartListeningAsync(
            int port = 0, 
            ICommunicationInterface communicationInterface = null, 
            bool allowMultipleBindToSamePort = false)
        {
            CheckCommunicationInterface(communicationInterface);

            var ipAddress = (communicationInterface as CommunicationInterface)?.NativeIpAddress ?? IPAddress.Any;
            var ipEndPoint = new IPEndPoint(ipAddress, port);

            InitializeUdpClient(ipEndPoint, allowMultipleBindToSamePort);

            _messageCanceller = new CancellationTokenSource();

            await Task.Run(() => RunMessageReceiver(_messageCanceller.Token))
                .ConfigureAwait(false);
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
                await base.SendToAsync(data, length, address, port).ConfigureAwait(false);
            }
        }
    }
}
