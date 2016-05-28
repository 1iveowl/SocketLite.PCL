using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Connectivity;
using ISocketLite.PCL.Interface;
using ISocketLite.PCL.Model;
using SocketLite.Extensions;

namespace SocketLite.Model
{

    public class CommunicationsInterface : ICommunicationInterface
    {
        public string NativeInterfaceId { get; internal set; }

        public string Name { get; internal set; }

        public string IpAddress { get; internal set; }

        public string GatewayAddress { get; internal set; }

        public string BroadcastAddress { get; internal set; }

        public bool IsUsable => !string.IsNullOrWhiteSpace(IpAddress);

        private readonly string[] _loopbackAddresses = { "127.0.0.1", "localhost" };

        public bool IsLoopback => _loopbackAddresses.Contains(IpAddress);

        public CommunicationConnectionStatus ConnectionStatus { get; internal set; }

        protected internal HostName NativeHostName;

        protected internal NetworkAdapter NativeNetworkAdapter;

        public IEnumerable<ICommunicationInterface> GetAllInterfaces()
        {

            var profiles = NetworkInformation
                .GetConnectionProfiles()
                .Where(c => c.NetworkAdapter != null)
                .GroupBy(c => c.NetworkAdapter.NetworkAdapterId)
                .Select(x => x.FirstOrDefault())
                .ToDictionary(c => c.NetworkAdapter.NetworkAdapterId.ToString(), c => c);

            return NetworkInformation.GetHostNames()
                    .Where(h => h.IPInformation?.NetworkAdapter != null
                                && h.Type == HostNameType.Ipv4
                                && h.IPInformation.PrefixLength != null)
                    .Select(h =>
                    {
                        var adapter = h.IPInformation.NetworkAdapter;
                        var adapterId = adapter.NetworkAdapterId.ToString();
                        var subnetAddress = NetworkExtensions.GetSubnetAddress(
                            h.CanonicalName, 
                            h.IPInformation.PrefixLength.Value);

                        ConnectionProfile connectProfile;
                        var adapterName = profiles.TryGetValue(adapterId, out connectProfile) 
                            ? connectProfile.ProfileName 
                            : "{ unknown }";

                        return new CommunicationsInterface
                        {
                            NativeInterfaceId = adapterId,
                            Name = adapterName,
                            IpAddress = h.CanonicalName,
                            BroadcastAddress = NetworkExtensions.GetBroadcastAddress(IpAddress, subnetAddress),
                            NativeHostName = h,
                            NativeNetworkAdapter =  adapter,
                            ConnectionStatus = CommunicationConnectionStatus.Unknown
                        };
                    }); 
        }
    }
}
