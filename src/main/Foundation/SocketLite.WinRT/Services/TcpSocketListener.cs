using System;
using System.Linq.Expressions;
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
        //private readonly IObservable<ITcpSocketClient> _observableTcpSocket; //= new Subject<ITcpSocketClient>().AsObservable();

        //public IObservable<ITcpSocketClient> ObservableTcpSocket => _connectableObservableTcpSocket.Select(x =>
        //{
        //    return x;
        //});

        ////public IObservable<ITcpSocketClient> ObservableTcpSocket =>
        ////    _connectableObservableTcpSocket.Select(
        ////        socketClient => socketClient); 

        //private IConnectableObservable<ITcpSocketClient> _connectableObservableTcpSocket => ObserveTcpSocketConnectionsFromEvents.Publish();

        public IObservable<ITcpSocketClient> ObservableTcpSocket =>
            Observable.FromEventPattern<
                TypedEventHandler<StreamSocketListener, StreamSocketListenerConnectionReceivedEventArgs>,
                StreamSocketListenerConnectionReceivedEventArgs>(
                    ev => _streamSocketListener.ConnectionReceived += ev,
                    ev => _streamSocketListener.ConnectionReceived -= ev)
                .Select(handler => new TcpSocketClient(handler.EventArgs.Socket, BufferSize));

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

            //await _connectableObservableTcpSocket.Publish();

            //_connectableObservableTcpSocket = ObserveTcpSocketConnectionsFromEvents.Publish();

            var localServiceName = port == 0 ? "" : port.ToString();

            var adapter = (communicationInterface as CommunicationsInterface)?.NativeNetworkAdapter;

            if (adapter != null)
            {
                await _streamSocketListener.BindServiceNameAsync(
                    localServiceName, SocketProtectionLevel.PlainSocket,
                    adapter);
            }
            else
            {
                await _streamSocketListener.BindServiceNameAsync(localServiceName);
            }
            

            //_connectionSubscriber = _connectableObservableTcpSocket.Connect();

            //var r = _observableTcpSocket;

            //var t = await _observableTcpSocket.Merge(_connectableObservableTcpSocket).Select(x =>
            //{
            //    return x;
            //}); ;



        }

        public void StopListening()
        {
            _connectionSubscriber.Dispose();
            _streamSocketListener.Dispose();
        }

        public void Dispose()
        {
            StopListening();
        }
    }
}
