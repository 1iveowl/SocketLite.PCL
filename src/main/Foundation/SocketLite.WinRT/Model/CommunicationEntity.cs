using System.Linq;
using Windows.Networking;
using Windows.Networking.Connectivity;
using ISocketLite.PCL.Interface;
using ISocketLite.PCL.Model;

namespace SocketLite.Model
{

    public class CommunicationEntity : ICommunicationEntity
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

        // TODO: Move to singleton, rather than static method?

        //public async Task<List<CommunicationEntity>> GetAllInterfacesAsync()
        //{
        //    //return Task.Run(() =>
        //    //{
        //        var profiles = NetworkInformation
        //            .GetConnectionProfiles()
        //            .Where(cp => cp.NetworkAdapter != null)
        //            .GroupBy(cp => cp.NetworkAdapter.NetworkAdapterId)
        //            .Select(na => na.First())
        //            .ToDictionary(cp => cp.NetworkAdapter.NetworkAdapterId.ToString(), cp => cp);

        //        var interfaces =
        //            NetworkInformation
        //                .GetHostNames()
        //                .Where(hn => hn.IPInformation != null && hn.IPInformation.NetworkAdapter != null)
        //                .Where(hn => hn.Type == HostNameType.Ipv4)
        //                .Where(hn => hn.IPInformation.PrefixLength != null)
        //                .Select(hn =>
        //                {
        //                    var ipAddress = hn.CanonicalName;
        //                    var prefixLength = (int)hn.IPInformation.PrefixLength; // seriously why is this nullable

        //                    var subnetAddress = NetworkExtensions.GetSubnetAddress(ipAddress, prefixLength);
        //                    var broadcastAddress = NetworkExtensions.GetBroadcastAddress(ipAddress, subnetAddress);

        //                    var adapter = hn.IPInformation.NetworkAdapter;
        //                    var adapterId = adapter.NetworkAdapterId.ToString();
        //                    var adapterName = "{ unknown }";
        //                    ConnectionProfile matchingProfile;

        //                    if (profiles.TryGetValue(adapterId, out matchingProfile))
        //                        adapterName = matchingProfile.ProfileName;

        //                    return new CommunicationEntity
        //                    {
        //                        NativeInterfaceId = adapterId,
        //                        Name = adapterName,
        //                        IpAddress = ipAddress,
        //                        BroadcastAddress = broadcastAddress,
        //                        GatewayAddress = null,

        //                        NativeHostName = hn,
        //                        NativeNetworkAdapter = adapter,

        //                        ConnectionStatus = CommunicationConnectionStatus.Unknown
        //                    };
        //                }).ToList();

        //        return interfaces;
        //    //});
        //}
    }
}
