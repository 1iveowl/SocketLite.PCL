using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
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
        private ISubject<ITcpSocketClient> ObsTcpSocket { get; } = new Subject<ITcpSocketClient>();

        public IObservable<ITcpSocketClient> ObservableTcpSocket => ObsTcpSocket.AsObservable();

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

            _streamSocketListener.ConnectionReceived += OnConnectionReceived;

            var localServiceName = port == 0 ? "" : port.ToString();

            var adapter = (communicationInterface as CommunicationInterface)?.NativeNetworkAdapter;

            if (adapter != null)
            {
                await _streamSocketListener.BindServiceNameAsync(
                    localServiceName, SocketProtectionLevel.PlainSocket, 
                    adapter);
            }
            else
                await _streamSocketListener.BindServiceNameAsync(localServiceName);
        }

        public void OnConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs e)
        {
            try
            {
                var nativeSocket = e.Socket;

                ObsTcpSocket.OnNext(new TcpSocketClient(nativeSocket, BufferSize));
            }
            catch (Exception ex)
            {
                ObsTcpSocket.OnError(ex);
            }
        }

        public void StopListening()
        {
            _streamSocketListener.ConnectionReceived -= OnConnectionReceived;
            _streamSocketListener.Dispose();
        }

        public void Dispose()
        {
            _streamSocketListener.ConnectionReceived -= OnConnectionReceived;
            _streamSocketListener.Dispose();
            ObsTcpSocket.OnCompleted();
        }
    }
}
