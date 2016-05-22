using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace SocketLite.Model
{
    public partial class CommunicationEntity
    {
        protected static IPAddress GetSubnetMask(UnicastIPAddressInformation ip)
        {
            return ip.IPv4Mask;
        }
    }
}
