using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using ISocketLite.PCL.EventArgs;
using ISocketLite.PCL.Interface;
using SocketLite.Model;

namespace SocketLite.Services.Base
{
    public abstract class UdpSocketBase : CommonSocketBase
    {

        protected DatagramSocket BackingDatagramSocket;

        public event EventHandler<UdpSocketMessageReceivedEventArgs> MessageReceived;


        protected UdpSocketBase()
        {
            SetBackingSocket();
        }

        public virtual async Task SendAsync(byte[] data)
        {
            await SendAsync(data, data.Length);
        }

        public virtual async Task SendAsync(byte[] data, int length)
        {
            var stream = BackingDatagramSocket.OutputStream.AsStreamForWrite();

            await stream.WriteAsync(data, 0, data.Length);
            await stream.FlushAsync();
        }

        public virtual async Task SendToAsync(byte[] data, string address, int port)
        {
            await SendToAsync(data, data.Length, address, port);
        }

        public virtual async Task SendToAsync(byte[] data, int length, string address, int port)
        {
            var hostName = new HostName(address);
            var serviceName = port.ToString();

            var stream = (await BackingDatagramSocket.GetOutputStreamAsync(hostName, serviceName))
                            .AsStreamForWrite();

            await stream.WriteAsync(data, 0, length);
            await stream.FlushAsync();
        }

        internal void DatagramMessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
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

            var wrapperArgs = new UdpSocketMessageReceivedEventArgs(remoteAddress, remotePort, allBytes);

            MessageReceived?.Invoke(this, wrapperArgs);
        }

        internal void CloseSocket()
        {
            BackingDatagramSocket.Dispose();
            SetBackingSocket();
        }

        protected async Task BindeUdpServiceNameAsync(ICommunicationEntity communicationEntity, string serviceName)
        {
            if (communicationEntity != null)
            {
                var adapter = ((CommunicationEntity)communicationEntity).NativeNetworkAdapter;
                await BackingDatagramSocket.BindServiceNameAsync(serviceName, adapter);
            }
            else
            {
                await BackingDatagramSocket.BindServiceNameAsync(serviceName);
            }
        }

        private void SetBackingSocket()
        {
            var socket = new DatagramSocket();
            socket.MessageReceived += DatagramMessageReceived;

            BackingDatagramSocket = socket;
        }
    }
}
