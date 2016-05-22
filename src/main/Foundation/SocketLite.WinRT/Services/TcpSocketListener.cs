using System;
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

        private StreamSocketListener _backingStreamSocketListener;

        public event EventHandler<TcpSocketListenerConnectEventArgs> ConnectionReceived;

        public int LocalPort { get; internal set; }


        public TcpSocketListener() : base(bufferSize:0)
        {
        }

        public TcpSocketListener(int bufferSize) :base(bufferSize)
        {
        }

        public void OnConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs e)
        {
            var nativeSocket = e.Socket;
            var wrappedSocket = new TcpSocketClient(nativeSocket, BufferSize);

            var eventArgs = new TcpSocketListenerConnectEventArgs(wrappedSocket);
            ConnectionReceived?.Invoke(this, eventArgs);
        }

        public async Task StartListeningAsync(int port, ICommunicationEntity communicationEntity = null)
        {
            //Throws and exception if the communication interface is not ready og valid.
            CheckCommunicationInterface(communicationEntity);

            _backingStreamSocketListener = new StreamSocketListener();

            _backingStreamSocketListener.ConnectionReceived += OnConnectionReceived;

            var localServiceName = port == 0 ? "" : port.ToString();

            if (communicationEntity != null)
            {
                var adapter = ((CommunicationEntity)communicationEntity).NativeNetworkAdapter;

                await _backingStreamSocketListener.BindServiceNameAsync(
                    localServiceName, SocketProtectionLevel.PlainSocket, 
                    adapter);
            }
            else
                await _backingStreamSocketListener.BindServiceNameAsync(localServiceName);
        }

        public void StopListening()
        {
            _backingStreamSocketListener.Dispose();
        }
    }
}
