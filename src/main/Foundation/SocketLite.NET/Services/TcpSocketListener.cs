using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;
using ISocketLite.PCL.Interface;
using SocketLite.Services.Base;
using CommunicationInterface = SocketLite.Model.CommunicationsInterface;
using PlatformSocketException = System.Net.Sockets.SocketException;
using PclSocketException = ISocketLite.PCL.Exceptions.SocketException;


namespace SocketLite.Services
{
    public class TcpSocketListener : TcpSocketBase, ITcpSocketListener
    {

        public IObservable<ITcpSocketClient> ObservableTcpSocket => _connectableObservableTcpSocket.Select(
            x =>
            {
                System.Diagnostics.Debug.WriteLine($"Ip: {x.RemoteAddress}, Port:{x.RemotePort}");
                return x;
            });


        private IConnectableObservable<ITcpSocketClient> _connectableObservableTcpSocket;


        private IObservable<ITcpSocketClient> _observableTcpSocket => ObserveTcpClientFromAsync.Select(
            tcpClient =>
            {
                var client = new TcpSocketClient(tcpClient, BufferSize);
                return client;
            })
            .Where(tcpClient => tcpClient != null);

        private IObservable<TcpClient> ObserveTcpClientFromAsync => Observable.While(
            () => !_listenCanceller.IsCancellationRequested,
            Observable.FromAsync(GetTcpClientAsync));

        private TcpListener _tcpListener;
        private readonly CancellationTokenSource _listenCanceller = new CancellationTokenSource();
        private IDisposable _tcpClientSubscribe;

        public int LocalPort => ((IPEndPoint)_tcpListener.LocalEndpoint).Port;

        public TcpSocketListener(int port,
            ICommunicationInterface communicationEntity = null,
            bool allowMultipleBindToSamePort = false) : base(0)
        {
            var ipAddress = communicationEntity != null ? ((CommunicationInterface)communicationEntity).NativeIpAddress : IPAddress.Any;

            _tcpListener = new TcpListener(ipAddress, port)
            {
                ExclusiveAddressUse = !allowMultipleBindToSamePort
            };
        }

        private async Task<TcpClient> GetTcpClientAsync()
        {
            TcpClient tcpClient = null;
            try
            {
                tcpClient = await _tcpListener.AcceptTcpClientAsync();
            }
            catch (Exception ex)
            {
                //throw;
            }
            
            return tcpClient;
        }

        public TcpSocketListener(int bufferSize) : base(bufferSize)
        {

        }

        public async Task StartListeningAsync(
            int port, 
            ICommunicationInterface listenOn = null,
            bool allowMultipleBindToSamePort = false)
        {
            CheckCommunicationInterface(listenOn);

            var ipAddress = listenOn != null ? ((CommunicationInterface)listenOn).NativeIpAddress : IPAddress.Any;

            _tcpListener = new TcpListener(ipAddress, port)
            {
                ExclusiveAddressUse = !allowMultipleBindToSamePort
            };

            _connectableObservableTcpSocket = _observableTcpSocket.Publish();

            try
            {
                _tcpListener.Start();
            }
            catch (PlatformSocketException ex)
            {
                throw new PclSocketException(ex);
            }

            _tcpClientSubscribe = _connectableObservableTcpSocket.Connect();
        }

        public void StopListening()
        {
            _listenCanceller.Cancel();
            try
            {
                _tcpListener.Stop();
            }
            catch (PlatformSocketException ex)
            {
                throw new PclSocketException(ex);
            }

            _tcpListener = null;
        }

        public void Dispose()
        {
            _tcpClientSubscribe.Dispose();
            _tcpListener.Stop();
            _listenCanceller.Cancel();
        }
    }
}
