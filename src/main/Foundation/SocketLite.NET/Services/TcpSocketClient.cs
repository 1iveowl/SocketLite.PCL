using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using ISocketLite.PCL.Interface;
using ISocketLite.PCL.Model;
using SocketLite.Extensions;
using SocketLite.Services.Base;
using PlatformSocketException = System.Net.Sockets.SocketException;
using PclSocketException = ISocketLite.PCL.Exceptions.SocketException;

namespace SocketLite.Services
{
    public class TcpSocketClient : TcpSocketBase, ITcpSocketClient //, IExposeBackingSocket
    {
        //private readonly int _bufferSize;
        private SslStream _secureStream;
        private Stream _writeStream;

        public TcpClient Socket { get; private set; }

        public string RemoteAddress => RemoteEndpoint.Address.ToString();

        public int RemotePort => RemoteEndpoint.Port;

        public Stream ReadStream => _secureStream != null ? _secureStream as Stream : Socket.GetStream();

        public Stream WriteStream => _secureStream != null ? _secureStream as Stream : _writeStream;

        private IPEndPoint RemoteEndpoint
        {
            get
            {
                try
                {
                    return Socket.Client.RemoteEndPoint as IPEndPoint;
                }
                catch (PlatformSocketException ex)
                {
                    throw new PclSocketException(ex);
                }
            }
        }

        public TcpSocketClient() : base(0)
        {
            Socket = new TcpClient();
        }

        public TcpSocketClient(int bufferSize) : base(bufferSize)
        {

        }

        internal TcpSocketClient(TcpClient backingClient, int bufferSize) : base(bufferSize)
        {
            Socket = backingClient;
            InitializeWriteStream();
        }

        public async Task ConnectAsync(
            string address,
            int port,
            bool secure = false,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var connectTask = Socket.ConnectAsync(address, port).WrapNativeSocketExceptions();

            var ret = new TaskCompletionSource<bool>();
            var canceller = cancellationToken.Register(() => ret.SetCanceled());

            // if cancellation comes before connect completes, we honour it
            var okOrCancelled = await Task.WhenAny(connectTask, ret.Task);

            if (okOrCancelled == ret.Task)
            {
                // reset the backing field.
                // depending on the state of the socket this may throw ODE which it is appropriate to ignore
                try
                {
                    Disconnect();
                }
                catch (ObjectDisposedException)
                {

                }
                return;
                // notify that we did cancel
                //cancellationToken.ThrowIfCancellationRequested();
            }

            canceller.Dispose();

            InitializeWriteStream();

            if (secure)
            {
                var secureStream = new SslStream(_writeStream, true, ServerValidationCallback);
                secureStream.AuthenticateAsClient(address, null, System.Security.Authentication.SslProtocols.Tls, false);
                _secureStream = secureStream;
            }
        }

        public async Task ConnectAsync(
            string address,
            string service,
            bool secure = false,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var port = ServiceNames.PortForTcpServiceName(service);
            await ConnectAsync(address, port, secure, cancellationToken);
        }

        public void Disconnect()
        {
            Socket.Close();
            _secureStream = null;
            Socket = new TcpClient();
        }

        private void InitializeWriteStream()
        {
            _writeStream = BufferSize != 0 ? (Stream)new BufferedStream(Socket.GetStream(), BufferSize) : Socket.GetStream();
        }

        private bool ServerValidationCallback(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            switch (sslPolicyErrors)
            {
                case SslPolicyErrors.RemoteCertificateNameMismatch:
                    return false;
                case SslPolicyErrors.RemoteCertificateNotAvailable:
                    return false;
                case SslPolicyErrors.RemoteCertificateChainErrors:
                    return false;
            }
            return true;
        }
    }
}
