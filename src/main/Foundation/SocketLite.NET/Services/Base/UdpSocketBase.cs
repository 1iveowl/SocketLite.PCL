using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using ISocketLite.PCL.EventArgs;
using SocketLite.Model;
using SocketException = ISocketLite.PCL.Exceptions.SocketException;

namespace SocketLite.Services.Base
{
    public abstract class UdpSocketBase : UdpSendBase
    {
        public ISubject<IUdpMessage> ObservableMessages { get; } = new Subject<IUdpMessage>();

        protected void RunMessageReceiver(CancellationToken cancelToken)
        {
            var observeUdpReceive = Observable.While(
                () => !cancelToken.IsCancellationRequested,
                Observable.FromAsync(BackingUdpClient.ReceiveAsync))
                .Select(msg =>
                {
                    var message = new UdpMessage
                    {
                        ByteData = msg.Buffer,
                        RemotePort = msg.RemoteEndPoint.Port.ToString(),
                        RemoteAddress = msg.RemoteEndPoint.Address.ToString()
                    };

                    return message;
                }).SubscribeOn(Scheduler.Default);

            observeUdpReceive.Subscribe(
                // Message Received Args (OnNext)
                args =>
                {
                    ObservableMessages.OnNext(args);
                },
                // Exception (OnError)
                ex =>
                {
                    throw (NativeSocketExceptions.Contains(ex.GetType()))
                        ? new SocketException(ex)
                        : ex;
                }, cancelToken);
        }

        public void Dispose()
        {
            BackingUdpClient.Close();
            ObservableMessages.OnCompleted();
        }
    }
}
