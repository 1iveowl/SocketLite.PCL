using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ISocketLite.PCL.Model;

namespace ISocketLite.PCL.Interface
{
    public interface ITcpSocketClient : IDisposable
    {
        //string LocalPort { get; }
        //string LocalAddress { get; }

        Task ConnectAsync(
            string address,
            string service,
            bool secure = false,
            CancellationToken cancellationToken = default(CancellationToken),
            bool ignoreServerCertificateErrors = false,
            TlsProtocolVersion tlsProtocolVersion = TlsProtocolVersion.Tls12);

        void Disconnect();

        Stream ReadStream { get; }

        Stream WriteStream { get; }

        string RemoteAddress { get; }

        int RemotePort { get; }
    }
}
