using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;
using SocketLite.Extensions;
using SocketException = ISocketLite.PCL.Exceptions.SocketException;

namespace SocketLite.Services.Base
{
    public abstract class UdpSocketBase : CommonSocketBase
    {
        public event EventHandler<UdpSocketMessageReceivedEventArgs> MessageReceived;

        protected static readonly HashSet<Type> NativeSocketExceptions = new HashSet<Type> { typeof(System.Net.Sockets.SocketException) };
        protected UdpClient BackingUdpClient;

        public virtual async Task SendAsync(byte[] data)
        {
            await BackingUdpClient
                .SendAsync(data, data.Length)
                .WrapNativeSocketExceptions();
        }

        public virtual async Task SendAsync(byte[] data, int length)
        {
            await BackingUdpClient
                .SendAsync(data, length)
                .WrapNativeSocketExceptions();
        }

        public virtual async Task SendToAsync(byte[] data, string address, int port)
        {
            await BackingUdpClient
               .SendAsync(data, data.Length, address, port)
               .WrapNativeSocketExceptions();
        }

        public virtual async Task SendToAsync(byte[] data, int length, string address, int port)
        {
            await BackingUdpClient
                .SendAsync(data, length, address, port)
                .WrapNativeSocketExceptions();
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~UdpSocketBase()
        {
            Dispose(false);
        }

        protected void RunMessageReceiver(CancellationToken cancelToken)
        {
            var observeUdpReceive = Observable.While(
                () => !cancelToken.IsCancellationRequested,
                Observable.FromAsync(BackingUdpClient.ReceiveAsync)).SubscribeOn(Scheduler.Default);

            observeUdpReceive.Subscribe(
                // Message Received (OnNext)
                msg =>
                {
                    var wrapperArgs = new UdpSocketMessageReceivedEventArgs(
                        msg.RemoteEndPoint.Address.ToString(),
                        msg.RemoteEndPoint.Port.ToString(),
                        msg.Buffer);

                    MessageReceived?.Invoke(this, wrapperArgs);
                },
                // Exception (OnError)
                ex =>
                {
                    throw (NativeSocketExceptions.Contains(ex.GetType()))
                        ? new SocketException(ex)
                        : ex;
                },
                //OnCompletion
                () =>
                {

                }, cancelToken);
        }
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (BackingUdpClient != null)
                    ((IDisposable)BackingUdpClient).Dispose();
            }
        }

    }
}
