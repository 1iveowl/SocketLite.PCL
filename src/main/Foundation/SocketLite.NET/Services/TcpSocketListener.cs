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
using CommunicationInterface = SocketLite.Model.CommunicationInterface;
using PlatformSocketException = System.Net.Sockets.SocketException;
using PclSocketException = ISocketLite.PCL.Exceptions.SocketException;


namespace SocketLite.Services
{
    public class TcpSocketListener : TcpSocketBase, ITcpSocketListener
    {
        private readonly ISubject<ITcpSocketClient> _tcpSocketSubject = new Subject<ITcpSocketClient>();

        public IObservable<ITcpSocketClient> ObservableTcpSocket => _tcpSocketSubject.AsObservable();

        private TcpListener _tcpListener;
        private CancellationTokenSource _listenCanceller;

        public int LocalPort => ((IPEndPoint)(_tcpListener.LocalEndpoint)).Port;

        public TcpSocketListener() : base(0)
        {
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

            _listenCanceller = new CancellationTokenSource();

            _tcpListener = new TcpListener(ipAddress, port)
            {
                ExclusiveAddressUse = !allowMultipleBindToSamePort
            };

            try
            {
                _tcpListener.Start();
            }
            catch (PlatformSocketException ex)
            {
                throw new PclSocketException(ex);
            }

            await Task.Run(() => ListenForConnections(_listenCanceller.Token)).ConfigureAwait(false);
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

        private void ListenForConnections(CancellationToken cancelToken)
        {
            var observeTcpClient = Observable.While(
                () => !cancelToken.IsCancellationRequested,
                Observable.FromAsync(_tcpListener.AcceptTcpClientAsync)).SubscribeOn(Scheduler.Default);

            observeTcpClient.Subscribe(
                tcpClient =>
                {
                    var wrappedTcpClint = new TcpSocketClient(tcpClient, BufferSize);
                    _tcpSocketSubject.OnNext(wrappedTcpClint);
                },
                ex =>
                {
                    throw (NativeSocketExceptions.Contains(ex.GetType()))
                        ? new PclSocketException(ex)
                        : ex;
                }, cancelToken);
        }

        public void Dispose()
        {
            _tcpListener.Stop();
            _listenCanceller.Cancel();
        }
    }
}
