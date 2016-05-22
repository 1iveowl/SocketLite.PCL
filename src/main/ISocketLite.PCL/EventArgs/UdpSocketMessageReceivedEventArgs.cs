using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISocketLite.PCL.EventArgs
{
    public class UdpSocketMessageReceivedEventArgs
    {
        public string RemoteAddress { get; private set; }

        public string RemotePort { get; private set; }

        public byte[] ByteData { get; private set; }

        public UdpSocketMessageReceivedEventArgs(string remoteAddress, string remotePort, byte[] byteData)
        {
            RemoteAddress = remoteAddress;
            RemotePort = remotePort;
            ByteData = byteData;
        }
    }
}
