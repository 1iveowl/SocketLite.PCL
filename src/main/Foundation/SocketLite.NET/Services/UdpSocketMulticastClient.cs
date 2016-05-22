using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ISocketLite.PCL.Interface;
using SocketLite.Services.Base;
using CommunicationEntity = SocketLite.Model.CommunicationEntity;
using PlatformSocketException = System.Net.Sockets.SocketException;
using PclSocketException = ISocketLite.PCL.Exceptions.SocketException;

namespace SocketLite.Services
{
    public class UdpSocketMulticastClient : UdpSocketBase, IUdpSocketMulticastClient
    {
        


        private string _multicastAddress;
        private int _multicastPort;

        private CancellationTokenSource _messageCanceller;

        public int TTL { get; set; } = 1;

        public async Task JoinMulticastGroupAsync(string multicastAddress, int port, ICommunicationEntity multicastOn = null)
        {
            CheckCommunicationInterface(multicastOn);

            var bindingIp = multicastOn != null ? ((CommunicationEntity)multicastOn).NativeIpAddress : IPAddress.Any;
            var bindingEp = new IPEndPoint(bindingIp, port);

            var multicastIp = IPAddress.Parse(multicastAddress);

            try
            {
                BackingUdpClient = new UdpClient(bindingEp)
                {
                    EnableBroadcast = true
                };
            }
            catch (PlatformSocketException ex)
            {
                throw new PclSocketException(ex);
            }

            _messageCanceller = new CancellationTokenSource();

            try
            {
                BackingUdpClient.JoinMulticastGroup(multicastIp, TTL);
            }
            catch (Exception ex)
            {
                throw (NativeSocketExceptions.Contains(ex.GetType()))
                        ? new PclSocketException(ex)
                        : ex;
            }

            _multicastAddress = multicastAddress;
            _multicastPort = port;

            await Task.Run(() => RunMessageReceiver(_messageCanceller.Token));
        }

        public void Disconnect()
        {
            _messageCanceller.Cancel();
            BackingUdpClient.Close();

            _multicastAddress = null;
            _multicastPort = 0;
        }

        public async Task SendMulticastAsync(byte[] data)
        {
            await SendMulticastAsync(data, data.Length);
        }

        public async Task SendMulticastAsync(byte[] data, int length)
        {
            if (_multicastAddress == null)
                throw new InvalidOperationException("Must join a multicast group before sending.");

            await base.SendToAsync(data, length, _multicastAddress, _multicastPort);
        }

        public override void Dispose()
        {
            if (_messageCanceller != null && !_messageCanceller.IsCancellationRequested)
                _messageCanceller.Cancel();

            base.Dispose();
        }

        
    }
}
