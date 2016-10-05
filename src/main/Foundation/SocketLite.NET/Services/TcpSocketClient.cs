using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
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
    public class TcpSocketClient : TcpSocketBase, ITcpSocketClient
    {
        //private readonly int _bufferSize;
        private SslStream _secureStream;
        private Stream _writeStream;

        public TcpClient tcpClient { get; private set; }

        public string RemoteAddress => RemoteEndpoint.Address.ToString();

        public int RemotePort => RemoteEndpoint.Port;

        public Stream ReadStream => _secureStream != null ? _secureStream as Stream : tcpClient.GetStream();

        public Stream WriteStream => _secureStream != null ? _secureStream as Stream : _writeStream;

        private IPEndPoint RemoteEndpoint
        {
            get
            {
                try
                {
                    return tcpClient.Client.RemoteEndPoint as IPEndPoint;
                }
                catch (PlatformSocketException ex)
                {
                    throw new PclSocketException(ex);
                }
            }
        }

        public TcpSocketClient() : base(0)
        {
            tcpClient = new TcpClient();
        }

        public TcpSocketClient(int bufferSize) : base(bufferSize)
        {

        }

        internal TcpSocketClient(TcpClient backingClient, int bufferSize) : base(bufferSize)
        {
            tcpClient = backingClient;
            InitializeWriteStream();
        }

        

        private async Task ConnectAsync(
            string address,
            int port,
            bool secure = false,
            CancellationToken cancellationToken = default(CancellationToken),
            bool ignoreServerCertificateErrors = false,
            TlsProtocolType tlsProtocolType = TlsProtocolType.None)
        {
            if (ignoreServerCertificateErrors)
            {
                ServicePointManager.ServerCertificateValidationCallback += CertificateErrorHandler;
            }
            

            var connectTask = tcpClient.ConnectAsync(address, port).WrapNativeSocketExceptions();

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
            }

            canceller.Dispose();
            InitializeWriteStream();

            if (secure)
            {
                SslProtocols tlsProtocol;

                switch (tlsProtocolType)
                {
                    case TlsProtocolType.Tls10:
                        tlsProtocol = SslProtocols.Tls;
                        break;
                    case TlsProtocolType.Tls11:
                        tlsProtocol = SslProtocols.Tls11;
                        break;
                    case TlsProtocolType.Tls12:
                        tlsProtocol = SslProtocols.Tls12;
                        break;
                    case TlsProtocolType.None:
                        tlsProtocol = SslProtocols.None;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(tlsProtocolType), tlsProtocolType, null);
                }
                var secureStream = new SslStream(_writeStream, true, CertificateErrorHandler);
                secureStream.AuthenticateAsClient(address, null, tlsProtocol, false);
                _secureStream = secureStream;
            }
        }

        public async Task ConnectAsync(
            string address,
            string service,
            bool secure = false,
            CancellationToken cancellationToken = default(CancellationToken),
            bool ignoreServerCertificateErrors = false,
            TlsProtocolType tlsProtocolType = TlsProtocolType.None)
        {
            var port = ServiceNames.PortForTcpServiceName(service);

            await ConnectAsync(
                address, 
                port, 
                secure, 
                cancellationToken, 
                ignoreServerCertificateErrors).ConfigureAwait(false);
        }

        public void Disconnect()
        {
            tcpClient.Close();
            _secureStream = null;
            tcpClient = new TcpClient();
        }

        private bool CertificateErrorHandler(
            object sender,
            X509Certificate cert,
            X509Chain chain,
            SslPolicyErrors sslError)
        {
            switch (sslError)
            {
                case SslPolicyErrors.RemoteCertificateNameMismatch:
                    throw new Exception($"SSL/TLS error: {SslPolicyErrors.RemoteCertificateChainErrors.ToString()}");
                case SslPolicyErrors.RemoteCertificateNotAvailable:
                    throw new Exception($"SSL/TLS error: {SslPolicyErrors.RemoteCertificateNotAvailable.ToString()}");
                case SslPolicyErrors.RemoteCertificateChainErrors:
                    throw new Exception($"SSL/TLS error: {SslPolicyErrors.RemoteCertificateChainErrors.ToString()}");
                case SslPolicyErrors.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sslError), sslError, null);
            }
            return true;
        }

        private void InitializeWriteStream()
        {
            _writeStream = BufferSize != 0 ? (Stream)new BufferedStream(tcpClient.GetStream(), BufferSize) : tcpClient.GetStream();
        }

        public void Dispose()
        {
            _secureStream.Dispose();
            _writeStream.Dispose();
        }
    }
}
