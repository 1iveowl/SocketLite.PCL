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
            TlsProtocolVersion tlsProtocolVersion = TlsProtocolVersion.Tls12)
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

#pragma warning disable CS4014
                // ensure we observe the connectTask's exception in case downstream consumers throw on unobserved tasks
                connectTask.ContinueWith(t => $"{t.Exception}", TaskContinuationOptions.OnlyOnFaulted);
#pragma warning restore CS4014 

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

                switch (tlsProtocolVersion)
                {
                    case TlsProtocolVersion.Tls10:
                        tlsProtocol = SslProtocols.Tls;
                        break;
                    case TlsProtocolVersion.Tls11:
                        tlsProtocol = SslProtocols.Tls11;
                        break;
                    case TlsProtocolVersion.Tls12:
                        tlsProtocol = SslProtocols.Tls12;
                        break;
                    case TlsProtocolVersion.None:
                        tlsProtocol = SslProtocols.None;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(tlsProtocolVersion), tlsProtocolVersion, null);
                }

                var secureStream = new SslStream(_writeStream, true, CertificateErrorHandler);

                try
                {
                    //There is a bug here in Mono. Bay be related to this :https://bugzilla.xamarin.com/show_bug.cgi?id=19141 
                    // and similar to this: https://forums.xamarin.com/discussion/51622/sslstream-authenticateasclient-hangs? 
                    //Environment.SetEnvironmentVariable("MONO_TLS_SESSION_CACHE_TIMEOUT", "0");
                    secureStream.AuthenticateAsClient(address, null, tlsProtocol, false);

                    _secureStream = secureStream;
                }
                catch (Exception ex)
                {
                    
                    throw ex;
                }
                
            }
        }

        public async Task ConnectAsync(
            string address,
            string service,
            bool secure = false,
            CancellationToken cancellationToken = default(CancellationToken),
            bool ignoreServerCertificateErrors = false,
            TlsProtocolVersion tlsProtocolVersion = TlsProtocolVersion.Tls12)
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
