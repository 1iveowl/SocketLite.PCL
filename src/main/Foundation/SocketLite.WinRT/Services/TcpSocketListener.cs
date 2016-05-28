using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking.Sockets;
using ISocketLite.PCL.EventArgs;
using ISocketLite.PCL.Interface;
using SocketLite.Model;
using SocketLite.Services;
using SocketLite.Services.Base;

namespace SocketLite.Services
{
    public class TcpSocketListener : TcpSocketBase, ITcpSocketListener
    {
        private StreamSocketListener _streamSocketListener;

        private IDisposable _connectionSubscriber;

        private readonly ISubject<ITcpSocketClient> _tcpSocketSubject = new Subject<ITcpSocketClient>();

        private IObservable<ITcpSocketClient> ObserveTcpSocketConnectionsFromEvents =>
            Observable.FromEventPattern<
                TypedEventHandler<StreamSocketListener, StreamSocketListenerConnectionReceivedEventArgs>,
                StreamSocketListenerConnectionReceivedEventArgs>(
                    ev => _streamSocketListener.ConnectionReceived += ev,
                    ev => _streamSocketListener.ConnectionReceived -= ev)
                .Select(handler => new TcpSocketClient(handler.EventArgs.Socket, BufferSize));

        public IObservable<ITcpSocketClient> ObservableTcpSocket => _tcpSocketSubject.AsObservable();

        public int LocalPort { get; internal set; }

        public TcpSocketListener() : base(bufferSize:0)
        {
        }

        public TcpSocketListener(int bufferSize) :base(bufferSize)
        {
        }

        public async Task StartListeningAsync(
            int port, 
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = false)
        {
            //Throws and exception if the communication interface is not ready og valid.
            CheckCommunicationInterface(communicationInterface);

            _streamSocketListener = new StreamSocketListener();

            //_streamSocketListener.ConnectionReceived += OnConnectionReceived;
            SubscribeToConnections();

            var localServiceName = port == 0 ? "" : port.ToString();

            var adapter = (communicationInterface as CommunicationsInterface)?.NativeNetworkAdapter;

            if (adapter != null)
            {
                await _streamSocketListener.BindServiceNameAsync(
                    localServiceName, SocketProtectionLevel.PlainSocket, 
                    adapter);
            }
            else
                await _streamSocketListener.BindServiceNameAsync(localServiceName);
        }

        private void SubscribeToConnections()
        {
            _connectionSubscriber= ObserveTcpSocketConnectionsFromEvents.Subscribe(
                connection =>
                {
                    _tcpSocketSubject.OnNext(connection);
                },
                ex =>
                {
                    _tcpSocketSubject.OnError(ex);
                },
                Dispose);
        }

        public void StopListening()
        {
            _connectionSubscriber.Dispose();
            _streamSocketListener.Dispose();
        }

        public void Dispose()
        {
            _connectionSubscriber.Dispose();
            _streamSocketListener.Dispose();
        }
    }
}
