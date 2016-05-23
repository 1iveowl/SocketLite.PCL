using System;
using System.IO;
using System.Linq.Expressions;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using ISocketLite.PCL.EventArgs;
using ISocketLite.PCL.Interface;
using SocketLite.Model;

namespace SocketLite.Services.Base
{
    public abstract class UdpSocketBase : UdpSendBase
    {
        public ISubject<IUdpMessage> ObservableMessages { get; private set; } = new Subject<IUdpMessage>();

        protected UdpSocketBase()
        {
            InitializeSocket();
        }

        protected async Task BindeUdpServiceNameAsync(ICommunicationInterface communicationInterface, string serviceName)
        {
            
            var adapter = (communicationInterface as CommunicationInterface)?.NativeNetworkAdapter;

            if (adapter != null)
            {
                await DatagramSocket.BindServiceNameAsync(serviceName, adapter);
            }
            else
            {
                await DatagramSocket.BindServiceNameAsync(serviceName);
            }
        }

        protected void DatagramMessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            try
            {
                var remoteAddress = args.RemoteAddress.CanonicalName;
                var remotePort = args.RemotePort;
                byte[] allBytes;

                var stream = args.GetDataStream().AsStreamForRead();
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    allBytes = memoryStream.ToArray();
                }

                var udpMessage = new UdpMessage
                {
                    ByteData = allBytes,
                    RemoteAddress = remoteAddress,
                    RemotePort = remotePort
                };

                ObservableMessages.OnNext(udpMessage);
            }
            catch (Exception ex)
            {
                ObservableMessages.OnError(ex);
            }
        }

        protected void CloseSocket()
        {
            DatagramSocket.MessageReceived -= DatagramMessageReceived;
            DatagramSocket.Dispose();
            InitializeSocket();
        }

        private void InitializeSocket()
        {
            var socket = new DatagramSocket
            {
#if WINDOWS_UWP
                Control =
                {
                    MulticastOnly = true,
                }
#endif
            };

            socket.MessageReceived += DatagramMessageReceived;
            DatagramSocket = socket;
        }

        public void Dispose()
        {
            ObservableMessages.OnCompleted();
            DatagramSocket.MessageReceived -= DatagramMessageReceived;
            DatagramSocket.Dispose();
        }
    }
}
