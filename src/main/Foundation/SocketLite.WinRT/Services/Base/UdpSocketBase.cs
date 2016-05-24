using System;
using System.IO;
using System.Linq.Expressions;
using System.Reactive.Linq;
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
        private ISubject<IUdpMessage> ObsMsg { get; } = new Subject<IUdpMessage>();

        public IObservable<IUdpMessage> ObservableMessages => ObsMsg.AsObservable();

        protected UdpSocketBase()
        {
            InitializeUdpSocket();
        }

        protected async Task BindeUdpServiceNameAsync(
            ICommunicationInterface communicationInterface, 
            string serviceName,
            bool allowMultipleBindToSamePort)
        {
            ConfigureDatagramSocket(allowMultipleBindToSamePort);

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

        protected void ConfigureDatagramSocket(bool allowMultipleBindToSamePort)
        {
#if WINDOWS_UWP
            DatagramSocket.Control.MulticastOnly = allowMultipleBindToSamePort;
#endif

#if !WINDOWS_UWP
            if (allowMultipleBindToSamePort)
            {
                throw new ArgumentException("Multiple binding to same port is only support by Windows 10/UWP and not WinRT");
            }
#endif
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

                ObsMsg.OnNext(udpMessage);
            }
            catch (Exception ex)
            {
                ObsMsg.OnError(ex);
            }
        }

        protected void CloseSocket()
        {
            DatagramSocket.MessageReceived -= DatagramMessageReceived;
            DatagramSocket.Dispose();
            InitializeUdpSocket();
        }

        private void InitializeUdpSocket()
        {
            DatagramSocket = new DatagramSocket();

            DatagramSocket.MessageReceived += DatagramMessageReceived;
        }

        public void Dispose()
        {
            ObsMsg.OnCompleted();
            DatagramSocket.MessageReceived -= DatagramMessageReceived;
            DatagramSocket.Dispose();
        }
    }
}
