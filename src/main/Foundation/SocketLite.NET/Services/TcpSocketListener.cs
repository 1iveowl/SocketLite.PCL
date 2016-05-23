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
        public ISubject<ITcpSocketClient> ObservableTcpSocket { get; } = new Subject<ITcpSocketClient>();

        private TcpListener _backingTcpListener;
        private CancellationTokenSource _listenCanceller;

        public int LocalPort => ((IPEndPoint)(_backingTcpListener.LocalEndpoint)).Port;

        public TcpSocketListener() : base(0)
        {
        }

        public TcpSocketListener(int bufferSize) : base(bufferSize)
        {

        }

        public async Task StartListeningAsync(int port, ICommunicationInterface listenOn = null)
        {
            CheckCommunicationInterface(listenOn);

            var ipAddress = listenOn != null ? ((CommunicationInterface)listenOn).NativeIpAddress : IPAddress.Any;

            _listenCanceller = new CancellationTokenSource();

            _backingTcpListener = new TcpListener(ipAddress, port);

            try
            {
                _backingTcpListener.Start();
            }
            catch (PlatformSocketException ex)
            {
                throw new PclSocketException(ex);
            }

            await Task.Run(() => WaitForConnections(_listenCanceller.Token)).ConfigureAwait(false);
        }

        public void StopListening()
        {
            _listenCanceller.Cancel();
            try
            {
                _backingTcpListener.Stop();
            }
            catch (PlatformSocketException ex)
            {
                throw new PclSocketException(ex);
            }

            _backingTcpListener = null;
        }
        

        private void WaitForConnections(CancellationToken cancelToken)
        {
            var observeTcpClient = Observable.While(
                () => !cancelToken.IsCancellationRequested,
                Observable.FromAsync(_backingTcpListener.AcceptTcpClientAsync)).SubscribeOn(Scheduler.Default);

            observeTcpClient.Subscribe(
                tcpClient =>
                {
                    var wrappedTcpClint = new TcpSocketClient(tcpClient, BufferSize);
                    ObservableTcpSocket.OnNext(wrappedTcpClint);
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
            _backingTcpListener.Stop();
            ObservableTcpSocket.OnCompleted();
        }
    }
}
