using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ISocketLite.PCL.Interface;
using SocketLite.Extensions;
using SocketLite.Services.Base;
using CommunicationEntity = SocketLite.Model.CommunicationEntity;
using PlatformSocketException = System.Net.Sockets.SocketException;
using PclSocketException = ISocketLite.PCL.Exceptions.SocketException;

namespace SocketLite.Services
{
    public class UdpSocketReceiver : UdpSocketBase, IUdpSocketReceiver
    {
        private CancellationTokenSource _messageCanceller;

        public async Task StartListeningAsync(int port = 0, ICommunicationEntity listenOn = null)
        {
            CheckCommunicationInterface(listenOn);

            var ip = listenOn != null ? ((CommunicationEntity)listenOn).NativeIpAddress : IPAddress.Any;
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

        public new Task SendToAsync(byte[] data, string address, int port)
        {
            return SendToAsync(data, data.Length, address, port);
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

        public override void Dispose()
        {
            if (_messageCanceller != null && !_messageCanceller.IsCancellationRequested)
                _messageCanceller.Cancel();

            base.Dispose();
        }
    }
}
