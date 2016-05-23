using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using ISocketLite.PCL.Interface;
using SocketLite.Services.Base;

namespace SocketLite.Services
{
    public class TcpSocketClient : TcpSocketBase, ITcpSocketClient//, IExposeBackingSocket
    {
        public StreamSocket Socket { get; private set; }

        public Stream ReadStream => Socket.InputStream.AsStreamForRead(BufferSize);

        public Stream WriteStream => Socket.OutputStream.AsStreamForWrite(BufferSize);

        public string RemoteAddress => Socket.Information.RemoteAddress.CanonicalName;

        public int RemotePort => int.Parse(Socket.Information.RemotePort);

        //object IExposeBackingSocket.Socket => Socket;

        public TcpSocketClient() : base(0)
        {
            Socket = new StreamSocket();
        }

        public TcpSocketClient(int bufferSize) : base(bufferSize)
        {
        }

        internal TcpSocketClient(StreamSocket nativeSocket, int bufferSize) : base(bufferSize)
        {
            Socket = nativeSocket;
        }
        public async Task ConnectAsync(string address, int port, bool secure = false)
        {
            var service = port.ToString();
            await ConnectAsync(address, service, secure).ConfigureAwait(false);
        }

        public async Task ConnectAsync(string address, string service, bool secure = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            var hostName = new HostName(address);
            var remoteServiceName = service;
            var socketProtectionLevel = secure ? SocketProtectionLevel.Tls10 : SocketProtectionLevel.PlainSocket;

            await Socket.ConnectAsync(hostName, remoteServiceName, socketProtectionLevel);
        }

        public void Disconnect()
        {
            Socket.Dispose();
            Socket = new StreamSocket();
        }

        public void Dispose()
        {
            
        }
        
    }
}
