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
    public class TcpSocketClient : TcpSocketBase, ITcpSocketClient
    {
        public string LocalPort => Socket.Information.LocalPort;
        public string LocalAddress => Socket.Information.LocalAddress.CanonicalName;

        public StreamSocket Socket { get; private set; }

        public Stream ReadStream => Socket.InputStream.AsStreamForRead(BufferSize);

        public Stream WriteStream => Socket.OutputStream.AsStreamForWrite(BufferSize);

        public string RemoteAddress => Socket.Information.RemoteAddress.CanonicalName;

        public int RemotePort => int.Parse(Socket.Information.RemotePort);

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

        public async Task ConnectAsync(
            string address, 
            string service, 
            bool secure = false, 
            CancellationToken cancellationToken = default(CancellationToken), 
            bool ignoreServerCertificateErrors = false)
        {
            var hostName = new HostName(address);
            var remoteServiceName = service;
            var socketProtectionLevel = secure ? SocketProtectionLevel.Tls10 : SocketProtectionLevel.PlainSocket;

            try
            {
                await Socket.ConnectAsync(hostName, remoteServiceName, socketProtectionLevel);
            }
            catch (Exception ex)
            {
                if (ignoreServerCertificateErrors)
                {
                    Socket.Control.IgnorableServerCertificateErrors.Clear();

                    foreach (var ignorableError in Socket.Information.ServerCertificateErrors)
                    {
                        Socket.Control.IgnorableServerCertificateErrors.Add(ignorableError);
                    }

                    //Try again
                    try
                    {
                        await Socket.ConnectAsync(hostName, remoteServiceName, socketProtectionLevel);
                    }
                    catch (Exception retryEx)
                    {

                        throw retryEx;
                    }
                }
                else
                {
                    throw ex;
                }
                
            }
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
