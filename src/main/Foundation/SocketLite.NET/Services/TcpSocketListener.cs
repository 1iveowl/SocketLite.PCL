using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;
using ISocketLite.PCL.Interface;
using SocketLite.Services.Base;
using CommunicationEntity = SocketLite.Model.CommunicationEntity;
using PlatformSocketException = System.Net.Sockets.SocketException;
using PclSocketException = ISocketLite.PCL.Exceptions.SocketException;


namespace SocketLite.Services
{
    public class TcpSocketListener : TcpSocketBase, ITcpSocketListener
    {
        private TcpListener _backingTcpListener;
        private CancellationTokenSource _listenCanceller;

        public event EventHandler<TcpSocketListenerConnectEventArgs> ConnectionReceived;
        public int LocalPort => ((IPEndPoint)(_backingTcpListener.LocalEndpoint)).Port;

        public TcpSocketListener() : base(0)
        {
        }

        public TcpSocketListener(int bufferSize) : base(bufferSize)
        {

        }

        public async Task StartListeningAsync(int port, ICommunicationEntity listenOn = null)
        {
            CheckCommunicationInterface(listenOn);

            var ipAddress = listenOn != null ? ((CommunicationEntity)listenOn).NativeIpAddress : IPAddress.Any;

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

            await Task.Run(() => WaitForConnections(_listenCanceller.Token));
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
                    var wrappedClient = new TcpSocketClient(tcpClient, BufferSize);
                    var eventArgs = new TcpSocketListenerConnectEventArgs(wrappedClient);
                    ConnectionReceived?.Invoke(this, eventArgs);
                },
                ex =>
                {

                },
                () =>
                {

                }, cancelToken);
        }
        //public void Dispose()
        //{
        //    Dispose(true);
        //    //GC.SuppressFinalize(this);
        //}

        //~TcpSocketListener()
        //{
        //    Dispose(true);
        //}

        //private void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        _listenCanceller.Cancel();
        //        _backingTcpListener?.Stop();
        //    }
        //}
    }
}
