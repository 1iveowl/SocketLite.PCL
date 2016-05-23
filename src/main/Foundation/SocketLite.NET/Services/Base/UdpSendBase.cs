using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SocketLite.Extensions;

namespace SocketLite.Services.Base
{
    public abstract class UdpSendBase : CommonSocketBase
    {
        protected UdpClient BackingUdpClient;

        public virtual async Task SendAsync(byte[] data)
        {
            await BackingUdpClient
                .SendAsync(data, data.Length)
                .WrapNativeSocketExceptions()
                .ConfigureAwait(false);
        }

        public virtual async Task SendAsync(byte[] data, int length)
        {
            await BackingUdpClient
                .SendAsync(data, length)
                .WrapNativeSocketExceptions()
                .ConfigureAwait(false);
        }

        public virtual async Task SendToAsync(byte[] data, string address, int port)
        {
            await BackingUdpClient
               .SendAsync(data, data.Length, address, port)
               .WrapNativeSocketExceptions()
               .ConfigureAwait(false);
        }

        public virtual async Task SendToAsync(byte[] data, int length, string address, int port)
        {
            await BackingUdpClient
                .SendAsync(data, length, address, port)
                .WrapNativeSocketExceptions()
                .ConfigureAwait(false);
        }


    }
}
