using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ISocketLite.PCL.Interface
{
    public interface ITcpSocketClient
    {
        //Task ConnectAsync(string address, int port, bool secure = false);

        Task ConnectAsync(
            string address, 
            string service, 
            bool secure = false, 
            CancellationToken cancellationToken = default(CancellationToken));

        void Disconnect();

        Stream ReadStream { get; }

        Stream WriteStream { get; }

        string RemoteAddress { get; }

        int RemotePort { get; }
    }
}
